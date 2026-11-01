import { assertSnapshot } from './release-approval.mjs';
import semanticRelease from 'semantic-release';

if (!process.env.EXPECTED) throw new Error('Missing validated release version.');
assertSnapshot(process.env, process.cwd());
const result = await semanticRelease({ ci: true });
if (!result) throw new Error('No release was published.');
