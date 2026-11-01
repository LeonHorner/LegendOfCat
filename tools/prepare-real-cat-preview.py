"""Attributed, review-only real-cat excerpts; no gameplay sound is selected."""

from pathlib import Path
import subprocess


folder = Path(__file__).resolve().parents[1] / "docs/design/stomp-prototype/sound-options/real-cat"
for name, tempo, fade_start in [
    ("natural-reaction", 1, 1.14),
    ("short-reaction", 4 / 3, 0.84),
]:
    subprocess.run([
        "ffmpeg", "-hide_banner", "-loglevel", "error", "-y",
        "-ss", "0.9", "-t", "1.2", "-i", str(folder / "source-preview.mp3"),
        "-ac", "1", "-ar", "48000", "-af",
        f"atempo={tempo},afade=t=in:d=0.005,afade=t=out:st={fade_start}:d=0.06,volume=0.65",
        "-c:a", "pcm_s16le", str(folder / (name + ".wav")),
    ], check=True)
