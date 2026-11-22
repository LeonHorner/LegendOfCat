"""Fuller magical checkpoint success audition; writes review audio, never Assets."""
from array import array
import math
from pathlib import Path
import random
import wave

RATE = 44100
DURATION = 1.65
OUT = Path("art/audio/wind-and-magic-review")
OUT.mkdir(parents=True, exist_ok=True)
count = round(DURATION * RATE)
channels = [[0.0] * count for _ in range(2)]


def voice(onset, frequency, gain, decay, timbre="glass", pan=.5):
    start = round(onset * RATE)
    for i in range(start, count):
        t = (i - start) / RATE
        if timbre == "glass":
            envelope = (1 - math.exp(-t / .004)) * math.exp(-t / decay)
            modes = ((1, .72), (2.002, .2), (3.98, .05), (6.07, .012))
        else:
            # A short rounded fanfare bloom, with a softer sustained body.
            envelope = (1 - math.exp(-t / .023)) * math.exp(-max(0, t - .1) / decay)
            modes = tuple((harmonic, .68 / harmonic ** 1.65) for harmonic in range(1, 8))
        for channel, detune, weight in ((0, -.0009, math.sqrt(1 - pan)),
                                        (1, .0009, math.sqrt(pan))):
            value = sum(math.sin(2 * math.pi * frequency * ratio * (1 + detune) * t)
                        * strength * math.exp(-t * (ratio - 1) * (.6 if timbre == "glass" else 1.2))
                        for ratio, strength in modes)
            channels[channel][i] += value * envelope * gain * weight


# Preserve the liked upward gesture, with more space and a clear arrival.
for onset, frequency, gain, pan in ((0, 391.995, .11, .32), (.07, 523.251, .14, .42),
                                  (.14, 659.255, .14, .58), (.22, 783.991, .16, .65),
                                  (.34, 1046.502, .18, .5)):
    voice(onset, frequency, gain, .24, pan=pan)

# A warm low foundation and broad major voicing give the achievement weight.
for frequency, gain, pan in ((130.813, .065, .5), (261.626, .075, .35),
                             (329.628, .055, .65), (391.995, .055, .4),
                             (523.251, .045, .6)):
    voice(.27, frequency, gain, .34, "fanfare", pan)

rng = random.Random(74122)
for i in range(12):
    voice(.11 + i * .037 + rng.uniform(-.008, .008),
          rng.choice((1567.982, 2093.005, 2637.02, 3135.963)),
          .014, .055, pan=rng.uniform(.12, .88))

# Diffuse stereo reflections let the magical arrival breathe without a long drone.
dry = [values.copy() for values in channels]
for delay, gain in ((.031, .14), (.057, .11), (.091, .09), (.137, .07),
                    (.211, .055), (.317, .035), (.413, .018)):
    start = round(delay * RATE)
    for i in range(start, count):
        channels[0][i] += dry[1][i - start] * gain
        channels[1][i] += dry[0][i - start] * gain

peak = max(abs(value) for values in channels for value in values)
gain = .3 / max(peak, .000001)
for values in channels:
    for i in range(count):
        fade = min(1, max(0, (count - 1 - i) / (RATE * .25)))
        values[i] *= gain * fade


def write(name, audio):
    pcm = array("h")
    for frame in zip(*audio):
        pcm.extend(round(max(-.95, min(.95, value)) * 32767) for value in frame)
    with wave.open(str(OUT / name), "wb") as target:
        target.setparams((2, 2, RATE, 0, "NONE", "not compressed"))
        target.writeframes(pcm.tobytes())


write("checkpoint-epic-success.wav", channels)
audition = [[0.0] * round(5.4 * RATE) for _ in range(2)]
for onset in (.2, 3.2):
    start = round(onset * RATE)
    for channel in range(2):
        audition[channel][start:start + count] = channels[channel]
write("checkpoint-epic-audition.wav", audition)
