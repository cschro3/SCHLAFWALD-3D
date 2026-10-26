import {spawn} from 'node:child_process';
import {mkdir,writeFile} from 'node:fs/promises';
import {chromium} from '@playwright/test';
const output=(process.env.SCHLAFWALD_EVIDENCE||'/tmp/schlafwald-visits-evidence')+'/visits';await mkdir(output,{recursive:true});
async function freeze(p){await p.evaluate(()=>{window.__captureRender=window.__schlafwald.view.render;window.__schlafwald.view.render=()=>{};});}
async function thaw(p){await p.evaluate(()=>{window.__schlafwald.view.render=window.__captureRender;});}
const server=spawn(process.execPath,['tools/serve.mjs'],{stdio:'ignore'});let browser;const errors=[],metrics={};
try{
 for(let i=0;i<100;i++){try{await fetch('http://127.0.0.1:4173');break;}catch{await new Promise(r=>setTimeout(r,100));}}
 browser=await chromium.launch({executablePath:process.env.SCHLAFWALD_CHROMIUM||undefined,args:['--no-sandbox','--use-gl=angle','--use-angle=swiftshader','--enable-unsafe-swiftshader','--disable-dev-shm-usage']});
 for(const [name,width,height] of [['forest-tablet',1024,768],['forest-phone',844,390],['forest-portrait',390,844],['forest-desktop',1440,900]]){
  const p=await browser.newPage({viewport:{width,height},deviceScaleFactor:1});p.on('pageerror',e=>errors.push(e.message));await p.goto('http://127.0.0.1:4173');await p.waitForFunction(()=>window.__schlafwald?.view?.renderedFrames>2);await p.locator('#start').click();await p.waitForFunction(()=>window.__schlafwald.snapshot().phase==='forest');await p.waitForTimeout(1500);await freeze(p);await p.screenshot({timeout:120000,path:output+'/'+name+'.png'});metrics[name]=await p.evaluate(()=>window.__schlafwald.view.stats());await p.close();
 }
 const p=await browser.newPage({viewport:{width:1024,height:768},deviceScaleFactor:1});p.on('pageerror',e=>errors.push(e.message));await p.goto('http://127.0.0.1:4173');await p.waitForFunction(()=>window.__schlafwald);await p.evaluate(()=>window.__schlafwald.enterForest({scale:30,silent:true}));await p.locator('[data-visit="suri"]').click();await p.waitForFunction(()=>{const s=window.__schlafwald.snapshot();return s.phase==='intro'&&s.line.length>0;});await p.evaluate(()=>window.__schlafwald.pause(true));await p.waitForTimeout(1200);await freeze(p);await p.screenshot({timeout:120000,path:output+'/suri-first-person.png'});await thaw(p);await p.evaluate(()=>{window.__schlafwald.clock.scale=180;window.__schlafwald.pause(false);});await p.waitForFunction(()=>window.__schlafwald.snapshot().phase==='visit-ended',null,{timeout:100000});await freeze(p);await p.screenshot({timeout:120000,path:output+'/suri-visit-ended.png'});await p.evaluate(()=>window.__schlafwald.openParents());await freeze(p);await p.screenshot({timeout:120000,path:output+'/parents-device-voice.png'});
 await writeFile(output+'/evidence.json',JSON.stringify({type:'Actual Chromium/SwiftShader renders. Device speech quality is not tested by these silent QA captures.',metrics,errors},null,2));if(errors.length)throw Error(errors.join('; '));console.log('Captured free animal visits, first-person story, completed visit and honest device-voice state.');
}finally{await browser?.close();server.kill();}
