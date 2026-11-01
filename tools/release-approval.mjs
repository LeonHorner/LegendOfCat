import { execFileSync } from 'node:child_process';

// The checked-out commit must be the immutable workflow snapshot.
export function assertSnapshot(env, cwd) {
  if (!env.GITHUB_SHA) throw new Error('Missing workflow snapshot SHA.');
  const git = (...args) => execFileSync('git', args, { cwd, encoding: 'utf8' }).trim();
  const head = git('rev-parse', 'HEAD');
  if (head !== env.GITHUB_SHA)
    throw new Error('Release checkout does not match the workflow snapshot.');
}

export function verifyConditions(_config, { env, cwd }) {
  assertSnapshot(env, cwd);
}

export function assertReleaseApproval(nextRelease, allowMinor, allowMajor) {
  if (!nextRelease || !['patch', 'minor', 'major'].includes(nextRelease.type))
    throw new Error('Unsupported or missing release type.');
  if (nextRelease.type === 'minor' && allowMinor !== true && allowMinor !== 'true' && allowMinor !== undefined)
    throw new Error('Minor release requires allow_minor=true.');
  if (nextRelease.type === 'major' && allowMajor !== true && allowMajor !== 'true')
    throw new Error('Major release requires explicit allow_major=true.');
}

export function verifyRelease(_config, { nextRelease, env, cwd }) {
  assertSnapshot(env, cwd);
  if (nextRelease.gitHead !== env.GITHUB_SHA)
    throw new Error('Calculated release commit does not match the build snapshot.');
  assertReleaseApproval(nextRelease, env.ALLOW_MINOR, env.ALLOW_MAJOR);
  if (env.EXPECTED && env.EXPECTED !== nextRelease.version)
    throw new Error('Calculated release version differs from the validated build.');
}
