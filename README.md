# LegendOfCat

Unity starter for a 2D game with optional 3D visuals. The previous Java game is not migrated.

## Open
Install Unity Hub and Unity **6000.3.27f1**, then add this folder as a project.
Use **LegendOfCat > Create starter scene** to create/open the initial scene.
The first import generates Unity settings and asset metadata; commit them.
Unity is not installed on the machine used to create this scaffold, so the Editor build has not yet been verified.

## Repository language
All maintained repository content is English, including code, comments, documentation, branch names, commits, pull requests, UI text, and release notes. Preserve third-party legal notices and proper names verbatim.

## Development
Use short `feat/*`, `fix/*`, or `chore/*` branches and squash PRs into main.
PR titles must use Conventional Commits. No automatic CI on pushes or PRs.
See AGENTS.md. External code contributions are not accepted; issues are welcome.

## Release
Configure Actions secrets UNITY_LICENSE, UNITY_EMAIL, UNITY_PASSWORD (GameCI activation).
Run Actions > Release on main. No version input:
fix/perf = patch, feat = minor, !/BREAKING CHANGE = major.
docs/chore/test alone produce no release. The first release is 1.0.0.
Tests and Windows/Linux builds run only in this workflow. Publishing happens only after all jobs succeed.
Git tags are the version source; no bot version commits.
Builds are unsigned until a signing certificate is configured.

## Updates
Launcher/ is a small self-contained C# launcher using Velopack.
It checks public GitHub Releases once per launch, applies available updates before starting the game,
and launches the installed game if checking/downloading fails. Saves belong in Application.persistentDataPath.
Windows downloads use Setup.exe; Linux uses AppImage. Never distribute only the Unity player executable.
Update packages and feeds must stay attached to releases. Old Java builds cannot self-update to this project.
First-release installation and N -> N+1 updating on Windows/Linux must be verified before promoting to users.

## License
Original code: PolyForm Noncommercial 1.0.0 (LICENSE).
Original artwork/audio/models, where copyright exists: CC BY-NC 4.0 (ASSET-LICENSE.md).
Third-party components retain their own licenses. AI provenance does not guarantee copyright protection.

Release tooling dependency audit currently flags transitive braces/micromatch and npm/ip-address advisories even on latest direct packages. The affected pattern inputs are repository-controlled; no issue content is executed. Recheck before the first release.

Branch protection requires squash PRs, blocks deletion/force-push, and requires resolved conversations; no reviewer approval or CI checks are required. Conventional Commit subjects are validated by the manual release preflight (GitHub rejected its commit-pattern ruleset for this repository).
