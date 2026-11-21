# Golden River audio and completion

## Latest checkpoint rejection — 2026-11-21

The owner clarified “absolutely unsuitable” and rejected the short 0.6-second checkpoint variants. The replacement must be magical and clearly communicate that the player accomplished something. The new standalone `art/audio/wind-and-magic-review/checkpoint-success-audition.wav` presents one 0.75-second success gesture twice. It awaits listening approval and is not integrated. River and wind remain approved; movement sounds remain deferred. Earlier notes describing the short variants as awaiting selection are superseded by this rejection.

## Owner selection and new sound direction — 2026-11-21

Follow-up: the owner approved the wind audition. The magical checkpoint must be shorter to match the brief orange activation flash. New 0.6-second A/B cues in `art/audio/wind-and-magic-review/checkpoint-magic-short.wav` await listening approval. River and wind are selected; movement remains deferred. The earlier long magical cues are superseded. No new audio has been integrated during this audition revision.

- The owner approved the standalone `art/audio/auditions-2026-11-21/river-natural.wav` excerpt as the river sound. Preserve that selection; final looping and in-game mix remain separate review steps.
- Movement sounds may be unnecessary. Leave footsteps and the first candidate's jump/landing noises out for now; do not treat the earlier contact studies as selected.
- Instead, audition occasional wind gusts that arrive gently and fade away. Wind is environmental, independent of Mika's movement, not a continuous rushing loop. Exact production spacing and mix have not been locked.
- Checkpoint sound should be magical and match the orange illumination and sparks. The dry stone/wood audition direction is superseded. Present warm glowing and sparkling alternatives as standalone audio before integration.

Current new auditions: `art/audio/wind-and-magic-review`. Wind and magical checkpoint clips remain unapproved and outside Unity Assets. River approval does not waive the listening gate for the other categories.

## Owner rejection and audition gate — 2026-11-21

The owner rejected the first water texture, movement sounds and checkpoint cue as unconvincing or unsuitable. The original sound candidate is not accepted. GoldenRiverAudio is no longer attached to the playable level; all five original clips have moved out of Resources into Editor-only rejected regression fixtures with their metadata preserved. Completion/menu functionality remains available without the candidate sounds.

Before any further audio integration, present standalone listening samples outside Assets and obtain explicit owner approval of the actual clips. Do not substitute mixer tests, a running prototype or a new procedural pass for this listening gate. After clip approval, review levels, repetition, contact timing and pause/retry behavior in-game separately. Approval of an isolated clip does not establish final mix acceptance. Do not add constant footstep playback until the owner has heard and selected its sound and cadence.

The implementation and executed checks below describe the first candidate and technical infrastructure. They do not establish sound approval. New auditions belong under art/audio, outside Unity import and player packaging.

Issue #74 adds a reviewable first version of river ambience, restrained movement and checkpoint sounds, and a clear success state through the existing menu. Music is outside this initial pass. No campaign records or collectibles are saved.

## First candidate (audio rejected)

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
