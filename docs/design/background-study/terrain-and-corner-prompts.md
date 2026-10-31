# Terrain and foreground corner studies — 2026-10-25

2026-10-31 archive location: Historical painted-terrain.png now resides in Assets/Game/Art/BackgroundStudyArchive/. The foreground-corners.png remains active under Resources/BackgroundStudy. These prompts document the original experiments.

Built-in OpenAI image generation; exact model ID not exposed. The archived
Golden River Ruins v2 image supplies style and palette, not layout. Original PNG
pixels are retained. Terrain samples are tiled/mirrored at runtime; floating
undersides are bounded by a mesh. Corner alpha is thresholded to reject the faint
generated halo and limited to safe lower-corner regions.

## Painted terrain prompt

Use case: stylized-concept. Original hand-painted 2D platformer TERRAIN TEXTURE,
square 1024x1024. Reference only palette/style. A completely filled rectangular
side-on cross-section of solid mossy old stone and dark earth, topmost 8% a thin
warm golden-green grass turf cap with a CRISP STRAIGHT continuous flat walkable
top edge exactly along the canvas top boundary. Below the turf: dark charcoal
olive irregular small stone blocks, earth, moss and a few restrained ivy traces.
Image is an opaque square terrain fill, NO sky, NO background, NO transparency,
NO floating island silhouette, NO exposed bottom ledge, no perspective and no
inset. Stone and earth extend fully to left/right/bottom edges. Left and right
edges must match for horizontal tiling. Lower 80% a consistently dark restrained
low-detail stone/soil texture that can repeat vertically. Soft painted brush
texture inside the forms, clear solid mass, front darker than naturally lit grass
top. Avoid bright seams, horizontal glowing lines, evenly repeating brick grid,
flowers above the top, words or borders. Designed as a source for separately
sampling turf and dark ground fill; no other assets.

## Foreground corner prompt

Use case: stylized-concept. Original 2D game foreground corner framing asset,
wide landscape RGBA, 1536x1024. Reference style/palette only. Two tiny separate
clusters of dark warm olive fern fronds, low grasses and a few small ochre leaves
at the EXTREME BOTTOM LEFT and EXTREME BOTTOM RIGHT corners, like painted
vegetation framing a storybook scene. Everything else genuinely transparent:
central 65% width completely empty, upper 80% height completely empty. Each
cluster confined to outermost 17% width and lowest 20% height. Fronds emerge from
outside the bottom/side canvas edges, NO horizontal ground strip, no connecting
plants, no rocks, no trunks, no scene backdrop, no sky, no characters, no text or
borders. Keep foliage dark subdued and a little softer internally than gameplay
objects, but with definite irregular leaf alpha contours, no glow or haze.
Intentionally spare: foreground accents must leave gameplay surfaces and the cat
fully visible. This is ONLY two isolated corner cutouts on transparent background,
not a landscape.
