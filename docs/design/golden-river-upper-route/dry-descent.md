# Dry riverbank descent

## Superseded composition

The user rejected the open cave composition in the playable preview on 2026-11-10: exposed water/wall cuts, detached rock pieces, an entry unlike the reference, insufficient enclosure and a floating fountain. The [enclosed cave redesign plan](cave-polish-plan.md) governs the next revision. The implementation and measurements below describe the previous prototype and do not establish visual acceptance.

## Current direction — 2026-11-10

The user rejected the elevated garden and its position-dependent renderer reveal. The replacement is a permanent dry cleft below the riverbank, with two small shore openings, a concealed local catch, a dry rooted fountain niche and a separate ascent. The entry and niche reference paintings in this directory define the visual target; they are not runtime background panels.

The main route, checkpoints and finale retain their existing collision definitions. Water remains animated and hazardous between the two shore openings, including above the underground room. No collectible, optional checkpoint, ability or save-format change is introduced. `FutureCollectibleAnchor` reserves an empty location beside the fountain.

## Implementation

`UpperRoute` retains its existing asset identity but now defines the descent geometry and two-dimensional coverage. Swept water crossings are tested before underground dry-space exceptions, so falling from the main route cannot bypass the river. The controller's optional recovery delegate leaves other courses on their original boundary behavior. The same coverage produces the water mask and suppresses automatic wet-rock coloration inside the dry passage.

The camera follows the dry passage below zero with constant size. Main-route scenery leaves the image through descent. The catch is below the takeoff frame and becomes visible during a fall. Renderers never change activation in response to player position. The discarded garden paintings are moved out of Resources using the Editor asset database, preserving their GUIDs.

Artwork uses separate permanent cutouts and a quiet rock lining. The unique rooted fountain appears once. Lighting remains painted and uses the existing unlit rendering approach, with an original procedural shaft and twelve slow procedural dust sprites. Dust checks the controller's pause state. No package or rendering-pipeline change is required.

## Evidence and remaining acceptance

The geometry pilot completed the direct route in **63.60 simulated seconds**, including its short settling steps. This is scripted timing, not a human first-play measurement. An earlier geometry revision also passed the full local catch return and lifecycle checks. The five targeted spatial-water, coverage-mask and camera tests passed before final art integration.

Final artwork registration, normal-speed traversal recordings, current full-suite results, imported-memory measurements and human review remain required. Existing upper-route and garden videos/images in this directory are historical rejected evidence and do not validate this descent. PR #79 stays draft during this work. The exact new head still requires Windows/Linux player and launcher builds and merge-ready validation before readiness; no merge or release is authorized.

Asset source filenames and exact generation prompts are recorded in [descent-assets.json](descent-assets.json). Runtime procedural sources are `DescentLight.cs` and `UpperRoute.CreateMask`.
