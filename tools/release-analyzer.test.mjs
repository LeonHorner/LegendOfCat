import { test } from 'node:test';
import assert from 'node:assert/strict';
import releaseConfig from '../release.config.js';
import { analyzeCommits } from '@semantic-release/commit-analyzer';
import { generateNotes } from '@semantic-release/release-notes-generator';
import { assertReleaseApproval } from './release-approval.mjs';

function pluginOptions(name) {
  const plugin = releaseConfig.plugins.find(p => Array.isArray(p) && p[0] === name);
  assert.ok(plugin, 'Missing release plugin: ' + name);
  return plugin[1];
}
const analyzerOptions = pluginOptions('@semantic-release/commit-analyzer');
const notesOptions = pluginOptions('@semantic-release/release-notes-generator');
const logger = { log() {}, error() {} };
const repositoryUrl = 'https://github.com/LeonHorner/LegendOfCat';

async function analyze(message) {
  return analyzeCommits(analyzerOptions, {
    commits: [{ hash: 'abc123', message }],
    logger,
    options: { repositoryUrl }
  });
}

async function notes(message, type = 'patch') {
  return generateNotes(notesOptions, {
    commits: [{ hash: 'abc123', message }],
    lastRelease: { version: '0.7.0', gitTag: 'v0.7.0' },
    nextRelease: { version: { patch: '0.7.1', minor: '0.8.0', major: '1.0.0' }[type],
      gitTag: 'v' + { patch: '0.7.1', minor: '0.8.0', major: '1.0.0' }[type], type },
    options: { repositoryUrl },
    logger
  });
}

test('PR #66 prose never creates a major release or breaking changelog section', async () => {
  const message = [
    'fix: clean up in-game release history metadata (#66)',
    '',
    '## Changes',
    '- Add 14 parameterized Unity regression cases covering realistic',
    'published releases v0.5.0/v0.6.0/v0.7.0, mixed-case clauses, duplicated',
    'references, literal prose containing "closes", and preserved',
    'breaking-change descriptions.',
    '',
    '## Scope',
    '- GitHub Release bodies and version calculation are unchanged.'
  ].join('\n');
  assert.equal(await analyze(message), 'patch');
  assert.doesNotMatch(await notes(message), /BREAKING CHANGES/);
});

test('case variants of prose without a colon do not imply a breaking change', async () => {
  for (const line of [
    'breaking-change descriptions.', 'Breaking-Change descriptions.',
    'BREAKING-CHANGE descriptions.', 'breaking change descriptions.',
    'Breaking Change descriptions.', 'BREAKING CHANGE descriptions.'
  ]) {
    const message = 'fix: clean up release history\n\n' + line;
    assert.equal(await analyze(message), 'patch', line);
    assert.doesNotMatch(await notes(message), /BREAKING CHANGES/, line);
  }
});

test('only exact uppercase breaking footers make a major release', async () => {
  for (const marker of ['BREAKING CHANGE:', 'BREAKING-CHANGE:']) {
    const message = 'fix: change save format\n\n' + marker + ' old saves are incompatible';
    assert.equal(await analyze(message), 'major', marker);
    assert.match(await notes(message, 'major'), /BREAKING CHANGES/, marker);
  }
  for (const marker of [
    'breaking change:', 'Breaking Change:', 'bReAkInG cHaNgE:',
    'breaking-change:', 'Breaking-Change:', 'BREAKING-change:'
  ]) {
    const message = 'fix: explain save format\n\n' + marker + ' details';
    assert.equal(await analyze(message), 'patch', marker);
    assert.doesNotMatch(await notes(message), /BREAKING CHANGES/, marker);
  }
});

test('lowercase breaking footers preserve ordinary minor/patch releases', async () => {
  for (const marker of [
    'breaking change:', 'Breaking Change:', 'bReAkInG cHaNgE:',
    'breaking-change:', 'Breaking-Change:', 'BREAKING-change:'
  ]) {
    const body = '\n\n' + marker + ' ordinary text, not an authorized major';
    assert.equal(await analyze('feat: add new level' + body), 'minor', marker);
    assert.equal(await analyze('fix: correct movement' + body), 'patch', marker);
    assert.doesNotMatch(await notes('feat: add new level' + body, 'minor'),
      /BREAKING CHANGES/, marker);
  }
});

test('standard breaking shorthand always computes major release v1.0.0', async () => {
  for (const message of [
    'feat!: add a new level with incompatible saves',
    'fix!: alter existing save schema',
    'fix(api)!: change public API',
    'feat(gameplay)!: change gameplay contracts'
  ]) {
    assert.equal(await analyze(message), 'major', message);
    const changelog = await notes(message, 'major');
    assert.match(changelog, /BREAKING CHANGES/, message);
  }
});

test('release types and release permissions match realistic versions', async () => {
  for (const [message, type, version] of [
    ['fix: adjust camera', 'patch', '0.7.1'],
    ['feat: introduce new world', 'minor', '0.8.0'],
    ['feat!: redesign saved levels', 'major', '1.0.0'],
    ['fix!: incompatible save migration', 'major', '1.0.0'],
    ['fix(api)!: incompatible protocol', 'major', '1.0.0'],
    ['feat: modify save format\\n\\nBREAKING CHANGE: saves are incompatible', 'major', '1.0.0']
  ]) {
    const releaseType = await analyze(message);
    assert.equal(releaseType, type, message);
    const nextVersion = { patch: '0.7.1', minor: '0.8.0', major: '1.0.0' }[releaseType];
    assert.equal(nextVersion, version, message);
    if (releaseType === 'major') {
      assert.throws(() => assertReleaseApproval({ version, type }, true, false), /allow_major/);
      assert.doesNotThrow(() => assertReleaseApproval({ version, type }, false, true));
    } else {
      assert.doesNotThrow(() => assertReleaseApproval({ version, type }, true, false));
    }
    assert.ok((await notes(message, releaseType)).length > 0, message);
  }
});

test('fixes, features and CI commits retain their correct classification', async () => {
  assert.equal(await analyze('fix: disable Unity splash screen'), 'patch');
  assert.equal(await analyze('feat: add movement tutorial'), 'minor');
  assert.ok((await analyze('ci: harden release version classification and approval gates')) == null);
  assert.doesNotMatch(
    await notes('ci: harden release version classification and approval gates'),
    /harden release version classification/
  );
});
