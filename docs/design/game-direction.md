# Game direction

## Agreed intent
A charming, adventurous cat platformer in Unity, presented entirely in 2D. The Java game is a reference for the existing experience, not a specification to copy. Six levels may have entirely new themes, routes, arenas, mechanics and rewards. A new difficulty curve is welcome; overall challenge should remain comparable to the first six Java levels. Per-level difficulty, numerical timings, thirty coins, the old power-ups and exact boss health are not fixed requirements.

Retain three boss ideas: one grounded boss defeated by jumping on its head, a flying owl fought at range, and a flying dragon fought at range. Refine their identities and attacks as needed. The ranged weapon may be entirely different and does not have to be a pickup. Movement, attacks and animation should feel fluid, responsive and readable.

## Decisions and review gates
The cat remains the protagonist. Art direction and initial concepts are in [art-direction.md](art-direction.md). Do not prescribe new dash/double-jump abilities, weapon resources, collectibles, campaign themes or save import without checking with the user. Propose bounded options and use small playable prototypes before committing production content. Ask when requirements are unclear before creating a replacement issue plan.

Difficulty is experiential: compare learning load, jump precision, attack density, recovery windows and checkpoint retry cost. Java constants may inform prototype tuning but are not immutable. Improving animation must not delay input or hide actual collision timing. Telegraph, active attack and recovery should be distinguishable.

## Current implementation boundary
Inspected origin/main ab70aa45faf945828df38d94afd5f8b1f4b79f3e (PR #7). StarterScreen already supplies start menu, settings/update views and update-service integration. UpdatePolicy and VersionedJsonFile supply policy and versioned persistence. Restyle those views through a dedicated issue; preserve their existing behavior and launcher ownership. Do not recreate the update system. New gameplay must integrate with that foundation.

## Delivery strategy
Small coherent feature branches and PRs, with dependencies and user-facing acceptance criteria. Several related issues may share a branch. First prove movement and both combat modes, then one complete level, then expand the campaign. Scene concepts do not fix boss placement or campaign themes. Automated checks follow AGENTS.md: manual final merge preparation and explicitly requested releases, not every commit. Nothing in this plan authorizes merging or releasing.

## Legacy reference
Use the self-contained [Java gameplay reference](java-gameplay-reference.md) for historical controls, movement, checkpoints, bosses, weapon behavior, records and audiovisual context. No local Java source folder is required. This historical reference does not override the agreed redesign. If a needed detail is not documented, ask the user and record the decision instead of depending on a private disk path.
