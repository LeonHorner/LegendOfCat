# First installation and update test

Status: **not executed**. This document is a runbook, not evidence of verified update support.
Publishing N and N+1 requires separate explicit release authorization. Do not create an artificial
release merely to exercise the updater; use the next small, real fix for N+1.

## Preparation

- Finish the feature PR, merge current main normally, and run the manual `Prepare merge`
  workflow (`pull-request.yml`, `pr_number=<number>`) for the ready PR. Every test and both player/helper builds must pass on
  the exact head SHA before an explicitly authorized squash merge.
- Record the workflow URL, commit, tag, package names, OS version and installation location.
- Reserve a Windows desktop and a Linux desktop or graphical VM. Docker payload smoke tests
  do not verify a graphical installation. WSLg may be useful for supplementary testing but is
  not a substitute for the target Linux desktop installation/update test.
- Download stable packages only from https://github.com/LeonHorner/LegendOfCat/releases.
  Keep the release feeds and update packages attached. Builds are currently unsigned.

## Version N: fresh installation

1. After explicit authorization, run `Release` on main (no mode input). Let the workflow
   select the version; record it as N. Verify both platform packages and feeds were published.
2. On Windows, install using Setup.exe. On Linux, make the AppImage executable, place it in a
   writable location and launch it from a graphical session with required AppImage support.
3. Open the normal package entry. Unity's menu must appear without a separate launcher window.
   Verify Play, the test view, Back to menu, Settings, disabled Language, Versions & Updates,
   displayed installed version, official release links and Quit.
4. Disconnect networking and restart. The menu must stay usable; a failed background check
   must offer Retry/Later without preventing play. Restore networking and retry.
5. Capture the displayed version and the files under Unity's `Application.persistentDataPath`.
   Record the actual stable tag and shipped schema versions in the published format ledger in
   `docs/data-migrations.md`; do not freeze compatibility for unreleased preview data.
   Leave N installed on both systems. Do not install N+1 manually over these test installations.

## Version N+1: installed update

1. Deliver a small real visible fix through its own PR and exact-revision `prepare_merge`
   validation. After separately authorized merge and release, record the new tag as N+1.
2. Start installed N while online. Verify one pop-up for N+1 with Update now and Later.
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

## Evidence

| Check | Windows | Linux |
| --- | --- | --- |
| OS / desktop / installation location | Pending | Pending |
| N tag / commit / release workflow | Pending | Pending |
| Fresh install and graphical menu | Not run | Not run |
| Offline start and retry | Not run | Not run |
| N+1 tag / commit / release workflow | Pending | Pending |
| Later persists across restart | Not run | Not run |
| Settings update / progress / restart to N+1 | Not run | Not run |
| First-pop-up Update now | Not run | Not run |
| Interrupted download / retry | Not run | Not run |
| Preferences preserved / old version read-only | Not run | Not run |
| Logs / screenshots / remaining defects | Pending | Pending |

Do not mark update support verified until both real platform tests pass. If the game cannot
start at all after a failed installation, retain logs and use the official installer to repair
it; the in-game warning cannot be shown until the game can start again.
