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
