"""Prepare a standalone natural splash proposal; never writes Unity Assets."""
from pathlib import Path
import argparse
import subprocess

parser = argparse.ArgumentParser(description=__doc__)
variants = parser.add_mutually_exclusive_group()
variants.add_argument("--soft", action="store_true", help="Reduce level and soften the water impact for owner review.")
variants.add_argument("--quiet", action="store_true", help="Keep the natural attack, reduce only level and the sharpest top end.")
args = parser.parse_args()
folder = Path("art/audio/water-entry-review")
filters = "highpass=f=70:p=2,lowpass=f=8000:p=2,loudnorm=I=-22:TP=-3:LRA=7,afade=t=in:d=0.004,afade=t=out:st=1.5:d=0.6"
if args.soft:
    filters += ",lowpass=f=4200:p=2,acompressor=threshold=0.07:ratio=3:attack=1:release=110:makeup=1,volume=0.5,afade=t=in:d=0.025"
elif args.quiet:
    filters += ",lowpass=f=6500:p=2,volume=0.5"
name = "water-entry-splash-quiet.wav" if args.quiet else "water-entry-splash-soft.wav" if args.soft else "water-entry-splash.wav"
subprocess.run([
    "ffmpeg", "-v", "error", "-y", "-i",
    str(folder / "sources/yurkobb-280219-hq.mp3"), "-t", "2.1",
    "-af", filters,
    "-ar", "44100", "-ac", "2", "-c:a", "pcm_s16le",
    str(folder / name)
], check=True)
