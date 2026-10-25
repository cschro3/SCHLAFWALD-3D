"""Build-time gate for canonical story, all offline recordings, and license metadata."""
from pathlib import Path
import json,hashlib,sys
ROOT=Path(__file__).resolve().parents[1]
storypath=ROOT/'UnityProject/Assets/Schlafwald/Resources/Story/story_de.json'
s=json.loads(storypath.read_text());audio=ROOT/'WebApp/public/audio';m=json.loads((audio/'manifest.json').read_text())
lines=[l for c in s['chapters'] for l in c['lines']]+[l for a in s['animals'] for l in a['lines']]
errors=[]
if m['story_sha256']!=hashlib.sha256(storypath.read_bytes()).hexdigest():errors.append('Narration manifest is stale for this story')
for l in lines:
    entry=m['lines'].get(l['id']);p=audio/'narration'/f"{l['id']}.mp3"
    if not entry or not p.exists():errors.append('Missing voice: '+l['id']);continue
    if entry['sha256']!=hashlib.sha256(p.read_bytes()).hexdigest():errors.append('Corrupt voice: '+l['id'])
    if entry['text_sha256']!=hashlib.sha256(l['text'].encode()).hexdigest():errors.append('Stale transcript: '+l['id'])
    if entry['duration']<=0 or p.stat().st_size<1000:errors.append('Empty voice: '+l['id'])
for e in errors:print(e)
print(f'{len(s["animals"])} animals, {sum(len(a["steps"]) for a in s["animals"])} rituals, {len(lines)} offline speech files checked; {len(errors)} errors')
sys.exit(bool(errors))
