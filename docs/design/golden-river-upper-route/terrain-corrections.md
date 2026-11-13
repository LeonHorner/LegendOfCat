# Terrain consistency corrections — 2026-11-13

Owner screenshots rejected the previous shoreline review candidate: the fissure backdrop covered the main route, source images met at vertical cuts, recovery supports faded into nothing, an old catch shelf remained at the exit, and broad exit paintings were visibly enlarged. Passing physics checks did not establish visual acceptance.

## Changes

- The embedded fissure backdrop is geometrically bounded below -4.45, behind the riverbed and cave roof. It no longer extends to +2 above the main route. A regression assertion checks its actual world-space renderer bounds. Visibility is permanent; there is no camera/player-driven enable switch.
- The entrance floor has a fixed four-unit overlapping native-material join before the fountain. Its nearer depth is explicit so the join does not depend on equal-depth draw ordering. The opposite end of the discovery floor joins the shared foundation material before the return.
- Adjacent return foundations and rescue-tunnel undersides use one registered world-space stone material instead of differently cropped slab pictures. The underside rim spans to the next bank and has a shallow irregular silhouette.
- Recovery supports become one connected descending rock staircase rooted into the left cliff, plus three attached overhangs for the main path and final return. Paint and solid collision use the same outlines. The lower stair contacts retain their existing heights and right edges; the upper return keeps a clear passage underneath. Independent posts and their transparency fade are removed rather than disguising them with background columns.
- The old main-route catch at ascent index 35 is replaced by the permanent outer exit terrain, eliminating the remaining isolated clipped grass strip. Other main-route contacts/checkpoints remain.
- Exit rock color uses a new material-only generated stone source at a fixed 12-by-8-world-unit frame. The original large paintings still supply the native outer silhouette, but no longer determine the oversized rock/grass detail. Existing `bank-narrow-v3` grass is sampled at its normal small terrain scale. A 4096-sample floating-point contact profile registers it to each actual tread without stretching a complete painting across 65 units.
- The first detail trial using an entire landmark cliff as a repeating texture was rejected locally for an obvious repeated stair-wall pattern. It was replaced with an opaque material-only source without plants, platforms or scenery features.

## Asset provenance

`Assets/Game/Resources/GoldenRiver/Descent/terrain-stone-v1.png` is original built-in OpenAI image_gen artwork, model not disclosed, generated on 2026-11-13. The exact prompt, style-only reference, output filename, service terms and creative decisions are in [terrain-stone-assets.json](terrain-stone-assets.json) and ASSETS.md. The main-route grass is an existing original project asset with unchanged provenance. No external artwork, package or persistent format is added.

## Verification and limits

The actual-physics capture now includes four additional targeted poses: main route above the chamber, entrance floor overlap, return-bank join, and main route above the exit. These are checked alongside the previous entrance, fountain, hidden catch, recovery, outer mouth and rejoin poses in color at 960x540/640x360 and grayscale. Continuous forward/reverse recordings remain separate from deliberately placed review poses.

See [current correction evidence](terrain-correction-evidence/README.md) for executed checks. Earlier `permanent-shore-evidence` is retained as the owner-rejected candidate, not current approval. Entry/shore composition and final art acceptance remain under owner review; this correction does not establish the complete level as polished or satisfy the human-duration/performance gates. PR #79 remains draft. No merge or release is authorized.
