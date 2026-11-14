# Golden River main route: integration notes

Start with the concise [level design guide](level-design-guide.md) for reusable lessons and a task-based map of the detailed references. The older revisions below are history, not the current route specification.

The owner accepted a complete playthrough on 2026-11-08. No extra human-input recording is required; PR #61 integrates later independently. See the current acceptance and cleanup notes in [Finale](golden-river-main-route/finale.md). Final merge readiness still requires the normal exact-head CI gates.

Issue #71 is in progress. These notes describe the starting point and the work
needed to turn it into Level 1; they are not evidence of a finished route.

The requested final 120–150-second section is described in
[The crown of the river ruins](golden-river-main-route/finale.md). It follows
checkpoint 2 and relocates the provisional exit. Its first implementation adds
obstacles, mandatory reversals and vertical/diagonal supports; player acceptance
remains pending. Current main was merged normally on 2026-11-05;
integration details and remaining water-recovery overlap are recorded there.

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

The introduction pacing review used elapsed session time through the first
checkpoint, including retries and excluding pauses. The gameplay debug panel
that displayed this timing was removed in #84. Press P or the controller's
right bumper to open the pause menu. Retry returns Mika to the active
checkpoint, and Restart returns Mika to the level start.

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

## Variety correction after the 61-second playtest - 2026-11-01

The user reported 61 seconds for the expanded introduction but found it very
boring, specifically requesting platforms at different heights, more different
sizes, distinct floating-platform graphics and significant unique background
locations. That rejection supersedes the flat introduction as the design target.

The correction keeps the same checkpoint/exit anchors and approximate horizontal
extent, but replaces the flat repetition with 51 platforms across the preview:
29 grounded banks/steps and 22 suspended supports (12 rooted islands, ten masonry
shelves). Contact heights range from -0.25 to 1.75; widths range from 3.5 to 24.
A gentle first rise introduces height independently before short ascending and
descending groups. Broad banks between groups provide braking/recovery space.
Start and both checkpoint banks retain their safe unchanged contacts.

Three new background paintings appear once each: a watermill near x=84, broken
aqueduct near x=216 and ancient willow near x=342. Each has one absolute anchor
with local parallax, separate from the recycled distant layers. Returning or
retrying cannot create another copy. The recurring valley architecture recedes
with a smooth atmospheric tint while a landmark is nearby. Background landmarks
have no collision and never draw over the gameplay plane. The existing moving
foreground river is preserved.

This user-requested art addition is a bounded route-variety prototype in #71;
it does not claim final completion of scenery issue #73. These new assets and
their generation prompts are recorded in ASSETS.md and variety-prompts.md. No
serialized scene/prefab or existing metadata is hand-edited; Unity generates new
asset metadata on import. The new drawings still require in-game user review.

### Variety verification and review images

Unity 6000.3.27f2: **27/27 scoped tests passed**, zero failures/skips
(MenuMovementIntegrationTests, RunLifecycleTests, CatCollisionTests,
SceneryRendererTests). The complete actual-physics route remains reachable without
retry; uninterrupted scripted travel remains 60.40 seconds to checkpoint 1 and
72.20 seconds to the current exit. These measurements do not establish enjoyment,
first-player learning time or the final 4-6-minute course goal.

`GoldenRiverReview.Record` rendered four real runtime course locations at 960x540
and 640x360, with grayscale checks at 640x360. Inspected suspended grass contacts,
different shelf silhouettes, landmark presence behind the player/platforms and
small-image separation. These are static authoring renders with numerical water
time, not live player-input screenshots or motion capture; no new in-motion
acceptance is claimed.

- [First suspended steps](golden-river-main-route/variety-evidence/suspended-steps-960.png)
- [Watermill](golden-river-main-route/variety-evidence/watermill-960.png)
- [Aqueduct](golden-river-main-route/variety-evidence/aqueduct-960.png)
- [Willow, small grayscale](golden-river-main-route/variety-evidence/willow-640-gray.png)

The three landmark paintings are unique within this course, but the original
far valley/sky still recycle. Repeated underlying castle silhouettes remain a
known limitation for the later whole-level scenery pass. Final visual acceptance,
player review of challenge/fun, full-route player-input capture, remaining-section
pacing and PR #61 integration are pending. New art is covered by the existing
packed-art validator; Windows/Linux final builds and packaging remain separate.

The revised Windows development preview build succeeded, including the packed-art
checks for all five new paintings. Local output:
`Builds/GoldenRiverVarietyPreview/LegendOfCatPreview.exe --preview-view level-one`.
Earlier preview folders contain superseded layouts. Linux/player-launcher packages,
final exact-head merge validation and final user acceptance remain unrun/pending.

## Composition review correction - 2026-11-01

The user reported **60.7 seconds** for the varied introduction and supplied four
in-game crops. Feedback: the mill appeared pasted into the scene; connected
floating sequences mixed incompatible styles; mill/aqueduct boundaries looked
cut out; the willow was too bright. This is a timing observation and rejection
of those visual details, not final route/scenery acceptance.

Landmark rendering now retains the source paintings' partial alpha rather than
applying the crisp vegetation-cutout threshold to their atmospheric boundaries.
Existing sky/valley/canopy/foreground cutout policy is unchanged. Per-landmark
paint brightness and reduced pale fog mixing give the willow a darker, less
washed-out appearance and bring the mill/aqueduct into the surrounding palette.
No source image pixels, collision surfaces, jump rules or landmark anchors change.

Connected suspended sequences now use one family throughout: rooted islands in
the natural-bank sections, masonry throughout the ruined crossing. There are
still 22 suspended supports, now 14 root islands and eight masonry shelves.
Style changes occur at grounded section boundaries rather than alternating on
every jump. The visual review utility includes side-boundary views of the mill
and aqueduct and a view of a whole masonry sequence, in addition to center views.

### Composition correction verification

**28/28 scoped tests passed** on Unity 6000.3.27f2 with Direct3D 11, zero
failures/skips. The new GPU regression verifies that partial-alpha landmark
pixels survive, brightness grading darkens them, and original cutout behavior
remains selectable. The test explicitly skips on a Null graphics device rather
than claiming a software-rendered shader result. Sequence checks require adjacent
suspended supports to retain one family. Whole-route physics/lifecycle regressions
continue to pass.

Updated center, side-boundary and sequence renders were inspected at 960x540 and
640x360, including small grayscale views. Authored mist now softens the exposed
mill/aqueduct boundaries instead of leaving the former jagged cutoff; the willow
is visibly darker. These are static rendering checks, not final user acceptance:

- [Mill boundary](golden-river-main-route/variety-evidence/watermill-edge-960.png)
- [Aqueduct boundary](golden-river-main-route/variety-evidence/aqueduct-edge-960.png)
- [Consistent masonry sequence](golden-river-main-route/variety-evidence/masonry-sequence-960.png)
- [Darker willow](golden-river-main-route/variety-evidence/willow-960.png)

The Windows development preview build for this correction succeeded, including
packed-art checks. Local output:
`Builds/GoldenRiverCompositionPreview/LegendOfCatPreview.exe --preview-view level-one`.
The previous variety preview is superseded. No final Linux/player-launcher
packaging, merge-ready validation or release was performed.

## Lower crossings and landmark-base review - 2026-11-01

The user circled residual detached painted mist/ground at the mill's lower
boundary, asked to address the same behavior on the other background paintings,
and requested that suspended platforms can also lie closer to the water and
below grounded platforms.

All three landmark paintings now share a local artwork-space alpha transition at
their bases and outer sides, preserving partial-alpha contours while dissolving
the isolated painted ground skirt into the existing distant landscape. This
transition travels with each landmark under parallax. Recycled valley/canopy/sky
layers, world-space foreground river and source PNG pixels are unchanged.

The masonry crossing at x=202/210.5 now has contact heights -0.75/-0.25 below its
initial bank, and the rooted sequence at x=295/302.5/309/315.5 descends through
-0.5/-1/-0.75/-0.25 before returning to the zero-height grounded bank. These use
the same widths/gaps and grouped artwork families. Checkpoint and exit locations
are unchanged. The contact-height range is now -1..1.75. The lowest roots remain
above the visible river; no swimming mechanic or new recovery behavior is added.

Additional review views target the mill's lower base and both low crossings.
The GPU regression checks the local base/side transition while retaining central
painted opacity; geometry assertions require a genuinely low suspended platform
and a descent below a grounded entrance. Actual full-route simulation checks that
the descent and return remain possible using the existing movement rules.

### Lower-crossing verification

Unity 6000.3.27f2 with Direct3D 11: **28/28 scoped tests passed**, no failures or
skips, after lowering the crossings and adding the local base transition.
The full actual-physics route succeeds without retry, including both descents
and returns to grounded banks. The new lower-contact/family checks and GPU
base/side blending regression pass. A test-local closure lost across Play Mode
reload was replaced with explicit iteration before the successful rerun.

The inspected PR #61 source uses a local water-entry sole height of -3.25;
the new lowest standable contact is -1. This static interface check is not a
claim that the still-unmerged recovery implementation has been integrated or
playtested here.

The static Unity review now covers ten course locations at 960x540 and 640x360,
with grayscale versions of the smaller renders. Inspected the new lower
background-base transition and both low platform families. Landings remain above
the foreground water and distinct from the distant scenery in these static views;
actual gameplay review remains required.

- [Mill lower base](golden-river-main-route/variety-evidence/watermill-base-960.png)
- [Low rooted crossing](golden-river-main-route/variety-evidence/low-root-crossing-960.png)
- [Low masonry crossing](golden-river-main-route/variety-evidence/low-masonry-crossing-960.png)

The updated Windows development preview build succeeded, including packed-art
checks. Local output is
`Builds/GoldenRiverLowCrossingPreview/LegendOfCatPreview.exe --preview-view level-one`.
Final Windows/Linux player/launcher packaging, merge-ready validation, integration
of PR #61 and user acceptance remain pending; no release was performed.

## Supported landmark hills - 2026-11-01

The user rejected the most recent opacity blend as looking stranger and suggested
an actual background ground plane which runs downhill out of the picture without
an abrupt or blurred boundary. This explicitly replaces the preceding partial-
alpha/mist/base-feather experiments as the active composition direction.

All three landmarks now have painted supporting hills in the same source image
and parallax layer. The image includes the building/tree foundation, continuous
sloping grass/earth/embedded rock and solid ground to the lower canvas edge.
The final composition was zoomed out after the initial hill edits still had
high side contours. Runtime quads are 30x20 course units centered at y=-0.5;
the solid lower edge lies at y=-10.5, well below the fixed camera. Hillside sides
descend toward the foreground river instead of ending as exposed floating cutouts.
No screen-height or object-base opacity fade is used. The standard authored-cutout
edge treatment rejects generated glow outside the grass/architecture silhouettes.
Existing per-landmark brightness/fog grading is retained.

Original isolated cutouts are archived with unchanged GUIDs under
`Assets/Game/Art/GoldenRiverArchive`; the active Resources paintings use the
`-hill-v2` suffix. The packed-art validator requires the new hills and rejects
archive content. ASSETS.md and hill-prompts.md record source, editing calls,
model disclosure, license caveats and human direction. No old .meta file or
serialized scene/prefab was hand-edited.

Gameplay, lower suspended supports, motor/jump rules, checkpoints, exit and the
independent world-space foreground river are unchanged. The new asset regressions
check solid source bottoms and descending side coverage; runtime tests check hill
texture binding, off-camera bottom placement and reversible landmark registration.
The GPU test checks that ground opacity survives without feathering and that
brightness grading/source-halo rejection still work.

### Supported-hill verification

Unity 6000.3.27f2 with Direct3D 11: **29/29 scoped tests passed**, zero
failures/skips. New source-art checks confirm continuous opaque ground (allowing
the PNG's antialiased outermost border) and side contours below the main visible
area. Runtime checks confirm hill texture bindings, off-camera lower canvas and
reversible placement. The GPU regression confirms solid ground opacity without
base feathering, brightness grading and generated-halo rejection. Whole-course
actual-physics, checkpoints, timer, camera, pause, retry and restart tests pass.

The three archived original PNGs and their .meta files were compared to their
previous Git blobs: all six are byte-for-byte unchanged after the Unity move.
Static runtime renders at 960x540 and 640x360, plus small grayscale versions,
were updated for all ten review locations. Inspected the mill base, hill exits,
aqueduct foundations and willow ground. The painted hills remain continuous
through the platform gaps and descend beneath the independent foreground river;
no manufactured alpha fade or lower source-image cutoff is exposed in those
views. These are static authoring renders, not user acceptance or a full
player-input capture.

- [Mill on supporting ground](golden-river-main-route/variety-evidence/watermill-base-960.png)
- [Aqueduct ridge](golden-river-main-route/variety-evidence/aqueduct-960.png)
- [Willow hillside](golden-river-main-route/variety-evidence/willow-960.png)
- [Mill hill exit](golden-river-main-route/variety-evidence/watermill-edge-640.png)

The supported-hill Windows development preview build succeeded, including packed-
art checks for all active paintings and exclusion of the archived cutouts. Local
output: `Builds/GoldenRiverHillPreview/LegendOfCatPreview.exe --preview-view level-one`.
Earlier preview folders are superseded. Final Linux/player-launcher packaging,
exact-head merge validation, PR #61 integration and user acceptance remain pending;
no release was performed.

## Continuous scenery and matching platform shapes - 2026-11-02

The user reported obvious repetition in the scenery immediately ahead of the
clouds, mismatch between some suspended and grounded platforms, and excessive
reuse of scaled copies. The inspected valley and canopy sources both contained
recognizable ruins and were each mirrored/recycled. Golden River now uses one
new continuous valley painting and one continuous canopy painting, with distinct
once-only ruins and no duplicated panels. The user clarified that ruins should
be varied rather than removed, and explicitly requested a larger ruin too. The existing .14/.08 distance factors are
preserved; sky .05 and foreground water are unchanged. Movement Lab keeps its
existing selected rendering for comparison.

The valley strip is 104 course units wide, the canopy strip 80, covering current
start-to-exit parallax travel without mirroring or wrapping. Both retain their
native 3:1 source aspect. Cropping lower content shifts it beneath the viewport;
no broad crossfades or warped landmark strips are introduced. These cover the
current -5..507 course extent; later course expansion needs further authored
landscape rather than automatically stretching or recycling these images.

Three newly drawn grounded forms are selected for narrow (<10), medium (10..<18)
and broad (>=18) collision spans. Their rock, limestone, grass and light match the
suspended-root reference. New compact root and compact masonry drawings have
stout distinct silhouettes, while longer spans retain the existing long versions.
Seven terrain sources now supply the route: three grounded, two rooted and two
masonry. Each connected suspended sequence retains one material family. The old
independent shore-foot ornaments are omitted for this course because their
previous yellow foliage/rock style no longer fits the new continuous bank art.
New terrain is not mirrored. Contact coordinates, low crossings, collision widths,
Mika/motor/jump rules, checkpoints, exit and supported landmark hills are unchanged.

The new GPU-imported assets are listed in BuildScenery.GoldenRiverArt and included
in packed-art validation. ASSETS.md and continuity-prompts.md record original
sources, built-in generation/edit calls, model disclosure and license policy.
Runtime tests check seven distinct terrain bindings, single positive-scale
panorama panels, native aspect and complete viewport coverage at multiple route
positions and on reversal.

### Four authorized one-off easter eggs

The user authorized all four proposed finds and asked to distribute them across
sections, rather than putting everything in the introduction. Further feedback
required physically plausible sleeping-cat support, drifting fish and a larger
unique ruin. Their integrated locations/behavior are documented below.

## Unique ruins and one-off easter eggs - 2026-11-02

The user clarified that the ruins were liked; the correction is unique varied
ruins without repeated copies, not removal of architecture. The valley panorama
contains a left broken arch/pier group, one larger asymmetric ruined keep and a
right circular-window remnant. The canopy has one different unequal-tower ruin
partly hidden among the right grove. Each source is still rendered once without
mirroring/wrapping over the current course. The castle/cat statue registration
uses the valley painting's coordinate frame and preserves the selected .14 travel.

Four decorative finds are instantiated once per course, with no collision,
interaction, collectible, save state, UI notification or gameplay prerequisite:

| Find | Placement | Behavior |
| --- | --- | --- |
| Stone cat king | On a right battlement of the larger distant keep, around the middle of the introduction | Small fixed statue, registered to the unique valley panorama. |
| Two watching fish | River gap at x=96 by the mill | Two independently phased heads drift/bob/tilt, lower bodies occluded by the existing foreground river; pause freezes drift. |
| "No cats" noticeboard | x=440, after checkpoint 1 | Sleeping cat rests on a broad timber rain roof supported by two stout posts; original thin-edge sign was rejected and is unused. |
| Cat-shaped rock | x=499 near the exit | Small matching stone embedded into the bank front, discoverable by its subtle ear/eye-like cracks. |

The fish use two UV regions of one original pair painting, keeping their native
proportions but allowing independent bob/tilt phases. The pair's bounded horizontal
drift stays inside the river gap. They render behind the existing foreground water,
not on a second painted water strip. No new swimming/water-recovery system is added.
Retries/restarts do not instantiate another set; scenic phase is session-only.
The sign and cat rock remain in the later sections when sizing the remaining route.
Future layout changes must move these anchors with their intended sections.

`GoldenRiverReview.Record` includes a numerical-time five-second fish detail
sequence under Logs/watching-fish-frames (12 fps) for encoding. This is an actual
Unity rendering/animation review, explicitly a detail zoom and scripted water clock,
not a player-input or normal-framing capture. Center/entry/exit and egg locations
are also captured at the normal fixed gameplay framing.

The watching fish now lose ground gradually to the current and recover with short, staggered swimming strokes every 2.4 seconds. Each turns its painted head silhouette and inclines toward Mika within nine horizontal units (eleven units of total distance); outside that range both attend upstream. The existing painting has no independently animated pupils. Pause freezes the animation. Attention, stroke recovery and pause are covered by the Easter egg integration test.

2026-11-02 local preview validation: 30/30 scoped EditMode tests passed after fish attention/stroke changes. Windows development preview built successfully at Builds/GoldenRiverDiscoveryPreview/LegendOfCatPreview.exe; packed-art validation confirmed 25 active paintings. This does not replace the remaining exact-head Windows/Linux merge gates or player-input playtest.

The user rejected the separate stone-king cutout as visually disconnected. It is now painted directly into the unique central castle panorama; its runtime object is only a non-rendering review anchor. This guarantees shared depth, filtering, fog and physical masonry support. The original cutout is excluded from player resources.

The latest user review supersedes the pale limestone and flat-mountain variants: broad banks now reuse the original painted bank, other bank forms derive directly from it, and the valley restores forested cliffs and waterfalls. River tiles alternate the original boulder painting with an authored flat slab/split-stone/pebble variant, selected by absolute tile index so returning does not reshuffle them. The round blue/gold fish now swim horizontally below the surface with turquoise attenuation and translucent water over their bodies. Their nearby-Mika reaction uses whole-head silhouette turning, not independent pupils.

Latest original-style revision: 30/30 scoped Unity EditMode tests passed in Temp/restored-style-tests-final.xml. Eighteen normal-framing positions were captured at 960/640 widths, with grayscale companions, plus a five-second fish detail GIF. These are scripted scenery reviews, not full player-input playtest evidence.

The Windows development preview at Builds/GoldenRiverPaintedPreview/LegendOfCatPreview.exe built successfully and passed packed-art auditing (25 active paintings; archive excluded). Full Windows/Linux player/launcher gates remain pending while PR #77 is a draft.

### Nonrepeating river stones and supported statue refinement
The user still recognized the alternating stone tiles, so stones are now removed from the repeated wave layer. Six unique atlas cells are rendered once at x=22,78,163,273,394,487, independently of water tile recycling. They have no colliders and do not reshuffle on camera movement or retry. The water mesh now retains headroom through v=0.5, preventing a new painting's visible upper alpha from being cut into a rectangular bar. The statue was refined from a close-up study, masked and composited in PhotoCraft onto the existing parapet; the rest of the panorama was retained. Review captures include all six rock anchors and both sides of a water-tile boundary.

Validation for this correction: 30/30 scoped Unity EditMode tests passed (Temp/water-statue-tests.xml). Both the lab and route water paintings have maximum alpha 1 above the retained v=0.5 mesh crop, so no visible paint is clipped at that upper edge. A pixel comparison against the previous committed panorama found zero visible changes outside the statue edit area. PhotoCraft used a fresh local MCP connection to the existing 127.0.0.1 bridge; no Codex configuration or credentials were changed.

The final PhotoCraft statue layer was lowered two source pixels so the paws overlap the parapet rather than leaving a visible gap. The non-rendering review anchor follows the final composited statue position. Its contact was inspected in the actual Unity render.

Windows development preview built successfully at Builds/GoldenRiverWaterPreview/LegendOfCatPreview.exe; packed-art validation confirmed 26 active paintings and excluded archived art. Full Windows/Linux production and launcher checks remain pending for the draft feature.

### Boundary and river-motion review — 2026-11-02
Golden River camera limits now include twelve units of scenery beyond both terminal collision edges (-5 and 507), while the Movement Lab keeps its original limits. The continuous panoramas cover the resulting camera positions. No standable platforms or checkpoint/exit locations were changed. A camera integration test checks that both cliff edges remain inside the viewport instead of being hidden at its border.
The river no longer reacts to Mika's movement speed or grounded state: removed run-driven clock acceleration, refraction gain and extra foam pulses, and reduced periodic highlight strength. A fixed-time pixel comparison checks that running/stopping leaves the rendered lower river unchanged. Fish quads are 0.95 units with higher underwater visibility and slightly shallower position; nearby-Mika reactions, drift/strokes and pause behavior remain. The sleeping-cat sign uses the original painted platform style rather than fine photographic texture. PhotoCraft removes residual pixels of the previous statue behind the replacement forelegs.
2026-11-03. The fish retain their paint colors and reveal ribbons of the moving water through their bodies. Twelve individually painted river formations each occupy a fixed route location. The standalone cat rock was removed from the exit. The statue uses a higher resolution composite on the original parapet.
2026-11-03. Eighteen unique river-rock formations now occupy fixed positions. The latest six add gravel, ledges, cobbles and angular groups at a shallower depth. The statue silhouette uses the panorama behind its front leg and ear after stray sky pixels were removed.
2026-11-03. The watermill, willow and aqueduct use near, middle and far parallax speeds with matching atmospheric color. The mill spokes turn inside the existing painted wheel and the outer willow leaves sway. The new distant panorama uses a wooded ridge, open valley and lower hills across the three regions.
The near valley keeps both panorama boundaries anchored while shifting the keep away from the aqueduct view. The three landmark views and the statue were checked in the Unity capture. All 31 scoped EditMode tests passed and the Windows development preview built successfully.

## Ascent revision — 2026-11-04
The former short continuation is replaced by the ascent described in [ascent notes](golden-river-main-route/ascent.md). Earlier 51-platform geometry, exit/checkpoint coordinates and whole-route timing are historical. Intro geometry is retained. The 110–140-second ascent and 4–6-minute whole-level targets still need real player evidence.
