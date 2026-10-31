# Golden River: selected subtle distance

Issue [#50](https://github.com/LeonHorner/LegendOfCat/issues/50).
On 2026-10-30 the user selected Subtle distance, requested removal of the chooser
and asked for animated foreground water. The runtime now has one fixed direction.
Earlier A/B/C and connected-river studies remain archived evidence.

The [curated review archive](archive/README.md) preserves thirteen major stages,
the seven original feedback uploads, and compact versions of the local motion
captures independently of feature-branch history. Its manifest records source
revisions, checksums and known provenance limits.

## Run

The [2026-10-31 review corrections](review-2026-10-31.md) document historical
asset packaging, transformed-course alignment, the mipmap decision and remaining
Editor frame-rate investigation.

The selected scenery also runs in the existing checkpoint prototype: open
`Assets/Game/Scenes/Main.unity`, press Play, then choose **Movement lab (prototype)**
and **Enter movement lab**.
The course now spans 109 world units with fourteen platforms and four checkpoints
at x=9, x=34, x=65 and x=83. The original opening seven platforms are preserved.
Cross a gold marker on safe ground to activate it. R or a fall
retries that checkpoint; Backspace restarts the whole run; P pauses; Esc returns
to the existing menu. The selected art does not replace the checkpoint lifecycle.
The camera stays at y=1 so foreground water remains visible during jumps.
Start/checkpoint recovery coordinates are course-local and are converted using
the current root transform on Retry/Restart, including after runtime placement
changes. Supported transforms are translation and positive uniform scaling;
rotation, mirroring, zero scale and nonuniform scaling are unsupported.
The course's fall boundary is local Y < -7, so valid placements below world Y=-7
do not create a repeated fall/retry loop.
The separate 109-unit art lab below remains available for scenery stress review.
See the [integrated checkpoint evidence](checkpoint-prototype/README.md) for
screenshots, a short recording, lifecycle data and reproduction instructions.
The [extended prototype review](extended-prototype/README.md) records the current
compact overlay, longer route, input-response change and performance limits.
The normal HUD is 350x52 pixels; Controls or pause reveals help, actions and fps.

Resources material assets hold references to the three scenery shaders so player
builds retain them. Regenerate missing materials with
`LegendOfCat/Background study/Prepare runtime materials`.

Use Unity 6000.3.27f2, as pinned in ProjectSettings/ProjectVersion.txt.
Open Assets/Game/Scenes/BackgroundStudy.unity and press Play.
A/D/arrows move, Space jumps, R retries. Stop/restart Play after code changes.
The small help panel has movement instructions and an optional camera tour;
there are no concept buttons or parallax sliders.

The fixed travel factors are sky 0.05, recessed valley 0.14 and distant canopy
0.08. Gameplay cliffs, nearby shore decoration and foreground water remain in
world coordinates. The existing 109-unit route is a lab, not a five-minute
production layout. Begin with its opening approximately 30 units.
Camera framing stays at y=1 and fixed 16:9; Mika, collision and jump physics are
unchanged.

## Selected composition and motion

The background is an independently authored recessed valley with waterfalls
terminating in mist. Its lower forest/land does not connect to the near-water
painting. Grounded pillars/banks and shallow jagged suspended shelves form the
gameplay plane. Independent foreground waves/stones and sparse corner plants
frame the lower view. The rejected connected-river painting remains unloaded.

Water motion uses visible refraction, travelling light streaks and moving foam
inside bright cyan/white water pixels. Warm/dark rocks and plants remain fixed. The original alpha
wave edge stays fixed at cliff feet. World-space phase and fading UV displacement
at tile edges avoid camera-relative swimming and seam jumps. Normal Play advances directly from the shader game clock, without a script
feeding a time property. Explicit capture time is enabled only for deterministic
renders/tests.

This is deliberately restrained movement of the existing painting. It is not a
fluid simulation, animated rock layer, moving collider or new water mechanic.
Terrain and opaque paw support remain registered to the existing collision tops.

While Mika runs on the ground, actual horizontal body velocity smoothly raises
the water activity. At full running speed, accumulated extra flow time gives
approximately 2.2 times the standing flow rate. Additional fine wave crests and
35% stronger refraction make the water denser and livelier. Starting/stopping
blends over approximately 0.4 seconds; accumulated phase is never reset.
The response uses speed magnitude, so both run directions behave alike. Standing,
jumping and a suspended physics body return toward the calm state. This is visual
water activity only; movement and collision are unchanged.

## Current evidence

- [Selected view](subtle-distance.png) and [small grayscale](subtle-distance-small-grayscale.png).
- [Standing water, fixed camera](water-flow.mp4): six seconds at 25 fps.
- [Full running water activity](water-running.mp4): six seconds at 25 fps; an isolated water-state preview, not a player-input recording.
- [Opening route with camera reversal](subtle-animation.mp4): approximately 11.24 seconds at 25 fps.
- [Grounded shore](subtle-ground.png), [extended route](subtle-extended.png), [raised-camera stress](subtle-jump-stress.png).
- [Actual raised ground contact](play-ground-contact.png) and [highest-platform jump](play-jump.png).

Use LegendOfCat > Background study > Capture comparison to regenerate selected
evidence, then pwsh -File tools/export-background-study.ps1 with FFmpeg available.
The menu name is retained for existing workflows; it now captures only the
selected direction. Numbered frames live under ignored Logs/background-study-motion/.
Capture time advances explicitly at 25 fps. These are deterministic renders,
not real-time performance or complete player-input traversal recordings.

## Historical alternatives and selection rationale

| Archived direction | Depth/readability and cost | Evidence |
| --- | --- | --- |
| Stronger distant parallax | More noticeable relative motion; same modular art families | [A still](candidate-0.png), [video](candidate-0.mp4) |
| Quiet distant panorama | Least distant motion, flatter spatial separation | [B still](candidate-1.png), [video](candidate-1.mp4) |
| Subtle distance, selected | Retains atmospheric separation with calmer motion behind landing edges | [C still](candidate-2.png), [video](candidate-2.mp4) |

The [superseded subpixel water clip](water-animation.mp4) is retained as a record
of the motion the user found imperceptible.
These A/B/C captures predate the water animation. The [separated-depth reversal](separated-depth-review.mp4)
and [A/B/C comparison](comparison.gif) preserve the earlier comparison. The old
world-shore-review.mp4 records a rejected connected-world experiment. None of the
old studies is exposed by the current runtime.

The selected approach allows background scenery, gameplay modules and near water
to be reused independently, without a literal shoreline image join. Production
will still need matching-edge variants, occasional unique landmarks and a layout
based on actual movement/challenge pace. No production asset count or time estimate
for a 4–6-minute level has been established.

## Validation and limits

Ten focused Unity tests cover independent foreground/background sources and
depths, fixed 0.14 valley travel, run/stop response in both directions with continuous phase, stable foreground registration through reversal
and recycling, complete distant silhouettes, shallow shelf bounds, collision and
opaque paw contact, highest jumps, shore clearance, route span and fixed 16:9.
With a graphics device the integration test renders two explicit water times:
water pixels must change visibly over 0.4 seconds while upper scenery and warm rock/shore pixels remain unchanged. A second check disables the scene updater and capture clock and verifies motion from the native game clock alone. The pixel check is omitted on a null graphics backend; local validation
uses the actual graphics path.

Motor/jump checks use isolated, explicitly stepped 2D physics with live input
disabled, so headless scheduling cannot shorten a scripted held jump.
Tests and captures run in an ignored isolated project with its own Library.
Source matches and committed whitespace checks are inspected. Logs stay under
ignored Logs/. Recorded renders do not establish frame pacing, measured GPU cost
or full traversal readability. Player/launcher builds and final exact-head merge
validation are reported on the PR separately.

Mirrored panoramas and repeated waves/stone motifs remain prototype limitations.
Grounded bodies stretch vertically and grass is not pixel-flat. The selected
direction is a basis for #16, not final full-level artwork. ASSETS.md and prompt
archives document provenance; the current shader changes introduce no new pixels.

Ownership remains the BackgroundStudy lab, generator, shader, tests, exporter and
documentation. No overlap with open Blender/keepsake work. Existing local scene
and importer edits remain outside this feature's commits. The lab stays excluded
from release scenes.
