# Golden River background decision lab

Issue [#50](https://github.com/LeonHorner/LegendOfCat/issues/50).
This is an isolated review prototype. Production direction and A/B/C selection
remain pending user review.

## Run and scope

Open Assets/Game/Scenes/BackgroundStudy.unity with Unity 6000.3.27f1 and press
Play. A/D/arrows move, Space jumps, R retries. Stop and restart Play after script
changes. The route spans 109 world units, with ten grounded platforms and four
stationary suspended jump rocks. The original collision layout and Mika motor
remain unchanged. Fixed 16:9 framing follows horizontally at y=1, retaining
foreground water during the highest jump. The optional camera tour moves at
5 units/s, covering 100 units out and back in 40 seconds.

Ownership is limited to BackgroundStudy code, shaders, generator, isolated scene,
prototype art, tests and evidence. No overlap with PR #51's Blender sources.
Native relationships checked: #50 has none; #16 is blocked by #50. The working
base includes main d01d470 and PR #45. The lab is excluded from release scenes.

## Terrain system replacement — 2026-10-28

The user rejected the previous preview as far from the concept, especially the
right platform. Repeating stone textures on tall polygon walls, separate narrow
grass caps and attached foliage patches did not reproduce the concept's cohesive
painted formations. That terrain system is superseded.

Each platform now renders one complete original painting with its own alpha
silhouette: a compact pillar or a broad riverbank module. Large asymmetric rocks,
grass lips, flowers and hanging ivy belong to the same painting. The broad bank
is used for spans of six units and above; smaller spans use the pillar. Some
instances are mirrored. No rock-face UV repetition, separate slab cap or separate
platform-leaf overlay remains.

The mesh has a horizontal registration row at the real collider contact. Pixels
above that row occupy only 0.6 world units, so flowers do not become tall obstacles
when the rock extends farther down. The lower portion stretches independently
to the water; grounded bounds extend to y=-4.5, while suspended rocks retain
compact undersides. Painted turf spans are registered to collider widths.
Alpha clipping rejects the generated pillar's low-opacity outer haze without
editing its original pixels. Decorative protrusions have no collision.

This is a structural improvement under review, not a claim of matching or
acceptance. Two paintings still repeat, floating instances compress the pillar
vertically, and the nominal painted contact edge is not pixel-flat. More authored
variants and local contact refinement may be required. The river's underlying
mirrored banks and near vegetation also remain visible style/reuse limitations.

## Connected water and framing

One river-and-bank painting contains waterfalls, distant water and near stones.
Three background quads and three exactly registered foreground copies use the
same texture, UVs, tint, scale and motion. The foreground mask below y=-3.2 draws
after near-bank vegetation and overlaps the rock feet. There is no separate
foreground-water painting or scrolling factor. Corner plants frame the view.
The river and all scenery are decorative and have no colliders.

The old middle-cutout, foreground-river, meadow, terrain texture and hanging-leaf
sources remain archived; the superseded terrain sources are not loaded.

## Concepts

| Candidate | Far / middle / near travel | Review tradeoff |
| --- | --- | --- |
| A: Three layers | 0.08 / 0.28 / 0.55 | Strong depth; nearest vegetation draws more attention. Adjustable sliders. |
| B: Panorama | 0.08 / 0.08 / 0.08 | Quiet unified motion; less spatial separation. |
| C: Subtle layers | 0.05 / 0.14 / 0.24 | Calmer motion with some separation. |

Travel is relative to the camera: 0 stays on screen, 1 stays in world space.
Recommendation remains C for review, with authored variants and unique landmarks
before production. No candidate or parameter set is approved yet.

## Evidence and reproduction

Use LegendOfCat > Background study > Capture comparison with a graphics device,
then run pwsh -File tools/export-background-study.ps1 with FFmpeg available.
Intermediate frames live under ignored Logs/background-study-motion/. Existing
manual frames in this documentation folder are preserved.

| Evidence | A | B | C |
| --- | --- | --- | --- |
| Color | [1280x720](candidate-0.png) | [1280x720](candidate-1.png) | [1280x720](candidate-2.png) |
| Small grayscale | [640x360](candidate-0-small-grayscale.png) | [640x360](candidate-1-small-grayscale.png) | [640x360](candidate-2-small-grayscale.png) |
| Camera sweep | [MP4](candidate-0.mp4) | [MP4](candidate-1.mp4) | [MP4](candidate-2.mp4) |
| River contact | [PNG](river-contact-0.png) | [PNG](river-contact-1.png) | [PNG](river-contact-2.png) |

![Same camera sweep, A/B/C](comparison.gif)

[Highest-platform jump](play-jump.png) uses the actual motor and animation in Play
Mode; the EditMode test explicitly ticks camera LateUpdate after physics steps.
Jump-framing stills are raised-camera stress views, not runtime jump recordings.
Extended-course and wrap stills cover other portions and long/negative travel.
Motion videos are deterministic 5 fps camera renders, not real-time performance
captures or recordings of complete player traversal.

## Validation and remaining work

Eight focused Unity tests check ground contact and highest-platform jumps,
world-space collision stability through concept switches, route span, painted
landing registration, complete nonrepeating module UVs, submerged grounded bounds,
absence of accidental 3D colliders, river source/depth/registration, mirrored tile
coverage and reversal, and fixed 16:9 at four window shapes.

Tests and graphics captures run in an ignored isolated project under artifacts/
with its own Library while the working Editor is open. Logs remain under ignored
Logs/. No player/launcher builds, install/update smoke tests, target GPU timings
or full traversal claim is made. Alpha-tested terrain adds another two source
textures and fourteen module draws; actual performance needs measurement.

ASSETS.md and terrain-module-prompts.md record source, tool, license and creative
decisions. User visual review and A/B/C selection remain open. Keep PR #54 draft;
full exact-head merge validation follows completion of the decision gate.
