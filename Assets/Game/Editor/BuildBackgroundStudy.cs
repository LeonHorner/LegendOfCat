using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildBackgroundStudy
{
    const string ScenePath = "Assets/Game/Scenes/BackgroundStudy.unity";
    const string Evidence = "docs/design/background-study";

    // Resources materials retain shader references in player builds; Shader.Find alone does not.
    [MenuItem("LegendOfCat/Background study/Prepare runtime materials")]
    public static void PrepareRuntimeMaterials()
    {
        foreach (var pair in new[] {
            new[] { "background-material", "LegendOfCat/BackgroundStudy" },
            new[] { "terrain-material", "LegendOfCat/StudyTerrain" },
            new[] { "water-material", "LegendOfCat/StudyWater" }
        }) {
            string path = "Assets/Game/Resources/BackgroundStudy/" + pair[0] + ".mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(path)) continue;
            var shader = Shader.Find(pair[1]);
            if (!shader) throw new System.InvalidOperationException("Missing scenery shader: " + pair[1]);
            AssetDatabase.CreateAsset(new Material(shader), path);
        }
        AssetDatabase.SaveAssets();
    }

    [MenuItem("LegendOfCat/Background study/Record checkpoint prototype")]
    public static void RecordCheckpointPrototype()
    {
        SessionState.SetBool("GoldenRiverCheckpointReview", true);
        InstallPrototypeReview();
        EditorApplication.EnterPlaymode();
    }

    [InitializeOnLoadMethod]
    static void InstallPrototypeReview()
    {
        EditorApplication.playModeStateChanged -= PrototypeReviewState;
        EditorApplication.playModeStateChanged += PrototypeReviewState;
    }

    static void PrototypeReviewState(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool("GoldenRiverCheckpointReview", false)) return;
        SessionState.SetBool("GoldenRiverCheckpointReview", false);
        int exitCode = 0;
        try { CaptureCheckpointPrototype(); }
        catch (System.Exception error) { Debug.LogException(error); exitCode = 1; }
        if (Application.isBatchMode) EditorApplication.Exit(exitCode);
        else EditorApplication.ExitPlaymode();
    }

    static void CaptureCheckpointPrototype()
    {
        const string output = "Logs/checkpoint-scenery-review";
        Directory.CreateDirectory(output);
        var previous = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        var scene = UnityEngine.SceneManagement.SceneManager.CreateScene("Checkpoint scenery review",
            new UnityEngine.SceneManagement.CreateSceneParameters(UnityEngine.SceneManagement.LocalPhysicsMode.Physics2D));
        UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
        var root = Object.Instantiate(Resources.Load<GameObject>("MovementCourse"));
        var course = root.GetComponent<MovementCourse>();
        var cat = course.Cat;
        var body = cat.GetComponent<Rigidbody2D>();
        body.interpolation = RigidbodyInterpolation2D.None;
        cat.GetComponent<CatInput>().enabled = false;
        var scenery = root.GetComponentInChildren<BackgroundStudy>();
        var follow = scenery.View.GetComponent<CatCamera>();
        var presentation = cat.GetComponentInChildren<MikaSpritePresentation>();
        var csv = new System.Text.StringBuilder("frame,checkpoint,retries,paused,x,y\n");
        try {
            for (int frame = 0; frame < 200; frame++) {
                // Scripted review fixtures reach both existing checkpoints without changing the route.
                if (frame == 25) cat.RespawnAt(new Vector2(9, 0.52f));
                if (frame == 120) cat.RespawnAt(new Vector2(34, 1.52f));
                if (frame == 60 || frame == 145) body.position = new Vector2(19, -8);
                if (frame == 90) course.TogglePause();
                if (frame == 100) course.RetryCheckpoint();
                if (frame == 175) course.RestartRun();
                cat.Motion.SetMove(frame >= 35 && frame < 55 ? 1 : 0);
                for (int step = 0; step < 2; step++) {
                    cat.SendMessage("FixedUpdate");
                    scene.GetPhysicsScene2D().Simulate(Time.fixedDeltaTime);
                    scenery.SendMessage("FixedUpdate");
                }
                course.SendMessage("Update");
                presentation.RenderPose(1f / 25);
                follow.SendMessage("LateUpdate");
                scenery.SendMessage("LateUpdate");
                scenery.SetWaterTime(frame / 25f);
                Save(scenery.View, $"{output}/frame-{frame:000}.png", 960, 540, false);
                if (frame == 15 || frame == 45 || frame == 75 || frame == 135 || frame == 160 || frame == 190)
                    Save(scenery.View, $"{output}/checkpoint-{frame:000}.png", 1280, 720, false);
                csv.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,
                    "{0},{1},{2},{3},{4:F3},{5:F3}\n", frame, course.Run.Checkpoint, course.Run.Retries,
                    course.Run.Paused, body.position.x, body.position.y);
            }
            File.WriteAllText(output + "/playback.csv", csv.ToString());
        } finally {
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(previous);
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
        }
    }

    [MenuItem("LegendOfCat/Background study/Create isolated scene")]
    public static void Create()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        ImportLayers();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        new GameObject("Golden River background study").AddComponent<BackgroundStudy>();
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
    }

    [MenuItem("LegendOfCat/Background study/Import painted layers")]
    public static void ImportLayers()
    {
        foreach (string name in new[] { "painted-atlas", "foreground-river", "recessed-valley-v4", "foreground-corners", "painted-pillar-v2", "painted-bank-v2", "painted-shore-v2", "painted-shelf-v3", "distant-canopy-v3" }) {
            var importer = (TextureImporter)AssetImporter.GetAtPath($"Assets/Game/Resources/BackgroundStudy/{name}.png");
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = name == "meadow-surface" || name == "painted-terrain";
            importer.isReadable = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp; importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }
    }

    [MenuItem("LegendOfCat/Background study/Capture comparison")]
    public static void Capture()
    {
        bool stillsOnly = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-studyStills") >= 0;
        Directory.CreateDirectory(Evidence);
        const string Motion = "Logs/background-study-motion";
        Directory.CreateDirectory(Motion);
        ImportLayers();
        var scene = EditorSceneManager.NewPreviewScene();
        var root = new GameObject("Evidence only");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, scene);
        var study = root.AddComponent<BackgroundStudy>(); study.Initialize(); study.showControls = false;
        study.View.scene = scene;
        try {
            study.Cat.transform.position = new Vector3(15, 1.48f);
            var catSprite = study.Cat.GetComponentInChildren<SpriteRenderer>();
            catSprite.transform.position += Vector3.up * (1f - catSprite.transform.position.y);
            study.SetWaterTime(0);
            study.ApplyView(17, BackgroundStudy.CameraFloor);
            Save(study.View, $"{Evidence}/subtle-distance.png", 1280, 720, false);
            Save(study.View, $"{Evidence}/subtle-distance-small-grayscale.png", 640, 360, true);
            study.ApplyView(10, BackgroundStudy.CameraFloor);
            Save(study.View, $"{Evidence}/subtle-ground.png", 1280, 720, false);
            study.ApplyView(83, BackgroundStudy.CameraFloor);
            Save(study.View, $"{Evidence}/subtle-extended.png", 1280, 720, false);
            study.ApplyView(17, 3);
            Save(study.View, $"{Evidence}/subtle-jump-stress.png", 640, 360, true);
            foreach (float x in new[] { -120f, 57f, 59f, 240f }) {
                study.ApplyView(x, BackgroundStudy.CameraFloor);
                Save(study.View, $"{Evidence}/subtle-wrap-{x}.png", 640, 360, false);
            }
            // 25 fps evidence: fixed-camera water first, then the opening route and reversal.
            for (int frame = 0; !stillsOnly && frame < 150; frame++) {
                study.ApplyView(17, BackgroundStudy.CameraFloor); study.SetWaterTime(frame / 25f);
                Save(study.View, $"{Motion}/water-review-{frame:000}.png", 640, 360, false);
            }
            var water = System.Array.Find(study.GetComponentsInChildren<MeshRenderer>(),
                r => r.name.StartsWith("Foreground water tile")).sharedMaterial;
            for (int frame = 0; !stillsOnly && frame < 150; frame++) {
                study.ApplyView(17, BackgroundStudy.CameraFloor); study.SetWaterTime(frame / 25f);
                // Isolated full-running water preview; the motor-driven transition is covered in Play tests.
                water.SetFloat("_RunAmount", 1); water.SetFloat("_RunTimeOffset", frame / 25f * 1.2f);
                Save(study.View, $"{Motion}/water-running-review-{frame:000}.png", 640, 360, false);
            }
            water.SetFloat("_RunAmount", 0); water.SetFloat("_RunTimeOffset", 0);
            for (int frame = 0; !stillsOnly && frame <= 280; frame++) {
                study.ApplyView(6 + Mathf.PingPong(frame * 0.2f, 28), BackgroundStudy.CameraFloor);
                study.SetWaterTime(frame / 25f);
                Save(study.View, $"{Motion}/subtle-review-{frame:000}.png", 640, 360, false);
            }
        } finally { EditorSceneManager.ClosePreviewScene(scene); }
        AssetDatabase.Refresh();
    }

    static void Save(Camera camera, string path, int width, int height, bool grayscale)
    {
        var target = new RenderTexture(width, height, 24);
        var pixels = new Texture2D(width, height, TextureFormat.RGB24, false);
        var previous = RenderTexture.active;
        try {
            camera.targetTexture = target; camera.rect = new Rect(0, 0, 1, 1); camera.aspect = 16f / 9f; camera.Render();
            RenderTexture.active = target; pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0); pixels.Apply();
            if (grayscale) {
                var colors = pixels.GetPixels();
                for (int i = 0; i < colors.Length; i++) {
                    float y = colors[i].r * 0.2126f + colors[i].g * 0.7152f + colors[i].b * 0.0722f;
                    colors[i] = new Color(y, y, y);
                }
                pixels.SetPixels(colors); pixels.Apply();
            }
            File.WriteAllBytes(path, pixels.EncodeToPNG());
        } finally {
            camera.targetTexture = null; RenderTexture.active = previous; target.Release();
            Object.DestroyImmediate(target); Object.DestroyImmediate(pixels);
        }
    }
}
