import { planRelease } from './release-engine.mjs';
import { assertReleaseApproval } from './release-approval.mjs';
import { appendFileSync } from 'node:fs';

// github.sha is immutable even if other PRs are merged into main during this run.
const result = await planRelease();
if (result) assertReleaseApproval(result.nextRelease, process.env.ALLOW_MINOR, process.env.ALLOW_MAJOR);
const version = result ? result.nextRelease.version : '';
if (process.env.GITHUB_OUTPUT)
  appendFileSync(process.env.GITHUB_OUTPUT, 'version=' + version + '\n');
console.log(version ? 'Planned release: ' + version + ' (' + result.nextRelease.gitHead + ')'
  : 'No releasable changes.');
