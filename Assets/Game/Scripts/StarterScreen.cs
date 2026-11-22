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
    StompArena stompArena;
    RangedArena rangedArena;
    RunLifecycle ActiveRun => course != null ? course.Run : rangedArena != null ? rangedArena.Run : stompArena != null ? stompArena.Run : null;
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

    public bool ShowLevelProgress { get; private set; } = true;
    public float MasterVolume { get; private set; } = .7f;
    bool volumeDirty;
    float volumeSaveAt;

    void SetMasterVolume(float value)
    {
        float adjusted = Mathf.Round(Mathf.Clamp01(value) * 100) / 100;
        if (Mathf.Approximately(MasterVolume, adjusted)) return;
        MasterVolume = adjusted;
        AudioListener.volume = MasterVolume;
        if (settingsFile == null) return;
        settingsFile.Data["masterVolume"] = MasterVolume;
        volumeDirty = true;
        volumeSaveAt = Time.unscaledTime + .25f;
    }
    void SaveVolumePreference()
    {
        if (!volumeDirty) return;
        volumeDirty = false;
        try {
            settingsFile.Save();
        } catch (Exception) {
            warning = "The sound preference could not be saved. Existing settings were preserved.";
        }
    }

    void OpenLevelOne() => OpenLevel("golden-river");

    public void OpenLevel(string id)
    {
        if (course != null || stompArena != null || rangedArena != null || popup || applying) return;
        course = LevelRegistry.Create(id);
        course.Progress.Visible = ShowLevelProgress;
        course.BackToMenu = CloseCourse;
        course.Cat.GetComponent<CatInput>().TogglePause = ToggleGameplayPause;
        gameplaySettings = false;
    }

    void SetShowLevelProgress(bool visible)
    {
        ShowLevelProgress = visible;
        if (course != null) course.Progress.Visible = visible;
        try {
            settingsFile.Data["showLevelProgress"] = visible;
            settingsFile.Save();
        } catch (Exception) {
            warning = "The level progress preference could not be saved.";
        }
    }

    void OpenRangedArena()
    {
        if (course != null || stompArena != null || rangedArena != null || popup || applying) return;
        rangedArena = new GameObject("Ranged combat lab").AddComponent<RangedArena>();
        rangedArena.BackToMenu = CloseRangedArena;
        rangedArena.Cat.GetComponent<CatInput>().TogglePause = ToggleGameplayPause;
        gameplaySettings = false;
    }

    void CloseRangedArena()
    {
        if (rangedArena == null) return;
        rangedArena.gameObject.SetActive(false);
        Destroy(rangedArena.gameObject);
        rangedArena = null;
        page = "menu";
        lastPage = null;
        previousFocus.Clear();
    }

    void OpenStompArena()
    {
        if (course != null || stompArena != null || rangedArena != null || popup || applying) return;
        stompArena = new GameObject("Simple combat lab").AddComponent<StompArena>();
        stompArena.BackToMenu = CloseStompArena;
        stompArena.Cat.GetComponent<CatInput>().TogglePause = ToggleGameplayPause;
        gameplaySettings = false;
    }

    void CloseStompArena()
    {
        if (stompArena == null) return;
        stompArena.gameObject.SetActive(false);
        Destroy(stompArena.gameObject);
        stompArena = null;
        page = "menu";
        lastPage = null;
        previousFocus.Clear();
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
        ShowLevelProgress = (bool)settingsFile.Data["showLevelProgress"];
        MasterVolume = (float)settingsFile.Data["masterVolume"];
        AudioListener.volume = MasterVolume;
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
        if (volumeDirty && Time.unscaledTime >= volumeSaveAt) SaveVolumePreference();
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
            foreach (string viewName in new[] { "history", "history-bottom", "history-minimum", "history-wide", "history-narrow", "menu", "settings", "popup", "checking", "available", "downloading", "ready", "current", "error", "warning", "development", "narrow", "cutout", "popup-narrow" }) {
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
                if (viewName == "menu" || viewName == "settings") page = viewName;
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
        if (view == "ranged" || view == "ranged-paused" || view == "ranged-controls") {
            OpenRangedArena();
            if (view != "ranged") {
                yield return new WaitForSecondsRealtime(2);
                bool compact = args.Contains("--ranged-compact");
                Screen.SetResolution(compact ? 640 : 960, compact ? 360 : 540, FullScreenMode.Windowed);
                rangedArena.TogglePause();
            }
            string recording = Argument("--ranged-record");
            if (!string.IsNullOrEmpty(recording)) {
                yield return RecordRangedPreview(recording);
                Application.Quit();
                yield break;
            }
        }
        else if (view == "level-one" || view == "level-one-ascent" || view == "level-one-lift" || view == "level-one-finale" || view == "level-one-complete" || view == "level-one-audio" || view == "level-one-water" || view == "level-one-settings") {
            OpenLevelOne();
            if (view == "level-one-audio")
                course.Cat.RespawnAt(MovementCourse.LevelOneCheckpointSpawns[0] - Vector2.right * 5);
            if (view == "level-one-water")
                course.Cat.RespawnAt(new Vector2(20, -1.75f));
            if (view == "level-one-settings") {
                yield return null;
                course.TogglePause(); OpenGameplaySettings();
            }
            if (view == "level-one-complete") {
                course.Cat.RespawnAt(MovementCourse.LevelOneExit - Vector2.right * 3);
                course.Cat.Motion.SetMove(1);
                while (!course.HasReachedExit) yield return null;
            }
            if (view == "level-one-finale") course.StartFinalePreview();
            if (view == "level-one-ascent") course.StartAscentPreview();
            if (view == "level-one-lift") course.StartVerticalLiftPreview();
        }
        else if (view == "menu" || view == "settings" || view == "sound-credits") page = view;
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
        if (int.TryParse(Argument("--preview-width"), out int captureWidth) &&
            int.TryParse(Argument("--preview-height"), out int captureHeight))
            Screen.SetResolution(Mathf.Clamp(captureWidth, 640, 3840),
                Mathf.Clamp(captureHeight, 360, 2160), FullScreenMode.Windowed);
        if (string.IsNullOrEmpty(capture)) yield break;
        yield return new WaitForSecondsRealtime(12);
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(capture);
        yield return new WaitForSecondsRealtime(1);
        Application.Quit();
    }
    System.Collections.IEnumerator RecordRangedPreview(string directory)
    {
        Directory.CreateDirectory(directory);
        Application.runInBackground = true;
        yield return new WaitForSecondsRealtime(2);
        bool compact = Environment.GetCommandLineArgs().Contains("--ranged-compact");
        Screen.SetResolution(compact ? 640 : 960, compact ? 360 : 540, FullScreenMode.Windowed);
        yield return new WaitForSecondsRealtime(1);
        var input = rangedArena.Cat.GetComponent<CatInput>();
        rangedArena.Retry();
        rangedArena.Cat.RespawnAt(new Vector2(-3.05f, .5f));
        StartCoroutine(HighThrow());
        System.Collections.IEnumerator HighThrow()
        {
            yield return new WaitForSeconds(1.6f);
            input.LobPressed();
            yield return new WaitForSeconds(.56f);
            input.LobReleased();
        }
        for (int frame = 0; frame < 100; frame++) {
            if (frame == 0) input.PulsePressed();
            if (frame == 36) { input.ReloadPressed(); }
            if (frame == 42) { input.LobPressed(); }
            if (frame >= 44 && frame <= 72 && frame % 2 == 0) input.PulsePressed();
            if (frame == 76) input.LobPressed();
            if (frame == 78) input.ReloadPressed();
            if (frame == 30 || frame == 60) rangedArena.Cat.PressJump();
            if (frame == 33 || frame == 63) rangedArena.Cat.Motion.ReleaseJump();
            if (frame == 32) rangedArena.Cat.Motion.SetMove(1);
            if (frame == 42) rangedArena.Cat.Motion.SetMove(0);
            if (frame == 56) { rangedArena.Cat.Motion.SetMove(-1); input.FacingChanged(-1); }
            if (frame == 62) { rangedArena.Cat.Motion.SetMove(0); input.FacingChanged(1); }
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(directory, frame.ToString("D3") + ".png"));
            yield return new WaitForSecondsRealtime(.1f);
        }
        yield return new WaitForSecondsRealtime(1);
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
    void OnApplicationFocus(bool focused) { if (!focused) { SaveVolumePreference(); PauseForBackground(); } }
    void OnApplicationPause(bool paused) { if (paused) { SaveVolumePreference(); PauseForBackground(); } }
    void PauseForBackground()
    {
        if (ActiveRun == null || ActiveRun.Paused) return;
        if (course != null) course.TogglePause();
        else if (rangedArena != null) rangedArena.TogglePause();
        else stompArena.TogglePause();
        // Focus returning leaves the pause menu open until the player resumes.
    }
    bool BackFromMenu()
    {
        if (ActiveRun != null || applying || state == "applying") return false;
        SaveVolumePreference();
        if (popup) popup = false;
        else if (page == "settings") page = "menu";
        else if (page == "sound-credits" || page == "updates") page = "settings";
        else return false;
        previousFocus.Clear(); revealFocus = false; lastPage = null;
        return true;
    }
    void OpenGameplaySettings()
    {
        if (ActiveRun == null || !ActiveRun.Paused) return;
        gameplaySettings = true;
        gameplayMenuVisible = false;
    }
    void CloseGameplaySettings()
    {
        SaveVolumePreference();
        gameplaySettings = false;
        gameplayMenuVisible = false;
    }
    void ToggleGameplayPause()
    {
        if (gameplaySettings && ActiveRun != null && ActiveRun.Paused) { CloseGameplaySettings(); return; }
        gameplaySettings = false;
        if (course != null) course.TogglePause();
        else if (rangedArena != null) rangedArena.TogglePause();
        else if (stompArena != null) stompArena.TogglePause();
    }
    void OnApplicationQuit() { SaveVolumePreference(); ObserveDisplay(true); platform?.Dispose(); }
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
