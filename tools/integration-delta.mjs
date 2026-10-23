// Nonpublishing integration exercise with the real, pinned Velopack CLI and
// immutable official v0.1.0/v0.2.0 full release packages. No Unity rebuild or
// GitHub Release publication is required.
import { createHash } from 'node:crypto';
import { execFileSync } from 'node:child_process';
import { mkdtempSync, readFileSync, rmSync, statSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { resolve } from 'node:path';
import { assertFeedAsset, packageName } from './delta-release.mjs';

const repo = 'LeonHorner/LegendOfCat';
const previous = '0.1.0';
const target = '0.2.0';

function run(bin, args) {
  execFileSync(bin, args, { stdio: 'inherit' });
}

function sha256(path) {
  return createHash('sha256').update(readFileSync(path)).digest('hex');
}

export function testRealDelta(channel) {
  if (channel !== 'win' && channel !== 'linux')
    throw new Error('Expected a Windows or Linux channel');
  const root = mkdtempSync(join(tmpdir(), 'legendofcat-vpk-integration-'));
  try {
    const base = packageName(previous, channel);
    const full = packageName(target, channel);
    const feedName = `releases.${channel}.json`;
    const baseDir = join(root, 'base');
    const newDir = join(root, 'new');
    // Download verified published packages from their own stable tags.
    for (const [tag, filename, directory] of [
      ['v' + previous, base, baseDir],
      ['v' + target, full, newDir]
    ]) {
      run('gh', ['release', 'download', tag, '--repo', repo,
        '--pattern', filename, '--pattern', feedName, '--dir', directory]);
      const feed = JSON.parse(readFileSync(join(directory, feedName), 'utf8'));
      const matching = feed.Assets?.filter(entry => entry.FileName === filename);
      if (matching?.length !== 1) throw new Error('Invalid previous/new real release feed');
      assertFeedAsset(matching[0], join(directory, filename),
        tag.slice(1), channel, 'full');
    }

    const patch = join(root, packageName(target, channel, 'delta'));
    const reconstructed = join(root, 'reconstructed.nupkg');
    run('dotnet', ['vpk', 'delta', 'generate', '--mode', 'BestSpeed',
      '--base', join(baseDir, base), '--new', join(newDir, full), '--output', patch]);
    if (!statSync(patch).size) throw new Error('Velopack generated empty Delta');
    run('dotnet', ['vpk', 'delta', 'patch', '--base', join(baseDir, base),
      '--patch', patch, '--output', reconstructed]);
    if (sha256(reconstructed) !== sha256(join(newDir, full)))
      throw new Error('Real Velopack Delta patch did not reconstruct the published full package');
    console.log(`Real ${channel} Velopack Delta PASS: ${previous} -> ${target}; ` +
      `Delta ${statSync(patch).size} B, full ${statSync(join(newDir, full)).size} B`);
  } finally {
    rmSync(root, { recursive: true, force: true });
  }
}

if (process.argv[1] && resolve(process.argv[1]) === fileURLToPath(import.meta.url))
  testRealDelta(process.argv[2]);
