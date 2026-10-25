// Capture the actual QA build. No illustration, compositing, or generated screenshot.
// Requires: npx vite build --mode qa; node tools/offline-manifest.mjs
import {spawn} from 'node:child_process';
import {mkdir,writeFile,rename} from 'node:fs/promises';
import {chromium} from '@playwright/test';
const output=process.env.SCHLAFWALD_EVIDENCE||'/tmp/schlafwald-evidence';
await mkdir(output+'/screenshots',{recursive:true});await mkdir(output+'/video',{recursive:true});
const server=spawn(process.execPath,['tools/serve.mjs'],{stdio:'ignore'});
let browser;
const stats={},errors=[];
try{
  for(let n=0;n<100;n++){try{await fetch('http://127.0.0.1:4173');break;}catch{await new Promise(r=>setTimeout(r,100));}}
  browser=await chromium.launch({executablePath:process.env.SCHLAFWALD_CHROMIUM||undefined,args:['--no-sandbox','--use-gl=angle','--use-angle=swiftshader','--enable-unsafe-swiftshader','--disable-dev-shm-usage']});
  const page=await browser.newPage({viewport:{width:1024,height:768},deviceScaleFactor:1});page.on('pageerror',e=>errors.push(e.message));
  await page.goto('http://127.0.0.1:4173');await page.waitForFunction(()=>window.__schlafwald?.view?.renderedFrames>4);
  await page.screenshot({path:output+'/screenshots/tablet-welcome.png'});
  for(const id of ['fino','mila','piko','luna','nuri','leni','suri']){
    await page.evaluate(id=>window.__schlafwald.start({animals:[id],scale:80,silent:true}),id);
    await page.waitForFunction(id=>{const s=window.__schlafwald.snapshot();return s.phase==='ritual'&&s.activeId===id;},id,{timeout:60000});
    await page.evaluate(()=>window.__schlafwald.pause(true));await page.waitForTimeout(1200);
    await page.screenshot({path:output+'/screenshots/'+id+'-ritual.png'});
    stats[id]=await page.evaluate(()=>window.__schlafwald.view.stats());
  }
  await page.evaluate(()=>window.__schlafwald.openParents());await page.screenshot({path:output+'/screenshots/parents-tablet.png'});
  await page.locator('.settings-scroll').evaluate(e=>e.scrollTop=e.scrollHeight);await page.screenshot({path:output+'/screenshots/parents-options.png'});
  for(const [name,width,height] of [['phone-landscape',844,390],['phone-portrait',390,844],['desktop',1440,900]]){
    const p=await browser.newPage({viewport:{width,height},deviceScaleFactor:1,hasTouch:name.startsWith('phone')});p.on('pageerror',e=>errors.push(e.message));await p.goto('http://127.0.0.1:4173');await p.waitForFunction(()=>window.__schlafwald?.view?.renderedFrames>4);await p.screenshot({path:output+'/screenshots/'+name+'.png'});await p.close();
  }
  await page.close();
  const context=await browser.newContext({viewport:{width:1024,height:576},deviceScaleFactor:1,recordVideo:{dir:output+'/video',size:{width:1024,height:576}}});
  const p=await context.newPage();p.on('pageerror',e=>errors.push(e.message));await p.goto('http://127.0.0.1:4173');await p.waitForFunction(()=>window.__schlafwald?.view?.renderedFrames>4);
  await p.evaluate(()=>window.__schlafwald.start({animals:['suri'],scale:14,silent:true}));
  let down=false,roof=false,sleep=false;const deadline=Date.now()+150000;
  while(Date.now()<deadline){
    const s=await p.evaluate(()=>window.__schlafwald.snapshot());
    if(s.phase==='ended')break;
    if(!roof&&s.activeId==='suri'&&s.stepIndex===2){await p.screenshot({path:output+'/screenshots/suri-leaf-roof.png'});roof=true;}
    if(!sleep&&s.sleeping.includes('suri')){await p.evaluate(()=>window.__schlafwald.pause(true));await p.screenshot({path:output+'/screenshots/suri-asleep.png'});await p.waitForTimeout(900);await p.evaluate(()=>window.__schlafwald.pause(false));sleep=true;}
    const step=s.step;
    if(s.phase==='ritual'&&step&&!step.complete&&step.items.length){
      const point=step.items[0];
      if(step.type==='DragToTarget'){
        if(down){await p.mouse.up();down=false;}await p.mouse.move(point.x,point.y);await p.mouse.down();await p.mouse.move(step.target.x,step.target.y,{steps:12});await p.mouse.up();
      }else{await p.mouse.move(point.x,point.y);if(!down){await p.mouse.down();down=true;}}
    }else if(down){await p.mouse.up();down=false;}
    await p.waitForTimeout(60);
  }
  const end=await p.evaluate(()=>window.__schlafwald.snapshot());stats.suriRecorded={phase:end.phase,assists:end.assists,completed:end.completedSteps};
  if(end.phase!=='ended'||end.assists!==0)throw Error('Recorded Suri run did not finish through pointer input');
  await p.waitForTimeout(800);const video=p.video(),path=await video.path();await context.close();await rename(path,output+'/video/suri-actual-run.webm');
  await writeFile(output+'/render-metrics.json',JSON.stringify({renderer:'Chromium ANGLE SwiftShader; software rendering, not a mobile performance test',captureViewport:'1024x768; mobile viewport captures 844x390 and 390x844',video:'Actual browser render; QA story clock 14x; video has no audio',stats,errors},null,2));
  if(errors.length)throw Error(errors.join('\n'));
  console.log('Captured all seven live 3D scenes, phone/tablet/desktop, parent UI and Suri video.');
}finally{await browser?.close();server.kill();}
