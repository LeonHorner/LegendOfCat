# Mika in Level One

From the game menu, open Level One. In the Unity Editor, **Tools → Play Mika
in Level One** opens it directly for review.

The model uses the existing sprite presentation's ground anchor and follows
the controller's movement direction. Water recovery controls visibility
through the original sprite renderer. The running, walking, relaxed idle and
airborne clips are imported from the bundled FBX.

The editable rig and walk/idle authoring script are in
`art/blender/mika-rig/`. See ASSETS.md for provenance. Further animation
variants and transitions are tracked in issue #78.
