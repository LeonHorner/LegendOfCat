import { planRelease, assertSnapshot } from './release-engine.mjs';
import { assertReleaseApproval } from './release-approval.mjs';
import { inspectTag, publishDraft } from './release-recovery.mjs';
import { readdirSync, writeFileSync, mkdtempSync, rmSync } from 'node:fs';
import { join, basename } from 'node:path';
import { tmpdir } from 'node:os';
import { DOWNLOADS } from './release-assets.mjs';
import { downloadNotes } from './release-notes.mjs';

if (process.env.GITHUB_REF !== 'refs/heads/main' || !process.env.GITHUB_SHA)
  throw new Error('Release must be dispatched on main with a pinned github.sha.');
const sha = assertSnapshot();
const result = await planRelease({ inspectTagState: inspectTag });
if (!result) throw new Error('No releasable changes.');
const release = result.nextRelease;
assertReleaseApproval(release, process.env.ALLOW_MINOR, process.env.ALLOW_MAJOR);
if (process.env.EXPECTED !== release.version)
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
  await publishDraft(release, assets, notes);
} finally {
  rmSync(temporary, { recursive: true, force: true });
}
