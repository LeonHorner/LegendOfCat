import { DOWNLOADS } from './release-assets.mjs';

export function downloadNotes(tag) {
  if (!/^v[0-9]+\.[0-9]+\.[0-9]+(?:[-+][0-9A-Za-z.-]+)?$/.test(tag))
    throw new Error('Invalid stable release tag: ' + tag);
  const base = 'https://github.com/LeonHorner/LegendOfCat/releases/download/' + tag + '/';
  return [
    '## Download LegendOfCat',
    '',
    '- **Windows x64 (installer):** [Download Installer.exe](' + base + DOWNLOADS.windows + ')',
    '- **Linux x64:** [Download AppImage](' + base + DOWNLOADS.linux + ')',
    '- **Windows x64 (portable, optional):** [Download Portable.zip](' + base + DOWNLOADS.portable + ')',
    '',
    '> Early development build. The complete adventure is not available yet. See [current installation and update test status](https://github.com/LeonHorner/LegendOfCat/blob/main/RELEASE-TEST.md).'
  ].join('\n');
}

// semantic-release concatenates output from generateNotes plugins in order;
// this plugin runs before the conventional commit changelog.
export function generateNotes(_config, context) {
  return downloadNotes(context.nextRelease.gitTag);
}
