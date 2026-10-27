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
Mirrored reuse remains an evaluation technique with visible symmetry limitations
documented in the comparison. Keep gameplay surfaces
in world space and retain the readability requirements above. Full Level 1 content
remains scoped to #16.

## Reference provenance
See ASSETS.md. Images were produced with the built-in OpenAI image generation tool on 2026-10-23, using earlier generated scene references. Exact model ID was not exposed. Human direction selected cat and owl influences, rejected 2.5D, and required clearer gameplay/background separation. Full archived generation prompts are in [reference-prompts.md](reference-prompts.md).

## Historical boss artwork
See the [legacy boss design archive](references/legacy-bosses/README.md) for original sprite sheets, animation metadata and generation records. These references are optional inspiration, not fixed Unity designs.
