using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildBackgroundStudy
{
    const string ScenePath = "Assets/Game/Scenes/BackgroundStudy.unity";
    const string Evidence = "docs/design/background-study";

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
