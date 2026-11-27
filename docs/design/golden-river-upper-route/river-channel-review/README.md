# River cave playtest

The cave under Golden River is a secret shortcut between ascent banks 10 and 39. Jumping the gap at bank 10 keeps Mika on the surface, as at every other bank. Dropping off the bank edge instead lands on the dry slope into the cave.

## Route, 2026-11-27 revision

1. **Entrance and fountain hall.** The slope leads to a hall with a knee-high boulder to hop and the rooted fountain. The roof over the boulder is about one unit higher than before, so a full jump clears it.
2. **Waterfall crossing.** Four shelves cross the falls: 2.4 to 3.2 units wide, with gaps of 3.4 to 4.2 units. The jumps keep about 25 to 35 percent of Mika's reach in reserve, against the Golden River target of 20 percent. A miss drops onto the catch below the shelves. A wall at the spillway closes it, so the only way on is back up the catch steps to the first shelf. The painted pool sits below the catch floor, so no water appears where Mika can fall through.
3. **Spillway.** A short level breather past the broken spillway.
4. **Seep shelves.** Six shelves climb toward daylight: one narrow landing, one long flat jump, rises of up to 1.5 units and one wide resting shelf. Two gullies below never lead forward. The first returns up the spillway slope. The second has a rock column back up to the wide shelf.
5. **Daylight bank.** A hollow in the bank needs a hop out. The last climb to bank 39 is about 33 degrees, well inside the walkable limit of about 45 degrees.

Over both jump sections, the camera lowers so that the floor a miss would land on is in view before takeoff.

## Timing

`UpperRouteTests.CaveIsFasterThanTheSurfaceAtFullSpeed` runs both routes from the bank 10 lip to bank 39. Both runs hold right and only jump at edges. The test fails unless the cave is faster. The surface run and the cave's takeoff points are in `UpperLevelPhysicsPilot` and `CaveSpeedPilot`. Scripted times prove reachability and relative speed. They are not human timings.

## Frame pacing during real play

Start any build with `--frame-log <folder>` and play through Golden River. On quit, `frame-times.csv` holds every frame's duration and Mika's position. `frame-summary.txt` compares the cave with the surface: median, 95th and 99th percentile, worst frame, and the number of frames over 25 ms and over 34 ms. See [the earlier camera draw measurements](performance.md) for GPU cost per view.

## Play on Linux

Build the cave preview with `BuildGame.BuildLinuxCavePreview` (it also re-bakes the cave masks) and run the executable. It starts at the bank 10 lip. Use A/D or the arrow keys to move and Space to jump. R returns to the first main checkpoint. The regular game starts at the beginning.

Review images and recordings of earlier cave prototypes were removed on 2026-11-27 to keep the repository small. They remain in Git history at commit `02b664e`.
