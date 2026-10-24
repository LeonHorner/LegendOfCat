using System.Diagnostics;
using Velopack.Locators;

// The managed update helper must finish its work BEFORE the AppImage entry
// process exits. Only this staged native supervisor is allowed to outlive it.
internal static class LinuxUpdateHandoff
{
    private static readonly UnixFileMode PrivateDirectory =
        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;
    private static readonly UnixFileMode PrivateFile =
        UnixFileMode.UserRead | UnixFileMode.UserWrite;

    internal static async Task StartAsync(
        int gamePid, int launcherPid, string stateDirectory, string packageFilename)
    {
        if (!OperatingSystem.IsLinux())
            throw new PlatformNotSupportedException("Linux AppImage handoff is Linux-only.");

        var locator = VelopackLocator.Current;
        var nativeUpdater = Required(locator.UpdateExePath, "Native updater");
        var originalAppImage = Required(locator.RootAppDir, "Original AppImage");
        var packagesDir = Required(locator.PackagesDir, "Package directory");
        var runnerSource = Path.Combine(AppContext.BaseDirectory, "linux-update-runner.sh");
        var manifestSource = Path.Combine(Path.GetDirectoryName(nativeUpdater)!, "sq.version");
        var package = Path.Combine(packagesDir, Path.GetFileName(packageFilename));

        if (packageFilename != Path.GetFileName(packageFilename) ||
            string.IsNullOrWhiteSpace(packageFilename))
            throw new InvalidOperationException("Invalid package filename.");
        foreach (var required in new[] { nativeUpdater, manifestSource, runnerSource, originalAppImage, package }) {
            if (!File.Exists(required))
                throw new FileNotFoundException("Required Linux update handoff input missing.", required);
        }
        if (new FileInfo(package).Length == 0)
            throw new InvalidOperationException("The downloaded update package is empty.");

        // Capture kernel starttime as well as PID: PID reuse must never let a
        // new, unrelated process satisfy the shutdown barrier.
        var gameIdentity = ReadProcessIdentity(gamePid);
        var launcherIdentity = ReadProcessIdentity(launcherPid);

        // CreateTempSubdirectory is private and uniquely created atomically.
        // It must not be a predictable executable path in a shared /tmp folder.
        var stage = Directory.CreateTempSubdirectory("legendofcat-updater-").FullName;
        Process? process = null;
        try {
            Directory.SetUnixFileMode(stage, PrivateDirectory);
            var bin = Path.Combine(stage, "usr", "bin");
            Directory.CreateDirectory(bin);
            Directory.SetUnixFileMode(Path.Combine(stage, "usr"), PrivateDirectory);
            Directory.SetUnixFileMode(bin, PrivateDirectory);

            var stagedUpdater = Path.Combine(bin, "UpdateNix");
            File.Copy(nativeUpdater, stagedUpdater);
            File.SetUnixFileMode(stagedUpdater, PrivateDirectory);
            var stagedManifest = Path.Combine(bin, "sq.version");
            File.Copy(manifestSource, stagedManifest);
            File.SetUnixFileMode(stagedManifest, PrivateFile);
            var stagedRunner = Path.Combine(stage, "linux-update-runner.sh");
            File.Copy(runnerSource, stagedRunner);
            File.SetUnixFileMode(stagedRunner, PrivateFile);

            var ready = Path.Combine(stage, "ready");
            var log = Path.Combine(stateDirectory, "updates.log");
            var start = new ProcessStartInfo("/bin/sh") {
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = stage,
            };
            foreach (var argument in new[] {
                stagedRunner, ready, stage, log,
                launcherPid.ToString(), launcherIdentity,
                gamePid.ToString(), gameIdentity,
                originalAppImage, package, packagesDir
            }) start.ArgumentList.Add(argument);
            process = Process.Start(start) ??
                throw new InvalidOperationException("Unable to launch the Linux native update supervisor.");

            var watch = Stopwatch.StartNew();
            while (!File.Exists(ready)) {
                if (process.HasExited)
                    throw new InvalidOperationException("Linux update supervisor exited before acknowledging readiness.");
                if (watch.Elapsed > TimeSpan.FromSeconds(8))
                    throw new TimeoutException("Linux update supervisor did not acknowledge readiness.");
                await Task.Delay(40);
            }
            if (process.HasExited)
                throw new InvalidOperationException("Linux update supervisor exited during startup.");

            // From this point the external supervisor owns staging and cleanup.
            // It does not need the .NET helper or original mounted AppImage.
            await File.AppendAllTextAsync(log,
                $"{DateTimeOffset.UtcNow:O} Native updater staged and supervisor ready\n");
        } catch {
            if (process != null) {
                try { if (!process.HasExited) process.Kill(entireProcessTree: true); } catch { }
            }
            try { Directory.Delete(stage, recursive: true); } catch { }
            throw;
        } finally {
            process?.Dispose();
        }
    }

    internal static string ReadProcessIdentity(int pid)
    {
        if (pid <= 0) throw new ArgumentOutOfRangeException(nameof(pid));
        var path = $"/proc/{pid}/stat";
        var stat = File.ReadAllText(path);
        var closing = stat.LastIndexOf(") ", StringComparison.Ordinal);
        if (closing < 0) throw new InvalidDataException("Invalid Linux process metadata.");
        var fields = stat[(closing + 2)..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (fields.Length < 20 || fields[0] is "Z" or "X")
            throw new InvalidOperationException("The requested process is no longer running.");
        return fields[19];
    }

    private static string Required(string? value, string description) =>
        !string.IsNullOrWhiteSpace(value) ? value :
        throw new InvalidOperationException(description + " is unavailable.");
}
