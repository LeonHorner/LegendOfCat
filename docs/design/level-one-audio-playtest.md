# Golden River in-game sound review — 2026-11-22

The owner explicitly authorized integrating the latest soft-entry checkpoint cue, selected river and wind for an in-game feel test. They asked for water quiet in the background and wind becoming noticeable occasionally. After hearing the 17-second continuous-air study, the owner requested a shorter loud section and several irregular in-game variations. This authorizes the same-timbre bank revision. This supersedes the earlier standalone-only gate for these specific clips; it does not establish final in-game acceptance or authorize merging/releasing.

The owner approved `checkpoint-achievement-long-shimmer.wav` after the bell-layer removal and extended shiny entrance, explicitly requesting gameplay integration.

The owner requested Escape to toggle pause/resume, a slider for master volume, and an entry sound when Mika falls into water. After rejecting an over-softened splash, they approved the quieter version with a clear natural attack.

## Current candidate

- River: the selected Thimras natural-water audition, with its demonstration edge fades omitted and a 0.6-second equal-power overlap joining the loop. One 15.4-second loop; source volume 0.14 near the valley, decreasing smoothly to 0.06 high above it.
- Wind: four original continuous-air variations of the owner-selected 17-second study, now 11.5, 12.5, 13.5 and 14 seconds. The release starts during the shorter rise and uses a fourth-power cosine for a shorter loud crest with a long soft tail. Source volume 0.4, reduced from 0.75 after the owner found the wind too loud (about 5.5 dB less); per-clip peaks 0.272–0.32 before this mix gain. First gust after four active seconds, then random 10–24-second quiet intervals after complete gusts. Select randomly while excluding the previous clip; never overlap or tie wind to movement. Timing remains provisional for in-game review.
- Checkpoint: exact selected `checkpoint-achievement-long-shimmer.wav`, copied byte-for-byte into the runtime checkpoint asset. Source volume 0.7, once per newly reached checkpoint; no replay when lingering or retrying an already active checkpoint. Retain the warm low body, omitted middle bell layer and extended early high shimmer; total length 2.582 seconds. The source's later high ringing remains excluded.
- Water entry: approved `water-entry-splash-quiet.wav`, copied byte-for-byte into `water-entry.wav`. A separate WaterRecovery-owned, non-looping source at 0.8 plays once at immersion. Repeat checks cannot retrigger it; pause/focus loss preserves position, explicit resume continues, and retry/restart/menu/disable stop it. Golden River uses the entry sound; the retained Editor movement fixture can exercise the same recovery.
- No movement, jump, landing, shark or separate completion sounds are selected. Completion retains its existing success state and stops level audio.

Losing window focus or application suspension opens the existing pause menu for Golden River, ranged and stomp prototypes. The Movement Lab is retired: no player menu/registry entry or Resources prefab; its original GUID/prefab remains only in Editor test fixtures. Duplicate focus/pause notifications do not toggle back to play. Returning focus leaves the game paused until explicit Resume/P; completed runs stay completed and idle menus are unaffected. The same course pause path handles focus and manual pause. Golden River audio and moving-platform physics pause immediately in the callback, without waiting for another frame. Water, submerged ripples, mill/willow motion and recovery effects use pause-aware course/effect clocks; camera following and Mika presentation freeze too. Resume continues platform phases and scene clocks; retry re-enables/reset platforms. Menus stay responsive without changing global time scale.

Master volume is a horizontal 0–100% slider with one-percent steps and immediate mixer feedback. Tab navigation includes the slider; its native left/right keyboard adjustment remains available. Preference writes wait for 0.25 seconds of inactivity and flush on focus loss, suspension or quit, using the existing schemaVersion 0 masterVolume field. Unknown data/future-file protections remain unchanged.

Master volume from Settings applies to all sources. Pause freezes playback positions and wind scheduling; resume continues them. Retry/restart stop pending checkpoint/gust voices and start one fresh river loop, with the first wind delay reset. Menu return/deactivation stops all voices immediately. Settings → Sound credits provides the required CC BY attribution, source and license links plus the river/wind credits.

## Play the focused Windows preview

Build with the pinned Editor's `GoldenRiverAudioImport.BuildPreview` method and `-previewOutput <absolute-executable-path>`. It imports the selected audio settings and uses the existing Windows development build. The normal menu still starts the complete level.

For a quick sound check, launch with:

```text
-force-d3d11 -screen-fullscreen 0 --preview-view level-one-audio --preview-width 1280 --preview-height 720
```

This starts on safe ground just left of checkpoint 1 with checkpoint 1 still inactive. Walk right to activate its orange glow/sparks and sound. Stand still for passing wind, use Esc or P to open/close pause, R to retry and Backspace for a full restart. Choose Main menu while paused to return; Esc no longer exits gameplay. Controller bindings remain unchanged. Check the water/wind balance at your normal listening volume; sound settings remain available through the menu. This focused start is for review, not evidence of first-player timing or a full-level playtest.

## Preparation, package and integration boundaries

`python tools/prepare-level-one-audio.py` recreates seven runtime clips from the retained river/checkpoint/water-entry auditions and deterministic wind synthesis in `tools/prepare-runtime-wind.py`. Unity `GoldenRiverAudioImport.Prepare` preserves stereo and audition levels, preloads clips, uses Vorbis for ambient recordings and PCM for the checkpoint and water entry. Build validation checks all seven selected clips are packed and excludes rejected Editor fixtures/review-only source files.

River is a CC0 1.0 recording by Thimras. Wind is original OpenAI Codex-authored deterministic filtered-noise synthesis using Python/FFmpeg, under the project PolyForm Noncommercial 1.0.0 license; no generative audio model or third-party samples. Water entry is a CC0 1.0 lake-jump recording by yurkobb, adapted for level/length and documented in `art/audio/water-entry-review/README.md`. Checkpoint is an adaptation of TommasoMotteran's Fantasy Achievement Unlock - Magical Stinger, CC BY 4.0. Source URLs, checksums, transformations and attribution are retained in ASSETS.md and the source audition READMEs. No Pixabay audio is used.

Current main `aabd3e5` (merged ranged PR #88) is normally merged into this feature. Shared menu conflicts preserve ranged controls, scrolling and its result states as well as Golden River completion/settings. Audio owns the new runtime component, assets/import/audit helpers, scoped course/preview hooks and credits. PR #79 still owns cave geometry; no cave branch, terrain, scene, prefab, package or persistent format is changed. Its later course/build integration must preserve these hooks and credits.

## Remaining owner review

Earlier audio/slider revision validation (before menu retirement): all 120 selected Unity 6000.3.27f2 EditMode cases passed across GoldenRiverAtmosphereTests, MenuMovementIntegrationTests, MigrationTests, RunLifecycleTests, RangedCombatTests, SceneryRendererTests, WaterRecoveryTests and DesktopBehaviorTests. Coverage includes real splash mixer output and sample-position pause/resume, once-per-entry playback, retry/menu cleanup, Esc toggling without menu exit in each prototype, immediate slider/mixer feedback and persisted unknown settings, actual rendered-world freeze, all platform phases/positions and the existing movement/ranged/exit regressions. The later cosmetic slider-rail alignment was compiled and inspected in the final Windows preview; no additional behavior change.

The final slider/water-entry Windows development player build and package audits passed. All seven selected audio clips, 44 active paintings and three shader-reference materials are packed; rejected sounds and review-only source files are excluded. The Editor-generated audio metadata is retained. No final Linux player/launcher or exact-head merge-ready validation is claimed for this in-game review candidate.

- Check the softened checkpoint entrance against actual stone contact, glow and sparks.
- Review river/background quietness and the strength/frequency of passing gusts while traversing and standing still.
- Switch to another application during a platform ride, movement, wind and checkpoint playback: confirm frozen platform positions/phases, camera, Mika, water, foliage, timer/audio positions and explicit resume after returning. Compare against manual P pause; neither path may advance the world. Also check minimize, repeated focus changes and an already paused or completed run.
- Review pause/retry/restart/menu cleanup and the completion menu after the ranged-menu integration.
- Keep PR #89 draft during this review. Final exact-head full Unity/Windows/Linux player/launcher/package and merge-ready validation follows acceptance.

Current local preview: `artifacts/water-slider-preview/PlayAudioReview.cmd`. Runtime wind has about 3–4 one-second windows above 70% of its loudest RMS window, compared with seven in the original 17-second study; this is an envelope check, not subjective listening acceptance.

Focused platform/focus start: `artifacts/water-slider-preview/PlayPauseReview.cmd` starts at the ascent lift.

Focused water-entry start: `artifacts/water-slider-preview/PlayWaterReview.cmd` uses `--preview-view level-one-water`, placing Mika over the first river gap for a single immersion/recovery. If focus loss paused startup, press Esc to continue. The normal retry still returns to the course start. No native desktop input automation was used for Esc/slider review after the owner stopped Computer Use; keyboard bindings are covered through Unity input tests, and settings images come from the player's built-in capture path.

Settings slider views inspected in the actual D3D11 Windows player: [640x360](level-one-audio-review/volume-slider-640.png) and [960x540](level-one-audio-review/volume-slider-960.png). The thumb is centered on the rail; the existing scroll view keeps all settings reachable at small sizes. Both capture logs contain no runtime exceptions/errors.

Current navigation: Esc returns from title settings to Main menu, and from Sound credits/Versions & updates to Settings. An update notice dismisses first without canceling downloads. Esc at Main menu does not quit. In a paused round, Settings exposes volume/fullscreen/progress (progress only for the course); Esc first returns to Pause, then resumes on a subsequent press. These controls preserve the current run and use the same stored preferences. The quieter wind source volume is now 0.4. Navigation/removal checks and the updated Windows player build passed.

Development validation for the navigation/retirement revision: the 109-case expanded run passed 108 cases; the remaining menu cleanup test still assumed the retired 14-platform lab. That obsolete count assertion was removed while retaining actual decorative-collider/resource ownership checks, and all 12 focused atmosphere/navigation/real-level menu cases then passed. The archive GUID is unchanged. The existing broader run covers LevelProgress/Mika editor fixtures and desktop/combat/scene behavior. The Windows player build passed, including a report audit proving the retired movement prefab is not packed. Actual round-settings views were inspected at [640x360](level-one-audio-review/round-settings-640.png) and [960x540](level-one-audio-review/round-settings-960.png), plus the [main menu without Movement Lab](level-one-audio-review/menu-without-lab.png); no runtime errors/exceptions in those captures.

Latest local player: `artifacts/menu-settings-preview/PlayRoundSettings.cmd` opens the paused round settings directly. Existing audio/water/lift entry helpers are retained in that folder. The wind mix is 0.4 in this revision.
