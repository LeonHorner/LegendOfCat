# Painted menu and movement integration

PR #30 incorporates merged PR #31 through a normal merge of main. The captures below
come from the actual Unity 6000.3.27f1 Editor Game View on 2026-10-23; no new artwork
or update-service implementation was introduced.

| Capture | Inspected behavior |
|---|---|
| [Menu](menu.png) | Painted presentation retained; movement entry explicitly labeled as a prototype |
| [Introduction](entry.png) | Keyboard Tab/Enter reaches the course introduction; unfinished campaign and placeholder art are explicit |
| [Lab](lab.png) | One active course, no painted menu over gameplay; original controls and camera remain |
| [Returned menu](returned-menu.png) | Lab callback immediately deactivates the course and restores the painted menu |
| [Settings after return](settings-after-return.png) | Keyboard navigation and painted settings layout remain usable after gameplay |
| [Updates after return](updates-after-return.png) | Editor uses the existing unavailable-platform service; update actions remain disabled |

The integration retains one menu OnGUI in `StarterScreen.Presentation.cs`, which
returns before painting while the course is active. `StarterScreen` keeps receiving
update messages; entering or leaving the lab does not replace its platform service,
settings data or pending notices. Escape, controller Start and the lab's Back to menu
button share the return callback. A fresh course is created on the next entry.
The local scripted visual traversal fed the existing keyboard actions and completed
the four course jumps, ending on the finish platform at X=43.23. No movement values,
camera tuning or controller bindings changed during integration.
The shared Escape action also returned to the painted menu with zero active courses.

## Conflict review

- `StarterScreen.cs`: preserve PR #31's partial-class split and platform service;
  retain only PR #30's course lifecycle, wired through the painted presentation.
- `Assets/Game/Resources.meta`: retain main's folder GUID
  `96ae55028176f6d44bfa8cd19512d186`. No serialized asset referenced PR #30's old folder
  GUID `3e8cb38e1ebdadc4885b1f9e5b4411d6`; child prefab, script and artwork GUIDs are unchanged.
- Project settings merged without conflict: main's resizable-window and other settings
  remain, with `activeInputHandler: 2` (Both) from movement. Package configuration retains
  the pinned Input System and imported lockfile.
- ASSETS.md and development documentation merged cleanly; keep both features' provenance
  and replace obsolete entry descriptions with the combined behavior.

`MenuMovementIntegrationTests` adds an EditMode UnityTest that enters Play Mode for
the real menu/course lifecycle, movement, update-message preservation and re-entry.
It runs only in the final manual Prepare merge workflow, alongside the existing
menu, update, persistence and movement tests. Screenshots establish presentation,
not hardware input feel or high-refresh performance; see the
[movement tuning record](../movement-prototype.md) for those limits.
