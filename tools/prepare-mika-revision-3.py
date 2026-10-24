"""One coherent sheet, one global scale; no per-pose height normalization.

Review artifacts only. Local alpha cleanup/registration uses existing user approval.
"""
from pathlib import Path
from collections import deque
from PIL import Image, ImageDraw, ImageFilter
import json

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'docs/design/mika-animation/movement-completion/revision-3'
source=Image.open(OUT/'transition-source.png').convert('RGBA')
assert source.size==(1774,887)
SCALE=0.72
# Manually identified shoulder centers; translation changes location, never scale.
shoulders=[(319,278),(326,275),(326,257),(329,242),(326,250),(329,295),(329,276),(329,263)]
eye_y=[183,182,181,180,177,191,183,173]
tiles=[];soles=[];measurements=[]
for i in range(8):
    row,col=divmod(i,4)
    rect=(col*1774//4,row*887//2,(col+1)*1774//4,(row+1)*887//2)
    tile=source.crop(rect);w,h=tile.size;alpha=tile.getchannel('A')
    points={p for p,a in enumerate(alpha.tobytes()) if a>166};largest=set()
    while points:
        start=points.pop();queue=deque([start]);part={start}
        while queue:
            p=queue.popleft();x,y=p%w,p//w
            for n in (p-1 if x else -1,p+1 if x+1<w else -1,p-w if y else -1,p+w if y+1<h else -1):
                if n in points:points.remove(n);part.add(n);queue.append(n)
        if len(part)>len(largest):largest=part
    assert len(largest)>10000
    sole=max(p//w for p in largest);mask=Image.new('L',tile.size);values=bytearray(w*h)
    for p in largest:values[p]=255
    mask.frombytes(bytes(values));mask=mask.filter(ImageFilter.MaxFilter(3))
    tile.putalpha(Image.frombytes('L',tile.size,bytes(a if m and a>=16 else 0 for a,m in zip(alpha.tobytes(),mask.tobytes()))))
    sx,sy=shoulders[i]
    # Regional occupancy is diagnostic only; it is not a rescaling target.
    head=sum(1 for p in largest if 330<=p%w<440 and eye_y[i]-65<=p//w<eye_y[i]+45)
    torso=sum(1 for p in largest if 184<=p%w<319 and sy-50<=p//w<sy+30)
    measurements.append({'pose':i,'sourceRect':rect,'shoulder':shoulders[i],'scale':SCALE,'headRegionOpaquePixels':head,'torsoRegionOpaquePixels':torso})
    tiles.append(tile.resize((round(w*SCALE),round(h*SCALE)),Image.Resampling.LANCZOS));soles.append(round(sole*SCALE))

sequence=[(0,'Stand',0,250),(1,'Toe-off',45,90),(2,'Ascent',100,100),(3,'Apex',130,120),
          (4,'Descent',75,100),(5,'Contact',0,90),(6,'Recovery',0,100),(7,'Settle',0,100),(0,'Stand',0,350)]
review=[];diagnostic=[];delays=[]
for i,label,height,delay in sequence:
    canvas=Image.new('RGB',(720,480),(28,44,52));draw=ImageDraw.Draw(canvas)
    draw.rectangle((0,381,720,480),fill=(91,104,110));draw.line((0,380,720,380),fill=(177,185,178))
    sx,sy=(round(v*SCALE) for v in shoulders[i]);tile=tiles[i]
    y=380-soles[i] if height==0 else 282-height-sy
    canvas.paste(tile,(430-sx,y),tile)
    draw.text((20,20),'MIKA - PROPORTION REVIEW 3',fill='white')
    draw.text((20,43),'One sheet, one scale for every pose; tucked legs reduce silhouette height',fill=(195,205,205))
    draw.text((20,445),label,fill='white');review.append(canvas);delays.append(delay)
    check=Image.new('RGB',(720,480),(28,44,52));check.paste(tile,(430-sx,240-sy),tile)
    pen=ImageDraw.Draw(check);pen.text((20,20),'SIZE CHECK - FIXED SHOULDER / IDENTICAL GLOBAL SCALE',fill='white')
    pen.text((20,43),label+' | no jump trajectory | no per-frame resize',fill=(195,205,205))
    pen.line((328,240,430,240),fill=(170,190,190),width=1)
    diagnostic.append(check)
for filename,frames,times in [('review.gif',review,delays),('review-slow.gif',review,[v*3 for v in delays]),('size-check.gif',diagnostic,[300]*len(diagnostic))]:
    frames[0].save(OUT/filename,save_all=True,append_images=frames[1:],duration=times,loop=0,disposal=2)
sheet=Image.new('RGB',(1440,720),(28,44,52))
for n,frame in enumerate(review):sheet.paste(frame.resize((360,240)),(n%4*360,n//4*240))
sheet.save(OUT/'review-strip.png')
(OUT/'registration.json').write_text(json.dumps({'status':'review only; not integrated','globalScale':SCALE,'perPoseScaleAdjustment':False,'sequence':sequence,'measurements':measurements,'measurementLimit':'Regional opaque occupancy diagnoses gross size changes; not exact anatomical metrology.'},indent=2)+'\n')
for field in ['headRegionOpaquePixels','torsoRegionOpaquePixels']:
    values=[m[field] for m in measurements]
    print(field,min(values),max(values),'range ratio',round(max(values)/min(values),3))
print('Prepared one-scale review, slow loop and fixed-shoulder size check. Unity unchanged.')
