# Permanent cave shores

Current follow-up: [terrain consistency corrections](terrain-corrections.md). The owner found defects in the first shore candidate; its original screenshots are historical, not accepted artwork. The correction bounds the backdrop, repairs joins/supports and restores terrain detail density.

This replaces the rejected campaign integration recorded in `connected-cave-review.md`. The accepted isolated entrance remains the art reference; the former camera-depth/layer transition and rectangular overlaid banks are removed. PR #79 remains a draft for owner review.

## Landscape and collision

Five native cutouts provide a low entrance bank, opposing bank/riverbed, exit cliff, continuous exit staircase and attached recovery ledge. Main ascent contacts 10, 11, 33, 34 and 39 are supplied by these registered terrain contours. Their original renderers and colliders are not created. Other main contacts, movement constants, checkpoints and the fountain remain unchanged.

The entrance bank is low enough to approach from the preceding platform. Its downward ledges remain reversible. The exit continues through a permanent side opening and climbs the outer rock to platform 39, over 230 world units farther along the level. Looking back or re-entering uses the same opening.

`CaveRoute` separately describes bounded shore-water envelopes, dry playable air and solid-bearing terrain. A stepped passage no longer cuts its staircase silhouette out of the water. The generated mask uses the same water coverage as the hazard query. Swept water detection checks the player's feet before the submerged bed can stop the body; standing horizontally above the cave does not grant protection.

Water-mask edges lie inside rock. Permanent foreground wet footings and riverbed masses hide the lower water-texture boundary. A narrow irregular dark wet contact and restrained boundary foam join river and rock. The rest of the main river stays animated and hazardous. The riverbed tapers beyond this feature, avoiding a rectangular end when the camera follows the exit.

Native contour polygons drive collision and visible silhouettes. Concave outlines use ear-clipped triangulation rather than fans spanning an air opening. Painting registrations were measured against the imported opaque contact pixels: the low bank uses a 19.429-unit height, opposing bank a +0.2476-unit art correction, and exit steps use their measured tread heights. The final main contacts remain fixed. Initial stencil drawings are authoring guides; runtime contours are the final registration.

The recovery shaft keeps its hidden local landing and return. Its small steps now have rounded native lips and substantial bearing stone recessed into the rear wall. There is no checkpoint, highlight or player-triggered reveal. The catch remains below the takeoff viewport.

## Permanent camera and light

Camera Z stays -10, culling stays unchanged and orthographic size stays 5.5. Only continuous height tracking follows the descent and ascent. No renderer, layer, interior plane or background switches on exit. Rear rock ends at the physical mouth; the normal exterior is visible through that opening. Roofs and foundations overlap beyond the visible boundary to hide source joins.

Existing unlit painted light is retained: cooler enclosed rock, warm side fissure at the fountain, twelve pausable dust motes. No new lighting package or runtime lighting framework is introduced. The fountain remains embedded in its floor, with its future item anchor empty.

## Reproduction and evidence

Build with `CaveEntryStudyReview.BuildPreview`, then launch with `--preview-view level-one-descent`. This starts one normal main-route jump before the entrance. A/D or arrows move, Space jumps, P pauses, R retries the main checkpoint, Backspace restarts the entire level and Escape opens the menu. The standalone accepted slice remains available as `--preview-view cave-entry-study`.

`IntegratedCaveReview.Record` records actual 0.02-second physics simulation, continuous camera tracking and unchanged renderer states. It tests upstream approach, complete forward connection, descending the exit again, the central miss and recovery return, every entrance ledge in reverse, water falls above the cave, pause, retry and restart. Normal-speed recordings are sampled at 10 fps; exact frame-count files exclude stale frames from older captures. Scripted timing is not human timing.

Final executed results and captures are in [permanent-shore evidence](permanent-shore-evidence/README.md). Earlier connected-route evidence is historical and does not validate this revision. Asset prompts, refinement decisions and original output names are in [asset provenance](permanent-shore-assets.json) and `ASSETS.md`.

## Ownership and remaining acceptance

The same branch and draft PR #79 own this sequential feature. No concurrent agent changes or shared mutable Unity project are involved. Water PR #61 remains separate; this feature preserves existing animation and uses the optional spatial controller hazard hook. No packages, abilities, items or persistent-data formats are added.

Owner visual acceptance, human timing and same-hardware frame-rate measurements remain pending. The direct scripted traversal is below the earlier roughly 60-second target; it must not be presented as satisfying a human-duration target. The PR stays draft during this review. Exact-head Windows/Linux player and launcher builds, package checks and merge-ready are later final PR gates. Merging and releasing are excluded.
