# Connected river painting — 2026-10-27

Built-in OpenAI image generation with transparent output; exact model ID not
exposed. Style/palette reference: the archived Golden River Ruins v2 image. The
original generated RGBA PNG is retained without pixel edits as
`Assets/Game/Resources/BackgroundStudy/connected-river.png`.

One painting contains the banks, waterfalls, distant water, near water and small
stones. Registered background and foreground quads sample identical UVs,
texture, tint, scale and motion. The foreground material masks the lowest water
and draws after near-bank vegetation, so plants cannot hide it. It introduces no
independent water image, scale, scroll factor or image-space join. Mirrored reuse
is still a prototype limitation, not final production scenery.

## Generation prompt

Use case: stylized-concept. One original seamless scene layer for a painted 2D river platformer, wide 1536x1024 RGBA. Reference is palette/style only; no characters or platforms. Paint ONE CONNECTED RIVER extending from small distant waterfalls and recessed wooded banks in the middle distance, flowing continuously toward the viewer and filling the entire bottom 40% of the canvas. No separate foreground band, no horizontal splice, no second water plane, no abrupt change of water color, ripple scale or lighting. The river must read as one continuous painted body of water receding into the valley. Small rounded mossy stones scattered near the lower-third left/right banks (not exclusively the very bottom) belong to this same water. Upper 25% genuinely transparent sky space; middle 35% soft sage and muted ochre forested banks with a few small recessed ruined tower silhouettes, low contrast and no walkable ledges or bridges. River horizon roughly 40% up from the bottom, broad open center, continuous water painted fully opaque all the way to left/right/bottom edges. Unified soft teal, pale blue, sage and restrained golden reflections, no electric cyan or excessive white foam. Hand-painted storybook gouache texture with atmospheric distance, no blurred alpha fades, no glow outside silhouettes. Left/right edges have matching river heights and quiet matching banks for reuse. No sky paint, foreground grass strip, isolated floating platforms, animals, labels, borders or text. This single river-and-bank painting must replace separate near-water and far-water graphics. Keep every rock and ripple part of the coherent perspective and lighting.
