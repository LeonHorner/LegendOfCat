# Stomp encounter prototype

Issue #12. Enter **Stomp lab (prototype)** from the main menu. The isolated greybox reuses CatController, CatMotion, CatInput, RunLifecycle and MikaOriginal. It adds no campaign, save format, new health UI or traversal ability.

## Current encounter

- Warning: 1.2 seconds, striped ground band and closed dark head.
- Attack: 0.35 seconds, solid ground band and closed head.
- Recovery: 1.8 seconds, white vulnerable head. Descending feet must cross its top; side/body contact still hurts.
- The target moves in every phase, including recovery: x = 2 sin(0.6 t), radius 2 and maximum speed 1.2 units/second. Warnings, ground hazard and labels follow its actual position. Sweeps use relative player/target movement.
- A valid stomp removes one of three target health points, bounces Mika upward at 10 units/second and restarts the attack cycle without resetting patrol position.

The timing, patrol and health values remain prototype tuning for user review. A later production boss, phases, art and complex attack patterns belong to #18.

## Damage feedback selected for this revision

Damage preserves horizontal velocity and applies only an upward velocity of 7 units/second. Held movement remains active; no forced direction change, horizontal stop, lateral impulse or teleport occurs. Releasing jump does not shorten the automatic hop. Side contact, closed-head contact and the active ground band can cause damage.

A small original six-point cream contact star with a warm outline and three tiny sparks opens and fades over 0.15 seconds. It appears on the contacted boss/hazard surface at the collision sample, stays at that arena-space point instead of following either character, and has no collider. High-speed crossings use the swept contact; existing overlaps use the nearest surface point. The star uses native mesh geometry and contains no imported image. A reduced 35%-strength white flash fades over 80 ms so it does not compete with the star. No camera shake or hitstop is added.

Damage gives 1.25 seconds of protection. Mika gently pulses between original and half opacity every 0.25 seconds and never disappears. Protected contact causes no extra damage, hop or effect restart. Pause freezes physics, combat and effects. Retry/restart clear all transient feedback and restore the original sprite color. Effects finish fading after a terminal hit; win/loss retain Retry and Menu.

The previous lateral recoil and falling sine tone were rejected by the user. They are removed from active gameplay; **no damage sound is currently enabled**. The old `damage-tone.wav` is retained solely as a rejected historical preview.

![Contact star and restrained flash](contact-star.png)
![Held running right through a damage hop](damage-run-right.gif)
![Held running left through a damage hop](damage-run-left.gif)

## Sound choice pending

Three original unpitched noise-based proposals have comparable RMS level and quiet peak limits. They contain no external recordings, cat vocalizations, melody or pitch glide. Reproduce them with `python tools/generate-stomp-sound-previews.py`; the deterministic generator documents synthesis and envelope choices. None is wired into gameplay until user selection.

1. [Soft impact](sound-options/soft-impact.wav): low, softly damped impact, 120 ms.
2. [Muted tap](sound-options/muted-tap.wav): shorter, slightly clearer tap, 85 ms.
3. [Soft rustle](sound-options/soft-rustle.wav): airy fabric-like contact, 140 ms.

## Controls and reuse

Move/jump use the existing shared keyboard/controller actions, without a stomp button or device polling in combat. Retry: R / controller Select. Full restart: Backspace / controller LB. Pause: P / controller RB. Menu: Esc / controller Start. Retry and restart restore target health, patrol and phase, player health, movement/input state, protection and effects via RunLifecycle.ResetTransientState. Touch UI/mobile builds are outside this issue. Default unit scale and translation are supported; rotation/scaled placement is not a tuning contract.

## Evidence and validation

Run `StompReview.Record` in Unity 6000.3.27f2 to reproduce camera captures and CSVs. Phase screenshots explicitly advance state; seven motion sequences advance the real motor, isolated local 2D physics scene and combat. The running sequences feed shared motion values and preserve held direction through contact. CSVs record player/target position, health, protection, opacity, velocity and effect lifetime/contact position. The moving-head sequence starts above an open target to isolate hit detection and bounce. Camera captures exclude the GUI; GIF loops restart the demonstration and are not in-game teleports.

Compilation of runtime and regression sources plus the seven Play Mode captures completed with exit code 0 in an isolated review project with its own Library. The right-running trace starts at x=-1.1 with velocity (7,7) on damage, then x=-0.96 with velocity (7,6.4); left-running mirrors this without a direction reversal. The star remains at x=-0.85/+0.85 while its timer decreases. Captured contact-star rendering was visually inspected. The sound previews were checked for PCM format, duration, RMS and peak levels; subjective listening remains a user decision.

Regression sources cover top/side/closed windows, moving-head sweeps, protection expiry, preserved horizontal velocity, held run direction, hop release behavior, effect placement without retrigger, pause and retry cleanup. Automated tests and Windows/Linux player/launcher builds have not been run and remain part of final Prepare merge. Manual gameplay, small-window/grayscale review, sound choice and overall encounter acceptance remain pending. Keep PR #62 draft and #12 open until review and final exact-head validation complete.

## Coordination and review history

Started from main 989840a. Ownership covers new combat/arena/review/test code and narrow controller launch/menu entry integration. No overlap was identified with #51 (Blender authoring), #53 (reward design) or #59 (CI). Existing scenes, prefabs, packages, project settings, movement course and character artwork are preserved. No shared mutable Library is used for review captures. Review newer merged changes and merge main normally before readiness; no merge or release is authorized here.

User decisions on 2026-10-31: reject damage teleport; select short protection with transparency pulse; select a moving vulnerable head; exclude new heart-loss/health UI; reject the first tone and lateral recoil; preserve running with an upward-only damage hop; request sound alternatives and approve a small contact star with restrained flash. Earlier implementation screenshots/commits are historical, not acceptance of superseded feedback. Provenance is in ASSETS.md.
