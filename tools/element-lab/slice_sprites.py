"""Mechanical atlas slicing/resizing only; preserves generated RGBA and source PNG."""
import argparse
import hashlib
import json
from pathlib import Path
from PIL import Image

p = argparse.ArgumentParser()
p.add_argument('source', type=Path)
p.add_argument('output', type=Path)
p.add_argument('--columns', type=int, required=True)
p.add_argument('--rows', type=int, required=True)
p.add_argument('--row-cuts', help='Measured source row edges, e.g. 0,300,554,815,1024')
p.add_argument('--alpha-cutoff', type=int, default=0)
p.add_argument('--max-height', type=int, default=43)
a = p.parse_args()
im = Image.open(a.source).convert('RGBA')
if a.alpha_cutoff:
    im.putalpha(im.getchannel('A').point(lambda v: 255 if v >= a.alpha_cutoff else 0))
edges = [int(v) for v in a.row_cuts.split(',')] if a.row_cuts else [round(i*im.height/a.rows) for i in range(a.rows+1)]
assert len(edges) == a.rows+1 and edges[0] == 0 and edges[-1] == im.height
cells = []
for row in range(a.rows):
    for col in range(a.columns):
        rect = tuple(round(v) for v in (col*im.width/a.columns, edges[row], (col+1)*im.width/a.columns, edges[row+1]))
        cell = im.crop(rect)
        bounds = cell.getchannel('A').getbbox()
        if not bounds:
            raise ValueError(f'Empty frame {row},{col}')
        cells.append((rect, bounds, cell.crop(bounds)))
scale = min(44/max(c[2].width for c in cells), a.max_height/max(c[2].height for c in cells))
atlas = Image.new('RGBA', (a.columns*48, a.rows*48))
records = []
for i, (rect, bounds, body) in enumerate(cells):
    size = (max(1,round(body.width*scale)), max(1,round(body.height*scale)))
    body = body.resize(size, Image.Resampling.NEAREST)
    frame = Image.new('RGBA',(48,48))
    # Common feet baseline and centered silhouette; no frame-specific rescaling.
    frame.paste(body, ((48-size[0])//2, 46-size[1]))
    atlas.paste(frame, ((i%a.columns)*48,(i//a.columns)*48))
    records.append(dict(index=i,sourceCell=rect,sourceAlphaBounds=bounds,uniformScale=scale,opaqueBounds=frame.getchannel('A').getbbox(),sha256=hashlib.sha256(frame.tobytes()).hexdigest()))
a.output.parent.mkdir(parents=True,exist_ok=True)
atlas.save(a.output)
a.output.with_suffix('.slicing.json').write_text(json.dumps(dict(source=str(a.source),sourceSize=im.size,cell=[48,48],feetBaseline=46,pivot=[.5,2/48],frames=records),ensure_ascii=False,indent=2),encoding='utf-8')
atlas.resize((atlas.width*4,atlas.height*4),Image.Resampling.NEAREST).save(a.output.with_name(a.output.stem+'-review.png'))
print(json.dumps(dict(output=str(a.output),size=atlas.size,frames=len(records),distinct=len(set(r['sha256'] for r in records)),alphaExtrema=atlas.getchannel('A').getextrema())))
