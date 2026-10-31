import { test } from 'node:test';
import assert from 'node:assert/strict';
import { validateReleaseMessages } from './release-commit-policy.mjs';

test('ordinary prose including the original #66 trigger is accepted in any case', () => {
  for (const line of [
    'breaking-change descriptions.', 'Breaking-Change descriptions.',
    'BREAKING-CHANGE descriptions.', 'breaking change descriptions.'
  ]) assert.doesNotThrow(() => validateReleaseMessages([
    'fix: clean up release history (#66)\n\n' + line
  ]));
});

test('only uppercase breaking footer markers are accepted', () => {
  for (const marker of ['BREAKING CHANGE:', 'BREAKING-CHANGE:'])
    assert.doesNotThrow(() => validateReleaseMessages([
      'fix: change save format\n\n' + marker + ' old saves are incompatible'
    ]));
  for (const marker of [
    'breaking change:', 'Breaking Change:', 'bReAkInG cHaNgE:',
    'breaking-change:', 'Breaking-Change:', 'BREAKING-change:'
  ]) assert.throws(() => validateReleaseMessages([
    'fix: change save format\n\n' + marker + ' old saves are incompatible'
  ]), /UPPERCASE/);
});

test('breaking shorthand cannot silently downgrade to minor/patch', () => {
  for (const title of [
    'feat!: breaking feature',
    'fix(api)!: incompatible save format',
    'refactor!: change public API'
  ]) assert.throws(() => validateReleaseMessages([title]), /shorthand/);
  assert.doesNotThrow(() => validateReleaseMessages([
    'feat: add feature\n\nBREAKING CHANGE: old saves are incompatible'
  ]));
});

test('normal versioning and the CI-only PR title remain acceptable', () => {
  assert.doesNotThrow(() => validateReleaseMessages([
    'ci: harden release version classification and approval gates',
    'feat: new area',
    'fix: correct splash screen'
  ]));
});
