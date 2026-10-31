using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class StompReview
{
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
        if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool("StompReview", false)) return;
        SessionState.SetBool("StompReview", false);
        int result = 0;
        try { Capture(); }
        catch (System.Exception error) { Debug.LogException(error); result = 1; }
        if (Application.isBatchMode) EditorApplication.Exit(result);
        else EditorApplication.ExitPlaymode();
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
            foreach (var scenario in new[] { "left", "right", "closed-head", "ground", "moving-head", "run-right", "run-left", "signals" }) {
                arena.Retry();
                arena.Cat.GetComponent<CatInput>().enabled = false;
                var body = arena.Cat.GetComponent<Rigidbody2D>();
                body.interpolation = RigidbodyInterpolation2D.None;
                if (scenario == "moving-head") arena.Combat.Advance(1.56f);
                body.position = scenario == "signals" ? new Vector2(-5, .5f) : scenario == "moving-head" ? new Vector2(arena.Combat.TargetX, 3.4f) : (scenario == "right" || scenario == "run-left") ? new Vector2(1.1f, .5f)
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
                typeof(StompArena).GetField("previous", flags).SetValue(arena, scenario == "closed-head" ? new Vector2(-.2f, 2) : body.position);
                typeof(StompArena).GetField("previousTargetX", flags).SetValue(arena, arena.Combat.TargetX);
                var motorStep = typeof(CatController).GetMethod("FixedUpdate", flags);
                var combatStep = typeof(StompArena).GetMethod("FixedUpdate", flags);
                var presentation = arena.Cat.GetComponentInChildren<MikaSpritePresentation>();
                string frames = "Logs/stomp-damage-frames/" + scenario;
                Directory.CreateDirectory(frames);
                var csv = new System.Text.StringBuilder("frame,time,x,y,targetX,bossHealth,health,protection,alpha,velocityX,velocityY,burstTime,contactX,contactY\n");
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
                    csv.AppendLine(System.FormattableString.Invariant($"{frame},{frame * Time.fixedDeltaTime},{body.position.x},{body.position.y},{arena.Combat.TargetX},{arena.Combat.BossHealth},{arena.Combat.PlayerHealth},{arena.Combat.Invulnerability},{presentation.GetComponent<SpriteRenderer>().color.a},{body.linearVelocity.x},{body.linearVelocity.y},{typeof(StompArena).GetField("burstTime", flags).GetValue(arena)},{((Transform)typeof(StompArena).GetField("damageBurst", flags).GetValue(arena)).localPosition.x},{((Transform)typeof(StompArena).GetField("damageBurst", flags).GetValue(arena)).localPosition.y}"));
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
