# Level progress HUD

The top-center outline shows the full route. The painted stroke measures Mika's current distance along the route. Mika's
front-facing portrait moves with the stroke tip. The HUD shows every world
checkpoint at its fixed position on the bar. Mika passes in front of it, and
a small upward pointer guides the player to the next checkpoint. The pointer
glows when Mika reaches it and advances once the portrait moves five HUD
pixels from the checkpoint center at full HUD scale. It disappears after the final checkpoint and resets on restart.
Unreached checkpoints are gray, reached checkpoints use their
original colors, and the active retry point has a small gold underline. The default-on setting is available from the main menu.

A PR notch above the bar tracks the furthest progress reached in each level.
It stays at the record when Mika backtracks, retries, or restarts. Records
are saved on checkpoint activation, completion, menu return, and game quit.
Developer preview runs do not update records.

When adding a level, register its launch factory in LevelRegistry. Supply an
ordered local-space route through the start, checkpoint spawns, and finish.
Use a distinct painted fill and bind each checkpoint to its world renderer.
The integration test discovers levels from the same registry.

The mockups use archived gameplay captures to show Mika approaching, aligned
with, and passing a checkpoint at two viewport widths. The earlier PhotoCraft
portrait study is included. The runtime portrait uses the playable sprite
references documented in ASSETS.md.
The existing fill textures come from the original PhotoCraft concepts and
their pigment pass. Run paint-progress.py from the repository root with the
folder containing the original fill PNG exports to reproduce that pass.
