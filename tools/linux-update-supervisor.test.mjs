import assert from 'node:assert/strict';
import { spawn } from 'node:child_process';
import {
  chmodSync, copyFileSync, existsSync, mkdirSync, mkdtempSync,
  readFileSync, rmSync, writeFileSync
} from 'node:fs';
import { tmpdir } from 'node:os';
import { join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { test } from 'node:test';

const runnerSource = fileURLToPath(new URL('../Launcher/linux-update-runner.sh', import.meta.url));
const pause = milliseconds => new Promise(done => setTimeout(done, milliseconds));

function identity(pid) {
  const stat = readFileSync(`/proc/${pid}/stat`, 'utf8');
  const fields = stat.slice(stat.lastIndexOf(') ') + 2).trim().split(/\s+/);
  return fields[19];
}

function sleeper() {
  const child = spawn('sleep', ['30'], { stdio: 'ignore' });
  if (!child.pid) throw new Error('Unable to start real child process');
  return child;
}

async function stop(child) {
  if (!child) return;
  if (child.exitCode === null && child.signalCode === null) {
    const exited = new Promise(done => child.once('exit', done));
    child.kill('SIGTERM');
    await exited;
  }
}

async function until(check, maximum = 4000) {
  const end = Date.now() + maximum;
  while (!check()) {
    if (Date.now() > end) throw new Error('Timed out waiting for process result');
    await pause(25);
  }
}

function setup({ missingPackage = false } = {}) {
  const root = mkdtempSync(join(tmpdir(), 'legendofcat-supervisor-tests-'));
  const stage = join(root, 'private stage with spaces');
  const bin = join(stage, 'usr', 'bin');
  mkdirSync(bin, { recursive: true, mode: 0o700 });
  const runner = join(stage, 'linux-update-runner.sh');
  copyFileSync(runnerSource, runner);
  const updater = join(bin, 'UpdateNix');
  const argsFile = join(root, 'actual updater arguments');
  writeFileSync(updater, '#!/bin/sh\nprintf "%s\\n" "$@" > "$APPLIED_ARGS"\n');
  chmodSync(updater, 0o700);
  writeFileSync(join(bin, 'sq.version'), '{"id":"test","version":"1.0.0"}');
  const original = join(root, 'Renamed Original Game.AppImage');
  const packageDir = join(root, 'downloads');
  mkdirSync(packageDir);
  const pkg = join(packageDir, 'update full.nupkg');
  writeFileSync(original, 'original AppImage placeholder');
  if (!missingPackage) writeFileSync(pkg, 'verified package placeholder');
  return {
    root, stage, runner, original, packageDir, pkg,
    ready: join(stage, 'ready'), log: join(root, 'updates.log'), argsFile
  };
}

function start(details, launcher, game, seconds = 5) {
  const child = spawn('/bin/sh', [
    details.runner, details.ready, details.stage, details.log,
    String(launcher.pid), identity(launcher.pid),
    String(game.pid), identity(game.pid),
    details.original, details.pkg, details.packageDir, String(seconds)
  ], {
    cwd: details.stage, stdio: 'ignore',
    env: { ...process.env, APPLIED_ARGS: details.argsFile }
  });
  if (!child.pid) throw new Error('Unable to start shell supervisor');
  return child;
}

test('real Linux child processes gate native update until BOTH have exited', async () => {
  const details = setup();
  const game = sleeper();
  const launcher = sleeper();
  let supervisor;
  try {
    supervisor = start(details, launcher, game);
    await until(() => existsSync(details.ready));
    assert.equal(existsSync(details.argsFile), false);
    await stop(game);
    await pause(250);
    assert.equal(existsSync(details.argsFile), false,
      'Launcher still alive must block native updater');
    await stop(launcher);
    await until(() => supervisor.exitCode !== null || supervisor.signalCode !== null, 5000);
    assert.equal(supervisor.exitCode, 0, readFileSync(details.log, 'utf8'));
    const argv = readFileSync(details.argsFile, 'utf8').trim().split('\n');
    assert.deepEqual(argv, [
      '--log', details.log, 'apply', '--package', details.pkg,
      '--rootDir', details.original, '--packageDir', details.packageDir
    ]);
    assert.equal(existsSync(details.stage), false, 'Supervisor removes private staging');
  } finally {
    await stop(game); await stop(launcher); await stop(supervisor);
    rmSync(details.root, { recursive: true, force: true });
  }
});

test('shutdown timeout fails CLOSED and does not invoke native updater', async () => {
  const details = setup();
  const game = sleeper();
  const launcher = sleeper();
  let supervisor;
  try {
    supervisor = start(details, launcher, game, 1);
    await until(() => existsSync(details.ready));
    await until(() => supervisor.exitCode !== null || supervisor.signalCode !== null, 4000);
    assert.notEqual(supervisor.exitCode, 0);
    assert.equal(existsSync(details.argsFile), false);
    assert.match(readFileSync(details.log, 'utf8'), /timed out; refusing/);
    assert.equal(existsSync(details.stage), false);
  } finally {
    await stop(game); await stop(launcher); await stop(supervisor);
    rmSync(details.root, { recursive: true, force: true });
  }
});

test('missing verified package refuses readiness while game keeps running', async () => {
  const details = setup({ missingPackage: true });
  const game = sleeper();
  const launcher = sleeper();
  let supervisor;
  try {
    supervisor = start(details, launcher, game, 1);
    await until(() => supervisor.exitCode !== null || supervisor.signalCode !== null);
    assert.notEqual(supervisor.exitCode, 0);
    assert.equal(existsSync(details.ready), false);
    assert.equal(existsSync(details.argsFile), false);
    assert.equal(game.exitCode, null);
    assert.equal(launcher.exitCode, null);
  } finally {
    await stop(game); await stop(launcher); await stop(supervisor);
    rmSync(details.root, { recursive: true, force: true });
  }
});
