using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Velopack;
using Velopack.Sources;

VelopackApp.Build().SetAutoApplyOnStartup(false).Run();
if (args.Length == 5 && args[0] == "--update-service")
    return await UpdateService.Run(int.Parse(args[1]), int.Parse(args[2]), args[3], args[4]);
// One running game per installation prevents an update replacing another game's files.
using var instance = new Mutex(false, "LegendOfCat-" + Convert.ToHexString(
    System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(
        Environment.GetEnvironmentVariable("APPIMAGE") ?? AppContext.BaseDirectory))));
try { if (!instance.WaitOne(0)) return 2; } catch (AbandonedMutexException) { }
var game = Path.Combine(AppContext.BaseDirectory, "Game",
    OperatingSystem.IsWindows() ? "LegendOfCat.exe" : "LegendOfCat");
if (!File.Exists(game)) return 1;
var start = new ProcessStartInfo(game) {
    UseShellExecute = false, WorkingDirectory = Path.GetDirectoryName(game)!
};
foreach (var arg in args) start.ArgumentList.Add(arg);
start.ArgumentList.Add("--launcher-pid");
start.ArgumentList.Add(Environment.ProcessId.ToString());
using var process = Process.Start(start);
if (process == null) return 1;
await process.WaitForExitAsync();
return process.ExitCode;

static class UpdateService
{
    const string Repository = "https://github.com/LeonHorner/LegendOfCat";
    static readonly object OutputLock = new();
    static void Send(object message)
    {
        lock (OutputLock) { Console.WriteLine(JsonSerializer.Serialize(message)); Console.Out.Flush(); }
    }
    public static async Task<int> Run(int gamePid, int launcherPid, string stateDirectory, string installedVersion)
    {
        Directory.CreateDirectory(stateDirectory);
        var marker = Path.Combine(stateDirectory, "update-pending.txt");
        var log = Path.Combine(stateDirectory, "updates.log");
        var transfer = new ReportingDownloader((progress, downloadedBytes, totalBytes) =>
            Send(new { state = "downloading", progress, downloadedBytes, totalBytes }));
        var manager = new UpdateManager(new GithubSource(Repository, null, false, transfer));
        if (File.Exists(marker)) {
            var target = (await File.ReadAllTextAsync(marker)).Trim();
            // A pending marker is not proof of failure: verify the actual version.
            if (target == installedVersion || UpdatePolicy.IsNewerStableVersion(installedVersion, target)) {
                File.Delete(marker);
            } else {
                Send(new { state = "warning", message = "The previous update did not complete. Retry from Settings. See updates.log and Velopack logs." });
                // Keep the target across launches until installation is confirmed.
            }
        }
        UpdateInfo? update = null;
        bool downloaded = false;
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("LegendOfCat-Updater/1.0");
        while (await Console.In.ReadLineAsync() is { } command)
        {
            try {
                if (command == "check") {
                    Send(new { state = "checking" });
                    await UpdateWorkflow.CheckAsync(
                        async () => {
                            var releases = new List<object>();
                            for (var page = 1; ; page++) {
                                var batch = await client.GetFromJsonAsync<JsonElement[]>(
                                    $"https://api.github.com/repos/LeonHorner/LegendOfCat/releases?per_page=100&page={page}") ?? [];
                                foreach (var release in batch) {
                                    if (!UpdatePolicy.IsStableRelease(release.GetProperty("draft").GetBoolean(),
                                        release.GetProperty("prerelease").GetBoolean())) continue;
                                    releases.Add(new { version = release.GetProperty("tag_name").GetString(),
                                        notes = release.GetProperty("body").GetString() ?? "",
                                        url = release.GetProperty("html_url").GetString() });
                                }
                                if (batch.Length < 100) break;
                            }
                            Send(new { state = "history", releases });
                        },
                        async () => {
                            if (!manager.IsInstalled) { Send(new { state = "development" }); return; }
                            update = await manager.CheckForUpdatesAsync().WaitAsync(TimeSpan.FromSeconds(20));
                            if (update != null && !UpdatePolicy.IsNewerStableVersion(
                                update.TargetFullRelease.Version.ToString(), manager.CurrentVersion?.ToString() ?? installedVersion))
                                update = null;
                            downloaded = false;
                            Send(new { state = update == null ? "current" : "available",
                                version = update?.TargetFullRelease.Version.ToString() ?? "" });
                        },
                        async historyError => {
                            try {
                                await File.AppendAllTextAsync(log,
                                    $"{DateTimeOffset.UtcNow:O} Optional release history unavailable: {historyError.GetType().Name} (HTTP {(historyError as HttpRequestException)?.StatusCode})\\n");
                            } catch (Exception) { /* Diagnostics are best effort. */ }
                            Send(new { state = "warning", message = "Release history is temporarily unavailable. Update checking continues." });
                        });
                } else if (command == "download" && update != null) {
                    transfer.Configure(update);
                    transfer.ReportOverall(0);
                    using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(30));
                    await manager.DownloadUpdatesAsync(update,
                        transfer.ReportOverall, timeout.Token);
                    downloaded = true;
                    Send(new { state = "ready" });
                } else if (command == "apply" && update != null && downloaded) {
                    if (OperatingSystem.IsLinux()) {
                        // Prepare and acknowledge the staged native supervisor BEFORE
                        // asking Unity to exit. The .NET helper then exits before the
                        // AppImage mount can be torn down.
                        await LinuxUpdateHandoff.StartAsync(
                            gamePid, launcherPid, stateDirectory, update.TargetFullRelease.FileName);
                        await File.WriteAllTextAsync(marker, update.TargetFullRelease.Version.ToString());
                        await File.AppendAllTextAsync(log,
                            $"{DateTimeOffset.UtcNow:O} Linux shutdown authorized; staged updater ready\n");
                        Send(new { state = "applying" });
                        return 0;
                    }

                    // Retain the proven Windows sequence. No staging on Windows.
                    await File.WriteAllTextAsync(marker, update.TargetFullRelease.Version.ToString());
                    await File.AppendAllTextAsync(log, $"{DateTimeOffset.UtcNow:O} Applying {update.TargetFullRelease.Version}\n");
                    Send(new { state = "applying" });
                    await UpdateWorkflow.ApplyAsync(
                        false,
                        async () => {
                            await WaitForExit(gamePid);
                            await File.AppendAllTextAsync(log, $"{DateTimeOffset.UtcNow:O} Game exited\n");
                        },
                        async () => {
                            if (launcherPid > 0) await WaitForExit(launcherPid);
                            await File.AppendAllTextAsync(log, $"{DateTimeOffset.UtcNow:O} Launcher exited\n");
                        },
                        () => {
                            File.AppendAllText(log, $"{DateTimeOffset.UtcNow:O} Starting Windows Velopack handoff\n");
                            manager.ApplyUpdatesAndRestart(update);
                        });
                    return 0;
                }
            } catch (Exception error) {
                try { await File.AppendAllTextAsync(log, $"{DateTimeOffset.UtcNow:O} {error}\n"); } catch { }
                Send(new { state = "error", message = "Update failed. You can keep playing and retry from Settings." });
            }
        }
        return 0;
    }
    static async Task WaitForExit(int pid)
    {
        try { using var process = Process.GetProcessById(pid); await process.WaitForExitAsync(); }
        catch (ArgumentException) { /* Already exited. */ }
    }
}
