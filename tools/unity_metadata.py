"""Create stable metadata only where missing, or audit with --check. Never changes a GUID.

The baseline never ran Unity and had no .meta files. UUID5 path identities make
the initial metadata reproducible; subsequent renames must move the .meta too.
"""
from pathlib import Path
import argparse,uuid,re
ROOT=Path(__file__).resolve().parents[1]/'UnityProject/Assets'
NAMESPACE=uuid.UUID('9ab29596-993b-42c4-ade7-785443eac941')
def content(p):
    guid=uuid.uuid5(NAMESPACE,'Schlafwald/'+p.relative_to(ROOT).as_posix()).hex
    header=f'fileFormatVersion: 2\nguid: {guid}\n'
    if p.is_dir():return header+'folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
    importer={'.cs':'MonoImporter','.shader':'ShaderImporter','.asmdef':'AssemblyDefinitionImporter','.json':'TextScriptImporter','.mp3':'AudioImporter'}.get(p.suffix,'DefaultImporter')
    body='  externalObjects: {}\n'
    if importer=='MonoImporter':body+='  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n  icon: {instanceID: 0}\n'
    if importer=='ShaderImporter':body+='  defaultTextures: []\n  nonModifiableTextures: []\n'
    if importer=='AudioImporter':body+='  serializedVersion: 7\n  defaultSettings:\n    serializedVersion: 2\n    loadType: 0\n    sampleRateSetting: 0\n    sampleRateOverride: 44100\n    compressionFormat: 1\n    quality: 0.6\n    conversionMode: 0\n  platformSettingOverrides: {}\n  forceToMono: 1\n  normalize: 1\n  preloadAudioData: 1\n  loadInBackground: 1\n  ambisonic: 0\n  3D: 0\n'
    return header+importer+':\n'+body+'  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
def main():
    args=argparse.ArgumentParser();args.add_argument('--check',action='store_true');o=args.parse_args();errors=[];created=0;seen={}
    for p in sorted(ROOT.rglob('*')):
        if p.name.endswith('.meta') or p.name.startswith('.'):continue
        target=Path(str(p)+'.meta')
        if not target.exists():
            if o.check:errors.append('Missing metadata: '+str(p));continue
            target.write_text(content(p));created+=1
        text=target.read_text();m=re.search(r'^guid: ([a-f0-9]{32})$',text,re.M)
        if not m:errors.append('Invalid GUID: '+str(target));continue
        if m[1] in seen:errors.append('Duplicate GUID: '+str(target)+' / '+str(seen[m[1]]))
        seen[m[1]]=target
    for p in ROOT.rglob('*.meta'):
        if not Path(str(p)[:-5]).exists():errors.append('Orphan metadata: '+str(p))
    print(f'{len(seen)} metadata identities checked; {created} created; {len(errors)} errors')
    for e in errors:print(e)
    raise SystemExit(bool(errors))
if __name__=='__main__':main()
