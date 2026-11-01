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

## Playable route revision

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
entry. It fires once when unpaused Mika is grounded within x=110..114 at the
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

This remains a short layout awaiting pacing review. At seven units per second,
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
