# Golden River upper route

## Current playable test candidate — 2026-11-12

The forward enclosed cave candidate is built and open for player review. It now rejoins ascent bank 49, roughly 322 world units beyond entry, with a 60.48-second scripted traversal. See [test-run instructions, current evidence and validation](golden-river-upper-route/cave-test-run-evidence/README.md). The older progress notes and images below describe superseded prototypes. PR #79 stays draft during player review.

## Current replacement: dry descent — 2026-11-10

The elevated garden and its renderer reveal were rejected. The authorized replacement is the [permanent dry riverbank descent](golden-river-upper-route/dry-descent.md). Its entry and fountain references, spatial water contract, current implementation evidence and remaining acceptance supersede the historical sections below. Keep PR #79 draft during art and player review. Earlier passing tests and recordings do not validate the new route.

## Concealed garden prototype - 2026-11-10

The owner authorized implementation and a playable preview after confirming the
subtle entrance and complete separation from the main-route camera. This is the
historical rejected prototype; the exposed shelf chain and open garden illustrations below
are superseded. Actual player review of this revision is pending; keep PR #79 draft.

The branch leaves ascent bank 10 through a single small leftward tread beside an
ordinary ivy-covered wall. Successive leftward climbing treads enter enclosing
masonry and rise beyond the main camera corridor. The court sits at height 13,
with a complete original tree/dry-fountain painting and solid registered floor.
An upper return reverses through broken wall tops and descends to bank 14. There
are no optional checkpoints, rewards or new abilities. The first main checkpoint
remains the water-fall/manual-retry recovery point.

UpperGarden reveals the enclosed passage and return scenery only after Mika
chooses the first tread; the courtyard is not advertised from main-route ground.
Its concealment resets on return/retry. All collision remains physical. Return
treads stay above the nearby main-route jump envelope. The ordinary entrance wall
and first tread are the only route-specific visuals visible while bypassing it.

Scoped CatCamera framing shows the court/tree above its foundation at the same
zoom, then reveals the broad main rejoin before the last descent. Main-route
framing outside those upper-route position/height ranges is preserved. Tests
exercise actual physics traversal, each-shelf falls, checkpoint/retry/restart,
main-route bypass and real-camera exclusion of nearby main-platform contacts.

Preview: build with BuildGame.BuildPreview and launch with
`--preview-view level-one-garden`. This starts at the subtle entrance with the
real checkpoint-1 recovery state. Move left and jump onto the ivy-side tread;
continue left/up to discover the court. Explore its left side to find the upper
return, then travel right/down. R retries checkpoint 1; Backspace restarts the
whole level, P pauses, Escape returns to the existing menu.

New art, source identifiers, exact prompts and registration decisions are in
[garden-assets.md](golden-river-upper-route/garden-assets.md). Source PNGs and
Unity-generated metadata are committed with this implementation. The old CI
results and recordings below do not validate this redesign.

Local validation: Unity 6000.3.27f2/D3D11 passed 30/30 scoped tests, no failures or
skips (UpperRouteTests, MenuMovementIntegrationTests, CatCollisionTests and
RunLifecycleTests). Both whole-level traversal with the garden and whole-level
bypass completed. The new camera test confirms concealment at the entrance,
no main-platform contact inside the climbing/court camera crops, and visibility
of the rejoin before descent. The focused scripted garden run completed in 28.98
seconds of simulated game time; this is not a human timing/difficulty measurement.

Current [normal-speed garden loop](golden-river-upper-route/garden-evidence/garden-loop.mp4)
and color/grayscale 960/640-pixel stills are in garden-evidence. Reviewed the
ordinary-wall entrance, enclosed ascent, complete court/tree framing and visible
rejoin. The local Windows development preview built successfully and its packed
art audit verified 46 paintings plus three shader-reference materials. No current
Linux build, full final CI or player acceptance is claimed for this draft revision.

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

The owner selected the hidden ruin garden on 2026-11-09: enter through a breached
wall into an overgrown, surprising place. The collapsed lookout alternative is
not being pursued. This approves the concept direction; the spatial proposal
and revised gameplay still require review.

### Confirmed concealment requirement

The owner clarified and confirmed that the entrance must be inconspicuous, with
only a small visual hint. After entry, the normal route must disappear completely
from the camera view. The garden itself must not be visible or advertised from
the main route. Return to the main route happens later, after exploring the
separate passage. This supersedes the exposed courtyard-over-main composition
and the earlier proposal to tease the courtyard directly from below.

Use a modest ivy-framed wall crack and a partly visible stone tread as the hint,
without a sign, prompt, arrow, glow or conspicuous doorway. A sheltered climbing
passage then gains enough separation from the main route for its geometry to
leave the actual viewport at the existing zoom. It leads sideways around an
enclosing ruin wall before revealing the garden. The return must be separately
traversable and safe; it must not force a blind drop onto the unseen main route.
Rejoin visibility is restored before the player commits to the final descent.
Verify concealment with actual camera crops, not just decoration covering the
main route in a concept picture. Preserve the optional choice and main recovery.

The [three-view v2 storyboard](golden-river-upper-route/garden-concept-v2-storyboard.png)
shows this progression without a main-route lane under the passage or garden.
See [provenance and limitations](golden-river-upper-route/garden-concept-v2.md).

### First garden proposal (superseded spatial layout)

The first proposal has three beats: climb broken wall treads into a narrow breach,
discover a sheltered courtyard with a living tree embracing a dry stone fountain,
then return through a ruined side arcade and visible descending terraces. A
glimpse of the courtyard invites entry from the main route. The main path remains
independent below. The discovery is environmental, with no collectible, checkpoint
override or new ability. A single painted overview is for concept review only;
it is not collision, camera, gameplay or final-art evidence.

See [garden spatial concept v1](golden-river-upper-route/garden-concept-v1.png).
The image proposes enclosure, the reveal, material palette and return location.
Its nearly continuous terrace is not the final traversal: the playable prototype
must add deliberate short gaps, broken interior treads and a distinct descending
return while preserving visible landings. Do not copy the illustration's lower
route or tiny decorative gaps into the accepted main-route geometry. Exact
artwork scale and screen coverage must be checked against the unchanged camera.

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
