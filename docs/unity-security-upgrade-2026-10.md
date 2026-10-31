# Unity October 2026 security upgrade

Issue: [#55](https://github.com/LeonHorner/LegendOfCat/issues/55).
Assessment date: 2026-10-31. This upgrade stays on Unity 6.3 LTS.

## Editor and packages

The installed Windows Editor at `D:\Programs\Unity\Editors\6000.3.27f2\Editor\Unity.exe`
imported this actual worktree with its own new Library. The Editor wrote
`6000.3.27f2 (22188f149405)` to ProjectVersion.txt; it was not manually substituted.
The import log identifies the same engine revision and exits successfully (code 0).

Unity's installed `Data/Resources/PackageManager/Editor/manifest.json` specifies
Input System **1.20.1** as its minimum and bundled version. Import therefore updates
the project's 1.17.0 pin and lock entry to 1.20.1. This is an Editor compatibility
requirement. Import also aligns the Test Framework manifest entry from 1.4.5 to
the already locked, built-in 1.6.0. Other resolved dependencies remain unchanged.
Manifest ordering is Unity-authored. No unrelated package upgrade is intended.

The official registry reports Pipeline 0.9.1-exp.1 as latest. Its published
changelog addresses command attributes, Unity 7 analyzer warnings and non-Standalone
build settings, with the same dependencies. It identifies no fix for these
advisories. Retain the reviewed **com.unity.pipeline 0.9.0-exp.1**; do not infer
that a different experimental version is a required security fix.
Registry checked: <https://packages.unity.com/com.unity.pipeline>.

## Security coverage and data paths

[Unity's release notes](https://unity.com/releases/editor/whats-new/6000.3.27f2)
record fixes to Mono/IL2CPP Brotli, UnityTLS certificate rejection and Package
Manager's bundled Node.js. The Editor upgrade covers these Unity components;
existing published binaries do not change. Rebuilt players use the new runtime.

[eMudhra advisory](https://unity.com/security/unity-emudhra-compromise)
(CVE-2026-111943, as tracked by the issue): update the Editor and maintain OS trust
updates. The game's update/history requests run in the separate launcher process,
so a UnityTLS patch does not establish coverage for that process's TLS validation.

[Brotli advisory](https://unity.com/security/unity-brotli-decoder-vulnerability)
(CVE-2026-111497): exposure requires attacker-controlled Brotli decompression.
Package Manager is an Editor exposure even without a gameplay decoder; this project
uses the Unity registry, with no custom scoped registries. The Editor patch covers it.

The project assessment goes beyond identifier searches:

- `DesktopMenuPlatformService` starts the launcher helper and exchanges line-based
  commands/JSON through redirected standard input/output. It does not receive HTTP
  responses or decompress downloaded packages in the Unity process.
- Gameplay and presentation load repository assets through Unity resources and
  normal scene/prefab loading. Save/settings readers parse local versioned JSON;
  they do not decode compressed player input. There is no network content-upload
  or editable compressed-content import feature in this baseline.
- The resolved package cache contains Input System, Newtonsoft JSON, NUnit/Test
  Framework, Mono Cecil, Pipeline and Unity modules. No ML-Agents or AI Assistant
  package is installed. Neither is an in-scope dependency of Pipeline.
- Reviewed Pipeline source at package revision
  `4c0735f887a9994a6ce525694af6e83cb5cd5d65`: `BasePipelineServer` binds loopback,
  checks authentication/remote addresses, bounds request bodies and reads them
  through `StreamReader` from `HttpListenerRequest.InputStream`. It has no Brotli
  request decoder or automatic HTTP response decompression. Its interpreter's
  `DecompressInt` decodes metadata integers, not Brotli streams. No additional
  native plugin/DLL assets are supplied by this project. Keep the connection local
  and editor-only as required by AGENTS.md.

Conclusion: no current Unity gameplay path was identified that passes untrusted
Brotli to the affected APIs. This is a dependency/data-flow assessment, not a
malicious-stream penetration test or a guarantee about future features. The Editor
and rebuilt runtime are patched regardless of that present exposure finding.

## Separate launcher runtime

`Launcher.csproj` targets net8.0, pins Velopack 1.2.161 and publishes self-contained
win-x64/linux-x64 executables. Unity does not patch their bundled .NET runtimes.
`UpdateService.Run` uses a default HttpClient for GitHub release JSON, with no
automatic Brotli decompression. `ReportingDownloader` observes writes and delegates
HTTP to Velopack. The pinned
[Velopack implementation](https://github.com/velopack/velopack/blob/1.2.161/src/lib-csharp/Sources/HttpClientFileDownloader.cs)
sets only GZip and Deflate on its handler; response streams are copied to disk.
The custom override does not change those flags. No explicit Brotli decoder is
present in launcher code. These inspected HTTP paths therefore do not expose
Brotli decompression, consistent with
[Microsoft's advisory](https://github.com/dotnet/announcements/issues/494).

Residual limitation: this assessment does not certify the runtime bundled in an
already published installer, Velopack's native binaries, or platform certificate
stores as patched. Keep OS trust stores current and review the exact self-contained
runtime and native updater versions before the next authorized release. No launcher
dependency/runtime upgrade or release is included here.

## Validation

Local Windows commands use the exact Editor above and this worktree as
`-projectPath`; outputs/logs are ignored under `artifacts/security-upgrade/`.

```text
Unity.exe -batchmode -nographics -quit -projectPath <worktree> -logFile <output>/import.log
Unity.exe -batchmode -nographics -projectPath <worktree> -runTests -testPlatform EditMode -testResults <output>/editmode.xml -logFile <output>/editmode.log
```

- Import/compilation: passed, exit 0, exact engine revision confirmed in the log.
- EditMode: **137 total, 137 passed, 0 failed, 0 skipped**, test runner exit 0.
- Windows release player and Development preview: both built with the patched
  Editor and exited 0. Commands use `-buildTarget Win64 -executeMethod BuildGame.Build
  -releaseVersion 0.0.1 -releaseOutput <output>/win/Game/LegendOfCat.exe` and
  `-executeMethod BuildGame.BuildPreview -previewOutput <output>/preview/LegendOfCatPreview.exe`.
- Actual Windows preview startup, menu, Settings, movement-lab entry/idle rendering
  and Back to menu were inspected through the UI. Player.log confirms
  `6000.3.27f2 (22188f149405)` and contains no logged errors/exceptions.
  Short automated key presses did not establish visible running/jumping, so manual
  locomotion is not claimed; the EditMode suite covers input and simulated physics.
- Local Linux player attempt exited 1: `Currently selected scripting backend
  (Mono) is not installed.` The Windows Editor has Linux IL2CPP support, not the
  required Linux Mono backend. Do not change the project's scripting backend to
  hide this environment limitation. Linux cross-toolchain packages automatically
  added by that attempt were removed; final import/tests recheck the original graph.
- Self-contained launcher publish succeeded for win-x64 and linux-x64 with
  `dotnet publish Launcher/Launcher.csproj -c Release -r <rid> --self-contained true
  -p:RestoreLockedMode=true -o <output>/launcher-<platform>`.
  Both runtimeconfig files identify Microsoft.NETCore.App **8.0.33**; this records
  the local build runtime, not an assertion about previously shipped installers.
- `dotnet run --project Launcher.WorkflowTests/Launcher.WorkflowTests.csproj
  --configuration Release`: **19/19 passed**. The corresponding
  `tools/LauncherProgressTests/LauncherProgressTests.csproj` checks also passed.
- `npm ci --ignore-scripts` and `node --test tools/ci-workflow.test.mjs
  tools/release-assets.test.mjs tools/delta-release.test.mjs tools/application-icon.test.mjs`:
  **30/30 passed**. npm reported 37 development dependency audit findings
  (36 moderate, 1 high); no unrelated lockfile updates were made.

Prepare merge remains authoritative for exact-head Unity tests, Windows/Linux
players, launcher packaging and Linux payload startup. The existing game-ci actions
infer the Editor from ProjectVersion.txt (no workflow version override); their logs
must identify 6000.3.27f2. Cache keys include the version file and package graph.
Local Windows cannot establish Linux execution or installed AppImage behavior.
Until all required jobs pass and `merge-ready` succeeds, this upgrade is not
presented as merge-ready. See the PR checks for final revision-specific results.

## Coordination and integration

No native blockers or competing upgrade branch existed when starting. Main
`cb524b0` was the initial base. Reviewed open PRs #51 (Blender sources), #53
(keepsake design), #54 (painted course/input). This branch owns the Editor/package
pins and upgrade documentation; other worktrees retain their own Editors/Libraries.
Input System's required version change is the integration point for #54's input
work. No shared scene, prefab, asset, metadata or persistent schema changes belong
to this upgrade. Historical test reports retain their original Editor attribution.

Actual installed-player and N -> N+1 update smoke tests remain part of the separately
authorized release process. No publication or feature-to-main merge is authorized.
