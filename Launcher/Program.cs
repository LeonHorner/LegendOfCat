using System.Diagnostics;
using Velopack;
using Velopack.Sources;

VelopackApp.Build().Run();
var smokeTest = args.Contains("--smoke-test");
if (!smokeTest)
{
    try
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var manager = new UpdateManager(new GithubSource(
            "https://github.com/LeonHorner/LegendOfCat", accessToken: null, prerelease: false));
        if (manager.IsInstalled)
        {
            var update = await manager.CheckForUpdatesAsync().WaitAsync(timeout.Token);
            if (update != null)
            {
                await manager.DownloadUpdatesAsync(update);
                manager.ApplyUpdatesAndRestart(update);
                return 0;
            }
        }
    }
    catch (Exception error)
    {
        // Update failures must not prevent offline play.
        var logDirectory = Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData), "LegendOfCat");
        try
        {
            Directory.CreateDirectory(logDirectory);
            File.AppendAllText(Path.Combine(logDirectory, "launcher.log"),
                DateTimeOffset.UtcNow + " " + error.Message + Environment.NewLine);
        }
        catch { /* Logging must not prevent play either. */ }
    }
}
var game = Path.Combine(AppContext.BaseDirectory, "Game",
    OperatingSystem.IsWindows() ? "LegendOfCat.exe" : "LegendOfCat");
if (!File.Exists(game))
{
    Console.Error.WriteLine("Game files are missing. Reinstall LegendOfCat.");
    return 1;
}
var start = new ProcessStartInfo(game) {
    UseShellExecute = false,
    WorkingDirectory = Path.GetDirectoryName(game)!
};
foreach (var arg in args) start.ArgumentList.Add(arg);
using var process = Process.Start(start);
if (process == null) return 1;
await process.WaitForExitAsync();
return process.ExitCode;
