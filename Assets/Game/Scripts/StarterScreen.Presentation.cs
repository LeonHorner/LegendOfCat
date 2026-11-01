using System.Collections.Generic;
using UnityEngine;

public sealed partial class StarterScreen
{
    GUIStyle titleStyle, headingStyle, textStyle, smallStyle, buttonStyle, panelStyle;
    Texture2D backdrop, panelTexture, buttonTexture, hoverTexture, focusTexture;
    readonly List<string> focusable = new List<string>();
    readonly List<string> previousFocus = new List<string>();
    Vector2 popupScroll;
    bool wasPopup;
    float pageStarted;
    string lastPage;
    bool simulateCutout;
    float guiScale = 1;
    bool revealFocus;
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
        bool clicked = GUILayout.Button(label, buttonStyle, GUILayout.MinHeight(Mathf.Max(48, 48 / guiScale)));
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
        if (course != null || stompArena != null) return;
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
            if (Button("Movement lab")) page = "play";
            if (Button("Simple combat lab")) OpenStompArena();
            if (Button(string.IsNullOrEmpty(available) ? "Settings" : "Settings - Update available")) page = "settings";
            if (Button("Quit")) Application.Quit();
        } else if (page == "play") {
            Label("Movement lab prototype", headingStyle);
            Label("Run and jump through a longer painted river course with four checkpoints. Mika's campaign is still in development; this is a movement prototype.");
            GUILayout.Space(16);
            if (Button("Enter movement lab")) OpenCourse();
            if (Button("Back to menu")) page = "menu";
        } else if (page == "settings") {
            Label("Settings", headingStyle);
            Label("A few comforts for the journey.");
            GUI.enabled = false;
            Button("Language - Coming later");
            GUI.enabled = !popup;
            if (display != null) {
                GUILayout.Space(12);
                Label("Display", headingStyle);
                GUI.enabled = !popup && !display.Switching;
                if (Button(fullscreen ? "Fullscreen: On" : "Fullscreen: Off"))
                    SetFullscreen(!fullscreen);
                GUI.enabled = !popup;
                Label("Resize the window by dragging its borders. F11 or Alt+Enter toggles fullscreen.", smallStyle);
            }
            if (Button(string.IsNullOrEmpty(available) ? "Versions & updates" : "Versions & updates - Available")) page = "updates";
            if (Button("Back to menu")) page = "menu";
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
        foreach (var texture in new[] { panelTexture, buttonTexture, hoverTexture, focusTexture })
            if (texture != null) Destroy(texture);
    }
}
