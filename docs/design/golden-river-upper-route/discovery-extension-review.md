#
Current integration: the owner subsequently requested the actual level connection and forward return. See [Connected cave review](connected-cave-review.md); the isolated-scope statements below describe the earlier review stage.
 First discovery after the approved entrance

On 2026-11-13 the owner accepted the entrance and short passage after the painted-contact correction, then authorized extending that design. The accepted entrance paintings, Mika collider, motor, camera scale and corrected ledge contacts are retained.

This iteration extends the same playable study through the former end wall, under a continuous overhang, down two attached rock steps into a fountain alcove and up the opposite side. The new right-hand steps remain an explicit review endpoint. This is the next bounded visual review, not completion of the full 60-second route or its much-later campaign rejoin.

## Construction and lighting

- Three active native paintings provide the connected floor/stairs, deep ceiling mass and recessed fissure wall. The existing original fountain cutout is reused once, with its base embedded in the continuous floor.
- The old end-rock opening is permanently cut into its render geometry and collision. New rock overlaps conceal those construction joins. No scene/background appears in response to Mika's position or jumps.
- The side crack is painted into the quieter rear wall. Its fixed warm light falls diagonally down-left toward the fountain; the surrounding roof and terrain remain cooler. Twelve small slow dust motes use the study's pausable effect clock, without sparkle or reward signaling.
- Collision surfaces are registered to the final opaque paintings. The first floor candidate lacked air above its highest stair; the first roof candidate lacked sufficient upper coverage after placement. Both are archived through Unity, retaining metadata.
- An invisible `FutureCollectibleAnchor` reserves the free space beside the fountain. There is no collectible, trigger, new checkpoint, persistent data or ability.

## Scope and ownership

The same feature branch and draft PR #79 own this sequential scenery/geometry work. No concurrent component work or second implementation is involved. `CaveEntryDiscovery.cs` extends the existing study; the campaign's full route and foreground river remain unchanged.

The wider main-route shore/water integration, concealed central catch, full direct-run duration and distant campaign exit remain later feature work. Old full-cave screenshots and tests do not establish acceptance of that rejected visual prototype. Do not mark issue #72 complete or mark the PR ready on the basis of this study.

## Review checklist

Check the unchanged entrance, old/new tunnel joint, descending steps, fountain footing, side-light source, opposite stairs and upward camera coverage in both directions. Check actual rendered contact and walk-off behavior, not merely successful jump targets. Check 960x540, 640x360 and grayscale views. Verify that pause freezes the dust and that returning through the entrance remains possible.

Original asset prompts, sources, generator/model disclosure and creative decisions are in `discovery-extension-assets.json` and its roof refinement record; provenance is also appended to ASSETS.md. Human visual review of the extension is pending.

## Verification

The extended focused Unity test passed for this iteration. It covers the complete study in both directions, every new landing and stair exit, the previously corrected entrance contacts, pause including dust positions, deep-fall reset, permanent renderer activation and river recovery above the lower alcove. Contact checks compare actual source pixels with the rendered footing independently of the collider definition. Sampled exits include a small inward allowance for the existing box contact skin; the player collider and motor remain unchanged.

The local review tool captures ten positions in color at 960x540 and 640x360 and in grayscale, plus a fixed-physics forward recording through the extension. The recording is scripted at its actual physics speed; it is not a human first-play timing or proof of the complete 60-second optional route. Windows preview and packed-art verification are separate from the still-pending final Windows/Linux player, launcher, package and merge-ready checks.

The Windows development build and packed-art audit succeeded: 72 active paintings, historical sources excluded, and 83,911,168 imported bytes for all descent textures including earlier route art (under the 128 MiB target). The final extension recording is 9.20 scripted seconds. Evidence is in `discovery-evidence`; the focused test XML and build/runtime logs remain local under `artifacts`.
