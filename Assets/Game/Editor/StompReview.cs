using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class StompReview
{
    // Run only in the isolated review copy: a real free-aspect Game View includes IMGUI.
    public static void OpenInteractive()
    {
        if (!Application.dataPath.Replace('\\', '/').Contains("/artifacts/stomp-review-project/"))
            throw new System.InvalidOperationException("Use the isolated stomp review project.");
        UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene);
        SessionState.SetBool("StompInteractive", true);
        Install();
        EditorApplication.EnterPlaymode();
    }
    static EditorWindow reviewWindow;
    static double nextCapture;
    static int windowCapture;
    static bool capturePending;
    static int windowFrame;
    static readonly Vector2Int[] reviewSizes = { new Vector2Int(640, 360), new Vector2Int(480, 640), new Vector2Int(320, 240) };
    static void CaptureWindow()
    {
        if (!EditorApplication.isPlaying || !reviewWindow) { EditorApplication.update -= CaptureWindow; return; }
        if (EditorApplication.timeSinceStartup < nextCapture) return;
        if (capturePending) {
            string directory = "docs/design/stomp-prototype/window-review/window-" + windowCapture;
            Directory.CreateDirectory(directory);
            ScreenCapture.CaptureScreenshot(directory + "/" + windowFrame.ToString("D3") + ".png");
            if (windowFrame == 0) Debug.Log("Stomp live Game View capture: " + Screen.width + "x" + Screen.height);
            capturePending = ++windowFrame < 40;
            nextCapture = EditorApplication.timeSinceStartup + (capturePending ? .1 : 1);
            return;
        }
        if (windowCapture == reviewSizes.Length) { EditorApplication.update -= CaptureWindow; return; }
        var size = reviewSizes[windowCapture++];
        reviewWindow.position = new Rect(100, 100, size.x, size.y + 22);
        reviewWindow.Focus();
        capturePending = true;
        windowFrame = 0;
        nextCapture = EditorApplication.timeSinceStartup + 2;
    }
    public static void ConfigureDamageAudio()
    {
        var importer = (AudioImporter)AssetImporter.GetAtPath("Assets/Game/Resources/StompDamageVoice.wav");
        var settings = importer.defaultSampleSettings;
        settings.loadType = AudioClipLoadType.DecompressOnLoad;
        settings.compressionFormat = AudioCompressionFormat.PCM;
        settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
        settings.preloadAudioData = true;
        importer.defaultSampleSettings = settings;
        importer.loadInBackground = false;
        importer.SaveAndReimport();
    }

    // Uses the actual runtime arena, shared character and state-driven presentation.
    public static void Record()
    {
        SessionState.SetBool("StompReview", true);
        Install();
        EditorApplication.EnterPlaymode();
    }
    [InitializeOnLoadMethod]
    static void Install()
    {
        EditorApplication.playModeStateChanged -= StateChanged;
        EditorApplication.playModeStateChanged += StateChanged;
    }
    static void StateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("StompInteractive", false)) {
            SessionState.SetBool("StompInteractive", false);
            new GameObject("Interactive stomp review").AddComponent<StompArena>();
            EditorApplication.update += OpenReviewWindow;
            return;
        }
        if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool("StompReview", false)) return;
        SessionState.SetBool("StompReview", false);
        int result = 0;
        try { Capture(); }
        catch (System.Exception error) { Debug.LogException(error); result = 1; }
        if (Application.isBatchMode) EditorApplication.Exit(result);
        else EditorApplication.ExitPlaymode();
    }
    static void OpenReviewWindow()
    {
        EditorApplication.update -= OpenReviewWindow;
        var gameView = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
        foreach (var existing in Resources.FindObjectsOfTypeAll(gameView)) ((EditorWindow)existing).Close();
        reviewWindow = ScriptableObject.CreateInstance(gameView) as EditorWindow;
        reviewWindow.ShowUtility();
        gameView.GetProperty("selectedSizeIndex", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)?.SetValue(reviewWindow, 0);
        windowCapture = 0; nextCapture = 0;
        EditorApplication.update += CaptureWindow;
    }
    static void Capture()
    {
        var scene = SceneManager.CreateScene("Stomp review", new CreateSceneParameters(LocalPhysicsMode.Physics2D));
        SceneManager.SetActiveScene(scene);
        var root = new GameObject("Stomp review arena");
        var arena = root.AddComponent<StompArena>();

        var target = new RenderTexture(960, 540, 24);
        var pixels = new Texture2D(960, 540, TextureFormat.RGB24, false);
        var previous = RenderTexture.active;
        Directory.CreateDirectory("docs/design/stomp-prototype");
        try {
            arena.ArenaCamera.targetTexture = target;
            foreach (var phase in new[] { "anticipation", "attack", "recovery" }) {
                root.SendMessage("Present");
                arena.ArenaCamera.Render();
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, 960, 540), 0, 0); pixels.Apply();
                File.WriteAllBytes("docs/design/stomp-prototype/" + phase + ".png", pixels.EncodeToPNG());
                arena.Combat.Advance(phase == "anticipation" ? StompCombat.Anticipation : StompCombat.Active);
            }
            var scenarios = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-stompReviewHitsOnly") >= 0
                ? new[] { "moving-head", "player-hit", "enemy-hit", "enemy-final", "player-final" }
                : new[] { "left", "right", "closed-head", "ground", "moving-head", "run-right", "run-left", "signals", "player-hit", "enemy-hit", "enemy-final", "player-final" };
            foreach (var scenario in scenarios) {
                arena.Retry();
                arena.Cat.GetComponent<CatInput>().enabled = false;
                var body = arena.Cat.GetComponent<Rigidbody2D>();
                body.interpolation = RigidbodyInterpolation2D.None;
                if (scenario == "enemy-final") {
                    arena.Combat.Advance(1.56f); arena.Combat.TryStomp(true);
                    arena.Combat.Advance(1.56f); arena.Combat.TryStomp(true);
                }
                if (scenario == "moving-head" || scenario == "enemy-hit" || scenario == "enemy-final") arena.Combat.Advance(1.56f);
                if (scenario == "player-final") {
                    arena.Combat.TryDamage(); arena.Combat.Advance(1.26f);
                    arena.Combat.TryDamage(); arena.Combat.Advance(1.26f);
                    arena.Combat.Advance(1f);
                }
                body.position = (scenario == "enemy-hit" || scenario == "enemy-final" || scenario == "player-hit" || scenario == "player-final")
                    ? new Vector2(arena.Combat.TargetX, 1.7f) : scenario == "signals" ? new Vector2(-5, .5f) : scenario == "moving-head" ? new Vector2(arena.Combat.TargetX, 3.4f) : (scenario == "right" || scenario == "run-left") ? new Vector2(1.1f, .5f)
                    : scenario == "closed-head" ? new Vector2(-.2f, 1.7f)
                    : scenario == "ground" ? new Vector2(-2, .5f) : new Vector2(-1.1f, .5f);
                if (scenario == "run-right" || scenario == "run-left") {
                    float direction = scenario == "run-right" ? 1 : -1;
                    body.linearVelocity = new Vector2(direction * 7, 0);
                    arena.Cat.Motion.SetMove(direction);
                }
                if (scenario == "ground") {
                    arena.Combat.Advance(StompCombat.Anticipation);
                    body.position += Vector2.right * arena.Combat.TargetX;
                }
                arena.Cat.transform.position = body.position;
                var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                typeof(StompArena).GetField("previous", flags).SetValue(arena, scenario == "closed-head" ? new Vector2(-.2f, 2)
                    : (scenario == "enemy-hit" || scenario == "enemy-final" || scenario == "player-hit" || scenario == "player-final") ? new Vector2(body.position.x, 2) : body.position);
                typeof(StompArena).GetField("previousTargetX", flags).SetValue(arena, arena.Combat.TargetX);
                Physics2D.SyncTransforms();
                var motorStep = typeof(CatController).GetMethod("FixedUpdate", flags);
                var combatStep = typeof(StompArena).GetMethod("FixedUpdate", flags);
                var presentation = arena.Cat.GetComponentInChildren<MikaSpritePresentation>();
                string frames = "Logs/stomp-damage-frames/" + scenario;
                Directory.CreateDirectory(frames);
                var csv = new System.Text.StringBuilder("frame,time,x,y,targetX,bossHealth,health,protection,alpha,velocityX,velocityY,burstTime,contactX,contactY,enemyRingTime,enemyHeadFlashTime,enemyRingX,enemyHeadX,audioPlaying\n");
                for (int frame = 0; frame < (scenario == "signals" ? 180 : scenario == "moving-head" ? 150 : 80); frame++) {
                    if (frame > 0) {
                        motorStep.Invoke(arena.Cat, null);
                        scene.GetPhysicsScene2D().Simulate(Time.fixedDeltaTime);
                    }
                    combatStep.Invoke(arena, null);
                    presentation.RenderPose(Time.fixedDeltaTime);
                    arena.ArenaCamera.Render();
                    RenderTexture.active = target;
                    pixels.ReadPixels(new Rect(0, 0, 960, 540), 0, 0); pixels.Apply();
                    File.WriteAllBytes(frames + "/" + frame.ToString("D3") + ".png", pixels.EncodeToPNG());
                    csv.AppendLine(System.FormattableString.Invariant($"{frame},{frame * Time.fixedDeltaTime},{body.position.x},{body.position.y},{arena.Combat.TargetX},{arena.Combat.BossHealth},{arena.Combat.PlayerHealth},{arena.Combat.Invulnerability},{presentation.GetComponent<SpriteRenderer>().color.a},{body.linearVelocity.x},{body.linearVelocity.y},{typeof(StompArena).GetField("burstTime", flags).GetValue(arena)},{((Transform)typeof(StompArena).GetField("damageBurst", flags).GetValue(arena)).localPosition.x},{((Transform)typeof(StompArena).GetField("damageBurst", flags).GetValue(arena)).localPosition.y},{typeof(StompArena).GetField("flash", flags).GetValue(arena)},{typeof(StompArena).GetField("enemyHeadFlashTime", flags).GetValue(arena)},{((MeshRenderer)typeof(StompArena).GetField("impact", flags).GetValue(arena)).transform.localPosition.x},{((SpriteRenderer)typeof(StompArena).GetField("enemyHeadFlash", flags).GetValue(arena)).transform.localPosition.x},{arena.Cat.GetComponent<AudioSource>().isPlaying}"));
                }
                File.WriteAllText("docs/design/stomp-prototype/damage-" + scenario + ".csv", csv.ToString());
            }
        } finally {
            RenderTexture.active = previous;
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(pixels); Object.DestroyImmediate(target);
        }
        AssetDatabase.SaveAssets();
    }
}
