# Six-level adventure and challenge curve

## Status and scope

Proposal for issue #8, awaiting the user's decisions below. This document plans the campaign; it does not authorize dependent production content. Read [game direction](game-direction.md), [art direction](art-direction.md), the [implementation plan](implementation-plan.md) and [legacy reference](legacy-gameplay-reference.md) alongside it.

The journey takes Mika from a sheltered riverside through inhabited ruins and wind-powered heights into a dragon's mountain home. Working titles and environmental connections are proposals, not new lore requirements. All play remains fully 2D. No mandatory dash, double jump, power-up, currency quota or specific weapon is assumed.

Lengths below are target minutes for a first successful main-route traversal, including learning pauses but excluding failed attempts and optional detours. They are planning estimates, not measured playtimes. Boss ranges include learning the encounter on a successful attempt. Movement and combat prototypes must establish achievable geometry and timing before production layouts are committed.

## Proposed journey

| Level / working theme | Purpose and teaching beats | Main challenge and optional challenge | Target length | Checkpoints and boss placement |
| --- | --- | --- | --- | --- |
| 1. Golden River Path | Establish run, stop, turn, jump and safe landings. Demonstrate each on broad ground, then repeat across shallow gaps before a short independent sequence. | Main: judge clear landing edges with generous recovery ground. Optional: a visible upper loop combines familiar jumps and rejoins before the exit. | 4–6 min | Start plus two safe recovery points, after the introductory jumps and before the final sequence. No boss. |
| 2. Rootbound Ruins | Reuse level 1 movement with changes in elevation. Show a simple moving hazard alone before combining it with a jump. Introduce stomp vulnerability and recovery in a safe rehearsal before the arena. | Main: alternate patient traversal with deliberate descending head hits. Optional: a tighter side route uses only already taught movement. | 6–8 min | Recovery point before the combined traversal; another immediately before the boss. Grounded stomp boss at the end, followed by a safe exit. |
| 3. Autumn Sky Mills | Offer a traversal-focused breather after the first boss. Teach moving supports over safe ground, then crossing between them. | Main: read support motion and choose when to launch; avoid blind landings. Optional: a longer elevated loop tests timing without an exclusive ability. | 5–7 min | Two recovery points separating introduction, practice and final combination. No boss. |
| 4. Moonlit Observatory | Introduce the approved ranged interaction on an unthreatening target; add one readable threat before the owl. Keep playable edges clear despite night lighting. | Main: recognize attack warnings, move to safety, then use an opening to attack. Optional: a traversal overlook before the arena, never an arena prerequisite. | 7–9 min | Recovery point after ranged rehearsal; dedicated checkpoint immediately outside the arena. Flying clockwork owl at the end. |
| 5. High Ridge Passage | Consolidate traversal and ranged decisions in short alternating sections. Introduce the dragon's low/high warning distinction in a safe rehearsal, without prescribing the final attack implementation. | Main: switch attention between landing, hazard recognition and an attack opportunity. Optional: a precision loop with safe re-entry after a miss. | 6–8 min | Two recovery points before the longest combined sections; no boss. The final stretch releases pressure before level 6. |
| 6. Dragon's Summit | Briefly recall known skills, then resolve the journey with the flying dragon. Introduce any encounter-specific rule in isolation before combining familiar threats. | Main: distinguish low/high threats and attack during readable openings, with less recovery margin than the owl. Optional: a pre-arena route remix; no extra boss phase required for completion. | 8–10 min | Recovery point after the approach and dedicated checkpoint immediately outside the arena. Flying ranged dragon is the final boss; safe resolution after defeat. |

Total target: 36–48 minutes on successful main-route traversals. This is a scope budget, not a promise about full first-play session length. Optional loops add challenge rather than required progression; their reward, if any, remains undecided.

## Checkpoint and retry policy proposal

Place checkpoints on broad safe ground after a demonstrated skill has been learned, before the next substantial combination. Target roughly 1–2 minutes of already learned traversal between checkpoints, shortened when repeated failure would become tedious. Use skill boundaries rather than evenly spaced world coordinates. A checkpoint must not resume inside an active attack or moving-support trap.

Boss retry returns to the arena entrance and restarts the encounter cleanly, without replaying the approach or requiring a repeated weapon-acquisition trip. Any approved ranged interaction must remain available on retry. Exact health, resources, transient-state resets, timer behavior and persistence belong to their respective implementation decisions. Full-run restart remains distinct from checkpoint retry; this proposal does not change existing save formats or records.

Optional detours branch from safe visible ground and rejoin the main route clearly. Missing one must never block the exit or boss. A fall should return to a safe route or the latest checkpoint; do not add checkpoints that overwrite a player's safer main-route recovery position halfway through an optional challenge. No hidden irreversible drop or reward requirement should force a full restart.

## Challenge comparison and validation

The Java journey moves from three traversal stages into stomp, owl and dragon encounters. This proposal distributes bosses across levels 2, 4 and 6, using levels 3 and 5 for consolidation. It increases early combat learning but avoids saving all combat instruction for the last half. Overall challenge should remain comparable to the first six Java levels, subject to playtesting, rather than copying layouts, health, projectile counts or timings.

| Dimension | Java reference | Proposed target and review evidence |
| --- | --- | --- |
| Learning load | Traversal first; stomp in level 4, ranged in levels 5–6. | One new demand at a time, then safe practice and a combination. Review whether level 2 stomp arrives too early in the first slice and combat prototypes. |
| Jump precision | Acceleration, coyote time and buffered jump support repeated platform traversal. | Main route requires consistent basic movement, never an unapproved ability. Compare failed landings and near misses after movement tuning; optional loops carry tighter precision. |
| Attack density | Owl combines fans, waves and marked falls; dragon adds low/high beams and denser later phases. | Owl teaches distinct warnings before combinations. Dragon tests recognition and shorter recovery, while preserving visible safe responses. Inspect concurrent threats in motion. |
| Recovery windows | Legacy bosses expose timed vulnerability and become faster in later phases. | Preserve recognizable anticipation, active attack and recovery. Tune windows to permit an intentional response, without requiring identical Java timings or hit counts. |
| Retry cost | Route checkpoints and an extra final-arena checkpoint; weapon survives retry. | Dedicated checkpoint at every boss; no repeated equipment trip. Record time from failure to another meaningful attempt and reduce repetitive approach time. |

Use the movement course, stomp prototype, ranged prototype and complete first slice as evidence before fixing later production layouts. Record completion time excluding/including retries separately, failures by section, misunderstood warnings, missed openings and checkpoint replay time. Compare learning burden and player-reported frustration with the documented Java behaviors; the source snapshot alone cannot establish equal subjective difficulty. If no playable Java baseline is available, label that comparison qualitative and do not claim measured parity.

Review the real game in motion at a small window size and in grayscale. Standable edges must remain crisp and continuous, backgrounds softer and lower contrast, and hazards readable independently of hue. Detours must not be implied by decorative roots or bridges. Validate that animation never delays input or disguises active collision timing. This document contains no playable or visual implementation, so those checks remain downstream gates, not completed validation.

## User decision record

Approval is required before dependent content implementation. Pending decisions:

1. Accept the proposed river → ruins → mills → observatory → ridge → summit journey and boss order (stomp in 2, owl in 4, dragon in 6), or revise it.
2. Accept the 36–48 minute successful-route budget, frequent recovery points and dedicated boss checkpoints, or choose a different length/retry emphasis.
3. Accept optional, ability-independent detours with no required rewards, leaving their reward system to its own decision.

Weapon identity, acquisition, aiming, resources and controls; currency/collectible counts; power-ups and additional movement abilities; boss identities beyond the retained concepts, health and exact phases; campaign unlock rules; timer/pause rules; records and legacy save import remain open. Approval of this plan must not be treated as approval of those systems.

After approval, record the user's date and exact accepted scope here. Revisit pacing and checkpoint placement after the first slice; material theme, boss-order or scope changes require a further user decision. The [implementation plan](implementation-plan.md) continues to define the bounded delivery steps, with dependencies tracked through native GitHub relationships.
