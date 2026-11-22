"""Standalone auditions only. Never writes Unity Assets or installs gameplay sounds.

Requires ffmpeg on PATH and the source downloads documented in the audition README.
"""
from array import array
import math
from pathlib import Path
import random
import subprocess
import wave

RATE = 44100
OUT = Path("art/audio/auditions-2026-11-21")
RAW = Path("artifacts/audio-sources")
OUT.mkdir(parents=True, exist_ok=True)


def ffmpeg(*args):
    return subprocess.check_output(["ffmpeg", "-v", "error", *map(str, args)])


def write(name, samples):
    pcm = array("h", (round(max(-.95, min(.95, s)) * 32767) for s in samples))
    with wave.open(str(OUT / name), "wb") as target:
        target.setparams((1, 2, RATE, 0, "NONE", "not compressed"))
        target.writeframes(pcm.tobytes())


def mix(destination, samples, seconds, gain=1):
    start = round(seconds * RATE)
    for i, value in enumerate(samples[:len(destination) - start]):
        destination[start + i] += value * gain


# Keep real water movement and wildlife; remove rumble and fade this excerpt for audition.
ffmpeg("-y", "-ss", 120, "-i", RAW / "park_ambience_river.wav", "-t", 18,
       "-af", "highpass=f=40,loudnorm=I=-24:TP=-3:LRA=7,afade=t=in:d=0.4,afade=t=out:st=17.5:d=0.5",
       "-ar", RATE, "-c:a", "pcm_s16le", OUT / "river-natural.wav")

# A/B movement study: several real contacts, shortened and softened rather than a pitched beep.
movement = [0.0] * (RATE * 8)
for section, surface in enumerate(("Sand", "Stone")):
    for i in range(10):
        side, variant = ("L" if i % 2 == 0 else "R"), i % 3 + 1
        source = RAW / "footsteps/Fantozzi-footsteps/flac" / f"Fantozzi-{surface}{side}{variant}.flac"
        decoded = array("f", ffmpeg("-i", source, "-af", "highpass=f=220,lowpass=f=5500",
                                   "-ar", RATE, "-ac", 1, "-f", "f32le", "pipe:1"))
        peak = max(map(abs, decoded), default=1) or 1
        softened = []
        for j, value in enumerate(decoded[:round(.2 * RATE)]):
            edge = min(1, j / (RATE * .002), (round(.2 * RATE) - j) / (RATE * .03))
            softened.append(value / peak * .12 * max(0, edge))
        mix(movement, softened, section * 4 + .25 + i * .26, .9 if i % 2 else 1)
write("movement-comparison.wav", movement)

# Original non-melodic impacts: short stone resonance versus a dry wood tap.
def impact(modes, damping, noise_amount, seed):
    rng = random.Random(seed)
    result = []
    for i in range(round(.65 * RATE)):
        t = i / RATE
        envelope = min(1, t / .0015)
        body = sum(math.sin(2 * math.pi * frequency * t) * weight * math.exp(-damping * t * (1 + index * .35))
                   for index, (frequency, weight) in enumerate(modes))
        transient = rng.uniform(-1, 1) * math.exp(-140 * t) * noise_amount
        result.append((body * .13 + transient) * envelope)
    return result


checkpoint = [0.0] * (RATE * 7)
stone = impact([(440, .6), (719, .25), (1210, .08)], 12, .012, 7401)
wood = impact([(285, .7), (497, .25), (883, .06)], 30, .028, 7402)
for start, sample in ((.35, stone), (1.8, stone), (4.0, wood), (5.45, wood)):
    mix(checkpoint, sample, start)
write("checkpoint-comparison.wav", checkpoint)
