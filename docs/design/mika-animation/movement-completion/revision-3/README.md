# Proportion review 3 — not integrated

The user rejected revision 2 because Mika shrank during flight and returned to
normal size. This candidate uses one coherent eight-pose sheet for standing,
flight and landing rather than mixing differently proportioned source families.

Built-in OpenAI image generation created the source on 2026-10-24, followed by a
targeted head-consistency edit; exact model ID was not exposed. The sole identity
reference is the original standing Mika. `generation-prompts.txt` records both
exact prompts. Terms: ASSET-LICENSE.md where rights exist. No external art was
used. Human direction requires consistent head size, torso length/volume and
substantial paws; tucked legs may shorten the silhouette without shrinking the cat.

`tools/prepare-mika-revision-3.py` cleans disconnected alpha and applies the same
uniform 0.72 scale to all eight source cells. Shoulder registration is translation
only. No per-pose scaling, height normalization, body stretching, optical flow,
rig or crossfade is used. The head edit was generated in the whole sheet, not
assembled from detached head parts. registration.json records all crops, manual
landmarks, scale and regional occupancy diagnostics. The latter are approximate
checks for gross size variation, not proof of identical anatomy: torso-region
occupancy ranges about 4.4%, head-region occupancy about 14.4%, including changes
in neck/ear silhouette and crop occupancy. Do not represent these as exact volume
measurements or claim perfect anatomical consistency from them.

Review `review.gif` and `review-slow.gif` in motion. `size-check.gif` removes the
jump trajectory and fixes the shoulder position, making residual size changes
easier to distinguish from articulation. review-strip.png shows all phases.
The final bookend repeats pose 0; all intermediate images use the common scale.

This is a review candidate, not a claim that the user's visual feedback is
resolved. Unity assets, the four-frame run and the current playable prototype
are unchanged. PR #45 remains draft and issue #10 remains incomplete. New
integration and exact-head validation follow review, not merely successful tests.
