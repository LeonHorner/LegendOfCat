import { test } from 'node:test';
import assert from 'node:assert/strict';
import { assertReleaseApproval } from './release-approval.mjs';

test('release approvals are independent and default to minor enabled, major disabled', () => {
  for (const minor of [undefined, true, 'true', false, 'false']) {
    for (const major of [undefined, true, 'true', false, 'false']) {
      assert.doesNotThrow(() => assertReleaseApproval({ type: 'patch' }, minor, major));
      const checkMinor = () => assertReleaseApproval({ type: 'minor' }, minor, major);
      if (minor === undefined || minor === true || minor === 'true') assert.doesNotThrow(checkMinor);
      else assert.throws(checkMinor, /allow_minor/);
      const checkMajor = () => assertReleaseApproval({ type: 'major' }, minor, major);
      if (major === true || major === 'true') assert.doesNotThrow(checkMajor);
      else assert.throws(checkMajor, /allow_major/);
    }
  }
  assert.throws(() => assertReleaseApproval({ type: 'minor' }, 'TRUE', true), /allow_minor/);
  assert.throws(() => assertReleaseApproval({ type: 'major' }, true, 'TRUE'), /allow_major/);
  assert.throws(() => assertReleaseApproval({ type: 'unknown' }), /Unsupported/);
});
