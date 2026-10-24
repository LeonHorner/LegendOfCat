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

    [TestCase(0f)] [TestCase(0.08f)] [TestCase(0.28f)] [TestCase(0.55f)] [TestCase(1f)]
    public void MirroredTilesCoverViewAcrossNegativeAndPositiveWraps(float speed)
    {
        // Three 64-unit tiles must cover the widest supported 32:9 review viewport.
        for (int step = -2000; step <= 2000; step++) {
            float camera = step * 0.5f;
            float center = Call("Offset", camera, speed) + Call("TileCenter", camera, speed, 64f);
            Assert.That(center - 96, Is.LessThan(camera - 20));
            Assert.That(center + 96, Is.GreaterThan(camera + 20));
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
        Assert.That(colliders.Length, Is.EqualTo(7));
        var positions = colliders.Select(c => c.transform.position).ToArray();
        for (int mode = 0; mode < 3; mode++) {
            Study.GetField("concept").SetValue(study, Enum.ToObject(Study.GetNestedType("Concept"), mode));
            foreach (float x in new[] { -120f, 57f, 59f, 240f, 17f })
                Study.GetMethod("ApplyView").Invoke(study, new object[] { x, 3f });
            for (int i = 0; i < positions.Length; i++) Assert.That(colliders[i].transform.position, Is.EqualTo(positions[i]));
            foreach (float y in new[] { 3f, 5f }) {
                Study.GetMethod("ApplyView").Invoke(study, new object[] { 17f, y });
                var sky = root.GetComponentsInChildren<MeshRenderer>().First(r => r.transform.parent.name == "Painted depth 0");
                Assert.That(sky.bounds.min.y, Is.LessThan(y - 5.5f));
                Assert.That(sky.bounds.max.y, Is.GreaterThan(y + 5.5f));
            }
        }
        var cat = (Component)Study.GetProperty("Cat").GetValue(study);
        Assert.That(cat.GetComponentInChildren<SpriteRenderer>().sprite, Is.Not.Null);
        yield return new WaitForSeconds(0.25f);
        Assert.That(cat.transform.position.y, Is.InRange(0.4f, 0.6f), "Existing motor must settle onto world-space starting ground.");
        UnityEngine.Object.Destroy(root);
        yield return null;
        yield return new ExitPlayMode();
    }
}
