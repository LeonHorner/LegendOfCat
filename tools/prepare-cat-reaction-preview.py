"""Prepare a short review-only excerpt of the documented CC0 vocal performance."""

from pathlib import Path
import array
import subprocess
import sys
import wave


root = Path(__file__).resolve().parents[1]
folder = root / "docs/design/stomp-prototype/sound-options/cat-reaction"
decoded = subprocess.run(
    ["ffmpeg", "-v", "error", "-i", str(folder / "source-preview.mp3"),
     "-f", "s16le", "-ac", "1", "-ar", "48000", "pipe:1"],
    check=True, capture_output=True,
).stdout
samples = array.array("h", decoded)
if sys.byteorder != "little":
    samples.byteswap()
rate = 48000
excerpt = samples[round(0.045 * rate):round(0.465 * rate)]
gain = 0.35 * 32767 / max(abs(value) for value in excerpt)
for index in range(len(excerpt)):
    fade = min(1, index / (0.003 * rate), (len(excerpt) - 1 - index) / (0.035 * rate))
    excerpt[index] = round(excerpt[index] * gain * fade)
if sys.byteorder != "little":
    excerpt.byteswap()
with wave.open(str(folder / "short-reaction.wav"), "wb") as output:
    output.setparams((1, 2, rate, len(excerpt), "NONE", "not compressed"))
    output.writeframes(excerpt.tobytes())
