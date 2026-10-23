import { createHash } from 'node:crypto';
import { execFileSync } from 'node:child_process';
import {
  appendFileSync, existsSync, mkdirSync, mkdtempSync, readFileSync, readdirSync,
  rmSync, statSync, unlinkSync, writeFileSync
} from 'node:fs';
import { basename, join, resolve } from 'node:path';
import { tmpdir } from 'node:os';
import { fileURLToPath } from 'node:url';

const REPOSITORY = 'LeonHorner/LegendOfCat';
export const PACKAGE_ID = 'LeonHorner.LegendOfCat';
export const MAX_DELTA_RATIO = 0.90;
export const CHANNELS = ['win', 'linux'];

function versionParts(version) {
  if (!/^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)$/.test(version))
    throw new Error('Expected a stable three-part release version: ' + version);
  return version.split('.').map(Number);
}

export function compareVersions(left, right) {
  const a = versionParts(left), b = versionParts(right);
  for (let i = 0; i < 3; i++) if (a[i] !== b[i]) return Math.sign(a[i] - b[i]);
  return 0;
}

export function packageName(version, channel, type = 'full') {
  versionParts(version);
  if (!CHANNELS.includes(channel) || !['full', 'delta'].includes(type))
    throw new Error('Invalid channel or package type');
  return `${PACKAGE_ID}-${version}${channel === 'linux' ? '-linux' : ''}-${type}.nupkg`;
}

export function selectBaseline(release, targetVersion, channel) {
  versionParts(targetVersion);
  if (!CHANNELS.includes(channel)) throw new Error('Invalid channel: ' + channel);
  if (release === null) return null; // GitHub has no published stable release yet.
  if (release.draft || release.prerelease || !/^v\d+\.\d+\.\d+$/.test(release.tag_name ?? ''))
    throw new Error('Latest GitHub release is not a stable vX.Y.Z release');
  const previous = release.tag_name.slice(1);
  if (compareVersions(previous, targetVersion) >= 0)
    throw new Error('Baseline must precede the target version');
  const full = packageName(previous, channel);
  const assets = release.assets ?? [];
  if (!assets.some(asset => asset.name === `releases.${channel}.json`))
    throw new Error('Previous release is missing the ' + channel + ' update feed');
  if (!assets.some(asset => asset.name === full))
    throw new Error('Previous release is missing compatible full package: ' + full);
  return { tag: release.tag_name, version: previous, full };
}

export function checksum(path, algorithm = 'sha1') {
  const hash = createHash(algorithm);
  hash.update(readFileSync(path));
  return hash.digest('hex');
}

export function assertFeedAsset(entry, path, expectedVersion, channel, type) {
  if (entry.PackageId !== PACKAGE_ID ||
      entry.Version !== expectedVersion ||
      entry.Type !== (type === 'full' ? 'Full' : 'Delta') ||
      entry.FileName !== packageName(expectedVersion, channel, type))
    throw new Error('Unexpected version, channel, type or package ID in release feed');
  if (!existsSync(path) || statSync(path).size === 0)
    throw new Error('Missing or empty release package: ' + path);
  if (!Number.isSafeInteger(entry.Size) || entry.Size !== statSync(path).size ||
      typeof entry.SHA1 !== 'string' || entry.SHA1.toLowerCase() !== checksum(path))
    throw new Error('Release feed size/SHA-1 mismatch for ' + entry.FileName);
  if (entry.SHA256 != null &&
      (typeof entry.SHA256 !== 'string' || entry.SHA256.toLowerCase() !== checksum(path, 'sha256')))
    throw new Error('Release feed SHA-256 mismatch for ' + entry.FileName);
}

function github(args, opts = {}) {
  return execFileSync('gh', args, { encoding: 'utf8', stdio: ['ignore', 'pipe', 'pipe'], ...opts });
}

function queryLatestStable() {
  try {
    return JSON.parse(github(['api', `repos/${REPOSITORY}/releases/latest`]));
  } catch (error) {
    const detail = String(error.stderr ?? '');
    if (!/HTTP 404|Not Found \(HTTP 404\)/.test(detail)) throw error;
    // Distinguish a legitimate first stable release from a repository/network error.
    JSON.parse(github(['api', `repos/${REPOSITORY}/releases?per_page=1`]));
    return null;
  }
}

export function prepareRelease(channel, targetVersion, outputDir, stateFile, deps = {}) {
  if (!CHANNELS.includes(channel)) throw new Error('Invalid channel');
  versionParts(targetVersion);
  if (!existsSync(outputDir)) mkdirSync(outputDir, { recursive: true });
  if (readdirSync(outputDir).length)
    throw new Error('Release output directory must be empty before downloading a baseline');
  const previous = selectBaseline((deps.getLatest ?? queryLatestStable)(), targetVersion, channel);
  if (previous) {
    (deps.download ?? ((base) => github([
      'release', 'download', base.tag, '--repo', REPOSITORY,
      '--pattern', base.full, '--dir', outputDir
    ])))(previous);
    const basePath = join(outputDir, previous.full);
    if (!existsSync(basePath) || !statSync(basePath).isFile() || statSync(basePath).size === 0)
      throw new Error('Failed to download previous full package: ' + previous.full);
    const scratch = mkdtempSync(join(tmpdir(), 'legendofcat-base-feed-'));
    try {
      (deps.downloadFeed ?? ((base) => github([
        'release', 'download', base.tag, '--repo', REPOSITORY,
        '--pattern', `releases.${channel}.json`, '--dir', scratch
      ])))(previous, scratch);
      const sourceFeed = JSON.parse(readFileSync(join(scratch, `releases.${channel}.json`), 'utf8'));
      const candidates = sourceFeed.Assets?.filter(item =>
        item.FileName === previous.full && item.Version === previous.version && item.Type === 'Full');
      if (candidates?.length !== 1) throw new Error('Incompatible previous release feed');
      assertFeedAsset(candidates[0], basePath, previous.version, channel, 'full');
    } finally {
      rmSync(scratch, { recursive: true, force: true });
    }
  }
  mkdirSync(resolve(stateFile, '..'), { recursive: true });
  writeFileSync(stateFile, JSON.stringify({
    targetVersion, channel, previous
  }) + '\n');
  console.log(previous
    ? `Delta base for ${channel}: ${previous.tag} -> v${targetVersion}`
    : `No published stable Delta base for ${channel}: building full-only`);
  return previous;
}

function report(message) {
  console.log(message);
  if (process.env.GITHUB_STEP_SUMMARY)
    appendFileSync(process.env.GITHUB_STEP_SUMMARY, message + '\n');
}

export function finalizeRelease(channel, targetVersion, outputDir, stateFile, deps = {}) {
  if (!CHANNELS.includes(channel)) throw new Error('Invalid channel');
  const state = JSON.parse(readFileSync(stateFile, 'utf8'));
  if (state.channel !== channel || state.targetVersion !== targetVersion)
    throw new Error('Mismatched or outdated Delta build state');
  const previous = state.previous;
  if (previous && previous.full !== packageName(previous.version, channel))
    throw new Error('Invalid previous package recorded in build state');
  const full = packageName(targetVersion, channel);
  const delta = packageName(targetVersion, channel, 'delta');
  const fullPath = join(outputDir, full);
  const deltaPath = join(outputDir, delta);
  const feedPath = join(outputDir, `releases.${channel}.json`);
  const feed = JSON.parse(readFileSync(feedPath, 'utf8'));
  if (!Array.isArray(feed.Assets)) throw new Error('Invalid Velopack release feed');
  const current = feed.Assets.filter(item => item.Version === targetVersion);
  const fullEntries = current.filter(item => item.Type === 'Full');
  const deltaEntries = current.filter(item => item.Type === 'Delta');
  if (fullEntries.length !== 1 || deltaEntries.length > 1 ||
      current.length !== fullEntries.length + deltaEntries.length)
    throw new Error('Expected exactly one full and at most one Delta for the new version');
  assertFeedAsset(fullEntries[0], fullPath, targetVersion, channel, 'full');

  const expectedCurrentFiles = [full, ...(previous ? [delta] : [])];
  const actualCurrentFiles = readdirSync(outputDir).filter(name => name.endsWith('.nupkg') &&
    name !== previous?.full);
  if (actualCurrentFiles.length !== expectedCurrentFiles.length ||
      expectedCurrentFiles.some(name => !actualCurrentFiles.includes(name)))
    throw new Error('Unrecognized or missing new-version release packages');

  if (previous && deltaEntries.length !== 1)
    throw new Error('Velopack did not generate a Delta despite a compatible baseline');
  if (!previous && (deltaEntries.length || existsSync(deltaPath)))
    throw new Error('Unexpected Delta with no previous stable release');

  let publishDelta = false;
  let deltaSize = 0;
  if (previous) {
    assertFeedAsset(deltaEntries[0], deltaPath, targetVersion, channel, 'delta');
    const basePath = join(outputDir, previous.full);
    if (!existsSync(basePath) || !statSync(basePath).size)
      throw new Error('Previous full package disappeared before Delta verification');
    const temporary = mkdtempSync(join(tmpdir(), 'legendofcat-delta-'));
    try {
      const reconstructed = join(temporary, 'reconstructed.nupkg');
      (deps.patch ?? ((base, delta, output) => execFileSync('dotnet', [
        'vpk', 'delta', 'patch', '--base', base, '--patch', delta, '--output', output
      ], { stdio: 'inherit' })))(basePath, deltaPath, reconstructed);
      if (!existsSync(reconstructed) ||
          checksum(reconstructed, 'sha256') !== checksum(fullPath, 'sha256'))
        throw new Error('Delta reconstruction differs from the new full package (SHA-256)');
    } finally {
      rmSync(temporary, { recursive: true, force: true });
    }
    deltaSize = statSync(deltaPath).size;
    publishDelta = deltaSize <= statSync(fullPath).size * MAX_DELTA_RATIO;
  }

  // GitHubSource reads one release at a time. The public feed must not reference
  // predecessor assets stored in a different GitHub Release.
  feed.Assets = [fullEntries[0], ...(publishDelta ? [deltaEntries[0]] : [])];
  writeFileSync(feedPath, JSON.stringify(feed, null, 2) + '\n');
  if (previous) {
    unlinkSync(join(outputDir, previous.full));
    if (!publishDelta) unlinkSync(deltaPath);
  }
  const fullSize = statSync(fullPath).size;
  const saved = publishDelta ? fullSize - deltaSize : 0;
  report(`- **${channel}** v${previous?.version ?? 'none'} → v${targetVersion}: ` +
    `full ${fullSize} B; Delta ${previous ? deltaSize + ' B' : 'n/a'}; ` +
    `${publishDelta ? `saving ${saved} B (${(100 * saved / fullSize).toFixed(1)}%)` :
      previous ? 'Delta verified but omitted (below 10% bandwidth saving)' : 'full-only: no stable predecessor'}`);
  return { fullSize, deltaSize, publishDelta };
}

if (process.argv[1] && resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  const [mode, channel, targetVersion, outputDir] = process.argv.slice(2);
  const stateFile = join('artifacts', 'delta-state', channel + '.json');
  if (mode === 'prepare') prepareRelease(channel, targetVersion, outputDir, stateFile);
  else if (mode === 'finalize') finalizeRelease(channel, targetVersion, outputDir, stateFile);
  else throw new Error('Usage: node tools/delta-release.mjs prepare|finalize win|linux version outputDir');
}
