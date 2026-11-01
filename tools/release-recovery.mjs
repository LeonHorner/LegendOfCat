import { execFileSync } from 'node:child_process';
import { createHash, createReadStream } from 'node:fs';
import { stat } from 'node:fs/promises';
import { basename } from 'node:path';

const repo = 'LeonHorner/LegendOfCat';
const endpoint = 'repos/' + repo + '/';

export function gh(...args) {
  return execFileSync('gh', args, { encoding: 'utf8' }).trim();
}

function getJson(path) {
  try {
    return JSON.parse(gh('api', endpoint + path));
  } catch (error) {
    // Authentication/permission/network/invalid-JSON errors are NOT a missing release.
    if (error.status === 1 && /HTTP 404:/.test(String(error.stderr))) return null;
    throw error;
  }
}

// GitHub Release and Git ref are distinct records: a tag alone is NOT a release.
// Reject annotated tags, prereleases, missing tag refs for drafts, and conflicts.
export function inspectTag(tag) {
  const ref = getJson('git/ref/tags/' + tag);
  const release = getJson('releases/tags/' + tag);
  if (ref && (ref.ref !== 'refs/tags/' + tag || ref.object?.type !== 'commit' ||
      !/^[a-f0-9]{40}$/.test(ref.object.sha)))
    throw new Error('Unexpected tag reference for ' + tag);
  if (release && (!ref || release.tag_name !== tag || release.prerelease ||
      !Array.isArray(release.assets)))
    throw new Error('Contradictory GitHub Release/tag state for ' + tag);
  return { sha: ref?.object.sha ?? null, release };
}

export function assertMatchingTag(state, sha, tag) {
  if (state.sha && state.sha !== sha)
    throw new Error('Existing tag ' + tag + ' points to ' + state.sha +
      ' instead of frozen github.sha ' + sha + '; never move or delete tags.');
  if (state.release && !state.sha)
    throw new Error('GitHub Release exists without its expected tag: ' + tag);
}

function assertDraft(state, release, body) {
  if (!state.sha || !state.release || !state.release.draft ||
      state.release.tag_name !== release.gitTag ||
      state.release.prerelease || state.release.name !== release.gitTag ||
      state.release.target_commitish !== release.gitHead ||
      state.release.body?.trimEnd() !== body.trimEnd())
    throw new Error('Conflicting existing draft metadata for ' + release.gitTag +
      '; manual inspection required, no overwrite.');
}

async function digest(path) {
  const hash = createHash('sha256');
  for await (const chunk of createReadStream(path)) hash.update(chunk);
  return 'sha256:' + hash.digest('hex');
}

export async function assertDraftAssets(existing, files) {
  const local = new Map(files.map(path => [basename(path), path]));
  const seen = new Set();
  for (const asset of existing) {
    if (!local.has(asset.name) || seen.has(asset.name))
      throw new Error('Unexpected or duplicate existing draft asset: ' + asset.name);
    seen.add(asset.name);
    const path = local.get(asset.name);
    const size = (await stat(path)).size;
    const checksum = await digest(path);
    if (asset.size !== size || asset.digest !== checksum)
      throw new Error('Existing draft asset mismatch: ' + asset.name +
        '. No clobber or overwrite is permitted.');
  }
  return files.filter(path => !seen.has(basename(path)));
}

export async function publishDraft(release, assets, body) {
  const sha = release.gitHead;
  const tag = release.gitTag;
  let state = inspectTag(tag);
  assertMatchingTag(state, sha, tag);
  if (state.release && !state.release.draft)
    throw new Error('Release ' + tag + ' is already published; refusing to overwrite.');

  if (!state.sha) {
    // Create-only: GitHub rejects existing refs instead of moving them.
    gh('api', '--method', 'POST', endpoint + 'git/refs',
      '-f', 'ref=refs/tags/' + tag, '-f', 'sha=' + sha);
    state = inspectTag(tag);
    assertMatchingTag(state, sha, tag);
    if (!state.sha || state.release)
      throw new Error('Unexpected state after tag creation for ' + tag);
  }
  if (!state.release) {
    // No upload in create: a draft survives a failed/partial subsequent upload.
    gh('release', 'create', tag, '--repo', repo, '--verify-tag',
      '--target', sha, '--title', tag, '--notes-file', body, '--draft');
    state = inspectTag(tag);
    assertMatchingTag(state, sha, tag);
  }
  const expectedBody = (await import('node:fs')).readFileSync(body, 'utf8');
  assertDraft(state, release, expectedBody);
  const missing = await assertDraftAssets(state.release.assets, assets);
  if (missing.length) gh('release', 'upload', tag, ...missing, '--repo', repo);

  state = inspectTag(tag);
  assertMatchingTag(state, sha, tag);
  assertDraft(state, release, expectedBody);
  if ((await assertDraftAssets(state.release.assets, assets)).length)
    throw new Error('Draft has missing assets; cannot publish ' + tag);

  // Failure here leaves the draft recoverable; a response failure after
  // publication leaves a published release that a retry must not overwrite.
  gh('release', 'edit', tag, '--repo', repo, '--draft=false', '--latest');
  state = inspectTag(tag);
  assertMatchingTag(state, sha, tag);
  if (!state.release || state.release.draft || state.release.prerelease)
    throw new Error('Release did not reach published state: ' + tag);
  if ((await assertDraftAssets(state.release.assets, assets)).length)
    throw new Error('Published release is missing verified assets: ' + tag);
}
