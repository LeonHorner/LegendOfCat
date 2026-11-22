# Varied forest-wind listening samples — 2026-11-22

## Feedback and status

The owner reports that the in-game wind sounds the same each time and the ending resembles a thunderstorm, with thunder in the background. The runtime currently alternates only two equal-length clips from the same earlier recording, which explains predictable repetition. Do not claim to have independently identified literal thunder: this is the owner's perceived sound defect.

These replacement proposals use a different tree/leaf-wind source, four distinct source sections, different lengths, rise/release shapes and strength. Two 260 Hz high-pass stages reduce low-frequency rumble. This does not prove that the result sounds free of storm associations; owner listening is required. All files stay outside Unity Assets/gameplay. The running preview and runtime bank are not silently replaced during this audition step.

River and checkpoint mix are unchanged. After approval, replace the old bank, choose without immediate repeats instead of fixed A/B alternation, and retain varied quiet intervals. Timing/strength are provisional and must be reviewed in-game. Do not integrate unapproved wind timbres under the earlier approval of a different source.

## Listening

`wind-variety-audition.wav` plays four different gusts once, with 1.2-second quiet gaps so each ending can be judged. This compressed comparison is not the proposed in-game frequency.

| Gust | Audition start | Duration | Source section | Rise / release | Relative level |
| --- | ---: | ---: | --- | --- | ---: |
| 01 | 0 s | 4.5 s | 8–12.5 s | 0.8 / 2.4 s | 1.00 |
| 02 | 5.7 s | 6.5 s | 38–44.5 s | 1.6 / 3.6 s | 0.85 |
| 03 | 13.4 s | 5.2 s | 77–82.2 s | 0.7 / 3.0 s | 1.08 |
| 04 | 19.8 s | 7.0 s | 116–123 s | 2.0 / 4.2 s | 0.90 |

Individual `forest-gust-01.wav` through `forest-gust-04.wav` are retained for selection. All are stereo 44.1 kHz 16-bit PCM.

## Source and rights

[Forest, close up of trees rustling in the wind.wav](https://freesound.org/people/Anya_Media/sounds/523389/) by **Anya_Media**, published 2020-06-22, [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/). The author describes nearby branches/leaves rustling in gusts and distant swaying treetops, recorded in Hexham, Northumberland using a stereo Audio Technica BP4025 and Zoom F8. Do not claim this project recorded the source or that the author guarantees no storm-like noise.

The unmodified public high-quality MP3 preview is retained in `sources/anya-media-forest-wind-523389-hq.mp3`, from https://cdn.freesound.org/previews/523/523389_2010973-hq.mp3. Source/derivatives retain CC0. The large original WAV is not required or claimed as retained.

Preparation: OpenAI Codex-authored `tools/prepare-varied-wind-auditions.py`, Python and FFmpeg, 2026-11-22. No generative audio model or synthesized wind. Each source cut gets two 260 Hz second-order high-pass stages, a 6 kHz low-pass and -26 LUFS normalization before its individual envelope/level; peaks capped at 0.45 full scale if necessary. Original preparation code follows the project PolyForm Noncommercial 1.0.0 license; the CC0 recordings and derivatives retain CC0. Reproduce from the repository root with `python tools/prepare-varied-wind-auditions.py`.
Source MP3 SHA-256: `80eb06d3a6b4d8e00f89d9cd20f4d0262b90714ca7bf4603238ec5e51d8e5b2c`.
