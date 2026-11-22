"""Reduce upper-band percussive attacks in the selected checkpoint audition."""
from array import array
import cmath
import math
from pathlib import Path
import statistics
import wave

FOLDER = Path("art/audio/checkpoint-achievement-reference")
SIZE, HOP = 2048, 256


def fft(values, inverse=False):
    result = list(values)
    j = 0
    for i in range(1, len(result)):
        bit = len(result) >> 1
        while j & bit:
            j ^= bit
            bit >>= 1
        j ^= bit
        if i < j:
            result[i], result[j] = result[j], result[i]
    width = 2
    while width <= len(result):
        root = cmath.exp((2j if inverse else -2j) * math.pi / width)
        for start in range(0, len(result), width):
            phase = 1
            for offset in range(width // 2):
                a = result[start + offset]
                b = phase * result[start + offset + width // 2]
                result[start + offset] = a + b
                result[start + offset + width // 2] = a - b
                phase *= root
        width *= 2
    return [value / len(result) for value in result] if inverse else result


with wave.open(str(FOLDER / "checkpoint-achievement-soft-start.wav"), "rb") as source:
    rate, count = source.getframerate(), source.getnchannels()
    assert source.getsampwidth() == 2
    pcm = array("h", source.readframes(source.getnframes()))

window = [math.sin(math.pi * (i + .5) / SIZE) for i in range(SIZE)]
channels = [[sample / 32768 for sample in pcm[c::count]] for c in range(count)]
length = len(channels[0])
starts = list(range(0, length + SIZE, HOP))
spectra = []
for channel in channels:
    padded = [0.0] * SIZE + channel + [0.0] * (SIZE * 2)
    spectra.append([fft([padded[start + i] * window[i] for i in range(SIZE)]) for start in starts])
# One shared stereo mask preserves the image. Across-time persistence retains
# sustained notes; across-frequency energy identifies brief broadband attacks.
magnitudes = [[sum(abs(spectra[c][t][k]) for c in range(count)) / count
               for k in range(SIZE // 2 + 1)] for t in range(len(starts))]
for t, row in enumerate(magnitudes):
    gains = []
    for k in range(len(row)):
        frequency = k * rate / SIZE
        harmonic = statistics.median(magnitudes[frame][k]
                                    for frame in range(max(0, t - 15), min(len(starts), t + 16)))
        percussive = statistics.median(row[max(0, k - 8):k + 9])
        mask = harmonic ** 2 / (harmonic ** 2 + (1.6 * percussive) ** 2 + 1e-20)
        amount = min(1, max(0, (frequency - 900) / 900))
        transient_gain = 1 - amount * (1 - max(.15, mask))
        # A gentle upper shelf reduces the remaining bright ringing, without
        # substituting a new cue or changing its timing and low foundation.
        shelf = 10 ** (-9 * min(1, max(0, (frequency - 1800) / 1800)) / 20)
        gains.append(transient_gain * shelf)
    for c in range(count):
        for k, gain in enumerate(gains):
            spectra[c][t][k] *= gain
            if 0 < k < SIZE // 2:
                spectra[c][t][SIZE - k] *= gain

output = []
for c in range(count):
    mixed = [0.0] * (starts[-1] + SIZE)
    weight = [0.0] * len(mixed)
    for start, spectrum in zip(starts, spectra[c]):
        frame = fft(spectrum, inverse=True)
        for i, value in enumerate(frame):
            mixed[start + i] += value.real * window[i]
            weight[start + i] += window[i] ** 2
    output.append([mixed[i] / max(weight[i], 1e-12) for i in range(SIZE, SIZE + length)])

result = array("h")
for frame in zip(*output):
    result.extend(round(max(-1, min(32767 / 32768, value)) * 32768) for value in frame)
with wave.open(str(FOLDER / "checkpoint-achievement-reduced-knock.wav"), "wb") as target:
    target.setparams((count, 2, rate, 0, "NONE", "not compressed"))
    target.writeframes(result.tobytes())
