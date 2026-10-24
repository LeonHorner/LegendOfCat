// Real FUSE/mount-lifetime regression using two independently packaged Velopack
// AppImages. No GitHub releases or external network update feed are involved.
// This does not drive Unity's in-game menu, so graphical UX still needs review.
import assert from 'node:assert/strict';
import { spawn } from 'node:child_process';
import { createHash } from 'node:crypto';
import {
  chmodSync, copyFileSync, existsSync, mkdirSync, mkdtempSync,
  readFileSync, rmSync
} from 'node:fs';
import { tmpdir } from 'node:os';
import { basename, dirname, join, resolve } from 'node:path';

const [baseAppImage, futurePackage] = process.argv.slice(2).map(x => x && resolve(x));
if (!baseAppImage || !futurePackage) throw new Error('Expected: <old AppImage> <next full nupkg>');
if (!existsSync('/dev/fuse')) throw new Error('FUSE unavailable: actual mounted AppImage test cannot run');
if (!existsSync(baseAppImage) || !existsSync(futurePackage))
  throw new Error('Missing input package to FUSE update integration');

const runnerSource = resolve('Launcher/linux-update-runner.sh');
const sha = file => createHash('sha256').update(readFileSync(file)).digest('hex');
const sleep = ms => new Promise(done => setTimeout(done, ms));

async function stop(process) {
  if (!process || process.exitCode !== null || process.signalCode !== null) return;
  const exit = new Promise(done => process.once('exit', done));
  process.kill('SIGTERM');
  await Promise.race([exit, sleep(3000)]);
  if (process.exitCode === null && process.signalCode === null) process.kill('SIGKILL');
}

function identity(pid) {
  const s = readFileSync(`/proc/${pid}/stat`, 'utf8');
  return s.slice(s.lastIndexOf(') ') + 2).trim().split(/\s+/)[19];
}
async function until(check, duration = 25000) {
  const end = Date.now() + duration;
  while (!check()) {
    if (Date.now() > end) throw new Error('Timed out waiting for real AppImage update');
    await sleep(40);
  }
}

async function mount(original) {
  const child = spawn(original, ['--appimage-mount'], { stdio: ['ignore', 'pipe', 'pipe'] });
  let line = '';
  let diagnostics = '';
  child.stderr.on('data', data => { diagnostics += String(data).slice(0, 2048); });
  child.stdout.on('data', data => { line += String(data); });
  try {
    await until(() => line.includes('\n') || child.exitCode !== null, 15000);
    if (child.exitCode !== null)
      throw new Error('AppImage exited before FUSE mount: ' + diagnostics);
    const mountPath = line.split('\n')[0].trim();
    const manifest = join(mountPath, 'usr', 'bin', 'sq.version');
    await until(() => existsSync(manifest), 10000);
    return { child, mountPath, manifest };
  } catch (error) {
    await stop(child);
    throw error;
  }
}

const root = mkdtempSync(join(tmpdir(), 'legendofcat-fuse-integration-'));
const original = join(root, 'Renamed Original Legend Of Cat.AppImage');
const stage = join(root, 'private staging');
const ready = join(stage, 'ready');
const log = join(root, 'native-updater.log');
let mounted;
let remounted;
let game;
let supervisor;

try {
  copyFileSync(baseAppImage, original);
  chmodSync(original, 0o755);
  const oldHash = sha(original);
  mounted = await mount(original);
  const oldManifest = readFileSync(mounted.manifest, 'utf8');
  console.log('Real FUSE mount confirmed:', basename(mounted.mountPath));

  const bin = join(stage, 'usr', 'bin');
  mkdirSync(bin, { mode: 0o700, recursive: true });
  chmodSync(stage, 0o700);
  copyFileSync(join(mounted.mountPath, 'usr', 'bin', 'UpdateNix'), join(bin, 'UpdateNix'));
  copyFileSync(mounted.manifest, join(bin, 'sq.version'));
  chmodSync(join(bin, 'UpdateNix'), 0o700);
  const runner = join(stage, 'linux-update-runner.sh');
  copyFileSync(runnerSource, runner);
  const actualPackageDir = dirname(futurePackage);
  game = spawn('sleep', ['60'], { stdio: 'ignore' });
  assert.ok(game.pid && mounted.child.pid);
  supervisor = spawn('/bin/sh', [
    runner, ready, stage, log, String(mounted.child.pid), identity(mounted.child.pid),
    String(game.pid), identity(game.pid), original, futurePackage, actualPackageDir, '60'
  ], { cwd: stage, stdio: 'ignore' });
  assert.ok(supervisor.pid);
  await until(() => existsSync(ready));
  assert.equal(sha(original), oldHash, 'Must not replace AppImage before shutdown');
  await stop(game);
  assert.equal(sha(original), oldHash, 'Must not replace AppImage while mount owner remains');
  await stop(mounted.child);
  await until(() => supervisor.exitCode !== null || supervisor.signalCode !== null, 90000);
  assert.equal(supervisor.exitCode, 0,
    'Native update failed; read persistent diagnostic log: ' +
    (existsSync(log) ? readFileSync(log, 'utf8').slice(-4000) : '(missing log)'));
  assert.notEqual(sha(original), oldHash, 'The exact same renamed AppImage path must be replaced');
  remounted = await mount(original);
  const newManifest = readFileSync(remounted.manifest, 'utf8');
  assert.notEqual(newManifest, oldManifest, 'Mounted AppImage metadata must be the N+1 version');
  console.log('PASS: mounted FUSE AppImage N -> N+1 updated at original path');
} finally {
  await stop(game);
  await stop(mounted?.child);
  await stop(remounted?.child);
  await stop(supervisor);
  rmSync(root, { recursive: true, force: true });
}
