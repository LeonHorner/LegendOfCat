# Asset provenance

## Mika character authoring and articulated preview

`Assets/Game/Art/Mika/` contains original generated character studies, retained
pose-sheet experiments, and the rejected articulated experiment. The built-in OpenAI
image generation tool produced the pose sources on 2026-10-23 and the cutout source
and seam-cleanup edit on 2026-10-24. Exact model IDs were not exposed. References
were the project's earlier original Mika images; no third-party character, font,
music or asset was imported. Terms: `ASSET-LICENSE.md` where rights exist; generation
does not establish exclusivity or copyright.

Human direction selected the natural orange-and-cream quadruped, modest head/eyes,
striped tail, smooth motion and grounded paws. The user explicitly authorized local
image cleanup/registration. `tools/prepare-mika-sprites.py` registers pose frames;
`tools/interpolate-mika-sprites.py` produced experimental optical-flow inbetweens
that were rejected for running. These pose experiments are retained for provenance
and are not the default character preview. `tools/prepare-mika-rig.py` extracts
eight cutout parts from `mika-rig-clean-source.png`, removes stray alpha pixels and
registers joint axes. Original sources are preserved. Pillow/numpy/OpenCV are
authoring-only tools, not Unity runtime dependencies. Unity generated the sprite
metadata and candidate prefabs. No serialized scene was hand-edited.

Implementation decisions: a fixed torso/head silhouette, constant limb lengths,
alternating diagonal support feet, world-locked stance contacts, and leg flexion
for landing. No whole-body scaling or gameplay timing change is used. Full prompts,
review limitations and revision history are in `docs/design/mika-animation/`.
The user rejected the articulated experiment on 2026-10-24: feet/legs are too thin,
the assembled anatomy is unsuitable and the jump animation does not fit. The
optical-flow run experiment was also rejected. These assets/captures remain only
as provenance and negative examples, not accepted production content. The agreed
replacement is a consistently drawn complete-character run loop reviewed before
gameplay integration, followed by authored jump/landing sequences. See the decision
in `docs/design/art-direction.md` and `docs/design/mika-animation/README.md`.

`docs/design/mika-animation/rig-*.png` and the movement review video are actual local
Unity Camera.Render captures of the controller in an isolated local Physics2D
scene at 60 Hz on 2026-10-24, encoded with FFmpeg; no image generator was used for
the captures. Terms: project captures under `ASSET-LICENSE.md` where rights exist,
with underlying character provenance above. The recording reviews running, turns,
held/short jumps, landing and stopping; it does not verify installed builds or updates.

| Asset | Source / generator and model | Date | License / terms | Human creative decisions |
|---|---|---|---|---|
| docs/design/desktop-window-history/*.png | Actual local Unity 6000.3.27f1 Windows preview-player ScreenCapture; no generator/model | 2026-10-23 | Project captures under ASSET-LICENSE.md where rights exist; backdrop provenance below | Inspect fixed navigation with 60 release cards at standard, minimum, narrow and ultrawide sizes; preserve readable painted-menu layout and inspect remaining/total MB using simulated download data. |
| docs/design/menu-movement-integration/*.png | Actual local Unity Editor ScreenCapture of the integrated painted menu and movement lab; no image generator | 2026-10-23 | Project captures under ASSET-LICENSE.md where rights exist; underlying painted art provenance is recorded below | Preserve the approved painted menu and temporary readable course; explicitly label prototype gameplay and document the round trip. |
| Assets/Game/VisualEvidence/*.png; docs/design/references/movement-lab-playthrough.mp4 | Captures of the local Unity movement lab; Unity Pipeline capture / ScreenCapture, FFmpeg video encoding; no image generator | 2026-10-23 | Project screenshot/recording under ASSET-LICENSE.md where rights exist; Unity text resources remain Unity dependencies | Review the temporary cat and platform readability in normal and small grayscale views; record the easy and precision jumps without treating the greybox as final art. |
| Movement lab platforms, geometric cat, background silhouettes and labels | Original procedural Unity geometry; OpenAI Codex (GPT-6 family), no external assets or image generator | 2026-10-23 | Code under repository code license; original visual artwork under ASSET-LICENSE.md where rights exist; built-in Unity text resources remain Unity dependencies | Existing human direction requires an orange/cream quadruped, a clear playable plane and subdued backgrounds; this implementation uses temporary geometry pending character-art work in #10. Numerical course tuning is provisional. |
| docs/design/references/legacy-bosses/* (five PNG sheets, five JSON metadata files and generation records) | Java boss-art archive; records identify built-in OpenAI image generation; exact model, original reference images and generation dates unavailable | Archived 2026-10-23; generation dates unknown | ASSET-LICENSE.md where rights exist; historical reference only, provenance limitations in archive README | Preserve stomp guardian, flying owl and dragon ideas as optional inspiration; new character art may be refined or redesigned. PNG/JSON copies verified against originals. |
| docs/design/references/*-v2.png (three initial scene concepts) | Built-in OpenAI image generation; exact model ID not exposed; edits of earlier generated concepts in the same design conversation | 2026-10-23 | Project reference artwork under ASSET-LICENSE.md where rights exist; generated provenance does not establish copyright or exclusivity | User selected naturally proportioned cat, cartoon owl influence, painted 2D worlds; rejected 2.5D; required clear platform/background separation. See docs/design/reference-prompts.md. |
| Start menu and update UI | Original procedural layout; OpenAI Codex, model identifier not exposed | 2026-10-23 | Repository code license (PolyForm Noncommercial 1.0.0); no new image/font/audio assets | Original plain dark menu, superseded by the painted presentation below. Updates remain inside Settings with one reminder per new stable version. Existing Unity GUI resources are used. |

No third-party game assets have been imported.

## Mika movement completion and archive

The bounded apex experiment generated one pre-apex candidate with built-in OpenAI
image generation on 2026-10-24 (exact model ID not exposed). Source, endpoint
reference, exact prompt and rejection notes are in movement-completion/apex-pair.
Terms: ASSET-LICENSE.md where rights exist. The user requested two targeted added
frames, conditional on the first being visually sound. The first still mismatches
head/trunk proportions and was not integrated; no second candidate was created.
An independent alpha-only cleanup removes disconnected specks from the four
active transition textures, preserving RGB and connected silhouette pixels,
dimensions and pivots. tools/clean-mika-transition-specks.py records the operation.

The user rejected the cute legacy sample and retained the current in-game design.
movement-completion/in-game-inbetweens contains eight new whole-character midpoint
drawings generated with built-in OpenAI image generation on 2026-10-24 (exact
model ID not exposed), followed by a targeted recovery-pose edit. Exact prompts,
actual-game endpoint reference, mappings and registration choices are preserved.
Terms: ASSET-LICENSE.md where rights exist. Human direction requests more frames
between existing transitions, preserving character design, proportions and the
four-frame run. Processing uses one uniform scale, alpha cleanup and translation;
original endpoint drawings and overall preview duration are unchanged. This is
documentation-only review artwork, with no Unity integration or acceptance claim.

A small legacy-inspired style example is stored in movement-completion/legacy-cute-example.
Built-in OpenAI image generation created its eight complete-cat drawings on
2026-10-24; exact model ID was not exposed. The reference is the generated legacy
cat atlas (assets/images/cat/cat-actions-v1.png), whose PROMPT.md identifies built-in
image generation; its earlier generation date/model remain unknown. Only the cat
reference cells were cropped/mirrored, not legacy tile sheets or game code. Human
direction requested cuter round proportions, blue eyes, sturdy paws, curved body
postures and head/gaze following the jump. The exact prompt and processing choices
are preserved with the source. Terms: ASSET-LICENSE.md where rights exist. This
documentation-only example uses one 0.5 nearest-neighbor scale across all poses;
it does not alter Unity assets, the legacy checkout, physics or input timing.

Proportion review 3 is in movement-completion/revision-3: a coherent eight-pose
sheet generated with built-in OpenAI image generation on 2026-10-24 and refined
by a targeted head-consistency edit (exact model ID not exposed). Its sole identity
reference is original standing Mika; both prompts and the selected source are
preserved. Terms: ASSET-LICENSE.md where rights exist. Human direction rejected
flight-time shrinking and requires head/trunk consistency. Every pose uses one
global 0.72 scale, with translation-only shoulder registration and alpha cleanup.
Normal/slowed and stationary size-check loops remain documentation-only review
artifacts. No Unity integration or user acceptance is claimed.

Following the user's report of abrupt transitions, movement-completion/revision-2
contains a review-only four-pose sheet from built-in OpenAI image generation,
2026-10-24, exact model ID not exposed. Terms: ASSET-LICENSE.md where rights exist.
Its identity reference uses the existing Mika drawings. The exact prompt and
uniform scale/toe-registration decisions are recorded alongside the source.
Human direction: shallower takeoff and more gradual recovery. No revision-2 art
is imported into Unity; it is not accepted production content.

Rejected Mika rig/optical-flow source, assets, code and tests have moved outside
Unity to docs/design/mika-animation/archive. Historical paths in this document
resolve through that prefix. Original Unity .meta files and exact source bytes
are preserved in manifest.json with SHA-256 hashes; archive checkout disables
line-ending conversion to preserve those hashes on both platforms.

Built-in OpenAI image generation created the four complete-character transition
drawings on 2026-10-24; exact model ID was not exposed. Source, exact prompt,
identity reference, normal/slowed review loops and manual registration decisions
are in docs/design/mika-animation/movement-completion. Active derived PNGs are
Assets/Game/Art/Mika/Transitions. Terms: ASSET-LICENSE.md where rights exist;
generation does not establish exclusivity or copyright. Human direction retained
the original four-frame run and sturdy paws, selected full-character drawing
instead of rejected rig/optical flow, and requested smoother flight/landing
transitions. Local processing only cleans alpha, applies one uniform scale and
translates drawings. The original active sheet has alpha-only speck cleanup;
all RGB values, dimensions, poses and retained alpha are unchanged. Its exact
original PNG and metadata remain archived. No new abilities or idle/fish behavior
were added. The integrated recording/strips are regenerated from Unity playback.

## Integrated Mika review evidence

`docs/design/mika-animation/integrated-movement-review.mp4`, the two
`integrated-review-*.png` strips and `integrated-review.json` were captured from
the original course/prefab in Unity 6000.3.27f1 on 2026-10-24 using
MikaMovementReview.Record. No image generator/model or third-party artwork was
used. FFmpeg encoded the video; Pillow cropped/labeled the strips and derived
grayscale. Terms follow the original project artwork under ASSET-LICENSE.md
where rights exist. Human direction retained the approved four-pose run and
requested real-character integration checks. The original source drawing is
unchanged; the Editor corrected run pose 6's sole pivot by ignoring disconnected
pixels. Review evidence is not acceptance of the remaining movement artwork.

## Complete-character Mika run study

On 2026-10-24 the user selected the earlier four-frame run for prototype
integration after comparing animated loops. `Assets/Game/Art/Mika/Original/`
contains Editor-created sprite metadata referencing the unchanged original
`mika-movement-candidate.png`; `Assets/Game/Resources/MikaOriginal.prefab` is
the corresponding cosmetic presentation. No new generated artwork is involved.
Human decisions: preserve original run poses 4–7 and substantial paws, remove
inbetweens, register visible sole pivots and retain physics/input timing.
Other original poses remain temporary; this does not accept the full movement set.
The apex transition reuses original tucked pose 6 as sprite slot 16 with the
airborne pivot shared by takeoff/fall. No image generation or repainting was used;
the human request was an additional pose at the ascent/descent transition.

`docs/design/mika-animation/frame-run-study/` contains an original generated
complete-cat run sheet and derived review artwork, produced with built-in OpenAI
image generation on 2026-10-24; exact model ID was not exposed. Terms:
ASSET-LICENSE.md where rights exist. The identity crop comes from the earlier
generated Mika movement source. Human direction selected fuller paws and
complete-character frames after rejecting the cutout rig. Local processing only
cleans alpha and translates frames; no optical flow or anatomical deformation.
See the study README for generation direction, processing and known defects.
This is review artwork with unresolved proportions and stride continuity, not
accepted production animation. No runtime assets or gameplay were replaced.

## Application icon

`tools/icon.ico` is the single authoritative original icon supplied by Leon Horner
from the legacy project's `assets/legendofcat.ico`, imported unchanged on 2026-10-23.
Original creation date and authoring software/model were not supplied; no image
generator was used for this import. Terms: project original artwork under
ASSET-LICENSE.md where rights exist. Human direction explicitly selected this
orange cat on a blue rounded square for all application surfaces, without redesign.
`Assets/Game/Branding/icon.png` is the exact 256 x 256 PNG entry extracted from the
ICO with `node tools/application-icon.mjs`; no resampling, recoloring or synthesis.
Unity generates platform icon sizes at build time. The launcher embeds the ICO;
Velopack uses the ICO on Windows and the PNG for Linux AppImage/desktop integration.
The earlier generated placeholder icons have been replaced/removed.

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

## Mika apex atlas-edge correction — 2026-10-24

`Assets/Game/Art/Mika/Original/original-apex-isolated.png` is a local Unity
Editor-derived copy of the existing apex atlas cell with a two-pixel transparent
sampling border. No new image generator/model, drawing or scaling was used.
Source: the previously documented generated Mika movement atlas; existing
ASSET-LICENSE.md terms apply where rights exist. Human direction: remove the
neighboring atlas speck and retain the accepted apex alignment (-2, -14 source
pixels). The user accepted the in-game result after playtesting. Original visible
RGBA and alpha are preserved; the atlas and sprite GUID remain intact.

## Golden River background study — 2026-10-24

`Assets/Game/Resources/BackgroundStudy/painted-atlas.png` is original generated
prototype scenery, made with the built-in OpenAI image generation tool. Exact
model ID was not exposed. License: ASSET-LICENSE.md where rights exist. Source
reference: our archived `docs/design/references/golden-river-ruins-v2.png`, used
only for palette and painterly style. No external assets or fonts were imported.
Human direction: compare approximately three depth layers with a calm panorama;
retain warm painted 2D art, softer backgrounds and unambiguous gameplay edges.
Implementation decisions: three equal panoramic strips, runtime UV composition,
atmospheric fades and mirrored reuse; no character art changes. This atlas is
prototype artwork, with no user approval of final production background direction.

Built-in generation prompt:

> Use case: stylized-concept. Create a single 1536x1024 background layer ATLAS for a warm hand-painted 2D side-scrolling Golden River Path game prototype. Reference image is STYLE AND PALETTE ONLY. No cat, owl, characters, gameplay platforms, foreground, text or borders. Atlas has precisely THREE equal horizontal strips stacked vertically, each its own very wide panoramic landscape, no margin. TOP third: ONLY warm pale golden sunset sky, soft clouds and extremely distant lavender hills, no trees/buildings. MIDDLE third: atmospheric blue sage distant riverside forest, small recessed ruined tower silhouettes, river mist; fill entire strip with pale atmospheric sky above and soft river below. BOTTOM third: closer soft sage and muted ochre riverside trees and vegetation with open gaps, fill strip with pale atmospheric sky above and mist below. All three strips low contrast, painterly brush texture, no dark outlines, no horizontal walkable ledges or bridges. In EACH strip left and right edges have identical low-detail mist colors and landscape heights for horizontal reuse; broad subtle forms rather than obvious repeating landmarks. The result must be beautiful painted scenery, separated into the specified three strip regions for runtime composition, not a screenshot of a game.

## Golden River transparent replacement layers — 2026-10-25

`Assets/Game/Resources/BackgroundStudy/middle-cutout.png` and `near-cutout.png`
are original RGBA paintings generated with the built-in OpenAI image tool; exact
model ID was not exposed. Source reference: our archived Golden River Ruins v2,
used for palette/style. License: ASSET-LICENSE.md where rights exist. No third-party
asset or font was added. Original generated pixels are preserved. Prompts and
composition constraints: docs/design/background-study/cutout-prompts.md.
Human decision: the user identified insufficient lower scenery and blurry layer
transitions in Unity. Implementation: enforce the existing 16:9 presentation,
lower camera framing, use independent transparent silhouettes rather than broad
strip crossfades, retain image proportions, reject low-alpha halos at runtime and
place smaller near vegetation below the gameplay plane. This feedback does not
select A/B/C or approve final production backgrounds.
