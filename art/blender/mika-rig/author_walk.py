"""Add review-only Walk and IdleRelaxed actions to the existing Mika rig."""
import json
import math
from pathlib import Path
import bpy
from mathutils import Quaternion, Vector

OUT = Path(__file__).resolve().parent
WALK_FRAMES = 28
STRIDE = .20
STANCE = .65
CONTACTS = {'Hind.L': 0, 'Front.L': .25, 'Hind.R': .5, 'Front.R': .75}


def author_actions(rig, scene, sole_offsets):
    bones = rig.data.bones

    def move(name, delta):
        rig.pose.bones[name].location = bones[name].matrix_local.to_3x3().inverted() @ Vector(delta)

    def rotate(name, angle, axis=(0, 1, 0), add=False):
        local_axis = bones[name].matrix_local.to_3x3().inverted() @ Vector(axis)
        rotation = Quaternion(local_axis, angle)
        pose = rig.pose.bones[name]
        pose.rotation_quaternion = pose.rotation_quaternion @ rotation if add else rotation

    for name, length in [('Walk', WALK_FRAMES), ('IdleRelaxed', 96)]:
        rig.animation_data.action = None
        for track in list(rig.animation_data.nla_tracks):
            if track.name == name:
                rig.animation_data.nla_tracks.remove(track)
        old = bpy.data.actions.get(name)
        if old:
            bpy.data.actions.remove(old)
        action = bpy.data.actions.new(name)
        action.use_fake_user = True
        rig.animation_data.action = action
        for sample in range(length*2+1):
            frame = 1+sample/2
            t = sample/(length*2)
            phase = math.tau*t
            walking = name == 'Walk'
            for pose in rig.pose.bones:
                pose.location = (0, 0, 0)
                pose.rotation_quaternion = (1, 0, 0, 0)
                pose.scale = (1, 1, 1)
            height = -.032 + (.003*math.cos(2*phase) if walking else .0015*math.sin(phase))
            sway = .004*math.sin(phase) if walking else .001*math.sin(phase)
            move('Pelvis', (0, sway, height))
            pitch = .025*math.sin(2*phase) if walking else .006*math.sin(phase)
            rotate('Pelvis', pitch)
            rotate('Spine', -.6*pitch)
            rotate('Chest', -.4*pitch)
            rotate('Neck', -.25, (0, 0, 1))
            rotate('Head', -.85 + (0 if walking else .045*math.sin(phase)), (0, 0, 1))
            rotate('Head', .015*math.sin(2*phase) if walking else .018*math.sin(phase-.5), add=True)
            # Start the sweep at the base, then carry a delayed bend through
            # the middle. Distal joints follow rather than supplying all motion.
            root_flex = .22 if walking else .28
            root_sweep = .16 if walking else .24
            rotate('Tail.1', (-1.12 if walking else -1.42)-pitch+root_flex*math.sin(phase-.7))
            rotate('Tail.1', root_sweep*math.sin(phase-1.1), (0, 0, 1), add=True)
            for i, (bias, flex) in enumerate([
                (-.12, .20), (-.08, .18), (-.18, .14), (-.20, .10)
            ], start=2):
                lag = .7 + .65*(i-1)
                if not walking:
                    bias = (.18, .43, .41, .20)[i-2]
                rotate(f'Tail.{i}', bias+flex*math.sin(phase-lag))
                rotate(f'Tail.{i}', .055*math.sin(phase-lag-.4), (0, 0, 1), add=True)
            if not walking:
                # A small ear adjustment accompanies the relaxed head movement.
                rotate('Ear.L', .10*math.sin(phase)**4, (0, 0, 1))
                rotate('Ear.R', -.07*math.sin(phase-.4)**4, (0, 0, 1))
            for key, delay in CONTACTS.items():
                x = z = pitch_paw = 0
                if walking:
                    u = (t-delay)%1
                    half = STRIDE*STANCE/2
                    if u <= STANCE:
                        x = half-STRIDE*u
                    else:
                        q = (u-STANCE)/(1-STANCE)
                        # Cubic recovery joins the planted backward velocity.
                        x = -half+2*half*(3*q*q-2*q*q*q)-STRIDE*(1-STANCE)*(2*q**3-3*q*q+q)
                        z = (.035 if key.startswith('Front') else .028)*math.sin(math.pi*q)**2
                        pitch_paw = -.20*math.sin(math.pi*q)**2
                move('CTRL_'+key, (x, 0, z+sole_offsets[key]))
                rotate('CTRL_'+key, pitch_paw)
            # The hips yield to supported paws without scaling or lengthening legs.
            bpy.context.view_layer.update()
            lower = 0
            for key, delay in CONTACTS.items():
                if walking and (t-delay)%1 > STANCE:
                    continue
                hip = rig.pose.bones[key+'.Upper'].head
                target = rig.pose.bones['CTRL_'+key].head
                reach = .985*(bones[key+'.Upper'].length+bones[key+'.Lower'].length)
                horizontal = (hip.x-target.x)**2+(hip.y-target.y)**2
                ceiling = target.z+math.sqrt(max(0, reach*reach-horizontal))
                lower = max(lower, hip.z-ceiling)
            if lower > 0:
                move('Pelvis', (0, sway, height-lower))
                bpy.context.view_layer.update()
            for pose in rig.pose.bones:
                if pose.bone.use_deform or pose.name.startswith('CTRL_'):
                    pose.keyframe_insert('location', frame=frame, group=pose.name)
                    pose.keyframe_insert('rotation_quaternion', frame=frame, group=pose.name)
        for layer in action.layers:
            for strip in layer.strips:
                for bag in strip.channelbags:
                    for curve in bag.fcurves:
                        for keyframe in curve.keyframe_points:
                            keyframe.interpolation = 'LINEAR'
        track = rig.animation_data.nla_tracks.new()
        track.name = name
        track.strips.new(name, 1, action)
        track.mute = True


if __name__ == '__main__':
    bpy.ops.wm.open_mainfile(filepath=str(OUT/'mika-rig.blend'))
    rig = bpy.data.objects['Mika_Rig']
    scene = bpy.context.scene
    report = json.loads((OUT/'rig-report.json').read_text())
    author_actions(rig, scene, report['sole_offsets'])
    report['actions'].update(Walk=WALK_FRAMES, IdleRelaxed=96)
    report['walk'] = {'stride_m': STRIDE, 'stance_fraction': STANCE, 'contacts': CONTACTS, 'visual_acceptance': False}
    (OUT/'rig-report.json').write_text(json.dumps(report, indent=2)+'\n')
    rig.animation_data.action = bpy.data.actions['Walk']
    scene.frame_start = 1
    scene.frame_end = WALK_FRAMES
    scene.frame_set(1)
    bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'mika-rig.blend'))
