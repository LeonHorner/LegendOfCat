using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class LevelProgressTests
{
    static Type RuntimeType(string name) => Type.GetType(name + ", Assembly-CSharp", true);
    static object Read(object target, string name) => target.GetType().GetProperty(name).GetValue(target);
    static object Call(object target, string name, params object[] args) =>
        target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Invoke(target, args);

    static IEnumerable<string> LevelIds =>
        (IEnumerable<string>)RuntimeType("LevelRegistry").GetProperty("Ids").GetValue(null);

    [UnityTest]
    public IEnumerator RegisteredLevelLaunchesWithProgress([ValueSource(nameof(LevelIds))] string id)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        yield return new EnterPlayMode();
        var host = new GameObject("Progress integration test");
        var screen = host.AddComponent(RuntimeType("StarterScreen"));
        // Exercise the default before Start reads the player's local settings.
        Call(screen, "OpenLevel", id);
        var course = screen.GetType().GetField("course", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(screen);
        Assert.That(course, Is.Not.Null, id);
        var hud = Read(course, "Progress");
        Assert.That(hud, Is.Not.Null, id);
        Assert.That(Read(hud, "Visible"), Is.True, id);
        Assert.That(Read(hud, "HasArtwork"), Is.True, id);
        Assert.That(Read(hud, "Artwork"), Is.EqualTo(id));
        var checkpoints = (IReadOnlyList<Vector2>)Read(course, "ProgressCheckpoints");
        Assert.That(Read(hud, "CheckpointCount"), Is.EqualTo(checkpoints.Count), id);
        var root = ((Component)course).transform;
        for (int i = 1; i <= checkpoints.Count; i++) {
            string path = id == "golden-river" ? $"Checkpoint waystone {i}/Engraved paw" : $"Safe checkpoint {i}";
            var world = root.Find(path)?.GetComponent<SpriteRenderer>();
            Assert.That(world, Is.Not.Null, $"{id} checkpoint {i} has no world artwork.");
            Assert.That(Call(hud, "GetCheckpointState", i, 0).ToString(), Is.EqualTo("Unreached"), id);
            Assert.That(Call(hud, "CheckpointVisual", i), Is.SameAs(world),
                $"{id} checkpoint {i} must use its world artwork in the HUD.");
        }
        Assert.That(Read(hud, "Fraction"), Is.EqualTo(0f), id);
        var settings = RuntimeType("MenuSettings").GetMethod("Open").Invoke(null,
            new object[] { Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".json") });
        screen.GetType().GetField("settingsFile", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(screen, settings);
        Call(screen, "SetShowLevelProgress", false);
        Assert.That(Read(hud, "Visible"), Is.False, id);

        var run = Read(course, "Run");
        if (checkpoints.Count > 0) {
            var spawn = checkpoints[0];
            Call(hud, "MarkCheckpointReached", 1);
            Call(run, "TryActivateCheckpoint", 1, spawn, true);
            Assert.That(Call(hud, "GetCheckpointState", 1, 1).ToString(), Is.EqualTo("Active"), id);
            Call(course, "RetryCheckpoint");
            float atCheckpoint = (float)Read(hud, "Fraction");
            Assert.That(atCheckpoint, Is.GreaterThan(0).And.LessThan(1), id);
            Assert.That(Call(hud, "CheckpointFraction", 1), Is.EqualTo(atCheckpoint), id);
            Call(hud, "UpdatePosition", spawn + Vector2.up * 2, false);
            Call(course, "RetryCheckpoint");
            Assert.That(Read(hud, "Fraction"), Is.EqualTo(atCheckpoint), id);
            if (checkpoints.Count > 1) {
                Assert.That(Call(hud, "GetCheckpointState", 2, 1).ToString(), Is.EqualTo("Unreached"), id);
                Call(hud, "MarkCheckpointReached", 2);
                Call(run, "TryActivateCheckpoint", 2, checkpoints[1], true);
                Assert.That(Call(hud, "GetCheckpointState", 1, 2).ToString(), Is.EqualTo("Reached"), id);
                Assert.That(Call(hud, "GetCheckpointState", 2, 2).ToString(), Is.EqualTo("Active"), id);
            }
        }
        Call(course, "RestartRun");
        Assert.That(Read(hud, "Fraction"), Is.EqualTo(0f), id);
        Assert.That(Read(run, "Checkpoint"), Is.EqualTo(0), id);
        Assert.That(Call(hud, "CheckpointFraction", 0), Is.Null, id);
        for (int i = 1; i <= checkpoints.Count; i++)
            Assert.That(Call(hud, "GetCheckpointState", i, 0).ToString(), Is.EqualTo("Unreached"), id);

        Call(screen, "CloseCourse");
        UnityEngine.Object.DestroyImmediate(host);
        yield return new ExitPlayMode();
    }

    [UnityTest]
    public IEnumerator MovementLabGuideWaitsForGroundedCheckpointActivation()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        yield return new EnterPlayMode();
        var host = new GameObject("Checkpoint activation test");
        var screen = host.AddComponent(RuntimeType("StarterScreen"));
        Call(screen, "OpenLevel", "movement-lab");
        var course = screen.GetType().GetField("course", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(screen);
        var hud = Read(course, "Progress");
        var run = Read(course, "Run");
        var cat = (Component)Read(course, "Cat");
        var grounded = cat.GetType().GetProperty("Grounded");
        var first = ((IReadOnlyList<Vector2>)Read(course, "ProgressCheckpoints"))[0];

        void Place(Vector2 position, bool onGround)
        {
            cat.GetComponent<Rigidbody2D>().position = position;
            grounded.SetValue(cat, onGround);
            Call(course, "Update");
        }

        var landing = first + Vector2.right * 1.5f;
        Place(landing + Vector2.up, false);
        Assert.That(Read(run, "Checkpoint"), Is.EqualTo(0));
        Assert.That(Call(hud, "UpdateGuideCheckpoint", 0, 20f), Is.EqualTo(1));

        Place(landing, true);
        int active = (int)Read(run, "Checkpoint");
        Assert.That(active, Is.EqualTo(1), "Landing beyond the post on its platform should activate the checkpoint.");
        Assert.That(Call(hud, "UpdateGuideCheckpoint", active, 20f), Is.EqualTo(2));

        Call(screen, "CloseCourse");
        UnityEngine.Object.DestroyImmediate(host);
        yield return new ExitPlayMode();
    }

    [Test]
    public void GuideAdvancesWithinTwoHudPixelsOfEachReachedCheckpoint()
    {
        var route = new[] { Vector2.zero, Vector2.right, Vector2.right * 2, Vector2.right * 3 };
        var hud = Activator.CreateInstance(RuntimeType("LevelProgress"), route, new[] { 1, 2 }, "golden-river");
        int Guide(int active, float offset) => (int)Call(hud, "UpdateGuideCheckpoint", active, offset);

        Assert.That(Guide(0, 0), Is.EqualTo(1));
        Assert.That(Guide(1, 1.9f), Is.EqualTo(1));
        Assert.That(Guide(1, 2), Is.EqualTo(2));
        Assert.That(Guide(1, 1.9f), Is.EqualTo(2), "Backtracking must keep the next checkpoint targeted.");
        Assert.That(Guide(2, 0), Is.EqualTo(2));
        Assert.That(Guide(2, -2), Is.Zero);
        Call(hud, "Reset", Vector2.zero, 0, false);
        Assert.That(Guide(0, 0), Is.EqualTo(1));
    }

    [Test]
    public void ProjectionFollowsSwitchbackAndVerticalCrossing()
    {
        var points = new[] { Vector2.zero, new Vector2(20, 0), new Vector2(20, 3),
            new Vector2(0, 3), new Vector2(0, 10) };
        var hud = Activator.CreateInstance(RuntimeType("LevelProgress"), points, new[] { 1 }, "golden-river");
        void Place(float x, float y) => Call(hud, "UpdatePosition", new Vector2(x, y), false);
        float Fraction() => (float)Read(hud, "Fraction");
        Place(4, 3);
        Assert.That(Fraction(), Is.LessThan(.2f), "An overlapping upper tier must not advance the route.");
        Call(hud, "Reset", points[1], 1, false);
        Place(20, 3);
        Assert.That(Fraction(), Is.EqualTo(23f / 50).Within(.001f));
        Place(16, 3);
        Assert.That(Fraction(), Is.EqualTo(27f / 50).Within(.001f));
        Place(18, 3);
        Assert.That(Fraction(), Is.EqualTo(25f / 50).Within(.001f));
        Call(hud, "UpdatePosition", points[4], true);
        Assert.That(Fraction(), Is.EqualTo(1));
        Place(0, 8);
        Assert.That(Fraction(), Is.LessThan(1));
    }

    [Test]
    public void MenuPreferenceDefaultsOnAndPersistsOff()
    {
        var dir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        try {
            string path = Path.Combine(dir.FullName, "menu-settings.json");
            var open = RuntimeType("MenuSettings").GetMethod("Open");
            object Open() => open.Invoke(null, new object[] { path });
            object ReadPreference(object file) => Read(file, "Data").GetType().GetProperty("Item",
                new[] { typeof(string) }).GetValue(Read(file, "Data"), new object[] { "showLevelProgress" });
            Assert.That(ReadPreference(Open()).ToString(), Is.EqualTo("True").IgnoreCase);
            File.WriteAllText(path, "{\"schemaVersion\":0,\"lastOfferedVersion\":\"\"}");
            Assert.That(ReadPreference(Open()).ToString(), Is.EqualTo("True").IgnoreCase);
            var file = Open();
            var data = Read(file, "Data");
            var value = Activator.CreateInstance(ReadPreference(file).GetType(), new object[] { false });
            data.GetType().GetProperty("Item", new[] { typeof(string) }).SetValue(data, value,
                new object[] { "showLevelProgress" });
            Call(file, "Save");
            Assert.That(ReadPreference(Open()).ToString(), Is.EqualTo("False").IgnoreCase);
        } finally {
            dir.Delete(true);
        }
    }
}
