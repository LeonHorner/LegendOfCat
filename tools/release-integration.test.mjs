import { test } from 'node:test';
import assert from 'node:assert/strict';
import { mkdtempSync, rmSync, writeFileSync, readFileSync, existsSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join, resolve } from 'node:path';
import { execFileSync } from 'node:child_process';
import { runSnapshotRelease } from './release-snapshot.mjs';
import { createSnapshotTag, publish as publishSnapshot } from './release-github.mjs';
import { assertSnapshot } from './release-approval.mjs';
import config from '../release.config.js';

const approval = resolve('tools/release-approval.mjs');
const changelog = config.plugins.find(p => Array.isArray(p) && p[0] === './tools/release-changelog.mjs');
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
async function run(f, {dryRun = false, minor = 'true', major = 'false', expected} = {}) {
  const env = { ...process.env, CI:'true', GITHUB_ACTIONS:'true', GITHUB_EVENT_NAME:'push', GITHUB_REF:'refs/heads/main', GITHUB_SHA:f.sha, ALLOW_MINOR:minor, ALLOW_MAJOR:major, EXPECTED:expected || '' };
  delete env.GITHUB_TOKEN; delete env.GH_TOKEN;
  return runSnapshotRelease({ cwd:f.cwd, env, dryRun, options:{ branches:['main'], tagFormat:'v${version}',
    plugins:[analyzer, approval, [resolve(changelog[0]), changelog[1]], f.mock] } });
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

test('a newer main does not alter the planned or published snapshot', async () => {
  const f = fixture('fix: repair menu');
  try {
    git(f.cwd, 'checkout', 'main'); git(f.cwd, 'commit', '--allow-empty', '-m', 'feat: newer feature'); git(f.cwd, 'push', 'origin', 'main'); git(f.cwd, 'checkout', '--detach', f.sha);
    const plan = await run(f, {dryRun:true});
    assert.equal(plan.nextRelease.version, '0.7.1');
    const result = await run(f, {expected:'0.7.1'});
    assert.equal(result.nextRelease.version, '0.7.1');
    assert.match(result.nextRelease.notes, /repair menu/);
    assert.doesNotMatch(result.nextRelease.notes, /newer feature/);
    assert.match(result.nextRelease.notes, /github\.com\/LeonHorner\/LegendOfCat/);
    assert.equal(git(f.cwd, 'rev-list', '-n', '1', 'v0.7.1'), f.sha);
    assert.equal(JSON.parse(readFileSync(join(f.dir, 'published.json'))).gitHead, f.sha);
  } finally { rmSync(f.dir, {recursive:true, force:true}); }
});


test('disabled minor approval fails during planning without publishing', async () => {
  const f = fixture('feat: add menu');
  try {
    await assert.rejects(run(f, {dryRun:true, minor:'false', major:'true'}), /allow_minor/);
    assert.equal(git(f.cwd, 'tag', '--list', 'v0.8.0'), '');
    assert.equal(existsSync(join(f.dir, 'published.json')), false);
  } finally { rmSync(f.dir, {recursive:true, force:true}); }
});


test('GitHub tag creation rejects existing releases and conflicting refs', async () => {
  const sha = 'a'.repeat(40), calls = [];
  const fake = async (url, options) => {
    calls.push({url, options});
    return calls.length === 1 ? {status:404, ok:false} : {
      status: calls.length === 2 ? 201 : 200, ok:true, json:async () => ({ref:'refs/tags/v0.7.1', object:{sha, type:'commit'}})
    };
  };
  await createSnapshotTag({ tag:'v0.7.1', sha, token:'test', request:fake });
  assert.deepEqual(JSON.parse(calls[1].options.body), {ref:'refs/tags/v0.7.1', sha});
  await assert.rejects(createSnapshotTag({tag:'v0.7.1', sha, token:'test', request:async () => ({status:200, ok:true})}), /already exists/);
  let count = 0;
  await assert.rejects(createSnapshotTag({tag:'v0.7.1', sha, token:'test', request:async () => ++count === 1 ? {status:404, ok:false} : {status:422, ok:false}}), /create release tag/);
  await assert.rejects(createSnapshotTag({tag:'v0.7.1', sha, token:'test', request:async () => ({status:403, ok:false})}), /check existing/);
});


test('missing or mismatched workflow SHA fails before running semantic-release', () => {
  const f = fixture('fix: repair menu');
  try {
    assert.throws(() => assertSnapshot({}, f.cwd), /Missing/);
    assert.throws(() => assertSnapshot({GITHUB_SHA:'0'.repeat(40)}, f.cwd), /snapshot/);
    assert.equal(existsSync(join(f.dir, 'published.json')), false);
  } finally { rmSync(f.dir, {recursive:true, force:true}); }
});

test('tag verification failure aborts after creation and requires inspection', async () => {
  const sha = 'a'.repeat(40);
  let count = 0;
  const request = async () => {
    count++;
    if (count === 1) return {status:404, ok:false};
    if (count === 2) return {status:201, ok:true};
    return {status:200, ok:true, json:async () => ({ref:'refs/tags/v0.7.1', object:{sha:'b'.repeat(40), type:'commit'}})};
  };
  await assert.rejects(createSnapshotTag({tag:'v0.7.1', sha, token:'test', request}), /inspect before recovery/);
  assert.equal(count, 3);
});


test('main may advance after planning and before publication', async () => {
  const f = fixture('fix: repair menu');
  try {
    const plan = await run(f, {dryRun:true});
    git(f.cwd, 'checkout', 'main');
    git(f.cwd, 'commit', '--allow-empty', '-m', 'feat: later change');
    const later = git(f.cwd, 'rev-parse', 'HEAD');
    git(f.cwd, 'push', 'origin', 'main');
    git(f.cwd, 'checkout', '--detach', f.sha);
    const result = await run(f, {expected:plan.nextRelease.version});
    assert.equal(result.nextRelease.version, '0.7.1');
    assert.equal(result.nextRelease.gitHead, f.sha);
    assert.match(result.nextRelease.notes, /repair menu/);
    assert.doesNotMatch(result.nextRelease.notes, /later change/);
    assert.equal(git(f.cwd, 'ls-remote', '--heads', 'origin', 'main').split(/\s+/)[0], later);
  } finally { rmSync(f.dir, {recursive:true, force:true}); }
});


test('publication adapter passes the snapshot SHA to the GitHub publisher only after tag verification', async () => {
  const f = fixture('fix: repair menu');
  try {
    const calls = [];
    const request = async (_url, options) => {
      calls.push(options.method || 'GET');
      if (calls.length === 1) return {status:404, ok:false};
      if (calls.length === 2) return {status:201, ok:true};
      return {status:200, ok:true, json:async () => ({ref:'refs/tags/v0.7.1', object:{sha:f.sha, type:'commit'}})};
    };
    let published = false;
    const context = {cwd:f.cwd, env:{GITHUB_SHA:f.sha, EXPECTED:'0.7.1', GITHUB_TOKEN:'test'},
      nextRelease:{gitHead:f.sha, gitTag:'v0.7.1', version:'0.7.1'}, options:{}, branch:{name:'main'}};
    await publishSnapshot({}, context, {request, publishRelease:async (_config, githubContext) => {
      published = true;
      assert.deepEqual(calls, ['GET', 'POST', 'GET']);
      assert.equal(githubContext.branch.name, f.sha);
      assert.equal(githubContext.options.repositoryUrl, 'https://github.com/LeonHorner/LegendOfCat.git');
    }});
    assert.equal(published, true);
    published = false;
    await assert.rejects(publishSnapshot({}, context, {request:async () => ({status:422, ok:false}), publishRelease:async () => { published = true; }}));
    assert.equal(published, false);
  } finally { rmSync(f.dir, {recursive:true, force:true}); }
});
