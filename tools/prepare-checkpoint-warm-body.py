"""Keep the diagnosed low body and a short airy entrance, omit the bell band."""
from pathlib import Path
import argparse
import subprocess

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--long-shimmer", action="store_true", help="Lengthen only the accepted early shimmer, retaining the removed bell band.")
args = parser.parse_args()
folder = Path("art/audio/checkpoint-achievement-reference")
# The owner hears the repeating bing in diagnosis B (900–2500 Hz), and more
# clearly at the end of C (>2500 Hz). Omit the middle band rather than another
# narrow notch; retain upper air only briefly at the beginning.
# Stretch only the accepted early high layer, not the later ringing source.
# atempo preserves pitch; no extra delayed notes or repeating loop is added.
entrance = "[air]volume=0.5,afade=t=out:st=0.15:d=0.65"
if args.long_shimmer:
    entrance += ",atempo=0.55"
filters = (
    "[0:a]acrossover=split='800 2500':order=12th[body][bells][air];"
    "[bells]anullsink;"
    + entrance + "[entrance];"
    "[body][entrance]amix=inputs=2:normalize=0:duration=first,"
    "afade=t=in:d=0.015,afade=t=out:st=2.54:d=0.042[out]"
)
subprocess.run([
    "ffmpeg", "-v", "error", "-y", "-i",
    str(folder / "checkpoint-achievement-reduced-knock.wav"),
    "-filter_complex", filters, "-map", "[out]", "-ar", "44100", "-ac", "2",
    "-c:a", "pcm_s16le", str(folder / ("checkpoint-achievement-long-shimmer.wav"
                                        if args.long_shimmer else "checkpoint-achievement-warm-body.wav"))
], check=True)
