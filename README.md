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
3. Run **Actions > Prepare merge** and enter the PR number.
   CLI equivalent: `gh workflow run pull-request.yml -f pr_number=123`.
4. The workflow validates the title and branch, then runs Unity tests and Windows/Linux player and launcher builds on the exact PR head SHA.
5. A successful run sets the required **merge-ready** status on that SHA. New commits require another final run; a newer main must first be merged into the branch.
6. Merge only when explicitly authorized: `gh pr merge 123 --squash --delete-branch --match-head-commit <validated-sha>`.

Main requires a PR, resolved conversations, the merge-ready status and an up-to-date branch. No reviewer approval is required.
Normal merge commits and rebase merges into main are disabled. Force-pushes and deletion of main are blocked, with no bypass actors.
Agents must never force-push any branch. Feature branches may merge main normally to update their base.
A requested issue implementation stops at a ready PR with final validation results; it does not imply permission to merge or release.

## Release
Configure Actions secrets UNITY_LICENSE, UNITY_EMAIL and UNITY_PASSWORD (GameCI activation).
Run **Actions > Release** on main. No mode or version input is needed.
Conventional Commits since the last release determine the version: fix/perf = patch, feat = minor, !/BREAKING CHANGE = major.
Docs/chore/test alone produce no release. The first release is 0.1.0. Later releases follow Conventional Commits; a breaking change can promote 0.x to 1.0.0.
The workflow reruns tests and both platform builds, packages and smoke-checks the payloads, then publishes only if everything succeeds.
Linux startup checks use Xvfb: this Unity player still requires a valid window backend with
`-batchmode -nographics`. Prepare merge also runs the Linux payload check. Both workflows retain
startup logs as artifacts; a virtual-display smoke check does not verify a graphical installation.
Git tags are the version source; no bot version commits. Builds remain unsigned until signing is configured.

## Updates
Launcher/ is a self-contained C# launcher using Velopack.
The package entry opens the Unity start menu immediately without a launcher window.
Play opens a test view; Settings contains the future Language option and Versions & Updates.
The updater checks public stable GitHub Releases in the background. Each new available version
prompts once with Update now / Later; Later persists across restarts and leaves a Settings badge.
Versions & Updates shows stable release history and release notes. Older versions cannot be installed.
Only the latest newer compatible stable version can be installed. Prereleases are excluded.
Downloads show progress while Unity remains open. Once ready, Unity exits and the background
helper waits for both game and package entry to exit, installs the update and restarts the game.
Network/download failures leave the current game playable with Retry / Later. Settings and
update diagnostics are stored under Application.persistentDataPath; incomplete installation
attempts show a warning on the next successful game start.
Editor/uninstalled builds and the internal Unity executable launched without the package entry
do not offer installation. Use the normal installed shortcut or AppImage for updates.
Saves belong in Application.persistentDataPath. Windows downloads use Setup.exe; Linux uses AppImage.
Menu settings use a versioned format with backup-before-migration and atomic writes. The
[persistent data contract](docs/data-migrations.md) governs future settings, save and level formats.
Keep update packages and feeds attached to releases. Old Java builds cannot self-update to this project.
Actual installation and N -> N+1 updates on Windows/Linux still require verification before promoting releases to players.
Follow [the first release test runbook](RELEASE-TEST.md) and record both platforms' actual results.

## Local Windows UI preview

Run Unity in batch mode with `-executeMethod BuildGame.BuildPreview` and
`-previewOutput <absolute-output-folder>/LegendOfCatPreview.exe` to build a windowed UI preview.
Keep the entire output folder with the executable; Unity players are not single-file applications.
Preview-only buttons show the update pop-up and simulate downloads/errors without networking,
installing updates or quitting to an updater. Preview settings use a separate persistent data
directory (`LeonHorner/LegendOfCat Preview`). The normal start menu, Settings and Play test view
are the same UI as the release build; simulation tools are compiled out of release builds.

## License and provenance
Original code: PolyForm Noncommercial 1.0.0 (LICENSE).
Original artwork/audio/models, where copyright exists: CC BY-NC 4.0 (ASSET-LICENSE.md).
Third-party components retain their own licenses. Record origins and human creative decisions in ASSETS.md.
AI provenance does not guarantee copyright protection.

## Setup status
Unity activation secrets are configured. The setup PR tracks final Windows/Linux validation.
Prepare merge and Release are separate manually triggered workflows. Neither runs automatically on pushes or PR events.
Release tooling audit reports transitive braces/micromatch and npm/ip-address advisories even with current direct dependencies. Recheck before the first release.

## Official Unity agent connection
The official com.unity.pipeline package is pinned to 0.9.0-exp.1. Open this project in Unity before using the connection.
The verified local CLI version is 1.0.0-beta.13. Run unity status to check readiness, then unity command get_hierarchy for a read-only check.
For Codex, configure a local MCP server running unity mcp --project-path <absolute-project-path>. Keep machine-specific executable and project paths out of Git.
The setup machine has a local .codex/config.toml excluded through .git/info/exclude. Reopen the project in Codex to load the connection.
CLI and MCP use the same Pipeline backend: CLI is convenient for scripts; MCP exposes discoverable structured tools directly to agents.
No tests are run by connecting MCP. Follow the manual final-validation policy in AGENTS.md.
