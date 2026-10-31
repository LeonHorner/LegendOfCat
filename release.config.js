// Shared semantic-release configuration for planning and publishing.
// The standard noteKeywords parser is case-insensitive; notesPattern is not.
// Footers must be UPPERCASE; bang headers retain their conventional MAJOR meaning.
const strictBreaking = {
  noteKeywords: ['BREAKING CHANGE', 'BREAKING-CHANGE'],
  notesPattern: keywords => new RegExp('^(' + keywords + '):[ \\t]+(.+)$'),
  // Preserve the conventionalcommits preset's default breakingHeaderPattern:
  // feat!:, fix!:, fix(api)!: and other bang headers are major releases.
  // Only explicit footer markers (not bang headers) must be uppercase.
};

export default {
  branches: ['main'],
  tagFormat: 'v' + '${version}',
  plugins: [
    ['@semantic-release/commit-analyzer', {
      preset: 'conventionalcommits',
      parserOpts: strictBreaking
    }],
    './tools/release-notes.mjs',
    ['@semantic-release/release-notes-generator', {
      preset: 'conventionalcommits',
      parserOpts: strictBreaking
    }],
    ['@semantic-release/github', {
      successComment: false,
      failComment: false,
      assets: [{ path: 'artifacts/published/*' }]
    }]
  ]
};
