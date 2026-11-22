"""Prepare a separately attributed achievement audition. Never writes Unity Assets."""
from pathlib import Path
import subprocess

folder = Path("art/audio/checkpoint-achievement-reference")
subprocess.run([
    "ffmpeg", "-v", "error", "-y", "-i",
    str(folder / "sources/tommasomotteran-859080-hq.mp3"),
    "-af", "silenceremove=start_periods=1:start_duration=0.002:start_threshold=-50dB,"
           "asetpts=N/SR/TB,"
           "loudnorm=I=-21:TP=-3:LRA=7,apad=whole_dur=3,atrim=duration=3,"
           "afade=t=in:d=0.004,afade=t=out:st=2.45:d=0.55",
    "-ar", "44100", "-ac", "2", "-c:a", "pcm_s16le",
    str(folder / "checkpoint-achievement-audition.wav")
], check=True)
