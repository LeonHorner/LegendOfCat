# Permanent shore review evidence — 2026-11-13

This is the replacement for the rejected campaign integration in `../connected-evidence`. It is a review candidate, not owner approval. See [construction and remaining acceptance](../permanent-shores.md).

## Recordings

- `forward-route.mp4`: upstream main-route approach, entrance, enclosed passage, fountain and forward exit to the later main bank. Direct scripted time: 48.64 seconds including approach. This is not a human timing and is below the earlier roughly 60-second target.
- `exit-return.mp4`: descend the outdoor stairs and re-enter the same permanently visible mouth.
- `entry-return.mp4`: climb back through the original entrance using every real contact.
- `recovery.mp4`: miss the central jump, land on the hidden local catch, then return through its separate shaft. The explicit test starts above the miss; it is not a continuous human recording.

All clips use the actual controller, colliders, 0.02-second simulation steps and continuous camera. Sampling is 10 fps, normal speed, without retiming. `frames.txt` beside each original PNG sequence supplies the exact frame count, preventing stale trailing images from older runs. This capture does not measure runtime FPS. Water and dust advance on the same simulation clock.

## Still views

Every view has a 960x540 color image, a `-640.png` 640x360 image and a `-gray.png` grayscale image at 640x360. These are actual Unity camera renders; selected poses are placed deliberately to review composition, separate from the continuous traversal recordings.

| Index | Subject |
| --- | --- |
| 0 | Entrance bank at the main contact |
| 1 | Descending entrance with the opposite river shore |
| 2 | Fountain contact, roof and side light |
| 3 | Central takeoff; catch below viewport |
| 4 | Local catch after falling |
| 5 | Enclosed approach to the exit |
| 6 | Final exterior steps approaching the main bank |
| 7 | Rejoined main contact |
| 8 | Upstream approach and water before the entrance |
| 9 | Attached recovery ledges and bearing rock |
| 10 | Permanent mouth from the outer landing |
| 11 | Looking back toward the mouth from outside |

## Executed checks

The fixed-physics review checks all forward targets, reverse exit and entrance, recovery return, water falls above the interior, pause, retry and restart. It also checks unique native main contacts, masked water boundaries inside actual solid rock, fixed camera depth/mask and unchanged renderer enable states throughout. No cave checkpoint is introduced.

- Pinned Unity 6000.3.27f2 full local suite: **236/236 passed**, zero failures or skipped tests. It includes complete main-level runs with and without the cave, both main checkpoints, spatial water behavior, fixed camera framing, concave terrain triangulation and actual shore registration.
- Windows development build and packed-art audit: 80 active paintings, archived art excluded. Imported descent textures total 106,987,008 bytes; the generated RGBA mask adds 16,777,216 GPU bytes. Their conservative combined allocation is approximately 118 MiB, below 128 MiB. This is not complete process-memory or frame-rate measurement. Final shore-color/feathering adjustments are additionally checked by the recorded actual camera views and player shader compilation.
- The rebuilt Windows player was launched with `--preview-view level-one-descent` and its actual visible game window was inspected at the upstream bank. This establishes the runnable preview, not human approval of the complete route. The development player remains open for the owner's test.

Human visual acceptance, first-player timing, same-hardware frame-rate comparison and final exact-head Windows/Linux player/launcher/package/merge-ready validation remain pending. PR #79 stays draft. No merge or release is authorized.
