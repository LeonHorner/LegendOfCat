# Ranged combat prototype (#13)

## Approved control concept — 2026-11-15

The user chose two attacks after comparing directional aiming, free aiming,
straight shots, arcing throws and single-button tap/hold controls. A shared
short/long-press button was rejected because it delays the direct shot until
release. Use separate buttons and movement-facing attacks, with no automatic aim.

- J / gamepad West: immediate straight paw pulse, one shot per press.
- K / gamepad right trigger: hold to charge, release to throw a light orb.
- Charging increases upward speed from 5 to 15 units/s over 0.8 seconds;
  forward speed stays 7 units/s. Orb gravity is 18 units/s². Charge saturates.
- The pulse moves at 15 units/s without gravity. Both use the facing direction
  at firing/release and share a 0.25-second recovery. No ammo, reload or pickup.
- A pulse cannot fire during an orb charge. Rejected cooldown presses are not
  buffered; another press is needed. Movement and jumping remain available.
- Existing A/D/arrows, Space, P/RB pause, Esc/Start menu, R/Select retry and
  Backspace/LB restart bindings are preserved. No mouse attack binding: clicking
  pause/menu controls must not shoot. No touch UI or mobile build is included.

The maximum ground-level orb apex is about 6.9 units; the flying target ranges
from 1.2 to 5.4 units. High targets are reachable without waiting for a descent.
Holding charge increases flight time and range as well as height. These are
prototype tuning values, not production boss specifications.

## Playable test

Choose **Ranged combat lab** in the main menu. A separate flat, enclosed arena
uses the retained complete-cat presentation and existing movement controller.
The moving practice target has six health points. Its 5-second cycle begins
with a 1.5-second shield, followed by 3.5 seconds open. A square silhouette and
SHIELDED label distinguish protection from the round OPEN target independently
of color. White hit flashes and HIT!/BLOCKED feedback distinguish outcomes.
There are no enemy attacks, contact damage, campaign changes or boss artwork.

Shots use relative swept collision against the moving target and disappear on
hit, floor/wall/ceiling contact, or after 3 seconds. At most eight are active.
Pulse and orb differ by silhouette as well as color. The muzzle indicator and
visible growing orb show facing and charge; the small lab HUD gives controls.

Pause freezes charge, flight, target and recovery. Releasing the charge while
paused cancels it without firing. Retry/restart restore the encounter, charge,
projectiles and facing. Focus loss and disabling clear charge/projectiles without
treating Input Action cancellation as a throw. Return disables the arena before
deferred destruction; re-entry creates a fresh test. Defeat stops combat and
uses the existing gameplay result menu. No persistent data format changes.

## Input and integration boundaries

CatInput owns Pulse and Lob bindings and forwards press/release, facing and
cancellation actions. RangedCombat owns charge, recovery, trajectory, collision
and target rules without reading devices. RangedArena connects these rules to
CatController, RunLifecycle and presentation. Future touch adapters can provide
the same actions; no touch aiming or automatic assistance has been selected.

This branch owns the new combat rules/arena/tests and the small CatInput/menu
integration. PR #79 overlaps StarterScreen and ASSETS.md; preserve its startup
measurement and append-only provenance during integration. No shared terrain,
scene, prefab, package or project-setting edits are required.

## Review and validation

Behavioral checks cover immediate single shots, variable charge and release
facing, shared recovery, pause, lifecycle cancellation, shield/open hits, defeat,
projectile cleanup, moving-target sweeps and real keyboard/gamepad actions.
Visual review must inspect the actual player in motion, also at 640×360 and in
grayscale. Player feel and human simultaneous move/jump/attack usability remain
playtest judgments; automated checks do not establish owner acceptance.

Use the pinned Editor and the normal ready-PR exact-head Prepare merge workflow
for final tests and Windows/Linux player/launcher/package checks. A preview can
open the arena with `--preview-view ranged`; `--preview-capture` uses the existing
actual-player screenshot path. No release or merge is authorized.
