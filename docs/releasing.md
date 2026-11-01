# Releasing and updates

Maintainer documentation. Do not trigger or publish a release without an explicit user request. See [Development](development.md) for merge preparation.

## Release
Configure GitHub Actions secrets `UNITY_EMAIL` and `UNITY_PASSWORD` for Unity Personal account activation in GameCI. No machine-bound `UNITY_LICENSE` file is required or passed to the workflows. GameCI uses account activation when only these credentials are provided. Ensure each CI job releases its temporary Unity activation normally; do not suppress activation or license return.
Run **Actions > Release** manually on `main` only after explicit authorization. Each job checks out the dispatched commit. semantic-release calculates the version and release notes from Conventional Commits and tags reachable from the dispatched commit. A temporary local remote pins its main branch to that commit for planning and publication; the GitHub publisher uploads the staged assets. The `allow_minor` input defaults to true and `allow_major` defaults to false. Patch releases are always allowed; an unapproved minor or major fails before Unity tests and builds. Version input and automatic downgrades are not supported. Before publishing, the version and release commit are checked against the validated snapshot. Later merges to main do not change the release: builds, notes and the tag remain tied to the dispatched commit, and later commits remain eligible for a subsequent release. A narrow adapter atomically creates the version tag through GitHub and verifies its target before publication. An existing release or tag, changed version, failed verification or invalid approval stops the run. The adapter never overwrites an existing tag or release.

Breaking headers such as `feat!:`, `fix!:`, and `fix(api)!:` or exact uppercase footers `BREAKING CHANGE:` and `BREAKING-CHANGE:` calculate a major release. Lowercase or mixed-case footer text does not. Inspect an unexpected major before authorizing it. Docs/chore/test alone produce no release. Tags are the version source; no bot version commits are made.

Unity EditMode tests and Windows/Linux builds run after planning. Packaging retains the launcher, smoke checks, installers, feeds and Delta verification. Publication waits for all required jobs. Linux startup uses Xvfb; this is not a graphical installation test.

### Interrupted release recovery

semantic-release treats an existing reachable version tag as a release boundary, even if publication failed. Do not blindly rerun or move/delete a tag. Inspect the original run, tag target, GitHub Release, uploaded assets and feeds, and preserve the original artifacts. Determine a manual recovery with explicit authorization; do not publish incomplete assets or overwrite a release. A rerun uses the original workflow commit, not later fixes. Builds remain unsigned until signing is configured.

## Public download assets

The release workflow first lets Velopack create its normal Windows/Linux packages. After
both platform builds and packaging succeed, `tools/release-assets.mjs` verifies the
`releases.win.json` and `releases.linux.json` feeds against their original referenced
`.nupkg` files, then stages the public release assets separately.

The three fixed player-facing asset names are:

- `LegendOfCat-Windows-Installer.exe` (the Velopack `*-Setup.exe` payload, renamed)
- `LegendOfCat-Windows-Portable.zip` (optional, not a self-contained save-data folder)
- `LegendOfCat-Linux-x86_64.AppImage` (the original Velopack AppImage, renamed)

The `LeonHorner.LegendOfCat` package ID, the `releases.*.json` feed contents,
all referenced `.nupkg` files and updater channels `win`/`linux` stay unchanged.
The `assets.*.json` upload manifests (only needed for Velopack deployment commands)
and legacy Squirrel `RELEASES` files are not attached to GitHub Releases.
Do not remove the feed files or `.nupkg` packages: `GithubSource` needs them for updates.
The staging script fails closed on missing, inconsistent or duplicate publish assets.
The packaging outputs themselves are not renamed or rewritten.


## Delta packages and publication gate

The **manually triggered** Release workflow uses the pinned Velopack CLI (v1.2.161)
and a separate packaging job for each channel (`win`, `linux`).

1. `node tools/delta-release.mjs prepare <channel> <target-version> <output-dir>`
   asks GitHub for the latest *stable published release*. On a first stable release
   (no predecessor) it deliberately uses full-only mode. Otherwise, it checks
   that the predecessor tag is older, both channel feed and correct full archive
   exist, downloads them by exact name, and verifies the archive size/SHA-1
   against that release's feed. A missing/incorrect asset, download problem or
   checksum error aborts the job rather than silently disabling Delta generation.
2. `dotnet vpk pack --delta BestSpeed` builds a new full archive and a Delta
   using the predecessor package in the same output directory.
3. `node tools/delta-release.mjs finalize ...` validates the new feed and
   its full/Delta SHA-1, optional SHA-256 and size, applies the new Delta to the downloaded
   previous full archive using `dotnet vpk delta patch`, and checks that the
   reconstructed new full archive has the same **SHA-256** as the built full
   archive. Missing or invalid Deltas block release publication.
4. A Delta is published only when it uses **at most 90%** of the new full
   package's size (at least 10% bandwidth savings). An oversized *verified*
   Delta is omitted, leaving a usable full-only release. A broken Delta never
   silently falls back. Previous version packages are removed from staging.
5. The public `releases.<channel>.json` is restricted to the exact new-version
   full and optional Delta. `tools/release-assets.mjs` independently verifies
   filenames, package ID, version, type, SHA-1, optional SHA-256 and byte size before publication.
   Output contains exactly the public player downloads and new packages/feeds.
   Package verification and Unity tests/Windows/Linux builds must all pass
   before any release becomes visible.

In `Prepare merge`, nonpublishing Windows/Linux integration jobs first exercise
the **actual pinned Velopack CLI** using official published full-package
v0.1.0 and v0.2.0 fixtures; they run `vpk delta generate` and
`vpk delta patch`, verifying the reconstructed full package's SHA-256.
These are genuine package-level tests (not mocked patches), but they do not
rebuild the next version, exercise `vpk pack` against its specific release
payload, or prove a graphical installation. The first authorized release still
runs the end-to-end packaging and reconstruction gate before publication.

GitHub's `GithubSource` can discover Deltas across tagged releases; each
GitHub Release includes only its own version's full and optional Delta.
The updater decides between Delta chains and full-package downloads.
A first update on Linux normally downloads the full package because an AppImage
installation does not have an existing local full-package Delta base.
The workflow reports package sizes and estimated bandwidth savings, not actual
runtime download savings. Real graphical N→N+1 and skipped-version updates must
still be tested separately on Windows and Linux as described in
[RELEASE-TEST.md](../RELEASE-TEST.md). The maintainer's successful
Windows v0.1.0→v0.2.0 test used the old full-only system and is not Delta evidence.


README links target `/releases/latest/download/<fixed-asset-name>`, while each release
description links to the exact release tag and puts the downloads above the changelog.
The README direct links will not resolve against v0.2.0, which still uses Velopack's
original names: coordinate merging the README change with the next separately authorized
release; do not claim new links are live until that release is available.

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
## User data and install paths

Unity's release builds set company name `LeonHorner` and product name `LegendOfCat`.
Current menu/update state is stored under `Application.persistentDataPath`:

- Windows: `%USERPROFILE%\AppData\LocalLow\LeonHorner\LegendOfCat\`
- Linux: `~/.config/unity3d/LeonHorner/LegendOfCat/` (or below
  `$XDG_CONFIG_HOME/unity3d/LeonHorner/LegendOfCat/` when set)

`menu-settings.json` stores versioned menu/update reminder settings.
`updates.log` and `update-pending.txt` support update diagnostics and recovery;
versioned settings backups may also exist. Gameplay saves/editable levels are **not yet
implemented**; when added, they must use this persistent-data directory and the
[storage migration contract](data-migrations.md). Preview builds use a separate product
directory to avoid overwriting release data.

Windows installs to `%LOCALAPPDATA%\LeonHorner.LegendOfCat\` by default
(per-user Velopack installation). On Linux, users choose where to keep the
self-updating AppImage. The Windows portable ZIP still stores settings in the
standard Unity directory, not next to the portable executable. Updates replace
application files, not this separate user data. Do not change the Unity
company/product names just to change the public installer filenames.

Menu settings use a versioned format with backup-before-migration and atomic writes. The
[persistent data contract](data-migrations.md) governs future settings, save and level formats.
Keep update packages and feeds attached to releases. Old Java builds cannot self-update to this project.
Actual installation and N -> N+1 updates on Windows/Linux still require verification before promoting releases to players.
Follow [the first release test runbook](../RELEASE-TEST.md) and record both platforms' actual results.

## Setup status
Unity Personal account credentials (`UNITY_EMAIL`, `UNITY_PASSWORD`) are configured. The old `UNITY_LICENSE` secret is unused and may be removed after successful account-only CI validation. The setup PR tracks final Windows/Linux validation.
Ready PRs automatically trigger Prepare merge checks; Draft PRs skip them. Release remains separately and manually triggered.
Release tooling audit reports transitive braces/micromatch and npm/ip-address advisories even with current direct dependencies. Recheck before the first release.

## Asset provenance

Third-party components retain their own licenses. Record origins and human creative decisions in [ASSETS.md](../ASSETS.md). AI provenance does not guarantee copyright protection.
