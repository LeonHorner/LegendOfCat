"""Compare unchanged in-game endpoints with added complete-character drawings."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFilter
from collections import deque
import re,json
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'docs/design/mika-animation/movement-completion/in-game-inbetweens'
atlas=Image.open(ROOT/'Assets/Game/Art/Mika/mika-movement-candidate.png').convert('RGBA')
source=Image.open(OUT/'midpoint-source.png').convert('RGBA')
assert source.size==(1774,887)
def original(i):
    if i<17:
        s=(ROOT/f'Assets/Game/Art/Mika/Original/original-frame-{i:02}.asset').read_text()
        m=re.search(r'm_Rect:\s+serializedVersion: \d+\s+x: ([\d.]+)\s+y: ([\d.]+)\s+width: ([\d.]+)\s+height: ([\d.]+)',s)
        x,y,w,h=map(float,m.groups());p=re.search(r'm_Pivot: \{x: ([\d.]+), y: ([\d.]+)\}',s);px,py=map(float,p.groups())
        return atlas.crop((round(x),round(atlas.height-y-h),round(x+w),round(atlas.height-y))),(px*w,py*h)
    tile=Image.open(ROOT/f'Assets/Game/Art/Mika/Transitions/jump-transition-{i:02}.png').convert('RGBA')
    s=(ROOT/f'Assets/Game/Art/Mika/Transitions/jump-transition-{i:02}.png.meta').read_text()
    px,py=map(float,re.search(r'spritePivot: \{x: ([\d.]+), y: ([\d.]+)\}',s).groups())
    return tile,(px*tile.width,py*tile.height)
def clean(tile):
    w,h=tile.size;a=tile.getchannel('A');points={p for p,v in enumerate(a.tobytes()) if v>166};largest=set()
    while points:
        p=points.pop();q=deque([p]);part={p}
        while q:
            p=q.popleft();x,y=p%w,p//w
            for n in (p-1 if x else -1,p+1 if x+1<w else -1,p-w if y else -1,p+w if y+1<h else -1):
                if n in points:points.remove(n);part.add(n);q.append(n)
        if len(part)>len(largest):largest=part
    mask=Image.new('L',tile.size);data=bytearray(w*h)
    for p in largest:data[p]=255
    mask.frombytes(bytes(data));mask=mask.filter(ImageFilter.MaxFilter(3))
    tile.putalpha(Image.frombytes('L',tile.size,bytes(v if m else 0 for v,m in zip(a.tobytes(),mask.tobytes()))))
    return tile,max(p//w for p in largest)
# Source-space shoulder landmarks; one scale for all new drawings.
anchors=[(305,266),(289,244),(300,252),(318,246),(314,250),(314,270),(315,280),(315,280)]
new=[]
for i,(sx,sy) in enumerate(anchors):
    row,col=divmod(i,4);tile,sole=clean(source.crop((col*1774//4,row*887//2,(col+1)*1774//4,(row+1)*887//2)))
    tile=tile.resize((round(tile.width*.72),round(tile.height*.72)),Image.Resampling.LANCZOS)
    pivot=(sx*.72-56,tile.height-sy*.72-110) if i<6 else (tile.width/2,tile.height-sole*.72)
    new.append((tile,pivot))
keys=[0,10,17,16,18,11,19,12,20,13,0]
heights=[0,35,90,125,110,65,20,0,0,0,0]
durations=[120,80,80,120,80,80,80,60,80,80,300]
midpoints={0:0,1:1,2:2,3:3,4:4,5:5,8:6,9:7}
def render(tile,pivot,height,label):
    screen=Image.new('RGB',(720,480),(28,44,52));draw=ImageDraw.Draw(screen)
    draw.rectangle((0,381,720,480),fill=(91,104,110));draw.line((0,380,720,380),fill=(177,185,178))
    screen.paste(tile,(round(370-pivot[0]),round(380-height-tile.height+pivot[1])),tile)
    draw.text((18,18),'MIKA - ORIGINAL GAME POSES + AUTHORED INBETWEENS',fill='white')
    draw.text((18,42),'Same endpoints and total duration | original four-frame run unchanged',fill=(195,205,205))
    draw.text((18,445),label,fill='white');return screen
frames=[];delays=[];before=[]
for n,key in enumerate(keys):
    tile,pivot=original(key);frame=render(tile,pivot,heights[n],f'Original game pose {key}')
    before.append(frame);frames.append(frame)
    if n in midpoints:
        delay=durations[n]//2;delays.append(delay)
        tile,pivot=new[midpoints[n]]
        frames.append(render(tile,pivot,(heights[n]+heights[n+1])/2,f'Added midpoint {midpoints[n]+1}'))
        delays.append(durations[n]-delay)
    else:delays.append(durations[n])
for name,items,times in [('review.gif',frames,delays),('review-slow.gif',frames,[v*3 for v in delays]),('before.gif',before,durations)]:
    items[0].save(OUT/name,save_all=True,append_images=items[1:],duration=times,loop=0,disposal=2)
strip=Image.new('RGB',(1440,1200),(28,44,52))
for n,frame in enumerate(frames):strip.paste(frame.resize((360,240)),(n%4*360,n//4*240))
strip.save(OUT/'review-strip.png')
(OUT/'registration.json').write_text(json.dumps({'status':'review only','endpoints':keys,'midpoints':midpoints,'newGlobalScale':.72,'shoulderLandmarks':anchors,'durationMs':sum(durations),'perPoseScale':False},indent=2)+'\n')
print('Prepared before/after loops at identical duration; no Unity changes.')
