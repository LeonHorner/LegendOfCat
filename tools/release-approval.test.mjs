import { test } from 'node:test';
import assert from 'node:assert/strict';
import { assertReleaseApproval } from './release-approval.mjs';

test('patch and minor need no permission; major requires explicit approval', () => {
  for (const type of ['patch', 'minor']) assert.doesNotThrow(() => assertReleaseApproval({ type }, false, false));
  for (const value of [undefined, false, 'false', 'TRUE'])
    assert.throws(() => assertReleaseApproval({ type: 'major' }, true, value), /allow_major/);
  assert.doesNotThrow(() => assertReleaseApproval({ type: 'major' }, false, 'true'));
  assert.throws(() => assertReleaseApproval({ type: 'unknown' }), /Unsupported/);
});
