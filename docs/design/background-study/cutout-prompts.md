# Transparent replacement layers — 2026-10-25

Built-in OpenAI image generation, transparent output requested. Exact model ID not
exposed. Both generations reference our Golden River Ruins v2 archive for style
and palette only. Project assets: `middle-cutout.png` and `near-cutout.png` under
`Assets/Game/Art/BackgroundStudyArchive/`. Original generated RGBA files are copied
without pixel edits. Runtime alpha treatment rejects the generated faint halo.

## Middle layer prompt

Use case: stylized-concept. Game background asset for warm hand-painted 2D Golden
River Path. Reference is palette and painterly style only. Generate ONE wide
1536x1024 RGBA panorama of a DISTANT riverside forest and a few small recessed
ruined towers on rounded hills. It is a middle-distance parallax layer. Entire
SKY ABOVE THE IRREGULAR TREE/HILL SILHOUETTE must be genuinely TRANSPARENT. No sky
paint, no clouds, no sun, no distant mountain background. Upper 40% of canvas
entirely transparent; treetops and ruins occupy middle 30%, bottom 30% is painted
blue-sage river and wooded riverbanks reaching the bottom edge. Keep landscape low
contrast, softly painted but with intact opaque silhouette boundaries, NOT
blurred, NOT semi-transparent haze, no broad alpha fade, no ghosting. Subtle cool
muted atmospheric blues and sage complement warm golden light. No foreground,
characters, platforms, walkable ledges, bridges, text, border or watermark.
Composition broad and horizontal, low-detail river at left and right edge matching
heights. This is an independent transparent layer to place over a separately
painted sky. Paint bounded tree and roof contours, solid-opacity scenery with a
real alpha cutout outside.

## Near layer prompt

Use case: stylized-concept. ONE 1536x1024 transparent RGBA game scenery asset: a
broad low irregular riverside strip of small sage/ochre shrubs, rounded stones
and a few graceful willow saplings, hand-painted storybook 2D in reference palette.
This is a NEAR BACKGROUND parallax cutout, not a full scene. Entire upper 65%
canvas empty transparent, vegetation confined to lower 35% with a few saplings
reaching half-height. Large transparent open spaces between plant groups. No
painted sky, mountains, distant forests, castles, scenery backdrop or river plane;
only the isolated shrubs, saplings and rocks. No fog, blur, glow, halos, translucent
veil or gradient fade: opaque painted shapes end at clearly defined irregular
leaf/branch silhouettes and genuine transparent alpha outside. Muted
lower-contrast sage/ochre colors, warm light, soft brush texture within clearly
bounded forms. No foreground platforms, flat horizontal banks, bridges,
characters, animals, text, logos or borders. Keep bottom low bushes continuous at
left and right matching heights for mirrored reuse, with open viewing gaps above.
Reference is style and palette only; DO NOT copy the full-scene layout.
Independent original painterly cutout.
