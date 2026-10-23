import { test } from 'node:test';
import assert from 'node:assert/strict';
import { createHash } from 'node:crypto';
import { copyFileSync, existsSync, mkdtempSync, mkdirSync, readFileSync, readdirSync, rmSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import {
  assertFeedAsset, compareVersions, finalizeRelease, packageName, prepareRelease, selectBaseline
} from './delta-release.mjs';

const version = '0.3.0';
const repository = 'LeonHorner.LegendOfCat';

function temporary(fn) {
  const root = mkdtempSync(join(tmpdir(), 'legendofcat-delta-tests-'));
  try { fn(root); } finally { rmSync(root, { recursive: true, force: true }); }
}

function entry(file, path, type, release = version) {
  const buffer = readFileSync(path);
  return {
    PackageId: repository, FileName: file, Version: release,
    Type: type, Size: buffer.length,
    SHA1: createHash('sha1').update(buffer).digest('hex').toUpperCase()
  };
}

function fixture(root, platform = 'win', prior = '0.2.0') {
  const directory = join(root, platform);
  mkdirSync(directory);
  const full = packageName(version, platform);
  const fullPath = join(directory, full);
  writeFileSync(fullPath, Buffer.alloc(1000, 42));
  const delta = packageName(version, platform, 'delta');
  const deltaPath = join(directory, delta);
  writeFileSync(deltaPath, Buffer.alloc(100, 18));
  const previous = prior ? { tag: 'v' + prior, version: prior, full: packageName(prior, platform) } : null;
  if (previous) writeFileSync(join(directory, previous.full), Buffer.from('old version'));
  const feedName = 'releases.' + platform + '.json';
  const feed = { Assets: [
    entry(full, fullPath, 'Full'),
    entry(delta, deltaPath, 'Delta'),
    ...(previous ? [entry(previous.full, join(directory, previous.full), 'Full', prior)] : [])
  ] };
  writeFileSync(join(directory, feedName), JSON.stringify(feed));
  const stateFile = join(root, 'delta-state.json');
  writeFileSync(stateFile, JSON.stringify({ channel: platform, targetVersion: version, previous }));
  return { directory, stateFile, full, delta, fullPath, deltaPath, previous, feedName };
}

test('selects only a compatible stable previous release and correct channel', () => {
  for (const channel of ['win', 'linux']) {
    const assetName = packageName('0.2.0', channel);
    const release = { tag_name: 'v0.2.0', draft: false, prerelease: false, assets: [
      { name: assetName }, { name: 'releases.' + channel + '.json' }
    ] };
    assert.deepEqual(selectBaseline(release, version, channel), {
      tag: 'v0.2.0', version: '0.2.0', full: assetName
    });
    assert.throws(() => selectBaseline(release, '0.2.0', channel), /precede/);
    assert.throws(() => selectBaseline(release, '0.1.0', channel), /precede/);
    assert.throws(() => selectBaseline({ ...release, prerelease: true }, version, channel), /not a stable/);
    assert.throws(() => selectBaseline({ ...release, assets: release.assets.slice(0, 1) }, version, channel), /feed/);
    assert.throws(() => selectBaseline({ ...release, assets: release.assets.slice(1) }, version, channel), /full package/);
  }
  assert.equal(compareVersions('0.2.10', '0.2.9'), 1);
  assert.equal(selectBaseline(null, version, 'win'), null);
});

test('baseline downloads require a nonempty matching package; no baseline supports full-only', () => {
  temporary(root => {
    const folder = join(root, 'new');
    const state = join(root, 'state', 'win.json');
    const empty = prepareRelease('win', version, folder, state, { getLatest: () => null });
    assert.equal(empty, null);
    assert.equal(JSON.parse(readFileSync(state, 'utf8')).previous, null);
    const other = join(root, 'bad');
    const release = { tag_name: 'v0.2.0', draft: false, prerelease: false, assets: [
      { name: packageName('0.2.0', 'win') }, { name: 'releases.win.json' }
    ] };
    assert.throws(() => prepareRelease('win', version, other, join(root, 'bad-state.json'), {
      getLatest: () => release, download: () => {}
    }), /Failed to download/);
  });
});

test('previous full package must match the signed-off feed checksum', () => {
  temporary(root => {
    const folder = join(root, 'fresh');
    const state = join(root, 'state.json');
    const previousVersion = '0.2.0';
    const previousFile = packageName(previousVersion, 'win');
    const priorRelease = { tag_name: 'v0.2.0', draft: false, prerelease: false,
      assets: [{ name: previousFile }, { name: 'releases.win.json' }] };
    const downloaded = {
      getLatest: () => priorRelease,
      download: () => writeFileSync(join(folder, previousFile), 'valid base'),
      downloadFeed: (_previous, scratch) => {
        const full = join(folder, previousFile);
        writeFileSync(join(scratch, 'releases.win.json'), JSON.stringify({
          Assets: [entry(previousFile, full, 'Full', previousVersion)]
        }));
      }
    };
    const selected = prepareRelease('win', version, folder, state, downloaded);
    assert.equal(selected.version, previousVersion);
    assert.equal(JSON.parse(readFileSync(state, 'utf8')).previous.full, previousFile);
  });
  temporary(root => {
    const folder = join(root, 'fresh');
    const state = join(root, 'state.json');
    const previousFile = packageName('0.2.0', 'win');
    const release = { tag_name: 'v0.2.0', draft: false, prerelease: false,
      assets: [{ name: previousFile }, { name: 'releases.win.json' }] };
    assert.throws(() => prepareRelease('win', version, folder, state, {
      getLatest: () => release,
      download: () => writeFileSync(join(folder, previousFile), 'corrupt base'),
      downloadFeed: (_previous, scratch) => writeFileSync(
        join(scratch, 'releases.win.json'), JSON.stringify({
          Assets: [{ PackageId: repository, FileName: previousFile,
            Version: '0.2.0', Type: 'Full', SHA1: 'wrong', Size: 12 }]
        })
      )
    }), /SHA-1 mismatch/);
  });
});

test('verified Delta is published when smaller and previous assets are stripped', () => {
  for (const platform of ['win', 'linux']) temporary(root => {
    const { directory, stateFile, full, delta, previous, feedName } = fixture(root, platform);
    const result = finalizeRelease(platform, version, directory, stateFile, {
      patch: (_base, _patch, target) => copyFileSync(join(directory, full), target)
    });
    assert.equal(result.publishDelta, true);
    assert.equal(existsSync(join(directory, previous.full)), false);
    const names = JSON.parse(readFileSync(join(directory, feedName), 'utf8')).Assets.map(a => a.FileName);
    assert.deepEqual(names, [full, delta]);
  });
});

test('Delta larger than threshold is discarded after successful patch verification', () => {
  temporary(root => {
    const { directory, stateFile, full, delta, deltaPath, feedName } = fixture(root);
    writeFileSync(deltaPath, Buffer.alloc(950, 17));
    const feed = JSON.parse(readFileSync(join(directory, feedName), 'utf8'));
    feed.Assets[1] = entry(delta, deltaPath, 'Delta');
    writeFileSync(join(directory, feedName), JSON.stringify(feed));
    const result = finalizeRelease('win', version, directory, stateFile, {
      patch: (_base, _patch, target) => copyFileSync(join(directory, full), target)
    });
    assert.equal(result.publishDelta, false);
    assert.equal(existsSync(deltaPath), false);
    assert.deepEqual(JSON.parse(readFileSync(join(directory, feedName), 'utf8')).Assets.map(a => a.FileName), [full]);
  });
});

test('exactly 10% savings is an eligible Delta', () => {
  temporary(root => {
    const { directory, stateFile, full, delta, deltaPath, feedName } = fixture(root);
    writeFileSync(deltaPath, Buffer.alloc(900, 17));
    const feedPath = join(directory, feedName);
    const feed = JSON.parse(readFileSync(feedPath, 'utf8'));
    feed.Assets[1] = entry(delta, deltaPath, 'Delta');
    writeFileSync(feedPath, JSON.stringify(feed));
    const result = finalizeRelease('win', version, directory, stateFile, {
      patch: (_base, _patch, destination) => copyFileSync(join(directory, full), destination)
    });
    assert.equal(result.publishDelta, true);
  });
});

test('SHA-256 feed hashes are verified for full and Delta packages when present', () => {
  temporary(root => {
    const { directory, stateFile, feedName } = fixture(root);
    const feedPath = join(directory, feedName);
    const feed = JSON.parse(readFileSync(feedPath, 'utf8'));
    for (const asset of feed.Assets) {
      asset.SHA256 = createHash('sha256').update(readFileSync(join(directory, asset.FileName)))
        .digest('hex').toUpperCase();
    }
    writeFileSync(feedPath, JSON.stringify(feed));
    const result = finalizeRelease('win', version, directory, stateFile, {
      patch: (_base, _patch, destination) => copyFileSync(join(directory, feed.Assets[0].FileName), destination)
    });
    assert.equal(result.publishDelta, true);
  });
  temporary(root => {
    const { directory, stateFile, feedName } = fixture(root);
    const feedPath = join(directory, feedName);
    const feed = JSON.parse(readFileSync(feedPath, 'utf8'));
    feed.Assets[1].SHA256 = 'BAD';
    writeFileSync(feedPath, JSON.stringify(feed));
    assert.throws(() => finalizeRelease('win', version, directory, stateFile), /SHA-256 mismatch/);
  });
});

test('Delta corruption, missing delta or tampered feed blocks release', () => {
  temporary(root => {
    const { directory, stateFile, deltaPath } = fixture(root);
    assert.throws(() => finalizeRelease('win', version, directory, stateFile, {
      patch: (_b, _d, output) => writeFileSync(output, Buffer.from('wrong content'))
    }), /reconstruction differs/);
    rmSync(deltaPath);
    assert.throws(() => finalizeRelease('win', version, directory, stateFile), /missing or empty|did not generate|Unrecognized or missing new-version/);
  });
  temporary(root => {
    const { directory, stateFile, feedName } = fixture(root);
    const feedPath = join(directory, feedName);
    const feed = JSON.parse(readFileSync(feedPath, 'utf8'));
    feed.Assets[0].SHA1 = 'bad';
    writeFileSync(feedPath, JSON.stringify(feed));
    assert.throws(() => finalizeRelease('win', version, directory, stateFile), /checksum mismatch/);
  });
});

test('full-only releases reject stray delta packages or inconsistent feed', () => {
  temporary(root => {
    const { directory, stateFile, deltaPath, feedName } = fixture(root, 'win', null);
    assert.throws(() => finalizeRelease('win', version, directory, stateFile), /Unexpected Delta|Unrecognized or missing new-version/);
    rmSync(deltaPath);
    const feedPath = join(directory, feedName);
    const feed = JSON.parse(readFileSync(feedPath, 'utf8'));
    feed.Assets = feed.Assets.slice(0, 1);
    writeFileSync(feedPath, JSON.stringify(feed));
    const result = finalizeRelease('win', version, directory, stateFile);
    assert.equal(result.publishDelta, false);
  });
});
