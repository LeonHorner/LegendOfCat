# LegendOfCat

Unity starter for a 2D game with optional 3D visuals. The previous Java game is not migrated.

## Open
Install Unity Hub and Unity **6000.3.27f1**, then add this folder as a project.
Use **LegendOfCat > Create starter scene** to create/open the initial scene.
Commit generated Unity settings, Packages/packages-lock.json and any new .meta files after the first import.
The starter scene test and Windows player build have passed in CI; full platform validation is recorded in the setup PR.

## Repository language
Use English for maintained code, comments, documentation, filenames, branches, commits, PRs, UI text and release notes. Preserve third-party legal notices and proper names verbatim.

## Issue-to-feature development
Tell an agent: "Implement issue #123." The agent reads AGENTS.md, the issue and related work, then reuses or creates a feature branch and PR.
One coherent feature can contain several issues and stays on one branch until complete.
Draft PRs track unfinished features. List every related issue, acceptance criterion, remaining task and documentation change.
Use an English Conventional Commit PR title describing the complete user-visible result. Squash merge turns it into one commit and one release-note entry.
External code contributions are not accepted; public bug reports and feature requests are welcome.

## Prepare a merge
Pushes, PR creation and individual commits do not run tests or builds.
Once the entire feature is complete:
1. Merge current main into the feature branch without rewriting published history.
2. Mark the PR ready and finish its documentation and self-review.
3. Run **Actions > Release**, select **prepare_merge**, and enter the PR number.
   CLI equivalent: `gh workflow run release.yml -f mode=prepare_merge -f pr_number=123`.
4. The workflow validates the title and branch, then runs Unity tests and Windows/Linux player and launcher builds on the exact PR head SHA.
5. A successful run sets the required **merge-ready** status on that SHA. New commits require another final run; a newer main must first be merged into the branch.
6. Merge only when explicitly authorized: `gh pr merge 123 --squash --delete-branch --match-head-commit <validated-sha>`.

Main requires a PR, resolved conversations, the merge-ready status and an up-to-date branch. No reviewer approval is required.
Normal merge commits and rebase merges into main are disabled. Force-pushes and deletion of main are blocked, with no bypass actors.
Agents must never force-push any branch. Feature branches may merge main normally to update their base.
A requested issue implementation stops at a ready PR with final validation results; it does not imply permission to merge or release.

## Release
Configure Actions secrets UNITY_LICENSE, UNITY_EMAIL and UNITY_PASSWORD (GameCI activation).
Run **Actions > Release** on main and explicitly choose **release**. No version input is needed.
Conventional Commits since the last release determine the version: fix/perf = patch, feat = minor, !/BREAKING CHANGE = major.
Docs/chore/test alone produce no release. The first release is 1.0.0.
The workflow reruns tests and both platform builds, packages and smoke-checks the payloads, then publishes only if everything succeeds.
Git tags are the version source; no bot version commits. Builds remain unsigned until signing is configured.

## Updates
Launcher/ is a self-contained C# launcher using Velopack.
It checks public GitHub Releases once per launch, applies available updates before starting the game, and starts the installed version if checking/downloading fails.
Saves belong in Application.persistentDataPath. Windows downloads use Setup.exe; Linux uses AppImage.
Keep update packages and feeds attached to releases. Old Java builds cannot self-update to this project.
Actual installation and N -> N+1 updates on Windows/Linux still require verification before promoting releases to players.

## License and provenance
Original code: PolyForm Noncommercial 1.0.0 (LICENSE).
Original artwork/audio/models, where copyright exists: CC BY-NC 4.0 (ASSET-LICENSE.md).
Third-party components retain their own licenses. Record origins and human creative decisions in ASSETS.md.
AI provenance does not guarantee copyright protection.

## Setup status
Unity activation secrets are configured. The setup PR tracks final Windows/Linux validation.
The first workflow setup PR remains open until its final checks pass; the already-registered Release workflow can be dispatched on that setup branch for bootstrapping.
Release tooling audit reports transitive braces/micromatch and npm/ip-address advisories even with current direct dependencies. Recheck before the first release.
