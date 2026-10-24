"""Remove disconnected alpha islands only; retain the complete connected drawing."""
from pathlib import Path
from collections import deque
from PIL import Image
import json
ROOT=Path(__file__).resolve().parents[1]
report=[]
for path in sorted((ROOT/'Assets/Game/Art/Mika/Transitions').glob('*.png')):
    im=Image.open(path).convert('RGBA');w,h=im.size;a=im.getchannel('A');raw=a.tobytes()
    points={i for i,v in enumerate(raw) if v>0};largest=set();components=[]
    while points:
        p=points.pop();q=deque([p]);part={p}
        while q:
            p=q.popleft();x,y=p%w,p//w
            # Preserve diagonal antialias connections along the silhouette.
            for dy in [-1,0,1]:
                for dx in [-1,0,1]:
                    if 0<=x+dx<w and 0<=y+dy<h:
                        n=(y+dy)*w+x+dx
                        if n in points:points.remove(n);part.add(n);q.append(n)
        components.append(part)
        if len(part)>len(largest):largest=part
    assert len(largest)>10000
    removed=sum(len(c) for c in components if c is not largest)
    if removed:
        cleaned=bytes(v if i in largest else 0 for i,v in enumerate(raw))
        rgb=im.convert('RGB').tobytes();im.putalpha(Image.frombytes('L',im.size,cleaned))
        assert im.convert('RGB').tobytes()==rgb
        im.save(path)
    report.append({'file':path.name,'removedDisconnectedPixels':removed,'retainedComponentPixels':len(largest)})
print(json.dumps(report))
