# Agent rules
- Use English for all repository content: code identifiers, comments, documentation, filenames, branch names, commits, PR titles and descriptions, maintainer-authored issues, UI text, logs, and release notes. This applies even when user instructions are in another language. Preserve third-party legal notices and proper names verbatim.
- One coherent feature or fix per branch and PR. Use an English Conventional Commit PR title describing the user-visible result; it becomes the squash commit and release-note entry. Keep implementation details in the PR description.
- Keep the project small; no framework layers without a concrete need.
- Unity version is pinned in ProjectSettings/ProjectVersion.txt. Commit Packages/packages-lock.json after Editor import.
- One coherent feature per branch/PR; several related issues can share it. Never force-push any branch.
- Squash PR titles must be Conventional Commits. Do not require external review approval for this solo workflow.
- Run automated tests only in manual prepare-merge validation once a feature is ready, and in explicitly requested releases. Do not trigger tests for individual commits. Require the merge-ready status before squash merge.
- Compilation and static inspection are allowed during development. Do not claim unrun checks passed.
- Do not trigger or publish a release unless the user explicitly requests it.
- Preserve all Unity .meta files; never hand-edit serialized scenes when Editor tooling is available.
- Keep Unity MCP local, editor-only, and pinned to a reviewed revision before adding it.
- Generated scenes and asset metadata must be committed after the first Editor import.
- Store saves/settings under Application.persistentDataPath, never inside the install folder.
- Follow docs/data-migrations.md for every persistent-data format change. Start each format at schemaVersion 0 and use independent counters for settings, saves and editable levels. Migrate only formats shipped in published stable releases; unreleased formats may evolve in place without compatibility migrations. Preserve originals/backups and unknown future files, and commit regression fixtures/tests when changing a released format incompatibly. Never silently reset a player's save.
- Use only original/generated assets or assets whose license allows public source redistribution.
- Record asset source, generator/model, date, license and human creative decisions in ASSETS.md.
- No unlicensed characters, brands, fonts, music or copied code. Preserve dependency notices.
- No secrets, build output, Library, Temp or Logs in Git.
- The launcher owns updates; Unity must not overwrite its running installation.
- Stable downloads/updates come only from LeonHorner/LegendOfCat GitHub Releases.
- Smoke-test actual Windows/Linux installs and N -> N+1 updates in the release process before claiming update support verified.


## Issue-to-PR workflow
1. Read the requested issue, linked issues, acceptance criteria, and existing PRs/branches before editing.
2. The unit of work is a coherent feature, not an issue. Several related issues may share one feature branch and one PR. Reuse an existing feature branch/PR instead of opening competing work.
3. Use a descriptive branch such as feat/player-movement, fix/camera-jitter, or chore/unity-upgrade. Never work directly on main.
4. Implement the agreed feature, update relevant documentation and asset provenance, and add meaningful tests for the behavior. Do not expand into unrelated issues.
5. Open a draft PR early for multi-issue features; list all related issues and remaining acceptance criteria. Use "Closes #123" only for issues fully completed by the PR. Keep incomplete issues explicitly listed as remaining work.
6. Keep the PR title, summary and checklist current as the feature evolves. The final title describes the complete player-visible result and becomes the single squash commit/changelog entry.
7. Perform static checks and self-review while developing. Do not run CI/tests after every commit, on every push, or merely because a draft PR exists.
8. Once the whole feature is ready, update the branch from main with a normal merge (never a force-push/rebase of shared history), mark the PR ready, and run the manual Prepare merge workflow (pull-request.yml) with pr_number=<number>. Inspect results and fix failures; rerun only after fixes or an invalidated revision.
9. Every test and both Windows/Linux player and launcher builds must pass for the exact PR revision. Any new commit requires final validation again. Do not bypass the merge-ready status.
10. Report the PR URL, implemented issues, documentation changes, validation results and remaining blockers. "Implement issue #123" authorizes the work through a ready PR and final checks when the feature is complete; it does not authorize merging or publishing a release.
11. Merge only when explicitly requested, using squash and the validated head SHA. If the feature contains unfinished issues, leave the PR as a draft and report progress without triggering final validation.

## Git safety
- Never force-push any branch, rewrite published history, delete main, or bypass repository rules.
- Never discard another agent's or the user's changes. Inspect the worktree before switching branches.
- Stage only task-related files. Never commit credentials, license secrets, generated build directories or unrelated assets.
- Use English Conventional Commits, including fix/perf/feat and BREAKING CHANGE when appropriate.
- A normal merge from main into a feature branch is allowed to update its base; merging a feature into main is squash-only.

## Design references
- Before gameplay or visual work, read docs/design/game-direction.md, docs/design/art-direction.md and the linked implementation plan. Preserve the agreed readability requirements and ask about unresolved product choices before locking them in.
- Track issue dependencies using native GitHub blocked-by relationships, not dependency lists in issue bodies. Read those relationships before starting work.
