"""Register Mika's generated 24-pose source without scaling or repainting anatomy.

User authorized local alpha cleanup and paw registration on 2026-10-23.
Requires Pillow. Run from the repository root with Python.
"""
from collections import deque
from pathlib import Path
from PIL import Image, ImageFilter

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / "Assets/Game/Art/Mika"
source = Image.open(ART / "mika-movement-refined-source.png").convert("RGBA")
assert source.size == (1536, 1024), "Review source coordinates before changing artwork"
top = [48, 300, 520, 772]
bottom = [292, 512, 756, 1008]
air_torso = {14: 130, 15: 95, 16: 80, 17: 120, 18: 88, 19: 98}
sheet = Image.new("RGBA", (1920, 1152))

for frame in range(24):
    row, column = divmod(frame, 6)
    sprite = source.crop((column * 256, top[row], (column + 1) * 256, bottom[row]))
    alpha = sprite.getchannel("A")
    w, h = sprite.size
    opaque = bytearray(v >= 160 for v in alpha.tobytes())
    largest = []
    for start in range(w * h):
        if not opaque[start]:
            continue
        opaque[start] = 0
        queue, component = deque([start]), []
        while queue:
            pixel = queue.popleft()
            component.append(pixel)
            x, y = pixel % w, pixel // w
            for neighbor in (pixel - 1 if x else -1, pixel + 1 if x + 1 < w else -1,
                             pixel - w if y else -1, pixel + w if y + 1 < h else -1):
                if neighbor >= 0 and opaque[neighbor]:
                    opaque[neighbor] = 0
                    queue.append(neighbor)
        if len(component) > len(largest):
            largest = component
    assert len(largest) > 5000, f"Missing cat silhouette in frame {frame}"
    mask = Image.new("L", (w, h))
    values = bytearray(w * h)
    for pixel in largest:
        values[pixel] = 255
    mask.frombytes(bytes(values))
    # Retain antialiased silhouette edges, discard unrelated low-alpha background specks.
    mask = mask.filter(ImageFilter.MaxFilter(5))
    clean = bytes(a if m and a >= 16 else 0 for a, m in zip(alpha.tobytes(), mask.tobytes()))
    sprite.putalpha(Image.frombytes("L", (w, h), clean))
    lowest_sole = max(pixel // w for pixel in largest)
    offset_y = 172 - air_torso[frame] if frame in air_torso else 250 - lowest_sole
    bounds = sprite.getbbox()
    assert offset_y + bounds[1] >= 0 and offset_y + bounds[3] <= 288, f"Insufficient padding for frame {frame}"
    sheet.paste(sprite, (column * 320 + 32, row * 288 + offset_y))

sheet.save(ART / "mika-movement-refined.png")
print("Registered 24 unscaled poses; grounded soles at y=250; airborne torso anchors preserved.")
