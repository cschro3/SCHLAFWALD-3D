import {test,expect} from '@playwright/test';
import {IDS} from '../../src/core.js';

async function load(page){const errors=[];page.on('pageerror',e=>errors.push(e.message));await page.goto('/');await page.waitForFunction(()=>window.__schlafwald?.view?.renderedFrames>1);return errors;}
async function start(page,animals=IDS,scale=100){await page.evaluate(({animals,scale})=>window.__schlafwald.start({animals,scale,silent:true}),{animals,scale});}
async function snapshot(page){return page.evaluate(()=>window.__schlafwald.snapshot());}
async function bot(page,{capture=null}={}){
  const seen=new Set(),end=Date.now()+150000;let down=false,lastKey='';
  while(Date.now()<end){
    const s=await snapshot(page);if(s.phase==='ended'){if(down)await page.mouse.up();return {state:s,seen:[...seen]};}
    const step=s.step,key=s.activeId+':'+s.stepIndex;
    if(s.phase!=='ritual'||!step||step.complete){if(down){await page.mouse.up();down=false;}await page.waitForTimeout(40);continue;}
    seen.add(key);
    if(capture&&lastKey!==key){await page.screenshot({path:`${capture}/${s.activeId}-step-${s.stepIndex}.png`});lastKey=key;}
    const p=step.items[0];if(!p){await page.waitForTimeout(40);continue;}
    expect(p.visible).toBe(true);
    if(step.type==='DragToTarget'){
      if(down){await page.mouse.up();down=false;}
      await page.mouse.move(p.x,p.y);await page.mouse.down();
      await page.mouse.move(step.target.x,step.target.y,{steps:8});await page.mouse.up();
    }else if(step.type==='Stroke'){
      await page.mouse.move(p.x-35,p.y);if(!down){await page.mouse.down();down=true;}
      await page.mouse.move(p.x+35,p.y,{steps:7});await page.mouse.move(p.x-35,p.y,{steps:7});
    }else{await page.mouse.move(p.x,p.y);if(!down){await page.mouse.down();down=true;}}
    await page.waitForTimeout(45);
  }
  throw new Error('Story did not finish: '+JSON.stringify(await snapshot(page)));
}

test('Suri: all three rituals through real pointer input, followed by shell retraction',async({page})=>{
  const errors=await load(page);await start(page,['suri'],32);const {state,seen}=await bot(page);expect(seen).toEqual(['suri:0','suri:1','suri:2']);expect(state.sleeping).toEqual(['suri']);expect(state.assists).toBe(0);
  expect(await page.evaluate(()=>window.__schlafwald.view.animal.retract)).toBeGreaterThan(.98);expect(errors).toEqual([]);
});
test('all seven complete without input; each ritual eventually assists once',async({page})=>{
  await load(page);await start(page,IDS,180);await page.waitForFunction(()=>window.__schlafwald.snapshot().phase==='ended',null,{timeout:150000});const s=await snapshot(page);expect(s.sleeping).toEqual(IDS);expect(s.assists).toBe(10);
});
test('all seven complete using real pointer input and every ritual type',async({page})=>{
  const errors=await load(page);await start(page,IDS,50);const {state,seen}=await bot(page);expect(state.sleeping).toEqual(IDS);expect(seen.length).toBe(10);expect(state.assists).toBe(0);expect(errors).toEqual([]);
});
test('touches outside the target never punish or prevent gentle completion',async({page})=>{
  await load(page);await start(page,['suri'],150);for(let i=0;i<35;i++){await page.mouse.click(6,100);await page.waitForTimeout(80);}await page.waitForFunction(()=>window.__schlafwald.snapshot().phase==='ended',null,{timeout:100000});expect((await snapshot(page)).sleeping).toEqual(['suri']);
});
test('pause freezes narration time and animation; resume continues',async({page})=>{
  await load(page);await start(page,['suri'],15);await page.waitForTimeout(400);await page.evaluate(()=>window.__schlafwald.pause(true));const before=await snapshot(page),pose=await page.evaluate(()=>window.__schlafwald.view.animal.head.rotation.z);await page.waitForTimeout(650);expect((await snapshot(page)).time).toBe(before.time);expect(await page.evaluate(()=>window.__schlafwald.view.animal.head.rotation.z)).toBe(pose);await page.evaluate(()=>window.__schlafwald.pause(false));await page.waitForTimeout(500);expect((await snapshot(page)).time).toBeGreaterThan(before.time);
});
test('parent gate requires holding then arithmetic; settings persist and selection cannot empty',async({page})=>{
  await load(page);await page.getByRole('button',{name:/Elternbereich/}).click();await expect(page.locator('#gate')).not.toBeVisible();
  const b=await page.locator('#parent-open').boundingBox();await page.mouse.move(b.x+b.width/2,b.y+b.height/2);await page.mouse.down();await page.waitForTimeout(3200);await page.mouse.up();await expect(page.locator('#gate')).toBeVisible();
  const question=await page.locator('#challenge').innerText(),[a,c]=question.match(/\d+/g).map(Number);await page.locator('#answers').getByRole('button',{name:String(a*c),exact:true}).click();await expect(page.locator('#parents')).toBeVisible();
  await page.locator('#set-master').fill('37');await page.locator('#set-master').dispatchEvent('input');for(const c of await page.locator('[name="animal"]').all())await c.uncheck();expect(await page.locator('[name="animal"]:checked').count()).toBe(1);await page.locator('#parents-done').click();await page.reload();await page.waitForFunction(()=>window.__schlafwald);expect(await page.evaluate(()=>window.__schlafwald.settings.master)).toBe(.37);
});
test('a fully cached build restarts offline with local speech files',async({page,context})=>{
  await load(page);await page.evaluate(()=>navigator.serviceWorker.ready);await page.waitForFunction(()=>navigator.serviceWorker.controller!==null);await context.setOffline(true);await page.reload();await page.waitForFunction(()=>window.__schlafwald?.audio?.available===101);expect(await page.locator('#start').isVisible()).toBe(true);expect(await page.evaluate(async()=>{const r=await fetch('./audio/narration/suri_o4.mp3');return r.ok&&(await r.arrayBuffer()).byteLength>500;})).toBe(true);await start(page,['suri'],180);await page.waitForFunction(()=>window.__schlafwald.snapshot().phase==='ended',null,{timeout:100000});
});
test('an unreadable save and unavailable narration recover without a crash',async({page})=>{
  await page.addInitScript(()=>localStorage.setItem('sw_web_progress_v1','{broken'));
  await page.route('**/audio/manifest.json',r=>r.fulfill({status:404,body:'missing'}));const errors=await load(page);await start(page,['suri'],180);await page.waitForFunction(()=>window.__schlafwald.snapshot().phase==='ritual');expect(await page.evaluate(()=>window.__schlafwald.audio.available)).toBe(0);await page.waitForFunction(()=>window.__schlafwald.snapshot().phase==='ended',null,{timeout:100000});expect(errors).toEqual([]);
});
test('touch pointer events drive the shell interaction and ignore a second finger',async({page})=>{
  await load(page);await start(page,['suri'],100);await page.waitForFunction(()=>window.__schlafwald.snapshot().stepIndex===2,null,{timeout:100000});
  const p=(await snapshot(page)).step.items[0],client=await page.context().newCDPSession(page);
  await client.send('Emulation.setTouchEmulationEnabled',{enabled:true});
  await client.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{x:p.x,y:p.y,id:1}]});
  await client.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{x:p.x,y:p.y,id:1},{x:12,y:120,id:2}]});
  await page.waitForTimeout(500);await client.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]});
  await page.waitForFunction(()=>window.__schlafwald.snapshot().phase==='ended',null,{timeout:100000});expect((await snapshot(page)).sleeping).toEqual(['suri']);expect((await snapshot(page)).assists).toBe(2);
});
