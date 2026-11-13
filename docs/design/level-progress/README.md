# Level progress HUD

The top-center stroke measures Mika's current distance along the route. Mika's
portrait moves with the stroke tip, and the world checkpoint icon marks the
active retry position. The default-on setting is available from the main menu.

When adding a level, register its launch factory in LevelRegistry. Supply an
ordered local-space route through the start, checkpoint spawns, and finish.
Use a distinct painted fill and bind each checkpoint to its world renderer.
The integration test discovers levels from the same registry.

The mockups use archived gameplay captures to show the HUD at start, past a
checkpoint, and after backtracking. The layered PhotoCraft portrait is included.
The existing fill textures come from the original PhotoCraft concepts and
their pigment pass. Run paint-progress.py from the repository root with the
folder containing the original fill PNG exports to reproduce that pass.
