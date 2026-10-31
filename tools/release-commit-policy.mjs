import { execFileSync } from 'node:child_process';

// A shorthand bang or lower/mixed-case breaking footer would silently bypass
// the strict parser and could otherwise be released as a minor or patch.
export function validateReleaseMessages(messages) {
  for (const message of messages) {
    const lines = message.split(/\r?\n/);
    const title = lines[0] ?? '';
    if (/^(?:feat|fix|perf|docs|style|refactor|test|build|ci|chore|revert)(?:\([^)]+\))?!: /.test(title))
      throw new Error('Breaking shorthand (!:) is not permitted: use an uppercase BREAKING CHANGE: footer. Commit: ' + title);
    for (const line of lines.slice(1)) {
      if (/^BREAKING[ -]CHANGE:[ \t]+/i.test(line) && !/^BREAKING[ -]CHANGE:[ \t]+/.test(line))
        throw new Error('Breaking change markers must be UPPERCASE: BREAKING CHANGE: or BREAKING-CHANGE:. Commit: ' + title);
    }
  }
}

export function inspectReleaseCommitHistory() {
  let latestTag = '';
  try {
    latestTag = execFileSync('git', ['describe', '--tags', '--match', 'v[0-9]*', '--abbrev=0'], {
      encoding: 'utf8', stdio: ['ignore', 'pipe', 'ignore']
    }).trim();
  } catch { /* No previous stable tag: inspect all commits. */ }
  const range = latestTag ? latestTag + '..HEAD' : 'HEAD';
  const raw = execFileSync('git', ['log', '--format=%B%x1e', range], { encoding: 'utf8' });
  const messages = raw.split('\x1e').map(s => s.trim()).filter(Boolean);
  validateReleaseMessages(messages);
  return messages;
}
