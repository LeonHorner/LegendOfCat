"""Prepare selected clips for the owner-authorized in-game mix review."""
from array import array
import math
from pathlib import Path
import shutil
import runpy
import wave

OUT = Path("Assets/Game/Resources/GoldenRiverAudio")
OUT.mkdir(parents=True, exist_ok=True)


def read(path):
    with wave.open(str(path), "rb") as source:
        if source.getsampwidth() != 2:
            raise ValueError("Expected the retained 16-bit audition PCM.")
        return source.getframerate(), source.getnchannels(), array("h", source.readframes(source.getnframes()))


def write(name, rate, channels, samples):
    with wave.open(str(OUT / name), "wb") as target:
        target.setparams((channels, 2, rate, 0, "NONE", "not compressed"))
        target.writeframes(samples.tobytes())


rate, channels, samples = read("art/audio/auditions-2026-11-21/river-natural.wav")
# Omit the audition's edge fades, then join the same selected river with a smooth overlap.
samples = samples[rate * channels:17 * rate * channels]
overlap = round(.6 * rate)
frames = len(samples) // channels
loop = array("h", samples[overlap * channels:(frames - overlap) * channels])
for i in range(overlap):
    phase = i / (overlap - 1) * math.pi / 2
    for channel in range(channels):
        tail = samples[(frames - overlap + i) * channels + channel]
        head = samples[i * channels + channel]
        loop.append(round(max(-32768, min(32767, tail * math.cos(phase) + head * math.sin(phase)))))
write("river-loop.wav", rate, channels, loop)

runpy.run_path("tools/prepare-runtime-wind.py")

# Preserve the approved warm-body cue with longer entrance shimmer byte-for-byte.
shutil.copyfile("art/audio/checkpoint-achievement-reference/checkpoint-achievement-long-shimmer.wav",
                OUT / "checkpoint.wav")

# The owner selected the quieter natural attack after rejecting the softened version.
shutil.copyfile("art/audio/water-entry-review/water-entry-splash-quiet.wav", OUT / "water-entry.wav")
