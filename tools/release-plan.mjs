import { planRelease } from './release-engine.mjs';
import { assertReleaseApproval } from './release-approval.mjs';
import { inspectReleaseCommitHistory } from './release-commit-policy.mjs';
import { appendFileSync } from 'node:fs';

const subjects = inspectReleaseCommitHistory()
  .map(message => message.split(/\r?\n/, 1)[0])
  .filter(Boolean);
const convention = /^(feat|fix|perf|docs|style|refactor|test|build|ci|chore|revert)(\([^)]+\))?!?: .+/;
const invalid = subjects.filter(subject => !convention.test(subject));
if (invalid.length) {
  throw new Error('Non-conventional commits since the last release:\n' + invalid.join('\n'));
}
const result = await planRelease();
if (result) assertReleaseApproval(result.nextRelease, process.env.ALLOW_MINOR, process.env.ALLOW_MAJOR);
const version = result ? result.nextRelease.version : '';
if (process.env.GITHUB_OUTPUT)
  appendFileSync(process.env.GITHUB_OUTPUT, 'version=' + version + '\n');
console.log(version ? 'Planned release: ' + version : 'No releasable changes.');
