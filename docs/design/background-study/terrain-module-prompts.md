# Painted terrain module generation — 2026-10-28

Built-in OpenAI image generation with transparent output. Exact model ID was not
exposed. Reference role for both calls: archived Golden River Ruins v2 as style,
palette and lighting reference, not a copied scene. PNG pixels are preserved.
License: ASSET-LICENSE.md where rights exist.

## Compact pillar

Saved as Assets/Game/Resources/BackgroundStudy/painted-pillar-v2.png.
Generated output ID: exec-0a9b5b28-f8bb-4ea0-9464-424e56626fe3.

Create a production 2D game terrain sprite on genuinely transparent background, original hand painted storybook gouache matching the attached reference Golden River Ruins. Reference role: style/palette/lighting only. One isolated grounded compact rock pillar, NOT a whole scene. Dark blue slate irregular large craggy stones, warm golden yellow grassy flat landing edge, soft sage and dark teal ivy hanging in asymmetric clusters down front, a few tiny white meadow flowers. Natural asymmetric chipped sides and broad rounded boulders at foot; solid rooted silhouette, no brick or masonry wall, no repetitive texture, no geometry slab or isometric top, no black outlines. Sunlight from upper left, broad painterly shapes with restrained details matching the concept. Front view side scrolling 2D, narrow visible turf thickness only. Entire object visible with transparent margin. Pillar silhouette approx 75% image width, grass landing edge from x20% to80%, surface contact at y18% from image top; boulder feet at y90%, small flowers above edge. Vertical face approximately as tall as landing width. No water, no background, no cat, no text. Use the concept's richly painted flowing silhouette rather than photoreal texture.

Output review: landing line is approximately 24% from the top, not requested 18%.
The implementation registers that actual line. Low-opacity haze is discarded by
alpha testing. The fully painted silhouette is retained rather than cropped.

## Wide bank

Saved as Assets/Game/Resources/BackgroundStudy/painted-bank-v2.png.
Generated output ID: exec-4e5da317-1008-4663-bd50-97d7477f7c37.

Use case stylized-concept. Original production 2D side-view platformer terrain sprite, one WIDE LOW GROUNDED river-bank rock ledge, horizontal 3:1 aspect ratio. Reference supplied is style/palette/lighting only. Match its hand-painted gouache slate-blue crags, warm golden grassy lip, small white meadow flowers, dark teal ivy. This is the BROAD mainland platform, not a tall rectangular pillar. Entire terrain silhouette isolated on genuinely transparent background, no glow/shadow haze outside silhouette, no background or water. Flat playable narrow golden grass landing edge at 20% image height, stretches from 10% to90% image width, small leaves/flowers above it. Rock face beneath is low, layered large irregular natural boulders with deep shadow crevices and asymmetric hanging ivy clumps; feet broaden in large rounded rocks at 92% image height. Craggy angled sides, broken stone profiles, integrated vegetation, warm left light and cool right shadows. Rich broad painterly shapes matching reference, no photo textures, no masonry, no repeated pattern, no black outlines, no isometric top face, no floating island, no cat, no water, no text. Keep transparent margin all around. Wide image.

Output review: landing line is approximately 40% from the top. The mesh maps this
actual row to the collider and compresses the transparent/decorative crown rather
than stretching flowers with body depth. Final visual acceptance remains pending.
