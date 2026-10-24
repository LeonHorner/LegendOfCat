"""A review-only revision after the user's abrupt-transition rejection.

Full-character drawings, translation and uniform scale only. Does not write Assets.
"""
from pathlib import Path
from PIL import Image, ImageDraw
import runpy,json

ROOT=Path(__file__).resolve().parents[1]
base=runpy.run_path(str(ROOT/'tools/prepare-mika-transition-review.py'))
OUT=ROOT/'docs/design/mika-animation/movement-completion/revision-2'
source=Image.open(OUT/'transition-source.png').convert('RGBA')
assert source.size==(1536,1024)
drawings,soles,shoulders=base['drawings'],base['soles'],base['shoulders']
anchors=[(610,300),(600,290),(610,240),(610,245)]
for i,anchor in enumerate(anchors):
    x,y=i%2*768,i//2*512
    tile,sole=base['clean'](source.crop((x,y,x+768,y+512)))
    tile=tile.resize((369,246),Image.Resampling.LANCZOS)
    key=21+i;drawings[key]=tile;soles[key]=round(sole*0.48)
    shoulders[key]=tuple(round(v*0.48) for v in anchor)
original=base['original'];w,h=original.width/4,original.height/4
drawings[14],soles[14]=base['clean'](original.crop((int(2*w),int(3*h),int(3*w),int(4*h))))
shoulders[14]=(240,130)

sequence=[(0,'Idle',0,250),(21,'Shallow takeoff',35,80),(22,'Shallow ascent',90,80),
 (16,'Apex',125,120),(18,'Early descent',110,80),(11,'Descent',65,80),(19,'Reach',20,80),
 (12,'Contact',0,50),(20,'Absorb',0,50),(23,'Rise 1',0,60),(24,'Rise 2',0,60),
 (13,'Settle 1',0,60),(14,'Settle 2',0,60),(0,'Idle',0,350)]
frames=[];delays=[]
for key,label,height,delay in sequence:
    canvas=Image.new('RGB',(720,480),(28,44,52));draw=ImageDraw.Draw(canvas)
    draw.rectangle((0,381,720,480),fill=(91,104,110));draw.line((0,380,720,380),fill=(177,185,178))
    tile=drawings[key];sx,sy=shoulders[key]
    if height==0:
        # Register the forward supporting toe through recovery; do not stretch anatomy.
        alpha=tile.getchannel('A');support=[x for y in range(max(0,soles[key]-2),min(tile.height,soles[key]+1)) for x in range(tile.width) if alpha.getpixel((x,y))>166]
        x=480-max(support);y=380-soles[key]
    else:x=420-sx;y=260-height-sy
    canvas.paste(tile,(x,y),tile)
    draw.text((20,20),'MIKA - TRANSITION REVISION 2 | REVIEW ONLY',fill='white')
    draw.text((20,43),'Shallower takeoff; more recovery drawings; original run unchanged',fill=(195,205,205))
    draw.text((20,445),label,fill='white');frames.append(canvas);delays.append(delay)
frames[0].save(OUT/'review.gif',save_all=True,append_images=frames[1:],duration=delays,loop=0,disposal=2)
frames[0].save(OUT/'review-slow.gif',save_all=True,append_images=frames[1:],duration=[v*3 for v in delays],loop=0,disposal=2)
sheet=Image.new('RGB',(1440,960),(28,44,52))
for i,frame in enumerate(frames):sheet.paste(frame.resize((360,240)),(i%4*360,i//4*240))
sheet.save(OUT/'review-strip.png')
(OUT/'registration.json').write_text(json.dumps({'status':'review only; not integrated','scale':0.48,'shoulderAnchors':anchors,'sequence':sequence,'groundAnchor':'forward supporting toe'},indent=2)+'\n')
print('Revision 2 review prepared; Unity assets and current playtest unchanged.')
