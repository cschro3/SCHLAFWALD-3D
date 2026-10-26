import './style.css';
import legacyStory from '../../UnityProject/Assets/Schlafwald/Resources/Story/story_de.json';
import {IDS,DEFAULTS,sanitizeSettings,readLocal,writeLocal,resumable,filterLines,phaseLines,GentleHelp,makeChallenge,StoryClock,validateStory,clamp,smooth} from './core.js';
import {ForestView} from './world.js';
import {ForestAudio} from './audio.js';
import visitContent from '../content/visits_de.json';
import {createVisitStory,sanitizeVisitProgress,rememberVisit} from './visits.js';
const visitStory=createVisitStory(legacyStory,visitContent);let story=legacyStory;

const icon=(name)=>({leaf:'<path d="M19 4C10 2 3 6 5 14c2 8 13 5 14-10Z"/><path d="M5 20 15 9"/>',moon:'<path d="M20 14A8 8 0 0 1 10 4a8 8 0 1 0 10 10Z"/>',lock:'<rect x="5" y="10" width="14" height="11" rx="3"/><path d="M8 10V7a4 4 0 0 1 8 0v3"/>',close:'<path d="m6 6 12 12M6 18 18 6"/>',play:'<path d="m9 5 11 7-11 7Z"/>',pause:'<path d="M9 5v14M16 5v14"/>',back:'<path d="m14 6-6 6 6 6"/>',audio:'<path d="M8 9H4v6h4l5 4V5L8 9Z"/><path d="M17 8a6 6 0 0 1 0 8"/>',arrow:'<path d="M4 12h15m-6-6 6 6-6 6"/>'}[name]||'');
const svg=n=>`<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">${icon(n)}</svg>`;
const names={fino:'Fino',mila:'Mila',piko:'Piko',luna:'Luna',nuri:'Nuri',leni:'Leni',suri:'Suri'};
const species={fino:'der Fuchs',mila:'das Kaninchen',piko:'der Igel',luna:'die Eule',nuri:'das Eichhörnchen',leni:'das Rehkitz',suri:'die Schnecke'};
const el=document.querySelector('#app');
el.innerHTML=`<main id="forest-app">
  <canvas id="forest" aria-label="Ein ruhiger, räumlicher Märchenwald. Begleite die Tiere mit deinem Finger in den Schlaf." tabindex="0"></canvas>
  <div class="edge-shade" aria-hidden="true"></div><div id="scene-fade" aria-hidden="true"></div>
  <header class="hud"><span class="wordmark">${svg('leaf')} SCHLAFWALD</span><button id="parent-open" aria-label="Elternbereich – drei Sekunden halten" title="Drei Sekunden halten">${svg('lock')}<span>Eltern</span></button></header>
  <section id="welcome" class="welcome"><h1>Ein kleiner Wald.<br>Ein leises Gute Nacht.</h1><p>Wen möchtest du heute besuchen?<br>Jedes Tier hat dir etwas zu erzählen.</p><div class="welcome-actions"><button id="start" class="primary">Tiere besuchen ${svg('arrow')}</button><button id="continue" class="quiet" hidden>Nacht fortsetzen</button></div><span class="welcome-foot">Zeit zum Kuscheln. Für euch beide.</span></section>
  <section id="forest-menu" hidden aria-labelledby="forest-title"><h1 id="forest-title">Bei wem klopfen wir an?</h1><p>Tippe einen Freund an und besuche ihn.</p><div id="forest-labels"></div><button id="night-finish" class="quiet">${svg('moon')} Für heute Gute Nacht</button></section>
  <button id="visit-back" class="scene-back" hidden>${svg('back')} Zum Wald</button>
  <section id="visit-finished" class="visit-finished" hidden><h2 id="visit-done-title"></h2><p id="visit-done-note">Danke für deinen lieben Besuch.</p><div><button id="visit-home" class="primary">Zum Wald ${svg('arrow')}</button><button id="visit-again" class="quiet">Noch einmal besuchen</button></div></section>
  <div id="chapter" class="chapter" hidden><span id="animal-name"></span><span id="animal-species"></span></div>
  <div id="narration" class="narration" aria-live="polite"><p id="subtitle"></p><span id="read-aloud" hidden>Zum gemeinsamen Vorlesen</span></div>
  <div id="gesture" class="gesture" hidden><span id="gesture-icon">${svg('leaf')}</span><span id="gesture-text"></span></div>
  <div id="journey" class="journey" aria-label="Die Tiere dieser Nacht" hidden>${IDS.map(x=>`<span data-animal="${x}" title="${names[x]}"><i></i><span>${names[x]}</span></span>`).join('')}</div>
  <div id="dim" aria-hidden="true"></div>
  <div id="audio-view" hidden><span>${svg('moon')}</span><p>Der Wald erzählt weiter.</p><button id="wake-screen" class="quiet">Bild wieder anzeigen</button></div>
  <section id="finished" class="finished" hidden><span>${svg('moon')}</span><h1>Schlaf schön.</h1><p>Der Mond passt auf alle auf.</p></section>
  <dialog id="gate" aria-labelledby="gate-title"><button class="close" data-close="gate" aria-label="Schließen">${svg('close')}</button><span class="dialog-icon">${svg('lock')}</span><h2 id="gate-title">Ein Moment für die Großen</h2><p>Löse die kleine Rechenaufgabe.</p><p id="challenge"></p><div id="answers" class="answers"></div><p id="gate-error" role="status"></p></dialog>
  <dialog id="parents" aria-labelledby="parents-title"><div class="parents-heading"><div><h2 id="parents-title">Eure ruhige Nacht</h2><p>So, wie es zu euch passt.</p></div><button class="close" data-close="parents" aria-label="Zur Geschichte zurück">${svg('close')}</button></div>
    <div class="settings-scroll"><section class="settings-section"><h3>Hören</h3><label for="set-voice">Deutsche Stimme auf diesem Gerät</label><select id="set-voice" aria-describedby="voice-note"></select><p id="voice-note" class="setting-note">Die Gerätestimme ist eine vorläufige Hörprobe. Eine liebevoll gesprochene Vertonung der neuen Geschichten steht noch aus.</p><button id="emma-preview" class="secondary">Emma · ausgewählte Version 2 anhören</button><p class="setting-note">Deine ausgewählte Suri-Hörprobe. Die vollständigen Besuchsgeschichten sind noch nicht mit Emma vertont.</p><button id="voice-preview" class="secondary">Gerätestimme anhören</button>${[['master','Gesamtlautstärke'],['narration','Erzähler'],['music','Musik'],['ambience','Waldgeräusche']].map(([id,label])=>`<label class="range-label" for="set-${id}"><span>${label}</span><output id="out-${id}"></output></label><input id="set-${id}" type="range" min="0" max="100" step="1">`).join('')}
    <label class="toggle"><span>Erzählertext anzeigen</span><input id="set-subtitles" type="checkbox"></label></section>
    <section class="settings-section"><h3>Zeit zum Einschlafen</h3><label for="set-minutes">Wunschdauer der Geschichte</label><select id="set-minutes"><option value="0">Ganz natürlich</option><option value="10">Etwa 10 Minuten</option><option value="15">Etwa 15 Minuten</option><option value="20">Etwa 20 Minuten</option><option value="30">Etwa 30 Minuten</option></select><p class="setting-note">Eine ungefähre Dauer. Jedes Kind darf sich Zeit lassen.</p>
    <label for="set-dim">Nach dem Finale abdunkeln</label><select id="set-dim"><option value="0">Nicht automatisch</option><option value="1">Nach 1 Minute</option><option value="3">Nach 3 Minuten</option><option value="5">Nach 5 Minuten</option></select>
    <label for="set-timer">Audio automatisch ausschalten</label><select id="set-timer"><option value="0">Nicht automatisch</option><option value="15">Nach 15 Minuten</option><option value="30">Nach 30 Minuten</option><option value="45">Nach 45 Minuten</option><option value="60">Nach 60 Minuten</option></select></section>
    <section class="settings-section"><h3>Bild &amp; Audio</h3><label class="toggle"><span>Nur zuhören, Bildschirm dunkel</span><input id="set-audioOnly" type="checkbox"></label><label class="toggle"><span>Audio im Hintergrund erlauben</span><input id="set-background" type="checkbox"></label><p class="setting-note" id="background-note">Bei gesperrtem Bildschirm kann der Browser die Wiedergabe unterbrechen. Fortschritt bleibt gespeichert.</p><button id="dim-now" class="secondary">Jetzt abdunkeln</button></section>
    <section class="settings-section"><h3>Wer schläft heute mit?</h3><div class="animal-options">${IDS.map(id=>`<label><input type="checkbox" name="animal" value="${id}"><span>${names[id]}<small>${species[id]}</small></span></label>`).join('')}</div><p class="setting-note">Die Auswahl gilt für die nächste Nacht.</p></section>
    <section class="settings-section"><h3>Geschichte</h3><button id="listen-round" class="secondary">Alle Tiere nacheinander anhören</button><p class="setting-note">Im Wald wählt ihr frei. Diese Runde begleitet die Tiere automatisch.</p><div class="parent-actions"><button id="pause" class="secondary">Geschichte pausieren</button><button id="restart" class="secondary">Neue Nacht beginnen</button><button id="resume" class="secondary">Fortschritt fortsetzen</button></div><p id="local-status" class="setting-note" role="status"></p><details><summary>Über Schlafwald</summary><p>Offline, ohne Werbung, Kinderkonto oder Tracking. Einstellungen und die schlafenden Tiere bleiben nur auf diesem Gerät.</p><p id="voice-info"></p><p>Neue Besuchsgeschichten: vorläufige Gerätestimme, sofern eine deutsche Offline-Stimme installiert ist. Die alte Piper-Teststimme wurde nicht als finale Vertonung übernommen.</p></details></section></div>
    <div class="parents-footer"><button id="parents-done" class="primary">Zurück in den Wald ${svg('arrow')}</button></div>
  </dialog>
  <dialog id="restart-confirm" aria-labelledby="restart-title"><h2 id="restart-title">Eine neue Nacht beginnen?</h2><p>Die Tiere der bisherigen Nacht werden wieder wach.</p><div class="confirm-actions"><button id="cancel-restart" class="secondary">Zurück</button><button id="confirm-restart" class="primary">Neue Nacht</button></div></dialog>
  <div id="orientation" role="status">${svg('leaf')}<span>Der Wald hat Platz für große Träume.<br>Dreh dein Gerät gern ins Querformat.</span></div>
  <div id="error" role="alert" hidden><h1>Der Wald braucht einen Moment.</h1><p id="error-message"></p><button class="primary" id="reload">Noch einmal öffnen</button></div>
</main>`;
const $=s=>document.querySelector(s), all=s=>[...document.querySelectorAll(s)];
const storage=(()=>{try{return window.localStorage;}catch{return null;}})();
let settings=sanitizeSettings(readLocal(storage,'sw_web_settings_v1',DEFAULTS));
let progress=readLocal(storage,'sw_web_progress_v1',null),active=IDS.slice(),sleeping=[],token=0,running=false,phase='welcome',activeId='suri',stepIndex=-1,endedAt=-1,sessionStart=0,parentPause=false,currentLine='',transition=0,assists=0,hints=0,assistedSteps=[],completedSteps=[];
let mode='legacy',visitProgress=sanitizeVisitProgress(readLocal(storage,'sw_visits_v1',progress)),forestOptions={scale:1,silent:false},forestIds=[...settings.animals],nightStarted=false,nightTime=0;
const clock=new StoryClock(),audio=new ForestAudio(settings);
let view;
function showError(msg){$('#error-message').textContent=msg;$('#error').hidden=false;}
try{const errors=validateStory(story);if(errors.length)throw new Error(errors.join(', '));view=new ForestView($('#forest'));view.showAnimal(story.animals.find(a=>a.id==='suri'));view.animal.root.position.set(.65,.12,.1);view.setWelcomeLayout(true);}catch(e){showError('Dein Browser konnte den 3D-Wald nicht öffnen. Versuche es in Safari oder Chrome mit aktivierter Grafikbeschleunigung.');console.error(e);}
$('#reload').onclick=()=>location.reload();
const audioReady=audio.init();
$('#continue').hidden=!visitProgress.sleeping.length&&!resumable(progress);$('#resume').disabled=!visitProgress.sleeping.length&&!resumable(progress);

function saveSettings(){settings=sanitizeSettings(settings);audio.settings=settings;const ok=writeLocal(storage,'sw_web_settings_v1',settings);$('#local-status').textContent=ok?'Einstellungen auf diesem Gerät gespeichert.':'Speichern ist in diesem Browser gerade nicht möglich. Die Einstellungen gelten für diese Sitzung.';}
function saveProgress(finished=false){if(mode!=='legacy'){for(const id of sleeping)visitProgress=rememberVisit(visitProgress,id);visitProgress={...visitProgress,updated:Date.now(),finished};writeLocal(storage,'sw_visits_v1',visitProgress);return;}progress={sleeping:[...sleeping],updated:Date.now(),finished};writeLocal(storage,'sw_web_progress_v1',progress);}
function pause(reason,on){if(on)clock.paused.add(reason);else clock.paused.delete(reason);if(clock.paused.size)audio.pause();else if(running||nightStarted)audio.resume();}
const cancelled=Symbol('story cancelled');
function check(t){if(t!==token)throw cancelled;}
async function wait(sec,t){await clock.wait(sec);check(t);}
async function say(line,t){
  check(t);
  if(phase==='finale'&&line.animalId){transition=1;await wait(1,t);view.restoreAsleep(story.animals.find(a=>a.id===line.animalId));activeId=line.animalId;transition=0;await wait(1,t);}
  if(phase==='finale'&&line.shotId==='moon'){view.cameraTarget.set(1.8,4.5,10);view.lookTarget.set(0,4,-10);}
  currentLine=line.text;$('#subtitle').textContent=line.text;$('#narration').hidden=!settings.subtitles;
  const playback=audio.play(line);$('#read-aloud').hidden=!audio.fallback;
  if(line.cue){const id=activeId;clock.wait(line.cueDelay||0).then(()=>{if(t===token&&activeId===id)view?.cue(line.cue);});}
  if(typeof playback==='number')await wait(playback,t);else{const ok=await playback.finished;check(t);if(!ok){$('#read-aloud').hidden=false;await wait(playback.duration,t);}}check(t);audio.stopVoice();currentLine='';$('#subtitle').textContent='';
  const pauseScale=settings.minutes?clamp(settings.minutes/18,.5,3):1;await wait((line.pauseAfter??1.2)*pauseScale,t);
}
async function lines(ls,t){for(const line of ls)await say(line,t);}
function updateJourney(){for(const e of all('#journey>[data-animal]')){const id=e.dataset.animal;e.hidden=!active.includes(id);e.classList.toggle('asleep',sleeping.includes(id));e.classList.toggle('current',activeId===id&&!sleeping.includes(id));}}
const instructions={FollowPath:'Begleite das kleine Licht mit deinem Finger.',DragToTarget:'Bring es ganz sanft zum leuchtenden Platz.',Stroke:'Streichle ganz behutsam über das weiche Fell.',Touch:'Berühre das leuchtende Ziel ganz sanft.'};
async function run(t,resume){
  try{
    if(!resume)await lines(filterLines(story.chapters.find(c=>c.kind==='intro').lines,active),t);
    for(const a of story.animals.filter(a=>active.includes(a.id))){
      if(sleeping.includes(a.id))continue;
      transition=1;await wait(1,t);activeId=a.id;phase='intro';view.showAnimal(a);updateJourney();
      $('#chapter').hidden=false;$('#animal-name').textContent=a.displayName;$('#animal-species').textContent=mode==='legacy'?species[a.id]:a.title;transition=0;await wait(1.4,t);
      await lines(phaseLines(a,'intro'),t);
      for(const [index,config] of a.steps.entries()){
        phase='ritual';stepIndex=index;const step=view.beginStep(config,index);
        $('#gesture').hidden=settings.audioOnly;$('#gesture-text').textContent=instructions[config.type];
        await lines(phaseLines(a,'step',index),t);
        const help=new GentleHelp(config.helpAfter??14,config.assistAfter??32);let last=step.progress,lastActivity=step.activity||0,lastTick=clock.time;
        while(!step.complete){
          await wait(.08,t);const elapsed=clock.time-lastTick;lastTick=clock.time;const level=help.tick(elapsed*(settings.audioOnly?6:1),{progress:step.progress>last||(step.activity||0)>lastActivity});last=step.progress;lastActivity=step.activity||0;
          if(level==='hint'){hints++;await lines(phaseLines(a,'hint',index),t);lastTick=clock.time;}
          if(level==='assist'){assists++;assistedSteps.push(a.id+':'+index);step.assisting=true;await lines(phaseLines(a,'assist',index),t);lastTick=clock.time;}
        }
        completedSteps.push({id:a.id+':'+index,type:config.type,assisted:step.assisting});
        $('#gesture').hidden=true;await lines(phaseLines(a,'success',index),t);await wait(.8,t);
      }
      phase='outro';stepIndex=-1;await lines(phaseLines(a,'outro'),t);view.sleep();await wait(5,t);sleeping.push(a.id);saveProgress();updateJourney();await wait(2,t);
    }
    if(mode==='visit'){phase='visit-ended';running=false;$('#gesture').hidden=true;$('#narration').hidden=true;$('#journey').hidden=true;showVisitEnd();return;}
    phase='finale';$('#gesture').hidden=true;$('#chapter').hidden=true;view.cameraTarget.set(1.8,4.5,10);view.lookTarget.set(0,3,-7);
    await lines(filterLines(story.chapters.find(c=>c.kind==='finale').lines,active),t);
    phase='ended';endedAt=clock.time;saveProgress(true);$('#finished').hidden=false;$('#journey').hidden=true;$('#narration').hidden=true;
  }catch(e){if(e!==cancelled){console.error(e);showError('Die Geschichte wurde unterbrochen. Öffne sie erneut; euer gespeicherter Fortschritt bleibt erhalten.');running=false;audio.stop();}}
}
async function start({resume=false,animals=null,scale=1,silent=false,kind='legacy'}={}){
  if(!view)return;mode=kind;story=kind==='legacy'?legacyStory:visitStory;view.setWelcomeLayout(false);token++;clock.cancel();audio.stopVoice();clock.paused.clear();clock.time=0;clock.scale=scale;audio.mutedForQA=silent;
  active=(animals??settings.animals).filter(x=>IDS.includes(x));if(!active.length)active=[...IDS];
  sleeping=resume?(kind==='legacy'&&resumable(progress)?progress.sleeping.filter(x=>active.includes(x)):kind!=='legacy'?visitProgress.sleeping.filter(x=>active.includes(x)):[]):[];
  running=true;phase='intro';endedAt=-1;sessionStart=0;activeId='suri';stepIndex=-1;parentPause=false;assists=0;hints=0;assistedSteps=[];completedSteps=[];$('#pause').textContent='Geschichte pausieren';
  $('#welcome').hidden=true;$('#forest-menu').hidden=true;$('#visit-finished').hidden=true;$('#visit-back').hidden=kind==='legacy';$('#finished').hidden=true;$('#dim').style.opacity='0';$('#audio-view').hidden=!settings.audioOnly;$('#audio-view p').textContent='Der Wald erzählt weiter.';$('#journey').hidden=kind==='visit';updateJourney();
  await audioReady;await audio.unlock();requestWakeLock();run(token,resume);
}
function showVisitEnd(){const a=visitStory.animals.find(a=>a.id===activeId);$('#visit-done-title').textContent=a.displayName+' schläft.';$('#visit-finished').hidden=false;$('#chapter').hidden=true;}
function enterForest({resume=true,scale=forestOptions.scale,silent=forestOptions.silent}={}){
  if(!view)return;token++;clock.cancel();audio.stopVoice();clock.paused.clear();clock.scale=scale;audio.mutedForQA=silent;forestOptions={scale,silent};
  if(!nightStarted||!resume||visitProgress.finished)forestIds=[...settings.animals];
  if(!resume||visitProgress.finished){visitProgress=sanitizeVisitProgress(null);nightTime=0;writeLocal(storage,'sw_visits_v1',visitProgress);}
  nightStarted=true;story=visitStory;mode='forest';running=false;phase='forest';activeId='';stepIndex=-1;currentLine='';sleeping=[...visitProgress.sleeping];endedAt=-1;transition=0;pointerId=null;
  for(const id of ['welcome','chapter','narration','gesture','journey','visit-finished','finished','visit-back'])$('#'+id).hidden=true;
  $('#forest-menu').hidden=false;$('#dim').style.opacity='0';$('#audio-view').hidden=!settings.audioOnly;$('#audio-view p').textContent='Der Wald erzählt weiter.';
  view.showClearing(visitStory.animals.filter(a=>forestIds.includes(a.id)),visitProgress.sleeping);renderForestLabels();audio.unlock();
  if(settings.audioOnly)startRound();
}
function renderForestLabels(){
  $('#forest-labels').replaceChildren();for(const a of visitStory.animals.filter(a=>forestIds.includes(a.id))){const b=document.createElement('button');const asleep=visitProgress.sleeping.includes(a.id);b.className='forest-label'+(asleep?' is-asleep':'');b.dataset.visit=a.id;b.setAttribute('aria-label',a.displayName+(asleep?' schläft – leise besuchen':' besuchen'));b.innerHTML='<span>'+a.displayName+'</span>'+(asleep?'<small>'+svg('moon')+' schläft</small>':'');b.onclick=()=>visitAnimal(a.id);$('#forest-labels').append(b);}positionForestLabels();
}
function positionForestLabels(){if(mode!=='forest'||!view)return;for(const p of view.clearingTargets()){const b=$('[data-visit="'+p.id+'"]');if(b){b.style.left=p.x+'px';b.style.top=p.y+'px';}}}
function visitAnimal(id,{repeat=false}={}){
  if(!IDS.includes(id)||!forestIds.includes(id))return;
  if(visitProgress.sleeping.includes(id)&&!repeat){token++;clock.cancel();audio.stopVoice();mode='visit';story=visitStory;running=false;phase='visit-ended';activeId=id;stepIndex=-1;$('#forest-menu').hidden=true;$('#visit-back').hidden=false;view.restoreAsleep(visitStory.animals.find(a=>a.id===id));showVisitEnd();return;}
  start({...forestOptions,animals:[id],kind:'visit'});
}
function startRound(){nightStarted=true;start({...forestOptions,animals:settings.animals,kind:'round',resume:visitProgress.sleeping.length>0});}
async function finishNight(){
  token++;clock.cancel();audio.stopVoice();clock.paused.clear();running=true;mode='round';phase='finale';story=visitStory;const t=token;$('#forest-menu').hidden=true;$('#visit-back').hidden=true;view.cameraTarget.set(0,6,12);view.lookTarget.set(0,8,-20);
  try{await lines(visitContent.closing,t);check(t);phase='ended';endedAt=clock.time;saveProgress(true);$('#finished').hidden=false;$('#narration').hidden=true;}catch(e){if(e!==cancelled)console.error(e);}
}
$('#start').onclick=()=>enterForest();$('#continue').onclick=()=>enterForest();$('#visit-back').onclick=()=>enterForest();$('#visit-home').onclick=()=>enterForest();$('#visit-again').onclick=()=>visitAnimal(activeId,{repeat:true});$('#night-finish').onclick=finishNight;

let pointerId=null;
$('#forest').addEventListener('pointerdown',e=>{if(pointerId!==null||clock.paused.size||settings.audioOnly||!running)return;pointerId=e.pointerId;$('#forest').setPointerCapture(pointerId);view?.pointer('down',e.clientX,e.clientY);});
$('#forest').addEventListener('pointermove',e=>{if(e.pointerId===pointerId&&!clock.paused.size)view?.pointer('move',e.clientX,e.clientY);});
for(const type of ['pointerup','pointercancel'])$('#forest').addEventListener(type,e=>{if(e.pointerId!==pointerId)return;view?.pointer(type==='pointerup'?'up':'cancel',e.clientX,e.clientY);pointerId=null;});

let held=null,holdTimer=null,challenge=null,previousFocus=null;
function cancelHold(){clearTimeout(holdTimer);holdTimer=null;$('#parent-open').classList.remove('holding');}
function openGate(){cancelHold();previousFocus=document.activeElement;pause('parent',true);view?.pointer('cancel',0,0);pointerId=null;challenge=makeChallenge();$('#challenge').textContent=challenge.question+' = ?';$('#answers').replaceChildren();$('#gate-error').textContent='';for(const answer of challenge.answers){const b=document.createElement('button');b.className='secondary';b.textContent=answer;b.onclick=()=>{if(answer===challenge.answer){$('#gate').close();renderSettings();$('#parents').showModal();}else{$('#gate-error').textContent='Versuche es noch einmal.';challenge=null;setTimeout(()=>{$('#gate').close();pause('parent',false);},650);}};$('#answers').append(b);}$('#gate').showModal();}
$('#parent-open').addEventListener('pointerdown',e=>{held=e.pointerId;$('#parent-open').setPointerCapture(held);$('#parent-open').classList.add('holding');holdTimer=setTimeout(openGate,3000);});
for(const type of ['pointerup','pointercancel','lostpointercapture'])$('#parent-open').addEventListener(type,()=>{held=null;cancelHold();});
$('#parent-open').addEventListener('keydown',e=>{if((e.key===' '||e.key==='Enter')&&!e.repeat){e.preventDefault();$('#parent-open').classList.add('holding');holdTimer=setTimeout(openGate,3000);}});
$('#parent-open').addEventListener('keyup',cancelHold);$('#parent-open').addEventListener('blur',cancelHold);
function closeParents(){for(const d of all('dialog[open]'))d.close();pause('parent',false);previousFocus?.focus();}
all('[data-close]').forEach(b=>b.onclick=closeParents);$('#parents-done').onclick=closeParents;
for(const d of all('dialog'))d.addEventListener('cancel',e=>{e.preventDefault();closeParents();});
function renderSettings(){
  for(const k of ['master','narration','music','ambience']){$('#set-'+k).value=Math.round(settings[k]*100);$('#out-'+k).textContent=Math.round(settings[k]*100)+' %';}
  for(const k of ['minutes','dim','timer'])$('#set-'+k).value=settings[k];
  for(const k of ['audioOnly','background','subtitles'])$('#set-'+k).checked=settings[k];
  for(const c of all('[name="animal"]'))c.checked=settings.animals.includes(c.value);
  renderVoices();$('#voice-info').textContent='Die '+visitContent.animals.length+' Besuchsgeschichten haben neue Texte. Ihre finale, ausdrucksvolle Vertonung ist noch offen. '+audio.available+' ältere Testaufnahmen bleiben im Projekt erhalten.';$('#resume').disabled=!visitProgress.sleeping.length&&!resumable(progress);
}
function renderVoices(){const voices=audio.deviceVoices;$('#set-voice').replaceChildren();const automatic=document.createElement('option');automatic.value='';automatic.textContent=voices.length?'Passende deutsche Stimme wählen':'Keine deutsche Offline-Stimme gefunden';$('#set-voice').append(automatic);for(const v of voices){const option=document.createElement('option');option.value=v.voiceURI;option.textContent=v.name;$('#set-voice').append(option);}$('#set-voice').value=settings.voiceURI;$('#voice-preview').disabled=!voices.length;$('#voice-note').textContent=voices.length?'Vorläufige Gerätestimme: Bitte hört die Probe an. Betonung und Qualität hängen vom Gerät ab. Eine finale Vertonung der neuen Geschichten steht noch aus.':'Auf diesem Gerät wurde keine deutsche Offline-Stimme gefunden. Bis eine Stimme oder fertige Aufnahme verfügbar ist, könnt ihr gemeinsam vorlesen.';}
audio.onVoicesChanged=()=>{if($('#parents').open)renderVoices();};
$('#set-voice').onchange=e=>{settings.voiceURI=e.target.value;settings.voiceMode='device';saveSettings();};
$('#emma-preview').onclick=async()=>{const button=$('#emma-preview');button.disabled=true;button.textContent='Emma spricht …';try{await audio.previewEmma();}finally{button.disabled=false;button.textContent='Emma · ausgewählte Version 2 anhören';}};
$('#voice-preview').onclick=async()=>{const button=$('#voice-preview');button.disabled=true;button.textContent='Hör einmal hin …';try{await audio.previewDevice('Hallo, ich bin Suri. Schön, dass du mich besuchst. Heute habe ich einen Tautropfen bestaunt. Jetzt werden meine Fühler müde. Bleibst du noch einen Moment bei mir?');}finally{button.textContent='Gerätestimme anhören';button.disabled=!audio.deviceVoices.length;}};
for(const k of ['master','narration','music','ambience'])$('#set-'+k).oninput=e=>{settings[k]=+e.target.value/100;$('#out-'+k).textContent=e.target.value+' %';saveSettings();};
for(const k of ['minutes','dim','timer'])$('#set-'+k).onchange=e=>{settings[k]=+e.target.value;saveSettings();};
for(const k of ['audioOnly','background','subtitles'])$('#set-'+k).onchange=e=>{settings[k]=e.target.checked;saveSettings();$('#audio-view').hidden=!settings.audioOnly;$('#narration').hidden=!settings.subtitles;if(k==='background')requestWakeLock();if(k==='audioOnly'&&settings.audioOnly&&mode==='forest'){closeParents();startRound();}};
for(const c of all('[name="animal"]'))c.onchange=()=>{const selected=all('[name="animal"]:checked');if(!selected.length){c.checked=true;$('#local-status').textContent='Mindestens ein kleiner Freund schläft mit.';return;}settings.animals=selected.map(c=>c.value);saveSettings();};
$('#pause').onclick=()=>{parentPause=!parentPause;pause('user',parentPause);$('#pause').textContent=parentPause?'Geschichte fortsetzen':'Geschichte pausieren';};
$('#restart').onclick=()=>$('#restart-confirm').showModal();$('#cancel-restart').onclick=()=>$('#restart-confirm').close();
$('#confirm-restart').onclick=()=>{closeParents();enterForest({resume:false});};$('#resume').onclick=()=>{closeParents();enterForest();};$('#listen-round').onclick=()=>{closeParents();startRound();};
$('#dim-now').onclick=()=>{settings.audioOnly=true;saveSettings();$('#audio-view').hidden=false;closeParents();};
$('#wake-screen').onclick=()=>{settings.audioOnly=false;saveSettings();$('#audio-view').hidden=true;};
document.addEventListener('visibilitychange',()=>{if(document.hidden){if(running&&phase!=='ended')saveProgress();if(!settings.background)pause('hidden',true);}else{pause('hidden',false);requestWakeLock();}});
window.addEventListener('pagehide',()=>{if((running||nightStarted)&&phase!=='ended')saveProgress();});
let wakeLock=null;async function requestWakeLock(){try{if(running&&!settings.audioOnly&&!document.hidden&&navigator.wakeLock)wakeLock=await navigator.wakeLock.request('screen');else await wakeLock?.release();}catch{/* optional; browser policy */}}
view&&(view.onContextLost=()=>{pause('graphics',true);showError('Die Grafik wurde unterbrochen. Der Fortschritt ist gesichert; du kannst den Wald erneut öffnen.');saveProgress();},view.onContextRestored=()=>{pause('graphics',false);$('#error').hidden=true;});
let last=performance.now(),lastDraw=last,frameTimes=[],drawDt=0;
function frame(now){
  requestAnimationFrame(frame);const realDt=Math.min(.1,(now-last)/1000);last=now;clock.tick(realDt);
  const dt=clock.paused.size?0:realDt*clock.scale;
  if(nightStarted&&!clock.paused.size&&mode!=='legacy')nightTime+=dt;
  drawDt+=dt;
  const calm=mode!=='legacy'?Math.min(1,(visitProgress.sleeping.length+(phase==='outro'?.5:0))/7):running?(sleeping.length+(phase==='outro'?.6:0))/active.length:0;
  if(view&&now-lastDraw>=(clock.paused.size?400:31)){frameTimes.push(now-lastDraw);if(frameTimes.length>300)frameTimes.shift();lastDraw=now;view.render(drawDt,phase==='ended'?1:calm,!settings.audioOnly&&!document.hidden);drawDt=0;}
  audio.update(phase==='ended'?1:calm,clock.time,endedAt);
  positionForestLabels();
  $('#scene-fade').style.opacity=String(transition);
  if(endedAt>=0&&settings.dim>0)$('#dim').style.opacity=String(.98*smooth((clock.time-endedAt-settings.dim*60)/45));
  if(settings.timer>0&&(running||nightStarted)&&(mode==='legacy'?clock.time-sessionStart:nightTime)>settings.timer*60){audio.stop();pause('timer',true);$('#audio-view').hidden=false;$('#audio-view p').textContent='Gute Nacht. Das Audio ruht jetzt.';}
}
requestAnimationFrame(frame);
if('serviceWorker' in navigator&&!import.meta.env.DEV)navigator.serviceWorker.register('./sw.js').catch(()=>{});

// Compile-time QA build only. Production bundles have no gate bypass or story skip API.
if(import.meta.env.MODE==='qa')window.__schlafwald={start,enterForest,visitAnimal,view,clock,audio,get settings(){return settings;},snapshot:()=>({phase,mode,visitProgress:{...visitProgress,sleeping:[...visitProgress.sleeping]},voiceSource:audio.source,activeId,stepIndex,assists,hints,assistedSteps:[...assistedSteps],completedSteps:[...completedSteps],sleeping:[...sleeping],active:[...active],paused:[...clock.paused],line:currentLine,time:clock.time,step:view?.step?{type:view.step.config.type,progress:view.step.progress,complete:view.step.complete,items:view.step.items.filter(i=>!i.done).map(i=>view.project(i.pos)),target:view.project(view.step.target)}:null,stats:view?.stats(),frames:frameTimes}),pause:(value)=>pause('qa',value),openParents:()=>{pause('parent',true);renderSettings();$('#parents').showModal();},loseContext:()=>view?.renderer.forceContextLoss(),restoreContext:()=>view?.renderer.forceContextRestore()};
