# Existing-game transition inbetweens — review only

Latest user direction: keep the in-game cat and original four-frame run; reject
the cute legacy alternative. Add images between existing jump/landing poses,
rather than replacing the character or changing its proportions/style.

endpoint-reference.png shows eight explicit start/end pairs loaded from actual
Unity sprite rectangles/pivots and transition PNGs. pairs.json records source
indices. Built-in OpenAI image generation created eight complete-character
midpoints on 2026-10-24, then corrected the seventh recovery drawing. Exact model
ID was not exposed. Both prompts are in generation-prompts.txt. Terms:
ASSET-LICENSE.md where rights exist. No external artwork or copied code was used.
Human direction requires the current game's natural proportions and substantial
paws, with additional intermediate articulation rather than a style redesign.

tools/prepare-mika-inbetweens.py leaves all endpoint images unchanged. Every new
drawing uses the same uniform 0.72 scale, alpha cleanup and manual shoulder/sole
registration; there is no optical flow, per-frame height normalization, crossfade
or body deformation. Landmark choices and mapping are in registration.json.
The seven previous variants remain historical studies, not accepted game assets.

before.gif and review.gif have the same total duration. Added images divide
existing pose holds rather than slowing the entire jump. review-slow.gif slows
the new sequence threefold; review-strip.png exposes every endpoint/midpoint.
This is a visual candidate, not evidence of accepted proportions or final quality.
No Unity assets, current playtest, run gait, input or physics were changed.
PR #45 stays draft; integration and exact-head validation follow visual review.
