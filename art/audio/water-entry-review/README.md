# Water-entry sound review - 2026-11-22

## Owner decisions and selected clip

The owner requested a water-fall effect and retained the audition-before-integration gate. The first natural splash was initially approved, then that decision was explicitly revised to quieter/softer before integration. The heavily softened revision was rejected as too soft. The owner explicitly approved `water-entry-splash-quiet.wav`: retain a clear natural attack, lower level and only gently soften the sharpest upper range. This is the runtime selection; do not substitute `water-entry-splash-soft.wav`.

The three retained standalone files share the first approximately 2.086 seconds of the source: base `water-entry-splash.wav`; rejected softened `water-entry-splash-soft.wav`; selected `water-entry-splash-quiet.wav`. The selected version keeps the 4 ms edge fade and natural attack, applies an additional 6.5 kHz low-pass and halves gain after the base processing. The rejected soft study instead used a 4.2 kHz low-pass, faster transient compression and 25 ms entry fade.

## Source and rights

`jump-into-water.flac` by yurkobb, published 2015-07-06: https://freesound.org/people/yurkobb/sounds/280219/ . The creator describes a person jumping into a lake from a few meters, recorded with a Zoom H1 and normalized. CC0 1.0: https://creativecommons.org/publicdomain/zero/1.0/ . No field recording is claimed to be made by this project. Recording and derivatives retain CC0.

The unmodified public MP3 preview is retained as `sources/yurkobb-280219-hq.mp3`, downloaded from https://cdn.freesound.org/previews/280/280219_2023784-hq.mp3 . SHA-256: `237a169a18640dc034a30048d6419d9ae6111e56b64ae5381f1357f9baa65f0e`. The original FLAC is not claimed as retained.

Prepared 2026-11-22 using FFmpeg and original OpenAI Codex-authored `tools/prepare-water-entry-audition.py`, no generative audio model, added sample or synthesized splash. Preparation code follows the project PolyForm Noncommercial 1.0.0 license. Base processing: first 2.1 seconds requested, 70 Hz high-pass, 8 kHz low-pass, -22 LUFS/-3 dBTP target, 4 ms entry fade and 0.6-second fade from 1.5 seconds. The decoded output is about 2.086 seconds, stereo 44.1 kHz 16-bit PCM. Reproduce the selected variant with `python tools/prepare-water-entry-audition.py --quiet`.

## Gameplay integration

`Assets/Game/Resources/GoldenRiverAudio/water-entry.wav` is a byte-for-byte copy of the approved quiet variant. WaterRecovery owns one non-looping source at volume 0.8, playing once on entry; repeated checks do not retrigger it. Master volume applies. Pause freezes it immediately and resume retains position; retry, sequence cleanup, disable and menu return stop it. It is available for Golden River and the Movement Lab water recovery. No extra shark, bubble, movement or completion sound is selected. Source/license information remains in ASSETS.md and Settings > Sound credits.
