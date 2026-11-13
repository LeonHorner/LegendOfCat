# Level progress HUD

The fill measures Mika's current distance along the route. The diamond marks
the most recently activated checkpoint. The pennant marks the finish. The
default-on setting is available from the main menu.

When adding a level, register its launch factory in LevelRegistry and supply
an ordered local-space route through its traversable waypoints, including
the start, each checkpoint spawn, and finish. Use distinct PhotoCraft track,
fill, checkpoint and finish artwork. Review the artwork in gameplay before
integration, especially the checkpoint against filled and unfilled track.
The integration test discovers levels from the same registry.

The review mockups composite the PhotoCraft exports over archived gameplay
captures at 290 pixels wide, with grayscale and enlarged icon previews.
Actual HUD layout still needs an in-editor check.
