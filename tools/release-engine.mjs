import semanticRelease from 'semantic-release';
import { analyzeCommits } from '@semantic-release/commit-analyzer';
import { generateNotes } from '@semantic-release/release-notes-generator';
import { execFileSync } from 'node:child_process';

export function git(...args) {
  return execFileSync('git', args, { encoding: 'utf8' }).trim();
}

export async function firstReleasePlan() {
  const tags = git('tag', '--merged', 'HEAD', '--list', 'v[0-9]*');
  if (tags) return null;
  const commits = git('log', '--format=%H%x1f%B%x1e').split('\x1e')
    .map(entry => entry.trim()).filter(Boolean).map(entry => {
      const separator = entry.indexOf('\x1f');
      return { hash: entry.slice(0, separator), message: entry.slice(separator + 1) };
    });
  const context = {
    cwd: process.cwd(), env: process.env,
    logger: { log() {}, error: console.error },
    options: { repositoryUrl: git('remote', 'get-url', 'origin') },
    branch: { name: 'main' }, commits, lastRelease: {},
    nextRelease: { version: '0.1.0', gitTag: 'v0.1.0', gitHead: git('rev-parse', 'HEAD') }
  };
  const type = await analyzeCommits({ preset: 'conventionalcommits' }, context);
  if (!type) return false;
  context.nextRelease.type = type;
  context.nextRelease.notes = await generateNotes({ preset: 'conventionalcommits' }, context);
  return { nextRelease: context.nextRelease };
}

export async function planRelease() {
  const initial = await firstReleasePlan();
  return initial === null ? semanticRelease({ dryRun: true, ci: true }) : initial;
}
