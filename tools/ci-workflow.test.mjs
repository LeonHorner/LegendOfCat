import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import test from 'node:test';

const prYaml = readFileSync(new URL('../.github/workflows/pull-request.yml', import.meta.url), 'utf8').replace(/\r\n/g, '\n');
const releaseYaml = readFileSync(new URL('../.github/workflows/release.yml', import.meta.url), 'utf8').replace(/\r\n/g, '\n');
const AsyncFunction = Object.getPrototypeOf(async function () {}).constructor;

function githubScript(job) {
  const lines = prYaml.split('\n');
  const begin = lines.indexOf('  ' + job + ':');
  assert.notEqual(begin, -1, 'Missing job: ' + job);
  let end = begin + 1;
  while (end < lines.length && !/^  [a-z][a-z-]*:$/.test(lines[end])) end++;
  let at = begin + 1;
  while (at < end && lines[at] !== '          script: |') at++;
  assert.ok(at < end, 'Missing github-script in ' + job);

  const script = [];
  for (let i = at + 1; i < end; i++) {
    if (lines[i].startsWith('            ')) script.push(lines[i].slice(12));
    else if (!lines[i].trim()) script.push('');
    else break;
  }
  return new AsyncFunction('github', 'context', 'core', 'process', script.join('\n'));
}

const runPreflight = githubScript('preflight');
const runGate = githubScript('gate');

function mock(options = {}) {
  const pr = structuredClone({
    number: 32,
    state: 'open',
    draft: false,
    title: 'ci: automate PR checks',
    base: { ref: 'main', sha: 'main-commit' },
    head: { sha: 'pr-commit', repo: { full_name: 'LeonHorner/LegendOfCat' } }
  });
  if (options.pr) {
    for (const [key, value] of Object.entries(options.pr)) pr[key] = value;
  }
  const statuses = [];
  const outputs = {};
  const failures = [];
  const results = { PREFLIGHT: 'success', TEST: 'success', BUILD: 'success', PACKAGE: 'success', ...options.results };
  return {
    pr,
    statuses,
    outputs,
    failures,
    github: {
      rest: {
        pulls: { get: async () => ({ data: pr }) },
        repos: {
          createCommitStatus: async status => { statuses.push(status); },
          compareCommitsWithBasehead: async () => ({
            data: { behind_by: options.behind ?? 0 }
          })
        }
      }
    },
    context: {
      repo: { owner: 'LeonHorner', repo: 'LegendOfCat' },
      eventName: options.eventName ?? 'workflow_dispatch',
      payload: { pull_request: { number: pr.number } },
      serverUrl: 'https://github.com',
      runId: 321
    },
    core: {
      setOutput: (key, value) => { outputs[key] = value; },
      setFailed: message => { failures.push(message); }
    },
    process: {
      env: {
        PR_NUMBER: options.input ?? '32',
        CHECKED_SHA: 'pr-commit',
        CHECKED_BASE: 'main-commit',
        CHECKED_TITLE: 'ci: automate PR checks',
        ...results
      }
    }
  };
}

test('preflight uses the exact PR head and marks it pending', async () => {
  const r = mock();
  await runPreflight(r.github, r.context, r.core, r.process);
  assert.deepEqual(r.outputs, {
    number: '32',
    sha: 'pr-commit',
    base: 'main-commit',
    title: 'ci: automate PR checks'
  });
  assert.equal(r.statuses.at(-1).state, 'pending');
  assert.equal(r.statuses.at(-1).sha, 'pr-commit');
  assert.equal(r.statuses.at(-1).context, 'merge-ready');
});

test('automatic PR event reads the event PR number', async () => {
  const r = mock({ eventName: 'pull_request', input: '' });
  await runPreflight(r.github, r.context, r.core, r.process);
  assert.equal(r.outputs.number, '32');
});

test('draft, out-of-date, and external PRs are not approved in preflight', async () => {
  for (const options of [
    { pr: { draft: true } },
    { behind: 1 },
    { pr: { head: { sha: 'pr-commit', repo: { full_name: 'external/LegendOfCat' } } } }
  ]) {
    const r = mock(options);
    await assert.rejects(runPreflight(r.github, r.context, r.core, r.process));
    assert.ok(!r.statuses.some(s => s.state === 'success'));
  }
});

test('the gate accepts only a current PR with all jobs successful', async () => {
  const r = mock();
  await runGate(r.github, r.context, r.core, r.process);
  assert.equal(r.statuses.at(-1).state, 'success');
  assert.deepEqual(r.failures, []);
});

test('the gate rejects failures, cancellations, skips and stale PR revisions', async () => {
  const scenarios = [
    { results: { TEST: 'failure' } },
    { results: { TEST: 'cancelled' } },
    { results: { TEST: 'skipped' } },
    { results: { BUILD: 'failure' } },
    { results: { BUILD: 'cancelled' } },
    { results: { BUILD: 'skipped' } },
    { results: { PACKAGE: 'failure' } },
    { results: { PACKAGE: 'cancelled' } },
    { results: { PACKAGE: 'skipped' } },
    { results: { PREFLIGHT: 'failure' } },
    { pr: { head: { sha: 'different-commit', repo: { full_name: 'LeonHorner/LegendOfCat' } } } },
    { pr: { base: { ref: 'main', sha: 'new-main-commit' } } },
    { pr: { title: 'ci: renamed' } },
    { pr: { draft: true } },
    { pr: { state: 'closed' } },
    { pr: { base: { ref: 'development', sha: 'main-commit' } } },
    { behind: 1 }
  ];
  for (const scenario of scenarios) {
    const r = mock(scenario);
    await runGate(r.github, r.context, r.core, r.process);
    assert.equal(r.statuses.at(-1).state, 'failure', JSON.stringify(scenario));
    assert.equal(r.failures.length, 1, JSON.stringify(scenario));
  }
});

test('automatic CI is limited to ready same-repo PRs; manual fallback remains', () => {
  assert.match(prYaml, /types: \[opened, reopened, ready_for_review, synchronize, edited, converted_to_draft\]/);
  assert.match(prYaml, /if: github\.event_name == 'workflow_dispatch' \|\| .*head\.repo\.full_name == github\.repository/);
  assert.match(prYaml, /!github\.event\.pull_request\.draft/);
  assert.match(prYaml, /group: pr-validation-\$\{\{ github\.event\.pull_request\.number \|\| inputs\.pr_number \}\}/);
  assert.match(prYaml, /cancel-in-progress: true/);
  assert.match(prYaml, /needs: \[preflight, test, build, package\]/);
  assert.match(prYaml, /build:\n    name: Build \(\$\{\{ matrix\.platform \}\}\)\n    needs: preflight/);
  assert.match(prYaml, /githubToken: ''/);
});

test('release is manual and publish depends on all required jobs', () => {
  const header = releaseYaml.split('\njobs:')[0];
  assert.match(header, /on:\n  workflow_dispatch:/);
  assert.doesNotMatch(header, /\bpull_request:/);
  assert.match(releaseYaml, /build:\n    needs: plan\n    if: needs\.plan\.outputs\.version != ''/);
  assert.match(releaseYaml, /package:\n    needs: \[plan, build\]/);
  assert.match(releaseYaml, /needs: \[plan, test, build, package\]/);
  assert.match(releaseYaml, /needs\.test\.result == 'success'/);
  assert.match(releaseYaml, /needs\.package\.result == 'success'/);
  assert.match(releaseYaml, /test "\$EXPECTED" = "\$ACTUAL"/);
  assert.match(releaseYaml, /githubToken: ''/);
});
