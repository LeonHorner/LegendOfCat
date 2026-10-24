# Golden River background decision lab

Issue [#50](https://github.com/LeonHorner/LegendOfCat/issues/50). Bounded prototype;
production selection is pending user review. No final Level 1 direction is approved.

## Coordination and scope

Ownership: the isolated BackgroundStudy runtime, shader, Editor generator, tests,
its Resources/BackgroundStudy atlas, scene and this evidence directory. No overlap
with PR #51's Blender authoring. Existing Mika prefabs, movement motor, shared
course, menu, packages and project settings are consumed without changes.
Native blocked-by relationships were read: #50 has none, and #16 includes #50.
Base: main d01d470, including the merged Mika presentation from PR #45.

## Inspect and run

Use Unity 6000.3.27f1. Open `Assets/Game/Scenes/BackgroundStudy.unity` and press
Play. Alternatively use **LegendOfCat > Background study > Create isolated scene**.
A/D/arrows move, Space jumps, R retries. The original movement course geometry is
reused as a short comparison route, not an approved Level 1 layout. The camera
follows the interpolated cat transform with smoothing and viewport-aware bounds.
Use the on-screen buttons to switch candidates without moving the camera or cat.
Camera-only tour travels at 5 units/s and reverses; turn it off to follow Mika.

| Candidate | Far / middle / near travel | Depth and readability | Variety and effort |
| --- | --- | --- | --- |
| A: Three layers | 0.08 / 0.28 / 0.55 | Strongest separation; near scenery moves noticeably. | Three authored layer families; reusable scenery needs asymmetric variants and occasional unique landmarks. |
| B: Panorama | 0.08 / 0.08 / 0.08 | All scenery travels together; calm painted vista with no relative depth. | Cheapest composition; recognizable towers stay on screen longer, but extended travel needs additional panoramas. |
| C: Subtle layers | 0.05 / 0.14 / 0.24 | Less motion distraction while retaining relative depth. | Same art cost as A; longer time before recognizable scenery repeats. |

Values mean scenery travel relative to camera: 0 stays on screen, 1 stays in
world coordinates. They are prototype tuning, not approved production settings.
Sliders affect A; B/C are fixed comparisons. All collision geometry remains in
world coordinates. No scenery has colliders. No asset suggests a bridge across a
gameplay gap; crisp lit tops and dark solid platform fronts identify landing edges.

## Reuse and limitations

The new generated atlas reinterprets the archived reference into three panoramic
strips; the original full scene is never used as a tiling layer. Runtime UV windows
and soft vertical alpha fades composite the strips. Their generated skies overlap:
these are prototype strips, not final clean transparent production source layers.
Alternating mirrored tiles meet with identical edge pixels; three 64-unit tiles per
depth cover the view. Absolute camera offsets prevent reversal drift. Mirroring
removes color discontinuities but can reveal symmetric tree/tower silhouettes;
it is a technical reuse demonstration, not acceptable variety for a full 4–6-minute
level. Production should use deliberately matching painted edges and multiple
modules, with unique landmarks placed in world space behind the gameplay plane.

Nine unlit quads share one 1536x1024 uncompressed texture (approximately 6 MiB RGBA
before platform overhead) and three materials. Transparent overlap adds overdraw;
no measured player GPU timings or platform performance claim is made.

## Evidence and decision

Capture reproducibly with **LegendOfCat > Background study > Capture comparison**
(requires a graphics device). Candidate images use the same camera, cat pose and
platforms. Small grayscale captures are 640x360 with luminance conversion.
Motion sequences use the same 5-unit/s camera sweep, reverse and return; they are
camera/scenery recordings, not recordings of player input. Wrap stress stills
inspect negative travel, the near-layer wrap boundary and long travel.

Remaining review: inspect captured images and motion, record observed defects and
recommendation, then present A/B/C to the user. Keep the PR draft and issue open
until the user chooses and that choice is recorded in art-direction.md. Full
merge validation is deferred while this acceptance criterion remains unfinished.
