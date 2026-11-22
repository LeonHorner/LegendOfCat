"""Standalone wind and magical checkpoint auditions. Never writes Unity Assets."""
from array import array
import math
from pathlib import Path
import random
import subprocess
import wave

RATE = 44100
OUT = Path("art/audio/wind-and-magic-review")
OUT.mkdir(parents=True, exist_ok=True)


def write(name, channels):
    pcm = array("h")
    for frame in zip(*channels):
        pcm.extend(round(max(-.95, min(.95, value)) * 32767) for value in frame)
    with wave.open(str(OUT / name), "wb") as target:
        target.setparams((len(channels), 2, RATE, 0, "NONE", "not compressed"))
        target.writeframes(pcm.tobytes())


def add(target, signal, offset, gain=1):
    start = round(offset * RATE)
    for i, value in enumerate(signal[:len(target) - start]):
        target[start + i] += gain * value


def decode_wind(start):
    data = subprocess.check_output([
        "ffmpeg", "-v", "error", "-ss", str(start), "-i",
        "artifacts/audio-sources/park_ambience_wind.wav", "-t", "8",
        "-af", "highpass=f=100,lowpass=f=4200,loudnorm=I=-27:TP=-6:LRA=7",
        "-ar", str(RATE), "-ac", "2", "-f", "f32le", "pipe:1"])
    interleaved = array("f", data)
    channels = [list(interleaved[channel::2]) for channel in range(2)]
    for channel in channels:
        for i in range(len(channel)):
            t = i / RATE
            # An unhurried swell followed by a longer, softer retreat into silence.
            rise = math.sin(min(1, t / 2.3) * math.pi / 2) ** 2
            fall = math.sin(min(1, max(0, (8 - t) / 4.5)) * math.pi / 2) ** 2
            channel[i] *= rise * fall
    return channels


wind = [[0.0] * (RATE * 25) for _ in range(2)]
for offset, source_start, gain in ((1, 25, 1), (15, 48, .8)):
    gust = decode_wind(source_start)
    for channel in range(2):
        add(wind[channel], gust[channel], offset, gain)
write("wind-gusts.wav", wind)


def magic(kind):
    count = round(3.2 * RATE)
    dry = [[0.0] * count for _ in range(2)]
    root = 261.6256 if kind == 0 else 349.2282
    for i in range(count):
        t = i / RATE
        bloom = (1 - math.exp(-t / .09)) * math.exp(-t / (.62 if kind == 0 else .8))
        tail = min(1, max(0, (3.2 - t) / .5))
        for channel, detune in ((0, -.001), (1, .001)):
            tone = sum(weight * math.sin(2 * math.pi * root * ratio * (1 + detune) * t)
                       for ratio, weight in ((1, .6), (2, .28), (3, .10), (4, .04)))
            dry[channel][i] = tone * bloom * tail * (.14 if kind == 0 else .09)
    rng = random.Random(74030 + kind)
    # A small irregular cloud of sparks, not a sequence of ascending notes.
    for spark in range(8 if kind == 0 else 14):
        offset = .12 + rng.random() * .85
        frequency = root * rng.choice((4, 5, 6, 8)) * rng.uniform(.997, 1.003)
        pan = rng.uniform(.15, .85)
        signal = []
        for i in range(round(.75 * RATE)):
            t = i / RATE
            env = (1 - math.exp(-t / .004)) * math.exp(-t / .12)
            signal.append(math.sin(2 * math.pi * frequency * t) * env * (.017 if kind == 0 else .021))
        add(dry[0], signal, offset, math.sqrt(1 - pan))
        add(dry[1], signal, offset, math.sqrt(pan))
    wet = [channel.copy() for channel in dry]
    for delay, gain in ((.071, .17), (.131, .12), (.223, .08), (.349, .04)):
        add(wet[0], dry[1], delay, gain)
        add(wet[1], dry[0], delay * 1.05, gain)
    return wet


comparison = [[0.0] * (RATE * 10) for _ in range(2)]
for kind, offset in ((0, .5), (1, 5.5)):
    cue = magic(kind)
    for channel in range(2):
        add(comparison[channel], cue[channel], offset)
write("checkpoint-magic-comparison.wav", comparison)
