using System;
using System.Linq;
using System.Collections;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class BackgroundStudyTests
{
    Type Study => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("BackgroundStudy")).First(t => t != null);
    float Call(string method, params object[] args) => (float)Study.GetMethod(method).Invoke(null, args);

    [TestCase(0f)] [TestCase(0.08f)] [TestCase(0.28f)] [TestCase(0.55f)] [TestCase(1f)] [TestCase(1.1f)]
    public void MirroredTilesCoverViewAcrossNegativeAndPositiveWraps(float speed)
    {
        // Both the sky and cutout tile families cover the fixed 16:9 viewport.
        foreach (float width in new[] { 16f, 40f, 48f, 64f })
        for (int step = -2000; step <= 2000; step++) {
            float camera = step * 0.5f;
            float center = Call("Offset", camera, speed) + Call("TileCenter", camera, speed, width);
            Assert.That(center - width * 1.5f, Is.LessThan(camera - 10));
            Assert.That(center + width * 1.5f, Is.GreaterThan(camera + 10));
        }
    }

    [Test] public void ReversalAndTeleportPreserveAbsoluteRegistration()
    {
        float initial = Call("Offset", 17f, 0.28f);
        Call("Offset", 240f, 0.28f); Call("Offset", -240f, 0.28f);
        Assert.That(Call("Offset", 17f, 0.28f), Is.EqualTo(initial));
        // A camera move of one metre produces the requested relative scenery motion.
        foreach (float speed in new[] { 0.08f, 0.28f, 0.55f })
            Assert.That(1 - (Call("Offset", 18f, speed) - InitialFor(speed)), Is.EqualTo(speed).Within(0.00001));
    }
    float InitialFor(float speed) => Call("Offset", 17f, speed);

    [TestCase("distant-canopy-v3")] [TestCase("recessed-valley-v4")]
    public void DistantPaintingKeepsTransparentHeadroomAboveCompleteSilhouettes(string name)
    {
        var source = Resources.Load<Texture2D>("BackgroundStudy/" + name);
        Assert.That(source, Is.Not.Null);
        var pixels = new Texture2D(2, 2);
        try {
            pixels.LoadImage(System.IO.File.ReadAllBytes(UnityEditor.AssetDatabase.GetAssetPath(source)));
            var data = pixels.GetPixels32();
            for (int y = pixels.height - pixels.height / 20; y < pixels.height; y++)
                for (int x = 0; x < pixels.width; x++)
                    Assert.That(data[y * pixels.width + x].a, Is.LessThan(26),
                        "No tree crown or tower may be cut by the top edge of the distant painting.");
        } finally { UnityEngine.Object.DestroyImmediate(pixels); }
    }

    [UnityTest] public IEnumerator SelectedParallaxAndWaterPreserveTerrainAndContact()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        yield return new EnterPlayMode();
        var previousScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        var scene = UnityEngine.SceneManagement.SceneManager.CreateScene("Background physics test",
            new UnityEngine.SceneManagement.CreateSceneParameters(UnityEngine.SceneManagement.LocalPhysicsMode.Physics2D));
        UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
        var root = new GameObject("Background study integration test");
        var study = root.AddComponent(Study);
        yield return null;
        // Drive the motor directly; live keyboard polling would release the scripted held jump.
        var liveInput = root.GetComponentsInChildren<MonoBehaviour>().First(c => c.GetType().Name == "CatInput");
        liveInput.enabled = false;
        var cat = (Component)Study.GetProperty("Cat").GetValue(study);
        var view = (Camera)Study.GetProperty("View").GetValue(study);
        var presentation = cat.GetComponentsInChildren<Component>().First(c => c.GetType().Name == "MikaSpritePresentation");
        cat.GetComponent<Rigidbody2D>().interpolation = RigidbodyInterpolation2D.None;
        var physics = scene.GetPhysicsScene2D();
        void Tick(int count) {
            Physics2D.SyncTransforms();
            for (int step = 0; step < count; step++) {
                cat.SendMessage("FixedUpdate"); physics.Simulate(Time.fixedDeltaTime);
                presentation.GetType().GetMethod("RenderPose").Invoke(presentation, new object[] { Time.fixedDeltaTime });
                study.SendMessage("LateUpdate");
            }
        }
        Physics2D.SyncTransforms();
        var colliders = root.GetComponentsInChildren<BoxCollider2D>().Where(c => !c.GetComponent<Rigidbody2D>()).ToArray();
        Assert.That(colliders.Length, Is.EqualTo(14));
        Assert.That(colliders.Last().bounds.max.x - colliders.First().bounds.min.x, Is.GreaterThanOrEqualTo(104),
            "The review course must be at least twice the original 52-unit length.");
        var terrain = root.GetComponentsInChildren<MeshRenderer>().Where(r => r.name.StartsWith("Painted grounded") || r.name.StartsWith("Painted floating")).ToArray();
        Assert.That(root.GetComponentsInChildren<MeshFilter>().Any(r => r.name == "Painted upper surface"), Is.False,
            "The integrated painting replaces the separate slab cap and repeated rock texture.");
        Assert.That(terrain.Length, Is.EqualTo(colliders.Length));
        var shoreFeet = root.GetComponentsInChildren<MeshRenderer>().Where(r => r.name == "Painted shore footing").ToArray();
        Assert.That(shoreFeet.Length, Is.EqualTo(5));
        Assert.That(shoreFeet.All(r => r.bounds.max.y < -1.3f && r.bounds.min.z > 2), Is.True,
            "Bank connections stay behind terrain and well below playable landing edges.");
        Assert.That(root.GetComponentsInChildren<Collider>().Length, Is.Zero,
            "Terrain art and corner foliage must not introduce 3D collision.");
        var frontWater = root.GetComponentsInChildren<MeshRenderer>().Where(r => r.name.StartsWith("Foreground water tile")).ToArray();
        var valley = root.GetComponentsInChildren<MeshRenderer>().Where(r => r.transform.parent.name == "Painted depth 1").ToArray();
        Assert.That(frontWater.Length, Is.EqualTo(3));
        Assert.That(valley.Length, Is.EqualTo(3));
        Assert.That(frontWater.All(r => r.bounds.max.z < 0), Is.True, "Foreground water draws before the cliff feet.");
        Assert.That(valley.All(r => r.bounds.min.z > 10), Is.True, "The valley stays recessed behind the playable world.");
        Assert.That(frontWater[0].sharedMaterial.mainTexture.name, Is.EqualTo("foreground-river"));
        Assert.That(valley[0].sharedMaterial.mainTexture.name, Is.EqualTo("recessed-valley-v4"));
        Assert.That(frontWater[0].sharedMaterial, Is.Not.SameAs(valley[0].sharedMaterial));
        Assert.That(frontWater[0].sharedMaterial.mainTexture, Is.Not.SameAs(valley[0].sharedMaterial.mainTexture),
            "Foreground and background must be independently authored sources, not copies of a connected river.");
        Assert.That(root.GetComponentsInChildren<Renderer>().Any(r => r.sharedMaterial && r.sharedMaterial.mainTexture &&
            (r.sharedMaterial.mainTexture.name == "connected-river" || r.sharedMaterial.mainTexture.name == "middle-cutout"
                || r.sharedMaterial.mainTexture.name == "near-cutout")), Is.False,
            "The rejected connected river and clipped near-tree layers must stay unloaded.");
        Assert.That(terrain.Count(r => r.name == "Painted grounded cliff"), Is.GreaterThan(terrain.Length / 2));
        for (int i = 0; i < terrain.Length; i++) {
            bool floating = terrain[i].name == "Painted floating rock";
            var mesh = terrain[i].GetComponent<MeshFilter>().sharedMesh;
            Assert.That(mesh.vertices[2].y, Is.EqualTo(colliders[i].bounds.max.y).Within(0.001f),
                "The painted landing row must align with the unchanged collider.");
            Assert.That(mesh.vertices[3].y, Is.EqualTo(colliders[i].bounds.max.y).Within(0.001f));
            Assert.That(terrain[i].bounds.max.y - colliders[i].bounds.max.y, Is.EqualTo(floating ? 0.35f : 0.6f).Within(0.001f),
                "Flowers above the contact row must stay bounded.");
            var parameters = new MaterialPropertyBlock(); terrain[i].GetPropertyBlock(parameters);
            var texture = parameters.GetTexture("_MainTex");
            Assert.That(texture, Is.Not.Null);
            Assert.That(texture.name, Is.EqualTo(floating ? "painted-shelf-v3" : colliders[i].bounds.size.x >= 6 ? "painted-bank-v2" : "painted-pillar-v2"));
            if (floating) Assert.That(terrain[i].bounds.size.y, Is.LessThanOrEqualTo(1.251f),
                "Suspended fragments need a shallow broken underside, not a compressed standing pillar.");
            Assert.That(mesh.uv.Min(uv => uv.y), Is.Zero);
            Assert.That(mesh.uv.Max(uv => uv.y), Is.EqualTo(1), "Use one complete module without repeated UVs.");
            if (terrain[i].name == "Painted grounded cliff")
                Assert.That(terrain[i].bounds.min.y, Is.LessThan(-3.2f), "Grounded rocks must extend below the river overlap.");
            if (i > 0) {
                Assert.That(colliders[i].bounds.min.x - colliders[i - 1].bounds.max.x, Is.InRange(1f, 4f));
                Assert.That(colliders[i].bounds.max.y - colliders[i - 1].bounds.max.y, Is.LessThanOrEqualTo(1f));
            }
        }
        var positions = colliders.Select(c => c.transform.position).ToArray();
        {
            // Foreground water stays registered to the world; the independent valley may parallax behind it.
            foreach (float anchor in new[] { -24.123f, -0.123f, 23.877f, 48.123f }) {
                Vector2? registered = null;
                foreach (float camera in new[] { anchor - 6, anchor + 6, anchor - 2, anchor - 6 }) {
                    Study.GetMethod("ApplyView").Invoke(study, new object[] { camera, camera > anchor ? 3f : 1f });
                    var panel = frontWater.First(r => r.bounds.min.x <= anchor && r.bounds.max.x >= anchor);
                    var uv = (Vector2)panel.transform.InverseTransformPoint(new Vector3(anchor, -3, panel.transform.position.z));
                    if (registered.HasValue) Assert.That(Vector2.Distance(uv, registered.Value), Is.LessThan(0.00001f),
                        "The separate foreground water must remain stable at world-space cliff feet.");
                    else registered = uv;
                }
            }
            foreach (float x in new[] { -120f, 57f, 59f, 240f, 17f })
                Study.GetMethod("ApplyView").Invoke(study, new object[] { x, 3f });
            Study.GetMethod("ApplyView").Invoke(study, new object[] { 17f, 1f });
            var valleyPanel = valley.First(r => r.bounds.min.x < 17 && r.bounds.max.x > 17);
            float before = valleyPanel.transform.InverseTransformPoint(new Vector3(17, 0, valleyPanel.transform.position.z)).x;
            Study.GetMethod("ApplyView").Invoke(study, new object[] { 18f, 1f });
            float after = valleyPanel.transform.InverseTransformPoint(new Vector3(17, 0, valleyPanel.transform.position.z)).x;
            Assert.That(Mathf.Abs(after - before), Is.EqualTo((1 - 0.14f) / 40).Within(0.00001f),
                "The selected subtle valley factor must stay fixed at 0.14.");
            for (int i = 0; i < positions.Length; i++) Assert.That(colliders[i].transform.position, Is.EqualTo(positions[i]));
            foreach (float y in new[] { 1f, 3f }) {
                Study.GetMethod("ApplyView").Invoke(study, new object[] { 17f, y });
                var sky = root.GetComponentsInChildren<MeshRenderer>().First(r => r.transform.parent.name == "Painted depth 0");
                Assert.That(sky.bounds.min.y, Is.LessThan(y - 5.5f));
                Assert.That(sky.bounds.max.y, Is.GreaterThan(y + 5.5f));
            }
        }
        if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null)
            AssertAnimatedWater(study, view);
        foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(640, 360),
            new Vector2Int(1024, 768), new Vector2Int(2560, 1080) }) {
            Study.GetMethod("ApplyViewport").Invoke(study, new object[] { size.x, size.y });
            Assert.That(view.aspect, Is.EqualTo(16f / 9f).Within(0.00001));
            Assert.That(size.x * view.rect.width / (size.y * view.rect.height), Is.EqualTo(16f / 9f).Within(0.00001));
        }
        Study.GetMethod("ApplyView").Invoke(study, new object[] { 17f, 1f });
        Assert.That(view.WorldToViewportPoint(new Vector3(15, 1)).y, Is.GreaterThanOrEqualTo(0.5f),
            "The raised landing should leave at least half the view for the lower river scenery.");
        Assert.That(cat.GetComponentInChildren<SpriteRenderer>().sprite, Is.Not.Null);
        Tick(30);
        Assert.That(cat.transform.position.y, Is.InRange(0.4f, 0.6f), "Existing motor must settle onto world-space starting ground.");
        // Verify the real presentation anchor at the raised bank platform as well as flat ground.
        cat.GetType().GetMethod("RespawnAt").Invoke(cat, new object[] { new Vector2(15f, 1.48f) });
        Tick(30);
        Assert.That(cat.GetType().GetProperty("Grounded").GetValue(cat), Is.True);
        presentation.GetType().GetMethod("RenderPose").Invoke(presentation, new object[] { 0.25f });
        presentation.GetType().GetMethod("RenderPose").Invoke(presentation, new object[] { 0.25f });
        Assert.That(VisiblePawBaseline(cat.GetComponentInChildren<SpriteRenderer>()), Is.EqualTo(1).Within(0.05f),
            "Mika's rendered paw baseline must meet the raised bank's collision contact.");
        Study.GetMethod("ApplyView").Invoke(study, new object[] { 10f, 1f });
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "-captureStudyJump") >= 0)
            CaptureJump(view, "play-ground-contact.png");
        cat.GetType().GetMethod("RespawnAt").Invoke(cat, new object[] { new Vector2(28.5f, 2.48f) });
        Tick(30);
        Assert.That(cat.GetType().GetProperty("Grounded").GetValue(cat), Is.True);
        Study.GetMethod("ApplyView").Invoke(study, new object[] { 28.5f, 1f });
        var motion = cat.GetType().GetField("Motion").GetValue(cat);
        motion.GetType().GetMethod("PressJump").Invoke(motion, null);
        float highest = cat.transform.position.y;
        for (int frame = 0; frame < 25; frame++) {
            Tick(1);
            highest = Mathf.Max(highest, cat.transform.position.y);
            Assert.That(view.transform.position.y, Is.EqualTo(1).Within(0.01f), "Keep foreground water framed during jumps.");
            Assert.That(cat.GetComponentInChildren<SpriteRenderer>().bounds.max.y,
                Is.LessThan(view.transform.position.y + view.orthographicSize), "The highest course jump must remain visible.");
            Assert.That(view.WorldToViewportPoint(cat.transform.position).x, Is.InRange(0.1f, 0.9f));
            if (frame == 12 && Array.IndexOf(Environment.GetCommandLineArgs(), "-captureStudyJump") >= 0)
                CaptureJump(view);
        }
        Assert.That(highest, Is.GreaterThan(3));
        UnityEngine.Object.Destroy(root);
        yield return null;
        UnityEngine.SceneManagement.SceneManager.SetActiveScene(previousScene);
        yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
        yield return new ExitPlayMode();
    }

    void AssertAnimatedWater(Component study, Camera view)
    {
        Study.GetMethod("ApplyView").Invoke(study, new object[] { 17f, 1f });
        Study.GetMethod("SetWaterTime").Invoke(study, new object[] { 0f });
        var before = Snapshot(view);
        Study.GetMethod("SetWaterTime").Invoke(study, new object[] { 1.7f });
        var after = Snapshot(view);
        int flowing = 0, warmStonePixels = 0;
        var panels = study.GetComponentsInChildren<MeshRenderer>().Where(r => r.name.StartsWith("Foreground water tile")).ToArray();
        var source = new Texture2D(2, 2);
        source.LoadImage(System.IO.File.ReadAllBytes(UnityEditor.AssetDatabase.GetAssetPath(panels[0].sharedMaterial.mainTexture)));
        source.wrapMode = TextureWrapMode.Clamp;
        try {
        for (int i = 0; i < before.Length; i++) {
            int y = i / 640;
            int difference = Mathf.Abs(before[i].r - after[i].r) + Mathf.Abs(before[i].g - after[i].g)
                + Mathf.Abs(before[i].b - after[i].b);
            if (y > 90) Assert.That(difference, Is.Zero, "Water animation must leave distant scenery and terrain unchanged.");
            else {
                if (difference > 3) flowing++;
                // Classify the original painting, not a warm reflection refracted from nearby water.
                var point = view.ViewportToWorldPoint(new Vector3((i % 640 + 0.5f) / 640, (y + 0.5f) / 360, 8));
                var panel = panels.First(r => r.bounds.min.x <= point.x && r.bounds.max.x >= point.x);
                var local = panel.transform.InverseTransformPoint(point);
                var original = source.GetPixelBilinear(local.x + 0.5f, local.y + 0.5f);
                if (original.a > 0.99f && original.r > original.g + 0.05f && original.b < original.g) {
                    warmStonePixels++;
                    Assert.That(difference, Is.LessThanOrEqualTo(1), "Warm rocks and shoreline colors must not ripple.");
                }
            }
        }
        Assert.That(flowing, Is.GreaterThan(100), "The foreground water must visibly change with time at a fixed camera.");
        Assert.That(warmStonePixels, Is.GreaterThan(20), "The fixed-color check needs visible rock/shore samples.");
        } finally { UnityEngine.Object.DestroyImmediate(source); }
    }

    static Color32[] Snapshot(Camera view)
    {
        var target = new RenderTexture(640, 360, 24);
        var pixels = new Texture2D(640, 360, TextureFormat.RGB24, false);
        var previous = RenderTexture.active; var previousTarget = view.targetTexture; var previousRect = view.rect;
        try {
            view.targetTexture = target; view.rect = new Rect(0, 0, 1, 1); view.Render();
            RenderTexture.active = target; pixels.ReadPixels(new Rect(0, 0, 640, 360), 0, 0); pixels.Apply();
            return pixels.GetPixels32();
        } finally {
            RenderTexture.active = previous; view.targetTexture = previousTarget; view.rect = previousRect;
            target.Release(); UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(pixels);
        }
    }

    static float VisiblePawBaseline(SpriteRenderer renderer)
    {
        // Sprite bounds include transparent padding; inspect the drawn support pixels instead.
        var sprite = renderer.sprite;
        for (int y = 0; y < sprite.rect.height; y++)
            for (int x = 0; x < sprite.rect.width; x++)
                if (sprite.texture.GetPixel((int)sprite.rect.x + x, (int)sprite.rect.y + y).a > 0.95f)
                    return renderer.transform.TransformPoint(new Vector3(
                        (x + 0.5f - sprite.pivot.x) / sprite.pixelsPerUnit,
                        (y + 0.5f - sprite.pivot.y) / sprite.pixelsPerUnit, 0)).y;
        Assert.Fail("Mika must have visible support pixels.");
        return float.NaN;
    }

    static void CaptureJump(Camera view, string filename = "play-jump.png")
    {
        // Optional graphics-device evidence of the actual motor/animation in Play mode.
        var target = new RenderTexture(1280, 720, 24);
        var pixels = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        var previous = RenderTexture.active; var previousTarget = view.targetTexture; var previousRect = view.rect;
        try {
            view.targetTexture = target; view.rect = new Rect(0, 0, 1, 1); view.Render();
            RenderTexture.active = target; pixels.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); pixels.Apply();
            System.IO.File.WriteAllBytes("docs/design/background-study/" + filename, pixels.EncodeToPNG());
        } finally {
            RenderTexture.active = previous; view.targetTexture = previousTarget; view.rect = previousRect;
            target.Release(); UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(pixels);
        }
    }
}
