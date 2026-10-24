# Bounded apex experiment

The user authorized exactly one added drawing before the apex first, then a
second after it only if the first improves the existing transition. Original
endpoints, four-frame run, jump duration and physics must remain unchanged.

Built-in OpenAI image generation produced before-apex-source.png on 2026-10-24;
exact model ID was not exposed. generation-prompt.txt is the exact prompt.
reference.png contains actual game sprites 17, 16 and 18. Terms:
ASSET-LICENSE.md where rights exist. The local comparison uses one uniform 0.26
scale and translation; no deformation or height normalization was applied.

Result: not integrated. Direct comparison still shows a smaller head and longer
trunk in the candidate. The second drawing has therefore not been generated.
single-transition.gif and its slowed version expose the mismatch, not an accepted
improvement. Preserve the current game rather than accumulating another bad pose.

Separately, tools/clean-mika-transition-specks.py removes disconnected alpha islands
from the four current transition textures using eight-neighbor connectivity.
It preserves every RGB value and every alpha value of the connected character.
Removed island pixels: pose 17: 18, pose 18: 29, pose 19: 24, pose 20: 35.
Dimensions, pivots, existing poses, timing, physics and metadata GUIDs are unchanged.
This addresses the reported edge speck without integrating the rejected candidate.
