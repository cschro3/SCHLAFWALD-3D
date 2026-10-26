import {estimate,clamp} from './core.js';
import {germanLocalVoices} from './visits.js';

// No runtime speech service, microphone permission, analytics, or remote request.
// Legacy clips remain bundled. New visits use local German device voices until approved recordings exist.
export class ForestAudio {
  constructor(settings){this.settings=settings;this.coverage={};this.voice=new Audio();this.voice.preload='auto';this.voice.playsInline=true;this.system=globalThis.speechSynthesis;this.system?.addEventListener('voiceschanged',()=>this.onVoicesChanged?.());this.source='none';this.mutedForQA=false;this.paused=false;this.failures=new Set();this.noteAt=0;this.notes=[196,220,261.63,329.63,293.66,261.63,220,196];}
  async init(){
    try{const r=await fetch('./audio/manifest.json');if(r.ok)this.coverage=await r.json();}catch{/* visible read-aloud fallback */}
    try{const r=await fetch('./audio/visits/manifest.json');if(r.ok)this.visitCoverage=await r.json();}catch{/* device voice remains available offline */}
  }
  async unlock(){
    this.paused=false;this.system?.resume();
    if(this.ctx){await this.ctx.resume().catch(()=>{});return;}
    const C=window.AudioContext||window.webkitAudioContext;if(!C)return;
    this.ctx=new C();this.master=this.ctx.createGain();this.master.connect(this.ctx.destination);
    this.music=this.ctx.createGain();this.music.gain.value=0;this.music.connect(this.master);
    this.ambience=this.ctx.createGain();this.ambience.gain.value=0;this.ambience.connect(this.master);
    for(const freq of [130.81,196,261.63]){const osc=this.ctx.createOscillator(),gain=this.ctx.createGain();osc.type='sine';osc.frequency.value=freq;gain.gain.value=.025;osc.connect(gain);gain.connect(this.music);osc.start();}
    const count=this.ctx.sampleRate*6,buffer=this.ctx.createBuffer(1,count,this.ctx.sampleRate),d=buffer.getChannelData(0);let seed=17,lp=0;
    for(let i=0;i<count;i++){seed=(Math.imul(seed,1664525)+1013904223)|0;lp+=(((seed>>>0)/4294967296*2-1)-lp)*.015;d[i]=lp*.13*Math.sin(Math.PI*i/count)**2;}
    const wind=this.ctx.createBufferSource();wind.buffer=buffer;wind.loop=true;const filter=this.ctx.createBiquadFilter();filter.type='lowpass';filter.frequency.value=600;wind.connect(filter);filter.connect(this.ambience);wind.start();
    await this.ctx.resume().catch(()=>{});
  }
  play(line){
    this.stopVoice();const entry=this.coverage.lines?.[line.id];this.fallback=!entry;this.toneGain=line.tone==='whisper'?.78:line.tone==='soft'?.9:1;
    if(this.mutedForQA){this.source='qa';return entry?.duration??estimate(line.text);}
    const approved=this.visitCoverage?.editorial_status==='approved'&&this.visitCoverage.lines?.[line.id];
    if(approved&&!this.settings.voiceURI){this.fallback=false;this.source='approved-recording';this.voice.src='./audio/visits/'+line.id+'.mp3';this.voice.volume=clamp(this.settings.master*this.settings.narration*this.toneGain);this.voice.onerror=()=>{this.fallback=true;};this.voice.play().catch(()=>{this.fallback=true;});return approved.duration;}
    if(line.voiceSource==='device'||this.settings.voiceMode!=='legacy')return this.speakDevice(line);
    if(!entry){this.source='none';return estimate(line.text);}
    this.source='legacy';
    this.voice.src=`./audio/narration/${line.id}.mp3`;this.voice.volume=clamp(this.settings.master*this.settings.narration*(line.tone==='whisper'?.78:line.tone==='soft'?.9:1));
    this.voice.onerror=()=>{this.failures.add(line.id);this.fallback=true;};
    this.voice.play().catch(()=>{this.failures.add(line.id);this.fallback=true;});
    return entry.duration;
  }
  get deviceVoices(){return germanLocalVoices(this.system?.getVoices()||[]);}
  speakDevice(line){
    const choices=this.deviceVoices,voice=choices.find(v=>v.voiceURI===this.settings.voiceURI)||choices[0];
    const duration=estimate(line.text);
    if(!voice||!globalThis.SpeechSynthesisUtterance){this.fallback=true;this.source='none';return duration;}
    this.fallback=false;this.source='device';const utterance=this.makeUtterance(line);
    let done=false,timer,elapsed=0;
    const finished=new Promise(resolve=>{
      const finish=ok=>{if(done)return;done=true;clearInterval(timer);this.finishSpeech=null;this.activeSpeech=null;resolve(ok);};this.finishSpeech=()=>finish(true);
      const fail=()=>{this.fallback=true;this.source='none';finish(false);};
      utterance.onend=()=>finish(true);utterance.onerror=fail;this.activeSpeech={line,utterance,end:utterance.onend,fail,replay:false};
      timer=setInterval(()=>{if(!this.paused)elapsed++;if(elapsed>Math.max(30,duration*4)){this.fallback=true;finish(false);this.system.cancel();}},1000);
      try{this.system.speak(utterance);}catch{this.fallback=true;finish(false);}
    });return {duration,finished};
  }
  makeUtterance(line){const voice=this.deviceVoices.find(v=>v.voiceURI===this.settings.voiceURI)||this.deviceVoices[0];if(!voice)return null;const u=new SpeechSynthesisUtterance(line.text);u.voice=voice;u.lang='de-DE';u.rate=.91;u.pitch=({fino:.98,mila:1.06,piko:.96,luna:.97,nuri:1.04,leni:1.02,suri:1.05})[line.speaker]||1;u.volume=clamp(this.settings.master*this.settings.narration*(line.tone==='soft'?.9:1));return u;}
  previewDevice(text){
    this.stopPreview();const current=this.activeSpeech;if(current){current.replay=true;current.utterance.onend=null;current.utterance.onerror=null;}this.system?.cancel();
    const u=this.makeUtterance({text,speaker:'suri',tone:'warm'});if(!u)return Promise.resolve(false);
    return new Promise(resolve=>{let done=false;const finish=ok=>{if(done)return;done=true;this.previewFinish=null;resolve(ok);};this.previewFinish=()=>{u.onend=null;u.onerror=null;finish(true);};u.onend=()=>finish(true);u.onerror=()=>finish(false);this.system.resume();this.system.speak(u);});
  }
  previewEmma(){
    this.stopPreview();const current=this.activeSpeech;if(current){current.replay=true;current.utterance.onend=null;current.utterance.onerror=null;}this.system?.cancel();
    const clip=new Audio('./audio/visits/emma-reference.mp3');clip.volume=clamp(this.settings.master*this.settings.narration);
    return new Promise(resolve=>{let done=false;const finish=ok=>{if(done)return;done=true;clip.pause();clip.onended=null;clip.onerror=null;this.previewFinish=null;resolve(ok);};this.previewFinish=()=>finish(true);clip.onended=()=>finish(true);clip.onerror=()=>finish(false);clip.play().catch(()=>finish(false));});
  }
  stopPreview(){if(this.previewFinish){this.previewFinish();this.system?.cancel();}}
  stopVoice(){this.stopPreview();this.finishSpeech?.();this.system?.cancel();this.voice.pause();this.voice.removeAttribute('src');this.voice.load();}
  pause(){this.paused=true;this.system?.pause();this.voice.pause();this.ctx?.suspend().catch(()=>{});}
  resume(){this.paused=false;this.stopPreview();this.system?.resume();const task=this.activeSpeech;if(task?.replay){task.replay=false;const u=this.makeUtterance(task.line);if(u){task.utterance=u;u.onend=task.end;u.onerror=task.fail;this.system.speak(u);}else task.fail();}if(this.voice.getAttribute('src')&&!this.voice.ended)this.voice.play().catch(()=>{});this.ctx?.resume().catch(()=>{});}
  stop(){this.pause();this.stopVoice();}
  update(calm,time,endedAt=-1){
    if(this.voice.getAttribute('src'))this.voice.volume=clamp(this.settings.master*this.settings.narration*(this.toneGain??1));
    if(!this.ctx)return;const now=this.ctx.currentTime,k=clamp(calm),fade=endedAt<0?1:Math.max(0,1-(time-endedAt)/45);
    this.master.gain.setTargetAtTime(this.mutedForQA?0:this.settings.master,now,.3);
    this.music.gain.setTargetAtTime(this.settings.music*(.65-.43*k)*fade,now,1.5);
    this.ambience.gain.setTargetAtTime(this.settings.ambience*(1-.9*k),now,2);
    if(!this.paused&&time>this.noteAt&&endedAt<0){this.noteAt=time+6+k*8;const osc=this.ctx.createOscillator(),gain=this.ctx.createGain();osc.type='sine';osc.frequency.value=this.notes[Math.floor(time/7)%this.notes.length];gain.gain.setValueAtTime(0,now);gain.gain.linearRampToValueAtTime(.045,now+.12);gain.gain.exponentialRampToValueAtTime(.0001,now+3.5);osc.connect(gain);gain.connect(this.music);osc.start();osc.stop(now+4);osc.onended=()=>{osc.disconnect();gain.disconnect();};}
  }
  get available(){return Object.keys(this.coverage.lines||{}).length;}
}
