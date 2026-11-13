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
reference_rgba = pixels.reshape(height, width, 4)[::-1]

def swatch(x, y):
    srgb = reference_rgba[y, x, :3]
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
    front = sum(w for name,w in legs if name.startswith("Front."))
    hind = sum(w for name,w in legs if name.startswith("Hind."))
    # Foreleg bands sweep across the upper shins. Hind bands follow the
    # diagonal thigh and fade above the largely unmarked lower hock.
    fore_band = sin(120*z - 54*x + .45*sin(260*x))
    hind_band = sin(82*z + 116*x + .45*sin(250*z))
    leg_band = (fore_band*front + hind_band*hind)/max(.001, leg)
    pattern = band*(1-leg) + leg_band*leg
    orange = stripe + (gold-stripe)*smooth(-.42,.62,pattern)
    bib = smooth(.065,.145,x)*smooth(.32,.17,z)*(1-smooth(.05,.09,abs(y)))
    belly = smooth(.245,.16,z)*(1-leg)*(1-tail)
    muzzle = smooth(.16,.68,head)*smooth(.362,.321,z)*smooth(.167,.222,x)
    # Long cream gradients end in rounded paws with fine toe divisions.
    socks = front*smooth(.123,.032,z) + hind*smooth(.139,.039,z)
    toe_color = paw
    for name,w in legs:
        if not name.endswith("Paw"):
            continue
        joint = rig.data.bones[name].head_local
        forward = smooth(.018,.036,x-joint.x)
        lateral = abs(y-joint.y)
        seam = (1-smooth(.0007,.0025,abs(lateral-.012))) * forward
        toe_color = paw*(1-.20*seam*w*smooth(.028,.009,z))
    light = max(bib, belly, muzzle)
    color = orange*(1-light) + cream*light
    # The sprite has a pale, rounded whisker pad and a small terracotta nose.
    whisker_pad = muzzle * smooth(.016,.049,abs(y))
    color = color*(1-whisker_pad) + swatch(305,113)*whisker_pad
    nose = smooth(.224,.241,x)*smooth(.318,.343,z)*smooth(.364,.350,z)*smooth(.042,.014,abs(y-.025))
    color = color*(1-nose) + swatch(314,103)*nose
    color = color*(1-socks)+toe_color*socks
    color = color*(1-.52*ears)+ear*.52*ears
    # Three pale rings follow the curved tail. The cap is solid orange.
    along_tail = tail_t/max(.001, tail) - 1
    ring = max(1-smooth(width, width+.19, abs(along_tail-center))
               for center,width in ((.65,.19), (1.85,.28), (3.12,.29)))
    tail_orange = swatch(53,108)
    tail_cream = swatch(63,112)
    tail_mix = smooth(.14,.85,tail)
    color = color*(1-tail_mix) + (tail_orange*(1-ring)+tail_cream*ring)*tail_mix
    # Register the flank against the rump, shoulder, back and belly.
    # The original back starts near (119, 135) and meets the neck at (233, 117).
    trunk_x = round(119 + (x + .20) * 365)
    trunk_y = round(119 + (.305 - z) * 465)
    trunk_weight = sum(weights.get(name, 0) for name in ("Pelvis", "Spine", "Chest", "Neck"))
    if (119 <= trunk_x <= 240 and 118 <= trunk_y <= 188
            and reference_rgba[trunk_y, trunk_x, 3] > .98):
        sampled = swatch(trunk_x, trunk_y)
        if max(sampled) > .30:
            flank = smooth(.20, .72, trunk_weight) * smooth(.028, .062, abs(y))
            flank *= smooth(118, 138, trunk_y) * smooth(188, 174, trunk_y)
            color = color*(1-flank) + sampled*flank
    # Register the idle sprite's profile against the nose, chin and ear tip.
    # Fade the side-view artwork across the forehead and muzzle centerline.
    px = round(315 + (x - .245) * 540)
    py = round(54 + (.470 - z) * 420)
    profile = 0
    if 241 <= px < 318 and 53 <= py <= 123 and reference_rgba[py, px, 3] > .96:
        profile = smooth(.015, .045, abs(y)) * smooth(.18, .64, head + ears)
        # Keep the mesh's eye texture registered to the eye socket.
        eye_radius = ((px - 293)/13)**2 + ((py - 95)/10)**2
        profile *= smooth(.8, 1.8, eye_radius)
        color = color*(1-profile) + swatch(px, py)*profile
    coat.data[vertex.index].color = (*color,1)
    # Retain the iris, nose and fine face features from the underlying rig.
    face = smooth(.05,.32,head) * smooth(.27,.32,z) * (1 - smooth(.10,.45,ears))
    # The upper forward face contains the original UV-painted eyes.
    eye_guard = smooth(.144,.177,x)*smooth(.315,.342,z)*smooth(.445,.366,z)
    # Fade fine forehead texture gradually into the sprite-sampled ear color.
    eye_guard *= 1-smooth(.10,.65,ears)*smooth(.374,.414,z)
    face_mix = max(face*(1-profile)*(1-.65*whisker_pad)*(1-nose), eye_guard)
    detail.data[vertex.index].color = (face_mix,face_mix,face_mix,1)

    delta = vertex.co-rig.data.bones["Head"].head_local
    vertex.co += Vector((delta.x*.13,delta.y*.14,delta.z*.18))*(head+ears)
    vertex.co.z += .017*smooth(.20,.32,z)*(head+ears) + .018*ears*smooth(.37,.46,z)
    # Shorten the protruding muzzle and round the whisker pads.
    vertex.co.x -= .009*muzzle*smooth(.196,.245,x)
    vertex.co.y += y*.12*whisker_pad
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
# Carry fine fur relief into the color bake for Unity's unlit material.
# The normal map's red channel supplies signed tangent-space relief.
normal = next(node for node in nodes if node.type == "NORMAL_MAP")
normal_texture = normal.inputs["Color"].links[0].from_socket
channels = nodes.new("ShaderNodeSeparateColor")
links.new(normal_texture, channels.inputs[0])
relief = nodes.new("ShaderNodeMath")
relief.operation = "MULTIPLY_ADD"
links.new(channels.outputs["Red"], relief.inputs[0])
relief.inputs[1].default_value = .48
relief.inputs[2].default_value = .76
textured = nodes.new("ShaderNodeMixRGB")
textured.blend_type = "MULTIPLY"
textured.inputs[0].default_value = 1
links.new(mix.outputs[0], textured.inputs[1])
links.new(relief.outputs[0], textured.inputs[2])
links.new(textured.outputs[0], emission.inputs["Color"])
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
        node.inputs["Strength"].default_value = .32

bpy.ops.wm.save_as_mainfile(filepath=str(folder/"mika-rig.blend"))
rig.animation_data.action = bpy.data.actions["IdleRelaxed"]
scene.frame_set(1)
scene.camera.location = (.55,2,.34)
scene.camera.rotation_euler = (Vector((-.07,0,.22))-scene.camera.location).to_track_quat("-Z","Y").to_euler()
scene.camera.data.ortho_scale = .92
scene.render.resolution_percentage = 75
scene.render.filepath = "/tmp/mika-fur-detail.png"
scene.view_settings.view_transform = "Standard"
scene.cycles.samples = 12
bpy.ops.render.render(write_still=True)
