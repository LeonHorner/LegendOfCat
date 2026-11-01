import { analyzeCommits } from '@semantic-release/commit-analyzer';
import { generateNotes } from '@semantic-release/release-notes-generator';
import { execFileSync } from 'node:child_process';
import releaseConfig from '../release.config.js';

const repositoryUrl = 'https://github.com/LeonHorner/LegendOfCat';

export function git(...args) {
  return execFileSync('git', args, { encoding: 'utf8' }).trim();
}

function runGit(cwd, ...args) {
  return execFileSync('git', args, { cwd, encoding: 'utf8' }).trim();
}

export function assertSnapshot(cwd = process.cwd(), expected = process.env.GITHUB_SHA) {
  const actual = runGit(cwd, 'rev-parse', 'HEAD');
  if (expected && actual !== expected)
    throw new Error('Release checkout does not match workflow github.sha: expected ' +
      expected + ', actual ' + actual);
  return actual;
}

// Only stable version tags reachable from the frozen snapshot qualify.
// Never ask the moving remote main branch for version or commit history.
export function lastStableRelease(cwd, sha) {
  const tags = runGit(cwd, 'tag', '--list', 'v[0-9]*', '--merged', sha)
    .split('\n').filter(tag => /^v(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)$/.test(tag));
  function parts(tag) {
    return tag.slice(1).split('.').map(Number);
  }
  tags.sort((a, b) => {
    const x = parts(a), y = parts(b);
    for (let i = 0; i < 3; i++) if (x[i] !== y[i]) return y[i] - x[i];
    return 0;
  });
  return tags[0] || null;
}

function nextVersion(lastVersion, type) {
  if (!lastVersion) return '0.1.0';
  const [major, minor, patch] = lastVersion.split('.').map(Number);
  if (type === 'major') return (major + 1) + '.0.0';
  if (type === 'minor') return major + '.' + (minor + 1) + '.0';
  if (type === 'patch') return major + '.' + minor + '.' + (patch + 1);
  throw new Error('Unsupported release type: ' + type);
}

function optionsFor(plugin) {
  const entry = releaseConfig.plugins.find(p => Array.isArray(p) && p[0] === plugin);
  if (!entry) throw new Error('Missing release plugin ' + plugin);
  return entry[1];
}

// Uses the actual semantic-release commit analyzer and notes generator on the
// frozen Git graph. The semantic-release core publish path is intentionally not
// invoked: its remote main push check can abort a valid older workflow snapshot.
export async function planRelease({ cwd = process.cwd(), snapshotSha = process.env.GITHUB_SHA,
  inspectTagState } = {}) {
  const sha = assertSnapshot(cwd, snapshotSha);
  let tag = lastStableRelease(cwd, sha);
  let incompleteTag = null;
  if (tag && inspectTagState) {
    const candidate = inspectTagState(tag);
    const localSha = runGit(cwd, 'rev-list', '-1', tag);
    if (candidate.sha !== localSha)
      throw new Error('Remote tag mismatch for ' + tag + '; aborting release.');
    if (!candidate.release || candidate.release.draft) {
      if (candidate.sha !== sha)
        throw new Error('Unfinished release ' + tag +
          ' is not on frozen github.sha; rerun the original workflow snapshot.');
      incompleteTag = tag;
      const previous = runGit(cwd, 'tag', '--list', 'v[0-9]*', '--merged', sha)
        .split('\n').filter(other => other !== incompleteTag &&
          /^v(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)$/.test(other));
      previous.sort((a, b) => {
        const x = a.slice(1).split('.').map(Number);
        const y = b.slice(1).split('.').map(Number);
        for (let i = 0; i < 3; i++) if (x[i] !== y[i]) return y[i] - x[i];
        return 0;
      });
      tag = previous[0] || null;
    }
  }
  if (tag && inspectTagState) {
    const baseline = inspectTagState(tag);
    if (!baseline.release || baseline.release.draft ||
        baseline.sha !== runGit(cwd, 'rev-list', '-1', tag))
      throw new Error('Previous release baseline is not a published matching tag: ' + tag);
  }
  const range = tag ? tag + '..' + sha : sha;
  const raw = runGit(cwd, 'log', '--format=%H%x1f%B%x1e', range);
  const commits = raw.split('\x1e').map(part => part.trim()).filter(Boolean)
    .map(part => {
      const separator = part.indexOf('\x1f');
      return { hash: part.slice(0, separator), message: part.slice(separator + 1) };
    });
  const convention = /^(feat|fix|perf|docs|style|refactor|test|build|ci|chore|revert)(\([^)]+\))?!?: .+/;
  const invalid = commits.filter(c => !convention.test(c.message.split('\n', 1)[0]));
  if (invalid.length)
    throw new Error('Non-conventional commits since the last release:\n' +
      invalid.map(c => c.message.split('\n', 1)[0]).join('\n'));

  const lastRelease = tag ? {
    version: tag.slice(1), gitTag: tag,
    gitHead: runGit(cwd, 'rev-list', '-1', tag)
  } : {};
  const context = {
    cwd, env: process.env,
    logger: { log() {}, error: console.error },
    options: { repositoryUrl }, branch: { name: 'main' },
    commits, lastRelease
  };
  const type = await analyzeCommits(optionsFor('@semantic-release/commit-analyzer'), context);
  if (!type) return false;
  const version = nextVersion(lastRelease.version, type);
  if (inspectTagState) {
    const targetState = inspectTagState('v' + version);
    if (targetState.sha && targetState.sha !== sha)
      throw new Error('Planned release tag v' + version +
        ' already targets another commit; refusing to replace it.');
    if (targetState.release && (!targetState.sha || !targetState.release.draft))
      throw new Error('Planned release v' + version +
        ' already has a published or inconsistent GitHub Release.');
  }
  if (incompleteTag && incompleteTag !== 'v' + version)
    throw new Error('Incomplete release tag ' + incompleteTag +
      ' disagrees with recomputed version v' + version + '; manual review required.');
  const release = {
    type, version, gitTag: 'v' + version, name: 'v' + version, gitHead: sha,
    channel: null
  };
  context.nextRelease = release;
  release.notes = await generateNotes(optionsFor('@semantic-release/release-notes-generator'), context);
  return { nextRelease: release, lastRelease, commits, incompleteTag };
}
