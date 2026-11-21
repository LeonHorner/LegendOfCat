# Wind and magical checkpoint auditions

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
