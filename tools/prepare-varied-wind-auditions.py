"""Different filtered forest gusts for listening, never imported into Unity here."""
from array import array
import math
from pathlib import Path
import subprocess
import wave

RATE = 44100
OUT = Path("art/audio/wind-variety-review")
SOURCE = OUT / "sources/anya-media-forest-wind-523389-hq.mp3"
OUT.mkdir(parents=True, exist_ok=True)
STUDIES = ((8, 4.5, .8, 2.4, 1), (38, 6.5, 1.6, 3.6, .85),
           (77, 5.2, .7, 3.0, 1.08), (116, 7, 2.0, 4.2, .9))


def write(name, channels):
    pcm = array("h")
    for frame in zip(*channels):
        pcm.extend(round(max(-.95, min(.95, value)) * 32767) for value in frame)
    with wave.open(str(OUT / name), "wb") as target:
        target.setparams((2, 2, RATE, 0, "NONE", "not compressed"))
        target.writeframes(pcm.tobytes())


studies = []
for index, (start, duration, rise, fall, level) in enumerate(STUDIES, 1):
    raw = array("f", subprocess.check_output([
        "ffmpeg", "-v", "error", "-ss", str(start), "-i", str(SOURCE), "-t", str(duration),
        "-af", "highpass=f=260:p=2,highpass=f=260:p=2,lowpass=f=6000,"
               "loudnorm=I=-26:TP=-6:LRA=7",
        "-ar", str(RATE), "-ac", "2", "-f", "f32le", "pipe:1"]))
    channels = [list(raw[channel::2]) for channel in range(2)]
    for channel in channels:
        for i in range(len(channel)):
            t = i / RATE
            attack = math.sin(min(1, t / rise) * math.pi / 2) ** 2
            release = math.sin(min(1, max(0, (duration - t) / fall)) * math.pi / 2) ** 2
            channel[i] *= attack * release * level
    peak = max(abs(value) for channel in channels for value in channel)
    if peak > .45:
        channels = [[value * .45 / peak for value in channel] for channel in channels]
    write(f"forest-gust-{index:02}.wav", channels)
    studies.append(channels)

# One of each distinct recording, separated just enough to hear their endings.
length = sum(len(study[0]) for study in studies) + round(1.2 * RATE) * 3
comparison = [[0.0] * length for _ in range(2)]
position = 0
for study in studies:
    for channel in range(2):
        comparison[channel][position:position + len(study[channel])] = study[channel]
    position += len(study[0]) + round(1.2 * RATE)
write("wind-variety-audition.wav", comparison)
