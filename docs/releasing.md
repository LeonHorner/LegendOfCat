# Releasing and updates

Maintainer documentation. Do not trigger or publish a release without an explicit user request. See [Development](development.md) for merge preparation.

## Release
Configure Actions secrets UNITY_LICENSE, UNITY_EMAIL and UNITY_PASSWORD (GameCI activation).
Run **Actions > Release** on main. No mode or version input is needed.
Conventional Commits since the last release determine the version: fix/perf = patch, feat = minor, !/BREAKING CHANGE = major.
Docs/chore/test alone produce no release. The first release is 0.1.0. Later releases follow Conventional Commits; a breaking change can promote 0.x to 1.0.0.
After successful version planning, Unity EditMode tests and Windows/Linux player builds run in parallel. Platform packaging (including the self-contained launchers, startup smoke checks, installers and update feeds) may proceed after both player builds, but **publish** waits for all tests, both builds and both packages to succeed. Failed, skipped or cancelled required jobs cannot publish. The final release plan/version recheck still runs immediately before publication. GitHub Release dispatch remains manual and restricted to `main`; no PR event publishes a release.
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
[persistent data contract](data-migrations.md) governs future settings, save and level formats.
Keep update packages and feeds attached to releases. Old Java builds cannot self-update to this project.
Actual installation and N -> N+1 updates on Windows/Linux still require verification before promoting releases to players.
Follow [the first release test runbook](../RELEASE-TEST.md) and record both platforms' actual results.

## Setup status
Unity activation secrets are configured. The setup PR tracks final Windows/Linux validation.
Prepare merge and Release are separate manually triggered workflows. Neither runs automatically on pushes or PR events.
Release tooling audit reports transitive braces/micromatch and npm/ip-address advisories even with current direct dependencies. Recheck before the first release.

## Asset provenance

Third-party components retain their own licenses. Record origins and human creative decisions in [ASSETS.md](../ASSETS.md). AI provenance does not guarantee copyright protection.
