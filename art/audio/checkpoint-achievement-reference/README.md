# Achievement reference and alternate listening sample — 2026-11-22

## Latest revision: gentler entry

The owner still finds the opening too abrupt. `checkpoint-achievement-soft-start.wav` keeps the same 0.4-second opening-effect removal but replaces the 20 ms linear entry with a 220 ms sine-squared rise. Main sound and existing ending remain byte-identical to the retained audition after this fade; duration remains about 2.58 seconds. Reproduce with `python tools/prepare-achievement-clean-start.py --soft-start`. This needs no new installed tools and remains a standalone proposal outside Unity, pending explicit listening approval.

The source and attribution remain TommasoMotteran's **Fantasy Achievement Unlock - Magical Stinger**, **CC BY 4.0**, with links and required credit below. Additional changes for this version: opening-effect removal and a longer smooth entry fade. No new sampled or generated sound is introduced.

## Current listening revision: remove the opening effect

The owner said this direction is better but identified the sound at the beginning as the problem. Keep the main sound and existing tail, and change only the entrance. `checkpoint-achievement-clean-start.wav` removes the first 0.4 seconds of the retained audition and applies a 20 ms entry fade. Its remaining duration is about 2.58 seconds. All subsequent samples, including the existing final fade, retain the exact original audition PCM. Reproduce with `python tools/prepare-achievement-clean-start.py` after preparing the base audition.

The new file remains a listening proposal outside Unity Assets/gameplay, not approved for integration. The source is still **Fantasy Achievement Unlock - Magical Stinger by TommasoMotteran**, under **CC BY 4.0**, with source/license links and required attribution below. Additional changes for this version: opening-effect removal and a 20 ms entry fade. No new sound, synthesis, reverb, gain adjustment or external material is layered over it. Direction approval does not approve this rendered revision.

The owner supplied [Achievement Unlock by Universfield on Pixabay](https://pixabay.com/sound-effects/film-special-effects-achievement-unlock-243762/) and asked for this direction instead of the previous crystal-only experiment. Its page describes a triumphant orchestral achievement fanfare. This is the current concrete style reference, superseding interpretations based only on the earlier timbre questionnaire. The actual Pixabay audio was not acquired or analyzed: direct page access encountered a browser challenge. Do not claim an aural or waveform comparison to that recording.

The Pixabay reference remains a link only. Its [Content License](https://pixabay.com/service/terms/) prohibits distributing essentially unchanged content as a standalone audio file; this repository requires redistributable source assets. No Pixabay recording, transcription, reconstructed melody or altered copy is committed here.

## New alternate audition

`checkpoint-achievement-audition.wav` is a single continuous three-second listening excerpt adapted from **Fantasy Achievement Unlock - Magical Stinger** by **TommasoMotteran**, published 2026-06-16 on [Freesound](https://freesound.org/people/TommasoMotteran/sounds/859080/), licensed [Creative Commons Attribution 4.0 International](https://creativecommons.org/licenses/by/4.0/). The author describes a rewarding fantasy achievement stinger with magical chimes, made with sound-design tools and royalty-free sounds without AI. It is a separate creator's work, not this project's synthesized recreation of Universfield's sound. Its suitability still needs owner listening review.

Changes by this project: remove initial silence, adjust audition loudness to -21 LUFS with -3 dBTP target, trim to three seconds, add a 4 ms edge fade and a soft fade over the final 0.55 seconds, export 44.1 kHz 16-bit stereo PCM. Prepared 2026-11-22 using FFmpeg and the OpenAI Codex-authored `tools/prepare-achievement-reference-audition.py`. Source and adapted sound retain CC BY 4.0; the preparation script uses the project license. No additional generative audio model or synthesis was used for this adaptation.

Required attribution for this audition and any future use:

> “Fantasy Achievement Unlock - Magical Stinger” by TommasoMotteran, https://freesound.org/people/TommasoMotteran/sounds/859080/, licensed under CC BY 4.0, https://creativecommons.org/licenses/by/4.0/. Adapted for audition: lead-in removal, loudness adjustment, three-second excerpt and edge fades. No endorsement implied.

The unmodified publicly available MP3 preview is retained in `sources/tommasomotteran-859080-hq.mp3`. Public source URL: https://cdn.freesound.org/previews/859/859080_18681946-hq.mp3. Reproduce from the repository root with `python tools/prepare-achievement-reference-audition.py` and FFmpeg on PATH.

Retained source SHA-256: `689d41efd0585c2e034436d64c1ef00c655c2d2a92eec0103574ff9b1c5ed2e9`.

This folder stays outside Unity Assets and gameplay. The reference selection is not approval of this alternate clip. Obtain explicit listening approval before integration; retain attribution in ASSETS.md and eventual player credits if selected. River/wind selections remain approved, movement remains deferred, and no checkpoint sound is approved yet.
