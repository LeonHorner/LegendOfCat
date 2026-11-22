# Golden River in-game sound review — 2026-11-22

The owner explicitly authorized integrating the latest soft-entry checkpoint cue, selected river and wind for an in-game feel test. They asked for water quiet in the background and wind becoming noticeable occasionally. This supersedes the earlier standalone-only gate for these specific clips; it does not establish final in-game acceptance or authorize merging/releasing.

## Current candidate

- River: the selected Thimras natural-water audition, with its demonstration edge fades omitted and a 0.6-second equal-power overlap joining the loop. One 15.4-second loop; source volume 0.14 near the valley, decreasing smoothly to 0.06 high above it.
- Wind: the two selected eight-second gusts, including their existing soft rises/fades. Source volume 0.75. First gust after four active seconds, then a randomly varied 8–16-second quiet interval after each complete gust. Alternate the two recordings; never layer gusts or tie them to movement. These are initial review values, not fixed campaign rules.
- Checkpoint: exact selected `checkpoint-achievement-soft-start.wav`, copied byte-for-byte into the runtime checkpoint asset. Source volume 0.7, once per newly reached checkpoint; no replay when lingering or retrying an already active checkpoint. Retain the 220 ms sine-squared entry and existing tail.
- No movement, jump, landing or separate completion sounds are selected. Completion retains its existing success state and stops level audio.

Master volume from Settings applies to all sources. Pause freezes playback positions and wind scheduling; resume continues them. Retry/restart stop pending checkpoint/gust voices and start one fresh river loop, with the first wind delay reset. Menu return/deactivation stops all voices immediately. Settings → Sound credits provides the required CC BY attribution, source and license links plus the river/wind credits.

## Play the focused Windows preview

Build with the pinned Editor's `GoldenRiverAudioImport.BuildPreview` method and `-previewOutput <absolute-executable-path>`. It imports the selected audio settings and uses the existing Windows development build. The normal menu still starts the complete level.

For a quick sound check, launch with:

```text
-force-d3d11 -screen-fullscreen 0 --preview-view level-one-audio --preview-width 1280 --preview-height 720
```

This starts on safe ground just left of checkpoint 1 with checkpoint 1 still inactive. Walk right to activate its orange glow/sparks and sound. Stand still for passing wind, use P to pause/resume, R to retry, Backspace for a full restart and Esc to return to menu. Check the water/wind balance at your normal listening volume; sound settings remain available through the menu. This focused start is for review, not evidence of first-player timing or a full-level playtest.

## Preparation, package and integration boundaries

`python tools/prepare-level-one-audio.py` recreates the four runtime clips from the committed selected auditions. Unity `GoldenRiverAudioImport.Prepare` preserves stereo and audition levels, preloads clips, uses Vorbis for ambient recordings and PCM for the checkpoint. Build validation checks all four selected clips are packed and excludes rejected Editor fixtures/review-only source files.

River/wind are CC0 1.0 recordings by Thimras. Checkpoint is an adaptation of TommasoMotteran's Fantasy Achievement Unlock - Magical Stinger, CC BY 4.0. Source URLs, checksums, transformations and attribution are retained in ASSETS.md and the source audition READMEs. No Pixabay audio is used.

Current main `aabd3e5` (merged ranged PR #88) is normally merged into this feature. Shared menu conflicts preserve ranged controls, scrolling and its result states as well as Golden River completion/settings. Audio owns the new runtime component, assets/import/audit helpers, scoped course/preview hooks and credits. PR #79 still owns cave geometry; no cave branch, terrain, scene, prefab, package or persistent format is changed. Its later course/build integration must preserve these hooks and credits.

## Remaining owner review

Development validation: Unity 6000.3.27f2 passed all 54 selected EditMode tests in GoldenRiverAtmosphereTests, MenuMovementIntegrationTests, MigrationTests, RunLifecycleTests and RangedCombatTests. This includes actual mixed river/wind output, pause/resume sample positions and frozen wind timing, separate quiet intervals, alternating non-overlapping gusts, checkpoint/exit/retry/restart cleanup, the full physical main route and ranged/menu regressions. Technical success does not establish mix acceptance or final cross-platform validation.

The Windows development player build and package audits passed. All four selected audio clips, 44 active paintings and three shader-reference materials are packed; rejected sounds and review-only source files are excluded. The Editor-generated audio metadata is retained. No final Linux player/launcher or exact-head merge-ready validation is claimed for this in-game review candidate.

- Check the softened checkpoint entrance against actual stone contact, glow and sparks.
- Review river/background quietness and the strength/frequency of passing gusts while traversing and standing still.
- Review pause/retry/restart/menu cleanup and the completion menu after the ranged-menu integration.
- Keep PR #89 draft during this review. Final exact-head full Unity/Windows/Linux player/launcher/package and merge-ready validation follows acceptance.
