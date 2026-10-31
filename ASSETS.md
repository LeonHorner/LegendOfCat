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

`Assets/Game/Art/BackgroundStudyArchive/middle-cutout.png` and `near-cutout.png`
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

## Golden River painted terrain and corner framing — 2026-10-25

`Assets/Game/Art/BackgroundStudyArchive/painted-terrain.png` and
`foreground-corners.png` were generated with the built-in OpenAI image tool;
exact model ID was not exposed. Source reference: our Golden River Ruins v2
archive for style/palette only. License: ASSET-LICENSE.md where rights exist.
No external assets/fonts were added. Original PNG pixels are preserved; runtime
sampling separates the turf cap from repeatable stone fill and rejects corner
alpha halos. Full prompts: docs/design/background-study/terrain-and-corner-prompts.md.
Human direction: add foreground accents in the corners, at least double the test
length, and make platforms look grounded and fit the reference; some suspended
jump platforms are also acceptable. Implementation: a 109-unit lab with ten
massive stone/earth fronts and four tapered stationary floating rocks, clear
flat collision-aligned grass tops, and sparse dark lower-corner foliage outside
the landing plane. This remains prototype art and does not approve a final
background concept, level layout or campaign expansion.

## Golden River painted depth and hanging foliage — 2026-10-25

`Assets/Game/Resources/BackgroundStudy/foreground-river.png`, `meadow-surface.png`
and `hanging-foliage.png` are original generations from the built-in OpenAI image
tool, exact model ID not exposed. Source: our Golden River Ruins v2 archive for
style/palette. License: ASSET-LICENSE.md where rights exist. Original PNG pixels
are preserved; the renderer samples foliage atlas columns and rejects low-alpha
halos. Full prompts: docs/design/background-study/depth-and-foliage-prompts.md.
Human direction: visible spatial depth like the reference, small stones/water in
front, varied platforms behind them and backgrounds farther back; hanging leaves
and greenery with differing shapes/lengths are important. Implementation: a shallow
painted meadow face recedes behind each exact collision lip, different rock tints
and registration break repetition, ivy/moss/fern patches hang across fronts and
sides, and a separate foreground river overlaps the lower cliff bases. This is
painted depth within the 2D prototype, with no new swimming, moving-platform or
3D gameplay behavior. No final production art or A/B/C selection is implied.

## Golden River composition refinement — 2026-10-27

No new images or generators were used. Existing documented PNGs remain unchanged.
Human feedback rejected the high water, exaggerated platform perspective and
front-rim stance, requesting foreground/background coherence like the concept.
Rendering decisions: smaller lower river motifs aligned to world-space platforms,
quieter water saturation, narrow grass bands surrounding the unchanged contact
line, restrained side shading, larger rock motifs and mipmapped ground textures.
The original movement/collision geometry and Mika artwork are preserved. This
records a revision target, not user acceptance of final artwork or an A/B/C choice.

## Connected Golden River painting — 2026-10-27

`Assets/Game/Art/BackgroundStudyArchive/connected-river.png` is an original RGBA
painting generated with the built-in OpenAI image tool; exact model ID was not
exposed. Source reference: our Golden River Ruins v2 archive for palette/style.
License: ASSET-LICENSE.md where rights exist. Original generated pixels are
preserved. Full prompt: docs/design/background-study/connected-river-prompt.md.
Human direction: foreground water must connect to background water, potentially
using one image. Implementation: replace the separate foreground-river and
middle-cutout water paintings with a single river/bank/waterfall composition.
The full scene and lower cliff-base overlap sample the same texture and UVs on
registered background/foreground quads, with shared tint, scale and movement. Sky, foliage and playable
terrain remain separate. Superseded paintings are retained but not loaded by
this lab. No final production or A/B/C selection is implied.

## River composition correction — 2026-10-28

User review showed floating-looking cliff bases and missing foreground water.
No new pixels or external assets were introduced. The same river painting now
uses registered foreground copies drawn after nearby bank vegetation, replacing
the incorrect multipass order. Grounded cliffs have subtly broadened submerged
feet, and this bounded course keeps camera height fixed during jumps to retain
the water. Collision contacts and the four intentional suspended rocks are
unchanged. These are human-directed prototype refinements, pending visual review.

## Painted terrain modules — 2026-10-28

Sources: painted-pillar-v2.png and painted-bank-v2.png under
Assets/Game/Resources/BackgroundStudy/. Original generated PNG pixels are
preserved. Built-in OpenAI image generation; exact model ID not exposed.
Reference: our archived Golden River Ruins v2, for palette, brushwork and lighting.
License: ASSET-LICENSE.md where rights exist. Full prompts and output identifiers:
docs/design/background-study/terrain-module-prompts.md.

Human direction rejected the repeating rectangular-wall prototype, especially the
right platform. The replacement uses whole painted rock silhouettes, integrated
golden turf, asymmetric ivy, flowers and broad boulder feet. A wide bank painting
serves broad ground; a compact pillar serves narrow supports. Code registers the
painted landing row to collision independently of decorative crown and submerged
body height. No change to Mika, collision layout or physics. Shader alpha clipping
rejects generated outer haze; pixels remain unmodified. Two-source reuse and body
stretching remain prototype limitations, with visual acceptance pending.

## Terrain integration grading — 2026-10-28

No new generated assets. The original pillar/bank PNGs are unchanged. In response
to feedback about detached-looking platforms, shader grading reduces yellow
saturation, introduces restrained shared sage atmosphere and cool wet coloration
at grounded feet. A lower, narrowly irregular water mask reveals the boulder
silhouettes. No painted fake foam or independent water texture was introduced.
Human direction: connect platform lighting and shore contact to the environment;
final visual acceptance is pending.

## Shore connection and grass registration — 2026-10-28

painted-shore-v2.png under Assets/Game/Resources/BackgroundStudy/ is an original
transparent painting generated with the built-in OpenAI image tool; exact model
ID not exposed. Style reference: our archived Golden River Ruins v2. License:
ASSET-LICENSE.md where rights exist. Original generated pixels preserved. Full
prompt/output identifier: docs/design/background-study/terrain-module-prompts.md.

Human direction: join the right land-adjacent platform to surrounding banks and
remove Mika's slight apparent hover. A low mossy boulder/fern buttress connects
five selected banks below the playable plane, with no added collision. Existing
pillar/bank paintings are unchanged; UV registration now uses the actual central
load-bearing grass rows (pillar 0.748, bank 0.587), rather than brighter leaf tips.
Mika's prefab, presentation and collision physics remain unchanged. Final visual
acceptance remains pending.

## World-space shore and distant parallax — 2026-10-29

The user rejected the moving shore contact and clipped middle/near trees, and
requested a more reliable layered composition. The lab now keeps connected river
banks, water and platform contact in world coordinates in every preset. Only sky
and an independent distant landscape parallax. The clipped near-cutout painting
is archived and no longer loaded. This supersedes the all-depths-moving study;
production direction and final visuals remain pending review.

New original generated PNGs under Assets/Game/Resources/BackgroundStudy/:
- painted-shelf-v3.png: shallow suspended fragment, jagged underside, no standing
  boulder feet; replaces vertically compressed pillar art on floating platforms.
- distant-canopy-v3.png: complete tree/tower silhouettes with transparent headroom,
  no river or connected foreground, placed behind the fixed world-space banks.

Generator: built-in OpenAI image generation, exact model ID not exposed.
Date: 2026-10-29. Reference: our Golden River Ruins v2 for style/palette/lighting.
License: ASSET-LICENSE.md where rights exist. Original pixels preserved.
Full prompts, output identifiers and creative decisions:
docs/design/background-study/world-shore-prompts.md.
Human decisions: preserve atmospheric parallax at a distance, stop contact drift,
replace cut trees, keep suspended bodies shallow without changing collision tops.
Two-point contact registration, existing paw alignment and separate shore
buttresses remain; no Mika source or movement physics changes were introduced.

## Separated valley and foreground test — 2026-10-29

New original source: Assets/Game/Resources/BackgroundStudy/recessed-valley-v4.png.
Built-in OpenAI image generation with transparent output; exact model ID not
exposed. Reference: our Golden River Ruins v2 for style/palette/lighting. License:
ASSET-LICENSE.md where rights exist. Original PNG pixels preserved. Full prompt
and output identifier: docs/design/background-study/separated-depth-prompts.md.

Human direction rejected the connected-river variants and explicitly requested
testing separate background, gameplay scenery and foreground. The valley has
recessed waterfalls terminating in mist and a forested lower edge, without a near
river plane. Existing foreground-river.png is reused independently, with its
original generation records above; no regeneration or pixel edit. Its earlier
rejection concerned attempted connection to background water. It now uses its own
material, world-space tiling and authored wave contour, with saturation/tint
applied only in the shader. The connected-river painting and StudyRiver pass are
archived and no longer loaded. Existing shelf/pillar/bank art, Mika and physics
remain unchanged. This is an experimental composition pending visual acceptance.

## Foreground water motion — 2026-10-30

No new bitmap assets or image generation. Existing foreground-river.png pixels
remain unchanged. Human direction selected Subtle distance, removed comparison
controls and requested animated near water. The StudyWater shader introduces
small color-masked refraction and moving highlights in bright cyan water while
retaining the original alpha shoreline and fixed warm/dark rocks. Its phase is
world-space and driven by explicit time for reproducible captures. Source,
generator and license records for the painting remain above. The selected
background factors are sky 0.05, valley 0.14 and canopy 0.08.

## Visible water flow revision — 2026-10-30

No new source artwork or generation. In response to invisible-looking motion,
StudyWater increases water/neutral-foam refraction and adds moving light streaks.
A second source-color mask prevents pulling rock colors into nearby water.
Normal animation uses the shader game clock; explicit time is opt-in for capture.
Original source pixels, warm/dark rocks, alpha wave contour and terrain are
unchanged. Previous subpixel shimmer is superseded.

## Running water activity — 2026-10-30

No source pixels or generator changes. Human direction requested stronger water
motion and more waves during running. Grounded body speed drives a smoothly
blended shader activity value, extra accumulated flow phase and additional fine
crests. Full running gives approximately 2.2 times baseline flow and 35% more
refraction; rock colors and authored alpha remain fixed. The dedicated
water-running.mp4 is an explicit full-activity render for visual comparison;
actual body-driven transitions are covered by motor/physics tests.

## Background study review archive (2026-10-30)

The curated evidence in `docs/design/background-study/archive/` contains exact
copies of historical repository media and seven user-provided feedback uploads,
plus H.264 review derivatives of existing local capture frames. No new artwork
was generated or retouched. Original paintings retain the source, generator and
license records elsewhere in this document; user screenshots document this project's Unity review,
and the reposted concept retains its existing generated-reference provenance.
Human curation selected thirteen major iterations, representative stills and
clips, and start/midpoint/end keyframes. The archive README records decisions;
`manifest.json` records source revisions, original upload names, SHA-256 hashes
and encoding details. Unknown capture dates/revisions are explicitly recorded.
All 291 raw capture frames remain untouched locally; the committed videos are
lossy review copies, not lossless backups of those frames.

## Golden River checkpoint prototype integration - 2026-10-30

The existing selected BackgroundStudy paintings now also dress the menu's
MovementCourse prototype. No new generated images or modified source pixels are
introduced. Human decisions: reuse fixed Subtle distance, painted ground/shelves,
separate foreground water and running-responsive waves; preserve the original
seven-platform layout and both checkpoint positions. The course camera holds
y=1 with a 5.5-unit orthographic half-height to retain the river during jumps.
Three Unity Editor-created Resources materials retain references to the original
scenery shaders in standalone builds. Their metadata is preserved.

The checkpoint-prototype evidence uses the actual Resources course, controller,
presentation and lifecycle in an isolated Unity physics scene. Scripted fixtures
place Mika at each checkpoint, induce falls, pause/retry and restart. The recording
is reproducible with BuildBackgroundStudy.RecordCheckpointPrototype; it is not a
claim of a continuous manually played traversal. No imported artwork or new license
is involved. Existing generator and license records still apply.

## Extended checkpoint prototype and trimmed water geometry - 2026-10-30

Existing paintings are reused unchanged for a 109-unit, fourteen-platform course
with four checkpoints. No new images, source-pixel edits or licensed assets were
introduced. Human choices: preserve the opening route, extend using existing
grounded and shallow suspended modules, reduce the default overlay to 350x52
pixels, and keep detailed controls behind Controls/pause.

The foreground water source's opaque content ends below v=0.242. A mesh covering
v=0..0.25 preserves world/UV registration and a filtering margin while omitting
75% empty quad area. The wave shader and source images are unchanged. The existing
takeoff drawing is selected immediately on grounded jump input; no character art
or jump-force constants changed. Actual Windows preview screenshots, the render
comparison and measurement limitations are recorded in
docs/design/background-study/extended-prototype/README.md. The earlier shader
branch experiment was discarded after showing no measured benefit.

## Background study review corrections - 2026-10-31

Six unused historical paintings (connected-river, middle-cutout, near-cutout,
painted-terrain, meadow-surface and hanging-foliage) now live under
Assets/Game/Art/BackgroundStudyArchive/ rather than Resources. Images, original
GUIDs, metadata and generation/license records are preserved. Current locations
and SHA-256 hashes are indexed in docs/design/background-study/historical-asset-locations.json.
The earlier grouped meadow/foliage generation records remain historical; only
foreground-river from that group is part of the selected runtime.

Active artwork and texture importer settings are unchanged. Mipmaps remain off
for the fixed orthographic alpha-cutout art pending a separate coverage-aware
minification review. This avoids changing grass/leaf contours and adding mip
storage in this correction; it is not a measured performance improvement.
Geometry/camera/parallax now use the placed course coordinate frame so translated
and uniformly scaled courses retain visual/collider contact and stable water UVs.
No new artwork, animation, traversal mechanic or source-pixel edit was introduced.
See docs/design/background-study/review-2026-10-31.md for tests and limitations.

## Stomp prototype greybox - 2026-10-31

- Source: original runtime rectangles, labels and impact marker in `StompArena.cs`; authored by Codex (GPT-6), no image generator or external asset source.
- License: repository project license. No third-party artwork was added. Existing MikaOriginal keeps its existing provenance.
- Human direction: issue #12 requests a small greybox head-jump encounter with readable anticipation/attack/recovery; exact rules and difficulty require user review.
- Creative choices proposed for review: stationary anonymous target, striped warning/solid ground attack, white exposed head, restrained impact flash and short damage protection.
- Evidence: `docs/design/stomp-prototype/{anticipation,attack,recovery}.png`, generated from the actual Unity runtime arena by `StompReview.Record`. These are review screenshots, not production background/character assets.

- Damage feedback revision (2026-10-31): user selected recoil without teleport and a smooth half-opacity protection pulse. No source art changed. `damage-{left,right,closed-head,ground}.gif` and corresponding CSVs were captured from Unity local-scene physics by `StompReview.Record`; FFmpeg encodes the generated frame sequence at its recorded speed. Loop resets are demonstration boundaries.

- Moving-head/feedback revision (2026-10-31): user selected a moving vulnerable head, a brief hit flash and a discreet tone, excluding new heart-loss/health UI. The bounded sinusoidal patrol, original sprite flash shader and 120 ms softly enveloped descending sine tone were authored with Codex (GPT-6), under the repository project license. No external audio, image generator or source-art edits are used. `damage-moving-head.gif`/CSV capture actual Unity local-scene physics; `damage-tone.wav` exports the generated runtime waveform at the configured source volume. Exact tuning and final encounter acceptance remain provisional.

- Damage-hop/contact-star revision (2026-10-31): the user rejected the lateral recoil and first sine tone, selected a vertical hop preserving running, and approved a small cream impact star with sparks and a reduced white flash. The star is original native six-point mesh geometry in StompArena, authored with Codex (GPT-6), under the repository project license; no raster generator or external asset is used. New contact-star/run-left/run-right evidence comes from actual Unity local-scene physics.
- The three unselected `docs/design/stomp-prototype/sound-options/*.wav` previews are original deterministic filtered-noise synthesis from `tools/generate-stomp-sound-previews.py`, authored with Codex (GPT-6) on 2026-10-31 under the repository project license. No third-party samples or pitched voices are used. Human direction requested discreet alternatives without the rejected tone/pitch glide; selection remains pending and gameplay currently has no damage sound. Earlier `damage-tone.wav` is a rejected historical preview.
