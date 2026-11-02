# Painted checkpoint prototype

Historical first integration: seven platforms and two checkpoints. The
[extended prototype](../extended-prototype/README.md) supersedes this route/HUD.
For current reproduction use the project's pinned Unity 6000.3.27f2. The capture
version below records the historical run and is intentionally retained.

Captured in Unity 6000.3.27f1 on 2026-10-30 using the actual menu Resources
MovementCourse prefab, CatController, Mika presentation and RunLifecycle.
The seven-platform layout and checkpoint coordinates are unchanged.

- [Opening view](opening.png)
- [Second checkpoint](checkpoint-two.png)
- [Eight-second review](review.mp4)
- [Recorded lifecycle/position data](playback.csv)

This is a scripted camera-only review, not a continuous manually played traversal.
The camera render does not include the runtime OnGUI control panel. At frame 25,
the fixture places Mika on checkpoint one; at 60 it induces a fall. Frame 90 pauses
and frame 100 retries. Frame 120 places Mika on checkpoint two, frame 145 induces
another fall, and frame 175 restarts the whole run. The recording runs at 25 fps,
with two 0.02-second physics steps per recorded frame. Recorded lifecycle state
confirms activation, fall recovery, pause/retry and full restart. Water capture
time advances explicitly; normal gameplay uses the native shader clock.

The former checkpoint-capture menu and batch recorder were removed with the
standalone scene. Existing frames and CSV remain historical evidence. For current
behavior, run the movement course from the main menu and its Unity integration tests.
The archived capture originally used `Logs/checkpoint-scenery-review/` at 25 fps.
Historical recordings used a separate Unity project and Library.

Open `Assets/Game/Scenes/Main.unity`, press Play, choose **Movement lab (prototype)**,
then **Enter movement lab** for interactive testing. Cross gold markers on safe
ground. R retries, Backspace restarts, P pauses, Esc returns to the menu.

Local validation: 148 Unity tests passed, including a real-physics integration
test that activates both checkpoints, falls/retries, resumes from pause and fully
restarts. The test also checks that scenery reuses one player and the original
seven platform colliders, and that the water material references its shader.
Final exact-revision Windows/Linux builds and full tests are tracked by PR #54.
