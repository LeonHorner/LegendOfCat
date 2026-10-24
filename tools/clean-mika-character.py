"""Remove disconnected alpha specks from the original character sheet.

Preserves every retained pixel's RGB and alpha, dimensions, grid, scale and poses.
The byte-identical original and .meta are archived before the first cleanup.
"""
from pathlib import Path
from collections import deque
from PIL import Image, ImageFilter
import hashlib, json, shutil

ROOT=Path(__file__).resolve().parents[1]
ARCHIVE=ROOT/'docs/design/mika-animation/archive'
SOURCE=ROOT/'Assets/Game/Art/Mika/mika-movement-candidate.png'
manifest=json.loads((ARCHIVE/'manifest.json').read_text())
for path in [SOURCE,Path(str(SOURCE)+'.meta')]:
    target=ARCHIVE/path.relative_to(ROOT)
    if not target.exists():
        target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(path,target)
        digest=hashlib.sha256(target.read_bytes()).hexdigest()
        assert digest==hashlib.sha256(path.read_bytes()).hexdigest()
        manifest.append({'original':path.relative_to(ROOT).as_posix(),'sha256':digest,'operation':'snapshot'})
(ARCHIVE/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
original=Image.open(ARCHIVE/SOURCE.relative_to(ROOT)).convert('RGBA')
result=original.copy();removed=0
for row in range(4):
    for col in range(4):
        box=(col*original.width//4,row*original.height//4,(col+1)*original.width//4,(row+1)*original.height//4)
        tile=original.crop(box);w,h=tile.size;alpha=tile.getchannel('A')
        points={i for i,a in enumerate(alpha.tobytes()) if a>166};largest=set()
        while points:
            start=points.pop();q=deque([start]);part={start}
            while q:
                p=q.popleft();x,y=p%w,p//w
                for n in (p-1 if x else -1,p+1 if x+1<w else -1,p-w if y else -1,p+w if y+1<h else -1):
                    if n in points:points.remove(n);part.add(n);q.append(n)
            if len(part)>len(largest):largest=part
        assert len(largest)>5000
        mask=Image.new('L',tile.size);data=bytearray(w*h)
        for p in largest:data[p]=255
        mask.frombytes(bytes(data));mask=mask.filter(ImageFilter.MaxFilter(5))
        cleaned=bytes(a if m and a>=16 else 0 for a,m in zip(alpha.tobytes(),mask.tobytes()))
        removed+=sum(a>0 and b==0 for a,b in zip(alpha.tobytes(),cleaned))
        tile.putalpha(Image.frombytes('L',tile.size,cleaned));result.paste(tile,box[:2])
assert result.convert('RGB').tobytes()==original.convert('RGB').tobytes(), 'Cleanup must never repaint the cat'
assert all(a==b or b==0 for a,b in zip(original.getchannel('A').tobytes(),result.getchannel('A').tobytes()))
result.save(SOURCE)
print(f'Cleaned disconnected alpha in 16 source cells; {removed} background/low-alpha pixels cleared; drawings not repainted.')
