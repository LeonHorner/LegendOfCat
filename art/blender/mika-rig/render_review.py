"""Render review-only animation frames from the saved rig; never alter gameplay."""
import bpy
import sys
from pathlib import Path
from mathutils import Vector
OUT=Path(__file__).resolve().parent
CACHE=Path('/tmp/mika-authoring/review-frames');CACHE.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=str(OUT/'mika-rig.blend'))
scene=bpy.context.scene;rig=bpy.data.objects['Mika_Rig']
scene.cycles.samples=8;scene.render.resolution_x=480;scene.render.resolution_y=400
camera_location=scene.camera.location.copy();camera_scale=scene.camera.data.ortho_scale;camera_rotation=scene.camera.rotation_euler.copy()
requested=set(sys.argv[sys.argv.index('--')+1:]) if '--' in sys.argv else set()
for name,length,step in [('Run',12,.5),('Idle',48,4),('Groom',96,6),('Stretch',72,6),('LookBack',72,6),('Listen',48,4),('TailTwitch',48,4),('JumpReview',48,3),('RunOblique',12,.5)]:
    if requested and name not in requested:continue
    scene.camera.location=camera_location;scene.camera.rotation_euler=camera_rotation;scene.camera.data.ortho_scale=camera_scale
    if name=='Run':
        scene.camera.location=(-.05,1.5,.37);scene.camera.data.ortho_scale=.86
        scene.camera.rotation_euler=(Vector((-.05,0,.25))-scene.camera.location).to_track_quat('-Z','Y').to_euler()
    if name=='RunOblique':
        scene.camera.location.x-=.04;scene.camera.data.ortho_scale=.86
    if name=='JumpReview':
        scene.camera.location.z+=.075;scene.camera.data.ortho_scale=.88
    rig.animation_data.action=bpy.data.actions['Run' if name=='RunOblique' else name]
    folder=CACHE/name;folder.mkdir(exist_ok=True)
    for index in range(round(length/step)):
        frame=1+index*step
        scene.frame_set(int(frame),subframe=frame%1);scene.render.filepath=str(folder/f'{index:03d}.png');bpy.ops.render.render(write_still=True)
    if name=='Run':
        scene.frame_set(5);scene.render.filepath=str(OUT/'pose-run.png');bpy.ops.render.render(write_still=True)
print('REVIEW_FRAMES_READY',flush=True)
