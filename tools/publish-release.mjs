import { runSnapshotRelease } from './release-snapshot.mjs';

if (!process.env.EXPECTED) throw new Error('Missing validated release version.');
const result = await runSnapshotRelease();
if (!result) throw new Error('No release was published.');
