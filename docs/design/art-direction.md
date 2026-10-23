# Art direction

## Chosen direction
Charming hand-painted 2D worlds, atmospheric depth and warm adventure. The protagonist uses the natural orange-and-cream quadruped proportions from the first original study: modest head and eyes, graceful limbs and striped tail. The clockwork owl takes the expressive outlined cartoon influence from the second study. Match palette and lighting so characters belong in the painted world. The user rejected the 2.5D study.

## Initial reference archive
These revised concepts were requested for documentation after feedback that the moonlit scene made platforms and background hard to distinguish. They document the initial direction, not fixed layouts or a user-approved final production asset set.

- [Golden River Ruins](references/golden-river-ruins-v2.png)
- [Moonlit Observatory](references/moonlit-observatory-v2.png)
- [Autumn Sky Mills](references/autumn-sky-mills-v2.png)

The same owl appears for comparison, not because three owl encounters are planned. Floating supports, gaps and scale are illustrative. Actual geometry must match implemented movement.

## Readability requirements
- One immediately identifiable playable plane. Standable surfaces have consistent crisp continuous upper edges, clear silhouettes and darker solid fronts.
- Backgrounds use lower contrast, softer edges, less detail and atmospheric separation. Do not rely on hue alone.
- Decorative roots, bridges and ledges must not advertise false routes or appear to bridge gameplay gaps.
- Plants and effects must not hide landing edges, the cat, attack warnings or hazards.
- Bright platform edges should look naturally lit, not like mandatory neon outlines. Night scenes must remain readable without flattening their atmosphere.
- Verify the real game in motion, at small window sizes and in grayscale. Concept review cannot establish collision accuracy or performance.

## Animation
Smooth idle/run/turn/jump/fall/landing transitions with grounded feet and consistent proportions. Do not substitute whole-body stretching for authored motion. Combat separates anticipation, active attack and recovery. Inputs remain immediate; cosmetic animation cannot silently change hit timing. Attack effects remain restrained enough for simultaneous threats to be read.

## Provenance
See ASSETS.md. Images were produced with the built-in OpenAI image generation tool on 2026-10-23, using earlier generated scene references. Exact model ID was not exposed. Human direction selected cat and owl influences, rejected 2.5D, and required clearer gameplay/background separation. Full archived generation prompts are in [reference-prompts.md](reference-prompts.md).

## Historical boss artwork
See the [legacy boss design archive](references/legacy-bosses/README.md) for original sprite sheets, animation metadata and generation records. These references are optional inspiration, not fixed Unity designs.
