"""Prepare one smooth, long wind study outside Unity using filtered noise."""
from array import array
import math
from pathlib import Path
import subprocess
import wave

RATE = 44100
DURATION = 17
OUT = Path("art/audio/long-wind-review")
OUT.mkdir(parents=True, exist_ok=True)


def noise(seed, filters):
    return array("f", subprocess.check_output([
        "ffmpeg", "-v", "error", "-f", "lavfi", "-i",
        f"anoisesrc=color=pink:seed={seed}:sample_rate={RATE}:duration={DURATION}",
        "-af", filters, "-f", "f32le", "pipe:1"]))


# Continuous noise, no leaves, impacts, rain samples or thunder recordings.
body = noise(741, "highpass=f=140:p=2,lowpass=f=1500:p=2")
air = noise(742, "highpass=f=550:p=2,lowpass=f=2900:p=2")
breath = noise(743, "bandpass=f=700:t=q:w=0.8")
channels = [[], []]
for i, (low, high, mid) in enumerate(zip(body, air, breath)):
    t = i / RATE
    rise = math.sin(min(1, t / 3.8) * math.pi / 2) ** 2
    # Most of the gesture is a continuous, progressively receding tail.
    tail = math.cos(min(1, max(0, (t - 4.2) / 12.8)) * math.pi / 2) ** 2
    envelope = rise * tail
    drift = .5 + .5 * math.sin(t * .47 - .9)
    sample = (low * .95 + high * (.16 + .16 * drift) + mid * .24) * envelope
    pan = .04 * math.sin(t * .21)
    channels[0].append(sample * (1 - pan))
    channels[1].append(sample * (1 + pan))

peak = max(abs(value) for channel in channels for value in channel)
gain = .32 / peak
pcm = array("h")
for frame in zip(*channels):
    pcm.extend(round(value * gain * 32767) for value in frame)
with wave.open(str(OUT / "long-receding-wind.wav"), "wb") as target:
    target.setparams((2, 2, RATE, 0, "NONE", "not compressed"))
    target.writeframes(pcm.tobytes())
