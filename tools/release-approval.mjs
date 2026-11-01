import { execFileSync } from 'node:child_process';

// Fail closed if main moved while the snapshot was being built.
export function assertSnapshot(env, cwd) {
  if (!env.GITHUB_SHA) throw new Error('Missing workflow snapshot SHA.');
  const git = (...args) => execFileSync('git', args, { cwd, encoding: 'utf8' }).trim();
  const head = git('rev-parse', 'HEAD');
  const remote = git('ls-remote', '--heads', 'origin', 'refs/heads/main').split(/\s+/)[0];
  if (head !== env.GITHUB_SHA || remote !== head)
    throw new Error('Release snapshot no longer matches main; start a new authorized run.');
}

export function verifyConditions(_config, { env, cwd }) {
  assertSnapshot(env, cwd);
}

export function assertReleaseApproval(nextRelease, _allowMinor, allowMajor) {
  if (!nextRelease || !['patch', 'minor', 'major'].includes(nextRelease.type))
    throw new Error('Unsupported or missing release type.');
  if (nextRelease.type === 'major' && allowMajor !== true && allowMajor !== 'true')
    throw new Error('Major release requires explicit allow_major=true.');
}

export function verifyRelease(_config, { nextRelease, env, cwd }) {
  assertSnapshot(env, cwd);
  if (nextRelease.gitHead !== env.GITHUB_SHA)
    throw new Error('Calculated release commit does not match the build snapshot.');
  assertReleaseApproval(nextRelease, true, env.ALLOW_MAJOR);
  if (env.EXPECTED && env.EXPECTED !== nextRelease.version)
    throw new Error('Calculated release version differs from the validated build.');
}
