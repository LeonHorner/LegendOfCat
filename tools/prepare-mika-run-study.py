"""Register complete-character drawings and render normal/slow review loops.

No optical flow, skeletal assembly, pose deformation, crossfade or frame scaling.
Local cleanup/registration uses the user's existing authorization. Requires Pillow.
"""
from collections import deque
from pathlib import Path
import json
from PIL import Image, ImageDraw, ImageFilter

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "docs/design/mika-animation/frame-run-study"
TEMP = ROOT / "Logs/mika-run-study"
TEMP.mkdir(parents=True, exist_ok=True)
source = Image.open(OUT / "run-source-v1.png").convert("RGBA")
assert source.size == (1536,1024), "Review crop layout before accepting another source"
frames, measurements = [], []
for index in range(16):
    row, col = divmod(index,4)
    part = source.crop((col*384,row*256,(col+1)*384,(row+1)*256))
    alpha = part.getchannel("A")
    w,h = part.size
    solid = bytearray(v >= 160 for v in alpha.tobytes())
    largest = []
    for start in range(w*h):
        if not solid[start]: continue
        solid[start] = 0
        queue, component = deque([start]), []
        while queue:
            p = queue.popleft(); component.append(p)
            x,y = p%w,p//w
            for n in (p-1 if x else -1,p+1 if x+1<w else -1,p-w if y else -1,p+w if y+1<h else -1):
                if n >= 0 and solid[n]: solid[n] = 0; queue.append(n)
        if len(component)>len(largest): largest=component
    assert len(largest)>10000, f"Incomplete silhouette in frame {index}"
    mask = bytearray(w*h)
    for p in largest: mask[p]=255
    mask_image=Image.frombytes("L",(w,h),bytes(mask)).filter(ImageFilter.MaxFilter(3))
    cleaned=bytes(a if m and a>=16 else 0 for a,m in zip(alpha.tobytes(),mask_image.tobytes()))
    part.putalpha(Image.frombytes("L",(w,h),cleaned))
    # Match the nose (upper silhouette) and supporting sole without changing proportions.
    nose=max(p%w for p in largest if p//w<150)
    sole=max(p//w for p in largest)
    dx,dy=382-nose,254-sole
    frame=Image.new("RGBA",(448,288))
    frame.paste(part,(dx,dy))
    assert 0<=dx+part.getbbox()[0] and dx+part.getbbox()[2]<448
    frame.save(TEMP/f"run-frame-{index:02}.png")
    frames.append(frame)
    measurements.append({"frame":index,"translation":[dx,dy],"source_support_y":sole,"source_nose_x":nose})

(OUT/"registration.json").write_text(json.dumps(measurements,indent=2)+"\n",encoding="utf-8")
atlas=Image.new("RGBA",(1792,1152))
for i,f in enumerate(frames): atlas.paste(f,(i%4*448,i//4*288))
atlas.save(OUT/"run-registered.png")

review=[]
for i,f in enumerate(frames):
    screen=Image.new("RGB",(720,480),(28,44,52))
    screen.paste(f,(136,126),f)
    draw=ImageDraw.Draw(screen)
    draw.rectangle((0,381,720,480),fill=(91,104,110))
    draw.line((0,380,720,380),fill=(177,185,178))
    draw.text((20,20),"MIKA - COMPLETE-CHARACTER RUN STUDY",fill="white")
    draw.text((20,43),"Review drawings and loop continuity before gameplay integration",fill=(195,205,205))
    draw.text((20,445),f"Pose {i+1:02}/16 | original full-character images | no morphing",fill="white")
    screen.save(TEMP/f"review-frame-{i:02}.png")
    review.append(screen)
review[0].save(OUT/"run-normal.gif",save_all=True,append_images=review[1:],duration=40,loop=0,disposal=2)
review[0].save(OUT/"run-slow.gif",save_all=True,append_images=review[1:],duration=120,loop=0,disposal=2)
sheet=Image.new("RGB",(1440,960))
for i,f in enumerate(review): sheet.paste(f.resize((360,240)),(i%4*360,i//4*240))
sheet.save(OUT/"review-contact-sheet.png")
print("Prepared 16 full-character poses; translation/alpha cleanup only; normal and slow loops saved.")
