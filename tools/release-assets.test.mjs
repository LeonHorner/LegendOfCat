import { test } from 'node:test';
import assert from 'node:assert/strict';
import { createHash } from 'node:crypto';
import {
  mkdtempSync, mkdirSync, writeFileSync, readFileSync, readdirSync, rmSync
} from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { DOWNLOADS, prepareAssets } from './release-assets.mjs';
import { packageName } from './delta-release.mjs';
import { downloadNotes, generateNotes } from './release-notes.mjs';

const VERSION = '0.3.0';
function fixture(run, withDeltas = false) {
  const root = mkdtempSync(join(tmpdir(), 'legendofcat-assets-'));
  const source = join(root, 'packages');
  const destination = join(root, 'public');
  for (const platform of ['win', 'linux']) {
    const dir = join(source, platform);
    mkdirSync(dir, { recursive: true });
    const full = packageName(VERSION, platform);
    const delta = packageName(VERSION, platform, 'delta');
    writeFileSync(join(dir, full), 'new full package');
    if (withDeltas) writeFileSync(join(dir, delta), 'new delta');
    const makeEntry = (name, type) => {
      const data = readFileSync(join(dir, name));
      return { PackageId: 'LeonHorner.LegendOfCat', FileName: name,
        Version: VERSION, Type: type, Size: data.length,
        SHA1: createHash('sha1').update(data).digest('hex').toUpperCase() };
    };
    writeFileSync(join(dir, 'releases.' + platform + '.json'),
      JSON.stringify({ Assets: [
        makeEntry(full, 'Full'),
        ...(withDeltas ? [makeEntry(delta, 'Delta')] : [])
      ] }));
    if (platform === 'win') {
      writeFileSync(join(dir, 'LeonHorner.LegendOfCat-win-Setup.exe'), 'installer');
      writeFileSync(join(dir, 'LeonHorner.LegendOfCat-win-Portable.zip'), 'portable');
      writeFileSync(join(dir, 'RELEASES'), 'old legacy feed');
      writeFileSync(join(dir, 'assets.win.json'), 'upload-only metadata');
    } else {
      writeFileSync(join(dir, 'LeonHorner.LegendOfCat.AppImage'), 'appimage');
      writeFileSync(join(dir, 'RELEASES-linux'), 'old legacy feed');
      writeFileSync(join(dir, 'assets.linux.json'), 'upload-only metadata');
    }
  }
  try { run(source, destination); } finally { rmSync(root, { recursive: true, force: true }); }
}

test('stages three stable player names and only the required full update packages', () => {
  fixture((source, destination) => {
    const staged = prepareAssets(source, destination, VERSION);
    assert.deepEqual(new Set(staged), new Set([
      DOWNLOADS.windows, DOWNLOADS.portable, DOWNLOADS.linux,
      'releases.win.json', 'releases.linux.json',
      packageName(VERSION, 'win'), packageName(VERSION, 'linux')
    ]));
    assert.deepEqual(new Set(readdirSync(destination)), new Set(staged));
    assert.equal(readFileSync(join(destination, DOWNLOADS.linux), 'utf8'), 'appimage');
    assert.equal(readFileSync(join(destination, 'releases.win.json'), 'utf8'),
      readFileSync(join(source, 'win', 'releases.win.json'), 'utf8'));
  });
});

test('stages verified Delta packages without modifying feeds', () => {
  fixture((source, destination) => {
    const staged = prepareAssets(source, destination, VERSION);
    assert.ok(staged.includes(packageName(VERSION, 'win', 'delta')));
    assert.ok(staged.includes(packageName(VERSION, 'linux', 'delta')));
    assert.equal(staged.length, 9);
  }, true);
});

test('rejects missing, inconsistent or stale release packages', () => {
  fixture((source, destination) => {
    writeFileSync(join(source, 'linux', 'releases.linux.json'),
      JSON.stringify({ Assets: [{ FileName: 'missing.nupkg', PackageId: 'LeonHorner.LegendOfCat' }] }));
    assert.throws(() => prepareAssets(source, destination, VERSION), /invalid or missing update package/);
  });
  fixture((source, destination) => {
    const old = packageName('0.2.0', 'linux');
    writeFileSync(join(source, 'linux', old), 'old full release');
    assert.throws(() => prepareAssets(source, destination, VERSION), /unexpected release package/);
  });
  fixture((source, destination) => {
    const feedPath = join(source, 'win', 'releases.win.json');
    const feed = JSON.parse(readFileSync(feedPath, 'utf8'));
    feed.Assets[0].PackageId = 'Another.Application';
    writeFileSync(feedPath, JSON.stringify(feed));
    assert.throws(() => prepareAssets(source, destination, VERSION), /invalid or missing update package/);
  });
});

test('README links and semantic-release asset paths agree with the staging contract', () => {
  const readme = readFileSync(new URL('../README.md', import.meta.url), 'utf8');
  const releaseConfig = JSON.parse(readFileSync(new URL('../.releaserc.json', import.meta.url), 'utf8'));
  for (const name of Object.values(DOWNLOADS)) {
    assert.ok(readme.includes('/releases/latest/download/' + name), name);
  }
  const notesIndex = releaseConfig.plugins.indexOf('./tools/release-notes.mjs');
  assert.ok(notesIndex !== -1);
  assert.ok(notesIndex < releaseConfig.plugins.findIndex(plugin =>
    Array.isArray(plugin) && plugin[0] === '@semantic-release/release-notes-generator'));
  const github = releaseConfig.plugins.find(plugin =>
    Array.isArray(plugin) && plugin[0] === '@semantic-release/github');
  assert.equal(github[1].assets[0].path, 'artifacts/published/*');
});

test('release notes link to the same stable names for a specific version', () => {
  const notes = downloadNotes('v0.3.0');
  for (const name of Object.values(DOWNLOADS)) {
    assert.ok(notes.includes('/releases/download/v0.3.0/' + name));
  }
  assert.equal(generateNotes({}, { nextRelease: { gitTag: 'v0.3.0' } }), notes);
});
