import { copyFileSync, existsSync, mkdirSync, readFileSync, readdirSync, rmSync, statSync } from 'node:fs';
import { basename, join, resolve } from 'node:path';
import { assertFeedAsset, packageName } from './delta-release.mjs';
import { fileURLToPath } from 'node:url';

export const DOWNLOADS = Object.freeze({
  windows: 'LegendOfCat-Windows-Installer.exe',
  portable: 'LegendOfCat-Windows-Portable.zip',
  linux: 'LegendOfCat-Linux-x86_64.AppImage'
});
const PACKAGE_ID = 'LeonHorner.LegendOfCat';

function requireSingle(files, suffix, platform) {
  const matches = files.filter(name => name.endsWith(suffix));
  if (matches.length !== 1) throw new Error(platform + ': expected exactly one *' + suffix + ', found ' + matches.length);
  return matches[0];
}

export function prepareAssets(sourceDir, destinationDir, version) {
  packageName(version, 'win'); // Reject missing or invalid release versions.
  const source = resolve(sourceDir);
  const destination = resolve(destinationDir);
  if (source === destination || destination.startsWith(source + '/'))
    throw new Error('The published-asset directory must be separate from packaging output.');

  const output = new Map();
  for (const platform of ['win', 'linux']) {
    const directory = join(source, platform);
    const files = readdirSync(directory).filter(name => statSync(join(directory, name)).isFile());
    const feedName = 'releases.' + platform + '.json';
    if (!files.includes(feedName)) throw new Error(platform + ': missing update feed ' + feedName);
    const feed = JSON.parse(readFileSync(join(directory, feedName), 'utf8'));
    if (!Array.isArray(feed.Assets) || feed.Assets.length === 0)
      throw new Error(platform + ': empty or invalid Velopack update feed');

    const packageNames = files.filter(name => name.endsWith('.nupkg'));
    const expectedFull = packageName(version, platform);
    const expectedDelta = packageName(version, platform, 'delta');
    if (!packageNames.includes(expectedFull) ||
        packageNames.some(name => name !== expectedFull && name !== expectedDelta))
      throw new Error(platform + ': unexpected release package version, channel or missing full package');
    const referenced = new Set();
    for (const entry of feed.Assets) {
      const filename = entry.FileName;
      if (entry.PackageId !== PACKAGE_ID || typeof filename !== 'string' ||
          basename(filename) !== filename || !packageNames.includes(filename))
        throw new Error(platform + ': invalid or missing update package referenced by feed: ' + filename);
      const type = filename === expectedFull ? 'full' : 'delta';
      if (referenced.has(filename)) throw new Error(platform + ': duplicate feed package');
      assertFeedAsset(entry, join(directory, filename), version, platform, type);
      referenced.add(filename);
    }
    if (packageNames.some(name => !referenced.has(name)) ||
        referenced.size !== packageNames.length)
      throw new Error(platform + ': a package is missing from the release feed');

    function add(filename, publicName = filename) {
      if (output.has(publicName)) throw new Error('Duplicate published asset: ' + publicName);
      const path = join(directory, filename);
      if (!existsSync(path) || statSync(path).size === 0)
        throw new Error('Missing or empty published asset: ' + path);
      output.set(publicName, path);
    }

    add(feedName);
    for (const name of packageNames) add(name);
    if (platform === 'win') {
      add(requireSingle(files, '-Setup.exe', platform), DOWNLOADS.windows);
      add(requireSingle(files, '-Portable.zip', platform), DOWNLOADS.portable);
    } else {
      add(requireSingle(files, '.AppImage', platform), DOWNLOADS.linux);
    }
    // assets.*.json are vpk upload inputs, and RELEASES files are only for
    // legacy Squirrel migration. Neither is used by our GithubSource updater.
  }

  // Validate everything before modifying the output directory.
  rmSync(destination, { recursive: true, force: true });
  mkdirSync(destination, { recursive: true });
  for (const [name, path] of output) copyFileSync(path, join(destination, name));
  return [...output.keys()];
}

if (process.argv[1] && resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  const files = prepareAssets(process.argv[2] ?? 'artifacts/releases', process.argv[3] ?? 'artifacts/published', process.argv[4]);
  console.log('Verified release assets: ' + files.join(', '));
}
