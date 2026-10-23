using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

public sealed class StarterScreen : MonoBehaviour
{
    [Serializable] public class Release { public string version, notes, url; }
    [Serializable] public class Message {
        public string state, version, message;
        public int progress;
        public Release[] releases;
    }
    [Serializable] public class Preferences { public string lastOfferedVersion = ""; }
    readonly ConcurrentQueue<string> messages = new ConcurrentQueue<string>();
    Process helper;
    Preferences preferences = new Preferences();
    Release[] releases = new Release[0];
    string page = "menu", state = "development", available = "", error = "", warning = "";
    string preferencesPath;
    bool popup, applying;
    int progress;
    Vector2 scroll;
    GUIStyle titleStyle, textStyle;

    public static bool ShouldOffer(string availableVersion, string lastOfferedVersion)
        => UpdatePolicy.ShouldOffer(availableVersion, lastOfferedVersion);

    void Start()
    {
        if (Environment.GetCommandLineArgs().Contains("--smoke-test")) { Application.Quit(0); return; }
        preferencesPath = Path.Combine(Application.persistentDataPath, "menu-settings.json");
        try {
            if (File.Exists(preferencesPath))
                preferences = JsonUtility.FromJson<Preferences>(File.ReadAllText(preferencesPath)) ?? new Preferences();
        } catch (Exception) { warning = "Settings could not be read. Defaults are in use."; }
        if (Application.isEditor) return;
        var executable = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..",
            Application.platform == RuntimePlatform.WindowsPlayer ? "LegendOfCat.exe" : "LegendOfCat"));
        if (!File.Exists(executable)) return;
        var args = Environment.GetCommandLineArgs();
        var index = Array.IndexOf(args, "--launcher-pid");
        int launcherPid = 0;
        if (index >= 0 && index + 1 < args.Length) int.TryParse(args[index + 1], out launcherPid);
        // Opening the internal player bypasses the entry process's installation lock.
        if (launcherPid == 0) return;
        try {
            var start = new ProcessStartInfo(executable) {
                UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true,
                Arguments = "--update-service " + Process.GetCurrentProcess().Id + " " + launcherPid + " " +
                    Quote(Application.persistentDataPath) + " " + Quote(Application.version)
            };
            helper = new Process { StartInfo = start };
            helper.OutputDataReceived += (_, eventArgs) => { if (eventArgs.Data != null) messages.Enqueue(eventArgs.Data); };
            helper.ErrorDataReceived += (_, eventArgs) => { };
            helper.Start();
            helper.BeginOutputReadLine();
            helper.BeginErrorReadLine();
            Send("check");
        } catch (Exception) { state = "error"; error = "Update service could not start. You can keep playing."; }
    }
    static string Quote(string value) => "\"" + value.Replace("\"", "\\\"") + "\"";
    void Send(string command)
    {
        try {
            helper.StandardInput.WriteLine(command);
            helper.StandardInput.Flush();
            if (command == "check") { state = "checking"; error = ""; }
            if (command == "download") { state = "downloading"; error = ""; popup = false; page = "updates"; }
        } catch (Exception) { state = "error"; error = "Update service is unavailable. Restart the game to retry."; }
    }
    void Update()
    {
        while (messages.TryDequeue(out var line)) {
            Message message;
            try { message = JsonUtility.FromJson<Message>(line); } catch (Exception) { continue; }
            if (message == null || string.IsNullOrEmpty(message.state)) continue;
            if (message.state == "history") { releases = message.releases ?? new Release[0]; continue; }
            if (message.state == "warning") { warning = message.message; continue; }
            state = message.state;
            progress = message.progress;
            if (state == "error") error = message.message;
            if (state == "available") {
                available = message.version;
                if (ShouldOffer(available, preferences.lastOfferedVersion)) {
                    popup = true;
                    preferences.lastOfferedVersion = available;
                    try {
                        Directory.CreateDirectory(Application.persistentDataPath);
                        File.WriteAllText(preferencesPath, JsonUtility.ToJson(preferences));
                    } catch (Exception) { warning = "The update reminder could not be saved."; }
                }
            }
            if (state == "current") available = "";
            if (state == "ready") Send("apply");
            if (state == "applying") { applying = true; Application.Quit(0); }
        }
        if (helper != null && !applying && state != "error") {
            try { if (helper.HasExited) { state = "error"; error = "Update service stopped. Restart the game to retry."; } }
            catch (InvalidOperationException) { }
        }
    }
    void OnApplicationQuit()
    {
        if (helper == null || applying) return;
        try { if (!helper.HasExited) helper.Kill(); } catch (Exception) { }
        helper.Dispose();
    }
    void OnGUI()
    {
        if (titleStyle == null) {
            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 30, alignment = TextAnchor.MiddleCenter };
            textStyle = new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = 16 };
        }
        var previous = GUI.color;
        GUI.color = new Color(0.055f, 0.07f, 0.11f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = previous;
        float width = Mathf.Min(720, Screen.width - 32), height = Mathf.Min(680, Screen.height - 32);
        GUILayout.BeginArea(new Rect((Screen.width - width) / 2, (Screen.height - height) / 2, width, height), GUI.skin.box);
        GUILayout.Label("LegendOfCat", titleStyle);
        GUILayout.Label("Version " + Application.version, textStyle);
        if (!string.IsNullOrEmpty(warning)) GUILayout.Label(warning, textStyle);
        GUI.enabled = !popup && state != "applying";
        scroll = GUILayout.BeginScrollView(scroll);
        if (page == "menu") {
            GUILayout.Space(30);
            if (Button("Play")) page = "play";
            if (Button(string.IsNullOrEmpty(available) ? "Settings" : "Settings • Update available")) page = "settings";
            if (Button("Quit")) Application.Quit();
        } else if (page == "play") {
            GUILayout.Label("Test scene", titleStyle);
            GUILayout.Label("The menu and update flow are ready to test. Gameplay is not available yet.", textStyle);
            if (Button("Back to menu")) page = "menu";
        } else if (page == "settings") {
            GUILayout.Label("Settings", titleStyle);
            GUI.enabled = false;
            Button("Language — Coming later");
            GUI.enabled = !popup;
            if (Button(string.IsNullOrEmpty(available) ? "Versions & Updates" : "Versions & Updates • Update available")) page = "updates";
            if (Button("Back")) page = "menu";
        } else {
            GUILayout.Label("Versions & Updates", titleStyle);
            GUILayout.Label(StatusText(), textStyle);
            if (!string.IsNullOrEmpty(warning)) GUILayout.Label(warning, textStyle);
            if (!string.IsNullOrEmpty(error)) GUILayout.Label(error, textStyle);
            bool busy = state == "checking" || state == "downloading" || state == "ready" || state == "applying";
            GUI.enabled = !popup && !busy && helper != null;
            if (Button(state == "error" ? "Retry" : "Check for updates")) Send("check");
            if (!string.IsNullOrEmpty(available) && Button("Update now — " + available)) Send("download");
            GUI.enabled = !popup;
            foreach (var release in releases) {
                GUILayout.Space(12);
                GUILayout.Label(release.version, textStyle);
                GUILayout.Label(release.notes, textStyle);
                if (Button("Release notes") && IsReleaseUrl(release.url)) Application.OpenURL(release.url);
            }
            GUILayout.Label("Older releases are listed for reference. Downgrades are not supported.", textStyle);
            if (Button(state == "error" ? "Later" : "Back")) page = "settings";
        }
        GUILayout.EndScrollView();
        GUI.enabled = true;
        GUILayout.EndArea();
        if (popup) {
            var popupWidth = Mathf.Min(460, Screen.width - 24);
            GUILayout.BeginArea(new Rect((Screen.width - popupWidth) / 2, (Screen.height - 230) / 2, popupWidth, 230), GUI.skin.window);
            GUILayout.Label("Update available", titleStyle);
            GUILayout.Label("Version " + available + " is available. Restart to install after the download completes.", textStyle);
            if (Button("Update now")) Send("download");
            if (Button("Later")) popup = false;
            GUILayout.EndArea();
        }
    }
    public static bool IsReleaseUrl(string url) => UpdatePolicy.IsReleaseUrl(url);
    static bool Button(string label) => GUILayout.Button(label, GUILayout.MinHeight(38));
    string StatusText()
    {
        switch (state) {
            case "checking": return "Checking for updates…";
            case "available": return "Update available: " + available;
            case "downloading": return "Downloading update: " + progress + "%";
            case "ready": case "applying": return "Restarting to install the update…";
            case "current": return "You are up to date.";
            case "error": return "Update unavailable. Your installed version is still playable.";
            default: return "Development build: update installation is unavailable.";
        }
    }
}
