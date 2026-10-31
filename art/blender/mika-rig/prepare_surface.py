"""Prepare a lower-density deformation surface and rebake the accepted appearance."""
import bpy
import bmesh
import json
from pathlib import Path
from mathutils import Vector

ROOT=Path(__file__).resolve().parents[3]
OUT=Path(__file__).resolve().parent
CACHE=Path('/tmp/mika-authoring');CACHE.mkdir(exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=str(ROOT/'art/blender/mika-v3/mika-look.blend'))
high=bpy.data.objects['Mika_Visual_Sculpt'];high.name='Mika_Accepted_Reference'
print('SOURCE',len(high.data.vertices),len(high.data.polygons),flush=True)
bm=bmesh.new();bm.from_mesh(high.data)
print('TOPOLOGY',sum(not e.is_manifold for e in bm.edges),'nonmanifold edges',flush=True)
bm.free()
# Preserve the immutable visual reference and rebuild only a working duplicate.
low=high.copy();low.data=high.data.copy();bpy.context.collection.objects.link(low);low.name='Mika_Deform_Surface'
bpy.ops.object.select_all(action='DESELECT');low.select_set(True);bpy.context.view_layer.objects.active=low
# Quadriflow is more stable on a clean, watertight input. Voxelize fine enough to
# retain facial planes, then project the result back to the original sculpt.
remesh=low.modifiers.new('Watertight authoring surface','REMESH');remesh.mode='VOXEL';remesh.voxel_size=.0035;remesh.use_smooth_shade=True
bpy.ops.object.modifier_apply(modifier=remesh.name)
print('VOXEL',len(low.data.vertices),len(low.data.polygons),flush=True)
# The source's topology makes Quadriflow cancel even after manifold cleanup.
# Un-subdivide the regular voxel quads instead; do not misreport cancellation as success.
reduce=low.modifiers.new('Reduce regular surface quads','DECIMATE');reduce.decimate_type='UNSUBDIV';reduce.iterations=2
bpy.ops.object.modifier_apply(modifier=reduce.name)
assert len(low.data.polygons)<60000, 'Surface reduction did not reach the authoring budget'
print('QUADS',len(low.data.vertices),len(low.data.polygons),flush=True)
wrap=low.modifiers.new('Fit accepted anatomy','SHRINKWRAP');wrap.target=high;wrap.wrap_method='NEAREST_SURFACEPOINT';wrap.wrap_mode='ON_SURFACE'
bpy.ops.object.modifier_apply(modifier=wrap.name)
for poly in low.data.polygons:poly.use_smooth=True
bpy.ops.wm.save_as_mainfile(filepath=str(CACHE/'retopology.blend'))
print('RETOPOLOGY_SAVED',flush=True)
# A fresh UV map is required: transferring source atlas seams onto new quads
# would interpolate unrelated texture islands across the new faces.
low.data.materials.clear();material=bpy.data.materials.new('Mika_Baked_Fur');material.use_nodes=True;low.data.materials.append(material)
bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project(angle_limit=1.15,island_margin=.006);bpy.ops.object.mode_set(mode='OBJECT')
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=8
nodes=material.node_tree.nodes;links=material.node_tree.links
image=bpy.data.images.new('Mika_BaseColor',width=2048,height=2048,alpha=True)
tex=nodes.new('ShaderNodeTexImage');tex.image=image;nodes.active=tex
highmat=high.data.materials[0];output=next(n for n in highmat.node_tree.nodes if n.type=='OUTPUT_MATERIAL');original=output.inputs['Surface'].links[0].from_socket
base=next(n for n in highmat.node_tree.nodes if n.type=='BSDF_PRINCIPLED').inputs['Base Color'].links[0].from_socket
emit=highmat.node_tree.nodes.new('ShaderNodeEmission');highmat.node_tree.links.new(base,emit.inputs['Color']);highmat.node_tree.links.new(emit.outputs[0],output.inputs['Surface'])
high.select_set(True);low.select_set(True);bpy.context.view_layer.objects.active=low
bpy.ops.object.bake(type='EMIT',use_selected_to_active=True,cage_extrusion=.006,max_ray_distance=.018,margin=12,use_clear=True)
image.filepath_raw=str(OUT/'mika-basecolor.png');image.file_format='PNG';image.save();image.pack()
highmat.node_tree.links.new(original,output.inputs['Surface']);highmat.node_tree.nodes.remove(emit)
links.new(tex.outputs['Color'],nodes['Principled BSDF'].inputs['Base Color']);nodes['Principled BSDF'].inputs['Roughness'].default_value=.82
nodes['Principled BSDF'].inputs['Specular IOR Level'].default_value=.20
# Retain fine surface relief with a tangent-space selected-to-active normal bake.
normal=bpy.data.images.new('Mika_Normal',width=2048,height=2048);normal.colorspace_settings.name='Non-Color'
ntex=nodes.new('ShaderNodeTexImage');ntex.image=normal;nodes.active=ntex
bpy.ops.object.bake(type='NORMAL',use_selected_to_active=True,cage_extrusion=.006,max_ray_distance=.018,normal_space='TANGENT',margin=12,use_clear=True)
normal.filepath_raw=str(OUT/'mika-normal.png');normal.file_format='PNG';normal.save();normal.pack()
nmap=nodes.new('ShaderNodeNormalMap');nmap.inputs['Strength'].default_value=.35;links.new(ntex.outputs['Color'],nmap.inputs['Color']);links.new(nmap.outputs['Normal'],nodes['Principled BSDF'].inputs['Normal'])
high.hide_render=True;high.hide_set(True)
scene.camera.location=(.65,1.0,.49);scene.camera.rotation_euler=(Vector((0,0,.24))-scene.camera.location).to_track_quat('-Z','Y').to_euler();scene.camera.data.ortho_scale=.67
scene.render.resolution_x=900;scene.render.resolution_y=750;scene.cycles.samples=20
bpy.ops.object.select_all(action='DESELECT');low.select_set(True);bpy.context.view_layer.objects.active=low
bpy.ops.wm.save_as_mainfile(filepath=str(CACHE/'prepared-surface.blend'))
scene.render.filepath=str(OUT/'surface-review.png');bpy.ops.render.render(write_still=True)
report={'source_vertices':len(high.data.vertices),'deformation_vertices':len(low.data.vertices),'deformation_faces':len(low.data.polygons),'quad_faces':sum(len(p.vertices)==4 for p in low.data.polygons),'method':'Fine voxel cleanup, regular-quad un-subdivision, nearest-surface fit and rebaked appearance. Requires deformation review.'}
(OUT/'surface-report.json').write_text(json.dumps(report,indent=2)+'\n');print('SURFACE',report,flush=True)
