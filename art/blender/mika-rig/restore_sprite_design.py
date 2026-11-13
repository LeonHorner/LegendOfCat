"""Bake the original Mika sprite's sampled palette and restyle the rigged mesh."""
from pathlib import Path
from math import sin
import bpy
import numpy as np
from mathutils import Vector

folder = Path(__file__).resolve().parent
bpy.ops.wm.open_mainfile(filepath=str(folder / "mika-rig.blend"))
skin = bpy.data.objects["Mika_Skin"]
rig = bpy.data.objects["Mika_Rig"]
if skin.get("sprite_palette_v2"):
    raise SystemExit("Start from the pre-redesign rig.")
reference = bpy.data.images.load(str(folder.parents[2] / "Assets/Game/Art/Mika/mika-movement-candidate.png"))
width, height = reference.size
pixels = np.empty(len(reference.pixels), dtype=np.float32)
reference.pixels.foreach_get(pixels)
def swatch(x, y):
    srgb = pixels.reshape(height, width, 4)[height-1-y, x, :3]
    return np.where(srgb <= .04045, srgb / 12.92, ((srgb + .055) / 1.055)**2.4)
stripe, gold, cream, paw, ear = [swatch(*xy) for xy in
    [(170,140), (177,145), (273,149), (243,245), (269,79)]]

def smooth(low, high, value):
    t = min(1, max(0, (value-low)/(high-low)))
    return t*t*(3-2*t)

groups = {g.index: g.name for g in skin.vertex_groups}
coat = skin.data.color_attributes.new(name="Sprite Coat", type="FLOAT_COLOR", domain="POINT")
detail = skin.data.color_attributes.new(name="Face Detail", type="FLOAT_COLOR", domain="POINT")
for vertex in skin.data.vertices:
    weights = {groups[g.group]: g.weight for g in vertex.groups}
    head = weights.get("Head", 0)
    ears = weights.get("Ear.L", 0) + weights.get("Ear.R", 0)
    tail = sum(w for name,w in weights.items() if name.startswith("Tail."))
    tail_t = sum(int(name[-1])*w for name,w in weights.items() if name.startswith("Tail."))
    legs = [(name,w) for name,w in weights.items() if name.startswith(("Front.", "Hind."))]
    leg = sum(w for _,w in legs)
    x,y,z = vertex.co
    band = sin(163*x + 20*z + .7*sin(75*z) + .35*sin(125*x + 180*z))
    leg_band = sin(134*z + 28*x)
    pattern = band*(1-leg) + leg_band*leg
    pattern = pattern*(1-tail) + sin(7.4*tail_t)*tail
    orange = stripe + (gold-stripe)*smooth(-.42,.62,pattern)
    bib = smooth(.065,.145,x)*smooth(.32,.17,z)*(1-smooth(.05,.09,abs(y)))
    belly = smooth(.245,.16,z)*(1-leg)*(1-tail)
    muzzle = head*smooth(.355,.303,z)*smooth(.15,.20,x)
    socks = leg*smooth(.065,.022,z)
    light = max(bib, belly, muzzle)
    color = orange*(1-light) + cream*light
    color = color*(1-socks)+paw*socks
    color = color*(1-.52*ears)+ear*.52*ears
    coat.data[vertex.index].color = (*color,1)
    # Retain the iris, nose and fine face features from the underlying rig.
    face = smooth(.05,.32,head) * smooth(.27,.32,z) * (1 - smooth(.10,.45,ears))
    detail.data[vertex.index].color = (face,0,0,1)

    delta = vertex.co-rig.data.bones["Head"].head_local
    vertex.co += Vector((delta.x*.13,delta.y*.14,delta.z*.18))*(head+ears)
    vertex.co.z += .017*smooth(.20,.32,z)*(head+ears) + .018*ears*smooth(.37,.46,z)
    vertex.co.z += .018*belly
    for name,w in legs:
        bone = rig.data.bones.get(name)
        if bone and not name.endswith("Paw"):
            center = bone.head_local.lerp(bone.tail_local, smooth(bone.head_local.z,bone.tail_local.z,z))
            vertex.co.x += (center.x-x)*.13*w
            vertex.co.y += (center.y-y)*.17*w

skin["sprite_palette_v2"] = True
skin.data.update()
material = skin.data.materials[0]
nodes = material.node_tree.nodes
links = material.node_tree.links
base = next(n for n in nodes if n.type=="BSDF_PRINCIPLED")
output = next(n for n in nodes if n.type=="OUTPUT_MATERIAL")
old = base.inputs["Base Color"].links[0].from_socket
attrs = []
for name in ("Sprite Coat","Face Detail"):
    node = nodes.new("ShaderNodeVertexColor")
    node.layer_name = name
    attrs.append(node)
mix = nodes.new("ShaderNodeMixRGB")
links.new(attrs[1].outputs["Color"],mix.inputs[0])
links.new(attrs[0].outputs["Color"],mix.inputs[1])
links.new(old,mix.inputs[2])
emission = nodes.new("ShaderNodeEmission")
links.new(mix.outputs[0],emission.inputs["Color"])
links.new(emission.outputs[0],output.inputs["Surface"])
image = bpy.data.images.new("Mika_SpritePalette",2048,2048,alpha=True)
target = nodes.new("ShaderNodeTexImage")
target.image = image
nodes.active = target
bpy.ops.object.select_all(action="DESELECT")
skin.hide_set(False)
skin.select_set(True)
bpy.context.view_layer.objects.active = skin
scene = bpy.context.scene
scene.render.engine = "CYCLES"
scene.cycles.samples = 1
# Bake in bind pose to keep the UV projection independent of animation.
rig.data.pose_position = "REST"
bpy.ops.object.bake(type="EMIT", margin=12)
rig.data.pose_position = "POSE"
image.filepath_raw = str(folder/"mika-basecolor.png")
image.file_format = "PNG"
image.save()
image.pack()
links.new(base.outputs[0],output.inputs["Surface"])
links.new(target.outputs["Color"],base.inputs["Base Color"])
for node in nodes:
    if node.type == "NORMAL_MAP":
        node.inputs["Strength"].default_value = .055

bpy.ops.wm.save_as_mainfile(filepath=str(folder/"mika-rig.blend"))
rig.animation_data.action = bpy.data.actions["IdleRelaxed"]
scene.frame_set(1)
scene.camera.location = (.55,2,.34)
scene.camera.rotation_euler = (Vector((-.07,0,.22))-scene.camera.location).to_track_quat("-Z","Y").to_euler()
scene.camera.data.ortho_scale = .92
scene.render.resolution_percentage = 75
scene.render.filepath = "/tmp/mika-sprite-match.png"
scene.view_settings.view_transform = "Standard"
scene.cycles.samples = 12
bpy.ops.render.render(write_still=True)
