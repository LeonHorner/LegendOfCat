# Development

Maintainer documentation for the Unity project. For player information, see the [README](../README.md). Follow [AGENTS.md](../AGENTS.md) for binding agent rules.

## Open
Install Unity Hub and Unity **6000.3.27f1**, then add this folder as a project.
Use **LegendOfCat > Create starter scene** to create/open the initial scene.
Commit generated Unity settings, Packages/packages-lock.json and any new .meta files after the first import.
The starter scene test and Windows player build have passed in CI; full platform validation is recorded in the setup PR.

## Repository language
Use English for maintained code, comments, documentation, filenames, branches, commits, PRs, UI text and release notes. Preserve third-party legal notices and proper names verbatim.

## Issue-to-feature development
Tell an agent: "Implement issue #123." The agent reads [AGENTS.md](../AGENTS.md), the issue and related work, then reuses or creates a feature branch and PR.
One coherent feature can contain several issues and stays on one branch until complete.
Draft PRs track unfinished features. List every related issue, acceptance criterion, remaining task and documentation change.
Use an English Conventional Commit PR title describing the complete user-visible result. Squash merge turns it into one commit and one release-note entry.
External code contributions are not accepted; public bug reports and feature requests are welcome.

For concurrent work, inspect open PRs, active branches and native issue dependencies before editing.
Record relevant shared components, ownership and integration checks in the PR's Coordination section.
Use a separate worktree and Unity project directory per concurrent agent, with its own Library.
Agree on a small interface, an integration owner or dependency sequencing when components overlap;
do not independently edit another feature's entry point or metadata. See AGENTS.md for the full rules.

## Prepare a merge
Draft PRs do not trigger full CI. Ready-for-review PRs automatically run **Prepare merge** on opening, reopening, becoming ready and new commits; title/base edits also revalidate. A new run for the same PR cancels the older one, never another PR's run. There is no automatic merge.

1. Review newly merged PRs, merge current main normally into the feature branch, and preserve both features when resolving conflicts. Inspect the relevant end-to-end behavior and update PR integration/risk notes without rewriting published history.
2. Finish documentation/self-review and mark the PR **Ready for review**. This triggers automatic validation; subsequent pushes and normal `main` merges trigger a fresh run. Draft work remains unvalidated.
3. If the automatic workflow did not run, use **Actions > Prepare merge** and enter the PR number. CLI fallback: `gh workflow run pull-request.yml -f pr_number=123`. Do not start both unnecessarily.
4. Preflight checks the live PR's title, branch and base. Unity tests and the Windows/Linux player and launcher builds then run independently, all checked out at the exact PR head SHA; Linux also receives the packaged-payload startup smoke check.
5. After every required job passes, the final gate rechecks the PR revision/base/title and marks **merge-ready** on the exact head. Tests and logs remain as downloadable workflow artifacts. An outdated base, changed PR, failed or skipped required job cannot grant readiness.
6. Merge only when explicitly authorized: `gh pr merge 123 --squash --delete-branch --match-head-commit <validated-sha>`.

See [CI validation and measurements](ci-validation.md) for the trigger, safety and performance contracts.

Main requires a PR, resolved conversations, the merge-ready status and an up-to-date branch. No reviewer approval is required.
Normal merge commits and rebase merges into main are disabled. Force-pushes and deletion of main are blocked, with no bypass actors.
Agents must never force-push any branch. Feature branches may merge main normally to update their base.
A requested issue implementation stops at a ready PR with final validation results; it does not imply permission to merge or release.

## Local Windows UI preview

Run Unity in batch mode with `-executeMethod BuildGame.BuildPreview` and
`-previewOutput <absolute-output-folder>/LegendOfCatPreview.exe` to build a windowed UI preview.
Keep the entire output folder with the executable; Unity players are not single-file applications.
Preview-only buttons show the update pop-up and simulate downloads/errors without networking,
installing updates or quitting to an updater. Preview settings use a separate persistent data
directory (`LeonHorner/LegendOfCat Preview`). The painted menu, Settings and movement-lab entry
are the same UI as the release build; simulation tools are compiled out of release builds.

## Official Unity agent connection
The official com.unity.pipeline package is pinned to 0.9.0-exp.1. Open this project in Unity before using the connection.
The verified local CLI version is 1.0.0-beta.13. Run unity status to check readiness, then unity command get_hierarchy for a read-only check.
For Codex, configure a local MCP server running unity mcp --project-path <absolute-project-path>. Keep machine-specific executable and project paths out of Git.
The setup machine has a local .codex/config.toml excluded through .git/info/exclude. Reopen the project in Codex to load the connection.
CLI and MCP use the same Pipeline backend: CLI is convenient for scripts; MCP exposes discoverable structured tools directly to agents.
No tests are run by connecting MCP. Follow the ready-PR automatic validation rules in [AGENTS.md](../AGENTS.md).

## Design references

**Movement lab (prototype) > Enter movement lab** opens the reusable movement lab. See [Movement prototype](design/movement-prototype.md)
for controls, course geometry, jump reach and the prefab regeneration command.

Read the [game direction](design/game-direction.md), [art direction](design/art-direction.md) and [implementation plan](design/implementation-plan.md) before gameplay or visual work. The Unity adventure reimagines the earlier Java game rather than copying its gameplay specification.

For release operations and updater behavior, see [Releasing](releasing.md).

## Painted menu inspection

The menu retains IMGUI and the existing settings schema (`schemaVersion: 0`).
`StarterScreen.Presentation.cs` owns styling and layout; `IMenuPlatformService`
is the small update boundary. Desktop launch detection and the unchanged line-based
service protocol live in `DesktopMenuPlatformService`. Other platforms and direct
player/editor launches use `UnavailableMenuPlatformService`: the menu/settings
remain usable, update actions are disabled, and no executable is assumed to exist.
The launcher still owns installation and automatic restart after a consented download.

Desktop player and preview windows can be resized by dragging their borders or corners.
Both build entry points enable Unity's resizable-window setting.

The content region follows `Screen.safeArea`, with a 420-pixel logical width reference,
bounded scaling and scrollable content. Buttons are at least 48 physical pixels high.
Tab/Shift+Tab or arrow keys move focus; Enter/Space activates buttons. A gold border
marks focus independently of text color. A short page opacity transition leaves input
immediate; scenery does not move. Update dialogs trap menu interaction and offer
Later as their initial focus. **Movement lab (prototype)** opens an introduction explaining
the temporary course and unfinished campaign; **Enter movement lab** starts it. The menu's
single OnGUI implementation skips all painting while the lab is active. The menu component
continues receiving update-service messages, preserving settings and once-per-version reminders.
Return with **Back to menu**, Escape or controller Start; the lab stops immediately, its input
actions are disabled and the painted menu resumes with fresh keyboard navigation state.

Open `Assets/Game/Scenes/Main.unity` in the pinned Editor and enter Play Mode to inspect the
combined flow. Verify menu keyboard focus, entry, run/jump/retry, return, then Settings and
Versions & updates. Repeat entry/return and resize the window; the complete campaign is not
playable. Input handling remains Both so the IMGUI menu and shared Input System movement
actions work together. Desktop launcher behavior and settings schema are unchanged.
See the [integration review and captures](design/menu-movement-integration/README.md)
for conflict resolutions and the menu/course regression test included in final validation.

The regression checks separate scene/input lifecycle from rigidbody simulation:
`MenuMovementIntegrationTests` verifies menu entry, update-message preservation, focus
loss/recovery, input shutdown and re-entry without frame-timing-dependent movement assertions.
`CatCollisionTests` calls the controller's fixed-step logic and `Physics2D.Simulate`
explicitly, then checks `Rigidbody2D.position` for running in both directions and jumping.
Do not assert physics movement using interpolated `Transform.position` after a fixed number
of `WaitForFixedUpdate` yields in headless Unity test runs.

Preview builds include state inspection controls (checking, available/pop-up,
download progress, ready, current, error, warning and unsupported platform).
Example releases are labeled as simulations and appear only in preview builds.
Optional player arguments produce actual rendered captures and exit:

```text
LegendOfCatPreview.exe -screen-fullscreen 0 -screen-width 1280 -screen-height 720 --preview-view menu --preview-capture <absolute-path>.png
```

`--preview-view` also accepts `settings`, `play`, `popup`, `available`, `checking`,
`downloading`, `ready`, `current`, `error`, `warning`, and `development` (unsupported).
`--preview-suite <absolute-directory>` captures the full state/size matrix and exits.
Keep the preview window visible during capture; a hidden window can produce black
images even while the simulation runs. The Preview tools / Capture view matrix
button writes the matrix to `Application.persistentDataPath/menu-preview-captures`
and exits. Direct3D 11 can be selected with `-force-d3d11` on Windows.
Allow the capture sequence to finish before closing it.
Add `--preview-cutout` to simulate 50-pixel side insets and 25-pixel top/bottom
insets. This is local visual inspection, not mobile store or touch-gameplay support.
Preview builds use a separate settings directory and never contact the update feed.
See [menu preview captures](design/menu-preview/README.md) for inspection evidence.


## Desktop display and compact release history

Players start in a resizable 1280×720 window. Settings and Alt+Enter select
borderless fullscreen; returning restores the last window dimensions. The independent
`display-settings.json` schema 0 follows the versioned-file contract and preserves
unknown fields. Resize observations debounce for 1.5 seconds; quitting or entering
fullscreen flushes the last observed window dimensions. Mode requests wait for actual
Unity/compositor observations (multiple frames and a settling interval); rejected
requests time out after five seconds and never save the requested mode as observed.
Minimized surfaces are ignored. Preview players use the same display behavior in
the separate Preview product data directory; the Editor does not change its host window.

960×540 is the target usable size, not a native enforced minimum. Unity's common
standalone settings expose resizability but no portable minimum client dimensions.
Win32 window procedure hooks would require lifetime/subclassing management; Linux
would need separate X11 and Wayland/compositor behavior. Those native dependencies
are not justified here. The fallback uses safe-area layout, readable minimum text
scale and scrolling content, with fixed top history navigation. It never calls
SetResolution in a resize loop or fights the window manager. Very small windows
below the target cannot guarantee the same amount of visible content. Positive
window sizes down to 320×180 can be retained; invalid/future files remain untouched.
Gameplay always uses the same orthographic 16:9 framing, with black letter/pillar
bars and a fixed camera width; menu layout fills the available window independently.

History uses the launcher's existing release messages, without changing feeds or
update ownership. A bounded line/token parser supports headings, bullet/numbered
lists, inline emphasis/code and link labels; generated download sections, commit
links, URLs, fenced code, tables and reference definitions are omitted. Each card
links to the full original GitHub notes. At most 100 cards, 120 content blocks per
card and 32 KiB of Markdown per release are rendered; parsed summaries are cached.
The top Back to settings control is outside the scroll area and keyboard focus does
not scroll the content when that fixed control is selected. Full Markdown remains
on GitHub. This is intentionally a compact summary renderer, not CommonMark.

## Linux KDE menu description

Velopack 1.2.161 embeds `Comment=LegendOfCat <version>` in its
AppImage desktop metadata. KDE or an AppImage integration utility may copy
that comment to a separate user-owned `~/.local/share/applications/*.desktop`
entry. In-place AppImage updates do not refresh such a copied entry.

At each normal Linux AppImage startup, the launcher narrowly updates a stale,
**generated** `Comment=LegendOfCat x.y.z` to
`Comment=A little cat, a big adventure` in an existing desktop entry only
when the entry's `Name=LegendOfCat` and `Exec=` points to the exact launched
AppImage. It respects `XDG_DATA_HOME`, leaves user-written descriptions and
other desktop keys unchanged, skips symlinks and treats inaccessible entries
as optional. It does not create or remove KDE menu entries or modify other
applications. Desktop menu refresh may occur on KDE's next cache update.
Freshly integrated AppImages may initially inherit Velopack's packaged
versioned comment until their first launch.

To inspect the current entry, check `Comment`, `Name` and `Exec` in
`~/.local/share/applications/*.desktop`. Regression tests exercise the
actual C# migration with temporary desktop files; do not use source-text
assertions as a substitute.

The authoritative application icon is tools/icon.ico. Regenerate its lossless PNG
variant with `node tools/application-icon.mjs`; do not edit the variant independently.
BuildGame configures default/standalone icons before automated player builds, and
Launcher.csproj embeds the original ICO. Both Prepare merge and Release package
Windows installers/shortcuts and Linux AppImages using these icons. Prepare merge
packages are validation artifacts and never publish a release.


## Update download size display

Downloading updates show remaining and total payload MB (1 MB = 1,000,000 bytes),
with one decimal place rounded up. The launcher observes completed writes through
Velopack's existing HTTP downloader, including responses without Content-Length;
it does not estimate bytes from the overall percentage, which also covers Delta
patch preparation. Concurrent Delta files contribute to the same total. If
Velopack falls back to a full package, the displayed total switches to that full
package and the byte count restarts. A transport retry resets only its own file.
Once payload bytes are complete, the UI says that the update is being prepared.
Wire overhead and previous failed-attempt traffic are not included in these numbers.

Optional downloadedBytes/totalBytes fields extend existing downloading messages;
older launcher helpers continue to show percentage only. No commands, release
feeds, persistent settings or update ownership changed. The byte observer delegates
HTTP, redirects, retries, checksum verification, Delta selection and application to
the pinned Velopack implementation. Progress messages are limited to four byte-only
updates per second, plus existing percentage/transfer-boundary notifications.


The native downloader integration checks run on Windows and Linux in Prepare merge:
`dotnet run --project tools/LauncherProgressTests/LauncherProgressTests.csproj --configuration Release`.
They drive the actual pinned downloader override with in-memory HTTP responses
without Content-Length, concurrent Delta files, full fallback and repeated attempts;
no installation or live update is performed.
