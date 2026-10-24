import { readFileSync, writeFileSync, mkdirSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { resolve } from 'node:path';

export const source = new URL('./icon.ico', import.meta.url);
export const variant = new URL('../Assets/Game/Branding/icon.png', import.meta.url);

// The original ICO already contains PNG entries. Extract the largest losslessly;
// Unity generates its required platform sizes during the player build.
export function largestPng(data) {
  if (data.readUInt16LE(0) !== 0 || data.readUInt16LE(2) !== 1) throw new Error('Invalid ICO');
  const count = data.readUInt16LE(4);
  let best;
  let bestSize = 0;
  for (let i = 0; i < count; i++) {
    const at = 6 + i * 16;
    const width = data[at] || 256, height = data[at + 1] || 256;
    const size = data.readUInt32LE(at + 8), offset = data.readUInt32LE(at + 12);
    if (offset + size > data.length) throw new Error('Truncated ICO');
    const png = data.subarray(offset, offset + size);
    if (png.subarray(0, 8).equals(Buffer.from('89504e470d0a1a0a', 'hex')) && width * height > bestSize) {
      best = png; bestSize = width * height;
    }
  }
  if (!best) throw new Error('Original ICO has no PNG entry');
  return best;
}

if (process.argv[1] && resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  mkdirSync(new URL('../Assets/Game/Branding/', import.meta.url), { recursive: true });
  writeFileSync(variant, largestPng(readFileSync(source)));
}
