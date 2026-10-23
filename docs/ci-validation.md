# CI validation, safety and timing

## Workflow responsibilities

- `.github/workflows/pull-request.yml` responds automatically to ready same-repository PRs on opening, reopening, ready-for-review, new commits and metadata edits. Draft pushes skip full validation; converting to Draft invalidates merge readiness. A manual `workflow_dispatch` with `pr_number` remains available.
- A per-PR concurrency group cancels superseded attempts without cancelling other PRs. Preflight resolves the live head/base/title, marks `merge-ready` pending on that **head SHA**, rejects fork branches and stale bases, and runs `node --test tools/ci-workflow.test.mjs` before allocating full Unity jobs.
- Unity EditMode tests and Windows/Linux player + self-contained launcher builds start independently after preflight. Linux pre-merge startup still requires its Xvfb smoke test. The final gate requires `preflight`, `test` and the complete `build` matrix to succeed; it re-reads the PR head, base, title and draft/state and checks that it is still up to date. Failure, cancellation and skipped required jobs never grant `merge-ready`.
- `.github/workflows/release.yml` is explicitly manual on `main`. After a successful version-producing plan, tests and the Windows/Linux player builds can overlap. Packaging starts only after the platform builds; publication depends **explicitly** on successful plan, tests, both platform builds and both packages. The final version recheck and `stable-release` concurrency remain.
- Both GameCI Test Runner invocations pass `githubToken: ''` to suppress the redundant neutral `Test Results` check. The Action still fails on its real test result and XML/log artifacts are uploaded regardless of test outcome.

A GitHub PR check marked `merge-ready` is not permission to merge, and no PR-triggered workflow can publish a release. Main remains protected; merge requires explicit authorization and an unchanged exact head SHA. Workflow cancellation does not replace these checks.

## Nonpublishing gate verification

`node --test tools/ci-workflow.test.mjs` executes the actual embedded preflight and final-gate JavaScript using mock GitHub API responses. Cases include:

| Case | Required behavior |
| --- | --- |
| Exact same-repository ready head, current base, all jobs successful | `merge-ready: success` |
| Unity test failure, skip or cancellation | `merge-ready: failure` |
| Platform matrix failure, skip or cancellation | `merge-ready: failure` |
| Preflight failure | `merge-ready: failure` |
| Head SHA, base SHA or title changed since preflight | `merge-ready: failure` |
| Draft, closed, retargeted or behind-base PR | `merge-ready: failure` |
| Draft push or fork event | No full Unity jobs |
| Release plan/test/build/package incomplete | `publish` cannot run |
| Release PR event | Not a supported workflow trigger |

The tests and structural assertions are *nonpublishing*. They cannot simulate Unity licensing, GitHub's scheduler/concurrency edge cases, runner availability, branch protection or actual installer behavior. Validate the real ready-PR run before merging; the first later **explicitly authorized** release supplies release performance evidence.

## Baseline measurements: October 23, 2026

- [Prepare merge run 40002310183](https://github.com/LeonHorner/LegendOfCat/actions/runs/40002310183) succeeded from **18:19:11–18:25:08 UTC** (about **5m57s** wall-clock). Its Unity test runner occupied approximately 2m19s, while the Windows and Linux builds occupied approximately 2m57s and 3m16s; in the old DAG builds waited for tests. Run scheduling and startup overhead affect these values.
- Test-job cache restored approximately **392 MB** into `Library`; Windows-build cache approximately **42 MB**, Linux-build cache approximately **404 MB**. Both builder logs nevertheless reported `Rebuilding Library because the asset database could not be found!`. A cache restore is not proof of a reusable Unity asset database. The test job's 392 MB restore also did not eliminate initial Editor import work.
- On that run, image downloads from `unityci/editor` took approximately **82 s** (test), **87 s** (Windows builder) and **83 s** (Linux builder). These fresh GitHub-hosted Docker pulls dominate the individual job durations and are not solved by caching `Library` alone.
- [Release run 40003386957](https://github.com/LeonHorner/LegendOfCat/actions/runs/40003386957) succeeded from **18:27:09–18:35:51 UTC** (about **8m42s**), using the old serialized plan → test → build → package → publish DAG.

## Optimization and follow-up evidence

Parallelism removes the unnecessary test-to-build dependency while keeping independent results mandatory. The issue's **3–4 minute** Prepare merge target is an estimate, not a benchmark result or a pass/fail threshold. After the implementation PR's first successful ready-PR run, record its actual checked head SHA, scheduling, start/end time and critical path here and in the PR. If licensing or runner contention makes concurrent Unity jobs unreliable, reduce concurrency while preserving the gate.

We deliberately **do not** enlarge/rekey the existing per-platform `Library` caches, cross-contaminate test/build caches, or add Docker-image caches, self-hosted runners or custom images without reproducible evidence. Diagnose whether GameCI's Unity asset database is being removed or never cached before any targeted cache experiment. GitHub-hosted runners are ephemeral, and GameCI Editor image downloads recur on fresh machines. Cost is not the only constraint: Unity seats, runner capacity, storage quotas, and canceled runs still matter.
