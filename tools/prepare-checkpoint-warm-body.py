"""Keep the diagnosed low body and a short airy entrance, omit the bell band."""
from pathlib import Path
import subprocess

folder = Path("art/audio/checkpoint-achievement-reference")
# The owner hears the repeating bing in diagnosis B (900–2500 Hz), and more
# clearly at the end of C (>2500 Hz). Omit the middle band rather than another
# narrow notch; retain upper air only briefly at the beginning.
filters = (
    "[0:a]acrossover=split='800 2500':order=12th[body][bells][air];"
    "[bells]anullsink;"
    "[air]volume=0.5,afade=t=out:st=0.15:d=0.65[entrance];"
    "[body][entrance]amix=inputs=2:normalize=0,"
    "afade=t=in:d=0.015,afade=t=out:st=2.54:d=0.042[out]"
)
subprocess.run([
    "ffmpeg", "-v", "error", "-y", "-i",
    str(folder / "checkpoint-achievement-reduced-knock.wav"),
    "-filter_complex", filters, "-map", "[out]", "-ar", "44100", "-ac", "2",
    "-c:a", "pcm_s16le", str(folder / "checkpoint-achievement-warm-body.wav")
], check=True)
