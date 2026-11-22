"""Attenuate narrow upper resonances in the previous checkpoint correction."""
from pathlib import Path
import subprocess

folder = Path("art/audio/checkpoint-achievement-reference")
# Prominent sustained upper-band peaks measured in the retained correction.
# Their relation to the owner's perceived background bing needs listening review.
bands = ((1238, 18, -16), (1394, 18, -16), (1556, 20, -24),
         (1857, 20, -24), (2336, 18, -12))
filters = [f"equalizer=f={frequency}:t=q:w={quality}:g={gain}"
           for frequency, quality, gain in bands]
subprocess.run([
    "ffmpeg", "-v", "error", "-y", "-i",
    str(folder / "checkpoint-achievement-reduced-knock.wav"),
    "-af", ",".join(filters), "-ar", "44100", "-ac", "2", "-c:a", "pcm_s16le",
    str(folder / "checkpoint-achievement-reduced-ring.wav")
], check=True)
