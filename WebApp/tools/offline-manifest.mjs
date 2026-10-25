import {readdir,readFile,writeFile} from 'node:fs/promises';
import {createHash} from 'node:crypto';
async function walk(dir){const result=[];for(const e of await readdir(dir,{withFileTypes:true})){const p=`${dir}/${e.name}`;if(e.isDirectory())result.push(...await walk(p));else if(e.name!=='sw.js')result.push(p);}return result;}
const files=(await walk('dist')).sort();const hash=createHash('sha256');for(const f of files)hash.update(await readFile(f));
const cache='schlafwald-'+hash.digest('hex').slice(0,16);
const paths=files.map(p=>'./'+p.slice(5));paths.push('./');
await writeFile('dist/sw.js',`const CACHE=${JSON.stringify(cache)},FILES=${JSON.stringify(paths)};
self.addEventListener('install',e=>{e.waitUntil(caches.open(CACHE).then(c=>c.addAll(FILES)).then(()=>self.skipWaiting()));});
self.addEventListener('activate',e=>{e.waitUntil(caches.keys().then(keys=>Promise.all(keys.filter(k=>k.startsWith('schlafwald-')&&k!==CACHE).map(k=>caches.delete(k)))).then(()=>self.clients.claim()));});
self.addEventListener('fetch',e=>{const u=new URL(e.request.url);if(u.origin!==self.location.origin||e.request.method!=='GET')return;e.respondWith((async()=>{const c=await caches.open(CACHE);const hit=await c.match(u.pathname);if(hit){const range=e.request.headers.get('Range');if(range){const b=await hit.arrayBuffer(),m=/bytes=(\\d+)-(\\d*)/.exec(range);if(m){const start=+m[1],end=m[2]?Math.min(+m[2],b.byteLength-1):b.byteLength-1;if(start>=b.byteLength)return new Response(null,{status:416});return new Response(b.slice(start,end+1),{status:206,headers:{'Content-Type':hit.headers.get('Content-Type')||'audio/mpeg','Content-Range':'bytes '+start+'-'+end+'/'+b.byteLength,'Content-Length':end-start+1,'Accept-Ranges':'bytes'}});}}return hit;}try{return await fetch(e.request);}catch(err){if(e.request.mode==='navigate')return (await c.match('./index.html'))||Response.error();return Response.error();}})());});`);
console.log(`Offline cache: ${files.length} assets, ${cache}`);
