using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Dedicated authoring scene: deliberately independent of MovementCourse and shared prefabs.
public static class MikaCharacterPreview
{
    public const string AtlasPath = "Assets/Game/Art/Mika/mika-movement-smooth.png";
    const string PrefabPath = "Assets/Game/Art/Mika/MikaCandidate.prefab";

    [MenuItem("LegendOfCat/Mika/Capture candidate contact sheet")]
    public static void Capture()
    {
        // A temporary preview scene keeps the user's scene and the shared course untouched.
        var scene = EditorSceneManager.NewPreviewScene();
        var root = new GameObject("Mika contact sheet");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, scene);
        var camera = new GameObject("Contact sheet camera").AddComponent<Camera>();
        camera.transform.SetParent(root.transform, false);
        camera.transform.position = new Vector3(0, 0, -10);
        camera.orthographic = true;
        camera.orthographicSize = 3.4f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.10f, 0.16f, 0.20f);
        camera.scene = scene;
        for (int i = 0; i < MikaAnimation.FrameCount; i++) {
            var item = new GameObject($"Frame {i:00}").AddComponent<SpriteRenderer>();
            item.transform.SetParent(root.transform, false);
            item.transform.localPosition = new Vector3((i % 6 - 2.5f) * 1.55f, (1.5f - i / 6) * 1.55f - 0.6f);
            item.transform.localScale = Vector3.one * 0.8f;
            item.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Game/Art/Mika/candidate-frame-{i:00}.asset");
        }
        var target = new RenderTexture(1440, 1000, 24);
        var pixels = new Texture2D(1440, 1000, TextureFormat.RGB24, false);
        var previous = RenderTexture.active;
        try {
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            pixels.ReadPixels(new Rect(0, 0, 1440, 1000), 0, 0);
            pixels.Apply();
            System.IO.File.WriteAllBytes("docs/design/mika-animation/candidate-preview.png", pixels.EncodeToPNG());
        }
        finally {
            RenderTexture.active = previous;
            camera.targetTexture = null;
            target.Release();
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(pixels);
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }

    [MenuItem("LegendOfCat/Mika/Create isolated character preview")]
    public static void Create()
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(AtlasPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 4096;
        importer.isReadable = true;
        importer.SaveAndReimport();
        var atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(AtlasPath);
        float width = atlas.width / 8f;
        float height = atlas.height / 10f;
        var frames = new Sprite[MikaAnimation.FrameCount];
        for (int i = 0; i < frames.Length; i++) {
            string path = $"Assets/Game/Art/Mika/candidate-frame-{i:00}.asset";
            int row = i / 8;
            var rect = new Rect(i % 8 * width, (9 - row) * height, width, height);
            var pivot = ContactPivot(atlas, rect);
            // Airborne pivots retain a stable torso anchor instead of pulling tucked paws down.
            if (i >= 14 && i <= 19 || i >= 48 && i <= 65) pivot.y = 37.5f / height;
            var sprite = Sprite.Create(atlas, rect, pivot, 256f / 1.8f, 0, SpriteMeshType.FullRect);
            sprite.name = $"candidate-frame-{i:00}";
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing) { EditorUtility.CopySerialized(sprite, existing); Object.DestroyImmediate(sprite); }
            else { AssetDatabase.CreateAsset(sprite, path); existing = sprite; }
            frames[i] = existing;
        }
        var cat = new GameObject("Mika candidate - visual review pending");
        cat.AddComponent<Rigidbody2D>();
        cat.AddComponent<BoxCollider2D>().size = new Vector2(0.9f, 0.9f);
        var controller = cat.AddComponent<CatController>();
        cat.AddComponent<CatInput>().controller = controller;
        var visual = new GameObject("Cosmetic sprites");
        visual.transform.SetParent(cat.transform, false);
        visual.transform.localPosition = new Vector3(0, -0.45f);
        visual.AddComponent<SpriteRenderer>().sprite = frames[0];
        var presentation = visual.AddComponent<MikaSpritePresentation>();
        presentation.controller = controller;
        presentation.frames = frames;
        PrefabUtility.SaveAsPrefabAsset(cat, PrefabPath);
        Object.DestroyImmediate(cat);
        AssetDatabase.SaveAssets();
        // Do not replace the user's open scene or commit a shared scene modification.
        if (Application.isBatchMode) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
        instance.transform.position = new Vector3(0, 0.45f);
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Preview floor";
        Object.DestroyImmediate(floor.GetComponent<BoxCollider>());
        floor.transform.position = new Vector3(0, -0.5f, 1);
        floor.transform.localScale = new Vector3(24, 1, 1);
        floor.AddComponent<BoxCollider2D>();
        var camera = new GameObject("Preview camera").AddComponent<Camera>();
        camera.transform.position = new Vector3(0, 1.8f, -10);
        camera.orthographic = true;
        camera.orthographicSize = 3;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.12f, 0.18f, 0.22f);
        Debug.Log("Mika candidate preview: play, use movement controls. Artwork and foot contacts require review before integration.");
    }

    public static Vector2 ContactPivot(Texture2D atlas, Rect rect)
    {
        // Ignore faint transparent RGB/background pixels; require a real horizontal sole span.
        for (int y = 0; y < (int)rect.height; y++) {
            int solid = 0;
            for (int x = 0; x < (int)rect.width; x++)
                if (atlas.GetPixel((int)rect.x + x, (int)rect.y + y).a > 0.65f) solid++;
            if (solid >= 5) return new Vector2(0.5f, (y + 0.5f) / rect.height);
        }
        throw new System.InvalidOperationException("Mika frame has no visible paw contact pixels.");
    }
}
