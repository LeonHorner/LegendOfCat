"""Small legacy-inspired style/motion example; documentation only, no Unity export."""
from pathlib import Path
from collections import deque
from PIL import Image, ImageDraw, ImageFilter
import json

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'docs/design/mika-animation/movement-completion/legacy-cute-example'
source=Image.open(OUT/'transition-source.png').convert('RGBA')
assert source.size==(1774,887)
SCALE=0.5
anchors=[(215,330),(215,330),(215,300),(230,215),(215,245),(215,295),(215,310),(215,315)]
tiles=[];soles=[]
for i in range(8):
    row,col=divmod(i,4)
    tile=source.crop((col*1774//4,row*887//2,(col+1)*1774//4,(row+1)*887//2))
    w,h=tile.size;alpha=tile.getchannel('A');points={p for p,a in enumerate(alpha.tobytes()) if a>166};largest=set()
    while points:
        start=points.pop();q=deque([start]);part={start}
        while q:
            p=q.popleft();x,y=p%w,p//w
            for n in (p-1 if x else -1,p+1 if x+1<w else -1,p-w if y else -1,p+w if y+1<h else -1):
                if n in points:points.remove(n);part.add(n);q.append(n)
        if len(part)>len(largest):largest=part
    assert len(largest)>10000
    sole=max(p//w for p in largest);mask=Image.new('L',tile.size);data=bytearray(w*h)
    for p in largest:data[p]=255
    mask.frombytes(bytes(data));mask=mask.filter(ImageFilter.MaxFilter(3))
    tile.putalpha(Image.frombytes('L',tile.size,bytes(a if m and a>=16 else 0 for a,m in zip(alpha.tobytes(),mask.tobytes()))))
    tiles.append(tile.resize((round(w*SCALE),round(h*SCALE)),Image.Resampling.NEAREST));soles.append(round(sole*SCALE))

sequence=[(0,'Stand',0,250),(1,'Load / look up',0,80),(2,'Rise / head up',65,100),
          (3,'Curl / look toward landing',115,120),(4,'Reach / head down',60,100),
          (5,'Contact / absorb',0,90),(6,'Recover',0,100),(7,'Settle',0,100),(0,'Stand',0,350)]
frames=[];durations=[]
for i,label,height,delay in sequence:
    screen=Image.new('RGB',(640,420),(29,44,53));draw=ImageDraw.Draw(screen)
    draw.rectangle((0,341,640,420),fill=(84,111,104));draw.line((0,340,640,340),fill=(191,207,161),width=2)
    ax,ay=(round(v*SCALE) for v in anchors[i])
    y=340-soles[i] if height==0 else 288-height-ay
    tile=tiles[i];screen.paste(tile,(325-ax,y),tile)
    draw.text((18,18),'MIKA - CUTE LEGACY-INSPIRED MOTION EXAMPLE',fill='white')
    draw.text((18,40),'Curved posture, head pitch and gaze follow the jump | review only',fill=(197,209,207))
    draw.text((18,390),label,fill='white');frames.append(screen);durations.append(delay)
for name,times in [('review.gif',durations),('review-slow.gif',[d*3 for d in durations])]:
    frames[0].save(OUT/name,save_all=True,append_images=frames[1:],duration=times,loop=0,disposal=2)
sheet=Image.new('RGB',(1280,630),(29,44,53))
for n,frame in enumerate(frames):sheet.paste(frame.resize((320,210)),(n%4*320,n//4*210))
sheet.save(OUT/'review-strip.png')
(OUT/'registration.json').write_text(json.dumps({'status':'small style/motion example only; not integrated','globalScale':SCALE,'resize':'nearest-neighbor, same factor in all poses','bodyAnchors':anchors,'sequence':sequence,'motion':'drawn spine/hip/neck articulation; no runtime warping'},indent=2)+'\n')
print('Prepared cute legacy-inspired example and slowed loop. No game assets changed.')
