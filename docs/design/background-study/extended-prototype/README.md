# Extended painted prototype and input response

2026-10-30 follow-up to the initial checkpoint integration, requested by the user.
The playable route grows from 52 to 109 world units, preserving the first seven
platforms and adding seven more. All thirteen gaps use the existing motor/jump
constants. Four safe checkpoints are at (9, 0.52), (34, 1.52), (65, 1.52) and
(83, 0.52). The camera boundary follows the new finish at x=100.

## Overlay and evidence

The normal overlay is a 350x52-pixel translucent status/help strip inside the
16:9 viewport. Controls or pause reveals detailed controls, retry/restart/menu
actions and a smoothed frame-rate reading. Buttons release GUI keyboard focus
after use. No new gamepad binding is introduced.

- [Normal overlay](course.png)
- [Extended course finish](course-end.png)
- [Expanded paused controls](course-paused.png)

These are actual local Windows Development Player screenshots, including runtime
OnGUI. The existing preview build supports `--preview-view course`, `course-end`
and `course-paused`, together with `--preview-capture <absolute-output.png>`.
The finish screenshot uses a direct spawn fixture, not a claim of a complete run.
No artwork was generated or retouched. Earlier checkpoint recordings are retained
as historical evidence rather than overwritten.

## Jump feedback

The user reported approximately 26 fps in the Unity Editor, with delay most
noticeable in Mika's jump. At that rate a render frame lasts about 38.5 ms;
dynamic input followed by fixed physics and interpolation can add visible delay.
This does not establish why that particular Editor runs at 26 fps.

Update (2026-10-31): the user now observes approximately 300–400 FPS in the
Unity Editor. This supersedes the earlier observation; no controlled comparison
or measured improvement caused by this PR is claimed.

A grounded jump now consumes the buffered request in the input callback, updates
takeoff velocity/state and displays the existing takeoff drawing in the current
render frame. It does not move the body or simulate an extra physics step.
The next fixed step applies the same gravity, release cutoff and horizontal
acceleration as before. Airborne coyote/buffered requests still resolve in physics.
Motor tests compare the old and early-feedback arcs, including a short movement
pulse and jump release; real-course tests check the visible drawing before the
next physics step. Existing input-fixture tests exercise actual keyboard events.
All 152 local Unity tests passed, including all four checkpoint recoveries,
thirteen real-physics gap crossings, keyboard-to-controller feedback, unchanged
fixed-step trajectories and the existing animation/water/menu regressions.

## Render work and limits

The foreground source is 1536x1024; its first alpha>127 row from the top is 777.
Opaque content therefore ends below v=0.242. The water mesh now covers v=0..0.25,
retaining a filtering margin and the same world/UV mapping. This removes 75% of
the empty quad area without changing painting pixels, wave formulas, stones or
animation strength. A source-alpha regression check protects that margin.
Parallax factors are cached instead of allocating an array every render update.

`SceneryPerformanceReview.MeasureWater` reproduces a 1280x720 full/cropped mesh
comparison with alternating samples and synchronous GPU readback. Twelve samples
per variant after warm-up gave medians of 33.316 ms (full) and 33.978 ms (cropped)
on the local validation run: **no end-to-end speed improvement was demonstrated**.
Readback/Editor overhead is included, so these numbers are not gameplay FPS or
physical key-to-screen latency. Mean channel difference was 0.00165/255, with
maximum summed RGB difference 8 from rasterization precision. An earlier shader
branch/clip experiment did not show a benefit and was discarded; the original
water shader is unchanged. No fixed FPS gain or resolution reduction is claimed.

Run the review in an isolated project/Library when the working Editor is open.
Output is `Logs/scenery-performance/render-comparison.json`;
the [recorded result](render-comparison.json) is retained here. To diagnose remaining
Editor slowness, compare the Controls fps reading with the Game view focused and
after background imports/builds have finished. The input change removes an extra
render-frame wait for takeoff feedback; it does not repair an unexplained low
Editor frame rate.
