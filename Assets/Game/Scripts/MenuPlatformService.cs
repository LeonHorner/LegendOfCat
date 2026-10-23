using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using UnityEngine;

// Presentation consumes this boundary; platforms without an updater can use the no-op service.
public interface IMenuPlatformService : IDisposable
{
    bool SupportsUpdates { get; }
    bool TryReceive(out string message);
    void Send(string command);
    void CompleteApply();
}

public sealed class UnavailableMenuPlatformService : IMenuPlatformService
{
    public bool SupportsUpdates => false;
    public bool TryReceive(out string message) { message = null; return false; }
    public void Send(string command) { }
    public void CompleteApply() { }
    public void Dispose() { }
}

public sealed class DesktopMenuPlatformService : IMenuPlatformService
{
    readonly ConcurrentQueue<string> messages = new ConcurrentQueue<string>();
    Process helper;
    bool applying, stopped;
    public bool SupportsUpdates => true;

    public DesktopMenuPlatformService(string executable, int launcherPid, string settingsPath, string version)
    {
        try {
            helper = new Process { StartInfo = new ProcessStartInfo(executable) {
                UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true,
                Arguments = "--update-service " + Process.GetCurrentProcess().Id + " " + launcherPid + " " +
                    Quote(settingsPath) + " " + Quote(version)
            }};
            helper.OutputDataReceived += (_, args) => { if (args.Data != null) messages.Enqueue(args.Data); };
            helper.ErrorDataReceived += (_, args) => { };
            helper.Start();
            helper.BeginOutputReadLine();
            helper.BeginErrorReadLine();
            Send("check");
        } catch (Exception) { Fail("Update service could not start. You can keep playing."); }
    }

    static string Quote(string value) => "\"" + value.Replace("\"", "\\\"") + "\"";
    void Fail(string message)
    {
        stopped = true;
        messages.Enqueue(JsonUtility.ToJson(new StarterScreen.Message { state = "error", message = message }));
    }
    public bool TryReceive(out string message)
    {
        if (!stopped && !applying && helper != null) {
            try { if (helper.HasExited) Fail("Update service stopped. Restart the game to retry."); }
            catch (InvalidOperationException) { }
        }
        return messages.TryDequeue(out message);
    }
    public void Send(string command)
    {
        try {
            helper.StandardInput.WriteLine(command);
            helper.StandardInput.Flush();
        } catch (Exception) { Fail("Update service is unavailable. Restart the game to retry."); }
    }
    public void CompleteApply() { applying = true; }
    public void Dispose()
    {
        if (helper == null) return;
        if (!applying) {
            try { if (!helper.HasExited) helper.Kill(); } catch (Exception) { }
        }
        helper.Dispose();
    }

    public static IMenuPlatformService Create()
    {
        if (Application.isEditor || (Application.platform != RuntimePlatform.WindowsPlayer &&
            Application.platform != RuntimePlatform.LinuxPlayer)) return new UnavailableMenuPlatformService();
        var executable = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..",
            Application.platform == RuntimePlatform.WindowsPlayer ? "LegendOfCat.exe" : "LegendOfCat"));
        var args = Environment.GetCommandLineArgs();
        var index = Array.IndexOf(args, "--launcher-pid");
        int launcherPid = 0;
        if (index >= 0 && index + 1 < args.Length) int.TryParse(args[index + 1], out launcherPid);
        // Opening the internal player bypasses the entry process's installation lock.
        if (!File.Exists(executable) || launcherPid == 0) return new UnavailableMenuPlatformService();
        return new DesktopMenuPlatformService(executable, launcherPid, Application.persistentDataPath, Application.version);
    }
}
