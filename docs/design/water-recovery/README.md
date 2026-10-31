# Mika water recovery — first playable version

Issue #60. The user authorized an integrated first version on 2026-10-31 after
requesting a splash, sinking face close-up, short blackout and checkpoint return.
This is a review prototype; visual acceptance and final merge validation remain open.

[Normal-speed Unity preview](recovery.mp4) · [Half-speed preview](recovery-slow.mp4)

![Face close-up](face-closeup.png)

![Head submerging](head-submerging.png)

## Playback

Open the normal main scene in Unity 6000.3.27f2, restart Play Mode after importing
this branch, enter the movement lab and walk off a platform into the river.
Mika splashes, the camera closes in, four complete-character drawings show a
surprised eye, upward glance, blink and closed eyes, then the water covers her head.
The screen fades to black and returns to the last activated checkpoint, or the
course start before any checkpoint. The sequence takes 2.5 seconds.

For quicker repetition, while playing the movement course select
`LegendOfCat > Water recovery > Preview first gap`. It places Mika over the first
gap; normal falling triggers entry. Outside Play Mode the command opens an unsaved
course preview after offering to save the current scene, then enters Play; invoke
it again to drop Mika. R retries immediately; Backspace restarts the whole run;
P pauses; Escape returns to the menu. Pause freezes the recovery clock. Movement
and jump cannot interrupt submersion. Manual retry clears all recovery effects.
The independent background-study lab uses its existing return-to-start behavior.

## Implementation and limits

`WaterRecovery` intercepts descending collider contact at course-local y=-3.25,
registered with the foreground water. It suspends body simulation, presents rigid
complete drawings through a world-space waterline clip, creates droplets/ripples,
and calls the course's existing retry callback once under black. `CatCamera`
yields during the close-up; scenery follows that camera before rendering.
No persistent format, checkpoint policy or movement ability is added.

The surface is a level plane matching the prototype river, not per-pixel buoyancy
or freely swimmable water. The waterline mask has a small wave. Four reaction
frames and the change from the existing airborne art remain subject to user
review. There is no new sound. Generated expressions are prototype artwork, not
acceptance of a replacement locomotion set. Current running/airborne drawings are
retained outside recovery. Close framing may expose scenery reuse more clearly.

`WaterRecoveryPreview.Prepare` reproducibly imports the source without power-of-two
distortion, mipmaps or texture compression. `WaterRecoveryPreview.Capture` renders
76 deterministic frames at 30 fps under ignored `Logs/water-review`. Camera.Render
does not invoke IMGUI, so this authoring capture applies the same fade numerically.
These are Unity-rendered presentation captures, not a recording of player input.

## Coordination

Ownership: water entry/recovery, the controller's recovery lock, camera handoff,
course/study attachment, the new reaction sheet, authoring tool and scoped tests.
Open PRs #51 (Blender character authoring), #53 (keepsake decision docs) and #59
(CI/licensing) have no overlapping files. Issue #60 has no native blocked-by
relationships at inspection. Existing local Linux toolchain package edits are
unrelated and excluded from this branch's commits. No serialized scene/prefab
editing is needed; runtime course construction attaches the behavior.

## Validation

- Unity 6000.3.27f2 imported and compiled the first version.
- Three focused EditMode tests passed in an isolated project with its own Library:
  descending entry and single checkpoint retry after blackout, pause/manual retry
  cleanup, and rejection of upward entry. A translated/scaled course is exercised.
- The final scoped regression run passed 14/14 tests across WaterRecoveryTests,
  CatCollisionTests and RunLifecycleTests, with zero failures or skipped tests.
- Unity-rendered review frames were inspected; timing was adjusted to hold the
  facial reaction longer, and texture import was corrected to preserve proportions.
- Final regression results are recorded in the PR. User Play Mode visual review,
  Windows/Linux player and launcher builds, and exact-revision merge validation
  remain pending. Keep the PR draft. No merge or release is authorized.
