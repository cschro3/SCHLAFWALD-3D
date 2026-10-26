import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
import {createVisitStory,sanitizeVisitProgress,rememberVisit,germanLocalVoices} from '../src/visits.js';
import {IDS,phaseLines,validateStory,sanitizeSettings} from '../src/core.js';
const base=JSON.parse(readFileSync(new URL('../../UnityProject/Assets/Schlafwald/Resources/Story/story_de.json',import.meta.url)));
const visits=JSON.parse(readFileSync(new URL('../content/visits_de.json',import.meta.url)));
test('seven independent first-person visits retain all ten canonical rituals without mutating Unity data',()=>{
 const before=JSON.stringify(base),story=createVisitStory(base,visits);assert.equal(JSON.stringify(base),before);assert.deepEqual(validateStory(story),[]);assert.equal(story.animals.flatMap(a=>a.steps).length,10);
 for(const a of story.animals){assert.ok(a.title);assert.ok(a.voiceDirection);assert.match(phaseLines(a,'intro').map(l=>l.text).join(' '),/\bich\b/i);assert.ok(!a.lines.some(l=>l.animalId&&l.animalId!==a.id));assert.equal(a.steps,base.animals.find(x=>x.id===a.id).steps);}
});
test('a missing or duplicate visit fails validation before the user can enter',()=>{const broken=structuredClone(visits);broken.animals[0].id='suri';assert.throws(()=>createVisitStory(base,broken));});
test('multi-step visits provide an instruction and relevant help for every individual ritual',()=>{const story=createVisitStory(base,visits);for(const id of ['suri','leni']){const a=story.animals.find(a=>a.id===id);for(let i=0;i<a.steps.length;i++)for(const phase of ['step','hint','assist'])assert.ok(phaseLines(a,phase,i).length);assert.ok(new Set(a.steps.map((_,i)=>phaseLines(a,'hint',i)[0].id)).size===a.steps.length);}});
test('visits can be completed in arbitrary order and repeated without duplicate progress',()=>{let p=sanitizeVisitProgress(null,1000);for(const id of ['suri','nuri','suri','fino'])p=rememberVisit(p,id,1001);assert.deepEqual(p.sleeping,['fino','nuri','suri']);assert.equal(p.finished,false);});
test('old, future, completed or hostile visit saves start a fresh night',()=>{for(const p of [null,{updated:2000,sleeping:IDS},{updated:0,sleeping:IDS},{updated:999,sleeping:IDS,finished:true}])assert.deepEqual(sanitizeVisitProgress(p,1000+13*60*60e3).sleeping,[]);assert.deepEqual(sanitizeVisitProgress({updated:999,sleeping:['suri','x','suri']},1000).sleeping,['suri']);});
test('device voice selection only includes installed German voices and prefers enhanced voices',()=>{const rows=[{name:'English',lang:'en-US',localService:true},{name:'Google Deutsch',lang:'de-DE',localService:false},{name:'Deutsch Standard',lang:'de-DE',localService:true},{name:'Anna Enhanced',lang:'de-DE',localService:true}];assert.deepEqual(germanLocalVoices(rows).map(v=>v.name),['Anna Enhanced','Deutsch Standard']);assert.equal(sanitizeSettings({voiceURI:{},voiceMode:'remote'}).voiceURI,'');assert.equal(sanitizeSettings({voiceMode:'remote'}).voiceMode,'device');});
