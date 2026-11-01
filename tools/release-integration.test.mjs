import { test } from 'node:test';
import assert from 'node:assert/strict';
import { mkdtempSync, rmSync, writeFileSync, readFileSync, existsSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join, resolve } from 'node:path';
import { execFileSync } from 'node:child_process';
import semanticRelease from 'semantic-release';
import config from '../release.config.js';

const approval = resolve('tools/release-approval.mjs');
const analyzer = config.plugins.find(p => Array.isArray(p) && p[0] === '@semantic-release/commit-analyzer');
function git(cwd, ...args) { return execFileSync('git', args, { cwd, encoding: 'utf8', stdio: ['ignore','pipe','pipe'] }).trim(); }
function fixture(message) {
  const dir = mkdtempSync(join(tmpdir(), 'loc-semrel-'));
  const remote = join(dir, 'remote.git'), cwd = join(dir, 'repo');
  git(dir, 'init', '--bare', '-b', 'main', remote);
  git(dir, 'clone', remote, cwd);
  git(cwd, 'config', 'user.name', 'Test'); git(cwd, 'config', 'user.email', 'test@example.invalid');
  git(cwd, 'commit', '--allow-empty', '-m', 'chore: baseline');
  git(cwd, 'tag', 'v0.7.0'); git(cwd, 'push', 'origin', 'main', '--tags');
  git(cwd, 'commit', '--allow-empty', '-m', message);
  const sha = git(cwd, 'rev-parse', 'HEAD'); git(cwd, 'push', 'origin', 'main');
  const mock = join(dir, 'mock.mjs');
  writeFileSync(mock, `import { writeFileSync } from 'node:fs';\nexport async function generateNotes() { return 'test notes'; }\nexport async function publish(_c, ctx) { writeFileSync(${JSON.stringify(join(dir,'published.json'))}, JSON.stringify(ctx.nextRelease)); return {name:'mock'}; }\n`);
  git(cwd, 'checkout', '--detach', sha);
  return { dir, cwd, remote, sha, mock };
}
async function run(f, {dryRun = false, major = 'false', expected} = {}) {
  const env = { ...process.env, CI:'true', GITHUB_ACTIONS:'true', GITHUB_EVENT_NAME:'push', GITHUB_REF:'refs/heads/main', GITHUB_SHA:f.sha, ALLOW_MAJOR:major, EXPECTED:expected || '' };
  delete env.GITHUB_TOKEN; delete env.GH_TOKEN;
  return semanticRelease({ repositoryUrl: f.remote, branches:['main'], tagFormat:'v${version}', dryRun, ci:true,
    plugins:[analyzer, approval, f.mock] }, { cwd:f.cwd, env });
}

test('semantic-release plans and publishes patch and minor through its real lifecycle', async () => {
  for (const [message, version, major] of [['fix: repair menu','0.7.1', 'false'], ['feat: add menu','0.8.0', 'false'], ['fix!: incompatible save','1.0.0', 'true']]) {
    const f = fixture(message);
    try {
      const plan = await run(f, {dryRun:true, major});
      assert.equal(plan.nextRelease.version, version);
      assert.equal(git(f.cwd, 'tag', '--list', 'v' + version), '');
      const result = await run(f, {expected:version, major});
      assert.equal(result.nextRelease.version, version);
      assert.equal(git(f.cwd, 'rev-list', '-n', '1', 'v' + version), f.sha);
      assert.equal(JSON.parse(readFileSync(join(f.dir, 'published.json'))).gitHead, f.sha);
    } finally { rmSync(f.dir, {recursive:true, force:true}); }
  }
});

test('unapproved major and mismatched validated versions never publish or tag', async () => {
  for (const [message, options] of [['fix!: incompatible save', {}], ['fix: repair menu', {expected:'9.9.9'}]]) {
    const f = fixture(message);
    try {
      await assert.rejects(run(f, options));
      assert.equal(git(f.cwd, 'tag', '--list', message.startsWith('fix!') ? 'v1.0.0' : 'v0.7.1'), '');
      assert.equal(existsSync(join(f.dir, 'published.json')), false);
    } finally { rmSync(f.dir, {recursive:true, force:true}); }
  }
});

test('a reachable tag is a release boundary even without a GitHub publication', async () => {
  const f = fixture('fix: repair menu');
  try {
    git(f.cwd, 'tag', 'v0.7.1'); git(f.cwd, 'push', 'origin', 'v0.7.1');
    assert.equal(await run(f, {dryRun:true}), false);
    assert.equal(existsSync(join(f.dir, 'published.json')), false);
  } finally { rmSync(f.dir, {recursive:true, force:true}); }
});

test('a newer main prevents publishing an older checked-out snapshot', async () => {
  const f = fixture('fix: repair menu');
  try {
    git(f.cwd, 'checkout', 'main'); git(f.cwd, 'commit', '--allow-empty', '-m', 'feat: newer feature'); git(f.cwd, 'push', 'origin', 'main'); git(f.cwd, 'checkout', '--detach', f.sha);
    assert.equal(await run(f), false);
    assert.equal(existsSync(join(f.dir, 'published.json')), false);
  } finally { rmSync(f.dir, {recursive:true, force:true}); }
});
