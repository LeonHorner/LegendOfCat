"""Short magical activation studies, outside Unity until owner listening approval."""
from array import array
import math
from pathlib import Path
import random
import wave

RATE = 44100
DURATION = .6
OUT = Path("art/audio/wind-and-magic-review")
OUT.mkdir(parents=True, exist_ok=True)


def cue(kind):
    root = 261.6256 if kind == 0 else 349.2282
    channels = [[0.0] * round(DURATION * RATE) for _ in range(2)]
    for i in range(len(channels[0])):
        t = i / RATE
        envelope = (1 - math.exp(-t / .009)) * math.exp(-t / .095)
        fade = min(1, max(0, (DURATION - t) / .08))
        for channel, detune in ((0, -.001), (1, .001)):
            tone = sum(weight * math.sin(2 * math.pi * root * ratio * (1 + detune) * t)
                       for ratio, weight in ((1, .6), (2, .28), (3, .1), (4, .04)))
            channels[channel][i] = tone * envelope * fade * (.17 if kind == 0 else .12)
    rng = random.Random(74040 + kind)
    for _ in range(5 if kind == 0 else 8):
        start = round(rng.uniform(.015, .09) * RATE)
        frequency = root * rng.choice((4, 5, 6, 8))
        pan = rng.uniform(.2, .8)
        for i in range(start, len(channels[0])):
            t = (i - start) / RATE
            env = (1 - math.exp(-t / .002)) * math.exp(-t / .035)
            fade = min(1, max(0, (len(channels[0]) - i) / (RATE * .08)))
            value = math.sin(2 * math.pi * frequency * t) * env * fade * .022
            channels[0][i] += value * math.sqrt(1 - pan)
            channels[1][i] += value * math.sqrt(pan)
    # Compact reflections only; no lingering shimmer after the visual activation.
    dry = [values.copy() for values in channels]
    for delay, gain in ((.025, .1), (.057, .055)):
        start = round(delay * RATE)
        for i in range(start, len(channels[0])):
            channels[0][i] += dry[1][i - start] * gain
            channels[1][i] += dry[0][i - start] * gain
    for values in channels:
        for i in range(len(values)):
            values[i] *= min(1, (len(values) - 1 - i) / (RATE * .03))
    return channels


comparison = [[0.0] * (RATE * 4) for _ in range(2)]
for kind, offset in ((0, .3), (1, 2.3)):
    source = cue(kind)
    start = round(offset * RATE)
    for channel in range(2):
        comparison[channel][start:start + len(source[channel])] = source[channel]
pcm = array("h")
for frame in zip(*comparison):
    pcm.extend(round(max(-.95, min(.95, sample)) * 32767) for sample in frame)
with wave.open(str(OUT / "checkpoint-magic-short.wav"), "wb") as target:
    target.setparams((2, 2, RATE, 0, "NONE", "not compressed"))
    target.writeframes(pcm.tobytes())
