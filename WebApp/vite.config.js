import { defineConfig } from 'vite';
export default defineConfig({base:'./',build:{target:'es2020'},server:{fs:{allow:['..']}},preview:{port:4173,host:'0.0.0.0'}});
