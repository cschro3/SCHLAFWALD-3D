# Prüft rechnerisch, dass alle Ritual-Objekte von Suri in den Kameraeinstellungen (interact/sleep) in 16:9, 19.5:9, 4:3 und 3:2 im Bild liegen.
# Nutzung: python3 tools/check_camera_framing.py UnityProject/Assets/Schlafwald/Resources/Story/story_de.json
import numpy as np, math, json, sys
story=json.load(open(sys.argv[1]))
suri=[a for a in story["animals"] if a["id"]=="suri"][0]
def v(d): return np.array([d["x"],d["y"],d["z"]],float)
def vfov(v169,aspect):
    r=16/9
    if aspect>=r: return v169
    h=2*math.atan(math.tan(math.radians(v169)/2)*r); return math.degrees(2*math.atan(math.tan(h/2)/aspect))
def project(cam,look,fov,aspect,p):
    f=look-cam; f/=np.linalg.norm(f); r=np.cross([0,1,0],f); r/=np.linalg.norm(r); u=np.cross(f,r)
    d=p-cam; z=d@f
    if z<=0: return None
    t=math.tan(math.radians(vfov(fov,aspect))/2)
    return (d@r)/(z*t*aspect),(d@u)/(z*t)   # NDC -1..1
pts={}
st=suri["steps"]
for i,p in enumerate(st[0]["points"]): pts["path%d"%i]=v(p)+np.array([0,0.2 if i in(0,4) else 0.05,0])
pts["stoneA_top"]=np.array([1.7,0.34,73.35]); pts["stoneB_top"]=np.array([3.3,0.38,72.9])
pts["leaf_start"]=v(st[1]["points"][0])+np.array([0,0.1,0]); pts["leaf_target"]=v(st[1]["target"]); pts["shell"]=v(st[2]["target"])
pts["leaf_far_corner"]=v(st[1]["target"])+np.array([0.75,0,0.55])
pts["leaf_near_corner"]=v(st[1]["target"])+np.array([-0.75,0,-0.55])
bad=0
for sh in suri["shots"]:
    cam=v(sh["position"]); look=v(sh["lookAt"])
    for name,asp in (("16:9",16/9),("19.5:9",19.5/9),("4:3",4/3),("3:2",1.5)):
        out=[]
        names=pts.keys() if sh["id"]=="interact" else ["shell","stoneB_top","leaf_target","leaf_near_corner","leaf_far_corner"]
        for n in names:
            q=project(cam,look,sh["fov"],asp,pts[n])
            if q is None or abs(q[0])>0.92 or abs(q[1])>0.92: out.append("%s(%s)"%(n,None if q is None else "%.2f,%.2f"%q))
        print(sh["id"],name,"OK" if not out else "AUSSERHALB: "+", ".join(out)); bad+=len(out)
print("Probleme:",bad)
