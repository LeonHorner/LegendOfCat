"""Remove the audition's opening effect; preserve the remaining PCM and its tail."""
from array import array
from pathlib import Path
import wave

folder = Path("art/audio/checkpoint-achievement-reference")
with wave.open(str(folder / "checkpoint-achievement-audition.wav"), "rb") as source:
    rate, channels = source.getframerate(), source.getnchannels()
    if source.getsampwidth() != 2:
        raise ValueError("Expected the retained 16-bit PCM audition.")
    source.setpos(round(.4 * rate))
    samples = array("h", source.readframes(source.getnframes() - source.tell()))

fade_frames = round(.02 * rate)
for frame in range(min(fade_frames, len(samples) // channels)):
    gain = frame / (fade_frames - 1)
    for channel in range(channels):
        index = frame * channels + channel
        samples[index] = round(samples[index] * gain)

with wave.open(str(folder / "checkpoint-achievement-clean-start.wav"), "wb") as target:
    target.setparams((channels, 2, rate, 0, "NONE", "not compressed"))
    target.writeframes(samples.tobytes())
