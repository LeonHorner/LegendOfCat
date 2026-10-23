import { test } from 'node:test';
import assert from 'node:assert/strict';
import { mkdtempSync, mkdirSync, writeFileSync, readFileSync, readdirSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { DOWNLOADS, prepareAssets } from './release-assets.mjs';
import { downloadNotes, generateNotes } from './release-notes.mjs';

function fixture(run) {
  const root = mkdtempSync(join(tmpdir(), 'legendofcat-assets-'));
  const source = join(root, 'packages');
  const destination = join(root, 'public');
  for (const platform of ['win', 'linux']) {
    const dir = join(source, platform);
    mkdirSync(dir, { recursive: true });
    const name = 'LeonHorner.LegendOfCat-0.3.0-' + platform + '-full.nupkg';
    writeFileSync(join(dir, name), 'package');
    writeFileSync(join(dir, 'releases.' + platform + '.json'),
      JSON.stringify({ Assets: [{ FileName: name, PackageId: 'LeonHorner.LegendOfCat' }] }));
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

test('stages three stable player names and only the required updater files', () => {
  fixture((source, destination) => {
    const staged = prepareAssets(source, destination);
    assert.deepEqual(new Set(staged), new Set([
      DOWNLOADS.windows, DOWNLOADS.portable, DOWNLOADS.linux,
      'releases.win.json', 'releases.linux.json',
      'LeonHorner.LegendOfCat-0.3.0-win-full.nupkg',
      'LeonHorner.LegendOfCat-0.3.0-linux-full.nupkg'
    ]));
    assert.deepEqual(new Set(readdirSync(destination)), new Set(staged));
    assert.equal(readFileSync(join(destination, DOWNLOADS.linux), 'utf8'), 'appimage');
    assert.equal(readFileSync(join(destination, 'releases.win.json'), 'utf8'),
      readFileSync(join(source, 'win', 'releases.win.json'), 'utf8'));
  });
});

test('refuses missing feed packages without creating an incomplete release', () => {
  fixture((source, destination) => {
    writeFileSync(join(source, 'linux', 'releases.linux.json'),
      JSON.stringify({ Assets: [{ FileName: 'missing.nupkg', PackageId: 'LeonHorner.LegendOfCat' }] }));
    assert.throws(() => prepareAssets(source, destination), /invalid or missing update package/);
    assert.equal(readdirSync(source).includes('win'), true);
  });
});

test('release notes link to the same stable names for a specific version', () => {
  const notes = downloadNotes('v0.3.0');
  for (const name of Object.values(DOWNLOADS)) {
    assert.ok(notes.includes('/releases/download/v0.3.0/' + name));
  }
  assert.equal(generateNotes({}, { nextRelease: { gitTag: 'v0.3.0' } }), notes);
});
