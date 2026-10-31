// Fail closed before Unity builds, packaging, or any public release.
export function assertReleaseApproval(nextRelease, expectedVersion, allowMajor = false) {
  if (!nextRelease || typeof nextRelease.version !== 'string')
    throw new Error('No release version was planned.');
  if (typeof expectedVersion !== 'string' ||
      !/^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)$/.test(expectedVersion))
    throw new Error('A valid expected_version (X.Y.Z) must be explicitly provided.');
  if (expectedVersion !== nextRelease.version)
    throw new Error('Release version mismatch: approved ' + expectedVersion +
      ', computed ' + nextRelease.version + '. Review commits; do not bypass.');
  if (nextRelease.type === 'major' && allowMajor !== true && allowMajor !== 'true')
    throw new Error('Major release requires separate allow_major approval.');
}
