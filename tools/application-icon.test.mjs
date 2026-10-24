import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { createHash } from 'node:crypto';
import test from 'node:test';
import { largestPng, source, variant } from './application-icon.mjs';

const read = path => readFileSync(new URL('../' + path, import.meta.url), 'utf8');
test('authoritative user icon and lossless platform variant are present', () => {
  const ico = readFileSync(source);
  assert.equal(createHash('sha256').update(ico).digest('hex'), 'c2da18f97c6d8f27e8a476102cf3b4665bba737d5be257a642be8ae7395d5c08');
  assert.deepEqual(readFileSync(variant), largestPng(ico));
  assert.equal(readFileSync(variant).readUInt32BE(16), 256);
});
test('Unity and launcher embed icons during automated builds', () => {
  const build = read('Assets/Game/Editor/BuildGame.cs');
  assert.match(build, /SetIcons\(NamedBuildTarget.Unknown/);
  assert.match(build, /SetIcons\(NamedBuildTarget.Standalone/);
  assert.match(build, /Assets\/Game\/Branding\/icon.png/);
  assert.match(read('Launcher/Launcher.csproj'), /<ApplicationIcon>..\/tools\/icon.ico<\/ApplicationIcon>/);
});
test('both workflows package installers and desktop integration using original-derived icons', () => {
  for (const path of ['.github/workflows/release.yml', '.github/workflows/pull-request.yml']) {
    const yaml = read(path);
    assert.match(yaml, /tools\/icon.ico/);
    assert.match(yaml, /Assets\/Game\/Branding\/icon.png/);
    assert.match(yaml, /--icon \$icon/);
    assert.match(yaml, /dotnet vpk pack/);
  }
});
test('validation packaging uses a version accepted by Velopack without publishing', () => {
  assert.match(read('.github/workflows/pull-request.yml'), /--packVersion 0\.0\.1 /);
});
test('fixed history navigation precedes the scrolling content and uses unique release controls', () => {
  const ui = read('Assets/Game/Scripts/StarterScreen.Presentation.cs');
  assert.ok(ui.indexOf('Button("Back to settings", fixedNavigation: true)') < ui.indexOf('scroll = GUILayout.BeginScrollView(scroll)'));
  assert.match(ui, /"release-" \+ index/);
  assert.match(ui, /else if \(!fixedNavigation\) scroll.y/);
});
