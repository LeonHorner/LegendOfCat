"""Author an editable quadruped rig and motion candidates from Mika's accepted surface."""
import bpy
import json
import math
import sys
from pathlib import Path
from mathutils import Vector, Matrix, Quaternion

OUT=Path(__file__).resolve().parent
sys.path.insert(0,str(OUT))
from revise_run import author_run, DURATION, STRIDE, STANCE, CONTACTS
CACHE=Path('/tmp/mika-authoring')
bpy.ops.wm.open_mainfile(filepath=str(CACHE/'prepared-surface.blend'))
scene=bpy.context.scene
skin=bpy.data.objects['Mika_Deform_Surface'];skin.name='Mika_Skin'
high=bpy.data.objects['Mika_Accepted_Reference']
# Align the source's diagonal longitudinal axis to Blender +X and center its trunk.
angle=math.radians(34.16797716816635);rotation=Matrix.Rotation(angle,4,'Z')
for obj in [skin,high]:
    for vertex in obj.data.vertices:vertex.co=rotation @ vertex.co+Vector((0,.035,0))
for obj in scene.objects:
    if obj.type in {'CAMERA','LIGHT'}:
        obj.matrix_world=rotation @ obj.matrix_world;obj.location.y+=.035
arm=bpy.data.armatures.new('MikaSkeleton');rig=bpy.data.objects.new('Mika_Rig',arm);scene.collection.objects.link(rig)
bpy.ops.object.select_all(action='DESELECT');rig.select_set(True);bpy.context.view_layer.objects.active=rig
bpy.ops.object.mode_set(mode='EDIT')
def bone(name,a,b,parent=None,deform=True):
    item=arm.edit_bones.new(name);item.head=a;item.tail=b;item.use_deform=deform
    if parent:item.parent=arm.edit_bones[parent]
    return item
bone('Root',(0,0,0),(0,0,.05))
bone('Pelvis',(-.145,0,.245),(-.065,0,.255),'Root')
bone('Spine',(-.065,0,.255),(.035,0,.27),'Pelvis')
bone('Chest',(.035,0,.27),(.11,0,.28),'Spine')
bone('Neck',(.11,0,.28),(.18,.030,.345),'Chest')
bone('Head',(.18,.030,.345),(.205,.10,.34),'Neck')
for side,a,b in [('L',(.140,.020,.395),(.127,.030,.431)),('R',(.220,.045,.393),(.231,.060,.432))]:bone('Ear.'+side,a,b,'Head')
tail=[(-.205,.050,.28),(-.235,.058,.325),(-.251,.067,.38),(-.255,.064,.435),(-.244,.048,.478),(-.222,.030,.487)]
for i in range(5):bone('Tail.'+str(i+1),tail[i],tail[i+1],'Pelvis' if i==0 else 'Tail.'+str(i))
legs={
 'Front.L':[(.105,.033,.25),(.084,.032,.137),(.101,.031,.033),(.135,.031,.013)],
 'Front.R':[(.105,-.045,.25),(.087,-.055,.137),(.103,-.059,.033),(.137,-.059,.013)],
 'Hind.L':[(-.145,.047,.25),(-.105,.063,.148),(-.177,.072,.065),(-.142,.072,.013)],
 'Hind.R':[(-.145,-.045,.25),(-.097,-.050,.16),(-.155,-.049,.071),(-.120,-.049,.013)]}
for key,points in legs.items():
    parent='Chest' if key.startswith('Front') else 'Pelvis'
    for i,label in enumerate(['Upper','Lower','Paw']):
        name=key+'.'+label;bone(name,points[i],points[i+1],parent);parent=name
    bone('CTRL_'+key,points[2],points[3],'Root',False)
    hip,knee,end=map(Vector,points[:3]);axis=(end-hip).normalized();projection=hip+axis*(knee-hip).dot(axis);bend=(knee-projection).normalized()
    pole=knee+bend*.15;bone('POLE_'+key,pole,pole+Vector((0,0,.025)),'Root',False)
bpy.ops.object.mode_set(mode='OBJECT');rig.show_in_front=True;arm.display_type='OCTAHEDRAL'
for pb in rig.pose.bones:pb.rotation_mode='QUATERNION'
# Heat weights follow mesh connectivity, keeping limb influences off neighboring
# legs and the trunk. Spatial left/right labels are not adequate for this pose.
arm.bones['Root'].use_deform=False
bpy.ops.object.select_all(action='DESELECT');skin.select_set(True);rig.select_set(True);bpy.context.view_layer.objects.active=rig
bpy.ops.object.parent_set(type='ARMATURE_AUTO')
arm.bones['Root'].use_deform=True
# Heat diffusion on the source's fused armpits reaches across the belly. Fade
# limb weights at their anatomical attachments, returning that weight to the
# trunk rather than letting a lifted paw drag the abdomen with it.
def smoothstep(a,b,value):
    t=max(0,min(1,(value-a)/(b-a)))
    return t*t*(3-2*t)
for vertex in skin.data.vertices:
    x,y,z=vertex.co
    returned=0
    for entry in list(vertex.groups):
        group=skin.vertex_groups[entry.group]
        if group.name.startswith('Front.'):
            gate=smoothstep(.025,.075,x)*(1-smoothstep(.165,.235,z))
        elif group.name.startswith('Hind.'):
            gate=(1-smoothstep(-.105,-.045,x))*(1-smoothstep(.23,.30,z))
        else:continue
        returned+=entry.weight*(1-gate)
        group.add([vertex.index],entry.weight*gate,'REPLACE')
    if returned:
        target='Chest' if x>.035 else 'Pelvis' if x<-.085 else 'Spine'
        group=skin.vertex_groups[target]
        prior=next((g.weight for g in vertex.groups if g.group==group.index),0)
        group.add([vertex.index],prior+returned,'REPLACE')
for modifier in skin.modifiers:
    if modifier.type=='ARMATURE':modifier.use_deform_preserve_volume=False
# The eyes remain open, rigid with the cranium. Ear tips retain their heat weights.
for vertex in skin.data.vertices:
    x,y,z=vertex.co
    if x>.18 and .315<z<.395:
        for group in skin.vertex_groups:group.remove([vertex.index])
        skin.vertex_groups['Head'].add([vertex.index],1,'REPLACE')
bpy.context.view_layer.objects.active=skin;bpy.ops.object.select_all(action='DESELECT');skin.select_set(True)
bpy.ops.object.vertex_group_limit_total(limit=4)
bpy.ops.object.vertex_group_normalize_all(lock_active=False)
bpy.context.view_layer.objects.active=rig;bpy.ops.object.select_all(action='DESELECT');rig.select_set(True)
pole_angles={}
for key in legs:
    ik=rig.pose.bones[key+'.Lower'].constraints.new('IK');ik.name='Planted paw IK';ik.target=rig;ik.subtarget='CTRL_'+key;ik.pole_target=rig;ik.pole_subtarget='POLE_'+key;ik.chain_count=2;ik.use_stretch=False
    rotation_constraint=rig.pose.bones[key+'.Paw'].constraints.new('COPY_ROTATION');rotation_constraint.name='Paw orientation';rotation_constraint.target=rig;rotation_constraint.subtarget='CTRL_'+key;rotation_constraint.owner_space='WORLD';rotation_constraint.target_space='WORLD'
    for part in ['Upper','Lower']:rig.pose.bones[key+'.'+part].ik_stretch=0
    candidates=[]
    for pole_angle in [0,math.pi/2,math.pi,-math.pi/2]:
        ik.pole_angle=pole_angle;bpy.context.view_layer.update()
        candidates.append(((rig.pose.bones[key+'.Upper'].tail-Vector(legs[key][1])).length,pole_angle))
    ik.pole_angle=min(candidates)[1];pole_angles[key]=ik.pole_angle
# Foot-control custom shapes are real Blender controls, excluded from render/export.
collection=bpy.data.collections.new('Rig control shapes');scene.collection.children.link(collection)
mesh=bpy.data.meshes.new('Paw control ring');verts=[(math.cos(t*math.tau/24)*.025,0,math.sin(t*math.tau/24)*.025) for t in range(24)];mesh.from_pydata(verts,[(i,(i+1)%24) for i in range(24)],[])
shape=bpy.data.objects.new('Paw_Control_Shape',mesh);collection.objects.link(shape);shape.hide_render=True;shape.hide_set(True)
for key in legs:
    pb=rig.pose.bones['CTRL_'+key];pb.custom_shape=shape;pb.use_custom_shape_bone_size=False

def rotate(name,axis,angle):
    local_axis=arm.bones[name].matrix_local.to_3x3().inverted() @ Vector(axis)
    delta=Quaternion(local_axis,angle)
    if name in {'Head','Neck'} and tuple(axis)!=(0,0,1):rig.pose.bones[name].rotation_quaternion=rig.pose.bones[name].rotation_quaternion @ delta
    else:rig.pose.bones[name].rotation_quaternion=delta
def reset():
    for pb in rig.pose.bones:pb.location=(0,0,0);pb.rotation_quaternion=(1,0,0,0);pb.scale=(1,1,1)
def translate(name,offset):
    # Pose locations use bone-local axes, even when a control is named Root.
    rig.pose.bones[name].location=arm.bones[name].matrix_local.to_3x3().inverted() @ Vector(offset)
def ease(value):return .5-.5*math.cos(math.pi*max(0,min(1,value)))
def pulse(t):return math.sin(math.pi*t)**2
# Keep every foot control in Root space, independent of pelvis breathing or FK.
def foot(key,offset=(0,0,0),pitch=0):
    pb=rig.pose.bones['CTRL_'+key]
    local=arm.bones[pb.name].matrix_local.to_3x3().inverted() @ Vector(offset)
    pb.location=local+arm.bones[pb.name].matrix_local.to_3x3().inverted() @ Vector((0,0,sole_offsets.get(key,0)))
    rotate(pb.name,(0,1,0),pitch)
# Measure sole offsets after the native IK solve and correct only the paw controls.
sole_offsets={}
bpy.context.view_layer.update()
evaluated=skin.evaluated_get(bpy.context.evaluated_depsgraph_get())
for key in legs:
    group=skin.vertex_groups[key+'.Paw'].index
    indices=[v.index for v in skin.data.vertices if any(g.group==group and g.weight>.75 for g in v.groups)]
    sole_offsets[key]=-min(evaluated.data.vertices[i].co.z for i in indices)

clips={'Idle':48,'LookBack':72,'Listen':48,'TailTwitch':48,'Groom':96,'Stretch':72,'Run':DURATION,'Takeoff':12,'Airborne':24,'Fall':12,'Land':12,'Stop':16,'JumpReview':48}
rig.animation_data_create()
for clip,length in clips.items():
    if clip=='Run':continue
    rig.animation_data.action=None;reset()
    for frame in range(1,length+2):
        t=(frame-1)/length;phase=t*math.tau;reset()
        for key in legs:foot(key)
        # The original sculpt looks toward the camera. A restrained neck/head turn
        # establishes a forward-looking stance while keeping the cranium rigid.
        rotate('Neck',(0,0,1),-.12);rotate('Head',(0,0,1),-.30)
        if clip=='Idle':
            translate('Pelvis',(0,0,.001*math.sin(phase)))
            rotate('Head',(0,1,0),.018*math.sin(phase));rotate('Neck',(0,0,1),-.12+.025*math.sin(phase))
        if clip=='LookBack':
            rotate('Neck',(0,0,1),-.12+.48*pulse(t));rotate('Head',(0,0,1),-.30+.75*pulse(t))
        if clip=='Listen':
            rotate('Ear.L',(0,0,1),.25*math.sin(phase));rotate('Ear.R',(0,0,1),-.20*math.sin(phase+.4));rotate('Head',(1,0,0),.08*math.sin(phase))
        for i in range(1,6):rotate('Tail.'+str(i),(0,1,0),.018*math.sin(phase+i*.3) if clip in {'Idle','Listen','TailTwitch'} else 0)
        if clip=='TailTwitch':
            rotate('Tail.4',(1,0,0),.13*math.sin(phase*2)*pulse(t));rotate('Tail.5',(1,0,0),.23*math.sin(phase*2+.4)*pulse(t))
        if clip=='Groom':
            amount=pulse(t);foot('Front.L',(.08*amount,-.01*amount,.235*amount),-.65*amount)
            rotate('Neck',(0,1,0),.20*amount);rotate('Head',(0,0,1),-.30+.2*amount);rotate('Head',(0,1,0),.22*amount+.05*math.sin(phase*4)*amount)
        if clip=='Stretch':
            amount=pulse(t);translate('Pelvis',(0,0,.009*amount));rotate('Spine',(0,1,0),.40*amount);rotate('Chest',(0,1,0),.28*amount);rotate('Neck',(0,1,0),-.22*amount)
            for key in ['Front.L','Front.R']:foot(key,(.075*amount,0,0))
        if clip in {'Takeoff','Airborne','Fall','Land','JumpReview'}:
            if clip=='Takeoff':crouch=.018*pulse(t);fold=.035*ease(t)
            elif clip=='Airborne':crouch=0;fold=.055+.012*math.sin(math.pi*t)
            elif clip=='Fall':crouch=0;fold=.055*(1-ease(t))
            elif clip=='Land':crouch=.018*math.sin(math.pi*t);fold=0
            else:
                translate('Root',(0,0,.16*math.sin(math.pi*t)))
                crouch=.012*max(0,1-t*6)+.014*max(0,(t-.83)/.17)*math.sin(math.pi*max(0,(t-.83)/.17))
                fold=.065*math.sin(math.pi*t)**1.5
            translate('Pelvis',(0,0,-crouch))
            for key in legs:foot(key,(-.015*math.sin(math.pi*t) if key.startswith('Front') else .025*math.sin(math.pi*t),0,fold),-.25*math.sin(math.pi*t))
            rotate('Spine',(0,1,0),-.04*math.sin(math.pi*t));rotate('Neck',(0,1,0),.045*math.sin(math.pi*t))
        if clip=='Stop':
            for key in legs:foot(key,(.025*(1-ease(t)),0,.016*math.sin(math.pi*t)),0)
        for pb in rig.pose.bones:
            if pb.bone.use_deform or pb.name.startswith('CTRL_'):
                pb.keyframe_insert('location',frame=frame,group=pb.name);pb.keyframe_insert('rotation_quaternion',frame=frame,group=pb.name)
    action=rig.animation_data.action;action.name=clip;action.use_fake_user=True
    track=rig.animation_data.nla_tracks.new();track.name=clip;track.strips.new(clip,1,action);track.mute=True
author_run(rig,scene,sole_offsets)
rig.animation_data.action=bpy.data.actions['Idle'];scene.frame_start=1;scene.frame_end=48;scene.render.fps=24;scene.frame_set(1)
rig['motion_review']='Unaccepted motion candidates. Preserve the accepted game until reviewed.'
rig['forward_axis']='+X';rig['unit']='meter';rig['source']='Accepted Mika v3 and its CC0 provenance.'
high.hide_render=True;high.hide_set(True)
scene.cycles.samples=16;scene.render.resolution_x=900;scene.render.resolution_y=750
# Camera transformed with the aligned mesh keeps the accepted review angle.
bpy.ops.object.select_all(action='DESELECT');rig.select_set(True);bpy.context.view_layer.objects.active=rig
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'mika-rig.blend'))
camera_location=scene.camera.location.copy();camera_scale=scene.camera.data.ortho_scale;camera_rotation=scene.camera.rotation_euler.copy()
for clip,frame in [('Idle',1),('Groom',49),('Stretch',37),('Run',5),('LookBack',37)]:
    scene.camera.location=camera_location;scene.camera.rotation_euler=camera_rotation;scene.camera.data.ortho_scale=camera_scale
    if clip=='Run':
        scene.camera.location=(-.05,1.5,.37);scene.camera.data.ortho_scale=.86
        scene.camera.rotation_euler=(Vector((-.05,0,.25))-scene.camera.location).to_track_quat('-Z','Y').to_euler()
    rig.animation_data.action=bpy.data.actions[clip];scene.frame_set(frame)
    scene.render.filepath=str(OUT/('pose-'+clip.lower()+'.png'));bpy.ops.render.render(write_still=True)
report={'bones':len(arm.bones),'deform_bones':sum(b.use_deform for b in arm.bones),'skin_vertices':len(skin.data.vertices),'actions':clips,'pole_angles':pole_angles,'sole_offsets':sole_offsets,'unweighted_vertices':sum(not v.groups for v in skin.data.vertices),'visual_acceptance':False,'unity_validated':False,'run':{'stride_m':STRIDE,'stance_fraction':STANCE,'contacts':CONTACTS,'revision':2,'visual_acceptance':False}}
(OUT/'rig-report.json').write_text(json.dumps(report,indent=2)+'\n');print('RIG_REPORT',report,flush=True)
