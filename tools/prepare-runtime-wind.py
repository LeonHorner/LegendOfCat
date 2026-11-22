"""Render four variations of the selected continuous-air wind direction."""
from array import array
import math
from pathlib import Path
import subprocess
import wave

RATE = 44100
OUT = Path("Assets/Game/Resources/GoldenRiverAudio")
OUT.mkdir(parents=True, exist_ok=True)
# Duration, rise, release start, air-band balance, breath frequency, relative level.
VARIANTS = ((12.5, 2.2, 1.8, .25, 700, 1),
            (14, 2.7, 2.1, .21, 620, .9),
            (11.5, 1.9, 1.5, .28, 780, .95),
            (13.5, 2.4, 1.9, .23, 660, .85))


def noise(seed, duration, filters):
    return array("f", subprocess.check_output([
        "ffmpeg", "-v", "error", "-f", "lavfi", "-i",
        f"anoisesrc=color=pink:seed={seed}:sample_rate={RATE}:duration={duration}",
        "-af", filters, "-f", "f32le", "pipe:1"]))


for index, (duration, rise, release_start, air_level, frequency, level) in enumerate(VARIANTS, 1):
    body = noise(741 + index * 10, duration, "highpass=f=140:p=2,lowpass=f=1500:p=2")
    air = noise(742 + index * 10, duration, "highpass=f=550:p=2,lowpass=f=2900:p=2")
    breath = noise(743 + index * 10, duration, f"bandpass=f={frequency}:t=q:w=0.8")
    channels = [[], []]
    for i, (low, high, mid) in enumerate(zip(body, air, breath)):
        t = i / RATE
        attack = math.sin(min(1, t / rise) * math.pi / 2) ** 2
        # Start receding during the rise: shorten the loud crest, keep a long tail.
        release = math.cos(min(1, max(0, (t - release_start) / (duration - release_start))) * math.pi / 2) ** 4
        drift = .5 + .5 * math.sin(t * (.42 + index * .02) - .9)
        sample = (low * .95 + high * (air_level * (.65 + .65 * drift)) + mid * .24) * attack * release
        pan = .04 * math.sin(t * .21 + index * .3)
        channels[0].append(sample * (1 - pan))
        channels[1].append(sample * (1 + pan))
    peak = max(abs(value) for channel in channels for value in channel)
    gain = .32 * level / peak
    pcm = array("h")
    for frame in zip(*channels):
        pcm.extend(round(value * gain * 32767) for value in frame)
    with wave.open(str(OUT / f"wind-{index:02}.wav"), "wb") as target:
        target.setparams((2, 2, RATE, 0, "NONE", "not compressed"))
        target.writeframes(pcm.tobytes())
