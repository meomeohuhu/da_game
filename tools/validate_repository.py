"""Read-only consistency checks; this is not a Unity importer or graphical test."""
from pathlib import Path
from PIL import Image
import json,re,wave,hashlib
root=Path(__file__).resolve().parents[1]
for p in [root/'Packages/manifest.json',*root.glob('Assets/**/*.asmdef')]:json.loads(p.read_text())
art=root/'Assets/EcoGuardian/Resources/Art'
for p in art.glob('*.png'):
 with Image.open(p) as im:
  im.verify()
 with Image.open(p) as im:
  assert im.mode=='RGBA' and im.width>=64 and im.height>=64,p
for p in root.glob('Assets/**/Audio/*.wav'):
 with wave.open(str(p)) as w:
  assert w.getnchannels()==1 and w.getframerate()==22050 and w.getnframes()>0
font=root/'Assets/EcoGuardian/Resources/Fonts/GuardianSans.ttf'
assert font.read_bytes()[:4]==b'\0\1\0\0'
scene_guids={}
for p in root.glob('Assets/**/*.meta'):
 m=re.search(r'^guid: (\w+)',p.read_text(),re.M)
 assert m and m[1] not in scene_guids,(p,scene_guids.get(m[1]) if m else None)
 scene_guids[m[1]]=p
for p in root.glob('Assets/**/*.unity'):
 for ref in re.findall(r'guid: ([0-9a-f]{32})',p.read_text()):assert ref in scene_guids,(p,ref)
for name in ['thorn','frost','bloom','bomb','poison','ancient']:
 assert (art/(name+'.png')).is_file()
assert len(list(root.glob('Assets/EcoGuardian/Scenes/*.unity')))==3
print('PASS: JSON, artwork, WAV, TTF, metadata GUID uniqueness, scene script bindings and resource paths.')
print('Not covered: Unity import, scene deserialization, UI interaction or build execution.')
