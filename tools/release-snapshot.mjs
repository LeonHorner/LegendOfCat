import { execFileSync } from 'node:child_process';
import { mkdtempSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { assertSnapshot } from './release-approval.mjs';
import semanticRelease from 'semantic-release';

// semantic-release fetches and verifies its configured branch. Give it an
// isolated remote whose only release branch is the immutable workflow commit.
export async function runSnapshotRelease({ cwd = process.cwd(), env = process.env, dryRun = false, options = {} } = {}) {
  assertSnapshot(env, cwd);
  const directory = mkdtempSync(join(tmpdir(), 'loc-release-snapshot-'));
  const remote = join(directory, 'snapshot.git');
  const git = (...args) => execFileSync('git', args, { cwd, env, stdio: 'pipe' });
  try {
    git('clone', '--bare', '--no-hardlinks', '--', cwd, remote);
    execFileSync('git', ['--git-dir', remote, 'update-ref', 'refs/heads/main', env.GITHUB_SHA], { env, stdio: 'pipe' });
    const result = await semanticRelease({ ...options, repositoryUrl: remote, dryRun, ci: true }, { cwd, env });
    assertSnapshot(env, cwd);
    return result;
  } finally {
    rmSync(directory, { recursive: true, force: true });
  }
}
