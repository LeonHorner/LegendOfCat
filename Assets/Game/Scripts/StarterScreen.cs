using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using UnityEngine;

public sealed partial class StarterScreen : MonoBehaviour
{
    [Serializable] public class Release { public string version, notes, url; }
    [Serializable] public class Message {
        public string state, version, message;
        public int progress;
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
    Vector2 scroll;
    MovementCourse course;
    VersionedJsonFile displayFile;
    bool fullscreen;
    int windowWidth = 1280, windowHeight = 720;
    int lastWidth, lastHeight;
    float changedAt;

    void LoadDisplay()
    {
        var path = Path.Combine(Application.persistentDataPath, "display-settings.json");
        displayFile = DisplaySettings.Open(path);
        if (!displayFile.CanWrite && !string.IsNullOrEmpty(displayFile.Warning))
            warning = displayFile.Warning;
        windowWidth = (int)displayFile.Data["width"];
        windowHeight = (int)displayFile.Data["height"];
        fullscreen = (bool)displayFile.Data["fullscreen"];
        Screen.SetResolution(windowWidth, windowHeight,
            fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        lastWidth = windowWidth;
        lastHeight = windowHeight;
        changedAt = Time.unscaledTime;
    }

    void SaveDisplay()
    {
        if (displayFile == null || !displayFile.CanWrite) return;
        try {
            displayFile.Data["width"] = windowWidth;
            displayFile.Data["height"] = windowHeight;
            displayFile.Data["fullscreen"] = fullscreen;
            displayFile.Save();
        } catch (Exception) {
            warning = "Display preferences could not be saved. Existing settings were preserved.";
        }
    }

    void SetFullscreen(bool value)
    {
        if (fullscreen == value) return;
        fullscreen = value;
        Screen.SetResolution(windowWidth, windowHeight,
            value ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        changedAt = Time.unscaledTime;
        SaveDisplay();
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
        if (!Preview) LoadDisplay();
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
        if (command == "download") { state = "downloading"; error = ""; popup = false; page = "updates"; }
    }
    void Update()
    {
        if (!Preview && displayFile != null) {
            if (Input.GetKeyDown(KeyCode.Return) &&
                (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
                SetFullscreen(!fullscreen);
            if (!fullscreen && Screen.width >= 640 && Screen.height >= 360) {
                if (lastWidth != Screen.width || lastHeight != Screen.height) {
                    lastWidth = Screen.width; lastHeight = Screen.height;
                    changedAt = Time.unscaledTime;
                } else if ((windowWidth != lastWidth || windowHeight != lastHeight) &&
                           Time.unscaledTime - changedAt > 1.5f) {
                    windowWidth = lastWidth; windowHeight = lastHeight;
                    SaveDisplay();
                }
            }
        }
        while (platform != null && platform.TryReceive(out var incoming)) messages.Enqueue(incoming);
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
            foreach (string viewName in new[] { "menu", "settings", "play", "popup", "checking", "available", "downloading", "ready", "current", "error", "warning", "development", "narrow", "cutout", "popup-narrow" }) {
                popup = false; warning = ""; error = ""; previewUnsupported = false;
                available = "0.1.0 (preview example)"; progress = 47;
                state = viewName; page = "updates"; scroll = Vector2.zero;
                simulateCutout = viewName == "cutout";
                bool narrow = viewName == "narrow" || viewName == "popup-narrow";
                Screen.SetResolution(narrow ? 390 : viewName == "cutout" ? 640 : 1280, narrow ? 844 : viewName == "cutout" ? 480 : 720, FullScreenMode.Windowed);
                if (viewName == "menu" || viewName == "settings" || viewName == "play") page = viewName;
                if (viewName == "narrow" || viewName == "cutout") page = "settings";
                if (viewName == "popup" || viewName == "popup-narrow") { state = "available"; popup = true; }
                if (viewName == "error") error = "Simulated download failure. Retry or choose Later.";
                if (viewName == "warning") { state = "current"; available = ""; warning = "Preview warning: settings could not be saved. Existing settings are preserved."; }
                if (viewName == "development") { previewUnsupported = true; available = ""; }
                if (viewName == "current") available = "";
                yield return new WaitForSecondsRealtime(0.7f);
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
            if (view == "downloading") progress = 47;
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
            messages.Enqueue("{\"state\":\"downloading\",\"progress\":" + value + "}");
            yield return new WaitForSecondsRealtime(0.1f);
        }
        messages.Enqueue("{\"state\":\"ready\"}");
    }
#endif
    void OnApplicationQuit() { platform?.Dispose(); }
    public static bool IsReleaseUrl(string url) => UpdatePolicy.IsReleaseUrl(url);
    string StatusText()
    {
        switch (state) {
            case "checking": return "Checking for updates...";
            case "available": return "Update available: " + available;
            case "downloading": return "Downloading update: " + progress + "%";
            case "ready": case "applying": return "Restarting to install the update...";
            case "current": return "You are up to date.";
            case "previewComplete": return "Preview download complete. No update was installed.";
            case "error": return "Update unavailable. Your installed version is still playable.";
            default: return "Updates are unavailable on this platform or when opened without the desktop launcher.";
        }
    }
}
