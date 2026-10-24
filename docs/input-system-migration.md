# Input System migration (#46)

The project enables only Unity Input System (`activeInputHandler: 1`) with the
existing pinned `com.unity.inputsystem` 1.17.0 package. The gameplay adapter,
controller bindings and display-settings schema are unchanged.

## Runtime audit

- `CatInput` already uses Input Actions for keyboard/gamepad movement, jump,
  checkpoint retry, run restart, pause and return to menu. Its focus and disable
  handlers cancel actions and reset motion.
- `StarterScreen` now polls Input System keyboard controls for either Alt key plus
  a new press of Return or keypad Enter. Holding the chord does not repeat;
  pressing Alt after an already held Enter does not toggle. A missing keyboard
  returns false. The existing display request/debounce/resize/persistence path is
  retained.
- `StarterScreen.Presentation` uses IMGUI `Event.current` for Tab/arrows/Enter
  navigation and pointer-driven controls. These events and `KeyCode` values do not
  poll the legacy Input Manager. Alt+Enter is consumed before menu activation.
  `MovementCourse` also draws its controls with IMGUI. There are no runtime
  EventSystem/StandaloneInputModule components or legacy axis/button/mouse polls
  in the production scripts or serialized assets.
- `ProjectSettings/InputManager.asset` and `ClusterInputManager.asset` are retained
  Unity settings files, not active runtime consumers. No input module replacement,
  package upgrade or scene/prefab change is required.

## Validation (2026-10-24)

- Unity 6000.3.27f1 imported this isolated worktree and passed all 130 EditMode
  tests. Nine new behavioral cases cover the four Alt/Enter combinations, holding,
  release/repress, ordinary Enter, Alt after held Enter, no keyboard, and real
  keyboard/gamepad action callbacks through focus loss/regain and jump release.
  InputTestFixture isolates simulated devices from native desktop input.
- Existing display tests cover asynchronous fullscreen confirmation, preservation
  of window dimensions, resize debounce and rejected-mode timeout. Existing menu
  and lifecycle integration tests remain green.
- All 30 Node workflow/packaging contract tests passed after installing the pinned
  development dependencies. `Packages/packages-lock.json` is unchanged after import.
- The Editor test log contains no legacy Input Manager deprecation warning or
  legacy polling exception with the new-only configuration.
- A production Windows player built successfully and pointer navigation to settings
  works. Automated desktop Alt+Enter injection did not produce a mode change;
  physical-keyboard verification remains pending. This is not recorded as a passed
  shortcut check. Exact-head CI remains pending while the PR is draft.

Linux graphical keyboard shortcuts have not been exercised on this Windows host.
A desktop/window-manager shortcut may intercept Alt+Enter before Unity receives
it; left/right Alt and keypad behavior therefore still need an actual Linux
session check. Automated input tests and Linux builds do not establish compositor
shortcut delivery. No verified Linux shortcut behavior is claimed.
