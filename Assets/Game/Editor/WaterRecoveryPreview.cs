using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class WaterRecoveryPreview
{
    [MenuItem("LegendOfCat/Water recovery/Prepare reaction texture")]
    public static void Prepare()
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath("Assets/Game/Resources/WaterRecovery/reactions.png");
        importer.textureType = TextureImporterType.Default;
        importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.maxTextureSize = 4096; importer.wrapMode = TextureWrapMode.Clamp;
        importer.SaveAndReimport();
    }

    [MenuItem("LegendOfCat/Water recovery/Preview first gap")]
    public static void Preview()
    {
        if (EditorApplication.isPlaying) {
            var course = Object.FindFirstObjectByType<MovementCourse>();
            if (course) course.Cat.RespawnAt(course.transform.TransformPoint(new Vector3(6, -1.8f)));
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Object.Instantiate(Resources.Load<GameObject>("MovementCourse"));
        EditorApplication.isPlaying = true;
        // Once Play starts, use this menu again to place Mika over the opening gap.
    }

    // Explicit batch authoring command; it never edits or saves a serialized scene.
    public static void Capture()
    {
        Prepare();
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var root = Object.Instantiate(Resources.Load<GameObject>("MovementCourse"));
        var course = root.GetComponent<MovementCourse>();
        Invoke(course, "Awake"); Invoke(course.Cat, "Awake");
        var scenery = root.GetComponentInChildren<BackgroundStudy>();
        var view = scenery.View;
        view.transform.position = new Vector3(6, BackgroundStudy.CameraFloor, -10);
        view.aspect = 16f / 9f;
        var water = course.Cat.GetComponent<WaterRecovery>();
        var body = course.Cat.GetComponent<Rigidbody2D>();
        body.position = new Vector2(6, -3);
        course.Cat.transform.position = body.position;
        Physics2D.SyncTransforms();
        water.TryEnter();
        var render = new RenderTexture(960, 540, 24);
        var pixels = new Texture2D(960, 540, TextureFormat.RGB24, false);
        view.targetTexture = render;
        string folder = Path.GetFullPath("Logs/water-review"); Directory.CreateDirectory(folder);
        float prior = 0;
        for (int frame = 0; frame <= 75; frame++) {
            float time = frame / 30f;
            water.Advance(time - prior); prior = time;
            if (water.CameraHeld) Invoke(water, "LateUpdate");
            else Invoke(view.GetComponent<CatCamera>(), "LateUpdate");
            scenery.ApplyView(view.transform.position.x, view.transform.position.y);
            view.Render(); RenderTexture.active = render;
            pixels.ReadPixels(new Rect(0, 0, 960, 540), 0, 0);
            // Camera.Render does not invoke IMGUI; reproduce the same fullscreen fade.
            var colors = pixels.GetPixels();
            for (int i = 0; i < colors.Length; i++) colors[i] *= 1 - water.Fade;
            pixels.SetPixels(colors); pixels.Apply();
            File.WriteAllBytes(Path.Combine(folder, $"water-{frame:000}.png"), pixels.EncodeToPNG());
        }
        RenderTexture.active = null; view.targetTexture = null;
        Object.DestroyImmediate(pixels); Object.DestroyImmediate(render);
        Debug.Log("Water review frames captured to " + folder);
    }

    static void Invoke(object target, string name) => target.GetType().GetMethod(name,
        BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, null);
}
