# Golden River audio and completion

Issue #74 adds a reviewable first version of river ambience, restrained movement and checkpoint sounds, and a clear success state through the existing menu. Music is outside this initial pass. No campaign records or collectibles are saved.

## Implemented candidate

Golden River owns three audio sources: looping river ambience, a bounded movement voice, and a bounded checkpoint/success voice. The river becomes quieter at higher elevations. Pause preserves playback position; retry/restart clear transient voices and restart one ambience loop. Deactivating the level stops all sources immediately. Initial spawn and recovery respawns do not produce landing sounds. Master volume is adjustable in Settings with keyboard-accessible Quieter/Louder buttons; zero mutes all game sounds.

The existing grounded exit freezes Mika and the elapsed timer, plays a short completion chime and opens the existing gameplay menu with “Golden River complete”. Retry, Restart and Main menu remain available; Pause cannot resume movement behind success. Retry/restart rearm completion. The immutable LevelCompletion value and Completed event expose only session level ID, elapsed seconds and retry count for future consumers. No save/unlock or collection logic subscribes yet.

The original deterministic sound source is tools/generate-golden-river-audio.py. Run it from the repository root to recreate all five PCM files. Artwork, platforms and movement tuning retain their existing ownership.

## Local preview

Build with the pinned Editor using BuildGame.BuildPreview and -previewOutput followed by an absolute executable path. The normal menu starts the full Golden River course. For a focused success-menu review, launch the development player with --preview-view level-one-complete; it starts on the final bank and walks through the real exit. Existing --preview-capture can save a screenshot. Inspect both 960×540 and 640×360. This shortcut demonstrates presentation, not full-route traversal or human pacing.

For deterministic screenshot dimensions add --preview-width 960 --preview-height 540 (or 640 and 360) with --preview-capture and an absolute PNG path. These arguments exist only in development previews.

Menu schema 0 gains optional masterVolume with default 0.7; this compatible addition needs no version bump. See docs/data-migrations.md. Tests preserve unknown settings and protect invalid/future files.

## Coordination

This feature owns new level audio/completion code, its tests, generated sound sources and this document. MovementCourse receives scoped lifecycle hooks; StarterScreen.Presentation receives the completion menu integration. PR #79 owns cave geometry and PR #88 owns ranged combat. Both also touch shared entry points: integrate their changes sequentially and preserve their preview paths and asset credits. No scenes, prefabs, packages or project settings are edited. Each worktree owns its Unity Library.

## Review gates

- [ ] Review river ambience and jump, landing, checkpoint and success cues in actual gameplay audio.
- [ ] Review the success menu at 960×540 and 640×360, including keyboard navigation.
- [ ] Validate pause, retry, restart, replay and immediate audio cleanup on menu return.
- [ ] Reconcile current main and compatible feature work; run exact-head Windows/Linux validation after feedback.

This feature remains a draft until playable review and final validation. No merge or release is authorized.

## Executed development checks

Unity 6000.3.27f2 passed all 43 selected EditMode tests: GoldenRiverAtmosphereTests, MenuMovementIntegrationTests, MigrationTests and RunLifecycleTests. Coverage includes a complete real-physics main-route traversal, repeated exit signals, frozen success movement/timing, retry/restart, actual mixed audio output, paused sample position, resumed output, movement/checkpoint cues, immediate source cleanup and compatible volume persistence. Test reports are local artifacts, not release validation. Full exact-head Windows/Linux player/launcher/package validation remains pending player review.
