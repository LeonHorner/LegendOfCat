# Implementation plan

See game-direction.md and art-direction.md before implementation. The following issues are deliberately small and dependency-driven. Product choices remain review gates; this is not a literal Java port.

1. **Define the six-level adventure and challenge curve** — Depends on: None. Propose a six-level journey, teaching beats, checkpoints and optional challenges. Retain one stomp boss and flying owl/dragon ranged bosses; level numbers and themes are proposals, not fixed Java mappings.

2. **Prototype responsive cat movement and camera** — Depends on: None. Build a small playable test course with run, jump, fall, landing and camera follow; Java CatAdventure is a reference for existing coyote time and input buffering, not a mandatory numeric spec.

3. **Create Mika's animation-ready character and movement set** — Depends on: Movement prototype. Use the naturally proportioned orange-and-cream cat from archived references. Create production-ready artwork and smooth idle, run, turn, jump, fall and landing transitions.

4. **Restyle the existing start menu, settings and update views** — Depends on: None; can progress alongside gameplay prototypes. Reskin StarterScreen and its existing menu/settings/update views from main ab70aa4 to match the approved painted 2D direction. Reuse existing launcher communication, UpdatePolicy and VersionedJsonFile behavior.

5. **Prototype stomp combat and boss attack readability** — Depends on: Movement prototype. Create a small greybox stomp encounter with a target vulnerable to head jumps. Explore readable anticipation, attack, recovery and satisfying impact.

6. **Prototype ranged combat against a flying target** — Depends on: Movement prototype. Try a compact ranged-combat concept against a moving flying target. The previous Yarn Launcher is optional; do not assume weapon pickup, magazine, reload or hold/tap rules.

7. **Implement checkpoint, retry and run lifecycle** — Depends on: Movement prototype; approved checkpoint policy. Implement checkpoint activation, falls, retry and full-run restart as distinct operations, with a small reusable playable scene.

8. **Define and implement rewards and optional abilities** — Depends on: Approved campaign and combat prototypes. Decide whether coins, alternative collectibles, power-ups or another small reward system support the new adventure. Ask the user to choose before implementation.

9. **Build a complete first-level art and gameplay slice** — Depends on: Approved campaign; movement, character, run lifecycle and chosen rewards. Produce one short complete level using painted environment layers, approved character animation and clear playable silhouettes. This is the quality reference for later level issues.

10. **Implement campaign progress and level results** — Depends on: Approved campaign and run lifecycle. Add completion, selected reward records and timing/results required by approved campaign. Integrate with existing versioned JSON storage rather than creating a competing framework.

11. **Design and implement the stomp boss encounter** — Depends on: Approved stomp prototype; first-level quality reference; campaign boss placement. Create one complete grounded boss defeated by head jumps, with refined original identity and arena. The forge guardian is inspiration; exact theme, health and phases need design approval.

12. **Design and implement the clockwork owl encounter** — Depends on: Approved ranged prototype; first-level quality reference; campaign boss placement. Develop the expressive clockwork owl idea and ranged encounter, using the cartoon owl influence and painted-world references.

13. **Design and implement the dragon encounter** — Depends on: Approved ranged prototype; owl encounter learnings; campaign boss placement. Develop a refined flying dragon ranged boss and arena, with a distinct identity from the owl. Crystal/prism theme is optional inspiration.

14. **Review the first slice and scope the remaining campaign levels** — Depends on: First-level slice and campaign plan approved. Turn playtest findings into individually scoped remaining-level issues. Avoid committing all six production layouts before movement/combat/art have been validated.

Production issues for the remaining non-boss levels follow approval of the first complete slice. Do not treat this tracking document as permission to implement everything in one PR.

## GitHub issues

- [Define the six-level adventure and challenge curve](https://github.com/LeonHorner/LegendOfCat/issues/8)
- [Prototype responsive cat movement and camera](https://github.com/LeonHorner/LegendOfCat/issues/9)
- [Create Mika's animation-ready character and movement set](https://github.com/LeonHorner/LegendOfCat/issues/10)
- [Restyle the existing start menu, settings and update views](https://github.com/LeonHorner/LegendOfCat/issues/11)
- [Prototype stomp combat and boss attack readability](https://github.com/LeonHorner/LegendOfCat/issues/12)
- [Prototype ranged combat against a flying target](https://github.com/LeonHorner/LegendOfCat/issues/13)
- [Implement checkpoint, retry and run lifecycle](https://github.com/LeonHorner/LegendOfCat/issues/14)
- [Define and implement rewards and optional abilities](https://github.com/LeonHorner/LegendOfCat/issues/15)
- [Build a complete first-level art and gameplay slice](https://github.com/LeonHorner/LegendOfCat/issues/16)
- [Implement campaign progress and level results](https://github.com/LeonHorner/LegendOfCat/issues/17)
- [Design and implement the stomp boss encounter](https://github.com/LeonHorner/LegendOfCat/issues/18)
- [Design and implement the clockwork owl encounter](https://github.com/LeonHorner/LegendOfCat/issues/19)
- [Design and implement the dragon encounter](https://github.com/LeonHorner/LegendOfCat/issues/20)
- [Review the first slice and scope the remaining campaign levels](https://github.com/LeonHorner/LegendOfCat/issues/21)
