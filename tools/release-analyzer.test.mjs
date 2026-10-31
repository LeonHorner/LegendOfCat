import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { analyzeCommits } from '@semantic-release/commit-analyzer';
import { generateNotes } from '@semantic-release/release-notes-generator';

const config = JSON.parse(readFileSync(new URL('../.releaserc.json', import.meta.url), 'utf8'));
function pluginOptions(name) {
  const plugin = config.plugins.find(p => Array.isArray(p) && p[0] === name);
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

test('PR #66 prose about breaking-change descriptions remains patch', async () => {
  // Reproduces the real PR #66 squash-commit body that incorrectly triggered v1.0.0.
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
  const notes = await generateNotes(notesOptions, {
    commits: [{ hash: 'abc123', message }],
    lastRelease: { version: '0.7.0', gitTag: 'v0.7.0' },
    nextRelease: { version: '0.7.1', gitTag: 'v0.7.1', type: 'patch' },
    options: { repositoryUrl },
    logger
  });
  assert.doesNotMatch(notes, /BREAKING CHANGES/);
});

test('explicit breaking footers and bang headers still produce major releases', async () => {
  for (const message of [
    'feat!: intentionally change the save format',
    'fix: change save API\n\nBREAKING CHANGE: old saves are incompatible',
    'fix: change save API\n\nBREAKING-CHANGE: old saves are incompatible'
  ]) assert.equal(await analyze(message), 'major', message);
});

test('real breaking notes remain visible in the generated changelog', async () => {
  const message = 'fix: change save format\\n\\nBREAKING CHANGE: old saves are incompatible';
  const notes = await generateNotes(notesOptions, {
    commits: [{ hash: 'abc123', message }],
    lastRelease: { version: '0.7.0', gitTag: 'v0.7.0' },
    nextRelease: { version: '1.0.0', gitTag: 'v1.0.0', type: 'major' },
    options: { repositoryUrl },
    logger
  });
  assert.match(notes, /BREAKING CHANGES/);
});

test('normal fix, feature and refactor commits retain expected types', async () => {
  assert.equal(await analyze('fix: disable Unity splash screen'), 'patch');
  assert.equal(await analyze('feat: add movement tutorial'), 'minor');
  assert.ok((await analyze('refactor: rename internal renderer')) == null);
});
