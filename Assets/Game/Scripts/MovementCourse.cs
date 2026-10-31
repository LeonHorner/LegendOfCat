using UnityEngine;

// A painted checkpoint prototype, reusable from the menu without campaign/save dependencies.
public sealed class MovementCourse : MonoBehaviour
{
    public static readonly Vector3[] Platforms = {
        new Vector3(0, -0.75f, 10), new Vector3(9, -0.75f, 4),
        new Vector3(15, 0.25f, 4), new Vector3(22.5f, 0.25f, 3),
        new Vector3(28.5f, 1.25f, 3), new Vector3(34, 0.25f, 4),
        new Vector3(42, -0.75f, 10), new Vector3(52, -0.75f, 4),
        new Vector3(58.5f, 0.25f, 3), new Vector3(65, 0.25f, 4),
        new Vector3(73, -0.75f, 6), new Vector3(83, -0.75f, 8),
        new Vector3(92, 0.25f, 4), new Vector3(100, -0.75f, 8)
    };
    public CatController Cat { get; private set; }
    public RunLifecycle Run { get; private set; }
    public static readonly Vector2[] CheckpointSpawns = {
        new Vector2(9, 0.52f), new Vector2(34, 1.52f),
        new Vector2(65, 1.52f), new Vector2(83, 0.52f)
    };
    public System.Action BackToMenu;
    Sprite square;
    Texture2D texture;
    Camera courseCamera;
    Transform visual;
    Transform[] feet = new Transform[4];
    float stride, facing = 1;
    string previousPose;
    float landing;
    bool originalMika;
    bool showHelp;
    GUIStyle hudStyle;
    string hudStatus;
    float hudRefresh, frameSeconds;

    void Awake()
    {
        texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white); texture.Apply();
        square = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1);
        foreach (var p in Platforms) {
            var platform = Block("Standable platform", new Vector2(p.x, p.y), new Vector2(p.z, 1.5f), new Color(0.22f, 0.28f, 0.28f));
            platform.AddComponent<BoxCollider2D>();
            platform.GetComponent<SpriteRenderer>().enabled = false;
        }
        var player = new GameObject("Mika prototype");
        player.transform.SetParent(transform, false);
        player.transform.localPosition = new Vector3(-3, 0.48f);
        player.AddComponent<Rigidbody2D>();
        player.AddComponent<BoxCollider2D>().size = new Vector2(0.9f, 0.9f);
        Cat = player.AddComponent<CatController>();
        Cat.fallReference = transform;
        // Session spawn coordinates belong to this course, not its current world placement.
        Run = new RunLifecycle(player.transform.localPosition);
        Cat.FellOutOfBounds += RetryCheckpoint;
        var input = player.AddComponent<CatInput>();
        input.controller = Cat;
        input.BackToMenu = () => BackToMenu?.Invoke();
        input.RetryCheckpoint = RetryCheckpoint;
        input.RestartRun = RestartRun;
        input.TogglePause = TogglePause;
        visual = new GameObject("Temporary quadruped silhouette").transform;
        visual.SetParent(player.transform, false);
        var original = Resources.Load<GameObject>("MikaOriginal");
        if (original) {
            Destroy(visual.gameObject);
            visual = Instantiate(original, player.transform).transform;
            visual.GetComponent<MikaSpritePresentation>().controller = Cat;
            originalMika = true;
        }
        if (!originalMika) {
        Part("Body", new Vector2(-0.04f, 0.03f), new Vector2(0.94f, 0.4f), new Color(0.95f, 0.53f, 0.2f));
        Part("Chest", new Vector2(0.3f, 0), new Vector2(0.22f, 0.38f), new Color(1, 0.86f, 0.62f));
        Part("Head", new Vector2(0.4f, 0.25f), new Vector2(0.35f, 0.32f), new Color(0.95f, 0.53f, 0.2f));
        Part("Ear", new Vector2(0.32f, 0.46f), new Vector2(0.13f, 0.18f), new Color(0.95f, 0.53f, 0.2f));
        Part("Eye", new Vector2(0.5f, 0.29f), new Vector2(0.045f, 0.055f), new Color(0.1f, 0.12f, 0.12f));
        var tail = Part("Tail", new Vector2(-0.65f, 0.18f), new Vector2(0.45f, 0.12f), new Color(0.8f, 0.36f, 0.12f));
        tail.localRotation = Quaternion.Euler(0, 0, -30);
        for (int i = 0; i < 4; i++) feet[i] = Part("Leg", new Vector2(-0.32f + i * 0.2f, -0.27f), new Vector2(0.1f, 0.32f), new Color(1, 0.79f, 0.5f));
        }
        courseCamera = new GameObject("Course camera").AddComponent<Camera>();
        courseCamera.transform.SetParent(transform, false);
        courseCamera.orthographic = true;
        courseCamera.orthographicSize = 5.5f;
        courseCamera.clearFlags = CameraClearFlags.SolidColor;
        courseCamera.backgroundColor = new Color(0.09f, 0.13f, 0.18f);
        courseCamera.depth = 1;
        // A lower camera clears the whole surface; the gameplay camera only clears its viewport.
        var bars = new GameObject("Viewport bars").AddComponent<Camera>();
        bars.transform.SetParent(transform, false);
        bars.depth = 0.5f;
        bars.clearFlags = CameraClearFlags.SolidColor;
        bars.backgroundColor = Color.black;
        bars.cullingMask = 0;
        var follow = courseCamera.gameObject.AddComponent<CatCamera>();
        follow.target = player.transform;
        follow.courseRoot = transform;
        follow.fixedHeight = true;
        var last = Platforms[Platforms.Length - 1];
        follow.rightBoundary = last.x + last.z * 0.5f + 1;
        Cat.ReturnedToStart += follow.Snap;
        var scenery = new GameObject("Golden River scenery");
        scenery.transform.SetParent(transform, false);
        scenery.AddComponent<BackgroundStudy>().InitializeForCourse(courseCamera, Cat, Platforms);
        for (int i = 0; i < CheckpointSpawns.Length; i++) {
            var point = CheckpointSpawns[i];
            Block("Safe checkpoint " + (i + 1), new Vector2(point.x, point.y + 0.45f),
                new Vector2(0.14f, 0.9f), new Color(0.95f, 0.75f, 0.36f));
            Label("CHECKPOINT " + (i + 1), new Vector2(point.x, point.y + 1.2f));
        }
        Label("FINISH / return or retry", new Vector2(100, 1));
    }
    GameObject Block(string name, Vector2 position, Vector2 size, Color color)
    {
        var item = new GameObject(name);
        item.transform.SetParent(transform, false);
        item.transform.localPosition = position;
        item.transform.localScale = size;
        var renderer = item.AddComponent<SpriteRenderer>();
        renderer.sprite = square; renderer.color = color;
        return item;
    }
    Transform Part(string name, Vector2 position, Vector2 size, Color color)
    {
        var part = Block(name, Vector2.zero, size, color).transform;
        part.SetParent(visual, false); part.localPosition = position;
        part.GetComponent<SpriteRenderer>().sortingOrder = 3;
        return part;
    }
    void Label(string text, Vector2 position)
    {
        var label = new GameObject(text).AddComponent<TextMesh>();
        label.transform.SetParent(transform, false); label.transform.localPosition = position;
        label.text = text; label.characterSize = 0.13f; label.fontSize = 32;
        label.anchor = TextAnchor.MiddleCenter; label.color = new Color(0.85f, 0.87f, 0.85f);
    }
    void Update()
    {
        if (Run == null) return;
        frameSeconds = Mathf.Lerp(frameSeconds, Time.unscaledDeltaTime, 0.08f);
        hudRefresh -= Time.unscaledDeltaTime;
        if (hudRefresh <= 0) {
            hudStatus = (Run.Paused ? "Paused | " : "") + "Checkpoint " + Run.Checkpoint + "/" + CheckpointSpawns.Length
                + " | R: retry | P: pause";
            hudRefresh = 0.25f;
        }
        Run.Advance(Time.deltaTime);
        if (Run.Paused || !Cat.Grounded) return;
        Vector2 position = transform.InverseTransformPoint(Cat.GetComponent<Rigidbody2D>().position);
        for (int i = 0; i < CheckpointSpawns.Length; i++) {
            Vector2 point = CheckpointSpawns[i];
            // Marker/spawn follow the course transform when reused in another scene.
            // Only grounded main-route positions may activate a checkpoint.
            if (Mathf.Abs(position.x - point.x) <= 0.7f && Mathf.Abs(position.y - point.y) <= 0.2f)
                Run.TryActivateCheckpoint(i + 1, point, true);
        }
    }

    public void RetryCheckpoint() => Cat.RespawnAt(transform.TransformPoint(Run.Retry()));
    public void RestartRun() => Cat.RespawnAt(transform.TransformPoint(Run.Restart()));
    public void TogglePause() => Cat.SetPaused(Run.TogglePause());

    void LateUpdate()
    {
        if (courseCamera != null) {
            courseCamera.rect = MenuLayout.Letterbox16By9(Screen.width, Screen.height);
            courseCamera.aspect = 16f / 9f;
        }
        var velocity = Cat.Motion.Velocity;
        if (originalMika) return;
        if (Mathf.Abs(velocity.x) > 0.1f) facing = Mathf.Sign(velocity.x);
        visual.localScale = new Vector3(facing, 1, 1);
        stride += Mathf.Abs(velocity.x) * Time.deltaTime * 2;
        if (previousPose == "Fall" && Cat.Grounded) landing = 0.1f;
        landing = Mathf.Max(0, landing - Time.deltaTime);
        for (int i = 0; i < feet.Length; i++) {
            float angle = Cat.Grounded ? Mathf.Sin(stride * 6 + i * Mathf.PI) * Mathf.Min(Mathf.Abs(velocity.x) * 5, 28)
                : (velocity.y > 0 ? -25 : 20);
            feet[i].localRotation = Quaternion.Euler(0, 0, angle);
        }
        previousPose = Cat.Pose;
    }
    void OnGUI()
    {
        if (Run == null || !courseCamera) return;
        if (hudStyle == null) hudStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, wordWrap = true };
        var viewport = courseCamera.rect;
        float width = Mathf.Min(350, Screen.width * viewport.width - 20);
        bool expanded = showHelp || Run.Paused;
        var area = new Rect(Screen.width * viewport.x + 10,
            Screen.height * (1 - viewport.yMax) + 10, width, expanded ? 176 : 52);
        var oldColor = GUI.color;
        GUI.color = new Color(1, 1, 1, 0.4f);
        GUI.Box(area, GUIContent.none);
        GUI.color = oldColor;
        GUILayout.BeginArea(new Rect(area.x + 6, area.y + 3, area.width - 12, area.height - 6));
        GUILayout.Label(hudStatus ?? "Checkpoint 0/4 | R: retry | P: pause", hudStyle);
        GUILayout.BeginHorizontal();
        GUILayout.Label("A/D: move | Space: jump | Esc: menu", hudStyle);
        if (GUILayout.Button(showHelp ? "Less" : "Controls", GUILayout.Width(62), GUILayout.Height(19))) {
            showHelp = !showHelp; GUI.FocusControl(null);
        }
        GUILayout.EndHorizontal();
        if (expanded) {
            GUILayout.Label("Arrows/W/Up also work. Backspace: restart entire run. Cross gold markers on safe ground.", hudStyle);
            GUILayout.Label("Controller: South jump, Select retry, LB restart, RB pause, Start menu.", hudStyle);
            GUILayout.Label("Retries " + Run.Retries + " | " + Run.ElapsedSeconds.ToString("F1")
                + "s | " + (1 / Mathf.Max(frameSeconds, 0.001f)).ToString("F0") + " fps", hudStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Retry")) { RetryCheckpoint(); GUI.FocusControl(null); }
            if (GUILayout.Button("Restart")) { RestartRun(); GUI.FocusControl(null); }
            if (GUILayout.Button(Run.Paused ? "Resume" : "Pause")) { TogglePause(); GUI.FocusControl(null); }
            if (GUILayout.Button("Menu")) { BackToMenu?.Invoke(); GUI.FocusControl(null); }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndArea();
    }
    void OnDestroy() { Destroy(square); Destroy(texture); }
}
