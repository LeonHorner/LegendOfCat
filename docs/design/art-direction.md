# Art direction

## Chosen direction
Charming hand-painted 2D worlds, atmospheric depth and warm adventure. The protagonist uses the natural orange-and-cream quadruped proportions from the first original study: modest head and eyes, graceful limbs and striped tail. The clockwork owl takes the expressive outlined cartoon influence from the second study. Match palette and lighting so characters belong in the painted world. The user rejected the 2.5D study.

## Initial reference archive
These revised concepts were requested for documentation after feedback that the moonlit scene made platforms and background hard to distinguish. They document the initial direction, not fixed layouts or a user-approved final production asset set.

- [Golden River Ruins](references/golden-river-ruins-v2.png)
- [Moonlit Observatory](references/moonlit-observatory-v2.png)
- [Autumn Sky Mills](references/autumn-sky-mills-v2.png)

The same owl appears for comparison, not because three owl encounters are planned. Floating supports, gaps and scale are illustrative. Actual geometry must match implemented movement.

## Readability requirements
- One immediately identifiable playable plane. Standable surfaces have consistent crisp continuous upper edges, clear silhouettes and darker solid fronts.
- Backgrounds use lower contrast, softer edges, less detail and atmospheric separation. Do not rely on hue alone.
- Decorative roots, bridges and ledges must not advertise false routes or appear to bridge gameplay gaps.
- Plants and effects must not hide landing edges, the cat, attack warnings or hazards.
- Bright platform edges should look naturally lit, not like mandatory neon outlines. Night scenes must remain readable without flattening their atmosphere.
- Verify the real game in motion, at small window sizes and in grayscale. Concept review cannot establish collision accuracy or performance.

## Animation
Smooth idle/run/turn/jump/fall/landing transitions with grounded feet and consistent proportions. Do not substitute whole-body stretching for authored motion. Combat separates anticipation, active attack and recovery. Inputs remain immediate; cosmetic animation cannot silently change hit timing. Attack effects remain restrained enough for simultaneous threats to be read.

### Mika animation decision — 2026-10-24

The user rejected the current cutout/IK rig as substantially worse than the earlier
complete-cat artwork: feet/legs are much too thin, assembled anatomy looks unnatural,
and calculated jump poses do not fit a cat. Optical-flow run inbetweens were also
rejected because they distorted the paws and degraded motion. These are discarded
experiments, not production candidates awaiting routine approval. Do not resume
either approach under the current direction without a new user decision.

A 2D skeletal rig is not inherently unsuitable, but requires purpose-designed,
well-overlapping artwork and an appropriate joint structure. These generated pieces
and automatic pose calculations did not meet that standard. Contact/segment-length
tests check mechanical properties; they do not establish visual quality or acceptance.

The agreed next approach is complete-character frame animation:

1. Return to the earlier complete-cat visual reference, preserving natural orange/
   cream quadruped proportions and adequately sized legs/paws. Earlier sprite
   experiments are references, not implicitly approved production animations.
2. Finish one consistently drawn run cycle first, with matching anatomy, volume,
   markings and registration across contact, passing and recovery poses. Do not
   smooth inconsistent drawings with optical flow or whole-body deformation.
3. Present a short looping animation, also slowed down, for user evaluation
   **before gameplay integration**. Frame count, screenshots and passing tests
   cannot substitute for reviewing the loop in motion.
4. After that review, author coherent takeoff, flight/apex, descent and landing
   sequences. Preserve immediate input and collision timing; do not use the
   rejected rig's calculated jump poses as the target.

At rest all four paws should meet the ground. During running, supporting paws stay
planted while swing paws deliberately lift and return; not every paw must touch
the ground in every running or airborne frame. Check sliding, hovering, halos,
changing proportions and artificial stretching.

See [Mika character authoring](mika-animation/README.md) for retained negative
examples and implementation status. No final Mika movement set has been accepted;
issue #10 remains incomplete.

Subsequent loop review on 2026-10-24: the user preferred the original four-frame
run over both added inbetweens and the new sixteen-pose study, and explicitly
authorized putting that four-frame run into the movement prototype. Use original
candidate poses 4–7 without interpolation. This selects the prototype run only;
the remaining movement set still requires review. See the authoring README for
the current course integration and reproduction steps.

The subsequent completion pass preserves that run, removes rejected experiments
from Unity's imported paths, cleans disconnected source alpha and adds four
complete-character flight/landing bridges. Normal/slowed drawing previews precede
integration; the integrated recording and small-scale color/grayscale strips are
in the Mika authoring folder. Airborne poses share a shoulder anchor; supporting
poses use visible sole anchors. No new idle/fish behavior or traversal ability is
introduced. Current code, provenance and final validation are tracked by PR #45;
the dated rejected experiments above remain historical warnings, not live tooling.

## Level 1 background evaluation — 2026-10-24

Issue #50's [isolated background comparison lab](background-study/README.md)
compares three-layer parallax, a unified panorama and subtler three-layer motion
over the same short movement segment. Candidate factors are adjustable prototype
values. Recommendation: consider subtle parallax with occasional unique landmarks;
**user selection is pending**. No final background production direction is approved.
The initial broad strip crossfades were rejected in Unity feedback on 2026-10-25
for blurry transitions and insufficient lower scenery. The revised lab enforces
the existing 16:9 viewport, lowers camera framing, and uses independent transparent
middle/near layers with their painted contours instead of height-based fades.
Further user feedback requested at least double the review length, foreground
corner plants and water/small stones, varied substantial platforms with some
floating jump rocks, visible top surfaces and differently hanging greenery. The
lab now spans 109 world units, with ten grounded cliff fronts, four suspended rock
platforms, receding painted meadow tops, varied ivy/moss/fern patches and a closer
river layer. These are painted depth cues within the 2D test; they do not change
the movement/collision contract or approve 3D gameplay or final Level 1 artwork.
On 2026-10-27 the user rejected the high water, exaggerated slab-like surface
perspective and Mika's front-rim stance, requesting a unified composition closer
to the concept. The follow-up lowers the river, registers it with the platforms,
quiets its saturation, narrows the grass bands around the unchanged contact line
and reduces side shading/texture repetition. This supersedes the exaggerated top
faces as the current prototype target; final visual acceptance is still pending.
Further feedback identified the near and far water as separate-looking graphics.
The lab now uses one continuous river/bank painting for both distances, with the
same UVs and movement throughout. Registered foreground copies of that same image draw after near-bank vegetation and overlap subtly flared cliff feet; the old foreground river and middle-distance
water painting are superseded. No second water texture/scrolling strip is used.
The course camera stays vertically fixed during jumps to retain foreground water. These corrections address the subsequent floating-cliff screenshot. Mirrored reuse remains an evaluation technique with visible symmetry limitations
documented in the comparison. Keep gameplay surfaces
in world space and retain the readability requirements above. Full Level 1 content
remains scoped to #16.

## Reference provenance
See ASSETS.md. Images were produced with the built-in OpenAI image generation tool on 2026-10-23, using earlier generated scene references. Exact model ID was not exposed. Human direction selected cat and owl influences, rejected 2.5D, and required clearer gameplay/background separation. Full archived generation prompts are in [reference-prompts.md](reference-prompts.md).

## Historical boss artwork
See the [legacy boss design archive](references/legacy-bosses/README.md) for original sprite sheets, animation metadata and generation records. These references are optional inspiration, not fixed Unity designs.

### Terrain system correction — 2026-10-28

The user rejected the latest tall-wall preview, especially its right platform,
as too far from Golden River Ruins. The lab now replaces repeated surface
textures and separate caps/leaves with complete painted pillar and wide-bank
silhouettes. Contact registration is independent of decorative flowers and the
submerged body; collision geometry is unchanged. See background-study/README.md
and terrain-module-prompts.md for evidence, generation and limitations. This
supersedes the textured-wall approach; it does not establish visual acceptance.

### Platform and river integration — 2026-10-28

Further user feedback identified the platforms as visually detached from scenery.
The terrain shader now reduces isolated yellow saturation, shares a restrained
sage atmospheric tint with the river banks, and gives lower submerged rocks a
cool damp tone. The foreground-water mask has a narrower irregular contact edge
and sits slightly lower, exposing more of the authored boulder feet instead of
cutting them with a soft horizontal band. Landing silhouettes stay crisp.
These are shader-only composition refinements; source artwork is unmodified.
Visual coherence remains under user review.

### Right bank and paw contact — 2026-10-28

User feedback identified an abrupt right bank connection and a slight apparent
hover. Low boulder/fern buttresses now join selected grounded formations to their
surroundings below the playable plane. Painted contact UVs follow actual central
grass rows rather than upper leaf highlights. Mika's presentation and collision
geometry are unchanged. The actual raised-platform paw baseline is checked in
Play Mode and recorded in background-study/play-ground-contact.png. See ASSETS.md
and terrain-module-prompts.md for the new shore source. Visual review is pending.

### World-space shore experiment — 2026-10-29

The user rejected the moving background's contact with grounded platforms, clipped
trees, and compressed standing-pillar artwork on suspended platforms. After
discussing retained distant parallax and reuse for a five-minute level, the user
asked to continue the bounded experiment.

Connected riverbanks/water, terrain and nearby shore buttresses now belong to one
world-space assembly. Sky and an independent complete distant canopy retain
parallax. No nearby tree strip moves through ground contact. Floating supports
use shallow jagged shelf artwork instead of boulder-foot pillars; their collision
tops and jump heights remain unchanged. The opening approximately 30-unit segment
is the review focus; the existing long route is retained for reuse stress.

This authorizes a prototype of the revised composition, not final production
acceptance, a full five-minute layout, or final A/B/C parameters. See
background-study/README.md, world-shore-prompts.md and ASSETS.md. The 2026-10-27/28
all-depths-moving records above describe superseded experiments.

### Separate foreground/background test — 2026-10-29

The user rejected the world-space connected-river variant as unsuitable, proposed
separating foreground from background, and explicitly authorized a test. The
current lab replaces the connected-river artwork/pass with a recessed valley
background and an independently authored foreground-water painting. Gameplay
cliffs and their shore decoration remain world-space; distant waterfalls end
inside mist rather than continuing into the nearby waves. Background presets
control only independent scenery. No source/UV/geometry join is required between
the two waters. The earlier connected-river records above are superseded.

The short opening-section composition is the review target. This is authorization
to test the separated approach, not final visuals or production selection.
See background-study/README.md, separated-depth-prompts.md and ASSETS.md.

### Selected subtle distance and water motion — 2026-10-30

The user selected Subtle distance as the fixed direction, requested removal of
the chooser, and asked for animated foreground water. This resolves the preset
selection gate for #50. The separated background/gameplay/foreground composition
remains the selected prototype basis; final production assets/layout remain #16.

Travel factors are fixed: sky 0.05, recessed valley 0.14 and distant canopy 0.08.
The foreground water and gameplay scenery stay world-space. The A/B/C chooser
and tuning sliders are removed. Earlier comparison captures remain historical
evidence, not alternative runtime settings.

Foreground water now has subtle procedural refraction and moving light modulation.
Only bright cyan water colors participate; warm/dark stones and plants retain
their positions, and the authored alpha shoreline is unchanged. Animation uses
world-space phase to remain stable through camera reversals and tile recycling.
No new source pixels or generated art were introduced. See background-study/
README.md for current evidence and validation. Selecting the direction does not
establish measured production performance or a complete five-minute level.

### Visible water-flow correction — 2026-10-30

The user reported no visible water movement. The previous refraction was below
one pixel in the small view and left bright foam largely stationary. The shader
now refracts cyan water and neutral foam more clearly and adds travelling light
streaks. Original warm/dark rock colors and authored alpha stay fixed. Normal
Play uses the shader game clock directly; explicit time is a separate opt-in
capture override, preventing stale scripted time from freezing normal animation.

Graphics regression checks now require a perceptible average change over 0.4
seconds and verify animation with the scene updater disabled and no capture-time
override. This supersedes the earlier barely visible shimmer, retaining the
selected subtle-distance factors and separated composition.

### Water response while running — 2026-10-30

The user requested faster animation and more waves while running. Actual grounded
horizontal body velocity now smoothly increases water activity in either
direction. At full run speed, extra integrated flow time adds 1.2 to the native
clock rate (approximately 2.2 times total), with denser fine crests and 35% stronger
water refraction. Starting/stopping blends over approximately 0.4 seconds without
resetting phase. Standing or leaving the ground returns toward calm water.
A body removed from physics simulation cannot trigger running activity.

The fixed Subtle distance factors, cliff/stone positions, alpha waterline and
gameplay physics are unchanged. Tests exercise actual motor movement in both
directions, slowing back to calm water and monotonic flow phase.

### Checkpoint prototype scenery integration - 2026-10-30

The user requested merging current main into the feature branch and applying the
selected graphics to the existing checkpoint prototype. Main cb524b0 was already
an ancestor of the branch; the normal merge reported already up to date.
The menu's MovementCourse now reuses the fixed subtle-distance scenery, painted
terrain and animated foreground water. Its seven platforms, collision spans,
two checkpoints, player animation and run lifecycle remain unchanged. A shared
scenery attachment follows the existing course camera and player rather than
creating duplicate gameplay objects. The camera holds the selected lower framing.
This integrates prototype presentation, not a finished five-minute Level 1.
See background-study/README.md for entry points, evidence and reproduction.

### Longer course, compact overlay and early takeoff feedback - 2026-10-30

The user requested a longer playable prototype and a smaller, quieter overlay,
then reported about 26 fps in the Editor and perceived delay especially in Mika's
jump. The course now spans 109 units with fourteen platforms and checkpoints at
x=9/34/65/83. Original opening geometry and the existing run lifecycle are retained.
The normal overlay is a small translucent strip; Controls/pause reveals detail.

Grounded input now exposes the existing takeoff state/drawing before the next
physics step. The body is not moved or simulated by the callback; the next fixed
step follows the existing trajectory and release rules. No new animation artwork
or movement ability is introduced. Airborne coyote/buffering remains fixed-step.
Water geometry omits empty source space without changing its authored mapping or
animation. Measurements do not establish an FPS gain or explain the user's low
Editor frame rate. See background-study/extended-prototype/README.md for evidence,
regressions and limits. The earlier seven-platform integration record is historical.

### Water-entry review prototype — 2026-10-31

For #60 the user requested a first playable version after discussing splash,
face-focused zoom, sinking head and a short fade back to the last checkpoint.
Four new complete-character reaction drawings are confined to water recovery;
existing locomotion artwork is retained. This explicit integration request permits
reviewing the bounded effect in-game. It does not accept the reaction artwork or
replace the existing Mika animation review decisions. See
[water-recovery review](water-recovery/README.md) and ASSETS.md for provenance,
playback, implementation limits and outstanding review.
