import { generateNotes as generateConventionalNotes } from '@semantic-release/release-notes-generator';

// The snapshot remote is local, but published changelog links must target GitHub.
export function generateNotes(config, context) {
  return generateConventionalNotes(config, {
    ...context,
    options: { ...context.options, repositoryUrl: 'https://github.com/LeonHorner/LegendOfCat.git' }
  });
}
