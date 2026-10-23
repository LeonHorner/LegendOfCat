import semanticRelease from 'semantic-release';
import { firstReleasePlan, git } from './release-engine.mjs';
import { execFileSync } from 'node:child_process';
import { readdirSync, writeFileSync, mkdtempSync, rmSync } from 'node:fs';
import { join } from 'node:path';
import { tmpdir } from 'node:os';
import { basename } from 'node:path';
import { DOWNLOADS } from './release-assets.mjs';
import { downloadNotes } from './release-notes.mjs';

const initial = await firstReleasePlan();
if (initial === null) {
  await semanticRelease({ ci: true });
} else if (initial) {
  const release = initial.nextRelease;
  if (git('branch', '--show-current') !== 'main') throw new Error('Release only from main.');
  if (process.env.EXPECTED && process.env.EXPECTED !== release.version)
    throw new Error('Release version changed after validation.');
  const collect = directory => readdirSync(directory, { withFileTypes: true })
    .flatMap(entry => entry.isDirectory() ? collect(join(directory, entry.name)) : [join(directory, entry.name)]);
  const assets = collect('artifacts/published');
  for (const filename of Object.values(DOWNLOADS)) {
    if (!assets.some(file => basename(file) === filename))
      throw new Error('Missing player download: ' + filename);
  }
  const temporary = mkdtempSync(join(tmpdir(), 'legendofcat-release-'));
  try {
    const notes = join(temporary, 'notes.md');
    writeFileSync(notes, downloadNotes(release.gitTag) + '\n\n' + release.notes);
    // Publish only after all files have uploaded successfully.
    execFileSync('gh', ['release', 'create', release.gitTag, ...assets,
      '--repo', 'LeonHorner/LegendOfCat', '--target', release.gitHead,
      '--title', release.gitTag, '--notes-file', notes, '--draft'], { stdio: 'inherit' });
    execFileSync('gh', ['release', 'edit', release.gitTag, '--repo',
      'LeonHorner/LegendOfCat', '--draft=false', '--latest'], { stdio: 'inherit' });
  } finally {
    rmSync(temporary, { recursive: true, force: true });
  }
} else {
  throw new Error('No releasable changes.');
}
