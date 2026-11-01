import * as github from '@semantic-release/github';
import { assertSnapshot } from './release-approval.mjs';

const repositoryUrl = 'https://github.com/LeonHorner/LegendOfCat.git';

function githubContext(context) {
  return { ...context, options: { ...context.options, repositoryUrl } };
}

export async function verifyConditions(config, context) {
  if (context.env.GITHUB_REPOSITORY !== 'LeonHorner/LegendOfCat')
    throw new Error('Unexpected GitHub repository for stable release.');
  await github.verifyConditions(config, githubContext(context));
}

// GitHub creates refs atomically and rejects an existing name even if it points
// to the same SHA. Never push --force, reuse a tag, or retry an interrupted run.
export async function createSnapshotTag({ tag, sha, token, request = fetch }) {
  if (!/^v\d+\.\d+\.\d+$/.test(tag) || !/^[a-f0-9]{40}$/.test(sha) || !token)
    throw new Error('Invalid release tag, snapshot SHA, or GitHub token.');
  const base = 'https://api.github.com/repos/LeonHorner/LegendOfCat';
  const headers = { Authorization: `Bearer ${token}`, Accept: 'application/vnd.github+json', 'X-GitHub-Api-Version': '2022-11-28' };
  const existing = await request(`${base}/releases/tags/${encodeURIComponent(tag)}`, { headers });
  if (existing.status !== 404) {
    if (existing.ok) throw new Error('A release already exists for ' + tag);
    throw new Error('Unable to check existing GitHub release: HTTP ' + existing.status);
  }
  const created = await request(`${base}/git/refs`, {
    method: 'POST', headers: { ...headers, 'Content-Type': 'application/json' },
    body: JSON.stringify({ ref: 'refs/tags/' + tag, sha })
  });
  if (!created.ok) throw new Error('Unable to create release tag: HTTP ' + created.status);
  const verify = await request(`${base}/git/ref/tags/${encodeURIComponent(tag)}`, { headers });
  if (!verify.ok) throw new Error('Unable to verify created tag; inspect before recovery.');
  const ref = await verify.json();
  if (ref.ref !== 'refs/tags/' + tag || ref.object?.sha !== sha || ref.object?.type !== 'commit')
    throw new Error('Created release tag does not match the workflow snapshot; inspect before recovery.');
}

export async function publish(config, context, { request = fetch, publishRelease = github.publish } = {}) {
  assertSnapshot(context.env, context.cwd);
  if (context.nextRelease.gitHead !== context.env.GITHUB_SHA || context.nextRelease.version !== context.env.EXPECTED)
    throw new Error('Release tag target or version does not match the validated snapshot.');
  await createSnapshotTag({ tag: context.nextRelease.gitTag, sha: context.env.GITHUB_SHA, token: context.env.GITHUB_TOKEN, request });
  // If the tag disappears before GitHub creates the release, target_commitish
  // must still name A, never the moving main branch.
  const target = githubContext(context);
  target.branch = { ...context.branch, name: context.env.GITHUB_SHA };
  return publishRelease(config, target);
}
