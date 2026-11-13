"""Author a gathered/extended running study from whole-body motion feedback.

The source rig and other actions remain editable. No gameplay asset is changed.
"""
import bpy
import json
import math
import sys
from pathlib import Path
from mathutils import Quaternion, Vector

OUT = Path(__file__).resolve().parent
DURATION = 12
STRIDE = .62
STANCE = .24
CONTACTS = {'Hind.L': 0, 'Hind.R': .06, 'Front.L': .46, 'Front.R': .52}


def author_run(rig, scene, sole_offsets):
    old = bpy.data.actions.get('Run')
    for track in list(rig.animation_data.nla_tracks):
        if track.name == 'Run': rig.animation_data.nla_tracks.remove(track)
    rig.animation_data.action = None
    if old: bpy.data.actions.remove(old)
    action = bpy.data.actions.new('Run')
    rig.animation_data.action = action
    action.use_fake_user = True
    bones = rig.data.bones

    def rotate(name, angle, axis=(0, 1, 0), add=False):
        local_axis = bones[name].matrix_local.to_3x3().inverted() @ Vector(axis)
        rotation = Quaternion(local_axis, angle)
        pose = rig.pose.bones[name]
        pose.rotation_quaternion = pose.rotation_quaternion @ rotation if add else rotation

    def move(name, delta):
        rig.pose.bones[name].location = bones[name].matrix_local.to_3x3().inverted() @ Vector(delta)

    def smooth_keys(keys, t, endpoint_slope=0):
        slopes=[endpoint_slope]
        for i in range(1,len(keys)-1):
            a,av=keys[i-1];b,bv=keys[i];c,cv=keys[i+1]
            slopes.append((cv-av)/(c-a) if (bv-av)*(cv-bv)>0 else 0)
        slopes.append(endpoint_slope)
        for i,((a, av), (b, bv)) in enumerate(zip(keys, keys[1:])):
            if t <= b:
                u=max(0,(t-a)/(b-a));span=b-a
                return (2*u**3-3*u*u+1)*av+(u**3-2*u*u+u)*span*slopes[i]+(-2*u**3+3*u*u)*bv+(u**3-u*u)*span*slopes[i+1]
        return keys[-1][1]

    for sample in range(DURATION*2+1):
        frame = 1 + sample/2
        t = sample/(DURATION*2)
        phase = math.tau*t
        for pose in rig.pose.bones:
            pose.location = (0, 0, 0)
            pose.rotation_quaternion = (1, 0, 0, 0)
            pose.scale = (1, 1, 1)
        # Gather ahead of hind contact, extend into fore contact, and let the
        # pelvis/chest pitch in opposite directions through the flight phases.
        gathered = math.cos(phase-math.tau*.85)
        body_pitch=-.20*math.cos(phase)
        pelvis_pitch=-.30-.45*gathered+body_pitch
        spine_pitch=.55+.80*gathered
        chest_pitch=-.10*gathered
        body_height=smooth_keys([(0,-.055),(.15,-.035),(.32,0),(.46,.01),(.62,.035),(.78,.005),(.92,-.05),(1,-.055)],t)
        move('Pelvis', (.025*gathered+.012*math.sin(phase), 0, body_height))
        rotate('Pelvis', pelvis_pitch)
        rotate('Spine', spine_pitch)
        rotate('Chest', chest_pitch)
        rotate('Neck', -.25, (0, 0, 1))
        rotate('Neck', -(pelvis_pitch+spine_pitch+chest_pitch)+.20, add=True)
        rotate('Head', -.85, (0, 0, 1))
        rotate('Head', -.18+.025*math.sin(phase-.4), add=True)
        # The root follows the pelvis; successive joints lag behind it with
        # increasing flexion, so the tip continues moving after the base turns.
        rotate('Tail.1', -1.05-pelvis_pitch-.10*(1+gathered)+.14*math.sin(phase-.7))
        for index, (bias, amplitude) in enumerate([
            (-.10, .20), (-.08, .28), (-.18, .36), (-.20, .42)
        ], start=2):
            lag = .7 + .8*(index-1)
            rotate(f'Tail.{index}', bias+amplitude*math.sin(phase-lag))
            rotate(f'Tail.{index}', .035*math.sin(phase-lag-.5), (0, 0, 1), add=True)
        for key, delay in CONTACTS.items():
            u = (t-delay) % 1
            front = key.startswith('Front')
            touch = .10
            release = touch-STRIDE*STANCE
            if u <= STANCE:
                x, z, pitch = touch-STRIDE*u, 0, 0
            else:
                q = (u-STANCE)/(1-STANCE)
                if front:
                    x = smooth_keys([(0,release),(.22,-.085),(.52,.035),(.82,.125),(1,touch)],q,-STRIDE*(1-STANCE))
                    z = smooth_keys([(0,0),(.23,.09),(.5,.115),(.80,.055),(1,0)],q)
                    pitch = -.65*math.sin(math.pi*q)
                else:
                    x = smooth_keys([(0,release),(.18,-.095),(.52,.11),(.80,.145),(1,touch)],q,-STRIDE*(1-STANCE))
                    z = smooth_keys([(0,0),(.20,.045),(.52,.085),(.82,.05),(1,0)],q)
                    pitch = -.45*math.sin(math.pi*q)
            move('CTRL_'+key, (x,0,z+sole_offsets[key]))
            rotate('CTRL_'+key,pitch)
            if front:
                move(key+'.Upper', (.009*math.cos(math.tau*u),0,.003*math.sin(math.tau*u)))
        # Supported feet constrain how high the body can rise. Keep those
        # targets fixed and let the torso absorb the landing through flexion.
        bpy.context.view_layer.update()
        lower=0
        for key,delay in CONTACTS.items():
            if (t-delay)%1>STANCE:continue
            hip=rig.pose.bones[key+'.Upper'].head
            target=rig.pose.bones['CTRL_'+key].head
            reach=(bones[key+'.Upper'].length+bones[key+'.Lower'].length)*.985
            horizontal=(hip.x-target.x)**2+(hip.y-target.y)**2
            ceiling=target.z+math.sqrt(max(0,reach*reach-horizontal))
            lower=max(lower,hip.z-ceiling)
        if lower>0:
            move('Pelvis',(.025*gathered+.012*math.sin(phase),0,body_height-lower))
            bpy.context.view_layer.update()
        # Recovering paws follow the body, with their targets inside the fixed
        # limb length. This does not change any supported paw's ground target.
        for key,delay in CONTACTS.items():
            if (t-delay)%1<=STANCE:continue
            hip=rig.pose.bones[key+'.Upper'].head
            target=rig.pose.bones['CTRL_'+key].head
            reach=(bones[key+'.Upper'].length+bones[key+'.Lower'].length)*.985
            delta=target-hip
            if delta.length>reach:
                position=hip+delta.normalized()*reach
                move('CTRL_'+key,position-bones['CTRL_'+key].head_local)
        for pose in rig.pose.bones:
            if pose.bone.use_deform or pose.name.startswith('CTRL_'):
                pose.keyframe_insert('location',frame=frame,group=pose.name)
                pose.keyframe_insert('rotation_quaternion',frame=frame,group=pose.name)
    # Dense authored samples use linear interpolation to preserve planted segments.
    for layer in action.layers:
        for strip in layer.strips:
            for bag in strip.channelbags:
                for curve in bag.fcurves:
                    for keyframe in curve.keyframe_points: keyframe.interpolation='LINEAR'
    track = rig.animation_data.nla_tracks.new(); track.name='Run'
    track.strips.new('Run',1,action);track.mute=True
    return action


if __name__ == '__main__':
    bpy.ops.wm.open_mainfile(filepath=str(OUT/'mika-rig.blend'))
    scene=bpy.context.scene; rig=bpy.data.objects['Mika_Rig']
    report=json.loads((OUT/'rig-report.json').read_text())
    author_run(rig,scene,report['sole_offsets'])
    report['actions']['Run']=DURATION
    report['run']={'stride_m':STRIDE,'stance_fraction':STANCE,'contacts':CONTACTS,'revision':4,'visual_acceptance':False}
    (OUT/'rig-report.json').write_text(json.dumps(report,indent=2)+'\n')
    rig.animation_data.action=bpy.data.actions['Run'];scene.frame_start=1;scene.frame_end=DURATION;scene.frame_set(1)
    bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'mika-rig.blend'))
    if '--no-render' in sys.argv: sys.exit(0)
    scene.cycles.samples=12;scene.render.resolution_x=720;scene.render.resolution_y=480
    scene.camera.location=(-.05,1.5,.37)
    scene.camera.rotation_euler=(Vector((-.05,0,.25))-scene.camera.location).to_track_quat('-Z','Y').to_euler()
    scene.camera.data.ortho_scale=.86
    for frame in [1,4,7,10]:
        scene.frame_set(frame);scene.render.filepath=f'/tmp/mika-authoring/tail-review/pose-{frame}.png';bpy.ops.render.render(write_still=True)
