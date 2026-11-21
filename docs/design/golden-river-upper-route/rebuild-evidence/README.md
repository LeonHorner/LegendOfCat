# Entrance rebuild review candidate

The latest entrance work follows the newly owner-approved concept. See [current concept implementation evidence](../concept-entry-evidence/README.md). The natural-bank evidence below predates that new art direction and is retained as history.

The latest correction is `natural-approach-960.png` with 640-pixel/grayscale variants, `natural-bank-physics.txt` and `natural-bank-validation.txt` (239 tests). The owner rejected the preceding three-terrace connected-approach candidate. Its images and older motion clips predate the continuous incline and rounded water contact and are historical evidence.

This is the first visual review slice, not acceptance of the complete route. The owner must evaluate the integrated entrance through its first closed passage before the remaining artwork is produced. The far return is currently a tested geometry prototype.

## Evidence

- `entrance-motion.mp4`: actual controller descent from the upstream approach through the first enclosed passage.
- `entrance-reverse.mp4`: the same entrance climbed in reverse with actual jump input.
- Six camera poses have 960x540 color, 640x360 color and 640x360 grayscale versions: upper bank (0), first lower hint (1), descending ledges (2), throat (6), first passage (8), and first passage crossing (9).
- The two clips use 0.02-second physics steps sampled every five steps and encoded at 10 fps. They are normal simulation speed, not a human run or hardware performance benchmark. Camera follow remains continuous.
- `geometry.txt` records forward reachability, the concealed catch return, the previously mismatched flat join, reverse entrance, water, pause, retry and restart checks.
- `texture-memory.txt` reports Unity's runtime estimate for distinct cave renderer textures and coverage masks. The owner removed the hard memory cap; this is informational, not a quality gate or a before/after frame-rate measurement.
- `tests.txt` records the regression suite. Final exact-head player/launcher builds and merge-ready remain later draft work.
- `startup-before.txt`, `startup-after.txt` and `startup-after-final.txt` record equivalent editor level construction on the same hardware: 3237.0 ms before precomputation, then 802.1 ms and 771.0 ms in separate runs. These single observations already have authoring textures imported; they are not cold-player timings. The prepared masks are checked pixel-for-pixel against procedural generation.
- `startup-player.txt` records 486.8 ms for the first Golden River construction in a fresh Windows preview process. OS and graphics-driver caches were not cleared. This measures the same `OpenLevelOne` method used by the menu button, not a general hardware benchmark or total application launch time.

## Review focus and limitations

Check the two river contacts, unchanged main route readability, correspondence of paws/contact edges, continuous cover into the passage, and material consistency while walking and reversing. The opaque body and static composite use identical source contours. Native entrance details use six-unit source frames; import preserves original non-power-of-two resolution rather than reducing to 1024 pixels. Painted boundary detail and vegetation sit over opaque stone, so their alpha cannot reveal the sky through a solid floor.

The full cave artwork, final exit treatment, human duration and hardware comparison are not complete. Unity archived 27 rejected prototype paintings with their metadata; retained standalone study assets remain available. Earlier terrain-correction evidence was rejected and does not apply to this candidate. No merge or release is authorized.
