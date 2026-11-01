"""Original unpitched damage sound proposals; none is enabled in gameplay."""

import math
from pathlib import Path
import random
import struct
import wave


def generate(name, duration, cutoff, attack, decay, high_pass):
    rate = 22050
    random_source = random.Random(20261031)
    low = slow = 0.0
    samples = []
    coefficient = 1 - math.exp(-2 * math.pi * cutoff / rate)
    for index in range(round(rate * duration)):
        t = index / rate
        low += coefficient * (random_source.uniform(-1, 1) - low)
        slow += 0.025 * (low - slow)
        noise = low - high_pass * slow
        envelope = (1 - math.exp(-t / attack)) * math.exp(-t / decay)
        # A smooth tail avoids an abrupt cut in the preview player.
        envelope *= math.sin(math.pi * index / (rate * duration)) ** 0.5
        samples.append(noise * envelope)
    rms = math.sqrt(sum(value * value for value in samples) / len(samples))
    gain = min(0.035 / rms, 0.24 / max(abs(value) for value in samples))
    output = Path(__file__).resolve().parents[1] / "docs/design/stomp-prototype/sound-options"
    output.mkdir(parents=True, exist_ok=True)
    with wave.open(str(output / (name + ".wav")), "wb") as clip:
        clip.setparams((1, 2, rate, len(samples), "NONE", "not compressed"))
        clip.writeframes(b"".join(struct.pack("<h", round(value * gain * 32767)) for value in samples))


if __name__ == "__main__":
    generate("soft-impact", 0.120, 450, 0.002, 0.025, 0.0)
    generate("muted-tap", 0.085, 1700, 0.001, 0.012, 0.7)
    generate("soft-rustle", 0.140, 2900, 0.009, 0.040, 1.0)
