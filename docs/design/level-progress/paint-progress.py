from PIL import Image, ImageDraw, ImageFilter
from pathlib import Path
import argparse
import random
parser = argparse.ArgumentParser(description="Texture the transparent PhotoCraft exports.")
parser.add_argument("exports", type=Path, help="Folder containing the original transparent PNGs")
args = parser.parse_args()
root=Path('Assets/Game/Resources/LevelProgress')
for path in sorted(root.glob('*-fill.png')):
    original=Image.open(args.exports / path.name).convert('RGBA')
    w,h=original.size
    rng=random.Random(path.name)
    enlarged=original.resize((w*3,h*3),Image.Resampling.BICUBIC)
    pixels=enlarged.load()
    paper=Image.new('L',enlarged.size)
    paper.putdata([rng.randrange(256) for _ in range(w*h*9)])
    grain=paper.filter(ImageFilter.GaussianBlur(1.4)).load()
    for y in range(h*3):
        for x in range(w*3):
            dx=rng.choice([-2,-1,0,0,0,1,2])
            r,g,b,a=enlarged.getpixel((max(0,min(w*3-1,x+dx)),y))
            pigment=(grain[x,y]-128)*.58+rng.gauss(0,4)
            pixels[x,y]=tuple(max(0,min(255,round(c+pigment*m))) for c,m in zip((r,g,b),(1,.84,.75)))+(a,)
    if 'track' in path.name:
        brush=Image.new('RGBA',enlarged.size)
        d=ImageDraw.Draw(brush)
        golden=path.name.startswith('golden')
        colors=[(196,149,153,105),(237,199,160,95),(62,43,69,160)] if golden else [(131,168,139,145),(224,205,157,125),(36,76,74,165)]
        for i in range(160):
            x=rng.randrange(20,w*3-20)
            y=rng.choice([rng.randrange(8,21),rng.randrange(56,78)])
            length=rng.randrange(3,36)
            d.line([(x,y),(x+length//2,y+rng.randrange(-3,4)),(x+length,y+rng.randrange(-4,5))],fill=rng.choice(colors),width=rng.randrange(1,5))
        for x in (24,53,790,818):
            for _ in range(13):
                y=rng.randrange(54,78); dx=rng.randrange(-18,19)
                d.ellipse((x+dx,y,x+dx+rng.randrange(4,15),y+rng.randrange(3,9)),fill=rng.choice([(72,112,91,180),(129,154,102,185),(178,172,103,170)]))
        enlarged=Image.alpha_composite(enlarged,brush)
    elif 'fill' in path.name:
        brush=Image.new('RGBA',enlarged.size)
        d=ImageDraw.Draw(brush)
        for _ in range(90):
            x=rng.randrange(6,w*3-8); y=rng.randrange(6,24)
            d.line((x,y,min(w*3-6,x+rng.randrange(4,40)),y+rng.choice([-1,0,1])),fill=(255,241,200,rng.randrange(20,90)),width=rng.randrange(1,4))
        enlarged=Image.alpha_composite(enlarged,brush)
    enlarged.putalpha(original.getchannel('A').resize(enlarged.size,Image.Resampling.BICUBIC))
    enlarged.resize((w,h),Image.Resampling.LANCZOS).save(path)
