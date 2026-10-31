// Shared semantic-release configuration for planning and publishing.
// The standard noteKeywords parser is case-insensitive; notesPattern is not.
// A breaking release therefore requires an explicit UPPERCASE footer.
const strictBreaking = {
  noteKeywords: ['BREAKING CHANGE', 'BREAKING-CHANGE'],
  notesPattern: keywords => new RegExp('^(' + keywords + '):[ \\t]+(.+)$'),
  // Do not infer a breaking release from feat!:; the required footer must be uppercase.
  breakingHeaderPattern: /(?!)/
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
