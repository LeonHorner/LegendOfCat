# River-carved cave playtest

This revision retains the surface fork on ascent bank 10 and the rejoin on bank 39. Walk off the lip to descend or jump across to continue along the surface. A low scoured basin leads to a dry fountain lit through a side fissure. Deeper shelves cross the old river channel above a recoverable fall. The broad return bends upward past an exposed root system and emerges on a continuous stepped bank before the second main checkpoint.

## Play on Linux

Open a Linux cave development player directly to start at the lip. Use A/D or the arrow keys to move and Space to jump. Relaunch the executable for another entrance start. R retries from the first main checkpoint. The standard Golden River menu and the normal campaign start remain available through the regular game.

To reproduce the local executable, use the Unity version in ProjectSettings/ProjectVersion.txt with Linux standalone build support. Run the following from the project root with the Editor executable in UNITY_EDITOR. Use a project copy with its own Library when the main working copy is open in Unity. The graphics device must support the existing wide landscape panorama.

```sh
mkdir -p artifacts/cave-playtest
"$UNITY_EDITOR" -batchmode -quit -projectPath "$PWD" -buildTarget Linux64 \
  -executeMethod BuildGame.BuildLinuxCavePreview \
  -previewOutput "$PWD/artifacts/cave-playtest/GoldenRiverCave.x86_64" \
  -logFile "$PWD/artifacts/cave-playtest/build.log"
./artifacts/cave-playtest/GoldenRiverCave.x86_64
```

## In-game evidence

The two full-level recordings use real movement input and fixed-step simulation. Each frame represents four 0.02-second steps and plays at 12.5 frames per second. Both routes reach the exit with both main checkpoints.

- [Cave entrance to far rejoin](cave-route.mp4), 47.82 simulated seconds.
- [Whole level through the cave](full-cave.mp4), 5 min 44 s.
- [Whole level along the surface](full-surface.mp4), 5 min 34 s.
- [Missed crossing and local catch](catch.mp4), 3 s.
- [Gameplay at 640 × 360](route-640.jpg) and [the same views in grayscale](route-640-gray.jpg). Each sheet runs left to right across four rows: entrance, channel sill, fountain, fissure, recovery shelf, rooted climb, exit mouth, rejoin.
- [Entrance](descent-entrance-960.png), [fountain](descent-niche-960.png), [climb](return-bench-960.png) and [rejoin](descent-rejoin-960.png) at 960 × 540.

In the captures, the gold grass disappears in the enclosed passage. Rock bedding continues across the sill and the far stairs. The angled light leads toward the fountain and the rising branch exposes the root wall. The risk and catch views show the vertical separation between the intended landing and the recovery shelf. The local controller pilot exercises the catch, its return jumps, both mouths, pause, retry, restart and the main river water-entry sequence.

Player review remains open for entrance discovery, perceived difficulty, first-play timing, texture continuity and the composition of the climb. The scripted runs do not measure hardware frame rate. Exact-head desktop packaging and merge-ready validation follow player review on the draft PR.

[Paintings, generation prompts and provenance](../river-channel-assets.json)
