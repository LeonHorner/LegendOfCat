# Asset provenance

| Asset | Source / generator and model | Date | License / terms | Human creative decisions |
|---|---|---|---|---|
| docs/design/menu-movement-integration/*.png | Actual local Unity Editor ScreenCapture of the integrated painted menu and movement lab; no image generator | 2026-10-23 | Project captures under ASSET-LICENSE.md where rights exist; underlying painted art provenance is recorded below | Preserve the approved painted menu and temporary readable course; explicitly label prototype gameplay and document the round trip. |
| Assets/Game/VisualEvidence/*.png; docs/design/references/movement-lab-playthrough.mp4 | Captures of the local Unity movement lab; Unity Pipeline capture / ScreenCapture, FFmpeg video encoding; no image generator | 2026-10-23 | Project screenshot/recording under ASSET-LICENSE.md where rights exist; Unity text resources remain Unity dependencies | Review the temporary cat and platform readability in normal and small grayscale views; record the easy and precision jumps without treating the greybox as final art. |
| Movement lab platforms, geometric cat, background silhouettes and labels | Original procedural Unity geometry; OpenAI Codex (GPT-6 family), no external assets or image generator | 2026-10-23 | Code under repository code license; original visual artwork under ASSET-LICENSE.md where rights exist; built-in Unity text resources remain Unity dependencies | Existing human direction requires an orange/cream quadruped, a clear playable plane and subdued backgrounds; this implementation uses temporary geometry pending character-art work in #10. Numerical course tuning is provisional. |
| docs/design/references/legacy-bosses/* (five PNG sheets, five JSON metadata files and generation records) | Java boss-art archive; records identify built-in OpenAI image generation; exact model, original reference images and generation dates unavailable | Archived 2026-10-23; generation dates unknown | ASSET-LICENSE.md where rights exist; historical reference only, provenance limitations in archive README | Preserve stomp guardian, flying owl and dragon ideas as optional inspiration; new character art may be refined or redesigned. PNG/JSON copies verified against originals. |
| docs/design/references/*-v2.png (three initial scene concepts) | Built-in OpenAI image generation; exact model ID not exposed; edits of earlier generated concepts in the same design conversation | 2026-10-23 | Project reference artwork under ASSET-LICENSE.md where rights exist; generated provenance does not establish copyright or exclusivity | User selected naturally proportioned cat, cartoon owl influence, painted 2D worlds; rejected 2.5D; required clear platform/background separation. See docs/design/reference-prompts.md. |
| Start menu and update UI | Original procedural layout; OpenAI Codex, model identifier not exposed | 2026-10-23 | Repository code license (PolyForm Noncommercial 1.0.0); no new image/font/audio assets | Original plain dark menu, superseded by the painted presentation below. Updates remain inside Settings with one reminder per new stable version. Existing Unity GUI resources are used. |

No third-party game assets have been imported. tools/icon.png is a simple programmatically drawn placeholder.

## Painted menu backdrop

`Assets/Game/Resources/Menu/painted-valley.png` is original generated menu scenery.
Built-in OpenAI image generation produced it on 2026-10-23; the tool did not expose
an exact model ID. Terms: ASSET-LICENSE.md where rights exist. No external font,
character, music or image was imported.

Human direction: the user selected a charming painted 2D adventure and required
clear foreground/background separation. Implementation decisions: a quiet dark
left area supports the readable menu panel; a warm valley vista fills wide windows.
The image is decorative scenery and does not establish campaign themes or routes.

Generation prompt (built-in tool):

> Use case: stylized-concept. Asset type: original 2D game main-menu background, landscape 1536x1024. A charming hand-painted forest river valley at warm golden hour, soft atmospheric blue-green distant hills and old mossy stone ruins, amber foliage framing the far edges, winding glimmering stream toward the distance. Painterly gouache brushwork, entirely 2D, warm adventure mood. Composition: left half tranquil low-detail deep teal woodland shadow to support a separate opaque UI panel, right half luminous misty valley vista; no sharp foreground platforms or apparent gameplay routes. Natural lighting, restrained contrast, soft edges. No characters, animals, text, logos, UI, buttons, buildings with recognizable brands, or watermark. This is decorative menu scenery, not a level layout.
