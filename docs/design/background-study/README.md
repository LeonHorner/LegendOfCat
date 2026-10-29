# Golden River world-shore decision lab

Issue [#50](https://github.com/LeonHorner/LegendOfCat/issues/50).
This remains a bounded art/technical experiment. Final visuals, production
selection and A/B/C parameters are pending user review.

## Run and review

Use Unity 6000.3.27f1. Open Assets/Game/Scenes/BackgroundStudy.unity and press Play.
A/D/arrows move, Space jumps, R retries. Stop/restart Play after code changes.

Start with the opening approximately 30 world units: a grounded bank, raised
pillar, two shallow suspended shelves and another grounded formation. Move both
ways and watch the shore contact. The existing 109-unit course is retained for
reuse/wrap stress; this is not a five-minute production layout.

Fixed 16:9 framing follows horizontally at y=1. Collider tops, gaps, Mika and the
motor are unchanged. The optional camera tour still covers 100 units out and back
at 5 units/s. Turn it off to follow Mika.

## Structural correction — 2026-10-29

The user rejected the all-depths-moving prototype: shoreline slipped under grounded
platforms, nearby trees exposed cut borders, and suspended platforms resembled
short standing pillars. Those are composition/asset problems, not just color
grading problems. The previous near-tree layer and moving shore contact are
superseded.

The revised assembly has three depth families:

| Family | Content | Relationship to platforms |
| --- | --- | --- |
| Sky | Painted sky strip | Very slow independent parallax |
| Distant landscape | Complete tree/tower silhouettes and mountains | Independent parallax; no physical joins to playable ground |
| Connected world | Riverbanks, water, grounded formations, nearby shore buttresses and suspended supports | Fixed world coordinates; no relative contact drift |

The independent distant landscape replaces near-cutout entirely. Its complete
upper silhouettes have transparent headroom; the lower edge stays below the
normal view. Trees that visibly belong to the immediate riverbanks are part of
the world-space river painting and cannot slide relative to the platforms.
Camera-relative corner plants remain sparse framing decoration without apparent
terrain joins.

The river still uses one connected painting, with three background tiles and
three registered foreground copies. Both copies share texture, UVs, tint, scale
and world-space phase. The low foreground mask overlaps submerged rock feet.
Recycling the mirrored tiles changes allocation, not the river pixels at a fixed
world position. Vertical registration is fixed too, including raised-camera
stress views. No independent foreground-water painting is introduced.

Floating platforms use a new shallow jagged shelf with short hanging greenery,
not compressed pillar art or broad standing boulder feet. Their body depth is
0.9 units, plus at most 0.35 units for flowers/grass above the contact row. The
visible opaque silhouette is smaller because source margins remain transparent.
Their collider heights are unchanged; this revision makes their bodies thinner,
not the jumps higher or lower.

Grounded formations retain integrated pillar/bank paintings. The painted grass
rows and Mika's authored paw pivot stay aligned to existing collision contacts.
The static exporter uses that pivot rather than padded Sprite bounds. Actual
opaque support pixels are also checked in Play Mode. Five low decorative shore
buttresses have no collider and stay below y=-1.3.

## Retained parallax comparison

| Preset | Sky travel | Distant landscape travel | Connected world travel | Tradeoff |
| --- | --- | --- | --- | --- |
| A: Distant parallax | 0.08 adjustable | 0.28 adjustable | 1 fixed | Stronger depth behind stable ground |
| B: Still distance | 0 | 0 | 1 fixed | Quiet screen-locked distant panorama |
| C: Subtle distance | 0.05 | 0.14 | 1 fixed | Calmer depth behind stable ground |

Travel is relative to the camera: 0 stays on screen, 1 stays in world space.
The A sliders affect only distant scenery. No slider can make connected terrain,
banks or water drift. Recommendation for review remains C, but no final preset
is chosen.

## Evidence and reproduction

Use LegendOfCat > Background study > Capture comparison with a graphics device,
then pwsh -File tools/export-background-study.ps1 with FFmpeg available.
Intermediate images are under ignored Logs/background-study-motion/. Existing
manual captures in this documentation folder are preserved.

[Opening shore/shelf review with reversal](world-shore-review.mp4): camera x=6
to x=34 and back, sampled at 5 fps (57 frames, approximately 11.4 seconds).
This is the primary bounded composition check.

| Evidence | A | B | C |
| --- | --- | --- | --- |
| Color | [1280x720](candidate-0.png) | [1280x720](candidate-1.png) | [1280x720](candidate-2.png) |
| Small grayscale | [640x360](candidate-0-small-grayscale.png) | [640x360](candidate-1-small-grayscale.png) | [640x360](candidate-2-small-grayscale.png) |
| Long camera stress sweep | [MP4](candidate-0.mp4) | [MP4](candidate-1.mp4) | [MP4](candidate-2.mp4) |
| Grounded shore | [PNG](river-contact-0.png) | [PNG](river-contact-1.png) | [PNG](river-contact-2.png) |

![A/B/C, same fixed world and camera sweep](comparison.gif)

[Actual raised ground contact](play-ground-contact.png) and
[highest-platform jump](play-jump.png) exercise the existing motor/presentation.
EditMode scheduling explicitly ticks camera LateUpdate after physics steps.
Jump-framing images are raised-camera stress stills, not runtime jump captures.
Extended-course and wrap images cover other route portions and long/negative
travel. Videos are deterministic camera renders, not real-time performance
recordings or complete player traversal evidence.

## Reuse, effort and limitations

For a 4–6-minute level, distant panorama modules can be reused behind authored
world-space terrain modules. Connected shore transitions must be designed once
per module family; they no longer need compensation for relative parallax.
This prototype does not establish an asset count, production time estimate or
measured five-minute route. Gameplay pace, stops, vertical travel and challenges
will determine the layout and unique landmark needs under #16.

The paintings still reveal mirrored clusters and repeated ruins. Grounded bodies
stretch vertically and painted grass is not pixel-flat. Suspended art is currently
one source. World-space riverbanks show more local repetition than distant
parallax would; production needs matching-edge variants and occasional unique
landmarks. Foliage/terrain palette and detailed composition remain under review.
Changing the architecture does not itself establish concept-art fidelity.

## Validation and coordination

Nine focused Unity tests cover actual opaque paw contact, highest-platform jumps,
world-space collision stability, route span, shallow shelf art/body bounds,
decorative shore clearance, module contact rows, no accidental 3D colliders,
fixed world-point river UVs through camera movement/reversal/tile recycling and
all presets, matching front/back water, distant silhouette headroom, tile coverage
and fixed 16:9 at four window shapes.

Tests and renders use an ignored isolated Unity project under artifacts/ with its
own Library while the working Editor is open. Source matches are verified. Logs
stay under ignored Logs/. No player/launcher builds, install/update smoke test,
GPU timings or full traversal claim is made. Transparent scenery and river
overlap need target-device profiling.

Ownership: BackgroundStudy code/shaders, Editor generator, prototype art, tests,
isolated scene, capture exporter and documentation. No overlap with open PR #51's
Blender work or #53's keepsake plan. Native relationships checked: #50 has no
blockers, and #16 is blocked by #50. Existing local scene/importer edits are
preserved. The lab is excluded from release build settings.

ASSETS.md, world-shore-prompts.md and earlier prompt archives document provenance.
Keep PR #54 draft until visual review and preset selection; current-main
integration and exact-head full merge validation follow that gate. No merge or
release is authorized.
