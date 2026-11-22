"""Remove the audition's opening effect; preserve the remaining PCM and its tail."""
from array import array
import argparse
import math
from pathlib import Path
import wave

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--soft-start", action="store_true", help="Use a 220 ms smooth entry instead of the original 20 ms fade.")
args = parser.parse_args()
folder = Path("art/audio/checkpoint-achievement-reference")
with wave.open(str(folder / "checkpoint-achievement-audition.wav"), "rb") as source:
    rate, channels = source.getframerate(), source.getnchannels()
    if source.getsampwidth() != 2:
        raise ValueError("Expected the retained 16-bit PCM audition.")
    source.setpos(round(.4 * rate))
    samples = array("h", source.readframes(source.getnframes() - source.tell()))

fade_frames = round((.22 if args.soft_start else .02) * rate)
for frame in range(min(fade_frames, len(samples) // channels)):
    gain = frame / (fade_frames - 1)
    if args.soft_start:
        gain = math.sin(gain * math.pi / 2) ** 2
    for channel in range(channels):
        index = frame * channels + channel
        samples[index] = round(samples[index] * gain)

name = "checkpoint-achievement-soft-start.wav" if args.soft_start else "checkpoint-achievement-clean-start.wav"
with wave.open(str(folder / name), "wb") as target:
    target.setparams((channels, 2, rate, 0, "NONE", "not compressed"))
    target.writeframes(samples.tobytes())
