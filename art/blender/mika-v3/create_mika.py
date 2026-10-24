"""Build the unrigged Mika look study from the documented CC0 anatomy source."""
import bpy
import json
import math
from pathlib import Path
from mathutils import Vector

OUT=Path(__file__).resolve().parent
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(OUT/'source/tabby-cat.glb'))
cat=next(o for o in bpy.data.objects if o.type=='MESH')
cat.name='Mika_Visual_Sculpt'
# Keep the detailed anatomical surface. Face the character generally along +X.
cat.rotation_mode='XYZ'
cat.rotation_euler.z=math.radians(90)
bpy.context.view_layer.objects.active=cat
bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
# A slightly narrower trunk follows the leaner adventure-cat reference.
for vertex in cat.data.vertices:
    x,y,z=vertex.co
    trunk=math.exp(-((x+.015)/.15)**4)*math.exp(-((z-.25)/.12)**4)
    vertex.co.y*=1-.075*trunk
for poly in cat.data.polygons:poly.use_smooth=True

# Recolor the existing strand-level coat through a reversible shader graph.
# Retain the original eye/ear/nose colors and embedded texture detail.
material=cat.data.materials[0];material.name='Mika_Ginger_and_Cream'
nodes=material.node_tree.nodes;links=material.node_tree.links
bsdf=next(n for n in nodes if n.type=='BSDF_PRINCIPLED')
source=next(n for n in nodes if n.type=='TEX_IMAGE' and n.image.name=='Image_0')
source.label='Original CC0 coat detail'
luma=nodes.new('ShaderNodeRGBToBW');luma.label='Original coat value';links.new(source.outputs['Color'],luma.inputs[0])
ramp=nodes.new('ShaderNodeValToRGB');ramp.label='Mika ginger and cream palette'
stops=[(0,(.055,.012,.002,1)),(.03,(.20,.045,.006,1)),(.10,(.43,.12,.018,1)),(.25,(.74,.29,.045,1)),(.46,(.98,.54,.17,1)),(.68,(1,.78,.48,1)),(.86,(.94,.89,.77,1)),(1,(1,.98,.91,1))]
ramp.color_ramp.elements.remove(ramp.color_ramp.elements[1])
for i,(pos,color) in enumerate(stops):
    e=ramp.color_ramp.elements[0] if i==0 else ramp.color_ramp.elements.new(pos)
    e.position=pos;e.color=color
ramp.color_ramp.interpolation='LINEAR';links.new(luma.outputs[0],ramp.inputs[0])
hsv=nodes.new('ShaderNodeSeparateColor');hsv.mode='HSV';links.new(source.outputs['Color'],hsv.inputs[0])
def math_node(op,a,b):
    n=nodes.new('ShaderNodeMath');n.operation=op
    for i,value in enumerate((a,b)):
        if isinstance(value,(int,float)):n.inputs[i].default_value=value
        else:links.new(value,n.inputs[i])
    return n.outputs[0]
green=math_node('MULTIPLY',math_node('GREATER_THAN',hsv.outputs[0],.15),math_node('LESS_THAN',hsv.outputs[0],.72))
pink=math_node('MULTIPLY',math_node('LESS_THAN',hsv.outputs[0],.035),math_node('GREATER_THAN',hsv.outputs[1],.18))
keep=math_node('MAXIMUM',green,pink)
mix=nodes.new('ShaderNodeMixRGB');mix.blend_type='MIX';mix.label='Preserve eyes and pink skin'
links.new(keep,mix.inputs[0]);links.new(ramp.outputs[0],mix.inputs[1]);links.new(source.outputs[0],mix.inputs[2]);links.new(mix.outputs[0],bsdf.inputs['Base Color'])
# Coat is soft and nonmetallic. Retain the source tangent-space normal map.
for link in list(bsdf.inputs['Metallic'].links):links.remove(link)
bsdf.inputs['Metallic'].default_value=0
bsdf.inputs['Specular IOR Level'].default_value=.20
for link in list(bsdf.inputs['Roughness'].links):links.remove(link)
bsdf.inputs['Roughness'].default_value=.82
for node in nodes:
    if node.type=='NORMAL_MAP':node.inputs['Strength'].default_value=.35
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=40;scene.cycles.use_denoising=True
scene.render.resolution_x=1200;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG';scene.view_settings.view_transform='AgX';scene.view_settings.exposure=-.35
scene.world=bpy.data.worlds.new('Soft neutral studio');scene.world.use_nodes=True
scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.35,.40,.46,1);scene.world.node_tree.nodes['Background'].inputs[1].default_value=.22
floor_mat=bpy.data.materials.new('Slate studio floor');floor_mat.use_nodes=True
floor_mat.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(.055,.070,.085,1)
floor_mat.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value=.9
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.0005));bpy.context.object.name='Preview floor';bpy.context.object.data.materials.append(floor_mat)
def aim(obj,target):obj.rotation_euler=(Vector(target)-obj.location).to_track_quat('-Z','Y').to_euler()
for name,loc,power,size in [('Key',(.4, -.6,1.1),28,.9),('Fill',(.1,.8,.65),16,1),('Rim',(-.7,-.2,.9),25,.7)]:
    bpy.ops.object.light_add(type='AREA',location=loc);obj=bpy.context.object;obj.name=name;obj.data.energy=power;obj.data.shape='DISK';obj.data.size=size;aim(obj,(0,0,.22))
bpy.ops.object.camera_add(location=(.65,1.0,.49));cam=bpy.context.object;cam.name='Look review camera';cam.data.type='ORTHO';cam.data.ortho_scale=.67;scene.camera=cam;aim(cam,(0,0,.24))
# Pack the known concept sheets into the Blender authoring file for comparison.
root=OUT.parents[2]
for title,rel in [('Daytime concept','docs/design/references/golden-river-ruins-v2.png'),('Moonlit concept','docs/design/references/moonlit-observatory-v2.png')]:
    image=bpy.data.images.load(str(root/rel));image.name=title;image.pack()
for image in bpy.data.images:
    if image.size[0]>0 and not image.packed_file:image.pack()
cat['status']='Appearance accepted on 2026-10-24. Unrigged WIP; topology and animation remain incomplete.'
cat['source']='https://grab3d.com/models/animals/cat/'
cat['source_license']='CC0-1.0; original license and AI disclosure in source/LICENSE.txt'
cat['changes']='Ginger/cream shader; preserved facial colors; slightly narrower trunk; studio look review.'
bpy.ops.object.select_all(action='DESELECT');cat.select_set(True);bpy.context.view_layer.objects.active=cat
for area in bpy.context.screen.areas:
    if area.type=='VIEW_3D':
        area.spaces.active.region_3d.view_perspective='CAMERA';area.spaces.active.shading.type='MATERIAL'
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'mika-look.blend'))
for name,loc,target,scale in [('three-quarter',(.65,1.0,.49),(0,0,.24),.67),('side',(0,1.2,.31),(0,0,.24),.67),('face',(.72,.8,.48),(.17,0,.34),.39)]:
    cam.location=loc;aim(cam,target);cam.data.ortho_scale=scale
    scene.render.filepath=str(OUT/f'preview-{name}.png');bpy.ops.render.render(write_still=True)
report={'mesh_vertices':len(cat.data.vertices),'mesh_triangles':len(cat.data.polygons),'armatures':sum(o.type=='ARMATURE' for o in bpy.data.objects),'packed_images':sum(bool(i.packed_file) for i in bpy.data.images),'status':'Appearance accepted; unrigged work in progress; not tested in Unity.'}
(OUT/'validation.json').write_text(json.dumps(report,indent=2)+'\n');print('LOOK_STUDY',report)
