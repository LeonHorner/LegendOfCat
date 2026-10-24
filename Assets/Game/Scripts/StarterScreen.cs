using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed partial class StarterScreen : MonoBehaviour
{
    [Serializable] public class Release {
        public string version, notes, url;
        [NonSerialized] public ReleaseHistory.Block[] summary;
    }
    [Serializable] public class Message {
        public string state, version, message;
        public int progress;
        public long downloadedBytes, totalBytes;
        public Release[] releases;
    }
    [Serializable] public class Preferences { public string lastOfferedVersion = ""; }
#if LEGEND_OF_CAT_PREVIEW
    static readonly bool Preview = true;
#else
    static readonly bool Preview = false;
#endif
    readonly ConcurrentQueue<string> messages = new ConcurrentQueue<string>();
    IMenuPlatformService platform;
    Preferences preferences = new Preferences();
    VersionedJsonFile settingsFile;
    Release[] releases = new Release[0];
    string page = "menu", state = "development", available = "", error = "", warning = "";
    string preferencesPath;
    bool popup, applying;
    int progress;
    long downloadedBytes, totalBytes;
    Vector2 scroll;
    MovementCourse course;
    VersionedJsonFile displayFile;
    bool fullscreen;
    DisplayState display;

    void LoadDisplay()
    {
        displayFile = DisplaySettings.Open(Path.Combine(Application.persistentDataPath, "display-settings.json"));
        if (!displayFile.CanWrite) warning = displayFile.Warning;
        display = new DisplayState((int)displayFile.Data["width"], (int)displayFile.Data["height"], (bool)displayFile.Data["fullscreen"]);
        fullscreen = display.Fullscreen;
        display.Request(fullscreen, Time.unscaledTime);
        var desktop = Screen.currentResolution;
        Screen.SetResolution(fullscreen ? desktop.width : display.Width, fullscreen ? desktop.height : display.Height,
            fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
    }

    void SaveDisplay()
    {
        if (displayFile == null || !displayFile.CanWrite) return;
        try {
            displayFile.Data["width"] = display.Width;
            displayFile.Data["height"] = display.Height;
            displayFile.Data["fullscreen"] = display.Fullscreen;
            displayFile.Save();
        } catch (Exception) {
            warning = "Display preferences could not be saved. Existing settings were preserved.";
        }
    }

    void ObserveDisplay(bool flush = false)
    {
        if (display == null) return;
        if (display.Observe(Screen.width, Screen.height, Screen.fullScreen, Time.unscaledTime, flush)) SaveDisplay();
        if (!display.Pending) fullscreen = display.Fullscreen;
    }

    void SetFullscreen(bool value)
    {
        if (display == null || display.Switching || fullscreen == value) return;
        // Capture a recent border resize before leaving windowed mode, even during debounce.
        ObserveDisplay(true);
        fullscreen = value;
        display.Request(value, Time.unscaledTime);
        var desktop = Screen.currentResolution;
        Screen.SetResolution(value ? desktop.width : display.Width, value ? desktop.height : display.Height,
            value ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
    }

    void OpenCourse()
    {
        if (course != null || popup || applying) return;
        course = Instantiate(Resources.Load<MovementCourse>("MovementCourse"));
        course.BackToMenu = CloseCourse;
    }

    void CloseCourse()
    {
        if (course == null) return;
        // Stop input and rendering immediately, before deferred destruction at the end of the frame.
        course.gameObject.SetActive(false);
        Destroy(course.gameObject);
        course = null;
        page = "menu";
        lastPage = null;
        previousFocus.Clear();
    }

    public static bool ShouldOffer(string availableVersion, string lastOfferedVersion)
        => UpdatePolicy.ShouldOffer(availableVersion, lastOfferedVersion);

    void Start()
    {
        if (Environment.GetCommandLineArgs().Contains("--smoke-test")) { Application.Quit(0); return; }
        preferencesPath = Path.Combine(Application.persistentDataPath, "menu-settings.json");
        LoadPreferences();
        if (!Application.isEditor) LoadDisplay();
#if LEGEND_OF_CAT_PREVIEW
        messages.Enqueue("{\"state\":\"history\",\"releases\":[{\"version\":\"0.1.0 (preview example)\",\"notes\":\"Sample release notes for UI inspection. No release has been published.\",\"url\":\"https://github.com/LeonHorner/LegendOfCat/releases/\"}]}");
#endif
        platform = Preview ? new UnavailableMenuPlatformService() : DesktopMenuPlatformService.Create();
        if (platform.SupportsUpdates) state = "checking";
#if LEGEND_OF_CAT_PREVIEW
        StartCoroutine(CapturePreview());
#endif
    }
    void LoadPreferences()
    {
        settingsFile = MenuSettings.Open(preferencesPath);
        preferences.lastOfferedVersion = (string)settingsFile.Data["lastOfferedVersion"];
        warning = settingsFile.Warning;
    }
    void Send(string command)
    {
#if LEGEND_OF_CAT_PREVIEW
        if (Preview) {
            previewUnsupported = false;
            if (command == "check") messages.Enqueue("{\"state\":\"available\",\"version\":\"0.1.0 (preview example)\"}");
            if (command == "download") { popup = false; page = "updates"; StartCoroutine(PreviewDownload()); }
            if (command == "apply") state = "previewComplete";
            return;
        }
#endif
        if (platform == null || !platform.SupportsUpdates) {
            if (command == "apply") { state = "error"; error = "Update service is unavailable. Restart the game to retry."; }
            return;
        }
        platform.Send(command);
        if (command == "check") { state = "checking"; error = ""; }
        if (command == "download") { downloadedBytes = totalBytes = 0; state = "downloading"; error = ""; popup = false; page = "updates"; }
    }
    // IMGUI navigation uses Event.current; only the desktop shortcut polls a device.
    internal static bool FullscreenShortcutPressed(Keyboard keyboard)
        => keyboard != null && (keyboard.f11Key.wasPressedThisFrame ||
            ((keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
                && (keyboard.leftAltKey.isPressed || keyboard.rightAltKey.isPressed)));

    void Update()
    {
        if (display != null) {
            ObserveDisplay();
            if (FullscreenShortcutPressed(Keyboard.current))
                SetFullscreen(!fullscreen);
        }
        while (platform != null && platform.TryReceive(out var incoming)) messages.Enqueue(incoming);
        while (messages.TryDequeue(out var line)) {
            Message message;
            try { message = JsonUtility.FromJson<Message>(line); } catch (Exception) { continue; }
            if (message == null || string.IsNullOrEmpty(message.state)) continue;
            if (message.state == "history") { releases = message.releases ?? new Release[0]; continue; }
            if (message.state == "warning") { warning = message.message; continue; }
            state = message.state;
            progress = Mathf.Clamp(message.progress, 0, 100);
            downloadedBytes = Math.Max(0, message.downloadedBytes);
            totalBytes = Math.Max(0, message.totalBytes);
            if (state == "error") error = message.message;
            if (state == "available") {
                available = message.version;
                if (ShouldOffer(available, preferences.lastOfferedVersion)) {
                    popup = true;
                    preferences.lastOfferedVersion = available;
                    try {
                        if (settingsFile == null) settingsFile = MenuSettings.Open(preferencesPath);
                        settingsFile.Data["lastOfferedVersion"] = available;
                        settingsFile.Save();
                    } catch (Exception) {
                        warning = settingsFile != null && !string.IsNullOrEmpty(settingsFile.Warning)
                            ? settingsFile.Warning : "The update reminder could not be saved. Existing settings are preserved.";
                    }
                }
            }
            if (state == "current") available = "";
            if (state == "ready") Send("apply");
            if (state == "applying" && platform != null && platform.SupportsUpdates) { applying = true; platform.CompleteApply(); Application.Quit(0); }
        }
    }
#if LEGEND_OF_CAT_PREVIEW
    // Optional local visual inspection, compiled out of production players.
    System.Collections.IEnumerator CapturePreview(string suiteOverride = null)
    {
        var args = Environment.GetCommandLineArgs();
        string Argument(string key) {
            int index = Array.IndexOf(args, key);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : "";
        }
        string suite = suiteOverride ?? Argument("--preview-suite");
        if (!string.IsNullOrEmpty(suite) || !string.IsNullOrEmpty(Argument("--preview-capture")))
            Application.runInBackground = true;
        if (!string.IsNullOrEmpty(suite)) {
            yield return new WaitForSecondsRealtime(12);
            Directory.CreateDirectory(suite);
            previewTools = false;
            // A real resize wakes graphics on hidden local capture windows before the first shot.
            Screen.SetResolution(1000, 700, FullScreenMode.Windowed);
            yield return new WaitForSecondsRealtime(1);
            foreach (string viewName in new[] { "history", "history-bottom", "history-minimum", "history-wide", "history-narrow", "menu", "settings", "play", "popup", "checking", "available", "downloading", "ready", "current", "error", "warning", "development", "narrow", "cutout", "popup-narrow" }) {
                popup = false; warning = ""; error = ""; previewUnsupported = false;
                available = "0.1.0 (preview example)"; progress = 47;
                state = viewName; page = "updates"; scroll = Vector2.zero; downloadedBytes = totalBytes = 0;
                simulateCutout = viewName == "cutout";
                bool narrow = viewName == "narrow" || viewName == "popup-narrow";
                Screen.SetResolution(narrow ? 390 : viewName == "cutout" ? 640 : 1280, narrow ? 844 : viewName == "cutout" ? 480 : 720, FullScreenMode.Windowed);
                if (viewName.StartsWith("history", StringComparison.Ordinal)) {
                    page = "updates"; state = "current"; available = "";
                    releases = Enumerable.Range(0, 60).Select(i => new Release {
                        version = "0.2.0",
                        notes = "## Download LegendOfCat\n- [Installer](https://example.org/download)\n# [0.2.0](https://example.org)\n### Bug Fixes\n* **settings:** preserve window dimensions ([abcdef123](https://example.org/commit))\n- Keep readable menus and [original notes](https://example.org/notes).",
                        url = "https://github.com/LeonHorner/LegendOfCat/releases/tag/v0.2.0"
                    }).ToArray();
                    Screen.SetResolution(viewName == "history-minimum" ? 960 : viewName == "history-wide" ? 2560 : viewName == "history-narrow" ? 540 : 1280,
                        viewName == "history-minimum" ? 540 : viewName == "history-wide" ? 1080 : 720, FullScreenMode.Windowed);
                }
                if (viewName == "menu" || viewName == "settings" || viewName == "play") page = viewName;
                if (viewName == "narrow" || viewName == "cutout") page = "settings";
                if (viewName == "popup" || viewName == "popup-narrow") { state = "available"; popup = true; }
                if (viewName == "downloading") { downloadedBytes = 47000000; totalBytes = 100000000; }
                if (viewName == "error") error = "Simulated download failure. Retry or choose Later.";
                if (viewName == "warning") { state = "current"; available = ""; warning = "Preview warning: settings could not be saved. Existing settings are preserved."; }
                if (viewName == "development") { previewUnsupported = true; available = ""; }
                if (viewName == "current") available = "";
                yield return new WaitForSecondsRealtime(0.7f);
                if (viewName == "history-bottom") { scroll.y = 100000; yield return null; }
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(Path.Combine(suite, viewName + ".png"));
                yield return new WaitForSecondsRealtime(0.5f);
            }
            Application.Quit();
            yield break;
        }
        string view = Argument("--preview-view");
        yield return null;
        if (view == "menu" || view == "settings" || view == "play") page = view;
        else if (!string.IsNullOrEmpty(view)) {
            page = "updates";
            state = view;
            available = "0.1.0 (preview example)";
            if (view == "popup") { state = "available"; popup = true; }
            if (view == "downloading") { progress = 47; downloadedBytes = 47000000; totalBytes = 100000000; }
            if (view == "error") error = "Simulated download failure. Retry or choose Later.";
            if (view == "warning") { state = "current"; available = ""; warning = "Preview warning: settings could not be saved. Existing settings are preserved."; }
            if (view == "current" || view == "development") available = "";
            if (view == "development") previewUnsupported = true;
        }
        simulateCutout = args.Contains("--preview-cutout");
        string capture = Argument("--preview-capture");
        if (string.IsNullOrEmpty(capture)) yield break;
        yield return new WaitForSecondsRealtime(12);
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(capture);
        yield return new WaitForSecondsRealtime(1);
        Application.Quit();
    }
    System.Collections.IEnumerator PreviewDownload()
    {
        for (int value = 0; value <= 100; value += 5) {
            messages.Enqueue("{\"state\":\"downloading\",\"progress\":" + value + ",\"downloadedBytes\":" + (value * 1000000L) + ",\"totalBytes\":100000000}");
            yield return new WaitForSecondsRealtime(0.1f);
        }
        messages.Enqueue("{\"state\":\"ready\"}");
    }
#endif
    void OnApplicationQuit() { ObserveDisplay(true); platform?.Dispose(); }
    public static bool IsReleaseUrl(string url) => UpdatePolicy.IsReleaseUrl(url);
    string StatusText()
    {
        switch (state) {
            case "checking": return "Checking for updates...";
            case "available": return "Update available: " + available;
            case "downloading":
                var size = DownloadProgress.Format(downloadedBytes, totalBytes);
                return (totalBytes > 0 && downloadedBytes >= totalBytes ? "Download complete. Preparing update: " : "Downloading update: ") +
                    progress + "%" + (size.Length > 0 ? "\n" + size : "");
            case "ready": case "applying": return "Restarting to install the update...";
            case "current": return "You are up to date.";
            case "previewComplete": return "Preview download complete. No update was installed.";
            case "error": return "Update unavailable. Your installed version is still playable.";
            default: return "Updates are unavailable on this platform or when opened without the desktop launcher.";
        }
    }
}
