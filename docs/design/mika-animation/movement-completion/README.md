# Complete-character movement transitions

Four new complete-cat drawings bridge late ascent, early descent, pre-contact
reach and landing recovery. Built-in OpenAI image generation produced
`jump-transitions-source.png` on 2026-10-24; exact model ID was not exposed.
Terms: ASSET-LICENSE.md where rights exist. `generation-prompt.txt` is the exact
prompt. The identity reference combines original candidate cells 10, 6, 11 and
12 on a dark review background; no outside artwork was used.

Human direction: retain the accepted original four-frame run and substantial
paws, avoid the rejected rig/optical-flow approaches, add complete drawings at
flight transitions and make landing recover coherently. The new poses were
shown as normal/slowed loops before integration. They do not add abilities,
extra idle/fish behavior or change input/collision timing.

`tools/prepare-mika-transition-review.py` removes disconnected alpha specks and
uses uniform scale 0.48 plus translation to register the new drawings. The
explicit lower-left crop includes the nose beyond the nominal grid. Shoulder
landmarks are recorded in registration.json. `--export-assets` writes four
registered PNGs under Assets/Game/Art/Mika/Transitions; the Unity Editor then
creates their import metadata and references in the character prefab. No image
morphing, crossfading, nonuniform scale or runtime stretching is used.

The original sheet's alpha is cleaned separately by tools/clean-mika-character.py.
It preserves every RGB pixel, retained alpha value, dimension and pose; the exact
uncleaned source and metadata are in the hash-verified archive. Idle is a quiet,
stable original drawing. Turning uses the two original directional poses.

## Playback

The distance-driven run stays at original poses 4–7 over a 2.4 m stride. Ascent
uses 10 then 17, apex uses 16, early descent uses 18, falling uses 11, and a
nearby supporting surface selects 19 before contact. Landing uses contact 12,
recovery 20 and settle 13 before idle. Moving landings return to running promptly;
a buffered jump interrupts recovery immediately. Ledge falls never invent a
jump apex. Airborne drawings share a shoulder anchor; planted poses use visible
sole anchors. The physics root remains authoritative throughout.

`jump-review.gif`, its slowed version and the strip show the authored sequence.
The current integrated recording in the parent folder captures real controller
and collision playback. Final test/build results are reported on PR #45, since
each new commit requires fresh exact-head validation.
