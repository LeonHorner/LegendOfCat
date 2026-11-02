import { runSnapshotRelease } from './release-snapshot.mjs';
import { appendFileSync } from 'node:fs';

const result = await runSnapshotRelease({ dryRun: true });
const version = result ? result.nextRelease.version : '';
if (process.env.GITHUB_OUTPUT) appendFileSync(process.env.GITHUB_OUTPUT, `version=${version}\n`);
console.log(version ? `Planned release: ${version}` : 'No releasable changes.');
