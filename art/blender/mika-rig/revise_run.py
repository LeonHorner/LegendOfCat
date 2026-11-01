"""Author a gathered/extended running study after the first gait was rejected.

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
        gathered = math.cos(phase-math.tau*.85)
        move('Pelvis', (.012*math.sin(phase), 0, -.04+.018*math.cos(phase-math.tau*.25)))
        rotate('Pelvis', -.15*gathered)
        rotate('Spine', .28*gathered)
        rotate('Chest', .10*gathered)
        rotate('Neck', -.25, (0, 0, 1))
        rotate('Neck', -.23*gathered+.20, add=True)
        rotate('Head', -.85, (0, 0, 1))
        rotate('Head', -.18+.025*math.sin(phase-.4), add=True)
        rotate('Tail.1', -1.05+.15*gathered+.055*math.sin(phase-.7))
        rotate('Tail.2', -.10+.04*math.sin(phase-1.0))
        rotate('Tail.3', -.08+.03*math.sin(phase-1.3))
        rotate('Tail.4', -.18+.025*math.sin(phase-1.6))
        rotate('Tail.5', -.20+.02*math.sin(phase-1.9))
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
    report['run']={'stride_m':STRIDE,'stance_fraction':STANCE,'contacts':CONTACTS,'revision':2,'visual_acceptance':False}
    (OUT/'rig-report.json').write_text(json.dumps(report,indent=2)+'\n')
    rig.animation_data.action=bpy.data.actions['Run'];scene.frame_start=1;scene.frame_end=DURATION;scene.frame_set(1)
    bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'mika-rig.blend'))
    if '--no-render' in sys.argv: sys.exit(0)
    scene.cycles.samples=12;scene.render.resolution_x=720;scene.render.resolution_y=480
    scene.camera.location=(-.05,1.5,.37)
    scene.camera.rotation_euler=(Vector((-.05,0,.25))-scene.camera.location).to_track_quat('-Z','Y').to_euler()
    scene.camera.data.ortho_scale=.86
    for frame in [1,4,7,10]:
        scene.frame_set(frame);scene.render.filepath=f'/tmp/mika-authoring/revision-2/pose-{frame}.png';bpy.ops.render.render(write_still=True)
