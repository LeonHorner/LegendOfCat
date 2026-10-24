"""Create reviewable optical-flow inbetweens from registered Mika poses.

Authoring-only dependencies: numpy, Pillow, opencv-python-headless.
No image-processing dependency is imported by the Unity game.
"""
import sys
from pathlib import Path
import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "Logs/mika-tools"))
import cv2

ART = ROOT / "Assets/Game/Art/Mika"
source = Image.open(ART / "mika-movement-refined.png").convert("RGBA")
frames = [np.array(source.crop((i % 6 * 320, i // 6 * 288,
                               i % 6 * 320 + 320, i // 6 * 288 + 288))) for i in range(24)]
pairs = [(6+i, 6+(i+1) % 8) for i in range(8)]
pairs += [(14,15), (15,16), (16,17), (17,18), (18,19), (19,20),
          (20,21), (21,22), (22,23), (23,0)]
yy, xx = np.mgrid[:288, :320].astype(np.float32)

def premultiplied(frame):
    f = frame.astype(np.float32) / 255
    f[:,:,:3] *= f[:,:,3:4]
    return f

def inbetweens(first, second):
    a, b = premultiplied(first), premultiplied(second)
    ga = cv2.cvtColor((a[:,:,:3]*255).astype(np.uint8), cv2.COLOR_RGB2GRAY)
    gb = cv2.cvtColor((b[:,:,:3]*255).astype(np.uint8), cv2.COLOR_RGB2GRAY)
    algorithm = cv2.DISOpticalFlow_create(cv2.DISOPTICAL_FLOW_PRESET_MEDIUM)
    forward = algorithm.calc(ga, gb, None)
    backward = algorithm.calc(gb, ga, None)
    for t in (0.25, 0.5, 0.75):
        wa = cv2.remap(a, xx-t*forward[:,:,0], yy-t*forward[:,:,1], cv2.INTER_LINEAR)
        wb = cv2.remap(b, xx-(1-t)*backward[:,:,0], yy-(1-t)*backward[:,:,1], cv2.INTER_LINEAR)
        blend = wa*(1-t) + wb*t
        blend[:,:,:3] /= np.maximum(blend[:,:,3:4], 1/255)
        blend[blend[:,:,3] < 0.06] = 0
        yield (np.clip(blend, 0, 1)*255).astype(np.uint8)

for first, second in pairs:
    frames.extend(inbetweens(frames[first], frames[second]))
assert len(frames) == 78
sheet = Image.new("RGBA", (2560, 2880))
for i, frame in enumerate(frames):
    sheet.paste(Image.fromarray(frame), (i % 8 * 320, i // 8 * 288))
sheet.save(ART / "mika-movement-smooth.png")
print("Created 78 review frames, including 32 run-cycle phases and progressive airborne/landing poses.")
