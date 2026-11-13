"""Package actual rendered frames into normal/slowed review loops."""
from pathlib import Path
import sys
from PIL import Image, ImageDraw, ImageOps

OUT=Path(__file__).resolve().parent
CACHE=Path('/tmp/mika-authoring/review-frames')
clips=[('Run',.5),('Idle',4),('Groom',6),('Stretch',6),('LookBack',6),('Listen',4),('TailTwitch',4),('JumpReview',3)]
# Reuse the other review tiles when only the running clips were rendered.
run_only = '--run-only' in sys.argv
if run_only:
    clips = clips[:1]
sheet = Image.open(OUT/'motion-review-sheet.png').convert('RGB') if run_only else Image.new('RGB',(960,872),'#20252a')
for index,(name,step) in enumerate(clips):
    frames=[Image.open(p).convert('RGB') for p in sorted((CACHE/name).glob('*.png'))]
    expected={'Run':24,'Idle':12,'Groom':16,'Stretch':12,'LookBack':12,'Listen':12,'TailTwitch':12,'JumpReview':16}[name]
    assert len(frames)==expected,(name,len(frames),expected)
    for suffix,multiplier in [('normal',1),('slow',3)]:
        # GIF time is quantized to 10 ms. Distribute the rounding error so the
        # complete loop keeps its authored duration rather than speeding up.
        ticks=[round(i*100*step/24*multiplier)*10 for i in range(len(frames)+1)]
        durations=[b-a for a,b in zip(ticks,ticks[1:])]
        prefix='run-v4' if name=='Run' else name.lower()
        frames[0].save(OUT/f'{prefix}-{suffix}.gif',save_all=True,append_images=frames[1:],duration=durations,loop=0,optimize=False)
    representative=frames[len(frames)//2].resize((240,200))
    col=index%4;row=index//4
    sheet.paste(representative,(col*240,row*218+18))
    ImageDraw.Draw(sheet).text((col*240+8,row*218+3),name,fill='white')
# A second row at small grayscale scale helps review silhouette readability.
for index,(name,_) in enumerate(clips):
    frame=Image.open(sorted((CACHE/name).glob('*.png'))[len(list((CACHE/name).glob('*.png')))//2])
    thumb=ImageOps.grayscale(frame).convert('RGB').resize((160,133))
    sheet.paste(thumb,((index%4)*240+40,454+(index//4)*180))
    ImageDraw.Draw(sheet).text(((index%4)*240+8,438+(index//4)*180),name+' / small grayscale',fill='white')
sheet.save(OUT/'motion-review-sheet.png')
frames=[Image.open(p).convert('RGB') for p in sorted((CACHE/'RunOblique').glob('*.png'))]
assert len(frames)==24
for suffix,multiplier in [('normal',1),('slow',3)]:
    ticks=[round(i*100/48*multiplier)*10 for i in range(25)]
    frames[0].save(OUT/f'run-oblique-{suffix}.gif',save_all=True,append_images=frames[1:],duration=[b-a for a,b in zip(ticks,ticks[1:])],loop=0,optimize=False)
print('Review loops and contact sheet assembled from Blender frames.')
