using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MikaRigPreview
{
    const string Folder = "Assets/Game/Art/Mika/";
    public const string PrefabPath = Folder + "MikaRigCandidate.prefab";

    // Isolated local physics playback: does not replace or simulate the user's open scene.
    [MenuItem("LegendOfCat/Mika/Record articulated movement review")]
    public static void RecordReview()
    {
        if (Application.isBatchMode) EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        SessionState.SetBool("MikaRecordPending",true);
        InstallRecordingCallback();
        if (EditorApplication.isPlaying) RecordingStateChanged(PlayModeStateChange.EnteredPlayMode);
        else EditorApplication.EnterPlaymode();
    }

    [InitializeOnLoadMethod]
    static void InstallRecordingCallback()
    {
        EditorApplication.playModeStateChanged -= RecordingStateChanged;
        EditorApplication.playModeStateChanged += RecordingStateChanged;
    }

    static void RecordingStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool("MikaRecordPending",false)) return;
        SessionState.SetBool("MikaRecordPending",false);
        try { RecordRuntime(); }
        catch (System.Exception error) {
            Debug.LogException(error);
            if (Application.isBatchMode) EditorApplication.Exit(1);
            return;
        }
        if (Application.isBatchMode) EditorApplication.Exit(0);
        else EditorApplication.ExitPlaymode();
    }

    static void RecordRuntime()
    {
        if (!AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath)) {
            Debug.LogError("Create the articulated preview before recording.");
            return;
        }
        var scene = SceneManager.CreateScene("Mika isolated motion recording", new CreateSceneParameters(LocalPhysicsMode.Physics2D));
        var cat = Populate(scene);
        var controller = cat.GetComponent<CatController>();
        cat.GetComponent<Rigidbody2D>().interpolation = RigidbodyInterpolation2D.None;
        var input = cat.GetComponent<CatInput>();
        input.enabled = false;
        var rig = cat.GetComponentInChildren<MikaRigPresentation>();
        var camera = System.Array.Find(scene.GetRootGameObjects(), go => go.GetComponent<Camera>()).GetComponent<Camera>();
        var physics = scene.GetPhysicsScene2D();
        var target = new RenderTexture(960,540,24);
        var pixels = new Texture2D(960,540,TextureFormat.RGB24,false);
        var oldTarget = RenderTexture.active;
        const string output = "Logs/mika-rig-recording";
        System.IO.Directory.CreateDirectory(output);
        float oldFixed = Time.fixedDeltaTime;
        float maxAnkleError = 0, maxSoleError = 0;
        try {
            Time.fixedDeltaTime = 1f/60;
            camera.targetTexture = target;
            for (int frame = 0; frame < 360; frame++) {
                float t = frame/60f;
                controller.Motion.SetMove(t < 0.6f || t > 4.7f ? 0 : t < 2.3f ? 1 : -1);
                if (frame == 84 || frame == 204) controller.Motion.PressJump();
                if (frame == 108 || frame == 213) controller.Motion.ReleaseJump();
                controller.SendMessage("FixedUpdate");
                physics.Simulate(1f/60);
                rig.RenderPose(1f/60);
                maxAnkleError = Mathf.Max(maxAnkleError,rig.MaximumAnkleError);
                if (controller.Grounded)
                    for (int leg = 0; leg < 4; leg++)
                        if (rig.Gait.Supporting[leg])
                            maxSoleError = Mathf.Max(maxSoleError,Mathf.Abs(rig.PawSole(leg).y));
                camera.transform.position = new Vector3(cat.transform.position.x,
                    1.25f+Mathf.Max(0,cat.transform.position.y-0.46f)*0.8f,-10);
                camera.Render();
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0,0,960,540),0,0);
                pixels.Apply();
                System.IO.File.WriteAllBytes($"{output}/frame-{frame:0000}.png", pixels.EncodeToPNG());
                if (frame == 30) System.IO.File.WriteAllBytes("docs/design/mika-animation/rig-standing.png",pixels.EncodeToPNG());
                if (frame == 72) System.IO.File.WriteAllBytes("docs/design/mika-animation/rig-running.png",pixels.EncodeToPNG());
            }
        }
        finally {
            Time.fixedDeltaTime = oldFixed;
            RenderTexture.active = oldTarget;
            camera.targetTexture = null;
            target.Release(); Object.DestroyImmediate(target); Object.DestroyImmediate(pixels);
            SceneManager.UnloadSceneAsync(scene);
        }
        Debug.Log("Recorded 360 frames of isolated controller/physics playback at 60 Hz.");
        Debug.Log($"Mika motion audit: max ankle reach error={maxAnkleError:F5} m; grounded sole error={maxSoleError:F5} m.");
    }

    static Sprite[] ImportParts()
    {
        var parts = new Sprite[8];
        for (int i = 0; i < 8; i++) {
            string path = Folder + $"rig-part-{i:00}.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.isReadable = true;
            importer.maxTextureSize = 1024;
            importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Vector2 pivot;
            float ppu;
            if (i == 0) { pivot = new Vector2(240f / 610, 61f / 395); ppu = 500; }
            else if (i == 1) { pivot = new Vector2(314f / 370, 67f / 360); ppu = 500; }
            else if (i <= 5) {
                pivot = new Vector2(0.5f, 224f / 256);
                ppu = 192 / (i == 2 ? 0.27f : i == 3 ? 0.26f : i == 4 ? 0.30f : 0.28f);
            }
            else {
                pivot = MikaCharacterPreview.ContactPivot(texture, new Rect(0, 0, texture.width, texture.height));
                pivot.x = (i == 6 ? 57f : 63f) / texture.width;
                float ankle = texture.height - (i == 6 ? 738 - 680 : 734 - 675);
                ppu = (ankle - pivot.y * texture.height) / 0.065f;
            }
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = pivot;
            importer.SetTextureSettings(settings);
            importer.spritePixelsPerUnit = ppu;
            importer.SaveAndReimport();
            parts[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        return parts;
    }

    public static void Create() => CreateScene();

    public static void Play()
    {
        if (CreateScene()) EditorApplication.EnterPlaymode();
    }

    static bool CreateScene()
    {
        var parts = ImportParts();
        var cat = new GameObject("Mika rig candidate");
        cat.AddComponent<Rigidbody2D>();
        cat.AddComponent<BoxCollider2D>().size = new Vector2(0.9f, 0.9f);
        var controller = cat.AddComponent<CatController>();
        cat.AddComponent<CatInput>().controller = controller;
        var visual = new GameObject("Articulated visual");
        visual.transform.SetParent(cat.transform, false);
        var rig = visual.AddComponent<MikaRigPresentation>();
        rig.controller = controller;
        rig.parts = parts;
        PrefabUtility.SaveAsPrefabAsset(cat, PrefabPath);
        Object.DestroyImmediate(cat);
        AssetDatabase.SaveAssets();
        if (Application.isBatchMode) return true;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Populate(SceneManager.GetActiveScene());
        Debug.Log("Mika articulated preview ready. Play: A/D, jump, R retry. Four planted paws at rest.");
        return true;
    }

    public static GameObject Populate(Scene scene)
    {
        var cat = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), scene);
        cat.transform.position = new Vector3(0, 0.46f);
        var floor = new GameObject("Preview floor");
        SceneManager.MoveGameObjectToScene(floor, scene);
        floor.transform.position = new Vector3(0, -0.5f, 1);
        floor.transform.localScale = new Vector3(24,1,1);
        floor.AddComponent<BoxCollider2D>().size = Vector2.one;
        var render = floor.AddComponent<SpriteRenderer>();
        render.color = new Color(0.38f,0.43f,0.46f);
        var camera = new GameObject("Preview camera").AddComponent<Camera>();
        SceneManager.MoveGameObjectToScene(camera.gameObject, scene);
        camera.transform.position = new Vector3(0,1.3f,-10);
        camera.orthographic = true;
        camera.orthographicSize = 2.3f;
        camera.backgroundColor = new Color(0.10f,0.16f,0.20f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.scene = scene;
        var follow = camera.gameObject.AddComponent<MikaPreviewCamera>();
        follow.target = cat.GetComponent<CatController>();
        follow.previewFloor = render;
        if (Application.isPlaying) follow.EnsureFloor();
        return cat;
    }
}
