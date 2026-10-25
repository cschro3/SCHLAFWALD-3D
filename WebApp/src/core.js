export const IDS = ['fino','mila','piko','luna','nuri','leni','suri'];
export const clamp = (x,lo=0,hi=1) => Math.max(lo, Math.min(hi, Number.isFinite(+x)?+x:lo));
export const smooth = x => {x=clamp(x);return x*x*(3-2*x);};
export const DEFAULTS = Object.freeze({master:0.75,narration:1,music:0.3,ambience:0.4,minutes:0,dim:1,timer:0,audioOnly:false,background:false,subtitles:true,animals:IDS});
export function sanitizeSettings(raw={}) {
  const s={...DEFAULTS,...raw};
  for(const k of ['master','narration','music','ambience']) s[k]=clamp(s[k]);
  for(const k of ['minutes','dim','timer']) s[k]=clamp(s[k],0,k==='minutes'?60:90);
  for(const k of ['audioOnly','background','subtitles']) s[k]=s[k]===true;
  s.animals=IDS.filter(x=>Array.isArray(s.animals)&&s.animals.includes(x));
  if(!s.animals.length) s.animals=[...IDS];
  return s;
}
export function readLocal(storage,key,fallback) {
  try{return JSON.parse(storage.getItem(key))??fallback;}catch{return fallback;}
}
export function writeLocal(storage,key,value) {try{storage.setItem(key,JSON.stringify(value));return true;}catch{return false;}}
export function resumable(p,now=Date.now()) {
  return !!(p&&!p.finished&&Array.isArray(p.sleeping)&&p.sleeping.some(x=>IDS.includes(x))&&now-p.updated>=0&&now-p.updated<90*60e3);
}
export function filterLines(lines,ids,total=7) {
  return lines.filter(l=>(!l.animalId||ids.includes(l.animalId))&&(l.when!=='all'||ids.length===total)&&(l.when!=='partial'||ids.length<total));
}
export function phaseLines(animal,phase,index) {
  const specific=animal.lines.filter(l=>l.phase===phase+(index??''));
  return specific.length?specific:animal.lines.filter(l=>l.phase===phase);
}
export function atmosphere(calm) {
  const k=smooth(calm);return {light:1-0.35*k,motion:1-0.82*k,particles:1-0.88*k,music:0.65-0.43*k,ambience:1-0.95*k};
}
export function estimate(text) {return Math.max(1,(text.match(/[\p{L}\p{N}]+/gu)||[]).length/1.8+(text.match(/[.,!?…]/g)||[]).length*0.4);}
export function makeChallenge(random=Math.random) {
  const a=6+Math.floor(random()*4),b=6+Math.floor(random()*4),answer=a*b;
  const answers=[answer,answer+3,answer-4];
  for(let i=2;i>0;i--){const j=Math.floor(random()*(i+1));[answers[i],answers[j]]=[answers[j],answers[i]];}
  return {question:`${a} × ${b}`,answers,answer};
}
export class GentleHelp {
  constructor(hint=14,assist=32){this.hint=hint;this.assist=Math.max(hint+1,assist);this.elapsed=0;this.hinted=false;this.assisted=false;}
  tick(dt,{progress=false,speaking=false}={}) {
    if(this.assisted)return null;
    if(progress){this.elapsed=0;this.hinted=false;return null;}
    if(speaking)return null; // narration pauses waiting; it never erases elapsed waiting
    this.elapsed+=Math.max(0,dt);
    if(this.elapsed>=this.assist){this.assisted=true;return 'assist';}
    if(this.elapsed>=this.hint&&!this.hinted){this.hinted=true;return 'hint';}
    return null;
  }
}
export function validateStory(story) {
  const errors=[],seen=new Set();
  if(story.animals?.length!==7)errors.push('Sieben Tiere erforderlich');
  for(const id of IDS)if(!story.animals?.some(a=>a.id===id))errors.push(`Tier fehlt: ${id}`);
  for(const a of story.animals??[]){
    if(!a.steps?.length)errors.push(`${a.id}: Ritual fehlt`);
    for(const [i,s] of a.steps.entries()) {
      if(!['Touch','Stroke','DragToTarget','FollowPath'].includes(s.type))errors.push(`${a.id}: Schritttyp`);
      if(!s.points?.length||!(s.radius>=0.6))errors.push(`${a.id}: Ziele`);
      for(const p of ['step','hint','assist'])if(!phaseLines(a,p,i).length)errors.push(`${a.id}: ${p}${i}`);
    }
    for(const p of ['intro','outro'])if(!phaseLines(a,p).length)errors.push(`${a.id}: ${p}`);
  }
  for(const l of [...(story.chapters??[]).flatMap(c=>c.lines),...(story.animals??[]).flatMap(a=>a.lines)]){
    if(!l.id||seen.has(l.id)||!l.text)errors.push(`Zeile ungültig: ${l.id}`);seen.add(l.id);
  }
  return errors;
}
export class StoryClock {
  constructor(){this.time=0;this.paused=new Set();this.waiters=[];this.scale=1;}
  tick(dt){if(this.paused.size)return;this.time+=Math.min(Math.max(dt,0),0.1)*this.scale;const done=this.waiters.filter(w=>this.time>=w.at);this.waiters=this.waiters.filter(w=>this.time<w.at);for(const w of done)w.resolve();}
  wait(seconds){return new Promise(resolve=>this.waiters.push({at:this.time+Math.max(0,seconds),resolve}));}
  cancel(){for(const w of this.waiters)w.resolve();this.waiters=[];}
}
