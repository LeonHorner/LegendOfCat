import { planRelease, assertSnapshot } from './release-engine.mjs';
import { assertReleaseApproval } from './release-approval.mjs';
import { execFileSync } from 'node:child_process';
import { readdirSync, writeFileSync, mkdtempSync, rmSync } from 'node:fs';
import { join, basename } from 'node:path';
import { tmpdir } from 'node:os';
import { DOWNLOADS } from './release-assets.mjs';
import { downloadNotes } from './release-notes.mjs';

if (process.env.GITHUB_REF !== 'refs/heads/main' || !process.env.GITHUB_SHA)
  throw new Error('Release must be dispatched on main with a pinned github.sha.');
const sha = assertSnapshot();
const result = await planRelease();
if (!result) throw new Error('No releasable changes.');
const release = result.nextRelease;
assertReleaseApproval(release, process.env.ALLOW_MINOR, process.env.ALLOW_MAJOR);
if (process.env.EXPECTED !== release.version || process.env.ACTUAL !== release.version)
  throw new Error('Release version changed since planning.');
if (release.gitHead !== sha)
  throw new Error('Release target differs from workflow github.sha.');

const collect = directory => readdirSync(directory, { withFileTypes: true })
  .flatMap(entry => entry.isDirectory() ? collect(join(directory, entry.name)) : [join(directory, entry.name)]);
const assets = collect('artifacts/published');
for (const filename of Object.values(DOWNLOADS))
  if (!assets.some(file => basename(file) === filename))
    throw new Error('Missing player download: ' + filename);

const temporary = mkdtempSync(join(tmpdir(), 'legendofcat-release-'));
try {
  const notes = join(temporary, 'notes.md');
  writeFileSync(notes, downloadNotes(release.gitTag) + '\n\n' + release.notes);
  // Draft releases may not create Git tag refs until publication. Explicitly
  // create a lightweight tag at the frozen SHA first; never fall back to main.
  // POST is create-only: an existing tag fails rather than being moved/reused.
  execFileSync('gh', ['api', '--method', 'POST',
    'repos/LeonHorner/LegendOfCat/git/refs',
    '-f', 'ref=refs/tags/' + release.gitTag,
    '-f', 'sha=' + sha], { stdio: 'inherit' });
  const tagRef = 'repos/LeonHorner/LegendOfCat/git/ref/tags/' + release.gitTag;
  const readTagSha = () => execFileSync('gh', ['api', tagRef, '--jq', '.object.sha'],
    { encoding: 'utf8' }).trim();
  if (readTagSha() !== sha)
    throw new Error('Release tag does not match workflow github.sha.');
  execFileSync('gh', ['release', 'create', release.gitTag, ...assets,
    '--repo', 'LeonHorner/LegendOfCat', '--verify-tag', '--target', sha,
    '--title', release.gitTag, '--notes-file', notes, '--draft'], { stdio: 'inherit' });
  if (readTagSha() !== sha)
    throw new Error('Draft release tag differs from workflow github.sha.');
  execFileSync('gh', ['release', 'edit', release.gitTag, '--repo',
    'LeonHorner/LegendOfCat', '--draft=false', '--latest'], { stdio: 'inherit' });
  if (readTagSha() !== sha)
    throw new Error('Published release tag differs from workflow github.sha.');
} finally {
  rmSync(temporary, { recursive: true, force: true });
}
