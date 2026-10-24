using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MikaCharacterPreview
{
    [MenuItem("LegendOfCat/Mika/Create isolated character preview")]
    public static void Create()
    {
        CreateOriginal();
        if (Application.isBatchMode) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        new GameObject("Mika original movement course").AddComponent<MovementCourse>();
    }

    [MenuItem("LegendOfCat/Mika/Prepare original four-frame prototype")]
    public static void CreateOriginal()
    {
        const string source = "Assets/Game/Art/Mika/mika-movement-candidate.png";
        const string folder = "Assets/Game/Art/Mika/Original";
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Game/Art/Mika", "Original");
        var importer = (TextureImporter)AssetImporter.GetAtPath(source);
        importer.isReadable = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        var atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(source);
        float width = atlas.width / 4f, height = atlas.height / 4f;
        var frames = new Sprite[MikaAnimation.FrameCount];
        for (int i = 0; i < 17; i++) {
            int sourceIndex = i == 16 ? 6 : i;
            var rect = new Rect(sourceIndex % 4 * width, (3 - sourceIndex / 4) * height, width, height);
            var pivot = ContactPivot(atlas, rect);
            if (i == 10 || i == 11 || i == 16) {
                // Match the shoulder anchor across complete airborne drawings.
                // Registration only: never scale or deform the cat at runtime.
                float shoulderX = i == 10 ? 220 : i == 11 ? 255 : 245;
                float shoulderY = i == 10 ? 145 : i == 11 ? 155 : 140;
                pivot = new Vector2((shoulderX - 56) / width, (height - shoulderY - 110) / height);
            }
            var sprite = Sprite.Create(atlas, rect, pivot, width / 1.8f, 0, SpriteMeshType.FullRect);
            sprite.name = $"original-frame-{i:00}";
            string path = $"{folder}/{sprite.name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing) { EditorUtility.CopySerialized(sprite, existing); Object.DestroyImmediate(sprite); }
            else { AssetDatabase.CreateAsset(sprite, path); existing = sprite; }
            frames[i] = existing;
        }
        for (int i = 17; i < frames.Length; i++) {
            string path = $"Assets/Game/Art/Mika/Transitions/jump-transition-{i:00}.png";
            var transitionImporter = (TextureImporter)AssetImporter.GetAtPath(path);
            transitionImporter.textureType = TextureImporterType.Sprite;
            transitionImporter.spriteImportMode = SpriteImportMode.Single;
            transitionImporter.alphaIsTransparency = true;
            transitionImporter.mipmapEnabled = false;
            transitionImporter.isReadable = true;
            transitionImporter.textureCompression = TextureImporterCompression.Uncompressed;
            transitionImporter.spritePixelsPerUnit = width / 1.8f;
            transitionImporter.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var settings = new TextureImporterSettings();
            transitionImporter.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = i == 20 ? ContactPivot(texture, new Rect(0,0,texture.width,texture.height))
                : new Vector2(0.5f,18f / texture.height);
            transitionImporter.SetTextureSettings(settings);
            transitionImporter.SaveAndReimport();
            frames[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        var visual = new GameObject("Mika original four-frame animation");
        var renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = frames[0]; renderer.sortingOrder = 3;
        var presentation = visual.AddComponent<MikaSpritePresentation>();
        presentation.frames = frames;
        PrefabUtility.SaveAsPrefabAsset(visual, "Assets/Game/Resources/MikaOriginal.prefab");
        Object.DestroyImmediate(visual);
        AssetDatabase.SaveAssets();
    }

    public static Vector2 ContactPivot(Texture2D atlas, Rect rect)
    {
        // Stray opaque pixels below the cat must not become the sole anchor.
        // Use the largest connected silhouette; do not change the drawing itself.
        int width = (int)rect.width, height = (int)rect.height;
        var solid = new bool[width * height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                solid[y * width + x] = atlas.GetPixel((int)rect.x + x, (int)rect.y + y).a > 0.65f;
        var queue = new System.Collections.Generic.Queue<int>();
        int largest = 0, sole = 0;
        for (int start = 0; start < solid.Length; start++) {
            if (!solid[start]) continue;
            solid[start] = false; queue.Enqueue(start);
            int count = 0, lowest = height;
            while (queue.Count > 0) {
                int pixel = queue.Dequeue(), x = pixel % width, y = pixel / width;
                count++; lowest = Mathf.Min(lowest, y);
                Visit(x > 0 ? pixel - 1 : -1);
                Visit(x + 1 < width ? pixel + 1 : -1);
                Visit(y > 0 ? pixel - width : -1);
                Visit(y + 1 < height ? pixel + width : -1);
            }
            if (count > largest) { largest = count; sole = lowest; }
        }
        if (largest < 20) throw new System.InvalidOperationException("Mika frame has no connected character silhouette.");
        return new Vector2(0.5f, (sole + 0.5f) / rect.height);

        void Visit(int pixel)
        {
            if (pixel < 0 || !solid[pixel]) return;
            solid[pixel] = false; queue.Enqueue(pixel);
        }
    }
}
