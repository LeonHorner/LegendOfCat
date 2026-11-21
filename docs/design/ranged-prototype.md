# Ranged combat prototype (#13)

## Accepted initial prototype controls - 2026-11-15

The user rejected explicit weapon switching and F/Q near the movement keys.
The owner accepted the initial prototype with two direct attacks and mouse
alternatives on 2026-11-15, authorizing regression checks and PR readiness.
Movement-facing aim remains; the mouse position does not aim shots.

| Attack | Magazine | Shot recovery | Reload |
| --- | --- | --- | --- |
| Paw pulse | 8 | 0.2 seconds | 1 second |
| Light orb | 3 | 0.4 seconds after release | 1.5 seconds |

- J / left mouse / gamepad West: immediate paw pulse, one shot per press.
- K / right mouse / gamepad RT: hold to charge the orb, release to throw.
- No weapon-switch action and no F/Q combat bindings. Both magazines are shown
  together at bottom center, with paw/orb icons, filled/hollow dots and progress.
- For this trial, only the most recently used attack reloads. Using the other
  attack cancels charge/reload and discards reload progress, without charging
  ammo for a canceled throw. R / gamepad North reloads the last-used attack early.
- Empty magazines automatically start a full reload when used or emptied.
  Recovery runs independently for each attack, including while the other is used.
  Ammo is consumed only when a projectile is emitted. Reserve ammo is unlimited.
- Orb upward speed increases from 5 to 15 units/s over 0.8 seconds; forward speed
  stays 7 units/s. Orb gravity is 18 units/s^2. Pulse speed is 15 units/s.
- Movement and jump remain free during charge, recovery and reload. A/D/arrows,
  Space/W/Up, P/RB pause, Esc/Start menu and Backspace/LB restart remain.
  Retry is in the pause menu here; other game modes retain R/Select retry.

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
Pulse and orb differ by silhouette as well as color. Mika's facing and the visible growing orb show direction and charge. The compact HUD sits at the bottom
center, without the former upper controls/debug panel.
Magazine ammunition uses one filled or hollow circle per round (eight for pulse,
three for orb). Empty circles fill only when reload completes; each attack's binding
and charge/reload percentage remain beside them. A narrow progress bar fills
beneath the dots during reload. A cool paw icon with a small light spark and a warm round orb icon
identify the two direct attacks by shape and color alongside their bindings.
The permanent blue muzzle bar is removed. The enlarged pause menu shows actions beside the current keyboard/mouse/controller
bindings at wide sizes, stacking them with scrolling in narrow windows.

Pause freezes charge, flight, target, recovery and reload. Releasing the charge while
paused cancels it without firing. Retry/restart restore the encounter, charge,
projectiles, magazines, reload, last-used attack and facing. Focus loss and disabling clear charge/projectiles without
treating Input Action cancellation as a throw. Return disables the arena before
deferred destruction; re-entry creates a fresh test. Defeat stops combat and
uses the existing gameplay result menu. No persistent data format changes.

## Input and integration boundaries

CatInput owns Pulse, Lob and Reload bindings and forwards press/release, facing and
cancellation actions. RangedCombat owns charge, recovery, trajectory, collision
and target rules without reading devices. RangedArena connects these rules to
CatController, RunLifecycle and presentation. Future touch adapters can provide
the same actions; no touch aiming or automatic assistance has been selected.

This branch owns the new combat rules/arena/tests and the small CatInput/menu
integration. PR #79 overlaps StarterScreen and ASSETS.md; preserve its startup
measurement and append-only provenance during integration. PR #89 overlaps the
shared pause/menu code; its recorded integration owner must reconcile its level
completion and audio/settings hooks with this menu before that feature is ready. No shared terrain,
scene, prefab, package or project-setting edits are required.

## Review and validation

Behavioral checks cover immediate single shots, variable charge and release
facing, independent recovery, magazines, automatic/manual reload, alternating attacks, pause, lifecycle cancellation, shield/open hits, defeat,
projectile cleanup, moving-target sweeps and real keyboard/mouse/gamepad actions.
Visual review must inspect the actual player in motion, also at 640x360 and in
grayscale. Player feel and human simultaneous move/jump/attack usability remain
playtest judgments; automated checks do not establish owner acceptance.

Use the pinned Editor and the normal ready-PR exact-head Prepare merge workflow
for final tests and Windows/Linux player/launcher/package checks. A preview can
open the arena with `--preview-view ranged`; `--preview-capture` uses the existing
actual-player screenshot path. No release or merge is authorized.


Actual-player clips and compact/grayscale inspection are in the [review evidence](ranged-prototype/README.md).

The owner accepted this initial version on 2026-11-15. Regression tests now cover
the direct attacks and mouse buttons. Final exact-head validation is required;
historical checks for the superseded switching controls do not validate this head.
