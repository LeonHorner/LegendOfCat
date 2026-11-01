# Golden River main route: integration notes

Issue #71 is in progress. These notes describe the starting point and the work
needed to turn it into Level 1; they are not evidence of a finished route.

## Existing prototype

The menu opens `MovementCourse`, a 109-unit painted movement lab with fourteen
platforms and four checkpoints. Its final sign does not provide a completion
event. The lab's existing geometry, checkpoints and menu round trip are covered
by integration tests and must remain usable when a separate Level 1 is added.
At the motor's maximum speed of seven units per second, crossing 109 units on
uninterrupted flat ground would take about 16 seconds. This arithmetic is not a
playtest or a prediction for the actual platform course; it shows why the
existing distance alone cannot substantiate the 4–6-minute first-success target.

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

## Remaining implementation and evidence

Design and implement the separate menu entry and main route using existing
movement, lifecycle and scenery. Place recovery points on safe ground at teaching
boundaries, then verify retries and full restarts against actual collision geometry.
Add a reachable exit and its completion contract. Exercise the course, camera,
pause, retry, restart and menu lifecycle in Unity behavior tests.

Record real first-success traversal times, failures by section and replay costs,
alongside full-route gameplay capture and small-window readability observations.
Use those observations to revise pacing and checkpoint placement. The target is
not measured or accepted yet. Integrate current main and the relevant recovery
work, then run exact-head Windows/Linux tests and builds before marking the PR
ready. No release or merge is authorized by this issue alone.
