# Golden River upper route

## Player review - 2026-11-09

The owner played the preview and rejected this first implementation as very
boring and unspectacular. The geometry/physics checks below remain technical
evidence only; they do not establish an accepted exploration experience.
PR #79 is back in draft and #72 remains incomplete.

The rejected layout adds seven largely repetitive floating shelves and a reused
gate without a distinctive spatial experience or a compelling discovery. Its
short extra traversal is not sufficient motivation to leave the main route.

Revision direction: author a recognizable place with an entrance that teases its
interior, varied traversal through that place, a discovery revealed by exploration,
and a distinct return path. Preserve known movement, visible landings, safe falls
and the accepted main route. Do not solve the feedback by lengthening the same
shelf chain or adding an unapproved collectible or ability.

Two bounded concepts are being offered for owner direction: an overgrown garden
entered through a broken ruin wall, or an exterior/interior climb through a
collapsed lookout tower. Neither is approved production content yet. Review the
chosen spatial proposal before authoring a new artwork set or replacing gameplay.

## First implementation (rejected)

Issue #72 owns the optional platforms and lookout, scoped course/scenery attachment,
behavior tests and review tooling. No overlapping active PR owns these platforms.
The accepted #71 main geometry, checkpoint banks, finale and Movement Lab retain
their ownership. PR #61 later reconciles its water-recovery attachment in
MovementCourse; this route reuses the existing fall callback without adding one.
No serialized scenes, prefabs, packages or project settings are authored here.

The bounded loop leaves ascent bank 10 and rejoins bank 14, well before checkpoint
2 and the finale. The first low shelf is visible over the broad entry bank; a
short leftward climb opens onto an elevated rightward crossing. The lower route
stays available. There are no optional checkpoints, rewards or ability gates.

The discovery is an abandoned arch lookout above the valley, initially composed
from existing painted masonry and gate artwork. This is placeholder composition
for #73, not final scenery approval. Falls can land on the main route or its
existing catch terrace; water falls use the last main-route recovery checkpoint.

## Layout and observations

Seven solid painted shelves leave the early ascent's broad bank 10 and rejoin
bank 14. The original provisional rejoin at the much later checkpoint-2 overlook
is intentionally shortened: this is a bounded detour, not a second entire ascent.
The first shelf invites a short leftward jump, followed by a rightward climb.
The first three rises are 1.6 units with 2.5-unit landing spans; adjacent main
landings are 3.5-10 units wide with rises at most one unit. This is a geometric
comparison, not a measured claim about subjective difficulty.

The first stacked layout interrupted takeoff with an overhead underside in real
collision traversal. The revised staircase separates those takeoff columns.
Main-route geometry, both checkpoint banks, movers, camera and exit are unchanged.

Unity 6000.3.27f2 with D3D11 passed the optional collision/lifecycle regression and
the complete beginning-to-exit traversal with the detour and both checkpoints.
The existing full-main-route regression also passed with no retries. Coverage
includes each optional landing, falls from every shelf, unchanged checkpoint 1,
manual retry, pause and full restart. These are actual physics/input simulations,
not teleported reachability assertions or human playtests.

The [normal-speed loop](golden-river-upper-route/optional-loop.mp4) is a complete
7.84-second scripted input traversal from entry to rejoin, sampled every four
50 Hz physics ticks and encoded at 12.5 fps. No velocity writes or teleports occur
during traversal. This timing excludes choosing/learning the detour and is not a
first-player completion time or performance measurement.

The [complete beginning-to-exit run](golden-river-upper-route/whole-level.mp4)
includes the upper loop, both main checkpoints and the accepted finale, without
a retry. It covers 334.12 seconds of simulated game time at normal playback speed.
This is a deterministic input recording, not a human 4-6 minute pacing result.
Inspected entry/leftward choice, upper crossing, descent/rejoin and exit frames
from this continuous recording as well as the focused loop. The
[grayscale loop](golden-river-upper-route/optional-loop-gray.mp4) provides the same
small-window motion without hue cues. No new timing or FPS target is inferred.

Reviewed 960x540/640x360 color stills and 640x360 grayscale: crisp shelf tops,
solid darker fronts, a visible first shelf beside broad ground, an arch grounded
on the lookout, and the broad rejoin below the final shelf. The next descending
landing remains in view. Backgrounds remain the existing unfinished #73 baseline;
the reused arch is placeholder discovery art.

- [Entrance](golden-river-upper-route/upper-entrance-640.png)
- [Lookout in grayscale](golden-river-upper-route/upper-lookout-640-gray.png)
- [Descent and broad rejoin](golden-river-upper-route/upper-descent-640.png)

## Reproduction

Use the pinned Editor with a graphics device. A null-graphics first import rejects
the existing 14332-pixel panorama on an 8192-limited null device; reimport it with
`GoldenRiverReview.ReimportPanoramas` under D3D11 if needed. No importer settings
or source art changes are required.

Run EditMode `UpperRouteTests` and the existing `MenuMovementIntegrationTests`.
Capture with `-executeMethod GoldenRiverReview.Record -upperReview -reviewOutput
<output-directory>`; add `-fullUpperRun` for the full beginning-to-exit recording.
Encode `motion/frame-%05d.png` at 12.5 fps. The capture uses the same Editor-only
input pilots as the collision tests and the actual runtime camera/scenery.

Final exact-head CI/platform/package results are recorded in the PR. No merge,
release, human difficulty acceptance or PR #61 water-animation integration is
claimed here.
