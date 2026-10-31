import { test } from 'node:test';
import assert from 'node:assert/strict';
import { assertReleaseApproval } from './release-approval.mjs';

test('accepts exact patch and minor version approvals', () => {
  assert.doesNotThrow(() => assertReleaseApproval({ version: '0.7.1', type: 'patch' }, '0.7.1'));
  assert.doesNotThrow(() => assertReleaseApproval({ version: '0.8.0', type: 'minor' }, '0.8.0'));
});

test('rejects missing, invalid or mismatched version approvals', () => {
  for (const expected of [undefined, '', 'v0.7.1', '0.7', '01.0.0', '1.0.0'])
    assert.throws(
      () => assertReleaseApproval({ version: '0.7.1', type: 'patch' }, expected),
      /expected_version|mismatch/
    );
  assert.throws(() => assertReleaseApproval(null, '0.7.1'), /No release version/);
});

test('major version requires both exact number and separate approval', () => {
  const major = { version: '1.0.0', type: 'major' };
  assert.throws(() => assertReleaseApproval(major, '0.7.1', true), /mismatch/);
  assert.throws(() => assertReleaseApproval(major, '1.0.0'), /allow_major/);
  assert.throws(() => assertReleaseApproval(major, '1.0.0', 'false'), /allow_major/);
  assert.doesNotThrow(() => assertReleaseApproval(major, '1.0.0', 'true'));
});
