"""Export direction-aware scripts or import reviewed recordings. No paid API calls.

python tools/visit_recordings.py export /tmp/visit-script
python tools/visit_recordings.py import /path/to/mp3s --reviewed
The explicit reviewed flag is an editorial acceptance, never set by an automated build.
"""
import argparse,csv,hashlib,json,subprocess,shutil
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]

def main():
    parser=argparse.ArgumentParser();parser.add_argument('action',choices=['export','import']);parser.add_argument('directory',type=Path);parser.add_argument('--reviewed',action='store_true');args=parser.parse_args()
    data=json.loads((ROOT/'WebApp/content/visits_de.json').read_text());lines=[l|{'direction':a['voiceDirection'],'title':a['title']} for a in data['animals'] for l in a['lines']]+[l|{'direction':'Warm, ruhig, mit natürlichen Pausen. Kein künstliches Flüstern.','title':'Gute Nacht'} for l in data['closing']]
    if args.action=='export':
        args.directory.mkdir(parents=True,exist_ok=True)
        with (args.directory/'besuchsgeschichten.csv').open('w',newline='') as f:
            writer=csv.DictWriter(f,fieldnames=['id','speaker','title','phase','text','direction','cue','pauseAfter'],extrasaction='ignore');writer.writeheader();writer.writerows(lines)
        (args.directory/'besuchsgeschichten.md').write_text('# Schlafwald – Sprecherbuch\n\n'+'\n\n'.join('## '+l['id']+' · '+l['title']+'\n\n'+l['direction']+'\n\n'+l['text'] for l in lines))
        print(f'{len(lines)} lines exported; no speech service called.');return
    if not args.reviewed:parser.error('Import requires --reviewed after listening and approving the recordings; it never approves files itself.')
    entries={}
    for l in lines:
        source=args.directory/(l['id']+'.mp3')
        if not source.is_file() or source.stat().st_size<500:raise ValueError('Recording missing or empty: '+l['id'])
        duration=float(subprocess.check_output(['ffprobe','-v','error','-show_entries','format=duration','-of','default=nw=1:nk=1',str(source)]))
        if duration<=.2:raise ValueError('Invalid duration: '+l['id'])
        entries[l['id']]={'duration':round(duration,3),'bytes':source.stat().st_size,'sha256':hashlib.sha256(source.read_bytes()).hexdigest(),'text_sha256':hashlib.sha256(l['text'].encode()).hexdigest()}
    out=ROOT/'WebApp/public/audio/visits';out.mkdir(parents=True,exist_ok=True)
    for l in lines:shutil.copyfile(args.directory/(l['id']+'.mp3'),out/(l['id']+'.mp3'))
    manifest={'version':1,'editorial_status':'approved','lines':entries};pending=out/'manifest.pending';pending.write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n');pending.replace(out/'manifest.json')
    print(f'{len(entries)} reviewed recordings imported. Add provider/license evidence to ASSET_LICENSES.md.')

if __name__=='__main__':main()
