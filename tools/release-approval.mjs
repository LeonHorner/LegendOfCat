// The version stays automatically calculated; these switches only authorize its type.
// Unapproved minor/major changes fail instead of being silently downgraded.
const enabled = value => value === true || value === 'true';

export function assertReleaseApproval(nextRelease, allowMinor = false, allowMajor = false) {
  if (!nextRelease || typeof nextRelease.version !== 'string')
    throw new Error('No release version was planned.');
  switch (nextRelease.type) {
    case 'patch':
      return;
    case 'minor':
      if (!enabled(allowMinor))
        throw new Error('Computed minor release ' + nextRelease.version +
          ' is blocked: enable allow_minor to permit this feature release.');
      return;
    case 'major':
      if (!enabled(allowMajor))
        throw new Error('Computed major release ' + nextRelease.version +
          ' is blocked: enable allow_major only for intentional breaking changes.');
      return;
    default:
      throw new Error('Unsupported release type: ' + String(nextRelease.type));
  }
}
