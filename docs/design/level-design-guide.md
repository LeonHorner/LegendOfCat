# Level design: start here

Read this before designing or changing a level, terrain, platform collision, moving support, camera corridor or scenic landmark. These are lessons from Golden River player feedback, not permission to add campaign mechanics. Keep [game direction](game-direction.md) and [art direction](art-direction.md) authoritative; confirm unresolved product choices.

## Where to look

| Task | Reference |
| --- | --- |
| Current route, latest corrections, timing and remaining acceptance | [Finale](golden-river-main-route/finale.md), [ascent](golden-river-main-route/ascent.md), [integration/exit contract](golden-river-main-route.md) |
| Checkpoint contact, appearance and states | [Checkpoint decisions](golden-river-main-route/checkpoints-and-pacing.md) |
| Painted terrain, architecture, landscape registration and source files | [Finale art](golden-river-main-route/finale-art.md), [asset provenance](../../ASSETS.md) |
| Actual jump envelope and rider behavior | [CatMotion](../../Assets/Game/Scripts/CatMotion.cs), [CatController](../../Assets/Game/Scripts/CatController.cs), [MovingSupport](../../Assets/Game/Scripts/MovingSupport.cs) |
| Layout, camera and rendered contact registration | [MovementCourse](../../Assets/Game/Scripts/MovementCourse.cs), [FinaleRoute](../../Assets/Game/Scripts/FinaleRoute.cs), [CatCamera](../../Assets/Game/Scripts/CatCamera.cs), [SceneryRenderer](../../Assets/Game/Scripts/SceneryRenderer.cs), [shared art profiles](../../Assets/Game/Scripts/TerrainArtProfile.cs) |
| Real collision traversal, recovery and visual evidence | [Integration tests](../../Assets/Game/Tests/Editor/MenuMovementIntegrationTests.cs), [collision tests](../../Assets/Game/Tests/Editor/CatCollisionTests.cs), [review capture](../../Assets/Game/Editor/GoldenRiverReview.cs) |

Dated corrections can supersede earlier proposals in the linked histories. Distinguish approved decisions, current implementation, historical evidence and remaining player acceptance; a passing test does not approve artwork or establish first-player timing.

## Reuse these lessons

1. **Length through decisions.** Vary height, width, approach, descent and readable reversals. Obstacles explain detours. Extend an interesting passage before the safe finale; avoid empty distance, slower controls and forced waits.
2. **Use real physics.** Preserve movement. Derive gaps from current acceleration, jump height and headroom. Golden River targets 20% reach reserve and roughly 3-unit precision landings (2.5 for brief harder steps). Verify actual collision traversal.
3. **Match collision to stone.** Golden River platforms are solid from below. Exclude empty corners and hanging ivy. Rubble needs flat treads/vertical risers: slopes caused deflection and idle sliding. Check walls, undersides, corner holds, landing and fresh jumps.
4. **Make movers useful and stable.** Destination height should reward riding without long waits. Test idle carry, running both ways, takeoff at both reversals, pause and retry phases. Rider-relative movement drives animation/camera facing; takeoff gains no carrier boost.
5. **Recover locally.** Visible catch terraces need short tested returns and headroom at relevant mover phases. Preserve working return edges when resizing floors. Both painted bearing feet must rest within solid ground, embedded behind matching grass.
6. **Keep the camera calm.** Constant zoom/Mika size, vertical dead zone and smooth height/facing changes. Reveal the next landing before takeoff, including reversals and descents. Follow genuine falls. Water stays at world height.
7. **Author shapes.** Keep gold vegetation, violet stone and teal ivy. Connected platforms share material; change family on broad ground. Avoid scaled copies, clipped ends and stretched cliffs. Register contact independently of grass tufts and preserve native proportions.
8. **Support scenery.** Foundations extend below the falling view. Openings have clean alpha; no copied sky, floating specks or blurred ground fades. Backgrounds stay quieter than gameplay. Check parallax/coverage through the exit and on return; preserve earlier panorama registration.
9. **Place landmarks deliberately.** Vary depth; do not tile recognizable ruins, rocks or Easter Eggs. Show start/end boundaries with scenery beyond them. Checkpoints activate on contact, sit naturally in distinct settings, pulse when current and stay dark orange without emission when passed; no toast.
10. **Review in motion.** Check 960×540, 640×360 and grayscale, including reverse travel, falls and underpasses. Preserve Movement Lab, intro and lifecycle. Record human time, failures, recovery cost, waiting and unclear landings against the revision. Scripted times/video prove reachability, not player acceptance. Document asset sources/prompts; leave unfinished acceptance open.

For maintenance, author finale geometry/material/recovery together through `FinaleRoute.Definitions` and `Recoveries`; do not edit the derived geometry array. Bind new visuals/colliders directly at creation. Update the approved-layout fixture only for an intentional approved redesign, not to make a refactor test pass.
