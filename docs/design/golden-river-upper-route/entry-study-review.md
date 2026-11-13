# Accepted entrance and first passage

Current integration: the owner subsequently requested the actual level connection and forward return. See [Connected cave review](connected-cave-review.md); the isolated-scope statements below describe the earlier review stage.

The previous full cave prototype is visually rejected. Passing its movement tests did not establish visual acceptance. The owner accepted the smaller entrance study after its contact correction. The rejected complete route remains frozen; see `discovery-extension-review.md` for the subsequent bounded extension.

## Initial approved scope

The initial review contained only the main-path cleft, attached descending rock ledges and a short enclosed passage, without a fountain, long return route, collectible or campaign checkpoint. It used the existing Mika presentation, motor, input and camera size. The current discovery extension is documented separately and still does not replace the campaign's route geometry.

Launch a development preview with `--preview-view cave-entry-study`. Move with A/D or arrows, jump with Space, reset with R or Backspace, pause with P and return to the menu with Escape. The initial endpoint was the rock wall on the right; that wall now opens into the discovery extension. The owner accepted this initial slice on 2026-11-13; the new continuation and discovery remain pending visual review.

## Construction

- Three native paintings describe connected left-bank ledges, opposing bank/ceiling and cave foundations. A fourth painting provides the quieter recessed rock wall.
- Collision contact contours are authored before painting. The source stencils and `entry-study-geometry.json` preserve that registration. The production paintings are separate cutouts, not a flattened concept picture used as a level.
- The player remains at the existing scale. Camera movement reveals the descent continuously. No renderer is enabled or disabled on entry, jumping or falling.
- Recess color and background occlusion depend on fixed world coordinates, not a player-triggered background swap.
- The river is drawn from permanent two-dimensional shore polygons. The same polygons determine downward water crossings. The passage has a separate deep-fall return boundary.
- This isolated scene frames the dry entry bank. Its substantial foreground rock hides the river from the cave interior; the campaign's visible foreground river is unchanged. The join to the complete campaign, including water outside the small shore opening, remains a later integration step after this visual review.

## Visual review gate

Check the actual entry, first two ledges, last descent ledge and enclosed passage at 960x540 and 640x360. Check jumps and falls as well as standing poses. No cropped source edge, flat wall boundary, detached ledge, paper-thin water edge or mismatched contact height may be presented as finished. Art is not accepted until the player review.

This study deliberately suspends the full-route duration and exit-position acceptance checks; it must not be reported as completion of issue #72 or PR #79.

## Local validation, 2026-11-13

- Existing Unity regression suite: 229 passed, zero failures. New isolated entrance physics test: one passed, zero failures. The latter uses real colliders to cover the main jump, each descending landing, gap crossing, reverse climb, pause, deep-fall reset, water crossing and permanent renderer activation.
- Actual runtime renders checked at 960x540 and 640x360. The built Windows development player was also launched and captured, with no runtime errors found in its log. This is scripted verification, not a human playthrough or visual acceptance.
- Windows development build and packed-art audit succeeded. Unity reports 71,325,568 bytes for all active descent textures, including the previously authored route; archived variants are excluded from the player.
- The last visual-only adjustment clips detached source flecks above the right bank's authored top edge. The source UV registration and collision contours stay fixed.
- No Linux player/launcher, package or merge-ready validation is claimed for this draft iteration. No merge or publication is authorized.

Generated asset provenance and complete prompts are in `entry-study-assets.json`, `entry-study-refinement-assets.json` and `entry-study-left-refinement.json`; original reference ownership is unchanged. Active terrain uses left-v2, right-v2, floor-v1, rear-v1 and valley-v1. Rejected first bank variants were moved out of Resources with Unity's AssetDatabase, preserving metadata.

## Contact correction after player review

The first review exposed a small vertical gap on the upper bank and an invisible extension of approximately 0.7 units at the lower floor's painted right edge. The initial geometry stencils were not a sufficient substitute for registering the final painted contours.

The upper left contact is lowered by 0.045 units. Cliff exits and both sides of the lower crack follow the final opaque stone contours, with a small inward exit allowance for contact skin. The player's box collider, motor, gravity, jump and coyote time are unchanged. New checks read the actual painted floor boundary independently from the collision definition, measure visible upper paw contact and assert downward motion within two physics ticks after the visible rear paw passes each reported edge. The extended focused Unity test passed. The owner subsequently accepted the corrected entrance and authorized extending that design.
