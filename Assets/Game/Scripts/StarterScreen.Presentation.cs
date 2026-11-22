using System.Collections.Generic;
using UnityEngine;

public sealed partial class StarterScreen
{
    GUIStyle titleStyle, headingStyle, textStyle, smallStyle, buttonStyle, panelStyle;
    GUIStyle runTimerStyle;
    GUIStyle volumeTrackStyle, volumeThumbStyle;
    Texture2D runClockIcon, runClockHand;
    Texture2D backdrop, panelTexture, buttonTexture, hoverTexture, focusTexture;
    Texture2D volumeTrackTexture;
    readonly List<string> focusable = new List<string>();
    readonly List<string> previousFocus = new List<string>();
    Vector2 popupScroll;
    bool wasPopup;
    float pageStarted;
    string lastPage;
    bool simulateCutout;
    float guiScale = 1;
    bool revealFocus;
    bool gameplayMenuVisible, gameplaySettings;
    Vector2 gameplayControlsScroll, gameplayMenuScroll;
    bool UpdatesSupported {
        get {
#if LEGEND_OF_CAT_PREVIEW
            if (previewUnsupported) return false;
#endif
            return Preview || (platform != null && platform.SupportsUpdates);
        }
    }
    static readonly Color Ink = new Color(0.94f, 0.91f, 0.80f);

    static Texture2D Solid(Color fill, Color edge)
    {
        var texture = new Texture2D(5, 5) { hideFlags = HideFlags.HideAndDontSave };
        for (int y = 0; y < 5; y++) for (int x = 0; x < 5; x++)
            texture.SetPixel(x, y, x == 0 || x == 4 || y == 0 || y == 4 ? edge : fill);
        texture.Apply();
        return texture;
    }

    void PrepareStyles()
    {
        if (titleStyle != null) return;
        backdrop = Resources.Load<Texture2D>("Menu/painted-valley");
        panelTexture = Solid(new Color(0.045f, 0.105f, 0.11f, 0.97f), new Color(0.52f, 0.50f, 0.32f));
        buttonTexture = Solid(new Color(0.13f, 0.23f, 0.22f), new Color(0.44f, 0.50f, 0.38f));
        hoverTexture = Solid(new Color(0.23f, 0.34f, 0.28f), new Color(0.88f, 0.73f, 0.42f));
        focusTexture = Solid(new Color(0.26f, 0.34f, 0.27f), new Color(1f, 0.86f, 0.52f));
        titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 40, fontStyle = FontStyle.Bold, wordWrap = true };
        headingStyle = new GUIStyle(titleStyle) { fontSize = 25 };
        textStyle = new GUIStyle(GUI.skin.label) { fontSize = 17, wordWrap = true };
        smallStyle = new GUIStyle(textStyle) { fontSize = 14 };
        foreach (var style in new[] { titleStyle, headingStyle, textStyle, smallStyle }) style.normal.textColor = Ink;
        buttonStyle = new GUIStyle(GUI.skin.button) {
            fontSize = 17, wordWrap = true, alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(18, 18, 10, 10), border = new RectOffset(2, 2, 2, 2),
            margin = new RectOffset(0, 0, 5, 5)
        };
        buttonStyle.normal.background = buttonTexture;
        buttonStyle.hover.background = hoverTexture;
        buttonStyle.focused.background = focusTexture;
        buttonStyle.active.background = focusTexture;
        buttonStyle.normal.textColor = buttonStyle.hover.textColor = buttonStyle.focused.textColor = buttonStyle.active.textColor = Ink;
        volumeTrackTexture = new Texture2D(5, 28) { hideFlags = HideFlags.HideAndDontSave };
        for (int y = 0; y < 28; y++) for (int x = 0; x < 5; x++)
            volumeTrackTexture.SetPixel(x, y, y < 9 || y > 18 ? Color.clear :
                buttonTexture.GetPixel(x == 0 || x == 4 || y == 9 || y == 18 ? 0 : 2, 2));
        volumeTrackTexture.Apply();
        volumeTrackStyle = new GUIStyle(GUI.skin.horizontalSlider) { fixedHeight = 28, border = new RectOffset(2, 2, 0, 0), padding = new RectOffset(), margin = new RectOffset(0, 0, 4, 8) };
        volumeTrackStyle.normal.background = volumeTrackTexture;
        volumeThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb) { fixedWidth = 24, fixedHeight = 28, border = new RectOffset(2, 2, 2, 2), margin = new RectOffset() };
        volumeThumbStyle.normal.background = hoverTexture;
        volumeThumbStyle.hover.background = volumeThumbStyle.active.background = volumeThumbStyle.focused.background = focusTexture;
        panelStyle = new GUIStyle { normal = { background = panelTexture }, border = new RectOffset(2, 2, 2, 2), padding = new RectOffset(24, 24, 20, 20) };
    }

    bool Button(string label, string control = null, bool fixedNavigation = false)
    {
        control = control ?? label;
        GUI.SetNextControlName(control);
        if (GUI.enabled) focusable.Add(control);
        bool keyboard = GUI.enabled && !Event.current.alt && GUI.GetNameOfFocusedControl() == control && Event.current.type == EventType.KeyDown &&
            (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter);
        if (keyboard) Event.current.Use();
        bool clicked = GUILayout.Button(label, buttonStyle, GUILayout.MinHeight(gameplayMenuVisible ? 48 : Mathf.Max(48, 48 / guiScale)));
        if (revealFocus && Event.current.type == EventType.Repaint && GUI.GetNameOfFocusedControl() == control) {
            var rect = GUILayoutUtility.GetLastRect();
            if (popup) popupScroll.y = Mathf.Max(0, rect.y - 80);
            else if (!fixedNavigation) scroll.y = Mathf.Max(0, rect.y - 80);
            revealFocus = false;
        }
        return clicked || keyboard;
    }
    void NavigateFocus()
    {
        var e = Event.current;
        if (e.type != EventType.KeyDown || previousFocus.Count == 0) return;
        if (e.keyCode != KeyCode.Tab && e.keyCode != KeyCode.DownArrow && e.keyCode != KeyCode.UpArrow) return;
        int index = previousFocus.IndexOf(GUI.GetNameOfFocusedControl());
        int direction = e.shift || e.keyCode == KeyCode.UpArrow ? -1 : 1;
        index = (index + direction + previousFocus.Count) % previousFocus.Count;
        GUI.FocusControl(previousFocus[index]);
        revealFocus = true;
        e.Use();
    }
    void Label(string text, GUIStyle style = null) => GUILayout.Label(text, style ?? textStyle);

    void OnGUI()
    {
        if (course != null || stompArena != null || rangedArena != null) {
            DrawRunTimer(ActiveRun.ElapsedSeconds);
            DrawGameplayMenu();
            return;
        }
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape && BackFromMenu()) {
            Event.current.Use(); GUI.FocusControl(null);
        }
        gameplayMenuVisible = false;
        PrepareStyles();
        var oldColor = GUI.color;
        var oldMatrix = GUI.matrix;
        GUI.color = Color.white;
        if (backdrop != null) GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), backdrop, ScaleMode.ScaleAndCrop);
        else { GUI.color = new Color(0.08f, 0.17f, 0.17f); GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture); }
        GUI.color = Color.white;
        var safe = Screen.safeArea;
#if LEGEND_OF_CAT_PREVIEW
        if (simulateCutout) safe = new Rect(50, 25, Mathf.Max(1, Screen.width - 100), Mathf.Max(1, Screen.height - 50));
#endif
        var region = MenuLayout.ContentRegion(Screen.width, Screen.height, safe);
        float scale = MenuLayout.Scale(region);
        guiScale = scale;
        GUI.matrix = Matrix4x4.TRS(new Vector3(region.x, region.y), Quaternion.identity, Vector3.one * scale);
        float width = region.width / scale, height = region.height / scale;
        float panelWidth = Mathf.Min(540, width - 24);
        var panel = new Rect(width > 1000 ? 48 : (width - panelWidth) / 2, 12, panelWidth, height - 24);
        if (lastPage != page) { lastPage = page; pageStarted = Time.unscaledTime; scroll = Vector2.zero; GUI.FocusControl(null); }
        if (Event.current.type == EventType.KeyDown && Event.current.alt &&
            (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)) Event.current.Use();
        NavigateFocus();
        focusable.Clear();
        GUILayout.BeginArea(panel, panelStyle);
        Label("LEGEND OF CAT", titleStyle);
        Label("A little cat. A wider world.", smallStyle);
        GUILayout.Space(16);
        GUI.enabled = !popup && !applying && state != "applying";
        // Fade page content only; controls are immediately interactive. No moving scenery.
        GUI.color = new Color(1, 1, 1, Mathf.Lerp(0.75f, 1, Mathf.Clamp01((Time.unscaledTime - pageStarted) / 0.16f)));
        if (page == "updates" && Button("Back to settings", fixedNavigation: true)) page = "settings";
        scroll = GUILayout.BeginScrollView(scroll);
        if (!string.IsNullOrEmpty(warning)) { GUILayout.Space(16); Label("Notice", headingStyle); Label(warning); }
        if (page == "menu") {
            Label("The adventure is taking shape.", headingStyle);
            Label("Explore the current build and make yourself at home. Mika's campaign is still in development.");
            GUILayout.Space(18);
            if (Button("Golden River preview")) OpenLevelOne();
            if (Button("Simple combat lab")) OpenStompArena();
            if (Button("Ranged combat lab")) OpenRangedArena();
            if (Button(string.IsNullOrEmpty(available) ? "Settings" : "Settings - Update available")) page = "settings";
            if (Button("Quit")) Application.Quit();
        } else if (page == "settings") {
            Label("Settings", headingStyle);
            Label("A few comforts for the journey.");
            DrawPlaySettings(false);
            GUI.enabled = false;
            Button("Language - Coming later");
            GUI.enabled = !popup;
            if (Button("Sound credits")) page = "sound-credits";
            if (Button(string.IsNullOrEmpty(available) ? "Versions & updates" : "Versions & updates - Available")) page = "updates";
            if (Button("Back to menu")) page = "menu";
        } else if (page == "sound-credits") {
            Label("Sound credits", headingStyle);
            Label("Checkpoint: Fantasy Achievement Unlock - Magical Stinger by TommasoMotteran. CC BY 4.0. Adapted: excerpt, softer entrance, bell-layer removal and extended shimmer.");
            if (Button("Checkpoint source and author")) Application.OpenURL("https://freesound.org/people/TommasoMotteran/sounds/859080/");
            if (Button("CC BY 4.0 license")) Application.OpenURL("https://creativecommons.org/licenses/by/4.0/");
            Label("River: Park ambiences by Thimras. CC0 1.0. Adapted into a river loop.");
            if (Button("River source")) Application.OpenURL("https://opengameart.org/content/park-ambiences");
            Label("Water entry: jump-into-water.flac by yurkobb. CC0 1.0. Shortened and made quieter.");
            if (Button("Water entry source")) Application.OpenURL("https://freesound.org/people/yurkobb/sounds/280219/");
            if (Button("CC0 1.0 dedication")) Application.OpenURL("https://creativecommons.org/publicdomain/zero/1.0/");
            Label("Wind: original continuous-air sound effects. PolyForm Noncommercial 1.0.0.");
            if (Button("Back to settings")) page = "settings";
        } else {
            Label("Versions & updates", headingStyle);
            Label(StatusText());
            if (state == "downloading") {
                var track = GUILayoutUtility.GetRect(1, 12, GUILayout.ExpandWidth(true));
                GUI.DrawTexture(track, buttonTexture);
                GUI.color = new Color(0.88f, 0.73f, 0.42f);
                GUI.DrawTexture(new Rect(track.x, track.y, track.width * Mathf.Clamp01(progress / 100f), track.height), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }
            if (!string.IsNullOrEmpty(error)) Label(error);
            bool busy = state == "checking" || state == "downloading" || state == "ready" || state == "applying";
            GUI.enabled = !popup && !busy && UpdatesSupported;
            if (Button(state == "error" ? "Retry" : "Check for updates")) Send("check");
            if (!string.IsNullOrEmpty(available) && Button("Update now - " + available)) Send("download");
            GUI.enabled = !popup;
            GUILayout.Space(16);
            Label("Release history", headingStyle);
            if (releases.Length == 0) Label("No release history loaded.", smallStyle);
            for (int index = 0; index < Mathf.Min(releases.Length, ReleaseHistory.MaxReleases); index++) {
                var release = releases[index];
                if (release == null) continue;
                GUILayout.Space(12);
                GUILayout.BeginVertical(GUI.skin.box);
                Label(ReleaseHistory.Inline(release.version ?? "Release"), headingStyle);
                if (release.summary == null) release.summary = ReleaseHistory.Parse(release.notes);
                foreach (var block in release.summary) {
                    // Conventional changelogs repeat the version/date above their section headings.
                    if (block.Heading && ReleaseHistory.IsVersionHeading(block.Text, release.version)) continue;
                    Label(block.Text, block.Heading ? headingStyle : textStyle);
                }
                if (Button("View full notes on GitHub", "release-" + index) && IsReleaseUrl(release.url)) Application.OpenURL(release.url);
                GUILayout.EndVertical();
            }
            if (releases.Length > ReleaseHistory.MaxReleases) Label("More releases are available on GitHub.", smallStyle);
            Label("Older releases are for reference. Downgrades are not supported.", smallStyle);
        }
#if LEGEND_OF_CAT_PREVIEW
        if (Preview) DrawPreviewTools();
#endif
        GUILayout.EndScrollView();
        GUI.color = Color.white;
        GUILayout.Space(12);
        Label("Version " + Application.version, smallStyle);
        if (Preview) Label("LOCAL PREVIEW - Simulated updates. Nothing is installed.", smallStyle);
        GUILayout.EndArea();
        GUI.enabled = true;
        if (popup) {
            focusable.Clear();
            GUI.color = new Color(0, 0, 0, 0.72f);
            GUI.DrawTexture(new Rect(0, 0, width, height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            float popupWidth = Mathf.Min(460, width - 24), popupHeight = Mathf.Min(350, height - 24);
            GUILayout.BeginArea(new Rect((width - popupWidth) / 2, (height - popupHeight) / 2, popupWidth, popupHeight), panelStyle);
            popupScroll = GUILayout.BeginScrollView(popupScroll);
            Label("A new chapter awaits", headingStyle);
            Label("Version " + available + " is available. Download now, then restart automatically to install. You can also leave it for later.");
            GUILayout.Space(12);
            if (Button("Update now")) Send("download");
            if (Button("Later")) popup = false;
            GUILayout.EndScrollView();
            GUILayout.EndArea();
            if (!wasPopup) GUI.FocusControl("Later");
        }
        wasPopup = popup;
        if (Event.current.type == EventType.Repaint) { previousFocus.Clear(); previousFocus.AddRange(focusable); }
        GUI.color = oldColor;
        GUI.matrix = oldMatrix;
    }

    void DrawRunTimer(float elapsedSeconds)
    {
        if (Event.current.type != EventType.Repaint) return;
        if (runTimerStyle == null) {
            runClockIcon = Resources.Load<Texture2D>("Menu/run-stopwatch");
            runClockHand = CreateRunClockHand();
            runTimerStyle = new GUIStyle(GUI.skin.label) {
                alignment = TextAnchor.MiddleRight, fontSize = 25, fontStyle = FontStyle.Bold,
                padding = new RectOffset()
            };
        }

        var region = MenuLayout.ContentRegion(Screen.width, Screen.height, Screen.safeArea);
        var viewport = MenuLayout.Letterbox16By9(Screen.width, Screen.height);
        float right = Mathf.Min(region.xMax, viewport.xMax * Screen.width);
        float top = Mathf.Max(region.y, (1 - viewport.yMax) * Screen.height);
        int seconds = Mathf.FloorToInt(elapsedSeconds);
        var label = new GUIContent($"{seconds / 60:00}:{seconds % 60:00}");
        float width = Mathf.Ceil(runTimerStyle.CalcSize(label).x);
        var digits = new Rect(right - width - 12, Mathf.Ceil(top) + 10, width, 36);
        const float clockSize = 36;
        if (runClockIcon != null) {
            var clock = new Rect(digits.x - clockSize - 4, digits.center.y - clockSize / 2, clockSize, clockSize);
            GUI.DrawTexture(clock, runClockIcon);

            // The painted dial sits below the winding crown within the texture.
            var pin = new Vector2(
                Mathf.Floor(clock.x + clock.width * 0.510f) + 0.5f,
                Mathf.Floor(clock.y + clock.height * 0.567f) + 0.5f);
            var previousColor = GUI.color;
            GUI.color = new Color(0.39f, 0.22f, 0.10f);
            for (int mark = 1; mark < 12; mark++) {
                if (mark % 3 == 0) continue;
                float angle = mark * Mathf.PI / 6f;
                var dot = pin + new Vector2(Mathf.Sin(angle), -Mathf.Cos(angle)) * 9f;
                GUI.DrawTexture(new Rect(Mathf.Floor(dot.x), Mathf.Floor(dot.y), 1f, 1f), Texture2D.whiteTexture);
            }
            GUI.color = previousColor;

            var matrix = GUI.matrix;
            // The decorative hand makes one full turn every twelve elapsed seconds.
            GUIUtility.RotateAroundPivot((seconds % 12) * 30f, pin);
            GUI.DrawTexture(new Rect(pin.x - 2.5f, pin.y - 9, 5, 9), runClockHand);
            GUI.matrix = matrix;
        }
        runTimerStyle.normal.textColor = new Color(0.13f, 0.09f, 0.17f);
        for (int y = -1; y <= 1; y++)
            for (int x = -1; x <= 1; x++)
                if (x != 0 || y != 0)
                    GUI.Label(new Rect(digits.x + x, digits.y + y, width, digits.height), label, runTimerStyle);
        runTimerStyle.normal.textColor = Ink;
        GUI.Label(digits, label, runTimerStyle);
    }

    static Texture2D CreateRunClockHand()
    {
        const int pixelsPerPoint = 8;
        const int width = 5 * pixelsPerPoint + 1;
        const int height = 9 * pixelsPerPoint;
        var hand = new Texture2D(width, height, TextureFormat.RGBA32, false) {
            hideFlags = HideFlags.HideAndDontSave,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };
        var pixels = new Color32[width * height];
        for (int y = 0; y < height; y++) {
            float distanceFromPin = (y + 0.5f) / pixelsPerPoint;
            // Use a narrow stem at the hub, then flare into the blade.
            float shoulder = Mathf.Lerp(0.4f, 1.1f, Mathf.InverseLerp(2f, 4f, distanceFromPin));
            float halfWidth = Mathf.Min(shoulder, (8.2f - distanceFromPin) * 0.3f);
            for (int x = 0; x < width; x++) {
                float distanceFromCenter = Mathf.Abs(x - width / 2) * (5f / width);
                float coverage = Mathf.Clamp01(0.5f + halfWidth - distanceFromCenter);
                pixels[y * width + x] = new Color32(75, 42, 23, (byte)Mathf.RoundToInt(255 * coverage));
            }
        }
        hand.SetPixels32(pixels);
        hand.Apply(false, true);
        return hand;
    }

    void DrawPlaySettings(bool inGame)
    {
        Label("Sound volume: " + Mathf.RoundToInt(MasterVolume * 100) + "%", smallStyle);
        GUI.enabled = inGame || !popup;
        GUI.SetNextControlName("Sound volume");
        if (GUI.enabled) focusable.Add("Sound volume");
        SetMasterVolume(GUILayout.HorizontalSlider(MasterVolume, 0, 1, volumeTrackStyle, volumeThumbStyle));
        if (display != null) {
            GUILayout.Space(12);
            Label("Display", headingStyle);
            GUI.enabled = (inGame || !popup) && !display.Switching;
            if (Button(fullscreen ? "Fullscreen: On" : "Fullscreen: Off")) SetFullscreen(!fullscreen);
            GUI.enabled = inGame || !popup;
            if (!inGame) Label("Resize the window by dragging its borders. F11 or Alt+Enter toggles fullscreen.", smallStyle);
        }
        if ((!inGame || course != null) && Button(ShowLevelProgress ? "Level progress bar: On" : "Level progress bar: Off"))
            SetShowLevelProgress(!ShowLevelProgress);
    }

    void DrawGameplayMenu()
    {
        bool levelFinished = course != null && course.LevelOne && course.HasReachedExit;
        bool finished = levelFinished || (rangedArena != null ? rangedArena.Combat.Finished : stompArena != null && stompArena.Combat.Finished);
        bool paused = ActiveRun?.Paused ?? false;
        if (!paused && !finished) {
            gameplaySettings = false;
            if (gameplayMenuVisible) {
                previousFocus.Clear();
                GUI.FocusControl(null);
                revealFocus = false;
                gameplayMenuVisible = false;
            }
            return;
        }

        PrepareStyles();
        var region = MenuLayout.ContentRegion(Screen.width, Screen.height, Screen.safeArea);
        var oldMatrix = GUI.matrix;
        var oldColor = GUI.color;
        int oldDepth = GUI.depth;
        // Keep pause controls above the water-recovery blackout.
        GUI.depth = -2000;
        guiScale = 1;
        GUI.matrix = Matrix4x4.TRS(new Vector3(region.x, region.y), Quaternion.identity, Vector3.one);
        GUI.color = Color.white;
        float panelWidth = Mathf.Max(1, Mathf.Min(900, region.width - 24));
        float panelHeight = Mathf.Max(1, Mathf.Min(620, region.height - 24));
        bool wide = panelWidth >= 600;
        if (!gameplayMenuVisible) {
            previousFocus.Clear(); GUI.FocusControl(null); revealFocus = false;
            gameplayMenuVisible = true;
            gameplayMenuScroll = gameplayControlsScroll = Vector2.zero;
        }
        NavigateFocus(); focusable.Clear();
        GUILayout.BeginArea(new Rect((region.width - panelWidth) / 2, (region.height - panelHeight) / 2, panelWidth, panelHeight), panelStyle);
        bool resume = false, retry = false, restart = false, menu = false, settings = false, back = false;
        if (gameplaySettings) {
            Label("Settings", headingStyle);
            gameplayMenuScroll = GUILayout.BeginScrollView(gameplayMenuScroll);
            DrawPlaySettings(true);
            GUILayout.EndScrollView();
            back = Button("Back to pause", fixedNavigation: true);
        } else {
            Label(levelFinished ? "Golden River complete" : finished ?
                (rangedArena != null || stompArena.Combat.BossHealth == 0 ? "Target defeated" : "Try again") : "Paused", headingStyle);
            if (levelFinished) Label("You made it through the valley.", smallStyle);
            GUILayout.Space(8);
            if (wide) { GUILayout.BeginHorizontal(); GUILayout.BeginVertical(GUILayout.Width(220)); }
            else gameplayMenuScroll = GUILayout.BeginScrollView(gameplayMenuScroll);
            resume = !finished && Button("Resume", fixedNavigation: true);
            retry = Button("Retry", fixedNavigation: true);
            restart = Button("Restart", fixedNavigation: true);
            settings = !finished && Button("Settings", fixedNavigation: true);
            menu = Button("Main menu", fixedNavigation: true);
            if (wide) {
                GUILayout.EndVertical(); GUILayout.Space(20); GUILayout.BeginVertical();
                Label("Controls", textStyle);
                gameplayControlsScroll = GUILayout.BeginScrollView(gameplayControlsScroll);
            } else { GUILayout.Space(16); Label("Controls", textStyle); }
            DrawGameplayControls();
            if (wide) { GUILayout.EndScrollView(); GUILayout.EndVertical(); GUILayout.EndHorizontal(); }
            else GUILayout.EndScrollView();
        }
        GUILayout.EndArea();
        if (Event.current.type == EventType.Repaint) {
            previousFocus.Clear();
            previousFocus.AddRange(focusable);
        }
        GUI.matrix = oldMatrix;
        GUI.color = oldColor;
        GUI.depth = oldDepth;

        if (back) { CloseGameplaySettings(); return; }
        if (settings) { OpenGameplaySettings(); return; }
        if (course != null) {
            if (resume) course.TogglePause();
            else if (retry) course.RetryCheckpoint();
            else if (restart) course.RestartRun();
            else if (menu) CloseCourse();
        } else if (rangedArena != null) {
            if (resume) rangedArena.TogglePause();
            else if (retry) rangedArena.Retry();
            else if (restart) rangedArena.Restart();
            else if (menu) CloseRangedArena();
        } else {
            if (resume) stompArena.TogglePause();
            else if (retry) stompArena.Retry();
            else if (restart) stompArena.Restart();
            else if (menu) CloseStompArena();
        }
    }

    void DrawGameplayControls()
    {
        Label("Keyboard / mouse", textStyle);
        if (rangedArena != null)
            Label("J / Left mouse: paw pulse\nK / Right mouse: hold to charge the light orb, release to throw\nR: reload the last-used attack\nUsing the other attack interrupts reload.", smallStyle);
        Label("Move: A/D or Left/Right arrows\nJump: Space, W or Up arrow\nPause / resume: Esc or P\nMain menu: choose Main menu while paused\nRestart run: Backspace", smallStyle);
        Label(rangedArena != null ? "Retry: choose Retry in this menu" : "Retry checkpoint: R", smallStyle);
        GUILayout.Space(12);
        Label("Controller", textStyle);
        if (rangedArena != null)
            Label("West button: paw pulse\nRT: hold / release light orb\nNorth button: reload last-used attack", smallStyle);
        Label("Move: left stick or D-pad\nJump: South button\nPause / resume: RB\nMain menu: Start\nRestart run: LB", smallStyle);
        if (rangedArena == null) Label("Retry checkpoint: Select", smallStyle);
    }

#if LEGEND_OF_CAT_PREVIEW
    bool previewTools;
    void DrawPreviewTools()
    {
        GUI.enabled = !popup;
        GUILayout.Space(20);
        if (Button(previewTools ? "Hide preview tools" : "Show preview tools")) previewTools = !previewTools;
        if (!previewTools) return;
        Label("Preview tools", headingStyle);
        if (Button("Capture view matrix")) StartCoroutine(CapturePreview(System.IO.Path.Combine(Application.persistentDataPath, "menu-preview-captures")));
        if (Button("Show update pop-up")) { previewUnsupported = false; popup = true; available = "0.1.0 (preview example)"; }
        if (Button("Simulate download error")) PreviewState("error", "Simulated download failure. Retry or choose Later.");
        if (Button("Show checking")) PreviewState("checking");
        if (Button("Show no update")) PreviewState("current");
        if (Button("Show ready to apply")) PreviewState("ready");
        if (Button("Show download progress")) { PreviewState("downloading"); progress = 47; downloadedBytes = 47000000; totalBytes = 100000000; }
        if (Button("Show warning")) { warning = "Preview warning: settings could not be saved. Existing settings are preserved."; page = "updates"; }
        if (Button("Show unsupported platform")) { state = "development"; available = ""; page = "updates"; previewUnsupported = true; }
        if (Button("Toggle display cutout")) simulateCutout = !simulateCutout;
    }
    bool previewUnsupported;
    void PreviewState(string value, string detail = "")
    {
        previewUnsupported = false; popup = false; page = "updates"; state = value; error = detail;
        if (value == "current") available = "";
    }
#endif
    void OnDestroy()
    {
        if (course != null) { course.gameObject.SetActive(false); Destroy(course.gameObject); }
        if (stompArena != null) { stompArena.gameObject.SetActive(false); Destroy(stompArena.gameObject); }
        if (rangedArena != null) { rangedArena.gameObject.SetActive(false); Destroy(rangedArena.gameObject); }
        foreach (var texture in new[] { panelTexture, buttonTexture, hoverTexture, focusTexture, runClockHand })
            if (texture != null) Destroy(texture);
    }
}
