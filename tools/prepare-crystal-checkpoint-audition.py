"""One continuous crystal/soft-bell audition, no choir and no Unity import."""
from array import array
import math
from pathlib import Path
import wave

RATE = 44100
DURATION = 2.7
OUT = Path("art/audio/checkpoint-crystal-review")
OUT.mkdir(parents=True, exist_ok=True)
count = round(DURATION * RATE)
audio = [[0.0] * count for _ in range(2)]


def crystal(frequency, onset, level, decay, pan):
    start = round(onset * RATE)
    modes = ((1, .78, 1), (2.01, .14, .42), (2.76, .035, .23),
             (4.07, .012, .14), (5.4, .004, .08))
    for i in range(start, count):
        t = (i - start) / RATE
        attack = 1 - math.exp(-t / .004)
        for channel, detune, balance in ((0, -.0006, math.sqrt(1 - pan)),
                                         (1, .0006, math.sqrt(pan))):
            value = sum(math.sin(2 * math.pi * frequency * ratio * (1 + detune) * t)
                        * strength * math.exp(-t / (decay * scale))
                        for ratio, strength, scale in modes)
            audio[channel][i] += value * attack * level * balance


# One closely aligned bright arrival, not a stepped fanfare or vocal pad.
crystal(493.883, 0, .08, .68, .5)
crystal(987.767, .004, .17, .85, .5)
crystal(1244.508, .014, .08, .73, .3)
crystal(1479.978, .025, .075, .71, .7)

# Fine irregular sparks dissolve into the main soft-bell resonance.
for frequency, onset, level, pan in ((1975.533, .09, .024, .25),
                                     (2489.016, .17, .018, .75),
                                     (2959.955, .29, .013, .35),
                                     (1975.533, .43, .009, .65),
                                     (3951.066, .57, .006, .3),
                                     (2489.016, .76, .004, .7)):
    crystal(frequency, onset, level, .16, pan)

dry = [channel.copy() for channel in audio]
for delay, level in ((.043, .14), (.067, .11), (.109, .09), (.163, .075),
                      (.227, .06), (.311, .045), (.409, .032), (.533, .022), (.683, .014)):
    start = round(delay * RATE)
    for i in range(start, count):
        audio[0][i] += dry[1][i - start] * level
        audio[1][i] += dry[0][i - start] * level

peak = max(abs(value) for channel in audio for value in channel)
gain = .32 / max(peak, .000001)
pcm = array("h")
for i in range(count):
    fade = math.sin(min(1, max(0, (count - 1 - i) / (RATE * .6))) * math.pi / 2) ** 2
    for channel in range(2):
        pcm.append(round(max(-.95, min(.95, audio[channel][i] * gain * fade)) * 32767))
with wave.open(str(OUT / "checkpoint-crystal-bloom.wav"), "wb") as target:
    target.setparams((2, 2, RATE, 0, "NONE", "not compressed"))
    target.writeframes(pcm.tobytes())
