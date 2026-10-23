# Persistent data and migration contract

This contract applies to future issues, maintainers and AI agents. Implementations must follow
it whenever they add or change files containing player-owned data. The current implementation
is `VersionedJsonFile` with the `MenuSettings` schema. There are no gameplay saves or editable
level files yet; do not invent their data models before those features exist.

## Independent schemas

- Each file format has its own integer `schemaVersion`, independent of the application version.
  Menu and display settings each use independent version **0** schemas, with no display predecessor and no display migration steps. Menu schema 0 shipped in v0.2.0.
  Future saves and editable levels also start at **0** and maintain their own
  version histories. Shipped Unity scenes/assets are build inputs, not migrated user files.
- `schemaVersion` is mandatory and must be a non-negative integer. Missing/invalid versions
  are protected as invalid data. No unversioned settings format has been published.
  A migration transforms exactly N to N+1; keep published steps intact.
- **Compatibility is promised only between published stable releases.** Unreleased schemas
  may evolve in place without a migration or increment. Before an incompatible change to a
  released format ships, bump its schema once and migrate directly from the last published
  schema. Do not add steps for commits, PRs, local previews or other unreleased intermediates.
- New optional fields with safe defaults may be backward-compatible without a version bump.
  Renaming/removing fields, changing their meaning/type or restructuring data requires a bump
  and a migration. Keep unknown fields when reading and writing supported schemas.
- Store player data under `Application.persistentDataPath`, outside the installation.
  Preview builds use a separate product name and therefore a separate data directory.

## Loading and migration

1. If the file is absent, use current defaults. Do not create a file until it needs saving.
2. Parse an object and read its version. Preserve malformed/unreadable files; use temporary
   defaults for settings and disable writes. Gameplay saves/levels must instead report that
   the file cannot be loaded: never silently create a replacement save.
3. If the version is newer than supported, preserve the file byte-for-byte and disable writes.
   Settings may use temporary defaults for the session; gameplay must refuse the affected file.
4. For an old supported version, run every migration in memory, update `schemaVersion` after
   each step, then validate the complete result before writing anything to the source file.
   Migration delegates must only transform the provided object; no file, network or game-state side effects.
5. Copy the exact original bytes to `<file>.v<N>.<unique-id>.bak`. Keep recovery copies; never
   overwrite an earlier backup. No backup or migration is needed for an already current file.
6. Write UTF-8 to a unique temporary file in the same directory, flush it to disk, then replace
   the original with `File.Replace`. For a new file, use `File.Move` without overwrite. Never
   delete the original as a replacement fallback. Unsupported replacement/filesystem errors
   leave the original in place and surface a warning.
7. On a migration failure, retain the original and any recovery copy, block writes and show a
   clear warning. The store checks for edits since load before replacement; callers must still
   arrange a single writer. This is not a multi-process database or a guarantee against disk failure.

`VersionedJsonFile.CanWrite` and `Warning` must be checked by callers. Its `Data` contains
temporary defaults on a failed/unsupported load, so gameplay must not treat a blocked load as
an empty real save. `Save` refuses writes after a blocked load. Backups are retained for manual
recovery; automatic recovery from an arbitrary backup and down-migrations are not implemented.

## Current settings format

Version 0: `{ "schemaVersion": 0, "lastOfferedVersion": "0.2.0" }`

Menu schema 0 shipped in v0.2.0. There is no 0-to-1 migration yet. A missing reminder uses an empty string. An explicitly
non-string reminder is invalid and must not be overwritten. Reminder writes preserve unknown
fields and go through the same validated atomic-save path. The chained migration tests use
synthetic examples to verify the mechanism; they do not establish released gameplay schemas.

## Desktop display settings

`display-settings.json` is independent from `menu-settings.json` and starts at schemaVersion 0. It stores `width` (640–16384), `height` (360–16384) and `fullscreen` (boolean). It uses `VersionedJsonFile` to preserve unknown fields and block writes to invalid, unreadable or newer files. A missing file defaults to a windowed 1280 × 720 player. Display settings never reset menu settings or game saves.

## Checklist for each future issue or PR

- Identify every affected format, its current version and whether the change is compatible.
- Check the last published stable release before planning compatibility work. For incompatible
  changes to a published format, increment only that format's version and add its N-to-N+1 step.
  A store for version N supplies N migration delegates, indexed 0 through N-1.
- Keep the schema-specific defaults and validator next to that format's migrations. Reuse the
  file handling procedure; do not couple settings, saves and levels to one version counter.
- Commit versioned fixture files and meaningful tests for preservation of real fields, chained
  migrations, backup bytes, malformed input, unknown future versions and failed validation.
- Verify that failed saves leave the original intact, supported round trips retain unknown
  fields, and migration is not repeated on an already current file. Add feature-specific cases
  such as stable level/object IDs when those models are introduced.
- At each stable release, record its tag and the shipped schema versions in the ledger below.
  Freeze a fixture for each shipped format. Update this document with actual transitions and recovery implications. Note any limits
  or deliberate incompatibility in the PR and user-facing release notes.
- Run automated tests only in the final manual Prepare merge workflow, per `AGENTS.md`.

Regression examples live in `Assets/Game/Tests/Editor/Fixtures`. Never change fixtures for
published formats to fit new code. Unreleased format examples may evolve with their schema.

## Published format ledger

| Stable release | Settings | Gameplay saves | Editable levels |
| --- | --- | --- | --- |
| v0.2.0 | Menu settings: 0; display settings: not shipped | Not implemented | Not implemented |

Only add actual published tags. Before releasing a new format version, validate migrations
against fixtures from every previously published supported schema. Preview data is disposable
and isolated from release data; it creates no compatibility obligation.
