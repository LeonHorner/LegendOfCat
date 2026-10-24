# Optional keepsakes

## Approved choice

For issue #15, the user selected optional keepsakes for exploration, without a
gameplay bonus, on 2026-10-24. Keepsakes give optional detours a visible reward
without changing Mika's movement, combat strength or access to the main route.
This choice does not approve coins, a shop, temporary powers or extra abilities.

Keep the approved [six-level adventure](six-level-adventure.md) and its
ability-independent detours. Missing a keepsake must never prevent an exit,
boss victory or campaign progression. No keepsake is consumed by gameplay.

## Proposed small demonstration

After the native issue dependencies are resolved, demonstrate a single optional
detour with one keepsake using the established movement and run lifecycle.
Do not populate all six levels. The exact object, artwork and placement remain
prototype choices to review in play; they are not approved production assets.

Use a distinct silhouette and restrained pickup feedback that remain readable
at small window sizes and in grayscale. Keep landing edges and hazard warnings
visible. Reaching the object must require only already taught movement, and the
detour must rejoin the main route safely.

Proposed session rules for the demonstration:

- A keepsake can be collected only once during a run.
- Checkpoint retries retain collected keepsakes and their collected appearance.
- A full-run restart restores the keepsake and clears the run's collection.
- Pause prevents collection; pickup feedback cannot delay movement or respawn.

These retry rules are a prototype proposal, not an additional recorded user
decision. Verify them against the completed combat prototypes before integration.
Campaign records and persistence belong to the campaign-progress feature; this
demonstration does not introduce a save format or promise cross-session retention.

## Completion evidence still required

- Implement reachable pickup, duplicate prevention and lifecycle integration.
- Add behavioral coverage for collection, pause, retry and full restart.
- Review the real demonstration in motion, at a small size and in grayscale;
  preserve a screenshot or short recording and asset provenance.
- Complete exact-revision tests and Windows/Linux player and launcher builds
  through the repository's final merge validation workflow.

Only the reward category has been selected. Issue #15 remains incomplete until
its playable demonstration and validation are complete. Consult native GitHub
blocked-by relationships for implementation order.
