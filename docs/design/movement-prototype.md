# Movement prototype

Issue #9 supplies a temporary movement lab through **Movement lab (prototype) > Enter movement lab**
on the painted menu integrated from PR #31. Return with **Back to menu**, Escape or controller Start.
This is a tuning baseline, not approval of final character art, campaign geometry or difficulty.
No traversal ability beyond running and jumping is introduced.

## Controls and reuse

The `Gameplay` Input System map defines Move (axis), Jump, Retry, FullRestart, Pause and Menu.
`CatInput` owns keyboard/controller bindings and forwards values and press/release callbacks
to `CatMotion`. The motor and collision controller contain no device polling.
Keyboard: A/D or arrows, Space/W/Up to jump, R to retry at the active checkpoint,
Backspace to restart the run, P to pause/resume, Escape to return.
Controller: stick/D-pad, South button to jump, Select to retry, left shoulder to
restart, right shoulder to pause/resume, Start to return.
Future touch bindings can drive the same actions; touch UI and mobile builds are outside this prototype.
Press/release between physics steps retains a short jump; a brief axis tap survives for one step.
Focus loss clears input and jump grace. Holding jump does not repeat jumps on landing.

`Assets/Game/Resources/MovementCourse.prefab` contains the independent `MovementCourse`
bootstrap. Instantiate it from another scene without menu or save dependencies.
The bootstrap constructs the course, cat, controls and camera; platform tuples in
`MovementCourse.Platforms` are center X, center Y and width, with 1.5 m solid fronts.
Use **LegendOfCat > Create movement course prefab** to regenerate the prefab in the Editor.
No serialized scene edits are required. The existing launcher/settings/update service stays active.

## Tuning and jump envelope

World units are treated as metres. The default physics step remains 0.02 s (50 Hz).

| Property | Baseline |
|---|---|
| Horizontal speed | 7 m/s |
| Acceleration / reversal and braking | 55 / 80 m/s² |
| Launch speed / downward acceleration | 12 m/s / 30 m/s² |
| Maximum fall speed | 20 m/s |
| Coyote time / input buffer | 0.10 / 0.12 s |
| Early-release upward speed cap | 5 m/s |
| Collision box | 0.9 × 0.9 m |

The continuous full-hold approximation gives a 2.4 m apex, 0.8 s flight and
5.6 m center travel at full speed on equal-height surfaces. Fixed-step gravity
reduces this to about 2.28 m and 5.46 m before returning to launch height.
A press/release in one step gives about 0.37 m rise. These figures exclude collider
edge overlap, coyote grace, acceleration from rest and ground-probe tolerance.
Do not use the theoretical maximum as a production gap budget.

| Course segment (left to right) | Edge gap | Landing rise | Purpose |
|---|---:|---:|---|
| Start → first island | 2 m | 0 m | Easy running jump |
| First → second island | 2 m | 1 m | Hold for height |
| Second → narrow island | 4 m | 0 m | Demanding takeoff/landing |
| Narrow → raised island | 3 m | 1 m | Demanding rising jump |
| Raised → lower island | 2 m | -1 m | Readable descent |
| Lower island → finish | 1 m | -1 m | Recovery |

All edges have continuous light upper surfaces and darker solid fronts.
Background columns have lower contrast and render behind the playable plane.
The orange/cream geometric quadruped is a placeholder; moving legs and jump/fall/landing
feedback never change collision dimensions or delay movement.

## Checkpoint and run lifecycle (issue #14)

The movement course is a reusable checkpoint greybox. Two visible checkpoint markers
sit on safe main-route platforms (X=9 and X=34). They activate only when the cat
is grounded beside the marker; progress is forward-only, so going backwards does
not replace a safer recovery point. Spawn positions are fixed safe standing centers,
never arbitrary mid-jump coordinates. A fall below the course automatically retries
at the latest checkpoint, or the starting point if none is active. Manual retry uses
the same path. The controller clears velocity, input buffers and jump grace, and
snaps the camera at every respawn.

On retry and full restart, `RunLifecycle.ResetTransientState` is raised before
teleporting the player. Future combat systems must subscribe to remove active
projectiles and reset each encounter to its pre-attempt state. The current
movement lab has neither projectiles nor an encounter; this event contract is
tested with synthetic projectile/encounter owners instead of pretending that
combat mechanics already exist.

The session-only run state does not modify gameplay saves or records, menu
preferences, or display settings. A full restart clears checkpoint progress,
retry count and the active run clock. No additional powers or pickups are
implemented, so no unapproved ability persistence policy has been chosen.
Future approved abilities must explicitly decide retry retention; do not infer
the rule from the original Java game.

The elapsed clock counts gameplay `Time.deltaTime` across retries and excludes
time spent paused. Pause disables the player's physics and clears held/buffered
inputs; retry exits pause and a full restart returns to an unpaused fresh run.
Returning to the menu destroys the course; re-entering starts a new session.
These are prototype timer and pause rules, not approved campaign scoring or
save-format decisions.

## Verification

The [painted-menu integration review](menu-movement-integration/README.md) records the
combined entry/return flow after merging PR #31. The earlier captures below predate
that menu integration; movement tuning and controls are unchanged.

Behavioral EditMode tests cover short taps, grace expiry, buffered landing, no held auto-jump,
reset, timestep-based horizontal motion, terminal velocity and the full-hold envelope.
Run them only through final **Prepare merge** validation as required by AGENTS.md.
The camera follows the interpolated render transform in LateUpdate with exponential smoothing,
fixed level bounds and immediate snapping on retry. Rigidbody interpolation and continuous
collision detection are enabled; zero-friction contact avoids wall sticking.

Visual review must include motion, small windows and grayscale; still images alone cannot prove
absence of jitter. Final artwork, campaign checkpoint placement, in-game settings and player
playtesting of the proposed difficulty remain separate work.

### Recorded local review (2026-10-23)

- Entered the lab through the existing menu in Unity 6000.3.27f1; inspected idle, run,
  jump, fall and retry. A temporary virtual keyboard fed the actual action bindings for
  the recorded four-jump traversal, ending at X=43.03 on the finish platform.
- [Playthrough recording](references/movement-lab-playthrough.mp4): Game View frames
  captured at approximately 10 fps and encoded with FFmpeg. This documents the route
  and camera follow; its capture rate cannot establish high-refresh smoothness.
- [Start view](../../Assets/Game/VisualEvidence/movement-lab-start.png) and
  [small grayscale view](../../Assets/Game/VisualEvidence/movement-lab-small-grayscale.png).
  The latter uses an approximately 800×624 Game View, exported at 640×480. Grayscale
  colors were applied temporarily in Play Mode and were not saved to the prefab.
- No character/camera discontinuities were observed in the inspected frames, but
  hardware/controller feel and high-refresh jitter still benefit from human playtesting.
