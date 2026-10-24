"""Review complete-character jump drawings before any Unity integration.

Uses the user's local alpha/registration authorization. Uniform scale and
translation only: no optical flow, rig, stretching or crossfade. Requires Pillow.
"""
from pathlib import Path
from collections import deque
from PIL import Image, ImageDraw, ImageFilter
import json
import sys

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'docs/design/mika-animation/movement-completion'
original = Image.open(ROOT/'Assets/Game/Art/Mika/mika-movement-candidate.png').convert('RGBA')
source = Image.open(OUT/'jump-transitions-source.png').convert('RGBA')
assert source.size == (1536, 1024)

def clean(tile):
    w,h=tile.size;alpha=tile.getchannel('A')
    points={i for i,a in enumerate(alpha.tobytes()) if a>166};largest=set()
    while points:
        start=points.pop();queue=deque([start]);part={start}
        while queue:
            p=queue.popleft();x,y=p%w,p//w
            for n in (p-1 if x else -1,p+1 if x+1<w else -1,p-w if y else -1,p+w if y+1<h else -1):
                if n in points: points.remove(n);part.add(n);queue.append(n)
        if len(part)>len(largest): largest=part
    assert len(largest)>5000
    mask=Image.new('L',tile.size);data=bytearray(w*h)
    for p in largest:data[p]=255
    mask.frombytes(bytes(data));mask=mask.filter(ImageFilter.MaxFilter(3))
    tile.putalpha(Image.frombytes('L',tile.size,bytes(a if m else 0 for a,m in zip(alpha.tobytes(),mask.tobytes()))))
    sole=max(p//w for p in largest)
    return tile,sole

# Explicit crop boundaries preserve the lower-left drawing beyond the nominal grid.
rects=[(0,0,768,512),(768,0,1536,512),(0,512,810,1024),(810,512,1536,1024)]
new_shoulders=[(520,300),(562,300),(620,238),(540,248)]
drawings={};soles={};shoulders={}
for i,rect in enumerate(rects):
    tile,sole=clean(source.crop(rect));factor=0.48
    tile=tile.resize((round(tile.width*factor),round(tile.height*factor)),Image.Resampling.LANCZOS)
    key=17+i;drawings[key]=tile;soles[key]=round(sole*factor)
    shoulders[key]=tuple(round(v*factor) for v in new_shoulders[i])

old_shoulders={0:(240,130),10:(220,145),16:(245,140),11:(255,155),12:(260,150),13:(240,150)}
for key,shoulder in old_shoulders.items():
    index=6 if key==16 else key;w,h=original.width/4,original.height/4
    tile,sole=clean(original.crop((int(index%4*w),int(index//4*h),int((index%4+1)*w),int((index//4+1)*h))))
    drawings[key]=tile;soles[key]=sole;shoulders[key]=shoulder

sequence=[(0,'Idle',0,250),(10,'Takeoff',35,80),(17,'Late ascent',90,80),
          (16,'Apex',125,120),(18,'Early descent',110,80),(11,'Descent',65,80),
          (19,'Pre-contact',20,80),(12,'Contact / absorb',0,80),
          (20,'Recovery',0,80),(13,'Settle',0,80),(0,'Idle',0,350)]
review=[];delays=[]
for key,label,height,delay in sequence:
    canvas=Image.new('RGB',(720,480),(28,44,52));draw=ImageDraw.Draw(canvas)
    draw.rectangle((0,381,720,480),fill=(91,104,110));draw.line((0,380,720,380),fill=(177,185,178))
    tile=drawings[key];sx,sy=shoulders[key]
    y=380-soles[key] if height==0 else 380-120-height-sy
    canvas.paste(tile,(420-sx,y),tile)
    draw.text((20,20),'MIKA - JUMP / LANDING TRANSITION REVIEW',fill='white')
    draw.text((20,43),'Complete drawings; review before integration; run poses unchanged',fill=(195,205,205))
    draw.text((20,445),label+(' | NEW DRAWING' if key>=17 else ' | original drawing'),fill='white')
    review.append(canvas);delays.append(delay)
review[0].save(OUT/'jump-review.gif',save_all=True,append_images=review[1:],duration=delays,loop=0,disposal=2)
review[0].save(OUT/'jump-review-slow.gif',save_all=True,append_images=review[1:],duration=[d*3 for d in delays],loop=0,disposal=2)
sheet=Image.new('RGB',(1440,720),(28,44,52))
for i,f in enumerate(review):sheet.paste(f.resize((360,240)),((i%4)*360,(i//4)*240))
sheet.save(OUT/'jump-review-strip.png')
(OUT/'registration.json').write_text(json.dumps({'newCropRects':rects,'newUniformScale':0.48,'newShoulders':new_shoulders,'oldShoulders':old_shoulders,'sequence':sequence,'status':'review only; not Unity assets'},indent=2)+'\n')
if '--export-assets' in sys.argv:
    asset_dir=ROOT/'Assets/Game/Art/Mika/Transitions'
    asset_dir.mkdir(exist_ok=True)
    for key in range(17,21):
        tile=drawings[key];sx,sy=shoulders[key]
        canvas=Image.new('RGBA',(512,288))
        y=250-soles[key] if key==20 else 160-sy
        canvas.paste(tile,(312-sx,y))
        bounds=canvas.getbbox()
        assert bounds and bounds[0]>0 and bounds[1]>0 and bounds[2]<512 and bounds[3]<288
        canvas.save(asset_dir/f'jump-transition-{key:02}.png')
print('Prepared transition review'+(' and four registered Unity images.' if '--export-assets' in sys.argv else '; no Unity asset changes.'))
