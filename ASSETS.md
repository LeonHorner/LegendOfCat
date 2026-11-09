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

## Mika natural look study, revision 3 — 2026-10-24

- Files: `art/blender/mika-v3/`, including the original source GLB, native Blender
  look study, editable material authoring script and actual Blender review renders.
- Human direction: both prior Blender revisions were rejected. Prioritize a
  natural-looking cat matching the concept art, especially anatomy, coat, eyes
  and ears; add bones later after the appearance works.
- Third-party source: Grab3D, **Tabby Cat**,
  https://grab3d.com/models/animals/cat/ (downloaded 2026-10-24).
  Source mesh and all source textures are dedicated to CC0 1.0. The original
  `source/LICENSE.txt` and `source/manifest.json` are preserved verbatim.
  The CC0 dedication permits modification and public source redistribution.
- Generator/model disclosure: Grab3D identifies an AI-assisted Meshy image-to-3D
  base, followed by cleanup, scaling, texturing and checking in Blender. Exact
  model revision and prompt are not provided. This source is not represented as
  original Codex sculpture or as a generation made from the user's concepts.
- Local work: Codex/Blender 5.2.3 LTS material adaptation to ginger and cream,
  preserving source facial colors and fur detail; slightly narrower trunk; packed
  project concept references and studio cameras for full-body and face review.
  Known concept-image provenance remains as recorded in the earlier entries.
- Review: the user accepted the visible model on 2026-10-24 and requested a
  work-in-progress push linked to issue #49. This acceptance covers appearance
  only. Production topology, skeleton, animation, Unity validation and gameplay
  integration remain unfinished. Previous Blender models remain rejected.

## Mika Blender rig and motion candidates — 2026-10-31

- Files: `art/blender/mika-rig/`, including the native rig, baked FBX, texture
  maps, authoring/verification scripts and actual Blender motion review renders.
- Source and license: the accepted revision 3 Grab3D Tabby Cat adaptation above;
  its preserved CC0 license and Meshy AI-assisted source disclosure still apply.
  No new external character or image generator was used.
- Authoring: Codex and Blender 5.2.3 LTS, 2026-10-31. A working duplicate received
  voxel cleanup, regular-quad un-subdivision, surface fitting, UV/texture baking,
  skeleton, constrained heat weights, paw IK and keyed motion candidates. The
  accepted dense look study remains unchanged. The working mesh has mixed
  topology; it is not represented as anatomical hand retopology.
- Human decisions: retain the accepted natural appearance, open eyes, four
  resting paw contacts, independent ear/tail motion and editable Unity export.
  Normal/slowed loops are rendered from the actual rig, not generated motion
  illustrations. The inherited dark underarm marking remains visible.
- Validation: normalized four-influence weights, native support measurements and
  FBX reimport passed. Motion appearance, Unity import/materials and game
  integration remain unaccepted/unverified. See the folder README and validation
  report. Keep issue #49 incomplete and the existing 2D playable character intact.
- Revision 2, 2026-11-01: the user rejected the first gait as unnatural. Codex and
  Blender reauthored the run with pelvis/spine/shoulder coordination, forward
  head/neck alignment, staggered contacts, gathered/extended recovery and a
  trailing tail. New side/three-quarter loops are actual Blender renders. The
  same CC0 source and editable skin are retained; no new generated imagery or
  copied motion footage is used. Reference reading is linked in the folder
  README. Visual acceptance and Unity validation remain pending.

- Revision 3, 2026-11-01: human feedback requested stronger body movement for the
  paired-leg run. Codex and Blender 5.2.3 LTS authored increased spinal flexion,
  pelvic tuck/extension, body pitch/heave and tail follow-through on the same
  CC0-derived skin. Local motion reference: user-supplied
  `tumblr_n0r2iuDJRW1sg7h2jo1_1280-2378486781.gif`; creator and license unverified.
  This reference is not redistributed, embedded or used as a texture. No motion
  capture data was extracted; the animation curves are locally authored. Actual
  Blender renders show the revised candidate. Visual acceptance remains pending.

- Publication decision, 2026-11-02: the user approved pushing the current rig/run
  and moving PR #51 out of draft. Issue #78 records further motion improvements,
  varied runs with random selection, walking, idle animations and integration.
  Existing source, license and generator disclosures remain unchanged. This is
  approval of the authoring deliverable, not a Unity/gameplay validation result.

- Tail revision 4, 2026-11-02: the user found the running tail too stiff. Codex
  and Blender 5.2.3 LTS authored progressively delayed joint flexion and subtle
  sideways follow-through. Only the Run tail curves changed; the same CC0-derived
  skin and textures are used. No external artwork or new generation was used.
  The user reviewed the actual Blender previews and approved adding the tail
  refinement to the original PR on 2026-11-02.

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

- Cat damage vocal candidate (2026-10-31): **Angry Cat Meow** by **bevibeldesign**, source https://freesound.org/people/bevibeldesign/sounds/350411/, source license **CC0 1.0**, https://creativecommons.org/publicdomain/zero/1.0/. Original is a performed pitch-bent human voice; no AI generator/model is involved. The public HQ MP3 preview and source/derived SHA256 hashes are retained in `docs/design/stomp-prototype/sound-options/cat-reaction/`. Codex edited a 420 ms excerpt with short fades and restrained peak level, with no further pitch change; reproduce using `tools/prepare-cat-reaction-preview.py`. Human direction rejected the three abstract noise proposals and satisfied/attention-seeking meow, requesting an abrupt, surprised, slightly rough damage cry. This vocal candidate is unselected and not enabled in gameplay; subjective approval remains pending.

- Real-cat vocal auditions (2026-10-31): **Cat Angry Meow.WAV** by **softcoresoft**, https://freesound.org/people/softcoresoft/sounds/327438/, licensed **CC BY 4.0**, https://creativecommons.org/licenses/by/4.0/. Actual cat hissing/meowing per creator; no AI generator/model. Public HQ source preview retained in `docs/design/stomp-prototype/sound-options/real-cat/` with hashes/attribution. Codex prepared source seconds 0.9-2.1 as a natural-speed 1.2-second excerpt and a pitch-preserving 4/3-tempo 0.9-second edit; mono PCM16/48 kHz, short fades and gain 0.65. Reproduce via `tools/prepare-real-cat-preview.py`. Human direction rejected the previous human-voice clip as too short/not cat-like and requested a real-cat voice with a longer recognizable ending. Both new auditions remain unselected and are not enabled in gameplay; attribution and modification disclosure must accompany redistribution.

- Selected damage voice (2026-10-31): user chose the second real-cat audition, `real-cat/short-reaction.wav` (0.9 seconds), copied unchanged to `Assets/Game/Resources/StompDamageVoice.wav`. **Cat Angry Meow.WAV** by **softcoresoft**, source https://freesound.org/people/softcoresoft/sounds/327438/, **CC BY 4.0**, https://creativecommons.org/licenses/by/4.0/. Edits remain the source 0.9-2.1-second excerpt, pitch-preserving 4/3 tempo, mono PCM16/48 kHz, short fades and gain 0.65; gameplay volume is 0.5. Editor import preloads PCM. Attribution and edit disclosure are bundled in `Assets/StreamingAssets/THIRD-PARTY-NOTICES.txt` for player redistribution. Prior audition-only status is superseded for this selected clip; all rejected variants remain inactive.

- Combat-signal refinement (2026-10-31): user requested both a better contact effect and clearer warning/attack/open-head cues. Codex (GPT-6) authored original native mesh geometry with an irregular feathered cream core, warm rim and tapered brush strokes, plus closed-head crosses, open-head chevrons, pulsing diagonal hatching and active strike marks within the collision band. No raster generator, external art or new audio was used; existing source artwork and selected voice remain unchanged. Prototype code follows the repository code license. `damage-signals.gif`, `signals-small-grayscale.gif`, refreshed contact/movement captures and the grayscale recovery PNG derive from actual Unity camera renders; FFmpeg creates the scaled/grayscale review copies. These are review evidence, not accepted production boss art.

- Distinct enemy-hit feedback (2026-10-31): the user selected separate forms for player and enemy damage. Codex (GPT-6) authored an original native feathered annulus mesh for a flat expanding ring and a separate narrow head-edge overlay. The ring stays at the contact point; the overlay follows the head without changing closed-head marks. Existing warm player burst and selected real-cat voice remain unchanged. No new external asset, image generator or audio is introduced; code follows the repository code license. Color/grayscale camera captures document both hit types and terminal fade for review. User also requested main-menu order Movement lab, then Simple combat lab.
- Review corrections (2026-11-01): original procedural cap/flash geometry now ends at the existing collision surface; ring placement uses the uncorrected swept contact and the moving head interval. Native Unity Game View captures include the responsive HUD and letterboxed arena at small/narrow window sizes; FFmpeg produces grayscale review copies. Authored with Codex (GPT-6), under the repository code license. No new source artwork, generated bitmap asset, external dependency or audio edit was introduced. Human direction requested exact contact alignment and small-window motion readability while preserving the selected damage behavior.
- Final-review readability pass (2026-11-01): the existing HUD title includes a compact phase hint in its original UI font, while world-space geometric signals stay unchanged. Pause is omitted on results. The `integration-review/` recordings are actual resized Unity Game View frames after normal main integration; FFmpeg encodes color/grayscale copies. Authored with Codex (GPT-6), under the repository code license. Human direction requested readable small-window labels without obstructing gameplay, preserving the approved artwork, audio and mechanics. No new art source, raster generator, font or audio edit was introduced.

## Golden River introduction variety - 2026-11-01

- Source: five original transparent PNG paintings generated with the built-in OpenAI image generation tool; exact model identifier was not exposed. No third-party reference image, font, brand or character was supplied.
- Runtime paths: `Assets/Game/Resources/GoldenRiver/{shelf-root,shelf-masonry,landmark-waterwheel,landmark-viaduct,landmark-willow}.png`. Generated source pixels and alpha are preserved; Unity imports clamp edges, disable mipmaps and use uncompressed textures with a 2048 maximum dimension.
- License: project original-art policy in ASSET-LICENSE.md (CC BY-NC 4.0 only to the extent project rights holders own applicable rights; no copyright claim is created for otherwise unprotected AI output). No additional third-party dependency or license.
- Human creative direction: the user measured the earlier introduction at 61 seconds but rejected it as very boring, requesting different platform heights/sizes, different floating-platform graphics and significant unique background locations. This authorizes this bounded gameplay/visual iteration, not final art acceptance or completion of #73.
- Creative implementation: rooted slate island and broken masonry shelf have distinct silhouettes and registered landing contacts, with their natural roughly 3:1 source proportions retained. One abandoned watermill, one broken aqueduct and one ancient willow mark separate sections; each is instantiated once with absolute anchor-based parallax, without tiling or mirrored duplicates. Atmospheric tint/saturation keep background landmarks behind the crisp playable plane. Existing bank, shelf, valley, water and Mika provenance remains applicable.
- Exact generation prompts: `docs/design/golden-river-main-route/variety-prompts.md`. `GoldenRiverReview.Record` captures actual Unity course views at 960x540 and 640x360, plus grayscale versions of the smaller renders; these static authoring captures are not player-input recordings.

### Golden River composition refinement - 2026-11-01

The user reported 60.7 seconds and rejected pasted/cut-out landmark boundaries,
mixed styles within floating sequences and the pale willow. The existing five
generated source PNGs remain unchanged. Original shader/code changes preserve
painted landmark partial alpha, apply per-landmark brightness/fog and assign one
art family per connected floating sequence. The willow is darker and retains more
source color instead of pale atmospheric mixing. No new image generator request,
source-pixel edit or third-party asset is involved. Existing generation prompts,
model disclosure and original-art license caveat above remain authoritative.

### Landmark base transitions and lower crossings - 2026-11-01

The user circled remaining lower mill mist/ground patches and requested the same
correction across the other landmark paintings, together with lower floating
supports near the water. All three landmark materials now use the same local
source-UV base/side alpha transition. The five original source PNGs and their
license/generation disclosures are unchanged. Geometry lowers two existing
suspended sequences; registered platform art follows their unchanged colliders'
widths. No new generated or third-party image is involved. Review captures now
also cover lower boundaries and the two low crossings.

## Supported Golden River landmark hills - 2026-11-01

- Source: built-in OpenAI image generation EDIT tool, two separate edit passes per landmark (six calls total); exact model identifier not exposed. First pass referenced the original project landmark; second pass referenced its intermediate solid-hill composition. Final source PNGs retain generated pixels/alpha without manual raster processing.
- Active files: `Assets/Game/Resources/GoldenRiver/landmark-{waterwheel,viaduct,willow}-hill-v2.png`. The two shelf PNGs keep their existing paths and provenance. Original isolated landmark PNGs and their GUID-bearing metadata are retained under `Assets/Game/Art/GoldenRiverArchive/`, moved with Unity AssetDatabase and excluded from player packages. The intermediate edit outputs are not runtime assets.
- License: the same original-art policy in ASSET-LICENSE.md, CC BY-NC 4.0 only to the extent project rights holders own applicable rights; no copyright claim in otherwise unprotected AI output. No third-party artwork or license was introduced.
- Human creative direction: the user rejected the latest mist/opacity blend as stranger than before and requested a real ground plane under the landmark, descending out of the image like a hill without an abrupt or blurred edge. This supersedes the local base/side feathering approach above.
- Creative decisions: paint complete rounded supporting hills in the same layer as each landmark, expand the surrounding landscape/headroom, retain recognizable landmark identity, render the source's natural grass/stone silhouette with the existing cutout policy, and position the solid lower canvas below the camera. Remove the experimental opacity feather and partial-alpha halo path; keep existing brightness/fog grading, reversible unique anchors, independent foreground river and gameplay geometry.
- Full prompts: `docs/design/golden-river-main-route/hill-prompts.md`. Final edited composition remains subject to in-game review. Static Unity center/entry/exit/low-crossing captures document the proposed result, not final acceptance.

## Continuous Golden River scenery and coherent terrain variants - 2026-11-02

- Source: seven original outputs from the built-in OpenAI image generation tool, exact model identifier not exposed. Two new-generation calls created valley/canopy panoramas; five edit calls referenced the existing original `shelf-root.png` or `shelf-masonry.png` to preserve material/lighting while authoring different grounded/compact shapes. No third-party image or character was supplied. Generated source pixels/alpha are preserved.
- Active additions under `Assets/Game/Resources/GoldenRiver/`: `valley-panorama-v3.png`, `canopy-panorama-v3.png`, `bank-{narrow,medium,wide}-v3.png`, `shelf-root-compact-v3.png`, `shelf-masonry-compact-v3.png`. Existing shelves/hill paintings and their provenance remain applicable. Unity-generated .meta files and importer policy accompany the additions.
- License: existing original-art policy in ASSET-LICENSE.md, CC BY-NC 4.0 only to the extent project rights holders own applicable rights, without asserting copyright in otherwise unprotected AI output. No third-party license or dependency added.
- Human direction: remove conspicuous scenery repetition before the clouds, match suspended platforms to grounded ones and stop relying only on scaled copies. Creative choices: two continuous asymmetric natural panoramas without repeating castles, three grounded rock structures matched to span widths, two stout compact suspended forms, common slate/limestone/grass/light palette and preservation of material families across each connected crossing. Existing clouds, foreground water, supported landmark hills and gameplay remain unchanged.
- Prompts/reference roles: `docs/design/golden-river-main-route/continuity-prompts.md`. Capture utility includes earlier problem locations, opening banks and late/exit panorama coverage. Easter egg ideas are recorded as proposals only; none was embedded into this asset set.

## Unique ruins and decorative easter eggs - 2026-11-02

- Source: built-in OpenAI image generation tool, exact model ID not exposed. Two edits restore different original ruins in the new panoramas, four new-generation calls create the original cat king/fish pair/noticeboard/cat-rock, and two follow-up edits enlarge the keep and replace the physically implausible thin-edge sleeping-cat sign with a broad roof. Intermediate candidates are not runtime assets. Generated final pixels/alpha are preserved.
- Active additions: `Assets/Game/Resources/GoldenRiver/egg-{cat-king,watching-fish,no-cats-sign,cat-rock}.png`; final panorama paths remain valley-panorama-v3.png/canopy-panorama-v3.png. No borrowed character, brand, text font file or other third-party image was used.
- Human direction: preserve cool ruins as unique different structures, allow one larger ruin, implement each of the four suggested jokes once, distribute them beyond the intro, make the sleeping cat physically supported and let the fish drift in water instead of remaining static.
- Creative implementation: unique keep battlement statue, independently bobbing/tilting fish sharing a single pair texture with separate UV regions, broad two-post rain-roof noticeboard with exact "No cats" text, and subtle boulder pareidolia embedded in the final bank. Fish bodies are occluded by the existing river. Decorative movement/placement code is original repository code; no new gameplay, music, sound, collectibles or durable data.
- License: existing original-art policy in ASSET-LICENSE.md (CC BY-NC 4.0 only to the extent applicable rights are owned; no copyright claim is created for unprotected AI output). Full prompts, edit references and rejected sign correction: `docs/design/golden-river-main-route/ruin-egg-prompts.md`. User visual acceptance remains pending.

2026-11-02: valley-panorama-v3.png revised using the built-in image-generation tool (model identifier not exposed), two edits from the existing original generated panorama. Original generated project artwork; no third-party source. Human direction: replace the rejected sharp floating cat-king cutout with a smaller stone carving supported by the castle parapet, sharing painted lighting and depth. Exact prompts: docs/design/golden-river-main-route/integrated-king-prompt.md. Rejected cutout archived with its Unity metadata.

2026-11-02: egg-watching-fish.png replaced with a built-in image-generation edit of the original rounded fish pair (model identifier not exposed). Human direction: retain round blue-grey and gold characters; horizontal swimming pose, smaller eyes, bodies below the water with turquoise attenuation. Slender trout experiment rejected and unused. Exact prompt: docs/design/golden-river-main-route/round-fish-prompt.md. Original generated artwork; no third-party sources.

2026-11-02 original-style correction: restored bank-wide-v3 from the original painted-bank-v2 without pixel changes; regenerated narrow/medium bank geology and the valley from original painted style references using the built-in image tool. The rejected pale limestone/flat mountain direction is superseded. Added river-stones-slab-v1 as an edit of the original foreground river, with a long flat slab, split angular stones and pebbles. Model identifier not exposed; original generated sources, no third-party inputs. Exact prompts and human decisions: docs/design/golden-river-main-route/original-style-prompts.md.

The same original-style restoration also repaints both long and compact root/masonry shelves from their own silhouettes using painted-bank-v2 as the style reference (four built-in image edits). Their original contact heights and family silhouettes are retained; prompts are in original-style-prompts.md.

2026-11-02 water/statue correction: river-water-v2 and river-rock-groups-v1 are original built-in generated images; repeated water contains no stones and six separate painted groups are each used once. Prior repeating slab artwork archived through Unity with metadata. A generated high-resolution stone cat study was vector-masked and composited onto the existing castle using PhotoCraft 0.19.0; sources/cat-king-composite.pcraft preserves the layers, with source/cutout PNGs alongside. The rejected full-panorama refinement is unused. Model identifier not exposed. Exact prompts and human composition decisions: docs/design/golden-river-main-route/water-and-statue-prompts.md.

2026-11-02: egg-no-cats-sign.png repainted with the built-in image tool using original painted-bank-v2 as the style reference, preserving the supporting roof, sleeping cat and exact text. Original generated assets, model identifier not exposed; prompt and creative decisions in docs/design/golden-river-main-route/sign-style-prompt.md. PhotoCraft layered statue source also removes residual old foreleg pixels behind the new carving; the remainder of the panorama stays intact.
2026-11-03. river-rock-groups-v2.png adds six unique river formations generated with the built-in image tool. Original project art. The six additional motifs are a broken ring, stepped strata, tilted blade, double shelf, uneven slabs and fractured boulder. Each motif has one fixed placement.
The standalone cat rock was archived with its Unity metadata. The statue was composited in PhotoCraft from its original cutout at twice the previous resolution, on a 4344 by 1448 valley canvas.
2026-11-03. river-rock-groups-v3.png adds six more individual river formations, created with the built-in image generator as original project artwork. The new locations, sizes, water depths and atlas cell bounds were selected by hand. PhotoCraft vector masks remove sky pixels from the statue cutout before compositing it onto the parapet.
2026-11-03. PhotoCraft vector masks remove detached paint specks from the Golden River bank and shelf textures. The masks target isolated alpha regions and preserve the attached terrain and larger flowers.

2026-11-03. far-valley-panorama-v4.png is an original built-in Imagegen edit of canopy-panorama-v3. Human direction sets three skyline shapes and valley fog, with the former canopy archived. The game positions the new painting higher behind the near valley, and cools its atmospheric color. Exact prompt:

> Edit this transparent 3:1 painted game panorama into a distant atmospheric landscape layer with fully transparent sky above the silhouette. Preserve canvas and opaque coverage at bottom. Shift the main skyline upward into the middle third so distant ridges remain visible behind a nearer forest. At 25–37% across, a densely forested tall ridge with irregular treetops and warm golden rim. At 40–54% across, a broad open valley notch framed by a pair of receding blue-violet ridges, the biggest open distant vista. At 55–70% across, overlapping lower rounded hills with alternating woodland and open slopes. Smoothly connect the three views, soft broad color masses and varied silhouettes, cool blue lavender distance, warm subtle sunset light, mist collected low in valleys, distinct crisp ridge contours, original storybook brushwork. No buildings, no repeated mountains, no white or black sky fill, no frames or text.

## Golden River ascent — 2026-11-04
Two original PNGs, landmark-hillside-fortress-v1 and shelf-ascent-masonry-v1, generated with the built-in OpenAI image generation tool. Exact model identifier was not exposed. No third-party artwork was used. License follows ASSET-LICENSE.md and its original/generated-art rights caveat. Human direction approved the original painted blue-violet rock, golden foliage and teal ivy style, unique supported ruins and varied platform shapes. The fortress has a continuous opaque hillside below its ruins. The shelf uses a cropped left UV margin to exclude the raised buttress from its standable silhouette. Exact prompts are in docs/design/golden-river-main-route/ascent-prompts.md. Original code supplies platform guides and deterministic motion. Existing source provenance remains applicable. Visual acceptance and human timing remain pending.

The fortress received one additional built-in image edit to fit its entire silhouette inside the canvas and remove the clipped left boundary. The final PNG replaces the intermediate at the same path; its Unity GUID is preserved.

## Ascent preview corrections — 2026-11-04
The user rejected clipped shelf ends, bare guides and stretched cliff banks. The built-in OpenAI image generation tool edited the original generated masonry shelf to create a complete broken end without the raised buttress. A new original tall cliff source was generated for high banks. Exact model identifier is not exposed. Existing ASSET-LICENSE.md original/generated-art policy applies. Human direction preserves golden foliage, blue-violet rock and teal ivy. Full prompts are appended to ascent-prompts.md. Shader grading retains more aqueduct/willow color without brightening the willow. Physics changes and removal of debug-like guides introduce no external asset or dependency.

Final tall-bank source bank-tall-ascent-v1.png is 1060x1484. The renderer retains its native aspect ratio and anchors the authored grass row to the collision surface; the full rocky foot remains below the foreground river. It is used only for high banks after the intro. One late narrow high bank was widened to preserve a submerged base at native proportions. The corrected shelf uses full UVs; the earlier cropped-left treatment is superseded. Bare platform guide lines are removed following user rejection.

## Continuous ascent landscape and matching cliff rim — 2026-11-04
Five built-in OpenAI image-generation edits (model identifier not exposed): two extended landscape compositions, two localized join repairs and one tall-bank vegetation correction. Only original project images were supplied. PhotoCraft 0.19.0 resized the extension studies, masked their generated left halves, preserved the original intro layers, aligned copied layers, and evaluated localized painted join studies. The final two-layer composite keeps the original intro over the continuous extension study with a narrow 32-source-pixel edge transition beyond the intro framing; the localized patch studies were excluded after render review. Layered sources are near-extended-panorama.pcraft and far-extended-panorama.pcraft under docs/design/golden-river-main-route/sources. Final PNGs are near-extended-panorama-v1 (8688x1448) and far-extended-panorama-v1 (4344x724). Unity imports the wider panorama with an 8192 cap; landscape height and original intro texel density are retained. Original source panoramas remain preserved. The tall rim edit uses bank-wide-v3 as the grass/ivy style reference, with detached upper paint specks masked in PhotoCraft. Exact prompts are in ascent-prompts.md. Existing ASSET-LICENSE.md original/generated-art rights caveat applies; no external licensed asset was introduced. Human decisions require continuous background motion, solid platforms, a matching leafy rim and removal of the easy crossing's catch shelf.

## Shelf collision contours — 2026-11-04
Codex authored original native polygon geometry against the five existing root/masonry shelf paintings. Contours follow the load-bearing stone body, excluding hanging foliage and roots, and share the exact rendered source registration. No bitmap pixels, character drawings, third-party asset or image-generation request were introduced. Human direction requested silhouette-matched hitboxes, smoother camera motion and correction of an edge-induced airborne hold. Geometry/code follow the repository code license; existing painting provenance remains applicable.
# Golden River checkpoint landmarks — 2026-11-05

Original generated artwork: `Assets/Game/Resources/GoldenRiver/checkpoint-states-v1.png`, `checkpoint-tree-v1.png`, and `checkpoint-arch-v1.png`. Created with the built-in image generation tool; underlying model identifier not exposed. No external asset/license dependency; generated for this repository and public redistribution. Human decisions: distinct tree/ruin checkpoint settings, clean transparent cutouts, complete grounded feet, blue-violet stone and golden foliage; dark unvisited paw, warm glowing current paw, dark burnt-orange passed paw without emission. The user rejected the initial pale passed state. Final sprite atlas removes the intermediate white halo. Runtime shader adds restrained engraving-only pulsing, no external bloom. Prompt specifications and generation/edit decisions: [checkpoint authoring](docs/design/golden-river-main-route/checkpoints-and-pacing.md).

2026-11-05 checkpoint grounding correction: original code-authored, UV-registered grass overlap meshes reuse the corresponding existing platform painting/material without editing bitmap pixels. Stone, tree roots and arch feet are embedded 0.08 world units. No new generator, third-party asset or license dependency. Human direction: visible contact should activate the checkpoint, with vegetation hiding the pasted-looking bottom seams.

2026-11-05 checkpoint activation presentation: original code-authored shader interpolation and fourteen procedural soft radial spark quads reuse the existing checkpoint atlas. Human direction: gradually brighten only the touched checkpoint and release warm rising sparks; keep the distant passed marker change immediate. The fixed stone silhouette is preserved while engraving colors blend. No new bitmap generation or third-party asset/license dependency.
# Golden River finale paintings — 2026-11-08

Original generated project artwork made with the built-in OpenAI image generation tool (exact underlying model ID not exposed), with no third-party sources or additional third-party redistribution restriction. Files in `Assets/Game/Resources/GoldenRiver`: `finale-wall-v1.png`, `finale-pier-v1.png`, `finale-garden-v1.png`, `finale-hillside-v1.png`, `finale-gate-v1.png`, `finale-guide-v1.png` and `finale-vertical-guide-v1.png`. Two additional original generated landscape strips extend the existing near/far panoramas through PhotoCraft native compositions in `docs/design/golden-river-main-route/sources/finale-{near,far}-panorama.pcraft`.

Human decisions: retain painted golden grass/daisies, violet stone and deep teal ivy; author distinct wall, compact pier and rooted garden forms; remove raised wall stumps from playable tops; create one supported hillside ruin and a distinct final pointed gate; give moving platforms credible painted counterweight frames; preserve complete feet, clean transparent openings and original intro/ascent registration. No new character artwork or Easter Eggs. Final prompt specifications, extraction edits, contact rows, collision/footing choices, landscape composition and provenance are recorded in [Finale art](docs/design/golden-river-main-route/finale-art.md). Unity-generated metadata is preserved. This records asset origin, not final player acceptance.

Follow-up on 2026-11-08: `finale-rubble-v1.png` is an additional original built-in OpenAI image generation asset (exact model not exposed), with no third-party source or additional third-party redistribution restriction. Human direction rejected a floating grass-topped shelf on top of a terrace and instead selected a low, flat-based stack of blue-violet fallen masonry, small teal moss and warm edges, without a lawn, hanging underside, scenery or halo. It is integrated with a stepped solid contour and registered grass at its base. The full final prompt specification is in the linked Finale art record.


## Golden River watchtower detour � 2026-11-08

`Assets/Game/Resources/GoldenRiver/finale-watchtower-v1.png` is an original generated transparent painting made with the built-in OpenAI image generation tool (exact model identifier not exposed). Source output: `exec-c32edae8-5c5e-4fbe-9ab3-f8a6619a7ad6.png`. Existing `ASSET-LICENSE.md` generated-art policy applies; no external asset or licensed reference was supplied. Human creative direction: lengthen the measured 100-second finale through a collapsed tower with a short reversal, varied landings and local recovery, preserving golden vegetation, violet masonry and teal ivy. Full prompt and integration decisions are recorded in `docs/design/golden-river-main-route/finale-art.md`.

The existing near panorama is extended to 14332�1448 using an original generated right-hand continuation (`exec-85ff45e6-365c-4214-ace3-bd4154ece0d6.png`, built-in image generation, exact model not exposed) and PhotoCraft composition. The original project's terminal edge was the only reference; the original intro/ascent mapping is preserved. Editable source: `docs/design/golden-river-main-route/sources/watchtower-near-panorama.pcraft`. The generated-art policy above applies. Prompt, rejected low-ridge study and contour/ground decisions are documented in `finale-art.md`.


## Optional upper lookout composition - 2026-11-09

Issue #72 reuses the original generated shelf-masonry-compact-v3 and finale-gate-v1 paintings, with their existing provenance and ASSET-LICENSE.md terms. No new bitmap, external asset, generator call or model is introduced. Codex authored the bounded platform layout and placed the gate behind a solid lookout landing as temporary abandoned-arch scenery. Human direction comes from the approved optional exploration loop: a visibly discoverable environmental moment, safe rejoin, no collectible or new ability. Final art remains #73.


## Hidden ruin garden concept - 2026-11-09

Review-only garden-concept-v1.png under docs/design/golden-river-upper-route was created with the built-in OpenAI image generation tool (exact model not exposed), using only the original project gameplay screenshot as palette/material reference. Source: exec-036b1e6c-f7fe-4abb-a495-665b268e5a48.png. Existing ASSET-LICENSE.md generated-art terms apply; no third-party asset was used. The owner chose the hidden overgrown garden after rejecting the shelf chain; Codex proposed the tree/fountain courtyard and side-arcade return. Full prompt, composition observations and gameplay limitations are in garden-concept.md and garden-concept-prompt.txt beside the image. Not approved production artwork or collision evidence.
