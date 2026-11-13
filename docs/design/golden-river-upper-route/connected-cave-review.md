# Connected cave review

The owner accepted the corrected entrance and requested integration into the actual level with a separate return. This supersedes the isolated endpoint described in the earlier study documents; the original complete cave composition remains rejected.

The accepted native entry and fountain chamber now share the campaign player, input, checkpoint state and camera. The return travels forward below the river and ascends to main ascent platform 39, approximately 241 world units beyond the entrance. Main-route platform contacts, two checkpoints, finale and movement constants are retained. The old broad catch below entrance platform 10 is replaced by the registered native shore ledges; other recovery geometry is permanently carved at the two shore openings.

## Spatial construction

`CaveRoute` defines the entrance/rejoin, water/shore contours, connected return banks, a deep-fall boundary and the concealed local catch. Swept water crossing stays hazardous above the lower cave; being horizontally over the interior never protects a fall through the river. The same world contours produce the permanent river mask and dry-stone treatment. There is no long horizontal water exemption.

`CaveLevelConnection` connects native paintings and actual collision to the existing course. Generic floor variants share a continuous deep stone grain; the roof uses its native underside. Interior foundations and ceiling masses extend past the viewport. A separate rescue shaft returns from the central miss to its approach. It has no checkpoint and is outside the normal takeoff camera.

The existing orthographic camera retains its zoom and Mika scale. It follows the real descent and ascent. Its depth moves behind the foreground river only after that plane is above the viewport, and returns to the exterior after the river is below the viewport. Interior lining belongs to a camera-only interior layer, with every renderer permanently enabled; this layer is added/removed from the camera only during those out-of-view transitions. Static recessed rock opens toward the exit through a fixed world-space fade. This is a rendering accommodation for the existing foreground river, not a player-triggered background scene replacement. Visual review must include these transitions in both directions.

The fountain, side light, twelve paused dust motes and empty future collectible anchor are retained. No item, enemy, switch, ability, checkpoint or persistent format is added. The main course can be played while skipping the cave.

## Reproduction and acceptance

The Windows development preview starts at the real level entrance with `--preview-view level-one-descent`. A/D or arrows move, Space jumps, P pauses, R retries the main checkpoint and Escape opens the menu. Retry intentionally does not create a cave checkpoint. The full level remains available through the normal menu.

The PR remains draft pending player review and final exact-revision validation. Scripted times demonstrate physical traversal, not first-player duration or approved artwork. Local captures and checks are recorded below after execution. Windows/Linux player and launcher builds, package validation and merge-ready remain the final completion gate. No merge or release is authorized.

## Coordination

This branch and draft PR #79 own the sequential cave integration. No concurrent agent or shared scene/metadata ownership is involved. PR #61 water behavior remains separate; this change preserves the existing river animation and supplies spatial crossing/coverage through existing optional controller hooks. Native blocked-by relationships remain authoritative.

## Local verification — 2026-11-13

- Pinned Unity 6000.3.27f2 full local suite: 231/231 passed, zero failures/skips. This includes actual full-level completion with the cave and while skipping it, both main checkpoints and the finale.
- Final focused cave suite: 8/8 passed. The integrated fixed-physics review additionally verifies the complete connection, central miss and rescue return, reversible entrance, main-water falls above the interior, pause, retry, full restart and unchanged renderer enable states.
- The direct scripted cave traversal is 45.04 seconds. This is a known scripted measurement, below the earlier roughly 60-second target; human pacing and discovery time remain unmeasured and require player review. Do not present it as a human run.
- Eight viewpoints are captured at 960x540 and 640x360, including grayscale. The current normal-speed connection recording uses 10 frames/second with actual 0.02-second physics steps, without speed changes. See [committed evidence](connected-evidence/README.md).
- Windows development preview and packed-art audit succeeded. The pack contains 75 active paintings and the required shader materials, excluding archived art. Imported descent textures total 90,205,312 bytes; the generated RGBA area mask adds 16,777,216 GPU bytes. Their conservative combined texture allocation is approximately 102 MiB, below 128 MiB. This is not an FPS or complete process-memory measurement.

Full-level updated recordings with/without the cave, human difficulty/visual acceptance, same-hardware frame-rate comparison and exact-head Windows/Linux player/launcher/package/merge-ready validation remain final PR acceptance work. Earlier rejected full-route recordings do not establish those results for this version. The playable level connection itself is available for the owner's test now.
