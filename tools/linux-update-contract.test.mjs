import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

// Contract-level regression checks; a real Linux AppImage update is still required.
const source = readFileSync(new URL('../Launcher/Program.cs', import.meta.url), 'utf8');

test('optional release history cannot prevent the Velopack update check', () => {
  const history = source.indexOf('try {\n                        var releases');
  const guarded = source.indexOf('catch (Exception historyError)');
  const actualCheck = source.indexOf('manager.CheckForUpdatesAsync()');
  assert.ok(history > 0 && guarded > history && actualCheck > guarded,
    'update check must remain outside the optional history try/catch');
  assert.match(source, /historyError is HttpRequestException or TaskCanceledException or JsonException/);
  assert.match(source, /Send\(new \{ state = "warning", message = "Release history is temporarily unavailable/);
});

test('Linux hands off to Velopack without waiting for launcher shutdown first', () => {
  const start = source.indexOf('if (OperatingSystem.IsLinux())');
  const end = source.indexOf('} else {', start);
  assert.ok(start > 0 && end > start);
  const linux = source.slice(start, end);
  assert.match(linux, /manager.ApplyUpdatesAndRestart\(update\)/);
  assert.doesNotMatch(linux, /WaitForExit\(launcherPid\)/);
  const windows = source.slice(end, source.indexOf('return 0;', end));
  assert.match(windows, /WaitForExit\(launcherPid\)/);
});
