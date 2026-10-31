import { test } from 'node:test';
import assert from 'node:assert/strict';
import { mkdtempSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { execFileSync } from 'node:child_process';
import { planRelease, assertSnapshot } from './release-engine.mjs';
import { assertReleaseApproval } from './release-approval.mjs';

function git(cwd, ...args) {
  return execFileSync('git', args, { cwd, encoding: 'utf8' }).trim();
}
function seedRepo() {
  const cwd = mkdtempSync(join(tmpdir(), 'legendofcat-release-snapshot-'));
  git(cwd, 'init', '-b', 'main');
  git(cwd, 'config', 'user.email', 'tests@example.invalid');
  git(cwd, 'config', 'user.name', 'Release Test');
  git(cwd, 'commit', '--allow-empty', '-m', 'chore: seed baseline');
  git(cwd, 'tag', 'v0.7.0');
  return cwd;
}
function add(cwd, title, body = '') {
  git(cwd, 'commit', '--allow-empty', '-m', title, ...(body ? ['-m', body] : []));
  return git(cwd, 'rev-parse', 'HEAD');
}

test('semantic-release analyzer plans the frozen commit despite main advancing', async () => {
  const cwd = seedRepo();
  try {
    const snapshot = add(cwd, 'fix: improve launch screen',
      'Clarify breaking-change descriptions. This is not a breaking footer.');
    const original = await planRelease({ cwd, snapshotSha: snapshot });
    assert.equal(original.nextRelease.version, '0.7.1');
    assert.equal(original.nextRelease.gitHead, snapshot);
    assert.equal(original.nextRelease.gitTag, 'v0.7.1');
    assert.doesNotThrow(() => assertReleaseApproval(original.nextRelease, true, false));

    // Another PR moves main while the release stays checked out at github.sha.
    git(cwd, 'checkout', '--detach', snapshot);
    const nextMain = add(cwd, 'feat: newly merged feature');
    git(cwd, 'branch', '-f', 'main', nextMain);
    git(cwd, 'checkout', '--detach', snapshot);
    assert.equal(git(cwd, 'rev-parse', 'main'), nextMain);
    assert.notEqual(git(cwd, 'rev-parse', 'main'), snapshot);
    const after = await planRelease({ cwd, snapshotSha: snapshot });
    assert.equal(after.nextRelease.version, '0.7.1');
    assert.equal(after.nextRelease.gitHead, snapshot);
    assert.equal(after.nextRelease.notes, original.nextRelease.notes);
    assert.doesNotThrow(() => assertSnapshot(cwd, snapshot));
    assert.throws(() => assertSnapshot(cwd, nextMain), /does not match workflow github.sha/);
  } finally {
    rmSync(cwd, { recursive: true, force: true });
  }
});

test('minor and major gates evaluate the actual pinned commit range', async () => {
  for (const [title, body, type, version] of [
    ['feat: add area', '', 'minor', '0.8.0'],
    ['fix!: change save format', '', 'major', '1.0.0'],
    ['fix: adjust saves', 'BREAKING CHANGE: older saves are incompatible', 'major', '1.0.0'],
    ['fix: clarify text', 'breaking-change descriptions.', 'patch', '0.7.1']
  ]) {
    const cwd = seedRepo();
    try {
      const sha = add(cwd, title, body);
      const result = await planRelease({ cwd, snapshotSha: sha });
      assert.equal(result.nextRelease.type, type, title);
      assert.equal(result.nextRelease.version, version, title);
      assert.equal(result.nextRelease.gitHead, sha, title);
      if (type === 'major')
        assert.throws(() => assertReleaseApproval(result.nextRelease, true, false), /allow_major/);
      if (type === 'minor')
        assert.throws(() => assertReleaseApproval(result.nextRelease, false, false), /allow_minor/);
      assert.doesNotThrow(() => assertReleaseApproval(result.nextRelease, true, true));
    } finally {
      rmSync(cwd, { recursive: true, force: true });
    }
  }
});

test('the last stable release is selected from the snapshot, not newer commits', async () => {
  const cwd = seedRepo();
  try {
    const snapshot = add(cwd, 'fix: fix UI');
    const newer = add(cwd, 'feat: add more features');
    git(cwd, 'tag', 'v0.8.0', newer);
    git(cwd, 'checkout', '--detach', snapshot);
    const release = await planRelease({ cwd, snapshotSha: snapshot });
    assert.equal(release.nextRelease.version, '0.7.1');
    assert.equal(release.nextRelease.gitHead, snapshot);
  } finally {
    rmSync(cwd, { recursive: true, force: true });
  }
});

test('publish targets and verifies the frozen SHA using a fake gh, never the moving main', async () => {
  const { chmodSync, mkdirSync, writeFileSync, readFileSync } = await import('node:fs');
  const { spawnSync } = await import('node:child_process');
  const { delimiter } = await import('node:path');
  const { fileURLToPath } = await import('node:url');
  const { DOWNLOADS } = await import('./release-assets.mjs');
  const cwd = seedRepo();
  try {
    const snapshot = add(cwd, 'fix: correct movement');
    git(cwd, 'checkout', '--detach', snapshot);
    const laterMain = add(cwd, 'feat: merged during release');
    git(cwd, 'branch', '-f', 'main', laterMain);
    git(cwd, 'checkout', '--detach', snapshot);

    const published = join(cwd, 'artifacts', 'published');
    mkdirSync(published, { recursive: true });
    for (const name of Object.values(DOWNLOADS))
      writeFileSync(join(published, name), 'dummy package');

    const bin = join(cwd, 'bin');
    mkdirSync(bin);
    const mockLog = join(cwd, 'gh-calls.jsonl');
    const mockRef = join(cwd, 'tag-ref.txt');
    const mockScript = String.raw`#!/usr/bin/env node
const fs = require('node:fs');
const a = process.argv.slice(2);
fs.appendFileSync(process.env.GH_MOCK_LOG, JSON.stringify(a) + '\n');
if (a[0] === 'api' && a.includes('POST')) {
  const ref = a.find(x => x.startsWith('ref='));
  const sha = a.find(x => x.startsWith('sha='));
  if (ref !== 'ref=refs/tags/v0.7.1' || !sha || fs.existsSync(process.env.GH_MOCK_REF))
    process.exit(7);
  fs.writeFileSync(process.env.GH_MOCK_REF, sha.slice(4));
} else if (a[0] === 'api' && a.includes('--jq')) {
  if (!fs.existsSync(process.env.GH_MOCK_REF)) process.exit(8);
  process.stdout.write(fs.readFileSync(process.env.GH_MOCK_REF, 'utf8') + '\n');
} else if (a[0] === 'release' && a[1] === 'create') {
  if (!a.includes('--draft') || !a.includes('--verify-tag') ||
      a[a.indexOf('--target') + 1] !== process.env.GITHUB_SHA ||
      !fs.existsSync(process.env.GH_MOCK_REF)) process.exit(9);
} else if (a[0] === 'release' && a[1] === 'edit') {
  if (!a.includes('--draft=false') || !a.includes('--latest')) process.exit(10);
} else {
  process.exit(11);
}
`;
    writeFileSync(join(bin, 'gh'), mockScript);
    chmodSync(join(bin, 'gh'), 0o755);
    const script = fileURLToPath(new URL('./publish-release.mjs', import.meta.url));
    const result = spawnSync(process.execPath, [script], {
      cwd, encoding: 'utf8', timeout: 30000,
      env: {
        ...process.env,
        PATH: bin + delimiter + process.env.PATH,
        GITHUB_REF: 'refs/heads/main', GITHUB_SHA: snapshot,
        ALLOW_MINOR: 'true', ALLOW_MAJOR: 'false',
        EXPECTED: '0.7.1', ACTUAL: '0.7.1',
        GITHUB_TOKEN: 'dummy-test-token',
        GH_MOCK_LOG: mockLog, GH_MOCK_REF: mockRef
      }
    });
    assert.equal(result.status, 0, result.stderr);
    assert.equal(readFileSync(mockRef, 'utf8'), snapshot);
    const calls = readFileSync(mockLog, 'utf8').trim().split('\n').map(JSON.parse);
    assert.equal(calls[0][0], 'api');
    assert.ok(calls[0].includes('ref=refs/tags/v0.7.1'));
    assert.ok(calls[0].includes('sha=' + snapshot));
    const created = calls.find(a => a[0] === 'release' && a[1] === 'create');
    assert.ok(created);
    assert.equal(created[created.indexOf('--target') + 1], snapshot);
    assert.equal(calls.at(-1)[0], 'api'); // post-publish tag recheck
    assert.equal(git(cwd, 'rev-parse', 'main'), laterMain);
  } finally {
    rmSync(cwd, { recursive: true, force: true });
  }
});
