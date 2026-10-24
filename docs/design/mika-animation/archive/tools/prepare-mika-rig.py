"""Prepare generated cutout pieces, retaining the source and fixed joint lengths.

Local alpha cleanup/registration authorized by the user. Pillow/numpy/OpenCV are
authoring-only dependencies; OpenCV is isolated under ignored Logs/mika-tools.
"""
import sys
from pathlib import Path
import numpy as np
from PIL import Image
ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "Logs/mika-tools"))
import cv2
ART = ROOT / "Assets/Game/Art/Mika"
source = Image.open(ART / "mika-rig-clean-source.png").convert("RGBA")
assert source.size == (1536, 1024)
# Reviewed source regions, proximal/distal joint coordinates in source pixels.
regions = [(20,40,630,435), (635,100,1005,460), (1015,110,1220,505),
           (1330,140,1480,505), (90,525,365,975), (535,550,680,950),
           (850,680,1115,885), (1235,675,1490,885)]
joints = {2: ((1134,198),(1162,450)), 3: ((1392,210),(1402,449)),
          4: ((164,603),(267,910)), 5: ((593,618),(623,891))}
for index, rect in enumerate(regions):
    part = np.array(source.crop(rect))
    _, labels, stats, _ = cv2.connectedComponentsWithStats((part[:,:,3] > 140).astype(np.uint8))
    largest = 1 + np.argmax(stats[1:, cv2.CC_STAT_AREA])
    mask = cv2.dilate((labels == largest).astype(np.uint8), np.ones((3,3),np.uint8))
    part[:,:,3] = np.where((mask > 0) & (part[:,:,3] >= 16), part[:,:,3], 0)
    image = Image.fromarray(part)
    if index in joints:
        start, end = map(lambda p: np.array(p, dtype=float), joints[index])
        direction = (end-start) / np.linalg.norm(end-start)
        normal = np.array([direction[1], -direction[0]])
        scale = 192 / np.linalg.norm(end-start)
        width_scale = 0.67 if index in (2,4) else 0.78
        # Inverse affine: canonical root=(64,32), distal joint=(64,224).
        xaxis = normal / (scale * width_scale)
        yaxis = direction / scale
        offset = start - np.array(rect[:2]) - xaxis*64 - yaxis*32
        image = image.transform((128,256), Image.Transform.AFFINE,
            (xaxis[0], yaxis[0], offset[0], xaxis[1], yaxis[1], offset[1]),
            Image.Resampling.BICUBIC)
    image.save(ART / f"rig-part-{index:02}.png")
print("Prepared eight cutout parts; joint axes registered without runtime bone scaling.")
