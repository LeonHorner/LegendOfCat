# First installation and update test

Status: **successful maintainer-reported in-app updates on Windows** (v0.1.0 -> v0.2.0)
**and Linux** (v0.5.0 -> v0.6.0). The full platform test matrix remains incomplete.
This document distinguishes observed smoke tests from comprehensive verification.
Publishing N and N+1 requires separate explicit release authorization. Do not create an artificial
release merely to exercise the updater; use the next small, real fix for N+1.

## Preparation

- Finish the feature PR, merge current main normally and mark the PR ready. Automatic `Prepare merge`
  validation (manual `pull-request.yml`/`pr_number=<number>` fallback) checks the exact head SHA.
  Every Unity test and both player/helper builds must pass before an explicitly authorized squash merge.
- Record the workflow URL, commit, tag, package names, OS version and installation location.
- Reserve a Windows desktop and a Linux desktop or graphical VM. Docker payload smoke tests
  do not verify a graphical installation. WSLg may be useful for supplementary testing but is
  not a substitute for the target Linux desktop installation/update test.
- Download stable packages only from https://github.com/LeonHorner/LegendOfCat/releases.
  Keep the release feeds and update packages attached. Player downloads are now named
  `LegendOfCat-Windows-Installer.exe`, `LegendOfCat-Windows-Portable.zip`, and
  `LegendOfCat-Linux-x86_64.AppImage`. Builds are currently unsigned.

## Version N: fresh installation

1. After explicit authorization, run `Release` on main (no mode input). Let the workflow
   select the version; record it as N. Verify both platform packages and feeds were published.
   Check that the release shows the three player downloads plus the two `releases.*.json`
   feeds and referenced `.nupkg` files; do not ship `assets.*.json` or legacy `RELEASES`.
   Verify the three player links in the release notes and the README `latest/download/` URLs.
   When upgrading from v0.2.0, expect its existing asset names to differ.
2. On Windows, install using `LegendOfCat-Windows-Installer.exe`. On Linux,
   make `LegendOfCat-Linux-x86_64.AppImage` executable, place it in a
   writable location and launch it from a graphical session with required AppImage support.
3. Open the normal package entry. Unity's menu must appear without a separate launcher window.
   Verify Play, the movement prototype, Back to menu, Settings, disabled Language, Versions & Updates,
   displayed installed version, official release links and Quit.
4. Disconnect networking and restart. The menu must stay usable; a failed background check
   must offer Retry/Later without preventing play. Restore networking and retry.
5. Capture the displayed version and the files under Unity's `Application.persistentDataPath`.
   Record the actual stable tag and shipped schema versions in the published format ledger in
   `docs/data-migrations.md`; do not freeze compatibility for unreleased preview data.
   Leave N installed on both systems. Do not install N+1 manually over these test installations.

## Version N+1: installed update

1. Deliver a small real visible fix through its own ready PR and exact-revision `Prepare merge`
   validation. After separately authorized merge and release, record the new tag as N+1.
2. Start installed N while online. Verify one pop-up for N+1 with Update now and Later.
   If N is an older release with the original Velopack installer/AppImage names,
   confirm its update succeeds without reinstalling or migrating user data.
3. Choose Later, quit and restart. The same version must not prompt again. Settings and
   Versions & Updates must continue to show the available update.
4. Open Versions & Updates, select Update now and observe download percentage. Unity remains
   open until the download completes; then it closes, the helper installs and the game restarts.
5. Confirm N+1 in the menu and Settings, unchanged saved preferences, no repeat pop-up and a
   read-only history entry for N. Check installation failures in `updates.log`, the pending
   marker and Velopack's own logs if restart fails. Relaunching N must warn about an incomplete update.
6. On a separate retained installation of N, interrupt networking during download. The current
   game must remain usable. Retry must succeed after networking is restored. Repeat the update
   sequence with Update now chosen directly in the first pop-up.
7. Confirm a second normal launch of the same installation cannot run a competing game/update.
   Directly launching the internal Unity player intentionally disables update installation.

## Linux update recovery regression (#40)

**Historical failures (before the v0.5.0 updater fix):** A Linux v0.1.0 AppImage
downloaded v0.2.0 but failed to apply and restart. After PR #42, a separate real
v0.3.0 -> v0.4.0 attempt still failed: the `--update-service` helper crashed
with SIGBUS shortly after the "Starting Velopack handoff" log entry. Crash
evidence is recorded in sanitized form in issue #40; the exact FUSE mount
lifetime explanation was not confirmed by a controlled trace in that investigation.

**Subsequent successful smoke test (maintainer report, 2026-10-31):** An in-app
Linux AppImage update from stable **v0.5.0 to v0.6.0 succeeded**. The repaired
Linux updater was therefore present in v0.5.0. This supersedes the earlier
failure as the latest reported outcome, but it does not retroactively turn the
v0.1.0/v0.3.0 tests into passes. Automatic restart, retained settings, the exact
Linux distribution, logs and failure/recovery cases were not separately reported.

The new Linux handoff stages the exact Velopack 1.2.161 `UpdateNix` and its
`sq.version` outside the original mount, launches a standalone POSIX supervisor,
and only then sends the Unity shutdown message. It waits for BOTH game and
entry launcher identities to disappear before launching the native updater.
On timeout it refuses to replace the old AppImage. This deliberately does not
rely on Velopack 1.2.161's `--waitPid` because that version can continue
after its 60-second wait fails. The Windows path remains separate.

**Verification levels and limitations:**

1. `node --test tools/linux-update-supervisor.test.mjs` executes the real POSIX
   supervisor with genuine Linux child PIDs; it checks readiness, ordering,
   timeouts, errors and safe argv handling. It uses a fake native updater.
2. The Linux native `package` job in `Prepare merge` builds local
   `0.0.1`/`0.0.2` packages and, if the runner supports FUSE, mounts a real
   old AppImage, tears down the old mount while the supervisor is running,
   applies a real full update with the staged Velopack `UpdateNix`, and
   verifies that the *same renamed file* mounts with different version
   metadata. This is a nonpublishing native lifecycle test, **not** a game-UI
   test; no success may be claimed if FUSE is unavailable.
3. `.github/workflows/linux-appimage-update.yml` offers an additional
   manually dispatched, isolated replay of the real FUSE test.
4. A real Linux N -> N+1 in-app update has been reported successful for
   v0.5.0 -> v0.6.0. The full acceptance matrix still requires explicit evidence
   of the graphical Settings UI, automatic restart, correct version on next
   launch, settings/saves retention, pending reconciliation, absence of new
   SIGBUS/core dumps, and recoverable failure scenarios before closing #40.
   A graphical VM or Xvfb can automate core functionality; a normal Linux
   desktop remains useful for dialogs and end-user presentation.

**For both cached/full and fresh download tests**, verify the original
AppImage path (including spaces and a user-renamed filename), record actual
PID/mount transitions, check the exit status/native logs and ensure a stalled
or failed preparation never authorizes shutdown. Verify Windows behavior after
this change. Do not publish unredacted machine paths, usernames or crash dumps.

The older published Linux AppImages still contain the old launcher and may
require a one-time manual replacement before the repaired updater is available.

## Delta update validation (future stable N -> N+1)

Before authorizing the release, confirm that the new Release workflow jobs for
**both** platforms passed the downloaded-baseline checksum validation, Delta
reconstruction SHA-256 check, feed validation and asset-staging gate. Record the
workflow summary's baseline version, complete package size, Delta size, savings
percentage or documented full-only reason. A first release with no previous
stable archive is legitimately full-only; a failed download or corrupt patch
must instead block publication. No artificial public release is required.

On Linux, the **first AppImage update normally requires a full package** because the
initial installation does not contain a local full-package Delta base. Test a subsequent
Linux update after that base exists before expecting an actual Delta download.

On a real Windows installation and a Linux desktop/AppImage, check:

1. Keep stable version N installed with identifiable saved preferences.
   Record the current install package and version. Do not assume the
   release workflow's patch verification proves the live update path.
2. After a separately authorized regular N+1 release, start N and monitor the
   actual update logs/network traffic to determine whether Velopack downloaded
   a Delta or full package. Where a Delta is available and beneficial, record
   which path was actually chosen rather than assuming it was used.
3. Apply the update. Confirm the restarted application reports N+1, retains
   settings, and presents no corruption or repeated update prompt.
4. Test an installation that skips one or more stable versions. Velopack may
   choose a Delta chain or a full archive; verify both download choice and
   successful result.
5. Where practical, exercise fallback to the full package and
   interrupted-download/retry behavior without publishing invalid Delta assets.
   Log failures and preserve a rollback/reinstall path.

The first N+1 release with verified Delta artifacts is a **package-generation
milestone**, not yet proof of correct end-to-end Delta updates on both desktops.
Only mark platform-specific Delta support verified after collecting actual
installation logs and platform test evidence.

## Observed Windows update: v0.1.0 -> v0.2.0

Recorded on 2026-10-23 from the maintainer's report of a real Windows installation:

- In-app update from stable v0.1.0 to stable v0.2.0 completed successfully.
- Existing settings were preserved.
- The game automatically restarted after the update.

This is a user-reported functional smoke test, not a CI result or a recorded
comprehensive platform validation. The Windows version/build, installation path,
download progress, log captures, Later/reminder behavior, interruption/retry,
release history, and detailed update package selection were not reported.
The update used the release process *before* public installer filenames were
changed; this observation does not validate Delta packages or new download names.
A subsequent Linux update (v0.5.0 -> v0.6.0) was successful according to a
separate maintainer report, though comprehensive Linux validation is incomplete.

## Observed Linux update: v0.5.0 -> v0.6.0

Recorded on 2026-10-31 from the maintainer's report:

- The Linux in-app update from stable v0.5.0 to v0.6.0 succeeded.
- The Linux updater fix was already included in v0.5.0.
- Automatic restart, settings preservation, exact installation path/desktop,
  runtime logs, checksum/feed details, update package selection, retry behavior
  and next-launch reconciliation were not separately confirmed in this report.

This is functional user-reported evidence, not a comprehensive validation run or
proof that every older Linux AppImage can update without a manual replacement.
See #40 for the earlier failures and remaining edge-case acceptance criteria.

## Evidence

| Check | Windows | Linux |
| --- | --- | --- |
| OS / desktop / installation location | Pending | Pending |
| N tag / commit / release workflow | Pending | Pending |
| Public download names, direct links and retained update feeds | Not run | Not run |
| Fresh install and graphical menu | Not run | Not run |
| Offline start and retry | Not run | Not run |
| N+1 tag / commit / release workflow | Pending | Pending |
| Later persists across restart | Not run | Not run |
| Installed N -> N+1 update / automatic restart | PASS (maintainer report, v0.1.0 -> v0.2.0, restart confirmed) | PASS (maintainer report, v0.5.0 -> v0.6.0; automatic restart not separately confirmed). Earlier FAILs: v0.1.0 -> v0.2.0 and v0.3.0 -> v0.4.0 (#40) |
| Settings update path and download progress | Not reported | Not run |
| Delta artifact SHA-256 reconstruction / release feed | Not run | Not run |
| Actual Delta or full download choice (N → N+1) | Not observed | Not run |
| Skipped-version Delta chain / full fallback | Not run | Not run |
| First-pop-up Update now | Not run | Not run |
| Interrupted download / retry | Not run | Not run |
| Preferences preserved across update | PASS (maintainer report) | Not run |
| Old version release history read-only | Not reported | Not run |
| Logs / screenshots / remaining defects | Pending | SIGBUS in update helper; sanitized evidence in #40 |

Do not mark the full cross-platform acceptance matrix verified on these smoke
reports alone. If the game cannot start at all after a failed installation,
retain logs and use the official installer to repair it; the in-game warning
cannot be shown until the game can start again.
