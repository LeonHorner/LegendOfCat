import semanticRelease from 'semantic-release';
import { appendFileSync } from 'node:fs';
import { execFileSync } from 'node:child_process';

let tag;
try {
  tag = execFileSync('git', ['describe', '--tags', '--match', 'v[0-9]*', '--abbrev=0'], {
    encoding: 'utf8', stdio: ['ignore', 'pipe', 'ignore']
  }).trim();
} catch { /* First release: inspect the entire history. */ }
const range = tag ? tag + '..HEAD' : 'HEAD';
const subjects = execFileSync('git', ['log', '--format=%s', range], { encoding: 'utf8' })
  .trim().split('\n').filter(Boolean);
const convention = /^(feat|fix|perf|docs|style|refactor|test|build|ci|chore|revert)(\([^)]+\))?!?: .+/;
const invalid = subjects.filter(subject => !convention.test(subject));
if (invalid.length) {
  throw new Error('Non-conventional commits since the last release:\n' + invalid.join('\n'));
}
const result = await semanticRelease({ dryRun: true, ci: true });
const version = result ? result.nextRelease.version : '';
if (process.env.GITHUB_OUTPUT)
  appendFileSync(process.env.GITHUB_OUTPUT, 'version=' + version + '\n');
console.log(version ? 'Planned release: ' + version : 'No releasable changes.');
