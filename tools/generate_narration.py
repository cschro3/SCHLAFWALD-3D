"""Render the canonical story offline with Piper; the engine/model is build-time only.

Usage: python tools/generate_narration.py --model /secure/cache/de_DE-thorsten-high.onnx
Requires piper-tts, ffmpeg. Does not call a speech service, collect data, or need an API key.
"""
from pathlib import Path
import argparse, json, wave, tempfile, subprocess, hashlib, time, re

ROOT=Path(__file__).resolve().parents[1]
STORY=ROOT/'UnityProject/Assets/Schlafwald/Resources/Story/story_de.json'

def main():
    p=argparse.ArgumentParser();p.add_argument('--model',type=Path,required=True);p.add_argument('--only',default='');p.add_argument('--force',action='store_true');args=p.parse_args()
    from piper import PiperVoice,SynthesisConfig
    import onnxruntime as ort
    opts=ort.SessionOptions();opts.intra_op_num_threads=2;opts.inter_op_num_threads=1
    voice=PiperVoice.load(args.model)
    # Bound CPU threads in the build tool; this does not ship in the application.
    voice.session=ort.InferenceSession(str(args.model),sess_options=opts,providers=['CPUExecutionProvider'])
    story=json.loads(STORY.read_text());lines=[l for c in story['chapters'] for l in c['lines']]+[l for a in story['animals'] for l in a['lines']]
    output=ROOT/'WebApp/public/audio';(output/'narration').mkdir(parents=True,exist_ok=True)
    manifest={'version':1,'engine':'Piper (build-time only)','voice':'de_DE-thorsten-high','model_sha256':hashlib.sha256(args.model.read_bytes()).hexdigest(),'dataset_license':'CC0','model_card':'https://huggingface.co/rhasspy/piper-voices/blob/main/de/de_DE/thorsten/high/MODEL_CARD','editorial_status':'synthesized, final listening review pending','story_sha256':hashlib.sha256(STORY.read_bytes()).hexdigest(),'lines':{}}
    if args.only and (output/'manifest.json').exists():manifest['lines']=json.loads((output/'manifest.json').read_text()).get('lines',{})
    start=time.monotonic()
    for n,line in enumerate(lines):
        if args.only and not line['id'].startswith(args.only):continue
        target=output/'narration'/f"{line['id']}.mp3"
        if not target.exists() or target.stat().st_size==0 or args.force:
            with tempfile.TemporaryDirectory(prefix='schlafwald-audio-') as tmp:
                wav=Path(tmp)/'line.wav'
                with wave.open(str(wav),'wb') as f:
                    voice.synthesize_wav(line['text'].replace('…',', '),f,SynthesisConfig(length_scale=1.45 if line['tone']=='warm' else 1.52,noise_scale=.55,noise_w_scale=.65))
                with wave.open(str(wav),'rb') as f: raw_duration=f.getnframes()/f.getframerate()
                words=len(re.findall(r'\w+',line['text']))
                target_duration=max(raw_duration,words/1.8+line['text'].count('…')*.7)
                ratio=raw_duration/target_duration
                tempos=[]
                while ratio<.5: tempos.append('atempo=0.5');ratio/=.5
                tempos.append(f'atempo={ratio:.6f}')
                # Gentle fixed-gain normalization, limited peaks, actual fades; no clipping.
                effects=','.join(tempos+['loudnorm=I=-20:TP=-3:LRA=7','afade=t=in:d=0.08','areverse','afade=t=in:d=0.12','areverse'])
                encoded=Path(tmp)/'line.mp3'
                subprocess.run(['ffmpeg','-hide_banner','-loglevel','error','-y','-i',str(wav),'-af',effects,'-ac','1','-ar','24000','-codec:a','libmp3lame','-b:a','64k',str(encoded)],check=True)
                pending=target.with_suffix('.pending');pending.write_bytes(encoded.read_bytes());pending.replace(target)
        data=json.loads(subprocess.check_output(['ffprobe','-v','error','-show_entries','format=duration','-of','json',str(target)]))
        manifest['lines'][line['id']]={'duration':round(float(data['format']['duration']),3),'sha256':hashlib.sha256(target.read_bytes()).hexdigest(),'text_sha256':hashlib.sha256(line['text'].encode()).hexdigest(),'bytes':target.stat().st_size}
        print(f"{n+1}/{len(lines)} {line['id']}: {manifest['lines'][line['id']]['duration']}s",flush=True)
    pending=output/'manifest.pending';pending.write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n');pending.replace(output/'manifest.json')
    print(f"Rendered {len(manifest['lines'])} lines in {time.monotonic()-start:.1f}s",flush=True)

if __name__=='__main__':main()
