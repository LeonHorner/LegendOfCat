# Wind and magical checkpoint auditions

## Latest rejection and new success cue

The owner explicitly confirmed that the 0.6-second magical A/B cues were completely unsuitable. They are rejected, not approved pending routine integration. The requested replacement must sound magical and clearly communicate an accomplishment while staying short. River and wind approval remains intact; movement remains deferred.

`checkpoint-success-audition.wav` repeats one new 0.75-second cue at 0.2 and 1.9 seconds for listening. The separate `checkpoint-success-cue.wav` contains the single cue. This new direction uses a rapid consonant rising glass/harp-like gesture, a warm resolving voicing and a few fine sparks rather than the rejected muted modal taps. Its character and success readability are design intentions awaiting owner review, not established acceptance.

Prepared 2026-11-21 with `tools/prepare-checkpoint-success-audition.py`, original deterministic OpenAI Codex-authored synthesis, project PolyForm Noncommercial 1.0.0 license. No external samples, copied tune or generative audio model. No Unity import or gameplay references; obtain explicit listening approval before integration. All earlier checkpoint studies below are historical.

## Latest owner decision and shorter checkpoint

Wind is approved. The owner requested a shorter checkpoint sound to match the brief orange activation flash. `checkpoint-magic-short.wav` presents A at 0.3 seconds and B at 2.3 seconds, each lasting 0.6 seconds including its fade. They retain the warm/sparkling timbres but concentrate the bloom and sparks at activation, with compact reflections instead of a long tail. These short cues remain unapproved and outside Unity. The previous long magical comparison is historical.

Reproduce the short cues independently with `python tools/prepare-short-checkpoint-audition.py`. Original deterministic OpenAI Codex-authored synthesis, project PolyForm Noncommercial 1.0.0 license; no external samples or generative audio model. Prepared 2026-11-21 in response to the owner's duration feedback. Approval of the wind clip does not approve exact production gust intervals or final mix.

Standalone listening samples only. Neither file is imported into Unity or referenced by gameplay. Obtain explicit approval of the actual clips before integration.

The owner approved the previous natural river excerpt, deferred movement sounds and asked instead for occasional wind that fades gently. The checkpoint should support the orange illumination and sparks with a magical sound. Dry material taps are superseded.

## Listening guide

- `wind-gusts.wav`: 25 seconds. A gentle gust rises around second 1 and fades to silence by second 9. A quieter gust arrives around second 15 and fades by second 23. The long quiet gap illustrates intermittent ambience, not final scheduling. It is not tied to movement.
- `checkpoint-magic-comparison.wav`: A at 0.5 seconds is a warmer glowing bloom with a few fine sparks; B at 5.5 seconds is a lighter, more sparkling variation. Both have soft attacks and brief spatial tails rather than a stepped ascending melody. These descriptions identify synthesis intent, not owner acceptance.

## Sources, processing and license

Prepared 2026-11-21. Wind uses `park_ambience_wind.wav` from [Park ambiences](https://opengameart.org/content/park-ambiences), recorded by Thimras in a public park in Adelaide and published 2022-07-25 under [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/). Its recording and derivatives retain CC0. Source: https://opengameart.org/sites/default/files/park_ambience_wind.wav.

Audition processing: excerpts at seconds 25–33 and 48–56, 100 Hz high-pass, 4.2 kHz low-pass, -27 LUFS target before the gust envelope, 2.3-second soft rise and 4.5-second retreat; second gust is 20% quieter. PCM stereo at 44.1 kHz. The excerpt windows and pauses are comparison choices, not final level timing. No field recording is claimed authored by this project.

The magical checkpoint studies are original deterministic synthesis authored through OpenAI Codex, covered by the project PolyForm Noncommercial 1.0.0 license. Simultaneous warm harmonic blooms, irregular high partials, restrained stereo placement and short cross-channel echoes express the owner's orange-glow/spark direction. No sampled third-party instruments, melody or generative audio model is used.

Reproduce with `python tools/prepare-golden-river-wind-and-magic.py` after downloading the wind source into ignored `artifacts/audio-sources/park_ambience_wind.wav`. The script writes only this review folder, never Assets. Sources, generator, date, license and decisions are also recorded in ASSETS.md.

Source WAV SHA-256: `5c381856745b4706e7eba55eb9271a61a530e90c05df8126adb2db89ecfa6c5a`.
