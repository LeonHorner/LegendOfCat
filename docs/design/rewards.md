# Optional keepsakes

## Approved choice

For issue #15, the user selected optional keepsakes for exploration, without a
gameplay bonus, on 2026-10-24. Keepsakes give optional detours a visible reward
without changing Mika's movement, combat strength or access to the main route.
This choice does not approve coins, a shop, temporary powers or extra abilities.

Keep the approved [six-level adventure](six-level-adventure.md) and its
ability-independent detours. Missing a keepsake must never prevent an exit,
boss victory or campaign progression. No keepsake is consumed by gameplay.

The user additionally selected permanent retention after level completion and a
collection menu showing collected keepsakes and previews of missing keepsakes.
New pickups are pending during the run; successfully completing the level adds
them to the saved collection. Previously saved keepsakes remain collected across
retries, full-run restarts and application restarts. Do not save new pickups
permanently at pickup time or erase saved collection entries on a later run.

The collection menu must distinguish saved collected entries from missing
entries, with a preview for each missing keepsake. Exact artwork and preview
presentation remain visual design work. Pending pickups must not be presented
as permanently unlocked before level completion.

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
- A full-run restart clears pending pickups and restores their objects; it does
  not clear the saved collection.
- Pause prevents collection; pickup feedback cannot delay movement or respawn.

These retry rules are a prototype proposal, not an additional recorded user
decision. Verify them against the completed combat prototypes before integration.
Permanent collection persistence must integrate with the existing versioned save
storage and the campaign-progress feature, following docs/data-migrations.md.
Use stable keepsake identifiers and preserve existing and unknown future data.
This design document introduces no serialized format. The playable feature must
include the level-completion save boundary and collection menu rather than claim
completion with session-only pickups.

## Completion evidence still required

- Implement reachable pickup, duplicate prevention and lifecycle integration.
- Add behavioral coverage for collection, pause, retry and full restart.
- Implement and test level-completion persistence, application restart retention,
  and preservation of saved entries when an unfinished run is restarted.
- Implement the collection menu with collected entries and missing-item previews.
- Review the real demonstration in motion, at a small size and in grayscale;
  preserve a screenshot or short recording and asset provenance.
- Complete exact-revision tests and Windows/Linux player and launcher builds
  through the repository's final merge validation workflow.

The reward category, level-completion persistence and collection menu have been
selected. Issue #15 remains incomplete until
its playable demonstration and validation are complete. Consult native GitHub
blocked-by relationships for implementation order.
