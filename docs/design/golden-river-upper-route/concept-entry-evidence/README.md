# Approved-concept entrance: in-game review candidate

The owner approved the new entrance concept and requested implementation on 2026-11-14. These are actual Unity captures of that implementation, not generated concept pictures. They supersede the earlier natural-bank candidate for entrance review. The full route remains a draft feature awaiting visual acceptance.

## Captures and motion

Five poses (0, 3, 5, 7 and 9) each have 960x540 color, 640x360 color and 640x360 grayscale captures. They cover the main bank, descent, massive overhang, enclosed throat and first passage. Camera zoom and depth remain fixed. Contact edges remain readable in the small grayscale views.

`entrance-motion.mp4` records the upstream approach and descent through the first enclosed passage. `entrance-reverse.mp4` records the climb out and approach in both directions. The reverse clip ends before the separate diagnostic respawn used to check incline idle/jumping. Both clips use actual controller input and 0.02-second physics steps sampled at 10 fps, encoded at normal simulation speed. They are not human timings or hardware FPS measurements.

## Verification

`physics.txt` records the complete forward cave traversal, concealed catch and return, passage/fountain seam in both directions, reverse entrance, connected approach, incline idle/jump, water recovery, pause, retry and restart. Ascent landings 9, 10, 11, 12, 33, 34 and 39 each have one effective registered collider; their old source platforms do not remain underneath the new formation.

The pinned Unity 6000.3.27f2 EditMode suite passed 239/239 tests with zero failures on the final terrain revision. The camera regression additionally requires the alcove upper seam to remain buried in solid roof above the riverbed, preserving the main-route sky. No renderer enable state, layer or camera depth changes while traversing.

The Windows development preview build and packed-art audit passed: 64 active paintings, five shader-reference materials and seven prepared masks. A real visible Windows player capture confirmed the compiled art loads correctly, with no shader/runtime exceptions in its log. First Golden River construction in that fresh player process measured 640.0 ms; see `startup-player.txt`. This is level construction with OS/driver caches intact, not a cold-storage or total application startup benchmark.

Editor Golden River construction measured 777.7 ms in this review run. Textures were already imported and OS/driver caches were not cleared. `texture-memory.txt` is Unity's informational renderer-texture estimate, not process memory or a frame-rate benchmark; the owner removed the earlier hard cap.

Native cliff faces and the new slate foundation preserve their source resolution. [Prompts and provenance](../concept-entry-assets.json) and [the approved concept](../approved-entrance-concept.png) are retained separately. No screenshot overlay, package or new game mechanic was added.

## Remaining review

Owner acceptance of the actual entrance remains open. Remaining full-route art, fountain lighting, far exit refinement, human duration, hardware frame-rate comparison, current-main integration and exact-head Windows/Linux player/launcher/package/merge-ready checks are later draft work. No merge or release is authorized.
