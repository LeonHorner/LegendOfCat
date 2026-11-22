# Long receding wind study - 2026-11-22

The owner rejected all four forest gusts as sounding like rain and requested longer wind gestures with a long release. Those forest studies are rejected, not selected replacements. Earlier source approval does not apply to this new study.

`long-receding-wind.wav` is a single continuous 17-second study: approximately 3.8 seconds of rise followed by a 12.8-second retreat beginning at 4.2 seconds. Slowly changing continuous filtered noise aims for an airy passing gust instead of granular tree/leaf patter. No rain, leaf, thunder, field-recording or third-party sample is used. Subjective wind character still requires owner listening; technical generation does not establish acceptance.

Source/generator: original deterministic synthesis in OpenAI Codex-authored `tools/prepare-long-wind-audition.py`, using Python and FFmpeg pink-noise sources; prepared 2026-11-22, no generative audio model. Three filtered bands, slow changes of air/body balance, gentle stereo drift and a continuous sine-squared envelope. Stereo 44.1 kHz 16-bit PCM, peak 0.32 full scale. Original code and generated sound follow the repository PolyForm Noncommercial 1.0.0 license.

Human direction: longer receding wind, avoid rain-like patter and storm rumble, audition before gameplay integration. Reproduce with `python tools/prepare-long-wind-audition.py` from the repository root. This file remains outside Unity Assets. The running preview, river, checkpoint and runtime wind are unchanged; PR #89 remains draft. Do not interpret this standalone study as a runtime repetition/scheduling fix.
