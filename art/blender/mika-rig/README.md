# Mika rig and motion candidates

Review-only continuation of the accepted [v3 look study](../mika-v3/README.md)
for [issue #49](https://github.com/LeonHorner/LegendOfCat/issues/49).
The accepted source file is unchanged. On 2026-11-02 the user approved publishing
the current running revision and making PR #51 ready as a Blender authoring
deliverable. This does not establish acceptance of every action or actual-game
integration. The playable 2D character is unchanged. Follow-up
[issue #78](https://github.com/LeonHorner/LegendOfCat/issues/78) tracks motion
refinement, varied runs and random selection, walking, idle animations and
Unity/gameplay integration.

The user rejected the first running candidate on 2026-11-01 as unnatural and
found revision 2 better, but requested stronger whole-body movement to match the
paired-leg run. Revision 3 adds a pronounced gather/extension cycle: the spine
arches as the hips tuck forward, lengthens on push-off, and the body pitches and
rises/falls through support and flight. The neck counter-rotates to steady the
head, and the tail follows the pelvis. Supported paws constrain body height;
recovering paws stay within limb reach. The model and other 12 actions are unchanged.

Review `run-v3-normal.gif`, `run-v3-slow.gif` and the matching
`run-oblique-*.gif` views. The user approved publishing this revision as the follow-up starting point.
Revision 2 remains in Git history at `97ee6b1`; the first loop is at `615a775`.
The user-supplied running-cat GIF was inspected locally for the sequence of
extension, forelimb support, spinal gathering and hindlimb push-off. Its creator
and redistribution license are unverified, so it is not included in the repository.

### Tail revision 4 — local review, 2026-11-02

The user found the published running tail too stiff. This revision increases
flexion progressively along the tail and delays each joint relative to its
parent, with a small sideways follow-through. The root compensates for pelvis
pitch while the tip continues its motion after the base turns. The run's body,
head and paw curves, skin/weights and other 12 actions are preserved.
Review `run-v4-normal.gif` and `run-v4-slow.gif`; the revision 3 side loops remain
available for comparison. The three-quarter previews show revision 4.
Native comparisons confirm all non-tail curves and skin/weights are identical.
The tail loop closes, and all five tail-joint positions are checked through
every half-frame of the exported run. For a run-only render refresh, use
`python art/blender/mika-rig/assemble_review.py --run-only`.
This local change awaits visual review before publication. Prior PR validation
applies to the published revision, not these new animation files.

## Files and use

- `mika-rig.blend`: editable skin, 25 deform bones, four paw IK controls, four
  knee/elbow pole controls and 13 keyed actions. Select `Mika_Rig` in Pose mode;
  select an action in the Action Editor. Muted NLA tracks preserve the actions.
- `mika-rig.fbx`: only the skinned character and 25 deform bones; control bones,
  cameras, studio floor and the hidden dense reference are excluded. Constraints
  are baked to animation keys. Base color and tangent normal textures are embedded
  and also provided as PNG files. Blender uses linear skinning to match export.
- `pose-*.png`, `*-normal.gif`, `*-slow.gif`: actual Blender renders for review.
- `validation.json`: native contact measurements and FBX reimport checks, not a
  Unity test result or artwork acceptance.

The character is approximately 0.49 m tall, authored in meters, facing Blender
+X. FBX uses Y up and -Z forward export axes; inspect the imported orientation
before attaching it to a controller. In Unity use a **Generic** rig, import
animation, preserve the model scale, use four bone influences, and inspect the
13 imported clips. Assign `mika-basecolor.png` to the material and import
`mika-normal.png` as a normal map. Do not import the dense reference or use the
`.blend` as the Unity delivery asset. This avoids a Blender dependency on game
build machines.

| Action | Frames at 24 fps | Review purpose |
| --- | ---: | --- |
| Idle | 48 | Restrained breathing, open eyes |
| LookBack | 72 | Neck/head turn with planted support |
| Listen | 48 | Ear rotation and small head tilt |
| TailTwitch | 48 | Distal tail motion |
| Groom | 96 | One raised forepaw, three support paws |
| Stretch | 72 | Lowered chest, forward forepaws |
| Run | 12 | Revision 3; 0.62 m virtual travel per cycle; half-frame keys |
| Takeoff / Airborne / Fall / Land / Stop | 12 / 24 / 12 / 12 / 16 | Transition building blocks |
| JumpReview | 48 | Authoring arc diagnostic; not a gameplay root-motion clip |

## Reproduction

Use Blender 5.2.3 LTS from the repository root. The scripts use
`/tmp/mika-authoring` for intermediate surfaces and rendered frames. They do not
modify scenes, prefabs, project settings or game code.

```sh
ALSOFT_DRIVERS=null blender --background --disable-autoexec --python-exit-code 1 --python art/blender/mika-rig/prepare_surface.py
ALSOFT_DRIVERS=null blender --background --disable-autoexec --python-exit-code 1 --python art/blender/mika-rig/build_rig.py
ALSOFT_DRIVERS=null blender --background --disable-autoexec --python-exit-code 1 --python art/blender/mika-rig/export_verify.py
ALSOFT_DRIVERS=null blender --background --disable-autoexec --python-exit-code 1 --python art/blender/mika-rig/render_review.py
python art/blender/mika-rig/assemble_review.py
```

Preparation rebuilds a working duplicate with fine voxel cleanup, regular-quad
un-subdivision and nearest-surface fitting, then bakes the accepted material into
a new UV map. It has 29,518 vertices and 41,150 mixed-topology faces, including
17,882 quads. This is **not** hand-authored anatomical retopology. Quadriflow
cancelled on this source; it is not used or reported as successful. Heat weights
are constrained at the limb attachments so a lifted paw does not drag the belly.
The hidden original remains available for comparison in the native file.

## Current validation and remaining work

On 2026-11-01, native weight normalization/four-influence checks and FBX reimport
passed: one mesh, 25 deform bones and all 13 actions. Sampled baked bone positions
matched native positions within 0.000001 m. The running support soles stayed
within 0.00031 m of the floor; support wrist displacement with 0.62 m/cycle virtual
travel was below 0.000001 m per half-frame sample. The running pose matrices match
at both loop endpoints. A native-file comparison also confirmed identical skin
coordinates/weights and all 12 non-run actions relative to revision 2. These
measurements are at authoring scale; inspect the full motion and actual game scale before acceptance.

Remaining: visual review of normal/slowed loops, shoulder/underarm deformation
refinement where needed, natural gait and action-transition review, actual Unity
import/material/clip validation, cosmetic interruption and grounded-stop behavior,
and accepted 2D output integration with the existing controller. The dense source
has fused/irregular anatomy at the armpits; weight cleanup alone does not establish
production deformation quality. The existing dark underarm marking is present in
the accepted source, rather than an export hole.

Unity 6000.3.27f2 is installed locally, but batch import stopped because no Editor
license was active. Unity tests and Windows/Linux player builds are unrun. Keep
issue #49 open; issue #78 carries the remaining animation and integration work.
PR #51 is ready for the repository's exact-head merge validation.

Source: CC0 Grab3D Tabby Cat, AI-assisted Meshy base as disclosed in the preserved
v3 source manifest. Local surface preparation, UV/texture bake, rig and keyed
motion were authored with Codex and Blender on 2026-10-31. No new image generator
or third-party character was used. Human direction prioritizes the accepted
natural cat appearance, open eyes, grounded support and editable Unity animation.
See `ASSETS.md` and the v3 source license/manifest for provenance.

Revision 2 reference reading: [Bertram and Gutmann, gallop mechanics](https://pmc.ncbi.nlm.nih.gov/articles/PMC2696142/)
and [cat gallop coordination](https://www.sciencedirect.com/science/article/pii/S0091677377922799).
These informed phase structure; the motion curves are locally authored artistic
choices, not measured cat motion, copied footage or a validated biomechanics model.
