# River, meadow and hanging foliage — 2026-10-25

2026-10-31 archive location: Historical meadow-surface.png and hanging-foliage.png now reside in Assets/Game/Art/BackgroundStudyArchive/. The foreground-river.png remains active under Resources/BackgroundStudy. These prompts document the original experiments.

Built-in OpenAI image generation; exact model ID not exposed. The archived Golden
River Ruins v2 reference supplies palette and brush style. No original image
pixels were edited. Runtime UV windows select foliage columns; original alpha is
retained in the PNGs and thresholded only by the rendering shader.

## Foreground river prompt

Use case: stylized-concept. Original 2D painted FOREGROUND RIVER layer for
side-scrolling Golden River Path. Wide 1536x1024 RGBA. Upper 70% completely
transparent, no sky, no landscape, no cliffs, no platforms. Lowest 25% filled with
turquoise teal flowing river water, restrained warm golden reflections and gentle
painted ripples, reaching left/right/bottom edges continuously. A few SMALL rounded
smooth dark mossy river stones break the water surface near bottom left and bottom
right, not in a continuous ridge, none taller than lowest 30% of canvas. Upper
water silhouette an irregular nearly horizontal soft river ripple edge, real alpha
above; no blur, no glow or haze. Designed as the closest world-space scenery layer,
viewed slightly from above, in front of distant stone platform bases. Keep most
center water open, no waterfall walls, foam obscuring scenery, large boulders,
bridge, walkable shore, characters, animals, text or borders. Painterly storybook
gouache matching reference, fully 2D with lighting/overlap suggesting depth. No
generated full scene; ONLY the lower foreground river and scattered small stones.

## Meadow surface prompt

Use case: stylized-concept. One original square seamlessly repeatable hand-painted
meadow ground texture, TOP DOWN view, warm storybook gouache matching reference
palette. Soft moss and short golden-green grass, scattered tiny pale stone flecks,
occasional restrained little white daisy and ochre leaves, no large flowers or
stones. Fully opaque filled canvas, no sky, no horizon, no borders, no cliff sides,
no words, no characters. This texture will be projected onto the slightly visible
TOP SURFACES of stone platform ledges in a side-scrolling painted game; use broad
low-detail brushwork, softly mottled olive and sunlit straw gold, no artificial
grid, no bright outlines. Natural non-uniform patches, quiet texture so a ginger
cat is clear against it. Continuous matching edges on all sides.

## Hanging foliage prompt

Use case: stylized-concept. Transparent RGBA SPRITE ATLAS of THREE DIFFERENT
hanging vegetation cutouts in exactly three equal vertical columns across a
1536x1024 canvas. Each column independent and separated by generous transparent
gutters. LEFT: trailing dark olive ivy with a few warm golden leaf tips, descending
from the top center into two long unequal strands. MIDDLE: short dense sage-green
moss and fern tuft that hangs only through the upper half, empty transparent lower
half. RIGHT: longer branching leafy vine with scattered small rounded leaves and
uneven fine roots hanging downward. Attachment starts near the TOP of each column.
No supporting platform, stone, trunk, soil, background, sky or labels. Just hanging
vegetation, full silhouettes with real transparent alpha outside, no blur, glow,
haze or shadow rectangle. Painterly 2D storybook gouache matching the mossy
river-platform reference. Natural distinct silhouettes and lengths, asymmetrical,
not geometric strings, no repeated identical leaves. Restrained warm dark greens
readable against dark rock; small warm leaf highlights, no neon green. These will
be draped over different platform fronts and side edges, without covering the
walkable top. Entire lower portions outside leaves must remain transparent.
