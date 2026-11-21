# Forward cave test-run build — 2026-11-12

This is the current playable test candidate. Earlier upper garden and open-hall evidence is superseded. Player visual/difficulty review is the purpose of this build; automated checks do not establish human acceptance.

## Play

Build with `BuildGame.BuildPreview`, then launch the resulting Windows player with `--preview-view level-one-descent`. The local test candidate is `artifacts/cave-test-run/LegendOfCatPreview.exe`. It starts on the original entry bank with main checkpoint 1 active. The lower connected landing leads right, then down through the riverbank opening. The cave advances to ascent bank 49, approximately 322 world units beyond its entry, before checkpoint 2 and the finale.

Move with A/D or arrow keys, jump with Space, retry the main checkpoint with R, pause with P and restart the whole level with Backspace. No optional checkpoint or collectible is present. The normal main route remains playable.

## Verified locally

- Unity 6000.3.27f2, D3D11: **229/229 tests passed**, zero failures/skips (`artifacts/cave-test-run-final.xml`).
- Actual physics: optional traversal, full level through the cave and main-route bypass, reversible entry, local catch return, per-ledge failures, water crossings, checkpoint ownership, pause/retry/restart and unchanged renderer activation.
- Scripted direct route: **60.48 seconds** in the test pilot; normal-speed render recording approximately 60.5 seconds. This is not a human first-play measurement.
- The recorded deliberate missed jump reaches the catch after approximately 3 seconds. Its return is separately verified by actual physics.
- Windows development build succeeded. Packed-art audit verified **64 active paintings and three shader-reference materials**, with historical artwork excluded.
- Imported descent image memory reported by Unity: **52,446,720 bytes**, approximately 50.0 MiB. Runtime coverage/contact fields add approximately 8 MiB plus small light/dust sources, below the additional-texture target of approximately 128 MiB. No actual player frame-time benchmark is claimed.
- 960×540 and 640×360 color stills, small grayscale stills, normal-speed route and deliberate-fall recordings are stored beside this document. Both captures use real motor input/physics with deterministic editor rendering.

The unique fountain is registered to its painted footing and the permanent side-light effect follows the chamber's world anchor. The route uses fixed rock geometry, two-dimensional coverage and shaped shore connections, not position-triggered renderer reveals. Main-route platform contacts, checkpoint 2 and the finale are retained.

## Delivery boundary

The local preview is available for the owner's requested playtest. PR #79 remains a draft during that review. Current Linux player/launcher/package and exact-head merge-ready checks are not claimed; those follow completion of player review under the repository workflow. No merge or release is authorized or performed.
