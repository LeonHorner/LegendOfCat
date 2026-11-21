# Golden River audio and completion

Issue #74 adds a reviewable first version of river ambience, restrained movement and checkpoint sounds, and a clear success state through the existing menu. Music is outside this initial pass. No campaign records or collectibles are saved.

## Coordination

This feature owns new level audio/completion code, its tests, generated sound sources and this document. MovementCourse receives scoped lifecycle hooks; StarterScreen.Presentation receives the completion menu integration. PR #79 owns cave geometry and PR #88 owns ranged combat. Both also touch shared entry points: integrate their changes sequentially and preserve their preview paths and asset credits. No scenes, prefabs, packages or project settings are edited. Each worktree owns its Unity Library.

## Review gates

- [ ] Review river ambience and jump, landing, checkpoint and success cues in actual gameplay audio.
- [ ] Review the success menu at 960×540 and 640×360, including keyboard navigation.
- [ ] Validate pause, retry, restart, replay and immediate audio cleanup on menu return.
- [ ] Reconcile current main and compatible feature work; run exact-head Windows/Linux validation after feedback.

This feature remains a draft until playable review and final validation. No merge or release is authorized.
