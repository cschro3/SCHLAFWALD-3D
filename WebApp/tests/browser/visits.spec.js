import {test,expect} from '@playwright/test';
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
 await load(page);await page.evaluate(()=>window.__schlafwald.openParents());await expect(page.locator('#set-voice option')).toHaveCount(2);await page.locator('#set-voice').selectOption('test:de');await page.locator('#voice-preview').click();await expect(page.locator('#voice-preview')).toHaveText('Stimme anhören');expect(await page.evaluate(()=>window.__spoken.at(-1).voice)).toBe('test:de');await page.locator('#parents-done').click();await page.reload();await page.waitForFunction(()=>window.__schlafwald);expect(await page.evaluate(()=>window.__schlafwald.settings.voiceURI)).toBe('test:de');
});
