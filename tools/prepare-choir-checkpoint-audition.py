"""One continuous choir/crystal audition, 2.7 seconds; never writes Unity Assets."""
from array import array
import math
from pathlib import Path
import subprocess
import wave

RATE = 44100
DURATION = 2.7
OUT = Path("art/audio/checkpoint-choir-review")
SOURCE = OUT / "sources/breviceps-short-choir-444491-hq.mp3"
count = round(DURATION * RATE)
mix = [[0.0] * count for _ in range(2)]


def source_voice(semitones):
    ratio = 2 ** (semitones / 12)
    processing = f"highpass=f=130,lowpass=f=7800,asetrate={RATE * ratio},aresample={RATE},atempo={.8 / ratio}"
    data = array("f", subprocess.check_output([
        "ffmpeg", "-v", "error", "-i", str(SOURCE), "-af", processing,
        "-ar", str(RATE), "-ac", "2", "-f", "f32le", "pipe:1"]))
    channels = [list(data[channel::2]) for channel in range(2)]
    # Align the public source's audible onset, avoiding a silence before activation.
    threshold = max(abs(value) for value in data) * .012
    onset = next((i for i in range(len(channels[0]))
                  if max(abs(channels[0][i]), abs(channels[1][i])) > threshold), 0)
    onset = max(0, onset - round(.003 * RATE))
    return [values[onset:] for values in channels]


# A single immediate wordless choral arrival; no repeated notes or fanfare sequence.
# The source's strongest pitch is close to B4. Harmonize it into one warm major voicing.
for semitones, level, delay, balance in ((0, .74, 0, .5), (-12, .12, .008, .5),
                                         (4, .22, .013, .3), (7, .18, .021, .7)):
    voice = source_voice(semitones)
    start = round(delay * RATE)
    for i in range(min(len(voice[0]), count - start)):
        t = i / RATE
        envelope = min(1, t / .009) * math.exp(-max(0, t - .28) / 1.35)
        for channel in range(2):
            pan = math.sqrt((1 - balance) if channel == 0 else balance) / math.sqrt(.5)
            mix[channel][start + i] += voice[channel][i] * level * envelope * pan

# Crystal flash at the same activation, followed by a small drifting sparkle cloud.
for frequency, level, onset, decay, balance in (
    (987.767, .07, 0, .17, .5), (1244.508, .045, .012, .16, .35),
    (1479.978, .038, .025, .14, .65), (1975.533, .025, .045, .13, .4),
    (2489.016, .012, .12, .09, .7), (2959.955, .01, .19, .08, .25),
    (1975.533, .009, .26, .08, .6), (3951.066, .006, .34, .06, .35)):
    start = round(onset * RATE)
    for i in range(start, count):
        t = (i - start) / RATE
        envelope = (1 - math.exp(-t / .002)) * math.exp(-t / decay)
        value = (math.sin(2 * math.pi * frequency * t) +
                 .13 * math.sin(2 * math.pi * frequency * 2.003 * t) * math.exp(-t / .045))
        mix[0][i] += value * level * envelope * math.sqrt(1 - balance)
        mix[1][i] += value * level * envelope * math.sqrt(balance)

# A continuous decaying space, not a second hit or a pause between phrases.
dry = [values.copy() for values in mix]
for delay, level in ((.037, .17), (.061, .13), (.103, .1), (.163, .07),
                      (.239, .055), (.353, .035), (.487, .024), (.631, .013)):
    start = round(delay * RATE)
    for i in range(start, count):
        mix[0][i] += dry[1][i - start] * level
        mix[1][i] += dry[0][i - start] * level

peak = max(abs(value) for values in mix for value in values)
gain = .38 / max(peak, .000001)
pcm = array("h")
for i in range(count):
    end_fade = math.sin(min(1, max(0, (count - 1 - i) / (RATE * .75))) * math.pi / 2) ** 2
    for channel in range(2):
        value = mix[channel][i] * gain * end_fade
        pcm.append(round(max(-.95, min(.95, value)) * 32767))
with wave.open(str(OUT / "checkpoint-choir-crystal.wav"), "wb") as target:
    target.setparams((2, 2, RATE, 0, "NONE", "not compressed"))
    target.writeframes(pcm.tobytes())
