"""Original deterministic review sounds. Python standard library; no sampled audio.

Run from the repository root. PCM sources remain editable through this script.
"""
import math
from pathlib import Path
import random
import struct
import wave

RATE = 44100
OUT = Path("Assets/Game/Resources/GoldenRiverAudio")
OUT.mkdir(parents=True, exist_ok=True)


def save(name, samples):
    with wave.open(str(OUT / (name + ".wav")), "wb") as wav:
        wav.setparams((1, 2, RATE, 0, "NONE", "not compressed"))
        wav.writeframes(b"".join(struct.pack("<h", round(max(-1, min(1, x)) * 32767)) for x in samples))


def noise(count, seed):
    rng = random.Random(seed)
    return [rng.uniform(-1, 1) for _ in range(count)]


# Cyclic, filtered noise with slow overlapping swells; no discontinuity at the loop.
duration = 12
raw = noise(RATE * duration, 74)
low = 0
river = []
for i in range(len(raw) * 2):
    t = (i % len(raw)) / RATE
    low += .11 * (raw[i % len(raw)] - low)
    if i >= len(raw):
        swell = .76 + .12 * math.sin(2 * math.pi * t / duration) + .07 * math.sin(6 * math.pi * t / duration)
        river.append(low * swell * .9)
save("river", river)


def rustle(name, duration, seed, attack, weight):
    filtered = 0
    samples = []
    for i, n in enumerate(noise(int(duration * RATE), seed)):
        t = i / RATE
        filtered += .16 * (n - filtered)
        envelope = min(1, t / attack) * (1 - t / duration) ** 3
        # Short soft contact, not a pitched cartoon jump or a footstep loop.
        body = math.sin(2 * math.pi * (95 * t - 50 * t * t)) * math.exp(-25 * t)
        samples.append((filtered * .6 + body * weight) * envelope)
    save(name, samples)


rustle("jump", .16, 741, .008, .12)
rustle("land", .21, 742, .003, .38)


def chime(name, notes, spacing, duration):
    samples = []
    for i in range(int(duration * RATE)):
        t = i / RATE
        value = 0
        for j, frequency in enumerate(notes):
            age = t - j * spacing
            if age < 0:
                continue
            envelope = min(1, age / .009) * math.exp(-5 * age) * min(1, (duration - t) / .08)
            value += .16 * envelope * (math.sin(2 * math.pi * frequency * age) + .15 * math.sin(4 * math.pi * frequency * age))
        samples.append(value)
    save(name, samples)


chime("checkpoint", [523.25, 659.25], .12, .9)
chime("complete", [523.25, 659.25, 783.99, 1046.5], .15, 1.8)
