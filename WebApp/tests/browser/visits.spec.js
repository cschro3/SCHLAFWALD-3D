import {test,expect} from '@playwright/test';
import {writeFile} from 'node:fs/promises';
const snap=page=>page.evaluate(()=>window.__schlafwald.snapshot());
async function load(page){const errors=[];page.on('pageerror',e=>errors.push(e.message));await page.goto('/');await page.waitForFunction(()=>window.__schlafwald?.view?.renderedFrames>1);return errors;}
async function forest(page){await page.evaluate(()=>window.__schlafwald.enterForest({scale:180,silent:true}));await expect(page.locator('#forest-menu')).toBeVisible();}
test('the child enters a real 3D clearing and freely chooses any of the seven friends',async({page})=>{
 const errors=await load(page);await page.locator('#start').click();await expect(page.locator('[data-visit]')).toHaveCount(7);for(const b of await page.locator('[data-visit]').all())await expect(b).toBeInViewport();
 await page.waitForTimeout(700);expect((await snap(page)).phase).toBe('forest');await forest(page);await page.locator('[data-visit="nuri"]').click();await page.waitForFunction(()=>window.__schlafwald.snapshot().line.includes('ich bin Nuri'));expect((await snap(page)).activeId).toBe('nuri');expect(errors).toEqual([]);
});
test('finishing Suri returns no automatic next chapter and another visit preserves her sleep',async({page})=>{
 await load(page);await forest(page);await page.locator('[data-visit="suri"]').click();await page.waitForFunction(()=>window.__schlafwald.snapshot().phase==='visit-ended',null,{timeout:100000});
 expect((await snap(page)).visitProgress.sleeping).toEqual(['suri']);await page.waitForTimeout(700);expect((await snap(page)).activeId).toBe('suri');await page.locator('#visit-home').click();await expect(page.locator('[data-visit="suri"]')).toHaveClass(/is-asleep/);
 await page.locator('[data-visit="fino"]').click();await page.waitForFunction(()=>window.__schlafwald.snapshot().phase==='visit-ended',null,{timeout:100000});expect((await snap(page)).visitProgress.sleeping).toEqual(['fino','suri']);
 await page.reload();await page.waitForFunction(()=>window.__schlafwald);await page.locator('#continue').click();await expect(page.locator('[data-visit="fino"]')).toHaveClass(/is-asleep/);await expect(page.locator('[data-visit="suri"]')).toHaveClass(/is-asleep/);
});
test('leaving an unfinished visit cancels its narration and leaves that animal awake',async({page})=>{
 await load(page);await forest(page);await page.locator('[data-visit="mila"]').click();await page.waitForFunction(()=>window.__schlafwald.snapshot().line.length>0);await page.locator('#visit-back').click();await page.waitForTimeout(1000);const s=await snap(page);expect(s.mode).toBe('forest');expect(s.line).toBe('');expect(s.visitProgress.sleeping).toEqual([]);expect(s.step).toBeNull();
});
test('a sleeping friend can be visited quietly and explicitly retold without duplicating progress',async({page})=>{
 await page.addInitScript(()=>localStorage.setItem('sw_visits_v1',JSON.stringify({sleeping:['suri'],updated:Date.now(),finished:false})));await load(page);await forest(page);await page.locator('[data-visit="suri"]').click();await expect(page.locator('#visit-finished')).toBeVisible();expect((await snap(page)).phase).toBe('visit-ended');
 await page.locator('#visit-again').click();await page.waitForFunction(()=>window.__schlafwald.snapshot().phase==='visit-ended',null,{timeout:100000});expect((await snap(page)).visitProgress.sleeping).toEqual(['suri']);
});
test('the night may end after one visit without requiring all seven',async({page})=>{
 await load(page);await forest(page);await page.locator('#night-finish').click();await page.waitForFunction(()=>window.__schlafwald.snapshot().phase==='ended');expect((await snap(page)).visitProgress.sleeping).toEqual([]);await expect(page.locator('#finished')).toBeVisible();
});
test('parent voice audition uses the chosen offline German voice; fake speech verifies controls only',async({page})=>{
 await page.addInitScript(()=>{window.__spoken=[];Object.defineProperty(window,'SpeechSynthesisUtterance',{value:class{constructor(text){this.text=text;}}});Object.defineProperty(window,'speechSynthesis',{value:{getVoices:()=>[{name:'Anna Enhanced',lang:'de-DE',localService:true,voiceURI:'test:de'},{name:'English',lang:'en-US',localService:true,voiceURI:'test:en'}],addEventListener(){},pause(){},resume(){},cancel(){},speak(u){window.__spoken.push({text:u.text,lang:u.lang,voice:u.voice.voiceURI});setTimeout(()=>u.onend?.(),50);}}});});
 await load(page);await page.evaluate(()=>window.__schlafwald.openParents());await expect(page.locator('#set-voice option')).toHaveCount(2);await page.locator('#set-voice').selectOption('test:de');await page.locator('#voice-preview').click();await expect(page.locator('#voice-preview')).toHaveText('Gerätestimme anhören');expect(await page.evaluate(()=>window.__spoken.at(-1).voice)).toBe('test:de');await page.locator('#parents-done').click();await page.reload();await page.waitForFunction(()=>window.__schlafwald);expect(await page.evaluate(()=>window.__schlafwald.settings.voiceURI)).toBe('test:de');
});
test('Emma visit recording plays to its real end after an offline restart',async({page,context})=>{
 await load(page);await page.evaluate(()=>navigator.serviceWorker.ready);await page.waitForFunction(()=>navigator.serviceWorker.controller!==null);await context.setOffline(true);await page.reload();await page.waitForFunction(()=>Object.keys(window.__schlafwald?.audio?.visitCoverage?.lines||{}).length===82);
 await page.locator('#start').click();await page.evaluate(()=>{window.__emmaEnded=[];window.__schlafwald.audio.voice.addEventListener('ended',()=>window.__emmaEnded.push(window.__schlafwald.audio.voice.currentSrc));});await page.locator('[data-visit="suri"]').click();await page.waitForFunction(()=>window.__schlafwald.audio.source==='emma-preview-recording'&&window.__schlafwald.audio.voice.currentTime>.1);await page.waitForFunction(()=>window.__emmaEnded.length>0,null,{timeout:45000});expect(await page.evaluate(()=>window.__emmaEnded[0])).toContain('visit_suri_01.mp3');expect(await page.evaluate(()=>window.__schlafwald.audio.failures.size)).toBe(0);
});
test('a saved robot voice cannot override the real Emma MP3',async({page})=>{
 await page.addInitScript(()=>{localStorage.setItem('sw_emma_v1','true');localStorage.setItem('sw_web_settings_v1',JSON.stringify({voiceURI:'stale:robot',voiceMode:'device'}));window.__robotCalls=0;Object.defineProperty(window,'speechSynthesis',{value:{getVoices:()=>[{name:'Robot',lang:'de-DE',localService:true,voiceURI:'stale:robot'}],addEventListener(){},cancel(){},resume(){},pause(){},speak(){window.__robotCalls++;}}});});
 await load(page);await page.locator('#start').click();await page.locator('[data-visit="suri"]').click();await page.waitForFunction(()=>window.__schlafwald.audio.source==='emma-preview-recording'&&window.__schlafwald.audio.voice.currentTime>.1);expect(await page.evaluate(()=>window.__robotCalls)).toBe(0);expect(await page.evaluate(()=>window.__schlafwald.audio.voice.playbackRate)).toBe(1);
});
test('fresh Emma entry bypasses an old cached start page and empty recording manifest',async({page})=>{
 const legacy='const CACHE="schlafwald-legacy-fixture";self.addEventListener("install",e=>e.waitUntil(caches.open(CACHE).then(async c=>{await c.put("/",new Response("<h1>Old robot app</h1>",{headers:{"Content-Type":"text/html"}}));await c.put("/audio/visits/manifest.json",new Response(JSON.stringify({lines:{}})));}).then(()=>self.skipWaiting())));self.addEventListener("activate",e=>e.waitUntil(self.clients.claim()));self.addEventListener("fetch",e=>e.respondWith(caches.open(CACHE).then(async c=>(await c.match(new URL(e.request.url).pathname))||fetch(e.request))));';
 await writeFile(new URL('../../dist/legacy-sw.js',import.meta.url),legacy);
 await load(page);await page.evaluate(async()=>{await navigator.serviceWorker.register('./legacy-sw.js',{scope:'./'});await navigator.serviceWorker.ready;});await page.waitForFunction(()=>navigator.serviceWorker.controller?.scriptURL.endsWith('/legacy-sw.js'));
 await page.goto('/');await expect(page.getByRole('heading',{name:'Old robot app'})).toBeVisible();expect(await page.evaluate(async()=>Object.keys((await (await fetch('./audio/visits/manifest.json')).json()).lines).length)).toBe(0);
 await page.goto('/emma.html');await page.waitForFunction(()=>window.__schlafwald?.view?.renderedFrames>1);expect(await page.evaluate(()=>Object.keys(window.__schlafwald.audio.visitCoverage.lines).length)).toBe(82);await page.locator('#start').click();await page.locator('[data-visit="suri"]').click();await page.waitForFunction(()=>window.__schlafwald.audio.source==='emma-preview-recording'&&window.__schlafwald.audio.voice.currentTime>.1);
});
