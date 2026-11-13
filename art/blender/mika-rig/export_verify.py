"""Export only the deforming character and measure native/FBX motion fidelity.

Run with Blender --background --python export_verify.py after build_rig.py.
Mechanical results do not establish artwork or Unity acceptance.
"""
import bpy
import json
import math
from pathlib import Path

OUT = Path(__file__).resolve().parent
bpy.ops.wm.open_mainfile(filepath=str(OUT / 'mika-rig.blend'))
rig = bpy.data.objects['Mika_Rig']
skin = bpy.data.objects['Mika_Skin']
scene = bpy.context.scene
source = json.loads((OUT / 'rig-report.json').read_text())
assert all(v.groups for v in skin.data.vertices)
assert all(abs(sum(g.weight for g in v.groups) - 1) < 1e-5 for v in skin.data.vertices)
assert all(len(v.groups) <= 4 for v in skin.data.vertices)
assert all(all(math.isfinite(c) for c in v.co) for v in skin.data.vertices)
legs = ['Front.L', 'Front.R', 'Hind.L', 'Hind.R']
soles = {}
for key in legs:
    group = skin.vertex_groups[key + '.Paw'].index
    soles[key] = [v.index for v in skin.data.vertices
                  if any(g.group == group and g.weight > .75 for g in v.groups)]
metrics = {}
samples = {}
for name, length in source['actions'].items():
    rig.animation_data.action = bpy.data.actions[name]
    heights = []
    errors = []
    bounds = []
    previous_support = {}
    stance_displacements = []
    step = .5 if name == 'Run' else 1
    first_pose = None
    for sample in range(round(length/step) + 1):
        frame=1+sample*step
        scene.frame_set(int(frame),subframe=frame%1)
        if first_pose is None:
            first_pose={b.name:b.matrix.copy() for b in rig.pose.bones}
        evaluated = skin.evaluated_get(bpy.context.evaluated_depsgraph_get())
        for key in legs:
            errors.append((rig.pose.bones[key + '.Lower'].tail -
                           rig.pose.bones['CTRL_' + key].head).length)
            if name in {'Idle', 'LookBack', 'Listen', 'TailTwitch', 'Stretch'} or (name == 'Groom' and key != 'Front.L'):
                heights.append(min(evaluated.data.vertices[i].co.z for i in soles[key]))
            if name == 'Run':
                delay = source['run']['contacts'][key]
                t = (frame - 1) / length
                u = (t - delay) % 1
                if u < source['run']['stance_fraction']:
                    heights.append(min(evaluated.data.vertices[i].co.z for i in soles[key]))
                    point = rig.pose.bones[key + '.Paw'].head.copy()
                    point.x += source['run']['stride_m'] * t
                    if key in previous_support:
                        old_frame, old_point = previous_support[key]
                        if old_frame == frame - step:
                            stance_displacements.append((point - old_point).length)
                    previous_support[key] = (frame, point)
                else:
                    previous_support.pop(key, None)
        if name == 'Run' or frame in {1, length // 2 + 1, length + 1}:
            samples[(name, frame)] = {key: rig.matrix_world @ rig.pose.bones[key].head
                                      for key in ['Head', 'Front.L.Paw', 'Hind.R.Paw'] +
                                      [f'Tail.{i}' for i in range(1, 6)]}
        if name == 'Run':
            bounds.append(tuple(rig.pose.bones['Head'].head))
    metrics[name] = {'max_ik_target_error_m': max(errors),
                     'support_sole_min_m': min(heights) if heights else None,
                     'support_sole_max_m': max(heights) if heights else None,
                     'max_stance_displacement_per_frame_m': max(stance_displacements) if stance_displacements else None}
    if name=='Run':
        seam=max(abs(b.matrix[row][col]-first_pose[b.name][row][col]) for b in rig.pose.bones for row in range(4) for col in range(4))
        assert seam<1e-5, 'Running loop does not close'
        metrics[name]['loop_seam_matrix_error']=seam
# Resting soles and fixed controller scales are useful regressions, not a visual gate.
print('RUN_CONTACTS', metrics['Run'], flush=True)
assert max(abs(metrics['Idle'][k]) for k in ['support_sole_min_m', 'support_sole_max_m']) < .006
assert metrics['Run']['max_ik_target_error_m'] < .001
assert metrics['Run']['max_stance_displacement_per_frame_m'] < .001
assert max(abs(metrics['Run'][k]) for k in ['support_sole_min_m', 'support_sole_max_m']) < .003
rig.animation_data.action = bpy.data.actions['Idle']
scene.frame_set(1)
bpy.ops.object.select_all(action='DESELECT')
rig.select_set(True)
skin.select_set(True)
bpy.context.view_layer.objects.active = rig
bpy.ops.export_scene.fbx(
    filepath=str(OUT / 'mika-rig.fbx'), use_selection=True,
    object_types={'ARMATURE', 'MESH'}, axis_forward='-Z', axis_up='Y',
    apply_unit_scale=True, add_leaf_bones=False, use_armature_deform_only=True,
    bake_anim=True, bake_anim_use_nla_strips=False, bake_anim_use_all_actions=True,
    bake_anim_force_startend_keying=True, bake_anim_simplify_factor=0,
    bake_anim_step=.5,
    path_mode='COPY', embed_textures=True)
# Reimport into an empty scene; validate the actual exported artifact.
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=str(OUT / 'mika-rig.fbx'))
armatures = [o for o in bpy.data.objects if o.type == 'ARMATURE']
meshes = [o for o in bpy.data.objects if o.type == 'MESH']
assert len(armatures) == 1 and len(meshes) == 1
assert len(armatures[0].data.bones) == source['deform_bones']
assert not any(b.name.startswith(('CTRL_', 'POLE_')) for b in armatures[0].data.bones)
assert len(meshes[0].data.vertices) == source['skin_vertices']
assert any(m.type == 'ARMATURE' for m in meshes[0].modifiers)
actions = [a.name for a in bpy.data.actions]
assert all(any(name in action for action in actions) for name in source['actions'])
roundtrip_errors = []
imported = armatures[0]
for (name, frame), positions in samples.items():
    imported.animation_data.action = next(a for a in bpy.data.actions if a.name.endswith('|' + name))
    bpy.context.scene.frame_set(int(frame),subframe=frame%1)
    for key, point in positions.items():
        roundtrip_errors.append((imported.matrix_world @ imported.pose.bones[key].head - point).length)
assert max(roundtrip_errors) < .001, 'FBX baked motion differs from native bone positions'
report = {'normalized_weights': True, 'maximum_influences': 4,
          'fbx_deform_bones': len(armatures[0].data.bones),
          'fbx_meshes': len(meshes), 'fbx_actions': actions,
          'native_motion_metrics': metrics,
          'max_fbx_sample_position_error_m': max(roundtrip_errors),
          'unity_import_tested': False, 'visual_acceptance': False}
(OUT / 'validation.json').write_text(json.dumps(report, indent=2) + '\n')
print('EXPORT_VALIDATION', json.dumps(report), flush=True)
