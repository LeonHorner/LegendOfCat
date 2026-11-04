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

    static string[] BoundTextureNames(MeshFilter[] meshes)
    {
        var names = new System.Collections.Generic.HashSet<string>();
        foreach (var mesh in meshes) {
            var block = new MaterialPropertyBlock();
            mesh.GetComponent<MeshRenderer>().GetPropertyBlock(block);
            Assert.That(block.GetTexture("_MainTex"), Is.Not.Null);
            names.Add(block.GetTexture("_MainTex").name);
        }
        return names.ToArray();
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
        var roots = course.GetComponentsInChildren<MeshFilter>().Where(f => f.name.StartsWith("Root island")).ToArray();
        var masonry = course.GetComponentsInChildren<MeshFilter>().Where(f => f.name.StartsWith("Masonry shelf")).ToArray();
        Assert.That(roots.Length, Is.GreaterThan(0));
        Assert.That(masonry.Length, Is.GreaterThan(0));
        var bankArt = course.GetComponentsInChildren<MeshFilter>().Where(f => f.name == "Painted grounded bank").ToArray();
        var rootArtNames = BoundTextureNames(roots);
        var masonryArtNames = BoundTextureNames(masonry);
        Assert.That(rootArtNames, Is.EquivalentTo(new[] { "shelf-root", "shelf-root-compact-v3" }));
        Assert.That(masonryArtNames, Is.EquivalentTo(new[] { "shelf-masonry", "shelf-masonry-compact-v3", "shelf-ascent-masonry-v1" }));
        Assert.That(BoundTextureNames(bankArt), Is.EquivalentTo(new[] { "bank-narrow-v3", "bank-medium-v3", "bank-wide-v3", "bank-tall-ascent-v1" }));
        var panoramas = course.GetComponentsInChildren<Transform>().Where(t => t.name.StartsWith("Continuous ")).ToArray();
        var waterTiles = course.GetComponentsInChildren<MeshFilter>().Where(t => t.name.StartsWith("Foreground water tile ")).ToArray();
        Assert.That(waterTiles, Has.Length.EqualTo(3));
        Assert.That(waterTiles[0].GetComponent<Renderer>().sharedMaterial.mainTexture.name, Is.EqualTo("river-water-v2"));
        foreach (var bank in bankArt) {
            var properties = new MaterialPropertyBlock(); bank.GetComponent<MeshRenderer>().GetPropertyBlock(properties);
            var source = (Texture2D)properties.GetTexture("_MainTex");
            if (source.name != "bank-tall-ascent-v1") continue;
            var bounds = bank.sharedMesh.bounds;
            Assert.That(bounds.size.x / bounds.size.y, Is.EqualTo((float)source.width / source.height).Within(0.001f));
            Assert.That(bounds.min.y, Is.LessThan(-4.5f), "Tall cliff feet must extend below the river.");
        }
        var rocks = course.GetComponentsInChildren<MeshFilter>().Where(t => t.name.StartsWith("Unique river rock group ")).ToArray();
        Assert.That(rocks, Has.Length.EqualTo(18));
        Assert.That(rocks.Select(t => (BoundTextureNames(new[] { t })[0], t.sharedMesh.uv[0])).Distinct().Count(), Is.EqualTo(18),
            "Each rock painting must appear once, independently of recycled wave tiles.");
        foreach (var rock in rocks) Assert.That(rock.GetComponents<Collider2D>(), Is.Empty);
        var waterPixels = new Texture2D(2, 2);
        try {
            waterPixels.LoadImage(System.IO.File.ReadAllBytes(AssetDatabase.GetAssetPath(waterTiles[0].GetComponent<Renderer>().sharedMaterial.mainTexture)));
            var pixels = waterPixels.GetPixels32();
            float topV = waterTiles[0].sharedMesh.uv.Max(uv => uv.y);
            int maxExcludedAlpha = 0;
            for (int row = Mathf.CeilToInt(topV * waterPixels.height) - 2; row < waterPixels.height; row++)
                for (int column = 0; column < waterPixels.width; column++)
                    maxExcludedAlpha = Mathf.Max(maxExcludedAlpha, pixels[row * waterPixels.width + column].a);
            Assert.That(maxExcludedAlpha, Is.LessThan(128),
                "The route water mesh must not slice through visible alpha into a rectangular upper edge.");
        } finally { UnityEngine.Object.Destroy(waterPixels); }
        Assert.That(panoramas, Has.Length.EqualTo(2));
        var panoramaApply = scenery.GetType().GetMethod("ApplyView");
        foreach (float x in new[] { -7.2f, -3f, 84f, 216f, 342f, 503f, 509.2f, 700f, 900f, 1067f, 84f }) {
            panoramaApply.Invoke(scenery, new object[] { x, 1f });
            foreach (var panorama in panoramas) {
                Assert.That(panorama.localScale.x, Is.GreaterThan(0), "No panorama mirroring.");
                Assert.That(panorama.parent.childCount, Is.EqualTo(1), "No duplicated/recycled panorama panels.");
                var bounds = panorama.GetComponent<MeshRenderer>().bounds;
                Assert.That(bounds.min.x, Is.LessThanOrEqualTo(x - 5.5f * 16 / 9));
                Assert.That(bounds.max.x, Is.GreaterThanOrEqualTo(x + 5.5f * 16 / 9));
                Assert.That(panorama.localScale.x / panorama.localScale.y, Is.EqualTo(6).Within(0.001f));
            }
        }
        panoramaApply.Invoke(scenery, new object[] { 700f, 10f });
        var beforeTravel = panoramas.Select(p => p.position.x).ToArray();
        panoramaApply.Invoke(scenery, new object[] { 710f, 10f });
        for (int i = 0; i < panoramas.Length; i++) {
            float travel = 10 - (panoramas[i].position.x - beforeTravel[i]);
            Assert.That(travel, Is.GreaterThan(0.7f), "Late scenery must keep perceptibly moving.");
        }
        var floatingIndices = (int[])course.GetType().GetField("LevelOneFloatingPlatforms").GetValue(null);
        var rootIndices = (int[])course.GetType().GetField("LevelOneRootPlatforms").GetValue(null);
        var geometry = (Vector3[])course.GetType().GetField("CompletePlatforms").GetValue(null);
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
        foreach (var landmark in landmarks) {
            var properties = new MaterialPropertyBlock();
            landmark.GetComponent<MeshRenderer>().GetPropertyBlock(properties);
            Assert.That(properties.GetTexture("_MainTex").name, Does.EndWith("-hill-v2"));
            Assert.That(landmark.GetComponent<MeshRenderer>().bounds.min.y, Is.LessThan(-8),
                "The hillside's solid bottom belongs below the viewport, not inside a platform gap.");
        }
        var apply = scenery.GetType().GetMethod("ApplyView");
        apply.Invoke(scenery, new object[] { 84f, 1f });
        var positions = landmarks.Select(t => t.localPosition).ToArray();
        apply.Invoke(scenery, new object[] { 94f, 1f });
        var screenTravel = new float[landmarks.Length];
        for (int i = 0; i < landmarks.Length; i++)
            screenTravel[i] = 10 - (landmarks[i].localPosition.x - positions[i].x);
        Assert.That(screenTravel[0], Is.GreaterThan(screenTravel[2]));
        Assert.That(screenTravel[2], Is.GreaterThan(screenTravel[1]));
        foreach (float x in new[] { 1000f, -200f, 342f, 84f }) apply.Invoke(scenery, new object[] { x, 1f });
        Assert.That(landmarks.Select(t => t.localPosition).ToArray(), Is.EqualTo(positions),
            "Landmarks must retain their unique anchors after travel, reversal and teleport.");
        var platforms = course.GetComponentsInChildren<BoxCollider2D>().Where(c => c.name == "Standable platform").ToArray();
        Assert.That(platforms.Max(c => c.transform.position.y) - platforms.Min(c => c.transform.position.y), Is.GreaterThan(1.5f));
        Assert.That(platforms.Select(c => c.transform.localScale.x).Distinct().Count(), Is.GreaterThanOrEqualTo(8));
    }

    [UnityTest]
    public IEnumerator EasterEggsOccurOnceAcrossSectionsAndFishDriftWithPause()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        yield return new EnterPlayMode();
        host = new GameObject("Easter egg behavior test");
        var screen = host.AddComponent(RuntimeType("StarterScreen"));
        yield return null;
        Call(screen, "OpenLevelOne");
        var course = (Component)Field(screen, "course");
        var scenery = course.GetComponentInChildren(RuntimeType("SceneryRenderer"));
        var eggs = course.GetComponentsInChildren<Transform>().Where(t => t.name.StartsWith("Easter egg ")).ToArray();
        Assert.That(eggs, Has.Length.EqualTo(3));
        Assert.That(eggs.Select(t => t.name).Distinct().Count(), Is.EqualTo(3));
        foreach (var egg in eggs) Assert.That(egg.GetComponentsInChildren<Collider2D>(), Is.Empty);
        var sign = eggs.First(t => t.name == "Easter egg no cats sign");
        Assert.That(sign.localPosition.x, Is.GreaterThan(420));
        var fish = eggs.First(t => t.name == "Easter egg watching fish");
        Assert.That(fish.childCount, Is.EqualTo(2));
        var capture = scenery.GetType().GetMethod("SetWaterTime");
        capture.Invoke(scenery, new object[] { -1f });
        var before = fish.GetChild(0).localPosition;
        for (int tick = 0; tick < 30; tick++) Call(scenery, "FixedUpdate");
        Assert.That(Vector3.Distance(before, fish.GetChild(0).localPosition), Is.GreaterThan(0.01f));
        Assert.That(fish.GetChild(0).localPosition.y, Is.Not.EqualTo(fish.GetChild(1).localPosition.y));
        Assert.That(fish.localPosition.y, Is.EqualTo(-3.30f));
        var fishProperties = new MaterialPropertyBlock();
        fish.GetChild(0).GetComponent<Renderer>().GetPropertyBlock(fishProperties);
        Assert.That(fishProperties.GetFloat("_Submerged"), Is.EqualTo(1), "Water must attenuate the submerged fish paint.");
        Assert.That(Mathf.Abs(fish.localPosition.x - 96), Is.LessThanOrEqualTo(0.18f));
        var cat = (Component)scenery.GetType().GetProperty("Cat").GetValue(scenery);
        var originalPosition = cat.transform.position;
        cat.transform.position = new Vector3(99, 0, 0);
        capture.Invoke(scenery, new object[] { 0f });
        Assert.That(fish.GetChild(0).localScale.x, Is.LessThan(0));
        Assert.That(fish.GetChild(1).localScale.x, Is.LessThan(0));
        cat.transform.position = new Vector3(93, 0, 0);
        capture.Invoke(scenery, new object[] { 0f });
        Assert.That(fish.GetChild(0).localScale.x, Is.GreaterThan(0));
        Assert.That(fish.GetChild(1).localScale.x, Is.GreaterThan(0));
        cat.transform.position = new Vector3(120, 0, 0);
        capture.Invoke(scenery, new object[] { 0f });
        Assert.That(fish.GetChild(0).localScale.x, Is.GreaterThan(0), "Distant Mika must not attract their attention.");
        capture.Invoke(scenery, new object[] { 1.8f });
        float downstream = fish.GetChild(0).localPosition.x;
        capture.Invoke(scenery, new object[] { 2.3f });
        Assert.That(fish.GetChild(0).localPosition.x, Is.LessThan(downstream - 0.08f), "A swimming stroke must recover the downstream drift.");
        cat.transform.position = originalPosition;
        capture.Invoke(scenery, new object[] { -1f });
        course.GetType().GetMethod("TogglePause").Invoke(course, null);
        var paused = fish.GetChild(0).localPosition;
        for (int tick = 0; tick < 30; tick++) Call(scenery, "FixedUpdate");
        Assert.That(fish.GetChild(0).localPosition, Is.EqualTo(paused));
        course.GetType().GetMethod("RetryCheckpoint").Invoke(course, null);
        course.GetType().GetMethod("RestartRun").Invoke(course, null);
        Assert.That(course.GetComponentsInChildren<Transform>().Count(t => t.name.StartsWith("Easter egg ")), Is.EqualTo(3));
        foreach (float x in new[] { 503f, -3f, 239f })
            scenery.GetType().GetMethod("ApplyView").Invoke(scenery, new object[] { x, 1f });
        Assert.That(sign.localPosition.x, Is.EqualTo(((Vector2[])course.GetType().GetField("LevelOneCheckpointSpawns").GetValue(null))[1].x - 3));
        var king = eggs.First(t => t.name == "Easter egg cat king");
        Assert.That(king.GetComponentsInChildren<Renderer>(), Is.Empty,
            "The stone king must be part of the valley painting, never a separate floating cutout.");
        float kingHeight = king.localPosition.y;
        scenery.GetType().GetMethod("ApplyView").Invoke(scenery, new object[] { 239f, 2f });
        Assert.That(king.localPosition.y, Is.EqualTo(kingHeight + 1).Within(0.001f),
            "The statue must retain its registration with the vertically placed valley layer.");
    }

    [UnityTest]
    public IEnumerator LevelOneCameraShowsBothTerminalCliffEdges()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        yield return new EnterPlayMode();
        host = new GameObject("Route boundary camera test");
        var screen = host.AddComponent(RuntimeType("StarterScreen"));
        yield return null;
        Call(screen, "OpenLevelOne");
        var course = (Component)Field(screen, "course");
        var cat = (Component)course.GetType().GetProperty("Cat").GetValue(course);
        var follow = course.GetComponentInChildren(RuntimeType("CatCamera"));
        var camera = follow.GetComponent<Camera>();
        foreach (float edge in new[] { -5f, 507f }) {
            cat.transform.position = course.transform.TransformPoint(new Vector3(edge, 0.52f));
            follow.GetType().GetMethod("Snap").Invoke(follow, null);
            Call(follow, "LateUpdate");
            float visibleEdge = camera.WorldToViewportPoint(course.transform.TransformPoint(new Vector3(edge, 0))).x;
            Assert.That(visibleEdge, Is.InRange(0.2f, 0.8f),
                "The camera must show scenery beyond the final standable edge, rather than hide the end at the screen boundary.");
        }
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
        var platforms = (Vector3[])((Vector3[])course.GetType().GetField("CompletePlatforms").GetValue(null)).Clone();
        var movers = course.GetComponentsInChildren(RuntimeType("MovingSupport"));
        foreach (var mover in movers) Call(mover, "Start");
        var run = course.GetType().GetProperty("Run").GetValue(course);
        var oldMode = Physics2D.simulationMode;
        Physics2D.simulationMode = SimulationMode2D.Script;
        try {
            Physics2D.SyncTransforms();
            int next = 0;
            float introductionTravel = -1, traversal = 0;
            for (int tick = 0; tick < 15000; tick++) {
                foreach (var mover in movers) {
                    Call(mover, "FixedUpdate");
                    int movingIndex = Array.FindIndex(platforms, p => Mathf.Abs(p.x - mover.transform.position.x) < 2 && Mathf.Abs(p.z - mover.transform.localScale.x) < 0.01f);
                    if (movingIndex >= 0) platforms[movingIndex] = new Vector3(mover.transform.position.x, mover.transform.position.y, mover.transform.localScale.x);
                }
                bool grounded = (bool)cat.GetType().GetProperty("Grounded").GetValue(cat);
                while (next < platforms.Length - 1 && body.position.x > platforms[next].x + platforms[next].z / 2)
                    next++;
                motion.GetType().GetMethod("SetMove").Invoke(motion, new object[] { !grounded && next >= 43 && body.position.x >= platforms[next].x && body.position.x < platforms[next].x + platforms[next].z / 2 - 1 ? 0f : 1f });
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
            Assert.That((bool)course.GetType().GetProperty("HasReachedExit").GetValue(course), Is.True, "Stopped at " + body.position + " next=" + next);
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
            Assert.That(camera.transform.position.x, Is.EqualTo(body.position.x + 2).Within(0.01f),
                "The exit margin must let the camera keep following until the terminal cliff is visible.");
            course.GetType().GetMethod("TogglePause").Invoke(course, null);
            var stopped = body.position;
            Call(cat, "FixedUpdate"); Physics2D.Simulate(Time.fixedDeltaTime);
            Assert.That(body.position, Is.EqualTo(stopped));
            course.GetType().GetMethod("RetryCheckpoint").Invoke(course, null);
            Assert.That(body.position.x, Is.EqualTo(((Vector2[])course.GetType().GetField("LevelOneCheckpointSpawns").GetValue(null))[1].x).Within(0.001f));
            Assert.That((float)course.GetType().GetProperty("IntroductionSeconds").GetValue(course), Is.GreaterThanOrEqualTo(0));
            Assert.That(body.simulated, Is.True);
            Assert.That((bool)course.GetType().GetProperty("HasReachedExit").GetValue(course), Is.False);
            course.GetType().GetMethod("RestartRun").Invoke(course, null);
            Assert.That(body.position.x, Is.EqualTo(-3).Within(0.001f));
            Call(camera, "LateUpdate");
            Assert.That(camera.transform.position.x, Is.EqualTo(body.position.x + 2).Within(0.01f),
                "Restart must preserve the scenery margin behind the starting bank.");
            Assert.That((float)course.GetType().GetProperty("IntroductionSeconds").GetValue(course), Is.EqualTo(-1));
            Assert.That((int)run.GetType().GetProperty("Checkpoint").GetValue(run), Is.Zero);
        } finally {
            Physics2D.simulationMode = oldMode;
        }
    }

    [UnityTest]
    public IEnumerator AscentSupportsCarryPauseAndResetWithHeightCamera()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        yield return new EnterPlayMode();
        host = new GameObject("Ascent support review");
        var screen = host.AddComponent(RuntimeType("StarterScreen"));
        yield return null;
        Call(screen, "OpenLevelOne");
        var course = (Component)Field(screen, "course");
        var cat = (Component)course.GetType().GetProperty("Cat").GetValue(course);
        ((Behaviour)cat.GetComponent(RuntimeType("CatInput"))).enabled = false;
        var supports = course.GetComponentsInChildren(RuntimeType("MovingSupport"));
        Assert.That(supports.Length, Is.EqualTo(2));
        Assert.That(course.GetComponentsInChildren<PlatformEffector2D>(), Is.Empty, "Course platforms must be solid from below.");
        yield return null;
        var previousMode = Physics2D.simulationMode;
        Physics2D.simulationMode = SimulationMode2D.Script;
        try {
            int sample = 0;
            foreach (var support in supports.Concat(supports)) {
                support.GetType().GetMethod("ResetPhase").Invoke(support, null);
                var origin = support.transform.position;
                if (sample++ >= 2)
                    for (int tick = 0; tick < 125; tick++) Call(support, "FixedUpdate");
                cat.GetType().GetMethod("RespawnAt").Invoke(cat, new object[] { (Vector2)support.transform.position + Vector2.up * 1.22f });
                SettleCourse(cat, course);
                var body = cat.GetComponent<Rigidbody2D>();
                var initialOffset = body.position - (Vector2)support.transform.position;
                for (int tick = 0; tick < 60; tick++) {
                    Call(support, "FixedUpdate"); Call(cat, "FixedUpdate"); Physics2D.Simulate(Time.fixedDeltaTime);
                }
                Assert.That(Vector2.Distance(body.position - (Vector2)support.transform.position, initialOffset), Is.LessThan(0.12f));
                cat.GetType().GetMethod("PressJump").Invoke(cat, null);
                Call(support, "FixedUpdate"); Call(cat, "FixedUpdate"); Physics2D.Simulate(Time.fixedDeltaTime);
                Assert.That(body.linearVelocity.y, Is.GreaterThan(10));
                Assert.That(Mathf.Abs(body.linearVelocity.x), Is.LessThan(0.01f));
                course.GetType().GetMethod("TogglePause").Invoke(course, null);
                var stopped = support.transform.position;
                Call(support, "FixedUpdate");
                Assert.That(support.transform.position, Is.EqualTo(stopped));
                course.GetType().GetMethod("RetryCheckpoint").Invoke(course, null);
                Assert.That(support.transform.position, Is.EqualTo(origin));
            }
            var ascent = (Vector3[])course.GetType().GetField("AscentPlatforms").GetValue(null);
            var motor = cat.GetType().GetField("Motion").GetValue(cat);
            foreach (int group in new[] { 4, 10, 13 }) {
                var bank = ascent[group * 5]; var shelf = ascent[group * 5 + 1];
                cat.GetType().GetMethod("RespawnAt").Invoke(cat, new object[] { new Vector2(shelf.x, bank.y - 0.29f) });
                SettleCourse(cat, course);
                var body = cat.GetComponent<Rigidbody2D>();
                bool jumped = false, recovered = false;
                for (int tick = 0; tick < 180; tick++) {
                    motor.GetType().GetMethod("SetMove").Invoke(motor, new object[] { body.position.x > bank.x ? -1f : 0f });
                    if (!jumped && body.position.x <= bank.x + bank.z / 2 + 1.1f) {
                        cat.GetType().GetMethod("PressJump").Invoke(cat, null); jumped = true;
                    }
                    Call(cat, "FixedUpdate"); Physics2D.Simulate(Time.fixedDeltaTime);
                    if (jumped && (bool)cat.GetType().GetProperty("Grounded").GetValue(cat) &&
                        body.position.y > bank.y + 1.15f) { recovered = true; break; }
                }
                Assert.That(recovered, Is.True, "Recovery terrace must permit a short return in group " + group);
            }
            cat.transform.localPosition = new Vector3(900, 14);
            var follow = course.GetComponentInChildren(RuntimeType("CatCamera"));
            follow.GetType().GetMethod("Snap").Invoke(follow, null); Call(follow, "LateUpdate");
            Assert.That(follow.transform.position.y, Is.GreaterThan(12));
            course.GetType().GetMethod("RestartRun").Invoke(course, null);
            Call(follow, "LateUpdate");
            Assert.That(follow.transform.position.y, Is.EqualTo(1).Within(0.01f));
        } finally { Physics2D.simulationMode = previousMode; }
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
            foreach (var point in (Vector2[])course.GetType().GetField("LevelOneCheckpointSpawns").GetValue(null)) {
                run.GetType().GetMethod("Advance").Invoke(run, new object[] { 3f });
                cat.GetType().GetMethod("RespawnAt").Invoke(cat, new object[] { point });
                SettleCourse(cat, course);
                float recorded = (float)course.GetType().GetProperty("IntroductionSeconds").GetValue(course);
                if (introductionTime < 0) introductionTime = recorded;
                Assert.That(recorded, Is.EqualTo(introductionTime), "Later checkpoints must not overwrite the intro time.");
            }
            Assert.That(introductionTime, Is.GreaterThanOrEqualTo(3));
            Assert.That((int)run.GetType().GetProperty("Checkpoint").GetValue(run), Is.EqualTo(2));
            cat.GetType().GetMethod("RespawnAt").Invoke(cat, new object[] { (Vector2)course.GetType().GetField("LevelOneExit").GetValue(null) });
            course.GetType().GetMethod("TogglePause").Invoke(course, null);
            SettleCourse(cat, course);
            Assert.That(exitSignals, Is.Zero, "Paused contact must not complete the route.");
            course.GetType().GetMethod("TogglePause").Invoke(course, null);
            SettleCourse(cat, course);
            SettleCourse(cat, course);
            Assert.That(exitSignals, Is.EqualTo(1));
            course.GetType().GetMethod("RetryCheckpoint").Invoke(course, null);
            Assert.That(body.position.x, Is.EqualTo(((Vector2[])course.GetType().GetField("LevelOneCheckpointSpawns").GetValue(null))[1].x).Within(0.001f));
            cat.GetType().GetMethod("RespawnAt").Invoke(cat, new object[] { (Vector2)course.GetType().GetField("LevelOneExit").GetValue(null) });
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
