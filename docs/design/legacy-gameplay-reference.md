# Legacy gameplay reference

Recorded from the local Java source on 2026-10-23. This document is self-contained: agents do not need the author's disk paths or a Java installation to plan the Unity implementation. It records observed legacy behavior, not mandatory Unity requirements. The agreed redesign in game-direction.md takes precedence.

## Campaign and progression

Six freely selectable stages: Sunny Meadows (grass, water, introductory jumps), Moonlit Ruins (night ruins and higher paths), Loft (wood platforms and shelves), Ember Forge (stomp guardian), Star Dock (ranged clockwork owl), Crystal Abyss (ranged prism dragon). Each has 30 optional coins. The redesign may replace themes, geometry, reward counts and systems, and redistribute difficulty while maintaining comparable overall challenge.

The original route has checkpoints around horizontal coordinates 10.2 and 20.2 in Java world units. Level 6 has an additional checkpoint at 25.15 before the arena. Start is approximately (0.8, 0.695), arena entry 25.6, goal basket 31.05. These coordinates describe the old layout only; they are not Unity coordinates or required lengths.

## Controls and movement

A/D or left/right move; W/up/space jump; R returns to checkpoint; J/F fires after acquiring the old weapon in levels 5 and 6. A full restart starts a fresh run separately from checkpoint retry. Keyboard events are buffered so brief inputs are not lost.

Legacy CatAdventure values in Java units: normal horizontal speed 2.6, sprint 3.35; jump velocity 3.6, boosted 4.3; downward acceleration 7 and terminal downward speed 5. Horizontal acceleration is 40 while running, 60 when reversing, 80 when stopping. Coyote time is 0.10 seconds; jump input buffer is 0.12 seconds. Physics uses a fixed 120 Hz step. Camera tracks with a horizontal look-ahead and bounds. These are comparison points, not directly transferable Unity tuning values.

## Powers, falls and retries

Sprint lasts 7 seconds, jump boost 9 seconds, shield absorbs one hit or one fall. A shielded fall returns the cat to its last safe standing position; otherwise falling below the level, pressing R, or losing all boss hearts respawns at the current checkpoint. Retry clears velocity, jump grace and transient powers, resets the boss and its projectiles; the acquired weapon survives checkpoint retry to prevent soft locks. Coins already collected remain removed during that run. Reloading the level restores its initial objects and coins. New power-up/resource/persistence rules require an explicit redesign decision.

## Stomp guardian

Four valid head hits defeat the boss; player has three hearts. Boss wakes at arena entry. It moves horizontally and cycles through warning, firing and vulnerable recovery. Wind-up lasts 0.8 seconds; one ground-height shot fires at that boundary. Core is vulnerable from cycle time 1.15 to 2.8 seconds. A valid stomp requires descending onto the head while vulnerable; it resets the attack cycle and bounces the player upward (legacy bounce velocity 3.7). Side/body contact and projectiles hurt instead. Damage protection lasts 1.25 seconds.

At two or fewer remaining boss hit points, cycle shortens from 4.2 to 3.4 seconds and shot speed rises from 2.2 to 3.0 Java units/second. Defeat clears projectiles. Main learning task: dodge fire, recognize open core, land a descending head hit, repeat with less recovery time.

## Ranged weapon

Old weapon has three rounds and automatically aims projectiles toward the boss's current position when fired; projectile speed is 5.5 Java units/second. Owl mode permits holding fire, 0.55 seconds between shots and 2.2 seconds to reload an empty magazine. Dragon mode requires fresh button presses, 0.7 seconds between shots and 2.8 seconds to reload. Both refill a partly used magazine after 2.2 seconds without firing. Weapon resets with the encounter. The Unity weapon, acquisition, aiming and resource model are explicitly open decisions.

## Clockwork owl

15 health, player has four hearts. Three phases: health above 10, above 5, then remaining health. Boss oscillates horizontally and vertically. Alternates an aimed feather fan, ground wind waves in both directions, and falling gears at marked columns. The first marked column tracks the player's position at cycle start; other columns are fixed. Later phases increase fan density and projectile speed. Cycle length is 4.0 / 3.6 / 3.2 seconds by phase. Wind-up is 0.8 seconds. Core vulnerability is cycle time 1.15 to 2.65 seconds. Successful boss hits are separated by 0.35 seconds; player protection is 1.2 seconds. Closed core consumes incoming shots without damage.

Main learning tasks: move through fan gaps, jump wind waves, leave marked columns, and shoot while the core is open. Shield can consume a hit. Defeat/retry clears active projectiles.

## Prism dragon

Uses the expert ranged encounter: 24 health, four player hearts, phases above 16 health, above 8, then remaining health. Adds a fourth alternating pattern: a horizontal beam at ground or high height. Ground beam requires jumping; high beam requires staying low. Beam is active for 0.27 seconds after wind-up. Crystal fans grow with each phase; later phases add a second ground wave at cycle time 1.28 seconds. Marked falling attacks remain part of the pattern sequence.

Wind-up is 0.95 minus 0.10 seconds per phase (0.85 / 0.75 / 0.65). Cycle length is 4.2 minus 0.35 seconds per phase (3.85 / 3.50 / 3.15). Vulnerability is cycle time 1.3 to 2.2 seconds. Extra pre-arena checkpoint and persistent weapon reduce retry cost. Main extra challenge is attack recognition, tighter openings and deliberate ammunition use. These numbers are historical, not a requirement for the redesigned dragon.

## Completion, records, animation and sound

After boss defeat (where applicable), entering the goal basket completes the run, records results, settles the cat onto a cushion, plays victory music and confetti, then returns to level selection after the music. Records include completion, maximum coins, fastest any-coin time and fastest all-coin time. Restart does not erase records. The elapsed timer continues through ordinary retries until victory. Legacy pause behavior is not established by this snapshot; decide it explicitly for Unity.

Java saves versioned JSON, reading four-, five- and six-level profiles and upgrading to six. Invalid data is preserved rather than overwritten. Unity must use its existing versioned persistence foundation and Application.persistentDataPath. Legacy save import is not yet a requirement; redesigned layouts make old times unsuitable for direct competitive comparison.

Cat visuals include quadruped running, jump/fall poses, looking, grooming, stretching, blinking, fish dropping/pickup and basket sleep. Idle actions begin after roughly 1.6 seconds. Bosses use 16 authored silhouette poses. Unity should improve motion and transitions without input delay or misleading collision timing; fish behavior is optional pending design review. Each stage has music, bosses have three intensity layers with phase transitions; pickup, jump, coin, hit and victory events have sounds.

## How to use this reference

Compare learning burden, jump precision, attack density, damage windows and retry cost, rather than copying constants. Prototype and obtain user decisions for unresolved movement, weapon and reward systems. The six new themes and exact boss placement are not fixed. Native GitHub blocked-by relationships define implementation order.

This is a source-derived behavioral snapshot, not an archived executable or a complete asset inventory. Historical subjective difficulty still needs playtesting; do not claim identical difficulty based on this document alone. If a new feature needs an undocumented legacy detail, ask the user and record the answer here rather than referencing a private path. No legacy assets are required to build the Unity project. Do not import old third-party assets without independent provenance and redistribution checks.

## Source traceability

Inspected Java files: KATZENPFAD.md; jade/LevelInfo.java and CampaignProgress.java; components/CatAdventure.java, CatPowers.java, YarnWeapon.java, BossBattle.java, YarnBossBattle.java and CatAnimationController.java; scenes/PlaySceneInitializer.java; tests/scenes/CampaignLevelTests.java. Java package paths are traceability labels, not links or build dependencies. Earlier art references are archived separately under docs/design/references.

## Historical boss artwork
See the [legacy boss design archive](references/legacy-bosses/README.md) for original sprite sheets, animation metadata and generation records. These references are optional inspiration, not fixed Unity designs.
