import {IDS,validateStory} from './core.js';

// Rituals and models remain the canonical project's. Each visit tells a self-contained story.
export function createVisitStory(canonical,visits){
  const ids=visits.animals.map(a=>a.id);
  if(ids.length!==IDS.length||new Set(ids).size!==IDS.length||IDS.some(id=>!ids.includes(id)))throw Error('Für jedes Tier wird eine Besuchsgeschichte benötigt.');
  const result={...canonical,chapters:[{kind:'intro',lines:[]},{kind:'finale',lines:visits.closing}],animals:canonical.animals.map(a=>({...a,...visits.animals.find(v=>v.id===a.id),steps:a.steps}))};
  const errors=validateStory(result);if(errors.length)throw Error(errors.join('; '));return result;
}
export function sanitizeVisitProgress(raw,now=Date.now()){
  const valid=raw&&Number.isFinite(raw.updated)&&raw.updated<=now&&now-raw.updated<12*60*60e3&&!raw.finished;
  return {sleeping:valid?IDS.filter(id=>Array.isArray(raw.sleeping)&&raw.sleeping.includes(id)):[],updated:now,finished:false};
}
export function rememberVisit(progress,id,now=Date.now()){
  return {...progress,sleeping:IDS.filter(x=>progress.sleeping.includes(x)||x===id),updated:now,finished:false};
}
export function germanLocalVoices(voices){
  return voices.filter(v=>/^de(?:-|_|$)/i.test(v.lang)&&v.localService===true).sort((a,b)=>{
    const quality=v=>/premium|enhanced|siri|natural/i.test(v.name)?3:/anna|petra|helena|katja/i.test(v.name)?2:v.default?1:0;
    return quality(b)-quality(a)||a.name.localeCompare(b.name,'de');
  });
}
