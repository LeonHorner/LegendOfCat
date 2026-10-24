# Mika natural look study — revision 3

## Current scope

The user rejected both earlier Blender models, including their anatomy, colors,
eyes and ears. The new direction is to prioritize the cat's appearance against
the supplied concept art and add bones only after the visible model works.
The user accepted this version's appearance on 2026-10-24 and requested a
work-in-progress push for [issue #49](https://github.com/LeonHorner/LegendOfCat/issues/49).
This file is an **unrigged visual study**, not an animation-ready Unity asset.

Open `mika-look.blend` in Blender 5.2.3 LTS. The model, source PBR textures,
editable ginger/cream material graph, studio and two concept references are
packed into the file. The default view uses the review camera. `preview-side.png`,
`preview-three-quarter.png` and `preview-face.png` are actual Blender renders.

## New approach and source

This revision uses a detailed cat anatomy base instead of extending the rejected
procedural volume assemblies. The base mesh and fur textures are **Grab3D's Tabby
Cat**, released under CC0 1.0:

- Model: https://grab3d.com/models/animals/cat/
- Original mesh: `source/tabby-cat.glb`
- Original rights/AI disclosure: `source/LICENSE.txt`
- Original metadata: `source/manifest.json`
- License: https://creativecommons.org/publicdomain/zero/1.0/legalcode

Grab3D states that the source mesh/textures were generated using Meshy and then
cleaned up, scaled, textured and checked in Blender. The exact generation model
and prompt are not supplied. This is an adaptation of that source, not a claim
that Codex hand-sculpted the anatomy or generated it from the user's concept.

Local changes: a ginger and cream coat based on the concept palette; preserved
green eyes and skin color through shader masks; a slightly narrower trunk; a
consistent review studio and side/face views. The underlying fur detail and natural
facial structure come from the CC0 source. No image is painted over the render.
The model remains editable from every angle.

## Accepted appearance and remaining work

Appearance acceptance covers the face, silhouette and coat shown in the renders.
The source is a dense static mesh in a walking pose with the head turned slightly.
It contains 765,012 triangles and is a visual sculpt/reference, not game-ready
joint topology. Retopology, neutral-pose preparation, eye separation where needed,
skinning, skeleton, authored motion and Unity tests remain unfinished.
Normal/slowed motion review and actual-game acceptance are still required.
There is intentionally no FBX labeled animation-ready and no skeleton in this file.
The existing game character is unchanged.

## Reproduction

Run from the repository root:

```sh
blender --background --disable-autoexec --python art/blender/mika-v3/create_mika.py
```

This imports the documented source GLB, applies the reversible material edits,
packs the known concept references, saves the Blender file and renders the three
views. It overwrites only this version's generated outputs. The source license
and asset-provenance record must remain alongside redistributed copies.
