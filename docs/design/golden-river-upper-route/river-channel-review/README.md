# River-carved cave playtest

At ascent bank 10, walk right down the mossy slope to enter the cave, or jump from the lip to follow the surface. The slope works in both directions. Follow the dry fountain's outflow to rooted shelves above a fern-lined waterfall. A lower ledge catches a missed crossing. A broken spillway marks the climb to a vine-framed opening, where two short hops connect the sloping banks back to surface bank 39.

## Play on Linux

Open a Linux cave development player directly to start at the lip. Use A/D or the arrow keys to move and Space to jump. Relaunch the executable for another entrance start. R retries from the first main checkpoint. The standard Golden River menu and the normal campaign start remain available through the regular game.

To reproduce the local executable, use the Unity version in ProjectSettings/ProjectVersion.txt with Linux standalone build support. Run the following from the project root with the Editor executable in UNITY_EDITOR. Use a project copy with its own Library when the main working copy is open in Unity. The graphics device must support the existing wide landscape panorama.

```sh
mkdir -p artifacts/stone-cave-playtest
"$UNITY_EDITOR" -batchmode -quit -projectPath "$PWD" -buildTarget Linux64 \
  -executeMethod BuildGame.BuildLinuxCavePreview \
  -previewOutput "$PWD/artifacts/stone-cave-playtest/GoldenRiverCave.x86_64" \
  -logFile "$PWD/artifacts/stone-cave-playtest/build.log"
./artifacts/stone-cave-playtest/GoldenRiverCave.x86_64
```

## In-game evidence

The two full-level recordings use real movement input and fixed-step simulation. Each frame represents four 0.02-second steps and plays at 12.5 frames per second. Both routes reach the exit with both main checkpoints.

- [Cave entrance to far rejoin](cave-route.mp4), 42.06 simulated seconds.
- [Whole level through the cave](full-cave.mp4), 5 min 39 s.
- [Whole level along the surface](full-surface.mp4), 5 min 34 s.
- [Missed crossing and local catch](catch.mp4), 3 s.
- [Gameplay at 640 × 360](route-640.jpg) and [the same views in grayscale](route-640-gray.jpg). The rows show the surface reference and bank tip, the lip and descent, the sill and fountain, the lower outcrop and waterfall, the recovery shelf and spillway footing, the inner and outer mouth, and the climb into daylight.
- [Surface approach](approach-960.png), [entrance](descent-entrance-960.png), [fountain](descent-niche-960.png), [waterfall](descent-risk-960.png), [spillway](spillway-foot-960.png), [exit arch](arch-foot-960.png) and [rejoin](descent-rejoin-960.png) at 960 × 540.

The exposed tips of the entrance and exit banks use the rounded ends of the original riverbank painting. Mossy bedrock takes over as the descent passes into shade. A continuous fluted edge with hanging roots follows the ceiling around the far bluff. The broken spillway sits on a recessed stone footing behind the playable shelves. The jumping shelves use the same root paintings and stone outlines as Golden River. Ferns, fiddleheads and bracket fungi mark the dry banks. The waterfall moves while the game runs and freezes when paused. Mika follows the incline only when the stone supports both sides of the grounded stance. The local controller check covers the entrance in both directions, both choices at the fork, the recovery climb, the far mouth, pause, retry, restart and the main river's water-entry sequence.

Player review remains open for entrance discovery, perceived difficulty, first-play timing, texture continuity and the composition of the climb. The scripted runs do not measure hardware frame rate. Exact-head desktop packaging and merge-ready validation follow player review on the draft PR.

[Paintings, generation prompts and provenance](../river-channel-assets.json)
