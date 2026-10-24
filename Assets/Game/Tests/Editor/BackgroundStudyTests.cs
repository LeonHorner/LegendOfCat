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
        foreach (float width in new[] { 24f, 32f, 64f })
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
        var upperSurfaces = root.GetComponentsInChildren<MeshFilter>().Where(r => r.name == "Painted upper surface").ToArray();
        Assert.That(upperSurfaces.Length, Is.EqualTo(colliders.Length));
        var foliage = root.GetComponentsInChildren<MeshRenderer>().Where(r => r.name == "Hanging platform foliage").ToArray();
        Assert.That(foliage.Length, Is.GreaterThan(colliders.Length));
        Assert.That(foliage.Select(r => r.bounds.size.y).Distinct().Count(), Is.GreaterThan(2),
            "Hanging greenery must vary rather than repeat a single length.");
        Assert.That(foliage.All(r => r.bounds.min.z > 0), Is.True, "Leaves render behind Mika's gameplay plane.");
        Assert.That(terrain.Length, Is.EqualTo(colliders.Length));
        Assert.That(root.GetComponentsInChildren<Collider>().Length, Is.Zero,
            "Terrain art and corner foliage must not introduce 3D collision.");
        Assert.That(terrain.Count(r => r.name == "Painted grounded cliff"), Is.GreaterThan(terrain.Length / 2));
        for (int i = 0; i < terrain.Length; i++) {
            Assert.That(terrain[i].bounds.max.y, Is.EqualTo(colliders[i].bounds.max.y).Within(0.001f));
            Assert.That(terrain[i].bounds.size.x, Is.EqualTo(colliders[i].bounds.size.x).Within(0.001f));
            Assert.That(upperSurfaces[i].sharedMesh.vertices[0].y, Is.EqualTo(colliders[i].bounds.max.y).Within(0.001f),
                "The receding top must meet the exact collision lip at its front edge.");
            if (terrain[i].name == "Painted grounded cliff") Assert.That(terrain[i].bounds.min.y, Is.EqualTo(-10).Within(0.001f));
            if (i > 0) {
                Assert.That(colliders[i].bounds.min.x - colliders[i - 1].bounds.max.x, Is.InRange(1f, 4f));
                Assert.That(colliders[i].bounds.max.y - colliders[i - 1].bounds.max.y, Is.LessThanOrEqualTo(1f));
            }
        }
        var positions = colliders.Select(c => c.transform.position).ToArray();
        for (int mode = 0; mode < 3; mode++) {
            Study.GetField("concept").SetValue(study, Enum.ToObject(Study.GetNestedType("Concept"), mode));
            foreach (float x in new[] { -120f, 57f, 59f, 240f, 17f })
                Study.GetMethod("ApplyView").Invoke(study, new object[] { x, 3f });
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
        UnityEngine.Object.Destroy(root);
        yield return null;
        yield return new ExitPlayMode();
    }
}
