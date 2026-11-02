# Continuous distance and platform-family artwork - 2026-11-02

Built-in OpenAI image generation tool; exact model identifier not exposed. Two separate generation calls for the panoramas, five separate edit calls referencing existing project shelf drawings for the terrain. All calls set transparent_background=true. Generated source pixels/alpha are preserved. Human direction: remove conspicuous repeated scenery just in front of the clouds, match suspended and grounded materials, and author genuinely different platform shapes rather than only scaling the same image. The subsequent ruin restoration and authorized Easter egg asset requests are recorded in ruin-egg-prompts.md; those panorama edits supersede the no-architecture constraint in this initial pass. Easter eggs are separate single instances, not baked duplicates in the panorama.

## valley-panorama-v3.png

New generation; no reference image.

Use case: stylized-concept. A SINGLE NONREPEATING very wide 3:1 transparent-alpha background panorama for a 2D Golden River platformer. Native landscape composition with small distant landscape details. UPPER 65 PERCENT of canvas completely transparent empty space, LOWER 35 PERCENT contains one continuous receding valley silhouette, soft low-contrast blue-gray cliffs, muted sage wooded slopes and distant peach/lavender peaks. Left to right tells one varied landscape: low wooded foothills, a broad quiet saddle, one winding recessed ravine, small separated escarpments, then lower wooded ridges. Change silhouette, tree group sizes and spacing across the width. No symmetry, no mirrored halves, no repeating motifs, NO castle or towers, no architecture, no large waterfalls, no foreground water, no playable-looking ledges. Lower edge solid receding terrain to bottom of image; natural transparent upper outline with at least 65% vertical headroom. Warm painted gouache fantasy landscape, atmospheric golden-hour light, restrained contrast. Render high-resolution extra-wide 3:1, request 3072x1024 if available. This is a complete panoramic painting, not a sprite sheet or separate panels.

## canopy-panorama-v3.png

New generation; no reference image.

Use case: stylized-concept. A SINGLE NONREPEATING extra-wide 3:1 transparent-alpha distant forest panorama for a hand-painted 2D Golden River landscape. UPPER 70 PERCENT of canvas entirely transparent; LOWER 30 PERCENT contains one continuous low silhouette of small distant trees and wooded rolling hills, no tall central tree. Left side small mixed rounded deciduous clumps, middle more open spacing with a shallow rounded ridge, right side sparse slender pines and a final small grove. Deliberately unequal asymmetric grouping, no symmetry or mirrored/repeated patterns, NO buildings, castles, ruins or towers. No clouds or painted sky, no water, no foreground ledges or characters, no text. Muted desaturated blue-sage leaves and lavender-blue shadows, soft low-contrast painterly gouache with subtle warm left light. Solid forest/hills to lower canvas border, natural transparent silhouette above, large true transparent headroom. High-resolution single panoramic painting 3:1, ideally3072x1024, not multiple panels.

## bank-narrow-v3.png

Edit reference: Assets/Game/Resources/GoldenRiver/shelf-root.png.

Use case: precise-object-edit. Turn this floating-root reference into a NEW narrow grounded rock bank sprite, a genuinely different upright boulder formation, preserving its material/style palette. Square 1:1 canvas. A standable short flat grass top at 30 percent down the image, load-bearing edge from x=6 to94%. Several large interlocked rounded blue-gray rocks descend to the bottom border with thick grounded mass and patches of the SAME pale limestone. One distinctive diagonal split in rock face, small moss patches, short sparse roots down the side. No floating pointed underside, no supports gap under the bank: solid rock extends beyond image bottom. Empty transparent headroom above grass and transparent space outside natural side outline. Avoid a repeated masonry wall and avoid a flat rectangular slab. Preserve the reference's painterly natural slate-blue rock with warm pale limestone patches, muted sage moss and short golden-green grass, upper-left warm daylight, cool dark rock fronts, crisp organic silhouette. No glowing yellow trim, no flowers, no logos, no text, no characters. Actual transparent alpha outside the asset.

## bank-medium-v3.png

Edit reference: Assets/Game/Resources/GoldenRiver/shelf-root.png.

Use case: precise-object-edit. Create a NEW medium-width grounded bank sprite from this reference, preserving its material/style palette but repainting a distinct complete silhouette. Wide 2:1 canvas. Flat continuous grass landing at 30 percent down canvas from x=5 to95%. Grounded bank has an asymmetric shoulder: chunky slate-blue outcrop on left, a broader pale limestone vein and a deep vertical crevice near right, with small moss/ivy patches. Broad dark rock front continues solidly through lower image border, no suspended underside. Short irregular roots touch rock sides; no giant decorative tall leaves. Actual transparent headroom/outside the organic sides. No water, no background, no repeating wall texture, no extra platforms. Preserve the reference's painterly natural slate-blue rock with warm pale limestone patches, muted sage moss and short golden-green grass, upper-left warm daylight, cool dark rock fronts, crisp organic silhouette. No glowing yellow trim, no flowers, no logos, no text, no characters. Actual transparent alpha outside the asset.

## bank-wide-v3.png

Edit reference: Assets/Game/Resources/GoldenRiver/shelf-root.png.

Use case: precise-object-edit. Repaint this reference as a NEW broad grounded river bank, preserving the same materials/palette and lighting but using genuinely different geological shapes. Very wide 3:1 canvas. Continuous level grass contact at exactly30 percent down from x=4 to96%. A broad low limestone stratum with several irregular dark slate boulders and a shallow central recess; modest hanging moss, sparse exposed roots, side corners have natural rounded shoulders. Solid deep rock front continues down beyond canvas bottom, no floating underside or empty space under the bank. A complete hand-painted bank silhouette, no repeated modular texture. Transparent headroom and outer side margins. Preserve the reference's painterly natural slate-blue rock with warm pale limestone patches, muted sage moss and short golden-green grass, upper-left warm daylight, cool dark rock fronts, crisp organic silhouette. No glowing yellow trim, no flowers, no logos, no text, no characters. Actual transparent alpha outside the asset.

## shelf-root-compact-v3.png

Edit reference: Assets/Game/Resources/GoldenRiver/shelf-root.png.

Use case: precise-object-edit. Repaint the floating root platform as a NEW compact rooted boulder island of different silhouette, same material family. Wide 2:1 canvas. Level grassy standable top at30 percent down canvas spanning x=6 to94%. Broad heavy rounded rock core with two unequal lobes, a notch cut into the lower right edge, one short crooked exposed root on the right instead of long left dangling roots. Short moss pads, blue-gray and pale limestone face. Fully suspended detached rock, no ground, no glow, no scenery. Entire object fits transparent padding all sides. Not a scaled version of the long triangular slab: deliberately stout, compact asymmetric rounded anatomy. Preserve the reference's painterly natural slate-blue rock with warm pale limestone patches, muted sage moss and short golden-green grass, upper-left warm daylight, cool dark rock fronts, crisp organic silhouette. No glowing yellow trim, no flowers, no logos, no text, no characters. Actual transparent alpha outside the asset.

## shelf-masonry-compact-v3.png

Edit reference: Assets/Game/Resources/GoldenRiver/shelf-masonry.png.

Use case: precise-object-edit. Repaint the floating masonry platform as a NEW compact broken ruin support of different silhouette, SAME materials and grass as reference. Wide 2:1 canvas. Continuous level grass landing at30 percent down, x=5 to95%. Three chunky weathered limestone masonry blocks with blue-gray shadowed broken stones, one noticeably projecting fractured block at left bottom and a small hanging ivy patch at right. Stout short asymmetric block mass, not the same long straight lintel scaled smaller. Suspended isolated platform with transparent padding all sides and no supporting columns. No background, no glow. Preserve the reference's painterly natural slate-blue rock with warm pale limestone patches, muted sage moss and short golden-green grass, upper-left warm daylight, cool dark rock fronts, crisp organic silhouette. No glowing yellow trim, no flowers, no logos, no text, no characters. Actual transparent alpha outside the asset.

