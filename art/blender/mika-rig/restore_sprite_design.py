"""Recreate the original sprite palette and silhouette on the animated Mika rig."""
from pathlib import Path
import bpy
import numpy as np
from mathutils import Vector

folder = Path(__file__).resolve().parent
bpy.ops.wm.open_mainfile(filepath=str(folder / "mika-rig.blend"))
skin = bpy.data.objects["Mika_Skin"]
rig = bpy.data.objects["Mika_Rig"]
if skin.get("original_sprite_design"):
    raise SystemExit("Apply this script to the pre-redesign rig.")
source = bpy.data.images["Mika_BaseColor"]
pixels = np.empty(len(source.pixels), dtype=np.float32)
source.pixels.foreach_get(pixels)
rgba = pixels.reshape(-1, 4)
rgb = rgba[:, :3].copy()
r, g, b = rgb.T
# Preserve the green iris and pink ear accents. Map the dark tabby fur to
# warm copper stripes and the pale fur to the original sprite's ivory.
warm = (r > g * 1.08) & (g > b * 1.04)
luma = .2126*r + .7152*g + .0722*b
for channel, stops in enumerate(((.37, .66, .86, .99), (.15, .34, .55, .79), (.07, .15, .29, .56))):
    mapped = np.interp(luma, (0, .30, .60, 1), stops)
    rgba[warm, channel] = mapped[warm]
ivory = (luma > .48) & (r - b < .22) & (g - r < .04)
for channel, tint in enumerate((1.04, 1.015, .91)):
    rgba[ivory, channel] = np.minimum(1, rgb[ivory, channel] * tint)
source.pixels.foreach_set(pixels)
source.update()
source.filepath_raw = str(folder / "mika-basecolor.png")
source.file_format = "PNG"
source.save()
source.pack()

head = rig.data.bones["Head"].head_local
groups = {group.index: group.name for group in skin.vertex_groups}
for vertex in skin.data.vertices:
    influence = sum(group.weight for group in vertex.groups
                    if groups[group.group] in {"Head", "Ear.L", "Ear.R"})
    delta = vertex.co - head
    vertex.co += Vector((delta.x * .06, delta.y * .05, delta.z * .11)) * influence
skin.data.update()
skin["original_sprite_design"] = True
for material in skin.data.materials:
    for node in material.node_tree.nodes:
        if node.type == "NORMAL_MAP":
            node.inputs["Strength"].default_value = .12

scene = bpy.context.scene
scene.render.image_settings.file_format = "PNG"
bpy.ops.wm.save_as_mainfile(filepath=str(folder / "mika-rig.blend"))
rig.animation_data.action = bpy.data.actions["IdleRelaxed"]
scene.frame_set(1)
scene.camera.data.ortho_scale *= 1.24
scene.render.resolution_percentage = 65
scene.render.filepath = "/tmp/mika-sprite-design.png"
scene.cycles.samples = 12
bpy.ops.render.render(write_still=True)
