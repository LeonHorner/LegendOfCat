# Checkpoint choir and crystal audition — 2026-11-22

Superseded: the owner corrected answer 2 to B only (crystals and soft bells), removing choir from the desired timbre. This candidate is historical and not approved for integration. The replacement is in `art/audio/checkpoint-crystal-review`.

## Owner-selected direction

After rejecting the previous fanfare-like direction, the owner selected:

- Feeling: success/reward **and** the orange stone awakening with magic.
- Timbre: a floating wordless choir **and** flowing crystal/soft-bell colors.
- Shape: an immediate magical burst followed by a soft decay.
- Duration: one continuous 2–3-second sound, without a pause or repeated audition blocks.

`checkpoint-choir-crystal.wav` is one 2.7-second proposal for that direction. A harmonized wordless choral arrival carries the body; crystal accents coincide with activation, followed by fine sparks and a continuous fading space. This is a listening candidate, not an approved clip or a claim that its intention is achieved. It stays outside Unity Assets and gameplay. River and wind approvals remain; movement remains deferred.

## Sources and license

Choral source: [Short Choir by Breviceps](https://freesound.org/people/Breviceps/sounds/444491/), published 2018-10-24, explicitly dedicated by the author to [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/). The provider describes a short wordless choir for an achievement/holy moment; it does not establish that this project recorded a real choir. This audition uses the publicly available high-quality MP3 preview, retained unmodified in `sources/breviceps-short-choir-444491-hq.mp3`. Source URL: https://cdn.freesound.org/previews/444/444491_9159316-hq.mp3. Source SHA-256: `6ee2b2b3db396c81ed6544f254b8470c6293e1d0eb349e7f896f1aa2ad8e5f83`. The source and its adapted choral material retain CC0.

Adaptation and original crystal design: OpenAI Codex-authored `tools/prepare-choir-checkpoint-audition.py`, Python standard library and FFmpeg, prepared 2026-11-22. No generative audio model or copied tune. The preparation script/original synthesis follow the project PolyForm Noncommercial 1.0.0 license; third-party CC0 rights remain intact.

Processing: remove source lead-in, high-pass at 130 Hz/low-pass at 7.8 kHz, harmonize the source around its approximately B4 pitch with octave/third/fifth voices using rate conversion plus tempo correction and a 0.8-tempo stretch, align them within the first 21 ms, shape a single decay, add original pitched crystal modes and fine sparks, then short stereo reflections and a smooth ending. Peak target is 0.38 full scale. PCM stereo, 44.1 kHz, 16 bit, exactly 2.7 seconds. Reproduce from the repository root with `python tools/prepare-choir-checkpoint-audition.py` and FFmpeg on PATH.

Explicit listening approval is required before integration. Direction selection does not approve this rendered file, final in-game mix, wind intervals or any completion sound.
