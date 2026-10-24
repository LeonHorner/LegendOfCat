# Cute legacy-inspired motion example

The user requested a small, cuter example toward the Java legacy game's style,
with a curved jumping posture and a head/gaze that follows the motion instead
of a rigid horizontal torso and constantly forward-facing head. This is a style
sample, not approval to replace the current playable character or art direction.

Reference: the generated cat atlas in the user-designated legacy checkout,
assets/images/cat/cat-actions-v1.png and its JSON/PROMPT.md. The legacy prompt
records built-in image generation; original generation date and exact model ID
are unavailable. Reference cells 0 and 24–27 were cropped from the recorded bounds,
mirrored to face right and composited on a dark review background. No legacy tile
sheet, third-party character, fish behavior, accessory or game code was imported.

Built-in OpenAI image generation created the new eight-pose transition-source.png
on 2026-10-24; exact model ID was not exposed. generation-prompt.txt is the exact
prompt. Terms: ASSET-LICENSE.md where rights exist. Human direction selected rounder
proportions, expressive blue eyes, sturdy short paws and a cute retro-platformer
influence. The source depicts articulated spine/hip/neck postures, upward gaze
on launch, downward gaze at the apex/descent and absorption/recovery on contact.

tools/prepare-mika-cute-example.py cleans disconnected alpha, applies the same
0.5 nearest-neighbor scale to every complete drawing and registers the drawn body
anchors. There is no per-frame height normalization, image morphing, rig or runtime
body deformation. review.gif is the small motion example; review-slow.gif slows
it threefold. review-strip.png and registration.json expose the poses and choices.
The anticipation timing is illustrative and does not alter game input or physics.

The legacy checkout and Unity assets are unchanged. No new production tests/builds
are claimed for this documentation-only example. PR #45 remains draft and issue
#10 remains subject to the user's visual feedback.
