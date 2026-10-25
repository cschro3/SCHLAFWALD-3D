import { mkdir,copyFile } from 'node:fs/promises';
await mkdir('public/content',{recursive:true});
await copyFile('../UnityProject/Assets/Schlafwald/Resources/Story/story_de.json','public/content/story_de.json');
