# Standalone sound auditions and owner decisions

## Latest owner feedback — 2026-11-21

`river-natural.wav` is approved as the river sound. Final loop and in-game mix still require review. Movement sounds are deferred and may be omitted. The dry checkpoint A/B study is superseded: the owner requested a magical sound matching the orange glow and sparks. The new wind/magic auditions live in `art/audio/wind-and-magic-review` and remain outside Unity until heard and explicitly approved. Historical descriptions below do not override these decisions.

The owner rejected the original #74 sound candidate on 2026-11-21 and requires listening before integration. These files are outside Assets, are not referenced by gameplay and must not enter Unity or a player package before explicit clip approval. The original sounds are now Editor-only rejected fixtures. The completion menu remains usable without them.

## Listening order

- `river-natural.wav`: 18-second excerpt of a real park river recording, with naturally present wildlife. This is a candidate texture, not a finished seamless level loop.
- `movement-comparison.wav`: A, soft-ground contacts from 0–3 seconds; B, stone contacts from 4–7 seconds. Both adapt real human-footstep recordings, shortened and softened for a small character. These are not recordings of actual cat paws. The sample cadence is for comparison, not approved animation timing or a decision to add constant running audio.
- `checkpoint-comparison.wav`: A, short stone-like resonance at 0.35/1.8 seconds; B, dry wood-like taps at 4/5.45 seconds. Both are original synthesis studies without an ascending melody.

Ask the owner to accept, reject or describe changes for each category. Only selected, explicitly approved clips may be integrated; cadence, variation and in-game mix need a subsequent review. Silence remains an option for movement or checkpoint interactions.

## Provenance and reproduction

Prepared 2026-11-21 using `tools/prepare-golden-river-auditions.py`, Python standard-library processing and FFmpeg. No generative audio model was used. Original synthetic checkpoint studies are authored through OpenAI Codex and covered by the project PolyForm Noncommercial 1.0.0 license. Third-party field recordings and their derivatives retain CC0 1.0.

| Category | Source and author | License | Adaptation |
| --- | --- | --- | --- |
| River | [Park ambiences](https://opengameart.org/content/park-ambiences), Thimras, 2022-07-25; `park_ambience_river.wav` | [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/) | Seconds 120–138; 40 Hz high-pass, -24 LUFS audition level, brief edge fades, 44.1 kHz PCM stereo. |
| Movement | [Fantozzi's Footsteps (Grass/Sand & Stone)](https://opengameart.org/content/fantozzis-footsteps-grasssand-stone), Fantozzi; `Fantozzi-footsteps.7z` | [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/) | Sand/Stone L1–3 and R1–3 FLAC contacts; 220 Hz high-pass, 5.5 kHz low-pass, up to 0.2-second contacts, soft edges and lowered levels; ten varied contacts per comparison section. |
| Checkpoint | Original modal-impact synthesis in the preparation script | Project license | Inharmonic damped resonances and short noise excitation; two timbres, each repeated twice for audition. |

Creative response to owner feedback: replace the synthetic water hiss with a field recording; compare quiet material contacts rather than pitched movement cues; replace the checkpoint melody with short material-like accents. None is claimed accepted or convincing until heard by the owner.

Download source files into ignored `artifacts/audio-sources`:

- [River WAV](https://opengameart.org/sites/default/files/park_ambience_river.wav), SHA-256 `da0c0c9b1249a83a1611cc116d5737b0864a8db9f45f1ae8dcc079d0f7c0bbbf`.
- [Footstep archive](https://opengameart.org/sites/default/files/Fantozzi-footsteps.7z), SHA-256 `415d360e0911c1c1c5355ee94023b772e6f4044f59b9b580610e7dbffd1840fd`. Extract to `artifacts/audio-sources/footsteps`.

Then run `python tools/prepare-golden-river-auditions.py` from the repository root. The large original river file stays an ignored download; these standalone excerpts and the exact transform script are committed for review. No Assets import or game installation is part of preparation.
