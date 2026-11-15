# Ranged combat prototype (#13)

## Approved control concept - 2026-11-15

The user first tested separate direct-attack buttons, then approved switching
between two magazines with one shared attack button. Movement-facing aim remains;
there is no automatic aim or aim stick.

| Weapon | Magazine | Shot recovery | Reload |
| --- | --- | --- | --- |
| Paw pulse | 8 | 0.2 seconds | 1 second |
| Light orb | 3 | 0.4 seconds after release | 1.5 seconds |

- F or J / gamepad West: use the selected weapon. Pulse fires on press; orb charges
  while held and fires on release. One shot per press/release, no auto-repeat.
- Q or K / gamepad right trigger: switch weapon. R / gamepad North: early reload.
- An empty magazine starts reloading immediately after the last shot. Reload
  and shot recovery overlap; each weapon's recovery keeps ticking while inactive.
- Only the active weapon reloads. Switching discards all reload progress. On
  returning to an empty weapon a fresh full reload starts; a partly full weapon
  waits for R or emptying. Reserve ammo is unlimited; there are no ammo pickups.
- Switching or early reload cancels an orb charge without spending ammo. Ammo is
  consumed only when a projectile is emitted. Rejected presses are not buffered.
- Charging increases upward speed from 5 to 15 units/s over 0.8 seconds;
  forward speed stays 7 units/s. Orb gravity is 18 units/s^2. Pulse speed is 15.
- Movement and jumping remain free during charge, recovery and reload.
- A/D/arrows, Space, P/RB pause, Esc/Start menu and Backspace/LB restart remain.
  Retry is in the pause menu for this arena. Other game modes keep R/Select retry.
  No mouse attacks, touch UI or mobile build are included.

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
visible growing orb show facing and charge. The compact HUD sits at the bottom
center, without the former upper controls/debug panel.
Magazine ammunition uses one filled or hollow circle per round (eight for pulse,
three for orb). Empty circles fill only when reload completes; the active weapon
and charge/reload percentage remain beside them. A narrow progress bar fills
beneath the dots during reload. A cool narrow pulse icon and warm round orb icon
identify the selected weapon by shape and color alongside its name.

Pause freezes charge, flight, target, recovery and reload. Releasing the charge while
paused cancels it without firing. Retry/restart restore the encounter, charge,
projectiles, magazines, reload, active weapon and facing. Focus loss and disabling clear charge/projectiles without
treating Input Action cancellation as a throw. Return disables the arena before
deferred destruction; re-entry creates a fresh test. Defeat stops combat and
uses the existing gameplay result menu. No persistent data format changes.

## Input and integration boundaries

CatInput owns Attack, SwitchWeapon and Reload bindings and forwards press/release, facing and
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
facing, independent recovery, magazines, automatic/manual reload, switching, pause, lifecycle cancellation, shield/open hits, defeat,
projectile cleanup, moving-target sweeps and real keyboard/gamepad actions.
Visual review must inspect the actual player in motion, also at 640x360 and in
grayscale. Player feel and human simultaneous move/jump/attack usability remain
playtest judgments; automated checks do not establish owner acceptance.

Use the pinned Editor and the normal ready-PR exact-head Prepare merge workflow
for final tests and Windows/Linux player/launcher/package checks. A preview can
open the arena with `--preview-view ranged`; `--preview-capture` uses the existing
actual-player screenshot path. No release or merge is authorized.


Actual-player clips and compact/grayscale inspection are in the [review evidence](ranged-prototype/README.md).
