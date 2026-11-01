import { test } from 'node:test';
import assert from 'node:assert/strict';
import { mkdtempSync, mkdirSync, writeFileSync, readFileSync, chmodSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join, delimiter } from 'node:path';
import { spawnSync, execFileSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';
import { DOWNLOADS } from './release-assets.mjs';
import { planRelease } from './release-engine.mjs';
import { assertDraftAssets, getJson, inspectTag } from './release-recovery.mjs';

const script = fileURLToPath(new URL('./publish-release.mjs', import.meta.url));
const fakeGh = String.raw`#!/usr/bin/env node
const fs = require('node:fs');
const cp = require('node:child_process');
const crypto = require('node:crypto');
const a = process.argv.slice(2);
const path = process.env.GH_MOCK_STATE;
const state = JSON.parse(fs.readFileSync(path, 'utf8'));
state.calls.push(a);
const save = () => fs.writeFileSync(path, JSON.stringify(state));
const error = (code = 1) => { save(); process.stderr.write('gh: Not Found (HTTP 404)\n'); process.exit(code); };
const fail = phase => { if (process.env.GH_MOCK_FAIL === phase) { save(); process.exit(45); } };
const tag = 'v0.7.1';
if (a[0] === 'api') {
  if (a.includes('POST')) {
    const ref = a.find(x => x.startsWith('ref='));
    const sha = a.find(x => x.startsWith('sha='));
    if (ref !== 'ref=refs/tags/' + tag || !sha || state.tags[tag]) error(4);
    state.tags[tag] = sha.slice(4);
    // A rerun has fetched the remote tag; reproduce that checkout state.
    cp.execFileSync('git', ['tag', tag, state.tags[tag]], { stdio: 'ignore' });
    save(); fail('afterTag');
  } else {
    const endpoint = a[1];
    if (endpoint === '--paginate') {
      if (!a.includes('--slurp') || !a.includes('repos/LeonHorner/LegendOfCat/releases?per_page=100')) error(10);
      if (process.env.GH_MOCK_FAIL === 'listForbidden') { save(); process.stderr.write('gh: Resource not accessible by integration (HTTP 403)\\n'); process.exit(1); }
      if (process.env.GH_MOCK_FAIL === 'listNotFound') error();
      if (process.env.GH_MOCK_FAIL === 'listNetwork') { save(); process.stderr.write('network error\\n'); process.exit(1); }
      const releases = Object.values(state.releases);
      const pages = [];
      for (let i = 0; i < releases.length; i += 100) pages.push(releases.slice(i, i + 100));
      process.stdout.write(JSON.stringify(pages.length ? pages : [[]]));
    } else if (endpoint.includes('/git/ref/tags/')) {
      const value = endpoint.split('/').at(-1);
      if (!state.tags[value]) error();
      process.stdout.write(JSON.stringify({
        ref: 'refs/tags/' + value, object: { type: 'commit', sha: state.tags[value] }
      }));
    } else if (endpoint.includes('/releases/tags/')) {
      const value = endpoint.split('/').at(-1);
      if (!state.releases[value] || state.releases[value].draft) error();
      process.stdout.write(JSON.stringify(state.releases[value]));
    } else error(10);
  }
} else if (a[0] === 'release' && a[1] === 'create') {
  if (state.releases[tag] || !state.tags[tag] || !a.includes('--draft') ||
      !a.includes('--verify-tag') || a[a.indexOf('--target') + 1] !== process.env.GITHUB_SHA)
    error(8);
  state.releases[tag] = {
    tag_name: tag, name: tag, prerelease: false, draft: true,
    // GitHub may return its default branch even for a pre-existing tag.
    target_commitish: 'main',
    body: fs.readFileSync(a[a.indexOf('--notes-file') + 1], 'utf8'), assets: []
  };
  save(); fail('afterDraft');
} else if (a[0] === 'release' && a[1] === 'upload') {
  if (!state.releases[tag]?.draft) error(9);
  const files = a.slice(3, a.indexOf('--repo'));
  for (const file of files) {
    const buffer = fs.readFileSync(file);
    const name = require('node:path').basename(file);
    if (state.releases[tag].assets.some(asset => asset.name === name)) error(6);
    state.releases[tag].assets.push({
      name, state: 'uploaded', size: buffer.length,
      digest: 'sha256:' + crypto.createHash('sha256').update(buffer).digest('hex')
    });
    save(); fail('duringUpload');
  }
} else if (a[0] === 'release' && a[1] === 'edit') {
  if (!state.releases[tag]?.draft || !a.includes('--draft=false')) error(9);
  fail('beforePublish');
  state.releases[tag].draft = false;
  save(); fail('afterPublish');
} else error(11);
save();
`;

function git(cwd, ...args) {
  return execFileSync('git', args, { cwd, encoding: 'utf8' }).trim();
}
function fixture() {
  const cwd = mkdtempSync(join(tmpdir(), 'legendofcat-release-retry-'));
  git(cwd, 'init', '-b', 'main');
  git(cwd, 'config', 'user.email', 'tests@example.invalid');
  git(cwd, 'config', 'user.name', 'Release Test');
  git(cwd, 'commit', '--allow-empty', '-m', 'chore: baseline');
  const base = git(cwd, 'rev-parse', 'HEAD');
  git(cwd, 'tag', 'v0.7.0');
  git(cwd, 'commit', '--allow-empty', '-m', 'fix: correct movement');
  const snapshot = git(cwd, 'rev-parse', 'HEAD');
  git(cwd, 'checkout', '--detach', snapshot);
  git(cwd, 'commit', '--allow-empty', '-m', 'feat: newer PR merged');
  const laterMain = git(cwd, 'rev-parse', 'HEAD');
  git(cwd, 'branch', '-f', 'main', laterMain);
  git(cwd, 'checkout', '--detach', snapshot);
  const published = join(cwd, 'artifacts', 'published');
  mkdirSync(published, { recursive: true });
  for (const name of Object.values(DOWNLOADS)) writeFileSync(join(published, name), 'fixture asset ' + name);
  // Real packaging also includes update feeds and nupkg; use dummy files here
  // because the recovery layer only needs to prove exactly-once upload behavior.
  const bin = join(cwd, 'bin');
  mkdirSync(bin);
  writeFileSync(join(bin, 'gh'), fakeGh);
  chmodSync(join(bin, 'gh'), 0o755);
  const statePath = join(cwd, 'state.json');
  const state = {
    tags: { 'v0.7.0': base },
    releases: {
      'v0.7.0': { tag_name: 'v0.7.0', name: 'v0.7.0',
        target_commitish: base, draft: false, prerelease: false, body: '', assets: [] }
    },
    calls: []
  };
  writeFileSync(statePath, JSON.stringify(state));
  const env = {
    ...process.env, PATH: bin + delimiter + process.env.PATH,
    GITHUB_REF: 'refs/heads/main', GITHUB_SHA: snapshot,
    GITHUB_TOKEN: 'mock-token',
    ALLOW_MINOR: 'true', ALLOW_MAJOR: 'false', EXPECTED: '0.7.1', ACTUAL: '0.7.1',
    GH_MOCK_STATE: statePath
  };
  const run = phase => spawnSync(process.execPath, [script], {
    cwd, env: { ...env, GH_MOCK_FAIL: phase || '' },
    encoding: 'utf8', timeout: 35000
  });
  const read = () => JSON.parse(readFileSync(statePath, 'utf8'));
  return { cwd, snapshot, laterMain, run, read, write: state => writeFileSync(statePath, JSON.stringify(state)) };
}

for (const phase of ['afterTag', 'afterDraft', 'duringUpload', 'beforePublish']) {
  test('retry resumes safely after ' + phase + ' without moving tag or overwriting draft assets', () => {
    const f = fixture();
    try {
      const first = f.run(phase);
      assert.notEqual(first.status, 0, 'Injected failure must interrupt the first run');
      const stateBefore = f.read();
      assert.equal(stateBefore.tags['v0.7.1'], f.snapshot);
      assert.ok(!stateBefore.releases['v0.7.1'] || stateBefore.releases['v0.7.1'].draft);
      const done = f.run();
      assert.equal(done.status, 0, done.stderr);
      const state = f.read();
      assert.equal(state.tags['v0.7.1'], f.snapshot);
      assert.equal(state.releases['v0.7.1'].draft, false);
      assert.equal(state.releases['v0.7.1'].assets.length, 3);
      assert.equal(git(f.cwd, 'rev-parse', 'main'), f.laterMain);
      assert.equal(state.calls.filter(a => a[0] === 'api' && a.includes('POST')).length, 1,
        'Git tag must be create-only');
      assert.equal(state.calls.filter(a => a[0] === 'release' && a[1] === 'create').length, 1,
        'Draft must not be recreated');
      const uploadCalls = state.calls.filter(a => a[0] === 'release' && a[1] === 'upload');
      assert.equal(new Set(uploadCalls.flatMap(a => a.slice(3, a.indexOf('--repo')))).size, 3,
        'Already uploaded matching assets must not be uploaded again');
      const again = f.run();
      assert.notEqual(again.status, 0, 'A published release cannot be overwritten');
      assert.equal(f.read().calls.filter(a => a[0] === 'release' && a[1] === 'edit').length,
        state.calls.filter(a => a[0] === 'release' && a[1] === 'edit').length);
    } finally { rmSync(f.cwd, { recursive: true, force: true }); }
  });
}

test('published despite lost edit response cannot be overwritten by a retry', () => {
  const f = fixture();
  try {
    assert.notEqual(f.run('afterPublish').status, 0);
    assert.equal(f.read().releases['v0.7.1'].draft, false);
    const again = f.run();
    assert.notEqual(again.status, 0);
    assert.equal(f.read().releases['v0.7.1'].draft, false);
    assert.equal(f.read().calls.filter(a => a[0] === 'release' && a[1] === 'create').length, 1);
  } finally { rmSync(f.cwd, { recursive: true, force: true }); }
});

test('conflicting tag SHA and contradictory draft states are rejected without writes', () => {
  for (const conflict of ['wrongTag', 'missingRef', 'published', 'wrongDraft']) {
    const f = fixture();
    try {
      const state = f.read();
      state.tags['v0.7.1'] = conflict === 'wrongTag' ? state.tags['v0.7.0'] : f.snapshot;
      if (conflict !== 'missingRef') git(f.cwd, 'tag', 'v0.7.1', state.tags['v0.7.1']);
      if (conflict === 'missingRef') delete state.tags['v0.7.1'];
      state.releases['v0.7.1'] = {
        tag_name: 'v0.7.1', name: conflict === 'wrongDraft' ? 'DIFFERENT' : 'v0.7.1',
        target_commitish: f.snapshot,
        draft: conflict !== 'published', prerelease: false, body: '', assets: []
      };
      f.write(state);
      const result = f.run();
      assert.notEqual(result.status, 0, conflict);
      assert.ok(!f.read().calls.some(a => a[0] === 'release' && ['create','edit','upload'].includes(a[1])), conflict);
    } finally { rmSync(f.cwd, { recursive: true, force: true }); }
  }
});

test('existing asset checksum must match before retry can upload or publish', async () => {
  const f = fixture();
  try {
    assert.notEqual(f.run('duringUpload').status, 0);
    const state = f.read();
    state.releases['v0.7.1'].assets[0].digest = 'sha256:' + '0'.repeat(64);
    f.write(state);
    const before = f.read().calls.length;
    const result = f.run();
    assert.notEqual(result.status, 0);
    assert.ok(/mismatch/.test(result.stderr));
    const calls = f.read().calls.slice(before);
    assert.ok(!calls.some(a => a[0] === 'release' && ['upload', 'edit'].includes(a[1])));
  } finally { rmSync(f.cwd, { recursive: true, force: true }); }
});

test('plan detects a same-snapshot incomplete tag, but rejects a different snapshot', async () => {
  const f = fixture();
  try {
    assert.notEqual(f.run('afterTag').status, 0);
    const state = f.read();
    const get = tag => ({
      sha: state.tags[tag] || null,
      release: state.releases[tag] || null
    });
    const plan = await planRelease({ cwd: f.cwd, snapshotSha: f.snapshot, inspectTagState: get });
    assert.equal(plan.incompleteTag, 'v0.7.1');
    assert.equal(plan.nextRelease.version, '0.7.1');
    assert.equal(plan.nextRelease.gitHead, f.snapshot);
    git(f.cwd, 'checkout', '--detach', f.laterMain);
    await assert.rejects(
      () => planRelease({ cwd: f.cwd, snapshotSha: f.laterMain, inspectTagState: get }),
      /Unfinished release/
    );
  } finally { rmSync(f.cwd, { recursive: true, force: true }); }
});


test('GitHub CLI recognizes actual 404 formats without hiding other failures', () => {
  for (const stderr of ['gh: Not Found (HTTP 404)\n', 'HTTP 404: Not Found\n']) {
    assert.equal(getJson('releases/tags/v1.0.0', () => { throw Object.assign(new Error('gh failed'), { status: 1, stderr }); }), null);
  }
  for (const [status, stderr] of [[1, 'gh: Forbidden (HTTP 403)'], [1, 'network timeout'], [1, 'gh: Bad Gateway (HTTP 502)'], [2, 'gh: Not Found (HTTP 404)']]) {
    assert.throws(() => getJson('releases/tags/v1.0.0', () => { throw Object.assign(new Error(stderr), { status, stderr }); }));
  }
  assert.throws(() => getJson('releases/tags/v1.0.0', () => '{invalid'), SyntaxError);
  for (const value of ['null', '[]', 'false'])
    assert.throws(() => getJson('releases/tags/v1.0.0', () => value), /Invalid GitHub API response/);
});

test('incomplete starter and unknown asset states block recovery without upload or publication', () => {
  for (const assetState of ['starter', 'processing', undefined]) {
    const f = fixture();
    try {
      assert.notEqual(f.run('duringUpload').status, 0);
      const state = f.read();
      state.releases['v0.7.1'].assets[0].state = assetState;
      f.write(state);
      const before = state.calls.length;
      const result = f.run();
      assert.notEqual(result.status, 0);
      assert.match(result.stderr, /manual recovery instructions/);
      assert.ok(!f.read().calls.slice(before).some(a => a[0] === 'release' && ['upload', 'edit'].includes(a[1])));
    } finally { rmSync(f.cwd, { recursive: true, force: true }); }
  }
});


test('new release completes the full simulated publication sequence', () => {
  const f = fixture();
  try {
    const result = f.run();
    assert.equal(result.status, 0, result.stderr);
    const state = f.read();
    assert.equal(state.tags['v0.7.1'], f.snapshot);
    assert.equal(state.releases['v0.7.1'].draft, false);
    assert.equal(state.releases['v0.7.1'].assets.length, 3);
    const writes = state.calls.filter(a => a.includes('POST') || (a[0] === 'release' && ['create', 'upload', 'edit'].includes(a[1])));
    assert.deepEqual(writes.map(a => a[0] === 'api' ? 'tag' : a[1]), ['tag', 'create', 'upload', 'edit']);
    assert.ok(state.calls.filter(a => a.includes('--paginate') && a.includes('--slurp')).length >= 4);
    assert.ok(!state.calls.some(a => a.some(x => x.includes('/releases/tags/'))));
  } finally { rmSync(f.cwd, { recursive: true, force: true }); }
});

test('drafts on a later release-list page are found using the actual remote tag', () => {
  const f = fixture();
  try {
    assert.notEqual(f.run('afterDraft').status, 0);
    const state = f.read();
    const draft = state.releases['v0.7.1'];
    delete state.releases['v0.7.1'];
    for (let i = 0; i < 101; i++) state.releases['filler' + i] = { tag_name: 'filler' + i };
    state.releases['v0.7.1'] = draft;
    f.write(state);
    const result = f.run();
    assert.equal(result.status, 0, result.stderr);
    assert.equal(f.read().releases['v0.7.1'].draft, false);
    assert.equal(f.read().calls.filter(a => a[0] === 'release' && a[1] === 'create').length, 1);
  } finally { rmSync(f.cwd, { recursive: true, force: true }); }
});

test('release-list permission, not-found and network errors stop before writes', () => {
  for (const phase of ['listForbidden', 'listNotFound', 'listNetwork']) {
    const f = fixture();
    try {
      const result = f.run(phase);
      assert.notEqual(result.status, 0, phase);
      assert.ok(!f.read().calls.some(a => a.includes('POST') || (a[0] === 'release' && ['create', 'upload', 'edit'].includes(a[1]))), phase);
    } finally { rmSync(f.cwd, { recursive: true, force: true }); }
  }
});

test('malformed, duplicate or failing release-list responses never mean absent', () => {
  const fake = response => (...args) => {
    if (args.includes('--paginate')) return response;
    throw new Error('unexpected ref lookup');
  };
  for (const response of ['null', '[]', '{}', '[{}]', '[[null]]', '[[{}]]', 'not json'])
    assert.throws(() => inspectTag('v0.7.1', fake(response)));
  assert.throws(() => inspectTag('v0.7.1', fake('[[{"tag_name":"v0.7.1"}],[{"tag_name":"v0.7.1"}]]')), /Multiple/);
  assert.throws(() => inspectTag('v0.7.1', () => { throw new Error('permission denied'); }), /permission denied/);
});
