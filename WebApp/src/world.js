import * as T from 'three';
import {mergeGeometries} from 'three/addons/utils/BufferGeometryUtils.js';
import {clamp,smooth,atmosphere} from './core.js';

const v=(x=0,y=0,z=0)=>new T.Vector3(x,y,z);
const sphere=new T.SphereGeometry(1,24,16);
const lowSphere=new T.SphereGeometry(1,12,8);
const materials=new Map();
function mat(color,roughness=0.8,emissive=0){const key=`${color}/${roughness}/${emissive}`;if(!materials.has(key))materials.set(key,new T.MeshStandardMaterial({color,roughness,emissive,emissiveIntensity:emissive?0.45:0}));return materials.get(key);}
function mesh(parent,name,geometry,material,pos=[0,0,0],scale=[1,1,1]){const o=new T.Mesh(geometry,material);o.name=name;o.position.set(...pos);o.scale.set(...scale);parent.add(o);return o;}
function blob(parent,name,color,pos,scale,detail=true){return mesh(parent,name,detail?sphere:lowSphere,mat(color),pos,scale);}
function group(parent,name,pos=[0,0,0]){const g=new T.Group();g.name=name;g.position.set(...pos);parent.add(g);return g;}
function tube(parent,name,points,radius,color,segments=24){const c=new T.CatmullRomCurve3(points.map(p=>v(...p)));const g=new T.TubeGeometry(c,segments,radius,7,false);return mesh(parent,name,g,mat(color));}
function branch(parent,a,b,r,color='#6f5752'){const dir=v(...b).sub(v(...a));const m=mesh(parent,'branch',new T.CylinderGeometry(r*.62,r,dir.length(),9),mat(color));m.position.copy(v(...a).add(v(...b)).multiplyScalar(.5));m.quaternion.setFromUnitVectors(v(0,1,0),dir.normalize());return m;}
function eyePair(parent,{spacing=.18,y=.08,z=.26,r=.115}={}){
  const eyes=[];
  for(const side of [-1,1]){
    const e=group(parent,'Eye'+(side<0?'L':'R'),[spacing*side,y,z]);
    blob(e,'white','#fff5df',[0,0,0],[r,r*1.12,r*.60]);
    blob(e,'iris','#414b50',[.012,-.006,r*.49],[r*.66,r*.8,r*.32]);
    blob(e,'pupil','#1c2933',[.015,-.003,r*.7],[r*.45,r*.60,r*.23]);
    mesh(e,'spark',sphere,new T.MeshBasicMaterial({color:'#fff9ea'}),[-r*.18,r*.32,r*.88],[r*.19,r*.22,r*.12]);
    const lid=tube(e,'ClosedLid',[[-r*.8,0,r*.54],[0,-r*.16,r*.7],[r*.8,0,r*.54]],.012,'#463c43',10);lid.visible=false;
    eyes.push(e);
  }
  return eyes;
}
function face(parent,color,pos,scale,eyeOptions={}){
  const g=group(parent,'Head',pos);blob(g,'head',color,[0,0,0],scale);
  const eyes=eyePair(g,eyeOptions);
  for(const s of [-1,1])blob(g,'blush','#df9794',[s*(eyeOptions.spacing??.18)*1.44,-.09,(eyeOptions.z??.26)*.98],[.085,.045,.022]);
  return {g,eyes};
}
function leaf(parent,color='#a1ba78',size=1){
  const shape=new T.Shape();shape.moveTo(0,-.67);shape.bezierCurveTo(-.49,-.35,-.64,.22,0,.76);shape.bezierCurveTo(.56,.25,.48,-.36,0,-.67);
  const geo=new T.ExtrudeGeometry(shape,{depth:.045,bevelEnabled:true,bevelSegments:2,steps:1,bevelSize:.045,bevelThickness:.025,curveSegments:12});geo.rotateX(-Math.PI/2);
  const height=(x,z)=>.21*(1-(x/.58)**2)-.13*(z/.76)**2;
  const positions=geo.attributes.position;for(let i=0;i<positions.count;i++)positions.setY(i,positions.getY(i)+height(positions.getX(i),positions.getZ(i)));geo.computeVertexNormals();
  const g=group(parent,'leaf');mesh(g,'leafSurface',geo,mat(color));
  const onLeaf=(x,z)=>[x,height(x,z)+.064,z];
  tube(g,'vein',[onLeaf(0,.62),onLeaf(0,0),onLeaf(0,-.71)],.014,'#cfda9b',12);
  for(const s of [-1,1])for(let i=0;i<3;i++)tube(g,'vein',[onLeaf(0,.3-i*.26),onLeaf(s*.22,.12-i*.22),onLeaf(s*.31,-.03-i*.2)],.007,'#c3d09a',8);
  batchStatic(g);
  g.scale.setScalar(size);return g;
}
function pointedEar(parent,color,inner,height=.48,width=.18){
  const profile=[[width*.7,0],[width,.10*height],[width*.8,.47*height],[width*.35,.83*height],[.012,height]].map(([x,y])=>new T.Vector2(x,y));
  const outer=mesh(parent,'tapered ear',new T.LatheGeometry(profile,16),mat(color));outer.scale.z=.62;
  const lining=mesh(parent,'soft inner ear',new T.LatheGeometry(profile,16),mat(inner),[0,.035,.065],[.61,.79,.22]);return outer;
}
function rng(seed=42){return()=>{seed|=0;seed=seed+0x6D2B79F5|0;let t=Math.imul(seed^seed>>>15,1|seed);t=t+Math.imul(t^t>>>7,61|t)^t;return((t^t>>>14)>>>0)/4294967296;};}
function batchStatic(root,recursive=true){
  root.updateMatrixWorld(true);const byMaterial=new Map(),inverse=root.matrixWorld.clone().invert();
  const collect=o=>{for(let ancestor=o;ancestor;ancestor=ancestor.parent){if(!ancestor.visible)return;if(ancestor===root)break;}if(!o.isMesh||o.userData.animated||o.isInstancedMesh||!o.material.isMeshStandardMaterial||Array.isArray(o.material))return;const key=o.material.uuid;if(!byMaterial.has(key))byMaterial.set(key,[]);byMaterial.get(key).push(o);};
  if(recursive)root.traverse(collect);else [...root.children].forEach(collect);
  for(const objects of byMaterial.values()){
    if(objects.length<2)continue;
    const geos=objects.map(o=>o.geometry.clone().applyMatrix4(inverse.clone().multiply(o.matrixWorld)));
    const geo=mergeGeometries(geos);geos.forEach(g=>g.dispose());
    if(!geo)continue;const m=new T.Mesh(geo,objects[0].material);m.name='batched scenery';root.add(m);objects.forEach(o=>o.parent?.remove(o));
  }
}

export class ForestView {
  constructor(canvas){
    this.canvas=canvas;this.scene=new T.Scene();this.scene.fog=new T.Fog('#53646e',12,39);
    this.renderer=new T.WebGLRenderer({canvas,antialias:true,alpha:false,powerPreference:'low-power',preserveDrawingBuffer:true});
    this.renderer.setPixelRatio(Math.min(window.devicePixelRatio||1,1.5));this.renderer.outputColorSpace=T.SRGBColorSpace;this.renderer.toneMapping=T.ACESFilmicToneMapping;this.renderer.toneMappingExposure=1.12;
    this.camera=new T.PerspectiveCamera(42,1,.05,90);this.camera.position.set(3.8,3.2,8.2);this.look=v(0,.85,0);this.camera.lookAt(this.look);
    this.cameraTarget=this.camera.position.clone();this.lookTarget=this.look.clone();
    this.hemi=new T.HemisphereLight('#e1d5e8','#42615a',2.5);this.scene.add(this.hemi);
    this.key=new T.DirectionalLight('#ffe1b5',3.1);this.key.position.set(-4,7,5);this.scene.add(this.key);
    const rim=new T.DirectionalLight('#a1cce5',1.5);rim.position.set(4,5,-5);this.scene.add(rim);
    this.habitat=group(this.scene,'habitat');this.props=group(this.scene,'ritualProps');this.clearing=group(this.scene,'Besuchslichtung');this.clearing.visible=false;this.clearingEntries=[];this.fireflies=[];this.leaves=[];this.time=0;
    this.buildForest();this.resize();
    this.onResize=()=>this.resize();window.addEventListener('resize',this.onResize);
    this.ray=new T.Raycaster();this.plane=new T.Plane(v(0,1,0),-.14);
    this.step=null;this.animals=new Map();this.calm=0;this.renderedFrames=0;
    this.canvas.addEventListener('webglcontextlost',e=>{e.preventDefault();this.contextLost=true;this.onContextLost?.();});
    this.canvas.addEventListener('webglcontextrestored',()=>{this.contextLost=false;this.onContextRestored?.();});
  }
  resize(){const w=this.canvas.clientWidth,h=this.canvas.clientHeight;this.renderer.setSize(w,h,false);this.camera.aspect=w/h;this.camera.fov=w/h<1.5?52:42;this.camera.updateProjectionMatrix();if(this.welcome)this.setWelcomeLayout(true);if(this.isClearing)this.layoutClearing();}
  setWelcomeLayout(enabled){this.welcome=enabled;if(!enabled)return;const portrait=this.camera.aspect<1;this.camera.position.set(...(portrait?[1.6,2.6,7.5]:[2.7,2.3,7]));this.look.set(...(portrait?[.65,-.15,0]:[-.85,.8,0]));this.cameraTarget.copy(this.camera.position);this.lookTarget.copy(this.look);this.camera.lookAt(this.look);}
  buildForest(){
    const skyGeo=new T.SphereGeometry(65,32,16);
    this.sky=new T.ShaderMaterial({side:T.BackSide,depthWrite:false,uniforms:{night:{value:0}},vertexShader:'varying vec3 d;void main(){d=position;gl_Position=projectionMatrix*modelViewMatrix*vec4(position,1.);}',fragmentShader:'varying vec3 d;uniform float night;void main(){float h=clamp(normalize(d).y,0.,1.);vec3 lo=mix(vec3(.65,.48,.48),vec3(.14,.25,.34),night);vec3 hi=mix(vec3(.13,.27,.34),vec3(.04,.10,.19),night);gl_FragColor=vec4(mix(lo,hi,pow(h,.45)),1.);}' });
    mesh(this.scene,'DuskSky',skyGeo,this.sky);
    const groundGeo=new T.PlaneGeometry(120,100,70,70);groundGeo.rotateX(-Math.PI/2);const p=groundGeo.attributes.position;
    const colors=[];const green=new T.Color('#546e50'),path=new T.Color('#9b8e6c');
    for(let i=0;i<p.count;i++){const x=p.getX(i),z=p.getZ(i),d=Math.abs(x-Math.sin(z*.11)*1.7);p.setY(i,(Math.sin(x*.24+z*.12)+Math.cos(z*.22))*Math.min(.7,Math.max(0,Math.abs(x)-5)*.15)-.11);const c=green.clone().lerp(path,Math.max(0,1-d/1.4)*.48).multiplyScalar(.87+.12*Math.sin(x*2+z*.7));colors.push(c.r,c.g,c.b);}
    groundGeo.setAttribute('color',new T.Float32BufferAttribute(colors,3));groundGeo.computeVertexNormals();mesh(this.scene,'moss floor',groundGeo,new T.MeshStandardMaterial({vertexColors:true,roughness:1}));
    const r=rng();
    for(let i=0;i<56;i++){
      const x=(r()-.5)*49,z=-2-r()*38;if(Math.abs(x)<4.5&&z>-14)continue;
      const tree=group(this.scene,'tree',[x,0,z]);const h=4.4+r()*6.5,rad=.25+r()*.4;
      branch(tree,[0,0,0],[.12,h,0],rad,'#665354');
      for(let j=0;j<3;j++){const a=j*2.3+r();branch(tree,[.08,h*.55,0],[Math.cos(a)*1.5,h*.82,Math.sin(a)*1.2],rad*.45);}
      for(let j=0;j<4;j++)blob(tree,'canopy',['#477264','#577b66','#66836b','#7b8d71'][i%4],[(r()-.5)*2,h-1+j*.58,(r()-.5)*1.7],[1.7+r(),1.1+r(),1.5+r()],false);
    }
    // Large foreground trunks frame the clearing, never the touch targets.
    for(const [x,z,h] of [[-6,1,12],[6.8,-.4,11],[-4.9,-7,9],[6,-9,10]]){
      const g=group(this.scene,'framingTree',[x,0,z]);branch(g,[0,-.5,0],[x<0?.5:-.4,h,0],.65);
      branch(g,[0,h*.55,0],[x<0?3:-3,h*.82,0],.28);for(let j=0;j<4;j++)blob(g,'oakCrown','#547568',[x<0?j*.7:-j*.7,h-1.2,0],[2.4,1.1,2],false);
      for(let j=0;j<3;j++)branch(g,[0,.15,0],[(j-1)*.95,-.07,.9],.15);
    }
    const grassGeo=new T.SphereGeometry(1,5,5),grass=new T.InstancedMesh(grassGeo,mat('#719368'),470);const o=new T.Object3D();
    for(let i=0;i<470;i++){let x=(r()-.5)*25,z=(r()-.5)*25-5;if(Math.abs(x)<2.5&&z>-3) x+=x>0?3:-3;o.position.set(x,.03,z);o.scale.set(.03,.12+r()*.22,.025);o.rotation.z=(r()-.5)*.8;o.updateMatrix();grass.setMatrixAt(i,o.matrix);}this.scene.add(grass);
    for(let i=0;i<20;i++){const x=(r()-.5)*14,z=-1-r()*12;if(Math.abs(x)<2.2&&z>-3)continue;this.mushrooms(x,z,.5+r()*.5);}
    for(const [x,z,s] of [[-3.1,1.2,1.5],[3.8,.6,1.2],[-2.9,-1.7,1],[2.6,-2.6,1.5]])this.fern(x,z,s);
    for(let i=0;i<8;i++){const a=i*1.2;blob(this.scene,'river stone','#70807b',[Math.sin(a)*5.4,-.06,Math.cos(a)*4.8-1],[.8,.3,.65],false);}
    const glow=new T.MeshBasicMaterial({color:'#f8dfa0',transparent:true,opacity:.8});
    for(let i=0;i<36;i++){const m=mesh(this.scene,'firefly',lowSphere,glow,[(r()-.5)*16,.7+r()*4,-2-r()*15],[.025,.025,.025]);this.fireflies.push({m,p:m.position.clone(),phase:r()*7});}
    mesh(this.scene,'moon',sphere,new T.MeshBasicMaterial({color:'#f7e3bb'}),[9,12,-24],[1.1,1.1,1.1]);
    batchStatic(this.scene);
    const starsGeo=new T.BufferGeometry();const stars=[];for(let i=0;i<140;i++){stars.push((r()-.5)*80,10+r()*34,-25-r()*20);}starsGeo.setAttribute('position',new T.Float32BufferAttribute(stars,3));this.stars=new T.Points(starsGeo,new T.PointsMaterial({color:'#fff2cf',size:.07,transparent:true,opacity:.5}));this.scene.add(this.stars);
  }
  fern(x,z,size){const g=group(this.scene,'fern',[x,.02,z]);g.scale.setScalar(size);for(let j=0;j<7;j++){const a=j/7*Math.PI*2;for(let k=0;k<4;k++){const q=(k+1)*.14;const l=blob(g,'frond',j%2?'#71936c':'#86a778',[Math.cos(a)*q,.12+Math.sin(q*2)*.25,Math.sin(a)*q],[.09,.025,.19],false);l.rotation.y=-a+Math.PI/2;l.rotation.z=k*.07;}}}
  mushrooms(x,z,size){const g=group(this.scene,'mushrooms',[x,0,z]);g.scale.setScalar(size);for(let i=0;i<3;i++){const a=i*2.3,h=.16+i*.09;blob(g,'stem','#cfc6a1',[Math.sin(a)*.3,h*.5,Math.cos(a)*.2],[.05,h*.6,.05],false);const cap=blob(g,'cap',i%2?'#c59576':'#b3837d',[Math.sin(a)*.3,h,Math.cos(a)*.2],[.18,h*.4,.17],false);for(let j=0;j<3;j++)blob(cap,'spot','#e9d7b2',[(j-1)*.4,.75,.2],[.10,.05,.13],false);}}
  clear(parent){while(parent.children.length){const c=parent.children[0];parent.remove(c);c.traverse(o=>{if(o.isMesh&&o.geometry!==sphere&&o.geometry!==lowSphere)o.geometry.dispose();});}}
  showAnimal(config){
    this.isClearing=false;this.clearing.visible=false;
    this.clear(this.habitat);this.clear(this.props);this.step=null;this.current=config;this.animal=buildAnimal(config.id,this.habitat);this.animal.config=config;
    this.buildHabitat(config.id);this.cameraTarget.set(config.id==='suri'?3.2:3.6,config.id==='luna'?3.2:2.9,7.7);this.lookTarget.set(0,config.id==='luna'?1.8:.8,0);
  }
  showClearing(configs,sleeping=[]){
    this.welcome=false;this.clear(this.habitat);this.clear(this.props);this.clear(this.clearing);this.step=null;this.animal=null;this.isClearing=true;this.clearing.visible=true;
    this.clearingEntries=configs.map((config,index)=>{
      const node=group(this.clearing,config.id+' home'),home=group(node,'shelter');
      blob(home,'moss bed','#687d55',[0,-.03,0],[1.03,.15,.83]);
      if(config.id==='fino'){
        mesh(home,'den entrance',new T.TorusGeometry(.88,.23,10,20,Math.PI),mat('#77756c'),[0,.05,-.35]);blob(home,'den interior','#394d49',[0,.43,-.45],[.72,.54,.08]);
      }else if(['luna','nuri'].includes(config.id)){
        branch(home,[0,0,-.55],[0,3.0,-.55],.49,'#796454');blob(home,'tree hollow','#38433f',[0,.95,-.04],[.37,.51,.06]);
        if(config.id==='luna')branch(home,[-.8,.92,.1],[.9,.96,.1],.08);
      }else for(let i=0;i<7;i++){const a=i/7*Math.PI*2;blob(home,'nest edge','#91a373',[Math.cos(a)*.82,.07,Math.sin(a)*.56],[.24,.12,.20],false);}
      if(config.id==='suri'&&sleeping.includes(config.id)){for(const x of [-.65,.65])branch(home,[x,0,-.38],[x,1.13,-.38],.025);const roof=leaf(home,'#91aa72',1.1);roof.position.set(0,1.17,0);roof.rotation.y=.4;}
      batchStatic(home);
      const rig=buildAnimal(config.id,node),scale=config.id==='suri'?.70:config.id==='luna'?.61:.72;rig.root.scale.setScalar(scale);rig.root.position.y=config.id==='luna'?.96:config.id==='nuri'?.22:.07;
      const asleep=sleeping.includes(config.id);if(asleep){rig.cue('sleep');for(let i=0;i<160;i++)rig.update(.08,i*.08);}
      rig.root.traverse(o=>{if(o.isMesh)o.userData.animated=false;});batchStatic(rig.root);
      return {id:config.id,node,root:rig.root,scale,asleep,index};
    });this.layoutClearing();
    this.camera.position.copy(this.cameraTarget);this.look.copy(this.lookTarget);this.camera.lookAt(this.look);this.scene.updateMatrixWorld(true);
  }
  layoutClearing(){
    const portrait=this.camera.aspect<1;
    const positions=portrait?[[-2.1,0,-11],[2.1,0,-11],[-2.1,0,-6],[2.1,0,-6],[-2.1,0,-1],[2.1,0,-1],[0,0,4]]:[[-3,0,1],[0,0,1],[3,0,1],[-4.2,0,-5],[-1.4,0,-5],[1.4,0,-5],[4.2,0,-5]];
    for(const [i,e] of this.clearingEntries.entries())e.node.position.set(...positions[i]);
    this.cameraTarget.set(...(portrait?[0,16,17]:[0,8.0,12.6]));this.lookTarget.set(0,.25,portrait?-3:-1.7);
  }
  clearingTargets(){this.clearing.updateMatrixWorld(true);return this.clearingEntries.map(e=>({id:e.id,...this.project(e.node.localToWorld(v(0,-.04,1.9)))}));}
  buildHabitat(id){
    const h=group(this.habitat,'habitat scenery');
    blob(h,'moss island','#687d55',[0,-.06,0],[2.35,.25,1.55]);
    if(id==='suri'){
      blob(h,'sleep stone','#87948a',[0,.02,-.05],[1.55,.19,.9]);
      for(const [x,z] of [[-.8,-.6],[.8,-.6]])branch(h,[x,.08,z],[x,1.47,z],.035);
      this.animal.root.position.set(-1.4,.12,.1);
    }else if(id==='fino'){
      const arch=new T.TorusGeometry(1.55,.46,12,30,Math.PI);mesh(h,'fox den',arch,mat('#77756c'),[0,.1,-.75],[1,1,1]);
      blob(h,'den darkness','#394d49',[0,.65,-1],[1.2,.8,.12]);
    }else if(id==='luna'||id==='nuri'){
      const trunk=group(h,'hollowTree',[0,0,-.9]);branch(trunk,[0,0,0],[0,6,0],1.1,'#796454');
      blob(trunk,'hollow','#38433f',[0,1.3,1.02],[.78,.91,.12]);
      if(id==='luna'){branch(h,[-1.6,1.2,0],[1.9,1.32,0],.15);this.animal.root.position.y=1.15;}
      else{blob(h,'hollow moss','#97a574',[0,.22,.15],[1.2,.20,.75]);this.animal.root.position.y=.16;}
    }else{
      for(let i=0;i<10;i++){const a=i*Math.PI/5;blob(h,'nest moss',i%2?'#7d9163':'#91a373',[Math.cos(a)*1.2,.10,Math.sin(a)*.75],[.39,.18,.33],false);}
      if(id==='piko')for(let i=0;i<9;i++){const l=leaf(h,i%2?'#ae9467':'#bdac76',.35);l.position.set(Math.cos(i*2.4)*1.1,.12,Math.sin(i*2.4)*.7);l.rotation.y=i;}
    }
    for(let i=0;i<5;i++)blob(h,'moss tuft','#93a479',[-1.5+i*.64,.13,-.85],[.28,.12,.24],false);
    batchStatic(h);
  }
  createProp(type){
    const g=group(this.props,type);
    if(type==='leaf'||type==='bigLeaf'){const l=leaf(g,type==='bigLeaf'?'#91aa72':'#c1ab78',type==='bigLeaf'?1.65:.48);l.rotation.y=.4;if(type==='bigLeaf'){l.scale.x*=1.12;l.rotation.z=-.08;}}
    else if(type==='blanket'){
      const shape=new T.Shape();shape.moveTo(-.72,-.65);shape.lineTo(.72,-.65);shape.quadraticCurveTo(.87,-.65,.87,-.50);shape.lineTo(.87,.50);shape.quadraticCurveTo(.87,.65,.72,.65);shape.lineTo(-.72,.65);shape.quadraticCurveTo(-.87,.65,-.87,.50);shape.lineTo(-.87,-.50);shape.quadraticCurveTo(-.87,-.65,-.72,-.65);
      const geo=new T.ExtrudeGeometry(shape,{depth:.035,bevelEnabled:true,bevelSize:.035,bevelThickness:.025,bevelSegments:3,curveSegments:12,steps:1});geo.rotateX(-Math.PI/2);
      const drape=(x,z)=>.18*Math.cos(x*1.7)-.13*(z/.65)**2+.035*Math.cos(x*8+z*2);const p=geo.attributes.position;for(let i=0;i<p.count;i++)p.setY(i,p.getY(i)+drape(p.getX(i),p.getZ(i)));geo.computeVertexNormals();mesh(g,'soft cloth',geo,mat('#a9b4cb'));
      for(let i=0;i<9;i++){const x=-.76+i*.18;tube(g,'stitched hem',[[x,drape(x,.61)+.06,.61],[x+.065,drape(x+.065,.61)+.06,.61]],.010,'#e4d3bf',3);}batchStatic(g);
    }else if(type==='acorn'){
      blob(g,'nut','#bb8c5c',[0,.16,0],[.17,.23,.17]);blob(g,'cap','#705b47',[0,.31,0],[.20,.09,.20]);branch(g,[0,.35,0],[.05,.43,0],.025);
    }else if(type==='mossPillow'){blob(g,'pillow','#92a77a',[0,.08,0],[.53,.17,.36]);for(let i=0;i<7;i++)blob(g,'soft tuft','#a8b48c',[(i%3-1)*.22,.19,Math.floor(i/3)*.17-.17],[.16,.065,.12],false);}
    else {const material=new T.MeshBasicMaterial({color:type==='firefly'?'#ffe2a1':'#f3dfb7'});mesh(g,'guide light',sphere,material,[0,0,0],[.075,.075,.075]);}
    return g;
  }
  beginStep(config,index){
    const previous=this.step; if(previous){for(const h of previous.halos){h.parent?.remove(h);h.geometry.dispose();h.material.dispose();}}
    const a=this.animal;const height=a.config.id==='luna'?1.2:0;
    const count=config.type==='FollowPath'?1:config.points.length;
    const step={config,index,items:[],halos:[],progress:0,done:0,travel:0,stroke:0,assisting:false,elapsed:0,pointer:false,dragging:null};
    let target=v(0,.6+height,.3);
    if(config.prop==='bigLeaf')target=v(0,1.52,0);
    if(config.prop==='mossPillow')target=v(.50,.17,.45);
    if(config.prop==='acorn')target=v(0,.62,.55);
    if(config.prop==='leaf')target=v(0,.18,.2);
    if(config.prop==='blanket')target=v(-.05,.48,.08);
    step.target=target;
    for(let i=0;i<count;i++){
      let pos;
      if(config.type==='FollowPath')pos=v(-.85,.25,.12);
      else if(config.type==='DragToTarget')pos=v((count>1?-1.6+i*1.5:1.75),.19,1.5+(count>1?(i%2)*.12:0));
      else if(config.prop==='firefly')pos=v(-1.2+i*1.15,1.9+height+(i%2)*.35,.35);
      else pos=a.config.id==='suri'?v(-.29,.82,.42):a.focus.clone().add(a.root.position);
      const go=config.prop?this.createProp(config.prop):null;if(go)go.position.copy(pos);
      const item={go,pos,start:pos.clone(),hold:0,done:false,placing:false};step.items.push(item);
      const ring=this.halo(pos,config.type==='FollowPath'?.23:.27);step.halos.push(ring);
    }
    if(config.type==='DragToTarget'){const ring=this.halo(target,.45);ring.material.opacity=.35;step.targetHalo=ring;step.halos.push(ring);}
    this.step=step;return step;
  }
  halo(pos,radius){const m=mesh(this.props,'touch halo',new T.TorusGeometry(radius,.014,6,48),new T.MeshBasicMaterial({color:'#ffe0a0',transparent:true,opacity:.65,depthTest:false}),pos.toArray());m.rotation.x=-.35;m.renderOrder=6;return m;}
  project(pos){const p=pos.clone().project(this.camera);const r=this.canvas.getBoundingClientRect();return {x:(p.x+1)*r.width/2+r.left,y:(1-p.y)*r.height/2+r.top,visible:p.z<1&&p.z>-1};}
  near(pos,x,y,radius=58){const p=this.project(pos);return p.visible&&Math.hypot(p.x-x,p.y-y)<Math.max(radius,this.canvas.clientHeight*.13);}
  pointer(type,x,y){
    const s=this.step;if(!s||s.complete)return;const old=s.lastPointer;s.lastPointer={x,y};s.pointer=type!=='up'&&type!=='cancel';
    if(type==='cancel'){if(s.dragging){s.dragging.returning=true;s.dragging=null;}return;}
    if(type==='down'&&s.config.type==='DragToTarget'){s.dragging=s.items.filter(i=>!i.done&&!i.placing).sort((a,b)=>{const p=this.project(a.pos),q=this.project(b.pos);return Math.hypot(p.x-x,p.y-y)-Math.hypot(q.x-x,q.y-y);}).find(i=>this.near(i.pos,x,y));if(s.dragging){s.dragging.returning=false;s.activity=(s.activity||0)+1;}}
    if(s.config.type==='Stroke'&&s.pointer&&old&&this.near(s.items[0].pos,x,y,95)){s.stroke+=Math.min(45,Math.hypot(old.x-x,old.y-y))/this.canvas.clientHeight;this.animal.pet=1;}
    if(s.dragging&&s.pointer){
      const it=s.dragging;
      if(old&&Math.hypot(old.x-x,old.y-y)>1)s.activity=(s.activity||0)+1;
      // A camera-facing plane passes through the goal: a child's drop has no invisible depth mismatch.
      const rect=this.canvas.getBoundingClientRect();this.ray.setFromCamera(new T.Vector2((x-rect.left)/rect.width*2-1,-(y-rect.top)/rect.height*2+1),this.camera);
      const plane=new T.Plane().setFromNormalAndCoplanarPoint(this.camera.getWorldDirection(v()),s.target);const hit=v();if(this.ray.ray.intersectPlane(plane,hit)){it.pos.copy(hit);it.pos.y=Math.max(.15,it.pos.y);}
    }
    if(type==='up'&&s.dragging){const it=s.dragging;s.dragging=null;if(this.near(s.target,x,y,90)){it.placing=true;}else it.returning=true;}
  }
  updateStep(dt){
    const s=this.step;if(!s||s.complete)return;s.elapsed+=dt;
    const p=s.lastPointer,held=s.pointer&&p;
    if(s.config.type==='FollowPath'){
      const item=s.items[0];const moving=s.assisting||(held&&this.near(item.pos,p.x,p.y));
      if(moving)s.travel=Math.min(1,s.travel+dt/10);
      this.animal.root.position.x=-1.4+1.4*smooth(s.travel);this.animal.crawling=moving?1:0;
      item.pos.set(-.85+1.25*smooth(s.travel),.3,.12);s.progress=s.travel;
    }else if(s.config.type==='Stroke'){
      if(s.assisting)s.stroke+=dt*.6;s.progress=clamp(s.stroke/(s.config.strokeLength||1.8));
    }else if(s.config.type==='Touch'){
      const target=s.items.find(i=>!i.done&&(s.assisting||(held&&this.near(i.pos,p.x,p.y))));
      if(target){target.hold+=dt;s.activity=(s.activity||0)+1;this.animal.pet=.7;if(target.hold>=(s.config.holdSeconds||1.2)){target.done=true;if(target.go)target.go.scale.setScalar(.3);}}
      s.progress=s.items.filter(i=>i.done).length/s.items.length;
    }else{
      if(s.assisting){const it=s.items.find(i=>!i.done);if(it)it.placing=true;}
      for(const [idx,it] of s.items.entries()){
        if(it.returning){it.pos.lerp(it.start,1-Math.exp(-dt*3));if(it.pos.distanceTo(it.start)<.02)it.returning=false;}
        if(it.placing){const goal=s.target.clone().add(v(s.items.length>1?(idx-1)*.27:0,idx*.025,0));it.pos.lerp(goal,1-Math.exp(-dt*2.4));if(it.pos.distanceTo(goal)<.025){it.done=true;it.placing=false;}}
      }
      s.progress=s.items.filter(i=>i.done).length/s.items.length;
    }
    for(const [i,it] of s.items.entries()){if(it.go)it.go.position.copy(it.pos);const halo=s.halos[i];if(halo){halo.position.copy(it.pos);halo.visible=!it.done;halo.scale.setScalar(1+Math.sin(this.time*1.8)*.045);}}
    if(s.progress>=.999){s.complete=true;for(const h of s.halos)h.visible=false;this.animal.crawling=0;}
  }
  cue(name){if(this.animal)this.animal.cue(name);}
  sleep(){if(this.animal)this.animal.cue('sleep');}
  restoreAsleep(config){
    this.showAnimal(config);this.animal.root.position.x=0;this.sleep();
    for(const [k,value] of Object.entries(this.animal.targets))this.animal[k]=value;
    this.animal.update(0,this.time);
    for(const [i,c] of config.steps.entries())if(c.type==='DragToTarget'){
      const s=this.beginStep(c,i);s.complete=true;
      for(const [j,it] of s.items.entries()){it.pos.copy(s.target).add(v(s.items.length>1?(j-1)*.27:0,j*.025,0));it.done=true;if(it.go)it.go.position.copy(it.pos);}
      for(const h of s.halos)h.visible=false;
    }
    this.step=null;
  }
  render(dt,calm=0,draw=true){
    this.time+=dt;this.calm+=(calm-this.calm)*(1-Math.exp(-dt*.4));const a=atmosphere(this.calm);
    this.key.intensity=3.1*a.light;this.sky.uniforms.night.value=this.calm;this.stars.material.opacity=.35+this.calm*.5;
    this.camera.position.lerp(this.cameraTarget,1-Math.exp(-dt*1.2));this.look.lerp(this.lookTarget,1-Math.exp(-dt*1.2));this.camera.lookAt(this.look);
    for(const f of this.fireflies){f.m.position.x=f.p.x+Math.sin(this.time*.25*a.motion+f.phase)*.3;f.m.position.y=f.p.y+Math.cos(this.time*.32*a.motion+f.phase)*.2;f.m.visible=(f.phase/7)<Math.max(.15,a.particles);}
    if(this.isClearing){for(const e of this.clearingEntries)e.root.scale.setScalar(e.scale*(1+Math.sin(this.time*.8+e.index)*.002));}else{this.animal?.update(dt,this.time);this.updateStep(dt);}
    if(draw&&!this.contextLost){this.renderer.render(this.scene,this.camera);this.renderedFrames++;}
  }
  stats(){return {calls:this.renderer.info.render.calls,triangles:this.renderer.info.render.triangles,geometries:this.renderer.info.memory.geometries,frames:this.renderedFrames,renderer:this.renderer.getContext().getParameter(this.renderer.getContext().RENDERER)};}
}

export function buildAnimal(id,parent){
  const root=group(parent,id);const body=group(root,'Body');let head,eyes=[],ears=[],tail=null,feelers=[],foot=[],shell=null;const parts={};
  const faceAt=(color,pos,scale,opts)=>{const f=face(body,color,pos,scale,opts);head=f.g;eyes=f.eyes;return head;};
  if(id==='suri'){
    for(let i=0;i<7;i++)foot.push(blob(body,'foot'+i,'#dec2a3',[-.9+i*.27,.13,.1],[.31,.13,.32]));
    shell=group(root,'Shell',[-.29,.70,-.08]);blob(shell,'shell','#d29c93',[0,0,0],[.58,.58,.45]);
    const points=[];for(let i=0;i<115;i++){const t=i/114*Math.PI*4.9,r=.025+Math.pow(i/114,1.2)*.47;points.push([Math.cos(t)*r,Math.sin(t)*r,.45-Math.pow(r,.8)*.09]);}tube(shell,'shell spiral',points,.028,'#f3c7ad',114);
    for(let i=0;i<16;i++){const a=i/16*Math.PI*2;const patch=blob(shell,'shell rim','#bc8883',[Math.cos(a)*.54,Math.sin(a)*.54,.10],[.04,.055,.08],false);patch.rotation.z=a;}
    const neck=blob(body,'Neck','#dfc3a4',[.58,.31,.08],[.26,.33,.28]);parts.neck=neck;
    faceAt('#e9cdb1',[.75,.50,.16],[.34,.30,.30],{spacing:.155,y:.08,z:.255,r:.12});
    tube(head,'smile',[[-.09,-.11,.28],[0,-.15,.30],[.09,-.11,.28]],.012,'#946b63',14);
    for(const s of [-1,1]){const g=group(head,'Feeler'+s,[s*.19,.24,-.04]);branch(g,[0,0,0],[s*.08,.32,-.015],.034,'#dfbd9d');blob(g,'feeler tip','#dca99e',[s*.08,.33,-.015],[.065,.07,.065]);feelers.push(g);}
  }else if(id==='fino'||id==='nuri'){
    const color=id==='fino'?'#c98853':'#be9478';blob(body,'body',color,[0,.5,0],[.68,.48,.51]);blob(body,'bib','#e3ccaa',[0,.55,.39],[.40,.40,.18]);
    for(const s of [-1,1])blob(body,'paw',color,[s*.39,.20,.36],[.21,.14,.31]);
    faceAt(color,[0,1.06,.16],[.48,.42,.39],{spacing:.205,y:.08,z:.33,r:.12});
    for(const s of [-1,1]){const ear=group(head,'Ear'+s,[s*.34,.29,-.05]);if(id==='fino')pointedEar(ear,color,'#e3b39a',.46,.18);else{blob(ear,'ear',color,[0,.13,0],[.17,.29,.16]);blob(ear,'inner','#dbb3a4',[0,.13,.11],[.10,.19,.06]);}ear.rotation.z=-s*.24;ears.push(ear);blob(head,'muzzle','#edd5b5',[s*.13,-.15,.36],[.20,.13,id==='fino'?.22:.16]);}
    if(id==='fino')blob(head,'long cream snout','#edd5b5',[0,-.15,.48],[.17,.11,.20]);
    blob(head,'nose','#554c4a',[0,-.10,id==='fino'?.65:.49],[.08,.06,.06]);tail=group(body,'Tail',[.50,.32,-.30]);
    const tp=id==='fino'?[[0,0,0],[.6,.08,.15],[.9,.18,.55],[.68,.25,1.0]]:[[0,0,0],[.65,.5,-.2],[.6,1.25,-.3],[.05,1.55,-.3]];
    tube(tail,'fluffy tail',tp,id==='fino'?.24:.30,color,26);blob(tail,'tail tip',id==='fino'?'#eddbb7':color,tp.at(-1),[.25,.25,.26]);
    if(id==='nuri'){const nut=group(body,'hug acorn',[0,.65,.65]);blob(nut,'acorn','#bc8a59',[0,0,0],[.18,.23,.17]);blob(nut,'cap','#715b46',[0,.17,0],[.2,.08,.19]);nut.visible=false;parts.nut=nut;for(const s of [-1,1]){const paw=blob(body,'arm',color,[s*.24,.56,.55],[.14,.24,.14]);paw.rotation.z=s*.5;}}
  }else if(id==='mila'){
    blob(body,'body','#d9cfc0',[0,.49,0],[.6,.51,.47]);for(const s of [-1,1])blob(body,'paw','#e3d9c7',[s*.34,.18,.4],[.23,.14,.33]);blob(body,'tail','#e9dfcd',[-.48,.39,-.32],[.23,.24,.23]);
    faceAt('#e2d7c4',[0,1.02,.15],[.44,.41,.36],{spacing:.18,y:.07,z:.30,r:.12});
    for(const s of [-1,1]){const e=group(head,'Ear'+s,[s*.22,.30,-.04]);blob(e,'ear','#ded3c0',[0,.47,0],[.16,.57,.15]);blob(e,'inner','#cba6a1',[0,.48,.11],[.09,.42,.055]);e.rotation.z=-s*.13;ears.push(e);}
    blob(head,'nose','#c38f91',[0,-.13,.39],[.065,.045,.045]);tube(head,'mouth',[[0,-.17,.37],[-.04,-.20,.36]],.008,'#89736e',6);
  }else if(id==='piko'){
    blob(body,'body','#927a64',[0,.45,-.08],[.73,.54,.6]);const r=rng(89);
    for(let i=0;i<66;i++){const theta=r()*Math.PI*2,phi=r()*Math.PI*.61;const x=Math.cos(theta)*Math.sin(phi)*.65,z=Math.sin(theta)*Math.sin(phi)*.53-.13,y=.42+Math.cos(phi)*.48;if(z>.17&&y<.64)continue;const q=blob(body,'soft quill',['#bdab87','#ab9675','#caba98'][i%3],[x,y,z],[.085,.18,.085],false);q.quaternion.setFromUnitVectors(v(0,1,0),v(x,y-.35,z+.13).normalize());}
    faceAt('#ddc7a5',[0,.50,.40],[.42,.33,.34],{spacing:.175,y:.035,z:.29,r:.10});blob(head,'nose','#51494a',[0,-.09,.46],[.10,.075,.075]);for(const s of [-1,1])blob(head,'ear','#c3a485',[s*.34,.19,-.04],[.12,.14,.10]);
    for(const s of [-1,1])blob(body,'paw','#c8af8b',[s*.32,.15,.36],[.15,.10,.20]);
  }else if(id==='luna'){
    blob(body,'body','#bca88f',[0,.61,0],[.58,.67,.44]);blob(body,'bib','#e0d1ae',[0,.54,.32],[.43,.47,.16]);
    head=group(body,'Head',[0,1.16,.02]);blob(head,'head','#b5a089',[0,0,0],[.59,.47,.42]);
    for(const s of [-1,1]){blob(head,'face disk','#e9dabb',[s*.255,0,.34],[.29,.33,.12]);const wing=blob(body,'wing','#9a8c7e',[s*.48,.62,.0],[.2,.47,.33]);wing.rotation.z=s*.14;ears.push(wing);for(let i=0;i<3;i++)blob(body,'feather','#d5c2a1',[s*.20,.39+i*.19,.44],[.08,.045,.025],false);}
    eyes=eyePair(head,{spacing:.255,y:.025,z:.44,r:.175});const beak=blob(head,'beak','#c59860',[0,-.15,.49],[.085,.13,.09]);beak.rotation.x=-.2;
    for(const s of [-1,1]){blob(body,'talon','#b99365',[s*.25,.06,.21],[.17,.09,.2]);const e=blob(head,'ear tuft','#aa947b',[s*.4,.37,-.10],[.13,.24,.12]);e.rotation.z=-s*.3;}
  }else if(id==='leni'){
    blob(body,'body','#c6a17d',[0,.55,-.05],[.79,.48,.47]);blob(body,'bib','#e2c6a1',[.36,.62,.28],[.33,.35,.21]);
    for(const s of [-1,1]){blob(body,'folded foreleg','#c8a17e',[s*.33,.22,.46],[.13,.15,.39]);blob(body,'small hoof','#7f6b59',[s*.33,.17,.76],[.12,.10,.13]);blob(body,'back leg','#c49c77',[s*.52,.23,-.24],[.26,.21,.26]);}
    parts.neck=blob(body,'slender neck','#cba681',[.33,.90,.20],[.23,.48,.24]);parts.neck.userData.animated=true;
    faceAt('#d1ad87',[.35,1.34,.27],[.32,.39,.34],{spacing:.155,y:.07,z:.29,r:.12});
    for(const s of [-1,1]){const e=group(head,'Ear'+s,[s*.25,.23,-.04]);pointedEar(e,'#c3a07e','#dfbaa8',.58,.19);e.rotation.z=-s*.55;ears.push(e);}
    blob(head,'forehead blaze','#ead1ad',[0,.19,.30],[.07,.20,.045]);blob(head,'muzzle','#e2c7a4',[0,-.17,.42],[.20,.17,.28]);blob(head,'nose','#5c514b',[0,-.11,.66],[.095,.06,.06]);
    for(const s of [-1,1])for(let i=0;i<5;i++)blob(body,'fawn spot','#f2dfb4',[-.5+i*.19,.77+(i%2)*.04,s*.33],[.065,.04,.032],false);
  }
  const rest={head:head.position.clone(),body:body.position.clone(),tail:tail?.rotation.clone()};
  if(id!=='suri')batchStatic(body,false);
  if(shell)batchStatic(shell);
  const state={root,body,head,eyes,focus:v(id==='suri'?.75:0,id==='suri'?.7:.9,.35),crawling:0,pet:0,sleepAmount:0,feel:0,retract:0,closed:0,earDrop:0,curl:0,tailOver:0,yawn:0,wobble:0,asleep:false,targets:{},parts};
  const to=(key,value)=>state.targets[key]=value;
  state.cue=name=>{
    if(['sleep','heavyLids','slowBlink','blink'].includes(name))to('closed',1);
    if(['earsDown','earsRelax','headInFeathers'].includes(name))to('earDrop',1);
    if(['curlUp','curlBall','lieDown','burrowIn'].includes(name))to('curl',1);
    if(['tailOverNose','tailBlanket'].includes(name))to('tailOver',1);
    if(['yawn','stretchYawn'].includes(name))state.yawn=1;
    if(name==='retractFeelers')to('feel',1);
    if(name==='enterShell')to('retract',.55);
    if(name==='enterShellDeep')to('retract',1);
    if(name==='shellWobble')state.wobble=1;
    if(name==='holdAcorn'&&parts.nut)parts.nut.visible=true;
    if(name==='sleep'){state.asleep=true;to('closed',1);to('sleepAmount',1);to('earDrop',1);to('curl',1);if(tail)to('tailOver',1);if(id==='suri'){to('feel',1);to('retract',1);}}
  };
  state.update=(dt,time)=>{
    for(const [k,target] of Object.entries(state.targets))state[k]+=(target-state[k])*(1-Math.exp(-dt*(k==='retract'?.7:1.2)));
    state.pet=Math.max(0,state.pet-dt*.4);state.yawn=Math.max(0,state.yawn-dt*.22);state.wobble=Math.max(0,state.wobble-dt*.32);
    const breath=Math.sin(time*(state.asleep?.85:1.15))*(state.asleep?.013:.016);
    body.scale.set(1+breath*.5,1+breath,1+breath*.7);body.position.y=-state.curl*.14;
    head.position.copy(rest.head);head.position.y-=state.curl*(id==='leni'?.48:id==='luna'?.13:.15);head.rotation.z=Math.sin(time*.55)*.025*(1-state.sleepAmount)+state.pet*.035;head.rotation.x=state.curl*.16+state.yawn*.11;
    let blink=0;const bt=time%6.4;if(bt>6.05)blink=Math.sin((bt-6.05)/.35*Math.PI);
    const shut=Math.max(state.closed,blink);for(const e of eyes){e.scale.y=Math.max(.035,1-shut*.96);e.getObjectByName('ClosedLid').visible=shut>.9;}
    for(const [i,e] of ears.entries()){const side=i===0?-1:1;e.rotation.z=-side*(.14+state.earDrop*.68);}
    if(tail){tail.rotation.y=-state.tailOver*1.75;tail.rotation.z=state.tailOver*.22+Math.sin(time*.7)*.025*(1-state.sleepAmount);}
    if(id==='suri'){
      const k=smooth(state.retract);head.position.lerp(v(-.23,.55,.04),k);head.scale.setScalar(Math.max(.001,1-k));parts.neck.scale.set(.26,.33,.28).multiplyScalar(Math.max(.001,1-k));
      for(const [i,f] of foot.entries()){f.scale.y=.13*(1+Math.sin(time*5-i*.9)*.10*state.crawling);f.scale.x=.31*(1+Math.sin(time*5-i*.9)*.06*state.crawling);f.visible=k<.96;f.position.x=(-.9+i*.27)*(1-k)-.23*k;f.scale.multiplyScalar(1-k*.97);}
      for(const [i,f] of feelers.entries()){f.scale.y=1-state.feel*.98;f.rotation.z=Math.sin(time*1.1+i*2)*.12*(1-state.feel);}
      shell.rotation.z=Math.sin(time*6)*state.wobble*.06;shell.scale.setScalar(1+breath*.4);
    }
    if(id==='piko'){head.scale.setScalar(1-state.curl*.7);head.position.z-=state.curl*.26;}
    if(id==='leni'){parts.neck.scale.y=.48*(1-state.curl*.55);parts.neck.position.y=.90-state.curl*.22;}
    if(id==='luna')head.rotation.x+=state.earDrop*.28;
  };
  return state;
}
