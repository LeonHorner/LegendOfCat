"""Original short magical success cue, standalone audition only; no Unity import."""
from array import array
import math
from pathlib import Path
import random
import wave

RATE = 44100
LENGTH = .75
OUT = Path("art/audio/wind-and-magic-review")
OUT.mkdir(parents=True, exist_ok=True)
count = round(LENGTH * RATE)
signal = [[0.0] * count for _ in range(2)]

# A rapid rising, consonant gesture resolves onto a brighter final voicing.
# Authored from individual resonant voices rather than speeding up a rejected cue.
notes = [(0, 523.251, .14), (.055, 659.255, .13), (.11, 783.991, .14),
         (.165, 1046.502, .15)]
for onset, frequency, gain in notes:
    start = round(onset * RATE)
    for i in range(start, count):
        t = (i - start) / RATE
        attack = 1 - math.exp(-t / .0035)
        for channel, detune in ((0, -.0007), (1, .0007)):
            # Decaying inharmonic upper modes add a glass/harp-like strike.
            body = 0.0
            for ratio, weight, decay in ((1, .72, .135), (2.002, .21, .075),
                                         (3.98, .055, .036), (6.07, .012, .018)):
                phase = 2 * math.pi * frequency * ratio * (1 + detune) * t
                body += math.sin(phase) * weight * math.exp(-t / decay)
            signal[channel][i] += body * gain * attack

# Brief warm harmonic reinforcement at the resolution: an audible accomplishment.
for i in range(round(.15 * RATE), count):
    t = i / RATE - .15
    envelope = (1 - math.exp(-t / .012)) * math.exp(-t / .115)
    value = sum(math.sin(2 * math.pi * f * t) for f in (261.626, 523.251, 659.255))
    for channel in signal:
        channel[i] += value * envelope * .022

# A tiny upward sparkling sweep follows the flash, without a prolonged drone.
rng = random.Random(74051)
for spark in range(7):
    onset = .03 + spark * .024
    frequency = rng.choice((1567.982, 2093.005, 2637.02, 3135.963))
    pan = rng.uniform(.15, .85)
    start = round(onset * RATE)
    for i in range(start, min(count, start + round(.15 * RATE))):
        t = (i - start) / RATE
        envelope = (1 - math.exp(-t / .0015)) * math.exp(-t / .025)
        value = math.sin(2 * math.pi * frequency * t) * envelope * .015
        signal[0][i] += value * math.sqrt(1 - pan)
        signal[1][i] += value * math.sqrt(pan)

dry = [channel.copy() for channel in signal]
for delay, gain in ((.037, .16), (.079, .09), (.127, .045)):
    start = round(delay * RATE)
    for i in range(start, count):
        signal[0][i] += dry[1][i - start] * gain
        signal[1][i] += dry[0][i - start] * gain
for channel in signal:
    for i in range(count):
        channel[i] *= min(1, (count - 1 - i) / (RATE * .09))


def write(path, channels):
    pcm = array("h")
    for frame in zip(*channels):
        pcm.extend(round(max(-.95, min(.95, value)) * 32767) for value in frame)
    with wave.open(str(path), "wb") as target:
        target.setparams((2, 2, RATE, 0, "NONE", "not compressed"))
        target.writeframes(pcm.tobytes())


write(OUT / "checkpoint-success-cue.wav", signal)
audition = [[0.0] * round(3.2 * RATE) for _ in range(2)]
for onset in (.2, 1.9):
    start = round(onset * RATE)
    for channel in range(2):
        audition[channel][start:start + count] = signal[channel]
write(OUT / "checkpoint-success-audition.wav", audition)
