# Golden River main route: integration notes

Issue #71 is in progress. These notes describe the starting point and the work
needed to turn it into Level 1; they are not evidence of a finished route.

## Existing prototype

The menu also opens `MovementCourse` as a 109-unit painted movement lab with fourteen
platforms and four checkpoints. Its final sign does not provide a completion
event. The lab's existing geometry, checkpoints and menu round trip are covered
by integration tests and must remain usable when a separate Level 1 is added.
At the motor's maximum speed of seven units per second, crossing 109 units on
uninterrupted flat ground would take about 16 seconds. This arithmetic is not a
playtest or a prediction for the actual platform course; it shows why the
existing distance alone cannot substantiate the 4â€“6-minute first-success target.

## Boundaries

- The main route owns its collision geometry, start, two recovery checkpoints,
  optional-route entrance and rejoin space, and a recognizable exit location.
- The movement lab remains a separate selectable prototype. Preserve its existing
  four-checkpoint behavior and regression coverage.
- A completion signal must identify the active run, fire once on a valid exit,
  and reset appropriately on retry or restart. Audio, completion presentation,
  menu transition and persistent results belong to later issues.
- PR #61 owns water-entry recovery and modifies `MovementCourse`, `CatController`
  and `CatCamera`. Reconcile its actual final implementation before claiming
  integration; do not duplicate its recovery sequence or overwrite its changes.
- Issues #72 and #73 own the optional traversal and scenery pass respectively.
  Reserve stable interfaces before either integrates shared scene or asset work.

## Initial playable route revision (superseded by the expanded introduction below)

The menu's **Golden River preview** creates a separate course. Movement Lab keeps
its fourteen platforms, four checkpoints and existing menu lifecycle. Level 1
uses eleven broad platforms from x=-5 to x=115, reusing the painted scenery and
unchanged run/jump motor:

| Section | Ground | Demand |
| --- | --- | --- |
| Sheltered introduction | x=-5..9 | Run, stop and turn on one broad bank; a sign introduces jumping at the edge. |
| First jumps | x=11..33 | Two level jumps with ten-unit landing banks; checkpoint 1 at x=28. |
| Practice | x=35..84 | Broad approach, half-unit rises, then descent; widths never below six units. |
| Final combination | x=86..115 | Rise, rise, descent using taught demands; broad exit bank. |

Checkpoint 2 is at x=78, before the final combination. Both checkpoint spawn
centers are 0.52 above their zero-height contact surface, well inside broad ground.
The old x=9/34/65/83 checkpoints belong exclusively to Movement Lab.
`UpperRouteEntrance` (39, 0.52) and `UpperRouteRejoin` (78, 0.52) reserve broad
main-route banks for #72; this revision adds no optional platforms or rewards.
No scenes, prefabs, metadata, packages, movement rules or character artwork are
hand-edited. Existing original/generated artwork is reused without new assets.

### Exit contract for #74

Subscribe to `MovementCourse.ExitReached` on the active course instance after
entry. It fires once when unpaused Mika is grounded within one unit before to three units after `LevelOneExit.x` at the
final bank's contact height. `HasReachedExit` is set before callbacks run. Retry
and full restart clear it; another valid traversal can signal again. Closing the
course destroys that session; consumers must detach handlers when their own
lifetime ends. No durable campaign result, final UI, sound or automatic menu
transition is implemented. `LevelOneExit` is the stable exit reference point.

### Evidence and remaining gates

The full-route regression uses the actual course colliders, CatController and
Physics2D simulation, with only move/jump commands. It checks traversal without
retry, both checkpoint activations, exit reachability, bounded camera position,
pause, retry and full restart. It is a deterministic reachability test, **not**
a first-player timing measurement or gameplay capture.

The initial revision was a short layout awaiting pacing review. At seven units per second,
the 114-unit start-to-exit distance is far below four minutes without substantial
learning pauses. Do not label this the completed 4–6-minute course or claim the
1–2-minute checkpoint replay target. Do not slow the motor or add forced waits
to manufacture duration. Use real playtest observations to decide how much
additional authored traversal is needed while retaining the existing composition.

The user agreed on 2026-11-01 to test the route. Record first-success time excluding
failures, total time including retries, falls per section, time from retry to the
next meaningful attempt and any unclear edge/sign. Capture a complete route,
review at 960x540 and in grayscale, and revise pacing/checkpoints from that evidence.
These are pending acceptance criteria, not optional polish.

PR #61 is still a draft awaiting foreground-placement review. Its water/camera
handoff is not merged into this route; this branch leaves CatController and
CatCamera untouched. Reconcile its accepted implementation before final readiness.
Current remote main was 2b18294 at inspection. Full exact-head Windows/Linux tests,
player/launcher builds and packaging gates remain required once implementation and
playtest evidence are complete. Keep the PR draft and #71 open until then.

## Local verification — 2026-11-01

Unity 6000.3.27f2: **15/15 scoped tests passed**, no failures or skips
(`MenuMovementIntegrationTests`, `RunLifecycleTests`, `CatCollisionTests`).
This includes scripted whole-course collision traversal and exit/checkpoint
lifecycle coverage. The initial preview test had a callback lost across Editor
Play Mode domain reload and frame-scheduling-dependent checkpoint checks; both
were corrected with a stable callback and explicit simulation. No player-input
playtest, measured first-success time, video or platform packaging gate is claimed.
The fresh Editor import produced no package-lock or asset-metadata changes.

For local playtesting, build with `BuildGame.BuildPreview` and launch
`LegendOfCatPreview.exe --preview-view level-one`; alternatively select
**Golden River preview** from its menu. A/D or arrows move, Space jumps, R retries,
Backspace restarts, P pauses and Escape returns to the menu. Keep the entire
preview output directory together. This local development build is not a release.

The local Windows development preview build completed successfully with the pinned
Editor. Windows/Linux release-style packaging and launcher builds remain unrun.

[Rendered start view](golden-river-main-route/start.png): the Windows player loaded
the route and Mika/scenery correctly. The saved preview display settings selected
1280x720 despite the requested 960x540 launch size; this single still does not
establish small-window or full-route motion readability.


## User playtest - 2026-11-01

The user reported a 20-second complete traversal of the supplied Windows preview,
then confirmed no falls and no unclear places. This is one user-reported run,
not instrumented timing or a recorded gameplay capture. No failed-attempt time
was reported; checkpoint recovery/replay cost was not exercised in this run.

The result is 12-18 times shorter than the approved 4-6-minute target. It supports
initial route reachability and clarity for this tester, not the intended duration,
learning curve or checkpoint spacing. The route needs substantial additional,
varied authored traversal before #71 can be considered complete. Preserve motor
responsiveness; do not manufacture duration with forced waits or slower movement.

## Expanded introduction - 2026-11-01

After the 20-second, no-fall playtest the user approved expanding and testing the
first section before sizing the remaining sections. This revision replaces the
initial three-bank introduction with 23 level banks ending at checkpoint 1 at
(420, 0.52). Its design target is 60-90 seconds including learning pauses, not
an asserted first-player result. The start-to-checkpoint distance is 423 units;
at unchanged maximum speed it requires roughly 60 seconds of forward travel.

| Beat | Contact spans | Intent |
| --- | --- | --- |
| Sheltered start | -5..19 | Run, brake and reverse without an immediate nearby gap. |
| First jumps | 21..171 | Two-unit gaps with broad landings; alternate short jump practice and longer banks with room to stop. |
| Wider crossings | 174..283 | Three-unit gaps at the same height, with ten-unit or wider landings; no simultaneous elevation demand. |
| Changing rhythm | 285..406 | Alternate two/three-unit gaps and shorter/longer banks, testing the same jump at different intervals. |
| Safe recovery | 409..425 | Sixteen-unit bank; checkpoint center five units before its right edge. |

The later practice/final geometry is translated by +392 units, without changing
its widths, heights or gaps. Checkpoint 2 is now (470, 0.52), the optional entrance
is (431, 0.52), rejoin is (470, 0.52), and exit reference is (503, 0.52). There are
still exactly two checkpoints beyond start and 31 main-route platforms. These
later sections remain short placeholders for subsequent pacing iterations.

The compact HUD shows **Intro** session seconds and freezes that reading on first
activation of checkpoint 1. It includes time spent before retries, excludes pause,
and resets on full restart. It is session feedback, not a saved record or a clean
attempt timer. For this review, play from full restart to **INTRODUCTION COMPLETE**,
report the displayed time, falls, and whether any stretch feels repetitive, empty
or awkward. The rest of the route remains traversable after that marker.

This is a collision/pacing iteration using existing scenery. The longer route
exposes repeated/mirrored banks and scenery; unique landmarks and final scenery
remain #73's scope. Duration alone does not establish an enjoyable introduction;
shorten or rearrange dull stretches after the next player review. Motor, jump,
Mika artwork, water shader, Movement Lab and serialized assets are unchanged.

### Expanded-introduction verification

Unity 6000.3.27f2: **15/15 scoped tests passed**, no failures/skips, after the
layout/timer update. Scripted whole-route physics traversal took **60.40 seconds
to checkpoint 1** and **72.20 seconds to the current exit**, with no retries.
These are simulated uninterrupted traversal times, not human first-success times.
Regression checks also cover the frozen introduction reading, full-restart reset,
both exact camera limits, both checkpoint activations, pause and exit rearming.
The user playtest of this longer revision is pending.

The updated Windows development preview build succeeded. Its local output is
`Builds/GoldenRiverIntroPreview/LegendOfCatPreview.exe`; launch with
`--preview-view level-one` or choose Golden River preview in the menu. The earlier
`Builds/GoldenRiverPreview` folder contains the superseded short revision.
No release, installer or final Windows/Linux packaging gate was run.
