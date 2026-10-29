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
        foreach (float width in new[] { 24f, 48f, 64f })
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

    [Test] public void DistantCanopyKeepsTransparentHeadroomAboveCompleteSilhouettes()
    {
        var source = Resources.Load<Texture2D>("BackgroundStudy/distant-canopy-v3");
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

    [UnityTest] public IEnumerator ConceptSwitchAndCameraTravelNeverMoveCollisionGeometry()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        yield return new EnterPlayMode();
        var root = new GameObject("Background study integration test");
        var study = root.AddComponent(Study);
        yield return null;
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
        var river = root.GetComponentsInChildren<MeshRenderer>().Where(r => r.sharedMaterial.shader.name == "LegendOfCat/StudyRiver").ToArray();
        Assert.That(river.Length, Is.EqualTo(6), "Background and foreground must use aligned copies of one painting.");
        Assert.That(river.Select(r => r.sharedMaterial.mainTexture).Distinct().Count(), Is.EqualTo(1));
        var backWater = river.Where(r => r.sharedMaterial.GetFloat("_Foreground") == 0).ToArray();
        var frontWater = river.Where(r => r.sharedMaterial.GetFloat("_Foreground") == 1).ToArray();
        Assert.That(backWater.Length, Is.EqualTo(3)); Assert.That(frontWater.Length, Is.EqualTo(3));
        Assert.That(frontWater.All(r => r.bounds.max.z < 0), Is.True,
            "Water must draw in front of near-bank vegetation and cliff faces.");
        Assert.That(river[0].sharedMaterial.mainTexture.name, Is.EqualTo("connected-river"));
        Assert.That(root.GetComponentsInChildren<MeshRenderer>().Any(r => r.name == "Foreground river tile"), Is.False,
            "Do not reintroduce an independent foreground-water graphic.");
        Assert.That(root.GetComponentsInChildren<Renderer>().Any(r => r.sharedMaterial && r.sharedMaterial.mainTexture &&
            (r.sharedMaterial.mainTexture.name == "foreground-river" || r.sharedMaterial.mainTexture.name == "middle-cutout"
                || r.sharedMaterial.mainTexture.name == "near-cutout")), Is.False,
            "Do not load the superseded independent water or clipped near-tree layers.");
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
        for (int mode = 0; mode < 3; mode++) {
            Study.GetField("concept").SetValue(study, Enum.ToObject(Study.GetNestedType("Concept"), mode));
            // Camera movement, reversals and recycled tiles must not slide river pixels under a fixed rock.
            foreach (float anchor in new[] { -24.123f, -0.123f, 23.877f, 48.123f }) {
                Vector2? registered = null;
                foreach (float camera in new[] { anchor - 6, anchor + 6, anchor - 2, anchor - 6 }) {
                    Study.GetMethod("ApplyView").Invoke(study, new object[] { camera, camera > anchor ? 3f : 1f });
                    var panel = backWater.First(r => r.bounds.min.x <= anchor && r.bounds.max.x >= anchor);
                    var uv = (Vector2)panel.transform.InverseTransformPoint(new Vector3(anchor, -3, panel.transform.position.z));
                    if (registered.HasValue) Assert.That(Vector2.Distance(uv, registered.Value), Is.LessThan(0.00001f),
                        "The connected river and shore must remain registered to world-space platforms.");
                    else registered = uv;
                }
            }
            foreach (float x in new[] { -120f, 57f, 59f, 240f, 17f })
                Study.GetMethod("ApplyView").Invoke(study, new object[] { x, 3f });
            for (int tile = 0; tile < 3; tile++) {
                Assert.That((Vector2)backWater[tile].bounds.center, Is.EqualTo((Vector2)frontWater[tile].bounds.center));
                Assert.That((Vector2)backWater[tile].bounds.size, Is.EqualTo((Vector2)frontWater[tile].bounds.size));
                Assert.That(backWater[tile].transform.localScale, Is.EqualTo(frontWater[tile].transform.localScale));
            }
            for (int i = 0; i < positions.Length; i++) Assert.That(colliders[i].transform.position, Is.EqualTo(positions[i]));
            foreach (float y in new[] { 1f, 3f }) {
                Study.GetMethod("ApplyView").Invoke(study, new object[] { 17f, y });
                var sky = root.GetComponentsInChildren<MeshRenderer>().First(r => r.transform.parent.name == "Painted depth 0");
                Assert.That(sky.bounds.min.y, Is.LessThan(y - 5.5f));
                Assert.That(sky.bounds.max.y, Is.GreaterThan(y + 5.5f));
            }
        }
        var cat = (Component)Study.GetProperty("Cat").GetValue(study);
        var view = (Camera)Study.GetProperty("View").GetValue(study);
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
        yield return new WaitForSeconds(0.25f);
        Assert.That(cat.transform.position.y, Is.InRange(0.4f, 0.6f), "Existing motor must settle onto world-space starting ground.");
        // Verify the real presentation anchor at the raised bank platform as well as flat ground.
        cat.GetType().GetMethod("RespawnAt").Invoke(cat, new object[] { new Vector2(15f, 1.48f) });
        for (int frame = 0; frame < 30; frame++) yield return new WaitForFixedUpdate();
        Assert.That(cat.GetType().GetProperty("Grounded").GetValue(cat), Is.True);
        var presentation = cat.GetComponentsInChildren<Component>().First(c => c.GetType().Name == "MikaSpritePresentation");
        presentation.GetType().GetMethod("RenderPose").Invoke(presentation, new object[] { 0.25f });
        presentation.GetType().GetMethod("RenderPose").Invoke(presentation, new object[] { 0.25f });
        Assert.That(VisiblePawBaseline(cat.GetComponentInChildren<SpriteRenderer>()), Is.EqualTo(1).Within(0.05f),
            "Mika's rendered paw baseline must meet the raised bank's collision contact.");
        Study.GetMethod("ApplyView").Invoke(study, new object[] { 10f, 1f });
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "-captureStudyJump") >= 0)
            CaptureJump(view, "play-ground-contact.png");
        cat.GetType().GetMethod("RespawnAt").Invoke(cat, new object[] { new Vector2(28.5f, 2.48f) });
        yield return new WaitForSeconds(0.25f);
        Study.GetMethod("ApplyView").Invoke(study, new object[] { 28.5f, 1f });
        var motion = cat.GetType().GetField("Motion").GetValue(cat);
        motion.GetType().GetMethod("PressJump").Invoke(motion, null);
        float highest = cat.transform.position.y;
        for (int frame = 0; frame < 25; frame++) {
            yield return new WaitForFixedUpdate();
            // EditMode coroutine scheduling does not guarantee a LateUpdate per physics step.
            Study.GetMethod("LateUpdate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(study, null);
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
        yield return new ExitPlayMode();
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
