import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import test from 'node:test';

import yaml from 'js-yaml';

const prWorkflow = yaml.load(readFileSync(new URL('../.github/workflows/pull-request.yml', import.meta.url), 'utf8'));
const releaseWorkflow = yaml.load(readFileSync(new URL('../.github/workflows/release.yml', import.meta.url), 'utf8'));
const AsyncFunction = Object.getPrototypeOf(async function () {}).constructor;

// Execute the actual github-script step from the parsed workflow, not a copied
// implementation or a line-indentation approximation.
function githubScript(job) {
  const steps = prWorkflow.jobs[job]?.steps ?? [];
  const matches = steps.filter(step => step.uses?.startsWith('actions/github-script@'));
  assert.equal(matches.length, 1, `Expected one github-script step in ${job}`);
  assert.equal(typeof matches[0].with?.script, 'string');
  return new AsyncFunction('github', 'context', 'core', 'process', matches[0].with.script);
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
  const results = { PREFLIGHT: 'success', TEST: 'success', BUILD: 'success', PACKAGE: 'success', DELTA_INTEGRATION: 'success', ...options.results };
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

test('manual preflight rejects invalid PR numbers before reading an issue', async () => {
  for (const input of ['', '0', '-1', '01', 'abc', '1.5', '9007199254740992']) {
    const r = mock({ input });
    await assert.rejects(runPreflight(r.github, r.context, r.core, r.process), undefined, input);
    assert.deepEqual(r.statuses, [], input);
  }
});

test('preflight rejects invalid titles, closed PRs and unexpected target branches', async () => {
  for (const pr of [
    { title: 'unstructured change' },
    { title: 'fix: ' },
    { state: 'closed' },
    { base: { ref: 'develop', sha: 'main-commit' } }
  ]) {
    const r = mock({ pr });
    await assert.rejects(runPreflight(r.github, r.context, r.core, r.process));
    assert.ok(!r.statuses.some(status => status.state === 'success'));
  }
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
    { results: { DELTA_INTEGRATION: 'failure' } },
    { results: { DELTA_INTEGRATION: 'cancelled' } },
    { results: { DELTA_INTEGRATION: 'skipped' } },
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


// Config tests inspect parsed workflow topology, not incidental YAML formatting.
// Script behavior is tested above by running the actual embedded scripts.
test('PR workflow triggers and job graph keep validation on ready PRs', () => {
  assert.deepEqual(prWorkflow.on.pull_request.types, [
    'opened', 'reopened', 'ready_for_review', 'synchronize', 'edited', 'converted_to_draft'
  ]);
  assert.equal(prWorkflow.on.workflow_dispatch.inputs.pr_number.required, true);
  assert.equal(prWorkflow.concurrency['cancel-in-progress'], true);
  assert.deepEqual(prWorkflow.jobs.gate.needs,
    ['preflight', 'test', 'build', 'package', 'delta-integration']);
  assert.equal(prWorkflow.jobs.test.needs, 'preflight');
  assert.equal(prWorkflow.jobs.build.needs, 'preflight');
  assert.deepEqual(prWorkflow.jobs.package.needs, ['preflight', 'build']);
  assert.equal(prWorkflow.jobs['delta-integration'].needs, 'preflight');
  assert.equal(prWorkflow.jobs.preflight.steps[0].uses, 'actions/github-script@v7');
  assert.deepEqual(prWorkflow.jobs.preflight.steps[0].env, {
    PR_NUMBER: '${{ inputs.pr_number }}'
  });
  assert.ok(prWorkflow.jobs.preflight.if, 'Preflight must be conditionally guarded');
});

test('release is manual and publishing is gated on required jobs', () => {
  assert.deepEqual(Object.keys(releaseWorkflow.on), ['workflow_dispatch']);
  assert.deepEqual(releaseWorkflow.jobs.package.needs, ['plan', 'build']);
  assert.equal(releaseWorkflow.jobs.build.needs, 'plan');
  assert.deepEqual(releaseWorkflow.jobs.publish.needs, ['plan', 'test', 'build', 'package']);
  const publishRequirements = new Set(
    releaseWorkflow.jobs.publish.if.split('&&').map(term => term.trim()));
  assert.deepEqual(publishRequirements, new Set(
    ['plan', 'test', 'build', 'package'].map(name =>
      `needs.${name}.result == 'success'`)));
});

test('both platforms perform real Velopack Delta reconstruction before merge gate', () => {
  const integration = prWorkflow.jobs['delta-integration'];
  assert.deepEqual(integration.strategy.matrix.include.map(({ os, platform }) => ({ os, platform })), [
    { os: 'windows-2022', platform: 'win' },
    { os: 'ubuntu-22.04', platform: 'linux' }
  ]);
  assert.ok(integration.steps.some(step =>
    step.run?.trim() === 'node tools/integration-delta.mjs ${{ matrix.platform }}'));
  const gate = prWorkflow.jobs.gate.steps.find(step => step.uses?.startsWith('actions/github-script@'));
  assert.deepEqual(Object.fromEntries(['PREFLIGHT', 'TEST', 'BUILD', 'PACKAGE', 'DELTA_INTEGRATION'].map(key =>
    [key, gate.env[key]])), {
    PREFLIGHT: '${{ needs.preflight.result }}',
    TEST: '${{ needs.test.result }}',
    BUILD: '${{ needs.build.result }}',
    PACKAGE: '${{ needs.package.result }}',
    DELTA_INTEGRATION: '${{ needs.delta-integration.result }}'
  });
});

test('release checks and builds both platforms before publishing artifacts', () => {
  const pkg = releaseWorkflow.jobs.package;
  assert.deepEqual(pkg.strategy.matrix.include.map(({ platform }) => platform), ['win', 'linux']);
  const steps = pkg.steps;
  const names = steps.map(step => step.name);
  const fetch = names.indexOf('Fetch and verify previous stable full package');
  const pack = names.indexOf('Package installer and update feed');
  const verify = names.indexOf('Verify Delta reconstruction and release feed');
  const upload = steps.findIndex((step, index) => index > verify && step.uses?.startsWith('actions/upload-artifact@'));
  assert.ok(fetch >= 0 && fetch < pack && pack < verify && verify < upload,
    'Baseline verification, packing and Delta reconstruction must precede artifact upload');
  assert.ok(releaseWorkflow.jobs.publish.steps.some(step =>
    step.name === 'Publish verified commit'));
});
