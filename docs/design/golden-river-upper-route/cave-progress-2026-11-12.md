# Forward cave implementation progress — 2026-11-12

Historical intermediate review notes. The subsequent [playable test candidate](cave-test-run-evidence/README.md) records the completed local test/build pass, current evidence and reproduction instructions. The checklist below predates the final test candidate and is retained as development history, not its current task list.

The route now rejoins ascent bank 49, approximately 322 world units beyond entry bank 10. Checkpoint 2 and the finale remain downstream. Forward traversal, reversible entry, local catch return and whole-level completion pass the six targeted route/water/camera tests in `artifacts/cave-natural-edges.xml`.

The geometry is a connected solid cave prototype: ceiling, riverbed, attached footings and carved shore/recovery openings share rendering and polygon collision outlines. World-space shading uses an owned permanent distance texture generated from those outlines. Four combined render meshes avoid one renderer per rock segment. The unique fountain is registered to its actual base and the new contact trims replace the old detached scalloped ledges.

The extended forward route completed in 60.48 simulated seconds in the real-physics pilot. Controls are unchanged; the additional duration comes from the longer forward passage. This is scripted timing, not a human first-play measurement.

## Remaining work

- The entry is still substantially unlike the approved narrow-cleft reference. Replace exposed rectangular bank/river joins with fitted natural shore rock bodies.
- Further refine footing silhouettes around the local rescue passage. Passing collision tests do not approve the visible pillar treatment.
- Finish the fountain chamber's physical side-light aperture and surrounding enclosure.
- Check all roof, floor and wall extents in motion, including reverse traversal and falls. Current stills under `artifacts/cave-contact-review` are diagnostic, not accepted evidence.
- Complete the current full suite, new visual captures, player frame-time/memory comparisons, a fresh preview and player review. The previous preview contains the rejected layout.
- Update draft PR #79 with the current implementation and remaining acceptance, preserve asset metadata/provenance, and obtain fresh exact-head platform/package/merge-ready checks only after implementation and review are complete.

New original image sources and exact prompts are recorded in `cave-stone-v2.json` and `cave-edge-v2.json`; the project-bound images live in `Assets/Game/Resources/GoldenRiver/Descent`. No merge or release has occurred.
