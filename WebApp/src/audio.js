import {estimate,clamp} from './core.js';

// No runtime speech service, microphone permission, analytics, or remote request.
// All spoken clips are pre-rendered and shipped with the application.
export class ForestAudio {
  constructor(settings){this.settings=settings;this.coverage={};this.voice=new Audio();this.voice.preload='auto';this.voice.playsInline=true;this.mutedForQA=false;this.paused=false;this.failures=new Set();this.noteAt=0;this.notes=[196,220,261.63,329.63,293.66,261.63,220,196];}
  async init(){
    try{const r=await fetch('./audio/manifest.json');if(r.ok)this.coverage=await r.json();}catch{/* visible read-aloud fallback */}
  }
  async unlock(){
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
    if(!entry||this.mutedForQA)return entry?.duration??estimate(line.text);
    this.voice.src=`./audio/narration/${line.id}.mp3`;this.voice.volume=clamp(this.settings.master*this.settings.narration*(line.tone==='whisper'?.78:line.tone==='soft'?.9:1));
    this.voice.onerror=()=>{this.failures.add(line.id);this.fallback=true;};
    this.voice.play().catch(()=>{this.failures.add(line.id);this.fallback=true;});
    return entry.duration;
  }
  stopVoice(){this.voice.pause();this.voice.removeAttribute('src');this.voice.load();}
  pause(){this.paused=true;this.voice.pause();this.ctx?.suspend().catch(()=>{});}
  resume(){this.paused=false;if(this.voice.getAttribute('src')&&!this.voice.ended)this.voice.play().catch(()=>{});this.ctx?.resume().catch(()=>{});}
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
