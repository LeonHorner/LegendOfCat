import { test } from 'node:test';
import assert from 'node:assert/strict';
import { assertReleaseApproval } from './release-approval.mjs';

test('patch releases always work regardless of toggles', () => {
  for (const minor of [undefined, false, 'false', true, 'true'])
    for (const major of [undefined, false, 'false', true, 'true'])
      assert.doesNotThrow(() =>
        assertReleaseApproval({ version: '0.7.1', type: 'patch' }, minor, major));
});

test('default permits minor releases but blocks major releases', () => {
  assert.doesNotThrow(() =>
    assertReleaseApproval({ version: '0.8.0', type: 'minor' }, 'true', 'false'));
  assert.throws(() =>
    assertReleaseApproval({ version: '1.0.0', type: 'major' }, 'true', 'false'),
    /allow_major/);
});

test('minor is blocked when its own permission is disabled or missing', () => {
  for (const minor of [undefined, false, 'false', '', null])
    assert.throws(() =>
      assertReleaseApproval({ version: '0.8.0', type: 'minor' }, minor, true),
      /allow_minor/);
  assert.doesNotThrow(() =>
    assertReleaseApproval({ version: '0.8.0', type: 'minor' }, true, false));
});

test('major needs its own permission even when minor is enabled', () => {
  for (const major of [undefined, false, 'false', '', null])
    assert.throws(() =>
      assertReleaseApproval({ version: '1.0.0', type: 'major' }, true, major),
      /allow_major/);
  // Separate switches: approving major does not implicitly approve minor.
  assert.doesNotThrow(() =>
    assertReleaseApproval({ version: '1.0.0', type: 'major' }, false, 'true'));
});

test('unexpected release types and absent plans fail closed', () => {
  assert.throws(() => assertReleaseApproval(null, true, true), /No release version/);
  assert.throws(() => assertReleaseApproval({ version: '0.8.0' }, true, true), /Unsupported release type/);
  assert.throws(() =>
    assertReleaseApproval({ version: '0.8.0', type: 'preview' }, true, true),
    /Unsupported release type/);
});
