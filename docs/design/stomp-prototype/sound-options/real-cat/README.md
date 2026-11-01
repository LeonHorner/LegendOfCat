# Real-cat damage reaction candidates

**Attribution:** Cat Angry Meow.WAV by **softcoresoft**, [Freesound source](https://freesound.org/people/softcoresoft/sounds/327438/), published 2015-11-02, licensed under [Creative Commons Attribution 4.0 International](https://creativecommons.org/licenses/by/4.0/). The author describes an actual cat hissing and meowing, then hitting the recorder. No AI generator/model is involved. This credit applies to the retained public HQ source preview and both edited candidates.

The user rejected the prior 420 ms performed human-voice candidate as too short and insufficiently cat-like. On 2026-10-31 the user selected short-reaction.wav (0.9 seconds) for gameplay. natural-reaction.wav remains a comparison preview. The selected file is copied unchanged to Assets/Game/Resources/StompDamageVoice.wav. The source description establishes that this is a real cat, not that the edited sound expresses surprise; subjective fit remains for the user.

- natural-reaction.wav: source seconds 0.9-2.1, 1.2 seconds, unchanged speed and pitch.
- short-reaction.wav: same excerpt at 4/3 tempo, 0.9 seconds, pitch preserved; the phrase is accelerated rather than cut earlier.
- Both: mono PCM16, 48 kHz, 5 ms onset fade and 60 ms release fade, gain 0.65. No synthetic vocal layer.

Prepared with FFmpeg via `python tools/prepare-real-cat-preview.py` on 2026-10-31. Source is the public MP3 preview, not the login-gated original WAV. provenance.json records the download URL and file hashes. CC BY 4.0 requires retaining creator attribution, a license link and modification disclosure on further redistribution. Runtime integration retains these credits in Assets/StreamingAssets/THIRD-PARTY-NOTICES.txt, which is included in player builds.
