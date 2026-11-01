using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools;
using UnityEngine;
using UnityEngine.TestTools;

public class MenuMovementIntegrationTests
{
    GameObject host;
    static int exitSignals;
    static void CountExit() => exitSignals++;
    static Type RuntimeType(string name) => AppDomain.CurrentDomain.GetAssemblies()
        .Select(a => a.GetType(name)).First(t => t != null);
    static object Field(object target, string name) => target.GetType()
        .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
    static void Call(object target, string name) => target.GetType()
        .GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, null);

    static UnityEngine.Object[] SceneryResources(Component course)
    {
        var study = course.GetComponentInChildren(RuntimeType("SceneryRenderer"));
        Assert.That(study, Is.Not.Null);
        // Check synchronously after OpenCourse, before deferred destruction could hide a collider.
        Assert.That(course.GetComponentsInChildren<Collider>(true), Is.Empty);
        var filters = course.GetComponentsInChildren<MeshFilter>(true);
        Assert.That(filters.Length, Is.GreaterThan(0));
        foreach (var filter in filters) {
            Assert.That(filter.GetComponent<Collider2D>(), Is.Null, filter.name);
            Assert.That(filter.sharedMesh, Is.Not.Null, filter.name);
            Assert.That(filter.GetComponent<MeshRenderer>().sharedMaterial, Is.Not.Null, filter.name);
        }
        Assert.That(course.GetComponentsInChildren<Collider2D>(true), Has.Length.EqualTo(15),
            "Only fourteen gameplay platforms and Mika may have colliders.");
        var quad = filters.First(f => f.name.StartsWith("Mirrored scenery tile")).sharedMesh;
        Assert.That(quad.bounds.center, Is.EqualTo(Vector3.zero));
        Assert.That(quad.bounds.size, Is.EqualTo(new Vector3(1, 1, 0)));
        Assert.That(quad.uv, Is.EqualTo(new[] { Vector2.zero, Vector2.right, Vector2.up, Vector2.one }));
        var owned = filters.Select(f => (UnityEngine.Object)f.sharedMesh)
            .Concat(filters.Select(f => (UnityEngine.Object)f.GetComponent<MeshRenderer>().sharedMaterial))
            .Distinct().ToArray();
        Assert.That(owned.All(o => o && !AssetDatabase.Contains(o)), Is.True,
            "Track generated resources, never imported artwork or shader-reference materials.");
        return owned;
    }

    [UnityTest]
    public IEnumerator LevelOneUsesDistinctShelvesAndNonRepeatingLandmarks()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        yield return new EnterPlayMode();
        host = new GameObject("Route variety test");
        var screen = host.AddComponent(RuntimeType("StarterScreen"));
        yield return null;
        Call(screen, "OpenLevelOne");
        var course = (Component)Field(screen, "course");
        var scenery = course.GetComponentInChildren(RuntimeType("SceneryRenderer"));
        var roots = course.GetComponentsInChildren<MeshFilter>().Where(f => f.name == "Root island").ToArray();
        var masonry = course.GetComponentsInChildren<MeshFilter>().Where(f => f.name == "Masonry shelf").ToArray();
        Assert.That(roots.Length, Is.GreaterThan(0));
        Assert.That(masonry.Length, Is.GreaterThan(0));
        foreach (var pair in new[] { (roots[0], "shelf-root"), (masonry[0], "shelf-masonry") }) {
            var block = new MaterialPropertyBlock();
            pair.Item1.GetComponent<MeshRenderer>().GetPropertyBlock(block);
            Assert.That(block.GetTexture("_MainTex"), Is.SameAs(Resources.Load<Texture2D>("GoldenRiver/" + pair.Item2)));
        }
        var floatingIndices = (int[])course.GetType().GetField("LevelOneFloatingPlatforms").GetValue(null);
        var rootIndices = (int[])course.GetType().GetField("LevelOneRootPlatforms").GetValue(null);
        var geometry = (Vector3[])course.GetType().GetField("LevelOnePlatforms").GetValue(null);
        float lowestContact = float.PositiveInfinity;
        bool descendsFromBank = false;
        foreach (int index in floatingIndices) {
            lowestContact = Mathf.Min(lowestContact, geometry[index].y + 0.75f);
            if (index > 0 && !floatingIndices.Contains(index - 1) && geometry[index].y < geometry[index - 1].y)
                descendsFromBank = true;
        }
        Assert.That(lowestContact, Is.LessThanOrEqualTo(-1f),
            "Some suspended supports should approach the foreground river.");
        Assert.That(descendsFromBank, Is.True, "A suspended crossing can descend below its grounded entrance.");
        for (int i = 1; i < floatingIndices.Length; i++)
            if (floatingIndices[i] == floatingIndices[i - 1] + 1)
                Assert.That(rootIndices.Contains(floatingIndices[i]), Is.EqualTo(rootIndices.Contains(floatingIndices[i - 1])),
                    "A connected suspended sequence must retain one visual family.");
        var landmarks = course.GetComponentsInChildren<Transform>().Where(t => t.name.StartsWith("Unique landmark ")).ToArray();
        Assert.That(landmarks, Has.Length.EqualTo(3));
        Assert.That(landmarks.Select(t => t.name).Distinct().Count(), Is.EqualTo(3));
        Assert.That(landmarks.All(t => t.GetComponent<Collider2D>() == null), Is.True);
        var apply = scenery.GetType().GetMethod("ApplyView");
        apply.Invoke(scenery, new object[] { 84f, 1f });
        var positions = landmarks.Select(t => t.localPosition).ToArray();
        foreach (float x in new[] { 1000f, -200f, 342f, 84f }) apply.Invoke(scenery, new object[] { x, 1f });
        Assert.That(landmarks.Select(t => t.localPosition).ToArray(), Is.EqualTo(positions),
            "Landmarks must retain their unique anchors after travel, reversal and teleport.");
        var platforms = course.GetComponentsInChildren<BoxCollider2D>().Where(c => c.name == "Standable platform").ToArray();
        Assert.That(platforms.Max(c => c.transform.position.y) - platforms.Min(c => c.transform.position.y), Is.GreaterThan(1.5f));
        Assert.That(platforms.Select(c => c.transform.localScale.x).Distinct().Count(), Is.GreaterThanOrEqualTo(8));
    }

    [UnityTest]
    public IEnumerator LevelOneCanBeTraversedThroughActualPhysicsWithoutRetries()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        yield return new EnterPlayMode();
        host = new GameObject("Full route physics test");
        var screen = host.AddComponent(RuntimeType("StarterScreen"));
        yield return null;
        Call(screen, "OpenLevelOne");
        var course = (Component)Field(screen, "course");
        var cat = (Component)course.GetType().GetProperty("Cat").GetValue(course);
        var body = cat.GetComponent<Rigidbody2D>();
        ((Behaviour)cat.GetComponent(RuntimeType("CatInput"))).enabled = false;
        var motion = cat.GetType().GetField("Motion").GetValue(cat);
        var platforms = (Vector3[])course.GetType().GetField("LevelOnePlatforms").GetValue(null);
        var run = course.GetType().GetProperty("Run").GetValue(course);
        var oldMode = Physics2D.simulationMode;
        Physics2D.simulationMode = SimulationMode2D.Script;
        try {
            Physics2D.SyncTransforms();
            int next = 0;
            float introductionTravel = -1, traversal = 0;
            for (int tick = 0; tick < 6000; tick++) {
                bool grounded = (bool)cat.GetType().GetProperty("Grounded").GetValue(cat);
                while (next < platforms.Length - 1 && body.position.x > platforms[next].x + platforms[next].z / 2)
                    next++;
                motion.GetType().GetMethod("SetMove").Invoke(motion, new object[] { 1f });
                if (grounded && next < platforms.Length - 1 &&
                    body.position.x >= platforms[next].x + platforms[next].z / 2 - 0.7f)
                    cat.GetType().GetMethod("PressJump").Invoke(cat, null);
                Call(cat, "FixedUpdate");
                Physics2D.Simulate(Time.fixedDeltaTime);
                Call(course, "Update");
                traversal = (tick + 1) * Time.fixedDeltaTime;
                if (introductionTravel < 0 && (int)run.GetType().GetProperty("Checkpoint").GetValue(run) >= 1)
                    introductionTravel = traversal;
                Assert.That((int)run.GetType().GetProperty("Retries").GetValue(run), Is.Zero,
                    "Failed on platform " + next + " at " + body.position);
                if ((bool)course.GetType().GetProperty("HasReachedExit").GetValue(course)) break;
            }
            Assert.That((bool)course.GetType().GetProperty("HasReachedExit").GetValue(course), Is.True);
            Assert.That((int)run.GetType().GetProperty("Checkpoint").GetValue(run), Is.EqualTo(2));
            Assert.That(introductionTravel, Is.InRange(60f, 65f),
                "Uninterrupted basic traversal should establish the introduction pacing baseline.");
            Debug.Log("Scripted physics only: introduction " + introductionTravel.ToString("F2")
                + "s; whole preview " + traversal.ToString("F2") + "s. Not a player playtest.");
            var camera = course.GetComponentInChildren(RuntimeType("CatCamera"));
            // Script simulation does not update the interpolated render transform.
            cat.transform.position = body.position;
            camera.GetType().GetMethod("Snap").Invoke(camera, null);
            Call(camera, "LateUpdate");
            Assert.That(camera.transform.position.x, Is.EqualTo(508f - 5.5f * 16 / 9).Within(0.01f));
            course.GetType().GetMethod("TogglePause").Invoke(course, null);
            var stopped = body.position;
            Call(cat, "FixedUpdate"); Physics2D.Simulate(Time.fixedDeltaTime);
            Assert.That(body.position, Is.EqualTo(stopped));
            course.GetType().GetMethod("RetryCheckpoint").Invoke(course, null);
            Assert.That(body.position.x, Is.EqualTo(470).Within(0.001f));
            Assert.That((float)course.GetType().GetProperty("IntroductionSeconds").GetValue(course), Is.GreaterThanOrEqualTo(0));
            Assert.That(body.simulated, Is.True);
            Assert.That((bool)course.GetType().GetProperty("HasReachedExit").GetValue(course), Is.False);
            course.GetType().GetMethod("RestartRun").Invoke(course, null);
            Assert.That(body.position.x, Is.EqualTo(-3).Within(0.001f));
            Call(camera, "LateUpdate");
            Assert.That(camera.transform.position.x, Is.EqualTo(-5f + 5.5f * 16 / 9).Within(0.01f));
            Assert.That((float)course.GetType().GetProperty("IntroductionSeconds").GetValue(course), Is.EqualTo(-1));
            Assert.That((int)run.GetType().GetProperty("Checkpoint").GetValue(run), Is.Zero);
        } finally {
            Physics2D.simulationMode = oldMode;
        }
    }

    static void SettleCourse(Component cat, Component course)
    {
        Physics2D.SyncTransforms();
        for (int i = 0; i < 20; i++) {
            Call(cat, "FixedUpdate");
            Physics2D.Simulate(Time.fixedDeltaTime);
            Call(course, "Update");
        }
    }

    [UnityTest]
    public IEnumerator LevelOneUsesSeparateCheckpointsAndSignalsItsExitOnce()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        yield return new EnterPlayMode();
        host = new GameObject("Level one menu test");
        var screen = host.AddComponent(RuntimeType("StarterScreen"));
        yield return null;
        Call(screen, "OpenLevelOne");
        var course = (Component)Field(screen, "course");
        var cat = (Component)course.GetType().GetProperty("Cat").GetValue(course);
        var body = cat.GetComponent<Rigidbody2D>();
        var run = course.GetType().GetProperty("Run").GetValue(course);
        ((Behaviour)cat.GetComponent(RuntimeType("CatInput"))).enabled = false;
        exitSignals = 0;
        course.GetType().GetEvent("ExitReached").AddEventHandler(course, (Action)CountExit);
        var oldMode = Physics2D.simulationMode;
        Physics2D.simulationMode = SimulationMode2D.Script;
        try {
            float introductionTime = -1;
            foreach (var point in new[] { new Vector2(420, 0.52f), new Vector2(470, 0.52f) }) {
                run.GetType().GetMethod("Advance").Invoke(run, new object[] { 3f });
                cat.GetType().GetMethod("RespawnAt").Invoke(cat, new object[] { point });
                SettleCourse(cat, course);
                float recorded = (float)course.GetType().GetProperty("IntroductionSeconds").GetValue(course);
                if (introductionTime < 0) introductionTime = recorded;
                Assert.That(recorded, Is.EqualTo(introductionTime), "Later checkpoints must not overwrite the intro time.");
            }
            Assert.That(introductionTime, Is.GreaterThanOrEqualTo(3));
            Assert.That((int)run.GetType().GetProperty("Checkpoint").GetValue(run), Is.EqualTo(2));
            cat.GetType().GetMethod("RespawnAt").Invoke(cat, new object[] { new Vector2(503, 0.52f) });
            course.GetType().GetMethod("TogglePause").Invoke(course, null);
            SettleCourse(cat, course);
            Assert.That(exitSignals, Is.Zero, "Paused contact must not complete the route.");
            course.GetType().GetMethod("TogglePause").Invoke(course, null);
            SettleCourse(cat, course);
            SettleCourse(cat, course);
            Assert.That(exitSignals, Is.EqualTo(1));
            course.GetType().GetMethod("RetryCheckpoint").Invoke(course, null);
            Assert.That(body.position.x, Is.EqualTo(470).Within(0.001f));
            cat.GetType().GetMethod("RespawnAt").Invoke(cat, new object[] { new Vector2(503, 0.52f) });
            SettleCourse(cat, course);
            Assert.That(exitSignals, Is.EqualTo(2), "Retry rearms the transient exit hook.");
            course.GetType().GetMethod("RestartRun").Invoke(course, null);
            Assert.That((int)run.GetType().GetProperty("Checkpoint").GetValue(run), Is.Zero);
        } finally {
            Physics2D.simulationMode = oldMode;
        }
        Call(screen, "CloseCourse");
        UnityEngine.Object.Destroy(host);
        yield return new ExitPlayMode();
    }

    [UnityTest]
    public IEnumerator MenuRoundTripPreservesUpdatesAndDisablesTheOldCourse()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        yield return new EnterPlayMode();
        host = new GameObject("Menu integration test");
        var screen = host.AddComponent(RuntimeType("StarterScreen"));
        yield return null;
        var platform = Field(screen, "platform");
        Assert.That(platform, Is.Not.Null);
        Call(screen, "OpenCourse");
        var course = (Component)Field(screen, "course");
        Assert.That(course, Is.Not.Null);
        var scenery = SceneryResources(course);
        var sourceArt = Resources.LoadAll<UnityEngine.Object>("BackgroundStudy");
        Call(screen, "OpenCourse");
        Assert.That(Field(screen, "course"), Is.SameAs(course), "Repeated entry must not create another course.");
        var cat = (Component)course.GetType().GetProperty("Cat").GetValue(course);
        var spawnX = cat.GetComponent<Rigidbody2D>().position.x;
        var input = cat.GetComponent(RuntimeType("CatInput"));
        // Headless Editor focus varies. Explicitly exercise foreground, loss, and recovery.
        input.SendMessage("OnApplicationFocus", true);
        var actions = input.GetType().GetProperty("Actions").GetValue(input);
        Assert.That(actions.GetType().GetProperty("enabled").GetValue(actions), Is.True);
        var motion = cat.GetType().GetField("Motion").GetValue(cat);
        motion.GetType().GetMethod("SetMove").Invoke(motion, new object[] { 1f });
        motion.GetType().GetMethod("PressJump").Invoke(motion, null);
        input.SendMessage("OnApplicationFocus", false);
        Assert.That(actions.GetType().GetProperty("enabled").GetValue(actions), Is.False);
        var afterFocusLoss = (Vector2)motion.GetType().GetMethod("Step").Invoke(motion,
            new object[] { Time.fixedDeltaTime, true, Vector2.zero });
        Assert.That(afterFocusLoss.x, Is.Zero, "Focus loss must clear movement input.");
        Assert.That((bool)motion.GetType().GetProperty("Jumped").GetValue(motion), Is.False,
            "Focus loss must clear buffered jumps.");
        input.SendMessage("OnApplicationFocus", true);
        Assert.That(actions.GetType().GetProperty("enabled").GetValue(actions), Is.True);
        // Movement is verified separately with scripted physics in CatCollisionTests.
        Assert.That(course.GetComponentInChildren<Camera>(), Is.Not.Null);
        var run = course.GetType().GetProperty("Run").GetValue(course);
        var checkpoint = run.GetType().GetMethod("TryActivateCheckpoint");
        Assert.That((bool)checkpoint.Invoke(run, new object[] { 1, new Vector2(9, 0.52f), true }), Is.True);
        course.GetType().GetMethod("RetryCheckpoint").Invoke(course, null);
        var catBody = cat.GetComponent<Rigidbody2D>();
        Assert.That(catBody.position.x, Is.EqualTo(9).Within(0.001f));
        Assert.That((int)run.GetType().GetProperty("Retries").GetValue(run), Is.EqualTo(1));

        // An actual fall in FixedUpdate uses the same checkpoint path as manual retry.
        catBody.position = new Vector2(9, -8);
        cat.GetType().GetMethod("FixedUpdate", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(cat, null);
        Assert.That(catBody.position.x, Is.EqualTo(9).Within(0.001f));
        Assert.That(catBody.position.y, Is.EqualTo(0.52f).Within(0.001f));
        Assert.That((int)run.GetType().GetProperty("Retries").GetValue(run), Is.EqualTo(2));

        course.GetType().GetMethod("TogglePause").Invoke(course, null);
        Assert.That(catBody.simulated, Is.False);
        course.GetType().GetMethod("RestartRun").Invoke(course, null);
        Assert.That(catBody.simulated, Is.True);
        Assert.That(catBody.position.x, Is.EqualTo(spawnX).Within(0.001f));
        Assert.That((int)run.GetType().GetProperty("Checkpoint").GetValue(run), Is.Zero);
        Assert.That((int)run.GetType().GetProperty("Retries").GetValue(run), Is.Zero);

        ((ConcurrentQueue<string>)Field(screen, "messages")).Enqueue("{\"state\":\"warning\",\"message\":\"Preserve this notice\"}");
        yield return null;
        Assert.That(Field(screen, "warning"), Is.EqualTo("Preserve this notice"));
        ((Action)course.GetType().GetField("BackToMenu").GetValue(course))();
        Assert.That(course.gameObject.activeSelf, Is.False, "Gameplay must stop before deferred destruction.");
        Assert.That(actions.GetType().GetProperty("enabled").GetValue(actions), Is.False);
        Assert.That(Field(screen, "course"), Is.Null);
        Assert.That(Field(screen, "page"), Is.EqualTo("menu"));
        Assert.That(Field(screen, "platform"), Is.SameAs(platform));
        Assert.That(Field(screen, "warning"), Is.EqualTo("Preserve this notice"));
        yield return null;
        Assert.That(course == null, Is.True);
        yield return null; // Also allow resources queued by OnDestroy to finish destruction.
        Assert.That(scenery.All(o => !o), Is.True, "Closing the course must release every generated scenery resource.");
        Assert.That(sourceArt.All(o => o), Is.True, "Imported artwork/materials must survive closing.");
        Call(screen, "OpenCourse");
        var next = (Component)Field(screen, "course");
        Assert.That(next, Is.Not.Null);
        Assert.That(next, Is.Not.SameAs(course), "Re-entry must create a new course.");
        var nextScenery = SceneryResources(next);
        var nextCat = (Component)next.GetType().GetProperty("Cat").GetValue(next);
        Assert.That(nextCat.GetComponent<Rigidbody2D>().position.x, Is.EqualTo(spawnX).Within(0.01f),
            "Re-entry must start at the original spawn.");
        Call(screen, "CloseCourse");
        yield return null;
        yield return null;
        Assert.That(nextScenery.All(o => !o), Is.True, "Reopened scenery must clean up too.");
        Assert.That(sourceArt.All(o => o), Is.True);
    }

    [UnityTest]
    public IEnumerator CombatMenuRoundTripPreservesMovementLabAndStopsAudio()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        yield return new EnterPlayMode();
        host = new GameObject("Combat menu integration test");
        var screen = host.AddComponent(RuntimeType("StarterScreen"));
        yield return null;
        Call(screen, "OpenStompArena");
        var arena = (Component)Field(screen, "stompArena");
        Assert.That(arena, Is.Not.Null);
        Call(screen, "OpenStompArena");
        Assert.That(Field(screen, "stompArena"), Is.SameAs(arena));
        Call(screen, "OpenCourse");
        Assert.That(Field(screen, "course"), Is.Null, "Only one lab may own gameplay input.");
        var cat = (Component)arena.GetType().GetProperty("Cat").GetValue(arena);
        var audio = cat.GetComponent<AudioSource>();
        audio.Play();
        var input = cat.GetComponent(RuntimeType("CatInput"));
        ((Action)arena.GetType().GetField("BackToMenu").GetValue(arena))();
        Assert.That(arena.gameObject.activeSelf, Is.False);
        Assert.That(audio.isPlaying, Is.False);
        var actions = input.GetType().GetProperty("Actions").GetValue(input);
        Assert.That(actions.GetType().GetProperty("enabled").GetValue(actions), Is.False);
        Assert.That(Field(screen, "page"), Is.EqualTo("menu"));
        Assert.That(Field(screen, "stompArena"), Is.Null);
        yield return null;
        Assert.That(arena == null, Is.True);
        Call(screen, "OpenCourse");
        var course = (Component)Field(screen, "course");
        Assert.That(course, Is.Not.Null);
        Assert.That(course.GetComponentInChildren(RuntimeType("SceneryRenderer")), Is.Not.Null);
        Call(screen, "OpenStompArena");
        Assert.That(Field(screen, "stompArena"), Is.Null);
        ((Action)course.GetType().GetField("BackToMenu").GetValue(course))();
        yield return null;
        Call(screen, "OpenStompArena");
        var reopened = (Component)Field(screen, "stompArena");
        var combat = reopened.GetType().GetField("Combat").GetValue(reopened);
        Assert.That(combat.GetType().GetProperty("PlayerHealth").GetValue(combat), Is.EqualTo(3));
        Assert.That(combat.GetType().GetProperty("BossHealth").GetValue(combat), Is.EqualTo(3));
    }

    [UnityTearDown]
    public IEnumerator Cleanup()
    {
        if (EditorApplication.isPlaying) {
            if (host != null) UnityEngine.Object.Destroy(host);
            yield return null;
            yield return new ExitPlayMode();
        }
    }
}
