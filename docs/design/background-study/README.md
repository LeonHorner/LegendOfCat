# Golden River separated-depth decision lab

Issue [#50](https://github.com/LeonHorner/LegendOfCat/issues/50).
This is a bounded prototype. The user authorized testing separated background,
gameplay scenery and foreground; final art and preset selection remain pending.

## Run and review

Use Unity 6000.3.27f1. Open Assets/Game/Scenes/BackgroundStudy.unity and press Play.
A/D/arrows move, Space jumps, R retries. Stop/restart Play after script changes.
Begin with the opening approximately 30 units: grounded bank, raised pillar,
two shallow suspended shelves and another grounded formation. Move both ways.
The existing 109-unit course remains for reuse stress, not a five-minute layout.

The camera follows horizontally at y=1 with fixed 16:9 framing. The optional
camera tour travels 100 units out and back at 5 units/s; disable it to follow Mika.
Physics, collision heights, Mika and jump timing are unchanged.

## Current composition — 2026-10-29

The user rejected the connected-river experiment, including its world-locked
variant, and explicitly asked to test separated foreground and background.

| Area | Artwork | Motion and purpose |
| --- | --- | --- |
| Background | Sky, distant canopy and recessed waterfall valley | Independent parallax; waterfalls disappear in distant mist |
| Gameplay | Painted cliffs, shallow suspended shelves and nearby shore buttresses | Fixed world coordinates with the colliders |
| Foreground | Independent nearby water/stone painting and sparse corner plants | Water remains stable at world-space cliff feet; corner plants frame the camera |

The distant valley contains no foreground river plane. Its lower edge is shadowed
forest/land, separating it spatially from the near water. The front water uses a
different source, material, tile width, scale and alpha silhouette. There is no
copied river pass, shared UV registration or claimed continuation from waterfalls
to foreground waves. The rejected connected-river painting is not loaded.

The reused foreground-river source was previously rejected when it was expected
to join another river painting. The new experiment deliberately removes that
expectation. Its authored waves form their own upper edge; no artificial global
waterline clips a duplicate background image. Warm grading reduces its original
cyan saturation. The old StudyRiver shader remains archived and unused.

Grounded platforms retain integrated pillar/bank paintings and low decorative
shore buttresses. The buttresses belong to the playable world, not to moving
background banks. Floating supports retain the shallow jagged shelf source,
0.9-unit body depth and at most 0.35 units of crown space. Grounded bodies extend
below the front water. No decorative scenery adds collision.

Grass registration and the authored paw pivot remain aligned to collider contact.
The exporter uses the pivot rather than padded Sprite bounds. Opaque support
pixels are checked in actual Play Mode after the landing animation settles.

## Background alternatives

| Preset | Sky | Recessed valley | Far canopy | Foreground water |
| --- | --- | --- | --- | --- |
| A: Distant parallax | 0.08 adjustable | 0.28 adjustable | Half the valley factor | 1 fixed |
| B: Still distance | 0 | 0 | 0 | 1 fixed |
| C: Subtle distance | 0.05 | 0.14 | 0.08 | 1 fixed |

Travel is relative to the camera: 0 stays on screen, 1 stays in world coordinates.
Sliders affect A's background only. Foreground and terrain cannot be dragged by
background tuning. C remains a recommendation for review, not a selected preset.

## Evidence and reproduction

Use LegendOfCat > Background study > Capture comparison with a graphics device,
then pwsh -File tools/export-background-study.ps1 with FFmpeg available.
Intermediate frames stay under ignored Logs/background-study-motion/. Existing
manual images and the rejected world-shore-review.mp4 are retained separately.

[Separated-depth opening review](separated-depth-review.mp4): camera x=6 to x=34
and back, 57 frames at 5 fps, approximately 11.4 seconds. This is the primary short
composition comparison. It is a camera render, not a player-input recording.

| Evidence | A | B | C |
| --- | --- | --- | --- |
| Color | [1280x720](candidate-0.png) | [1280x720](candidate-1.png) | [1280x720](candidate-2.png) |
| Small grayscale | [640x360](candidate-0-small-grayscale.png) | [640x360](candidate-1-small-grayscale.png) | [640x360](candidate-2-small-grayscale.png) |
| Long camera sweep/reversal | [MP4](candidate-0.mp4) | [MP4](candidate-1.mp4) | [MP4](candidate-2.mp4) |
| Foreground and cliff feet | [PNG](river-contact-0.png) | [PNG](river-contact-1.png) | [PNG](river-contact-2.png) |

![A/B/C, same terrain and foreground](comparison.gif)

[Ground contact](play-ground-contact.png) and [highest-platform jump](play-jump.png)
use the actual motor/presentation. The EditMode harness explicitly ticks camera
LateUpdate after physics steps. Jump-framing images are raised-camera stress
views, not runtime jump recordings. Extended-route and wrap images cover long
and negative travel. Videos are sampled at 5 fps and do not measure real-time
frame pacing or complete traversal readability.

## Reuse, limitations and effort

Background panoramas and foreground water can now be authored/reused independently
of the platform layout. They need matching style and atmospheric separation,
rather than a literal cross-depth shoreline join. Platform/shore modules still
need coherent shapes and unambiguous landings. This test does not establish a
five-minute layout, asset count or production schedule under #16.

Mirrored scenery repeats ruins/tree clusters; foreground waves/stones also repeat.
Grounded rock bodies stretch vertically and painted grass is not pixel-flat.
The bright near water and large distant ruins remain composition choices for
user review. These technical changes do not establish visual acceptance.
Production still needs authored variants, unique landmarks and player traversal
review at small size and in motion.

## Validation and coordination

Ten focused Unity tests cover distinct foreground/background sources and draw
depths, absence of the rejected connected river, independently moving valley,
stable world-point foreground UVs through reversal/recycling and all presets,
complete distant silhouette headroom, shallow shelf body/source, collision
stability, opaque paw support, highest-platform jump, shore clearance, route span,
tile coverage and fixed 16:9 at four window shapes.

Tests/renders use an ignored isolated project under artifacts/ with its own
Library while the working Editor is open. Source matches are verified; logs remain
under ignored Logs/. No Windows/Linux player/launcher builds, install/update
smoke test, measured GPU timing or full player traversal claim is made.

Ownership: BackgroundStudy runtime, Editor generator, prototype art/tests,
isolated scene, capture exporter and documentation. No overlap with PR #51's
Blender work or #53's keepsake plan. #50 has no native blockers; #16 is blocked by
#50. Existing local scene/importer edits are preserved. The lab stays excluded
from release scenes.

ASSETS.md, separated-depth-prompts.md and earlier prompt archives record provenance.
Keep PR #54 draft until visual review and preset selection; current-main
integration and exact-head full merge validation follow that gate.
