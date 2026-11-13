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

    [UnityTest]
    public IEnumerator PaintedCheckpointsRequireStoneContactAndKeepOnlyLatestLight()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        yield return new EnterPlayMode();
        var root = new GameObject("Checkpoint passage regression"); root.SetActive(false);
        var course = root.AddComponent(RuntimeType("MovementCourse"));
        course.GetType().GetField("LevelOne").SetValue(course, true);
        root.SetActive(true);
        var cat = (Component)course.GetType().GetProperty("Cat").GetValue(course);
        var run = course.GetType().GetProperty("Run").GetValue(course);
        var spawns = (Vector2[])course.GetType().GetField("LevelOneCheckpointSpawns").GetValue(null);
        var markers = root.GetComponentsInChildren(RuntimeType("CheckpointMarker"));
        void Refresh() { foreach (var marker in markers) marker.GetType().GetMethod("Refresh").Invoke(marker, new object[] { .1f }); }
        int State(int i) => (int)markers[i].GetType().GetProperty("State").GetValue(markers[i]);
        void Place(Vector2 point) {
            cat.GetType().GetMethod("RespawnAt").Invoke(cat, new object[] { point });
            Physics2D.SyncTransforms();
            Call(course, "Update"); Refresh();
        }
        void Pass(int i) => Place((Vector2)markers[i].transform.Find("Engraved paw").position + Vector2.up * .3f);
        Assert.That(State(0), Is.Zero); Assert.That(State(1), Is.Zero);
        Assert.That(markers[0].GetComponentInChildren<SpriteRenderer>().sprite.vertices, Has.Length.EqualTo(4),
            "Atlas state selection needs a full rectangle, not three disjoint tight sprite outlines.");
        Place(spawns[0] + new Vector2(-2.5f, 0));
        Assert.That(State(0), Is.Zero, "Approaching the tree must not activate its stone.");
        Place(spawns[0] + new Vector2(-.8f, 2.5f));
        Assert.That(State(0), Is.Zero, "Jumping entirely above the stone must not activate it.");
        course.GetType().GetMethod("TogglePause").Invoke(course, null);
        cat.GetComponent<Rigidbody2D>().position = spawns[0] + Vector2.up * 2;
        Call(course, "Update"); Refresh(); Assert.That(State(0), Is.Zero);
        course.GetType().GetMethod("TogglePause").Invoke(course, null);
        Pass(0); Assert.That(State(0), Is.EqualTo(1)); Assert.That(State(1), Is.Zero);
        var firstLight = markers[0].GetComponentInChildren<SpriteRenderer>().sharedMaterial;
        float initialLight = firstLight.GetFloat("_Active");
        Assert.That(initialLight, Is.InRange(.001f,.5f), "Touch must begin a gradual light ramp.");
        Assert.That(markers[0].transform.Find("Checkpoint sparks").GetComponent<MeshRenderer>().enabled, Is.True);
        markers[0].GetType().GetMethod("Refresh").Invoke(markers[0], new object[] { .4f });
        Assert.That(firstLight.GetFloat("_Active"), Is.GreaterThan(initialLight).And.LessThan(1));
        course.GetType().GetMethod("TogglePause").Invoke(course, null);
        float pausedLight = firstLight.GetFloat("_Active");
        var sparkMesh = markers[0].transform.Find("Checkpoint sparks").GetComponent<MeshFilter>().sharedMesh;
        var pausedSparks = sparkMesh.vertices;
        markers[0].GetType().GetMethod("Refresh").Invoke(markers[0], new object[] { .5f });
        Assert.That(firstLight.GetFloat("_Active"), Is.EqualTo(pausedLight));
        Assert.That(sparkMesh.vertices, Is.EqualTo(pausedSparks));
        course.GetType().GetMethod("TogglePause").Invoke(course, null);
        markers[0].GetType().GetMethod("Refresh").Invoke(markers[0], new object[] { 2f });
        Assert.That(firstLight.GetFloat("_Active"), Is.EqualTo(1));
        Assert.That(markers[0].transform.Find("Checkpoint sparks").GetComponent<MeshRenderer>().enabled, Is.False);
        Assert.That((bool)cat.GetType().GetProperty("Grounded").GetValue(cat), Is.False);
        course.GetType().GetMethod("RetryCheckpoint").Invoke(course, null);
        Refresh();
        Assert.That(markers[0].transform.Find("Checkpoint sparks").GetComponent<MeshRenderer>().enabled, Is.False,
            "Retry must not replay the checkpoint activation burst.");
        Assert.That(Vector2.Distance(cat.GetComponent<Rigidbody2D>().position, spawns[0]), Is.LessThan(.001f));
        Pass(1); Assert.That(State(0), Is.EqualTo(2)); Assert.That(State(1), Is.EqualTo(1));
        Assert.That(firstLight.GetFloat("_Active"), Is.Zero, "The distant passed marker changes directly without another effect.");
        Pass(0); Assert.That(State(0), Is.EqualTo(2)); Assert.That(State(1), Is.EqualTo(1));
        var material = markers[1].GetComponentInChildren<SpriteRenderer>().sharedMaterial;
        float pulse = material.GetFloat("_Pulse");
        markers[1].GetType().GetMethod("Refresh").Invoke(markers[1], new object[] { .8f });
        Assert.That(material.GetFloat("_Pulse"), Is.Not.EqualTo(pulse));
        course.GetType().GetMethod("TogglePause").Invoke(course, null);
        pulse = material.GetFloat("_Pulse");
        markers[1].GetType().GetMethod("Refresh").Invoke(markers[1], new object[] { .8f });
        Assert.That(material.GetFloat("_Pulse"), Is.EqualTo(pulse));
        course.GetType().GetMethod("RestartRun").Invoke(course, null); Refresh();
        Assert.That(State(0), Is.Zero); Assert.That(State(1), Is.Zero);
        Assert.That(markers[0].GetComponentsInChildren<Collider2D>().All(c => c.isTrigger), Is.True,
            "The stone's touch region must not block running or jumping.");
        Assert.That(markers[0].transform.Find("Checkpoint tree"), Is.Not.Null);
        Assert.That(markers[1].transform.Find("Checkpoint ruin arch"), Is.Not.Null);
        Assert.That(markers[1].transform.Find("Engraved paw").position.x,
            Is.EqualTo(markers[1].transform.Find("Checkpoint ruin arch").position.x).Within(.001f));
        Assert.That(root.GetComponentsInChildren<TextMesh>(), Is.Empty, "Main-route landmarks replace floating instructions.");
        UnityEngine.Object.Destroy(root);
        yield return new ExitPlayMode();
    }
    static void CountExit() => exitSignals++;
    static Type RuntimeType(string name) => AppDomain.CurrentDomain.GetAssemblies()
        .Select(a => a.GetType(name)).First(t => t != null);
    static object Field(object target, string name) => target.GetType()
        .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
    static void Call(object target, string name) => target.GetType()
        .GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Invoke(target, null);

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
        foreach (float x in new[] { -7.2f, -3f, 84f, 216f, 342f, 503f, 509.2f, 700f, 900f, 1067f, ((Vector2)course.GetType().GetField("LevelOneExit").GetValue(null)).x, 84f }) {
            panoramaApply.Invoke(scenery, new object[] { x, 1f });
            foreach (var panorama in panoramas) {
                Assert.That(panorama.localScale.x, Is.GreaterThan(0), "No panorama mirroring.");
                Assert.That(panorama.parent.childCount, Is.EqualTo(1), "No duplicated/recycled panorama panels.");
                var bounds = panorama.GetComponent<MeshRenderer>().bounds;
                Assert.That(bounds.min.x, Is.LessThanOrEqualTo(x - 5.5f * 16 / 9));
                Assert.That(bounds.max.x, Is.GreaterThanOrEqualTo(x + 5.5f * 16 / 9));
                Assert.That(panorama.localScale.x / panorama.localScale.y, Is.EqualTo(panorama.GetComponent<MeshRenderer>().sharedMaterial.mainTexture.width / (float)panorama.GetComponent<MeshRenderer>().sharedMaterial.mainTexture.height).Within(.01f));
            }
        }
        panoramaApply.Invoke(scenery, new object[] { 700f, 10f });
        var beforeTravel = panoramas.Select(p => p.position.x).ToArray();
        panoramaApply.Invoke(scenery, new object[] { 710f, 10f });
        for (int i = 0; i < panoramas.Length; i++) {
            float travel = 10 - (panoramas[i].position.x - beforeTravel[i]);
            Assert.That(travel, Is.GreaterThan(0.7f), "Late scenery must keep perceptibly moving.");
        }
        var ruin=course.GetComponentsInChildren<Transform>().Single(t=>t.name=="Unique crown hillside");
        float end=((Vector2)course.GetType().GetField("LevelOneExit").GetValue(null)).x;
        foreach(float x in new[]{1200f,1350f,1500f,1700f,end,1500f,1200f}) {
            panoramaApply.Invoke(scenery,new object[]{x,12f});float before=ruin.position.x;
            panoramaApply.Invoke(scenery,new object[]{x+10,12f});
            Assert.That(10-(ruin.position.x-before),Is.EqualTo(2.2f).Within(.002f),
                "The large finale ruin must keep visibly travelling through the route and on return.");
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
        var signBank = ((Vector3[])course.GetType().GetField("AscentPlatforms").GetValue(null))[44];
        Assert.That(sign.localPosition.x, Is.EqualTo(signBank.x + 1));
        Assert.That(sign.localPosition.y, Is.EqualTo(signBank.y + 1.79f).Within(.001f));
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
            float introductionTravel = -1, secondCheckpointTravel = -1, traversal = 0;
            FinalePhysicsPilot finalePilot = null;
            for (int tick = 0; tick < 25000; tick++) {
                foreach (var mover in movers) {
                    Call(mover, "FixedUpdate");
                    int movingIndex = Array.FindIndex(platforms, p => Mathf.Abs(p.x - mover.transform.position.x) < 2 && Mathf.Abs(p.z - mover.transform.localScale.x) < 0.01f);
                    if (movingIndex >= 0) platforms[movingIndex] = new Vector3(mover.transform.position.x, mover.transform.position.y, mover.transform.localScale.x);
                }
                if ((int)run.GetType().GetProperty("Checkpoint").GetValue(run) == 2 && finalePilot == null) finalePilot = new FinalePhysicsPilot(course);
                bool grounded = (bool)cat.GetType().GetProperty("Grounded").GetValue(cat);
                if (finalePilot != null) finalePilot.Command();
                else {
                while (next < platforms.Length - 1 && body.position.x > platforms[next].x + platforms[next].z / 2)
                    next++;
                float axis = !grounded && next >= 43 && body.position.x >= platforms[next].x && body.position.x < platforms[next].x + platforms[next].z / 2 - 1 ? 0f : 1f;
                bool waitForLift = next == 94 && grounded && platforms[next + 1].y - platforms[next].y > 1.8f;
                if (waitForLift && body.position.x > platforms[next].x + platforms[next].z / 2 - 1.5f)
                    axis = 0;
                motion.GetType().GetMethod("SetMove").Invoke(motion, new object[] { axis });
                if (grounded && !waitForLift && next < platforms.Length - 1 &&
                    body.position.x >= platforms[next].x + platforms[next].z / 2 - 0.7f)
                    cat.GetType().GetMethod("PressJump").Invoke(cat, null);
                }
                Call(cat, "FixedUpdate");
                Physics2D.Simulate(Time.fixedDeltaTime);
                Call(course, "Update");
                traversal = (tick + 1) * Time.fixedDeltaTime;
                if (introductionTravel < 0 && (int)run.GetType().GetProperty("Checkpoint").GetValue(run) >= 1)
                    introductionTravel = traversal;
                if(secondCheckpointTravel<0 && (int)run.GetType().GetProperty("Checkpoint").GetValue(run)>=2) secondCheckpointTravel=traversal;
                Assert.That((int)run.GetType().GetProperty("Retries").GetValue(run), Is.Zero,
                    "Failed on platform " + next + " at " + body.position);
                if ((bool)course.GetType().GetProperty("HasReachedExit").GetValue(course)) break;
            }
            Assert.That((bool)course.GetType().GetProperty("HasReachedExit").GetValue(course), Is.True, "Stopped at " + body.position + " next=" + next + " finale=" + finalePilot);
            Assert.That((int)run.GetType().GetProperty("Checkpoint").GetValue(run), Is.EqualTo(2));
            Assert.That(introductionTravel, Is.InRange(59f, 65f),
                "Uninterrupted basic traversal should establish the introduction pacing baseline.");
            Debug.Log("Scripted physics only: introduction " + introductionTravel.ToString("F2")
                + "s; whole preview " + traversal.ToString("F2") + "s; finale " + (traversal-secondCheckpointTravel).ToString("F2") + "s. Not a player playtest.");
            var exitPosition=body.position;
            CheckFinaleRecoveries(course,cat,body,motion);
            cat.GetType().GetMethod("RespawnAt").Invoke(cat,new object[]{exitPosition});
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

    static void CheckFinaleRecoveries(Component course,Component cat,Rigidbody2D body,object motor)
    {
        var type=RuntimeType("FinaleRoute");
        var route=(Vector3[])type.GetProperty("Platforms").GetValue(null);
        int courtyard=(int)type.GetProperty("CourtyardTurn").GetValue(null), crown=(int)type.GetProperty("CrownTurn").GetValue(null);
        Assert.That(route[courtyard+4].y-route[courtyard-1].y,Is.GreaterThan(2.4f),"Forward jumping must not bypass the courtyard stair.");
        Assert.That(route[crown+4].y-route[crown-1].y,Is.GreaterThan(2.4f),"Forward jumping must not bypass the crown stair.");
        int tower=(int)type.GetProperty("TowerTurn").GetValue(null);
        Assert.That(route[tower+4].y-route[tower-1].y,Is.GreaterThan(2.4f),"The collapsed tower flank must require the upper detour.");
        Assert.That(route[tower+1].x,Is.LessThan(route[tower].x),"The watchtower must contain an actual leftward turn.");
        var catches=(System.Collections.IList)type.GetProperty("Recoveries").GetValue(null);
        var movers=course.GetComponentsInChildren(RuntimeType("MovingSupport"));

            for(int i=0;i<catches.Count;i++) {
                var floor=course.GetComponentsInChildren<PolygonCollider2D>().Single(c=>c.name=="Finale recovery "+i);
                int returnIndex=(int)catches[i].GetType().GetProperty("ReturnPlatformIndex").GetValue(catches[i]);
                var landing=route[returnIndex];
                cat.GetType().GetMethod("RespawnAt").Invoke(cat,new object[]{new Vector2(floor.bounds.min.x+.7f,floor.bounds.max.y+.46f)});
                for(int step=0;step<8;step++){Call(cat,"FixedUpdate");Physics2D.Simulate(Time.fixedDeltaTime);}
                motor.GetType().GetMethod("SetMove").Invoke(motor,new object[]{-1f});Call(cat,"PressJump");
                bool reached=false;
                for(int step=0;step<180;step++) {
                    if(body.position.x<landing.x) motor.GetType().GetMethod("SetMove").Invoke(motor,new object[]{0f});
                    Call(cat,"FixedUpdate");Physics2D.Simulate(Time.fixedDeltaTime);
                    bool onRoof=Mathf.Abs(body.position.y-landing.y-1.2f)<.12f;
                    bool onRubble=course.GetComponentsInChildren<PolygonCollider2D>().Any(c=>c.name=="Solid fallen masonry" &&
                        c.bounds.min.x<landing.x+landing.z/2 && c.bounds.max.x>landing.x-landing.z/2 &&
                        Enumerable.Range(0,3).Any(j=>{
                            var feet=body.position+new Vector2((j-1)*.35f,-.45f);
                            return (c.ClosestPoint(feet)-feet).sqrMagnitude<.0064f;
                        }));
                    if((bool)cat.GetType().GetProperty("Grounded").GetValue(cat) && (onRoof||onRubble) && Mathf.Abs(body.position.x-landing.x)<landing.z/2+.4f){reached=true;break;}
                }
                Assert.That(reached,Is.True,"Recovery "+i+" could not rejoin at "+landing+"; stopped "+body.position);
            }
            var follow=course.GetComponentInChildren(RuntimeType("CatCamera"));var camera=follow.GetComponent<Camera>();
            foreach(var mover in movers.Skip(2)) {
                int index=(int)type.GetProperty(((Vector2)mover.GetType().GetField("Axis").GetValue(mover)).x==0?"LiftIndex":"DiagonalIndex").GetValue(null);
                foreach(int phase in new[]{63,187}) {
                    Call(mover,"ResetPhase");
                    for(int step=0;step<phase;step++){Call(mover,"FixedUpdate");Physics2D.Simulate(Time.fixedDeltaTime);}
                    var bounds=mover.GetComponent<PolygonCollider2D>().bounds;
                    cat.GetType().GetMethod("RespawnAt").Invoke(cat,new object[]{new Vector2(bounds.center.x,bounds.max.y+.46f)});
                    Call(follow,"LateUpdate");
                    var destination=route[index+1];
                    var screenPoint=camera.WorldToViewportPoint(new Vector3(destination.x-destination.z/2+.5f,destination.y+1.2f));
                    Assert.That(screenPoint.x,Is.InRange(.04f,.96f));Assert.That(screenPoint.y,Is.InRange(.08f,.96f),"Lift destination hidden at phase "+phase);
                }
            }
            // Camera must follow a genuine fall instead of holding the final gate reveal forever.
            cat.GetType().GetMethod("RespawnAt").Invoke(cat,new object[]{new Vector2(route.Last().x,-2)});
            Call(follow,"LateUpdate");Assert.That(camera.transform.position.y,Is.LessThan(4));

    }

    [UnityTest]
    public IEnumerator DiagonalSupportCarriesRunsJumpsAndResetsFinaleTimer()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        yield return new EnterPlayMode();
        host = new GameObject("Diagonal finale regression");
        var screen = host.AddComponent(RuntimeType("StarterScreen")); yield return null;
        Call(screen,"OpenLevelOne");
        var course=(Component)Field(screen,"course");
        Call(course,"StartFinalePreview");
        var cat=(Component)course.GetType().GetProperty("Cat").GetValue(course);
        ((Behaviour)cat.GetComponent(RuntimeType("CatInput"))).enabled=false;
        var run=course.GetType().GetProperty("Run").GetValue(course);
        var body=cat.GetComponent<Rigidbody2D>(); body.interpolation=RigidbodyInterpolation2D.None;
        var motor=cat.GetType().GetField("Motion").GetValue(cat);
        var mover=course.GetComponentsInChildren(RuntimeType("MovingSupport")).Single(m => {
            var axis=(Vector2)m.GetType().GetField("Axis").GetValue(m); return axis.x>0 && axis.y>0;
        });
        var guide=course.GetComponentsInChildren<MeshRenderer>().Single(r=>r.name=="Inclined counterweight bearings");
        var footing=(Collider2D)mover.GetType().GetProperty("RecoveryFloor").GetValue(mover);
        Assert.That(footing,Is.Not.Null,"The carrier must reference its authored recovery floor.");
        Assert.That(footing.bounds.min.x,Is.LessThanOrEqualTo(guide.bounds.min.x-.1f),"The left bearing must rest within the recovery floor.");
        Assert.That(footing.bounds.max.x,Is.GreaterThanOrEqualTo(guide.bounds.max.x+.1f),"The right bearing must rest within the recovery floor, not hang below its edge.");
        yield return null;
        var previous=Physics2D.simulationMode; Physics2D.simulationMode=SimulationMode2D.Script;
        try {
            foreach(int phase in new[]{0,63,125,187}) {
                Call(mover,"ResetPhase");
                cat.GetType().GetMethod("RespawnAt").Invoke(cat,new object[]{new Vector2(-3,.52f)});
                for(int i=0;i<phase;i++){Call(mover,"FixedUpdate");Physics2D.Simulate(Time.fixedDeltaTime);}
                var bounds=mover.GetComponent<PolygonCollider2D>().bounds;
                cat.GetType().GetMethod("RespawnAt").Invoke(cat,new object[]{new Vector2(bounds.center.x,bounds.max.y+1.1f)});
                for(int i=0;i<30;i++){Call(mover,"FixedUpdate");Call(cat,"FixedUpdate");Physics2D.Simulate(Time.fixedDeltaTime);}
                Assert.That((bool)cat.GetType().GetProperty("Grounded").GetValue(cat),Is.True);
                var offset=body.position-(Vector2)mover.transform.position;
                for(int i=0;i<30;i++){Call(mover,"FixedUpdate");Call(cat,"FixedUpdate");Physics2D.Simulate(Time.fixedDeltaTime);}
                Assert.That(Vector2.Distance(body.position-(Vector2)mover.transform.position,offset),Is.LessThan(.12f));
                var camera=course.GetComponentInChildren(RuntimeType("CatCamera"));
                Call(camera,"Snap");Call(camera,"LateUpdate");
                for(int i=0;i<60;i++){
                    Call(mover,"FixedUpdate");Call(cat,"FixedUpdate");Physics2D.Simulate(Time.fixedDeltaTime);
                    cat.transform.position=body.position;Call(camera,"LateUpdate");
                    Assert.That((float)Field(camera,"lookAhead"),Is.GreaterThan(1.5f),"Idle carrier drift must not flip the camera look direction.");
                }
                for(int i=0;i<24;i++){
                    motor.GetType().GetMethod("SetMove").Invoke(motor,new object[]{i<12?1f:-1f});
                    Call(mover,"FixedUpdate");Call(cat,"FixedUpdate");Physics2D.Simulate(Time.fixedDeltaTime);
                    Assert.That((bool)cat.GetType().GetProperty("Grounded").GetValue(cat),Is.True);
                }
                cat.GetType().GetMethod("PressJump").Invoke(cat,null);
                Assert.That(body.linearVelocity.y,Is.EqualTo(12).Within(.05f));
                Assert.That(Mathf.Abs(body.linearVelocity.x),Is.LessThanOrEqualTo(7.05f));
                Call(course,"TogglePause"); var position=body.position; var platform=mover.transform.position;
                Call(mover,"FixedUpdate");Call(cat,"FixedUpdate");Physics2D.Simulate(Time.fixedDeltaTime);
                Assert.That(body.position,Is.EqualTo(position));Assert.That(mover.transform.position,Is.EqualTo(platform));
                Call(course,"TogglePause");
            }
            Assert.That((bool)course.GetType().GetProperty("HasReachedExit").GetValue(course),Is.False,"Checkpoint 2 is not the exit.");
            run.GetType().GetMethod("Advance").Invoke(run,new object[]{9f});
            Assert.That((float)course.GetType().GetProperty("FinaleSeconds").GetValue(course),Is.GreaterThan(8));
            Call(course,"RetryCheckpoint");
            Assert.That((float)course.GetType().GetProperty("FinaleSeconds").GetValue(course),Is.EqualTo(0));
            Assert.That((float)mover.GetType().GetProperty("Phase").GetValue(mover),Is.Zero);
            Call(course,"RestartRun");
            Assert.That((float)course.GetType().GetProperty("FinaleSeconds").GetValue(course),Is.EqualTo(-1));
        } finally {Physics2D.simulationMode=previous;}
    }

    [UnityTest]
    public IEnumerator VerticalSupportAcceptsAirborneLandingsRunningAndFreshJumps()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        yield return new EnterPlayMode();
        host = new GameObject("Vertical rider regression");
        var screen = host.AddComponent(RuntimeType("StarterScreen")); yield return null;
        Call(screen, "OpenLevelOne");
        var course = (Component)Field(screen, "course");
        var cat = (Component)course.GetType().GetProperty("Cat").GetValue(course);
        ((Behaviour)cat.GetComponent(RuntimeType("CatInput"))).enabled = false;
        var mover = course.GetComponentsInChildren(RuntimeType("MovingSupport")).First(m =>
            ((Vector2)m.GetType().GetField("Axis").GetValue(m)).y > 0);
        yield return null;
        var body = cat.GetComponent<Rigidbody2D>();
        body.interpolation = RigidbodyInterpolation2D.None;
        var presentation = cat.GetComponentInChildren(RuntimeType("MikaSpritePresentation"));
        var animation = presentation.GetType().GetField("Animation").GetValue(presentation);
        var motor = cat.GetType().GetField("Motion").GetValue(cat);
        var previous = Physics2D.simulationMode; Physics2D.simulationMode = SimulationMode2D.Script;
        try {
            foreach (int phaseSteps in new[] { 0, 30, 125, 160 }) {
                mover.GetType().GetMethod("ResetPhase").Invoke(mover, null);
                cat.GetType().GetMethod("RespawnAt").Invoke(cat, new object[] { new Vector2(-3,.52f) });
                for (int tick = 0; tick < phaseSteps; tick++) { Call(mover,"FixedUpdate"); Physics2D.Simulate(Time.fixedDeltaTime); }
                Vector2 top = mover.GetComponent<PolygonCollider2D>().bounds.center;
                top.y = mover.GetComponent<PolygonCollider2D>().bounds.max.y;
                cat.GetType().GetMethod("RespawnAt").Invoke(cat, new object[] { top + Vector2.up * 1.1f });
                Physics2D.SyncTransforms(); bool landed = false;
                for (int tick = 0; tick < 30; tick++) {
                    Call(mover,"FixedUpdate"); Call(cat,"FixedUpdate"); Physics2D.Simulate(Time.fixedDeltaTime);
                    if ((bool)cat.GetType().GetProperty("Grounded").GetValue(cat)) { landed = true; break; }
                }
                Assert.That(landed, Is.True, "Landing was ignored at phase " + phaseSteps);
                var runningFrames = new System.Collections.Generic.HashSet<int>();
                for (int tick = 0; tick < 24; tick++) {
                    motor.GetType().GetMethod("SetMove").Invoke(motor,new object[] { tick < 12 ? 1f : -1f });
                    Call(mover,"FixedUpdate"); Call(cat,"FixedUpdate"); Physics2D.Simulate(Time.fixedDeltaTime);
                    Assert.That((bool)cat.GetType().GetProperty("Grounded").GetValue(cat), Is.True,
                        "Walking lost the rising/falling support at phase " + phaseSteps);
                    presentation.GetType().GetMethod("RenderPose").Invoke(presentation, new object[] { Time.fixedDeltaTime });
                    int frame = (int)animation.GetType().GetProperty("Frame").GetValue(animation);
                    if (frame >= 4 && frame <= 7) runningFrames.Add(frame);
                }
                Assert.That(runningFrames.Count, Is.GreaterThanOrEqualTo(2), "Running artwork must advance on the lift.");
                cat.GetType().GetMethod("PressJump").Invoke(cat,null);
                Assert.That(body.linearVelocity.y, Is.EqualTo(12).Within(.05f), "Platform speed must not change jump launch.");
                float start = body.position.y;
                for (int tick = 0; tick < 8; tick++) { Call(mover,"FixedUpdate"); Call(cat,"FixedUpdate"); Physics2D.Simulate(Time.fixedDeltaTime); }
                Assert.That(body.position.y, Is.GreaterThan(start + 1));
            }
            var ascent = (Vector3[])course.GetType().GetField("AscentPlatforms").GetValue(null);
            foreach (int phaseSteps in new[] { 187, 63 }) {
                mover.GetType().GetMethod("ResetPhase").Invoke(mover,null);
                for (int tick = 0; tick < phaseSteps; tick++) { Call(mover,"FixedUpdate"); Physics2D.Simulate(Time.fixedDeltaTime); }
                mover.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
                var bounds = mover.GetComponent<PolygonCollider2D>().bounds;
                cat.GetType().GetMethod("RespawnAt").Invoke(cat,new object[] { new Vector2(bounds.max.x - .7f, bounds.max.y + .46f) });
                SettleCourse(cat,course);
                motor.GetType().GetMethod("SetMove").Invoke(motor,new object[] { 1f });
                cat.GetType().GetMethod("PressJump").Invoke(cat,null);
                bool reached = false;
                var destination = ascent[52];
                for (int tick = 0; tick < 90; tick++) {
                    if (body.position.x >= destination.x) motor.GetType().GetMethod("SetMove").Invoke(motor,new object[] { 0f });
                    Call(cat,"FixedUpdate"); Physics2D.Simulate(Time.fixedDeltaTime);
                    if ((bool)cat.GetType().GetProperty("Grounded").GetValue(cat) &&
                        body.position.x > destination.x - destination.z / 2 &&
                        Mathf.Abs(body.position.y - destination.y - 1.2f) < .08f) { reached = true; break; }
                }
                Assert.That(reached, Is.EqualTo(phaseSteps == 63), "The upper landing must require the lift's high phase.");
            }
            // Entrance group 2 now uses the native cave ledges, covered by connected-cave traversal/reversal.
            foreach (int group in new[] { 5, 7, 9, 10, 13 }) {
                mover.GetType().GetMethod("ResetPhase").Invoke(mover,null);
                for (int tick = 0; tick < 187; tick++) { Call(mover,"FixedUpdate"); Physics2D.Simulate(Time.fixedDeltaTime); }
                var shelf = ascent[group * 5 + 1];
                var recovery = course.GetComponentsInChildren<PolygonCollider2D>().First(c =>
                    (c.name == "Recovery terrace"||c.name=="Shore opening recovery") && Mathf.Abs(c.transform.localPosition.x - shelf.x - (group == 10 ? 1.6f : 0)) < .01f);
                // An intentionally carved shore is exercised by the actual entry/rejoin route tests.
                if(recovery.name=="Shore opening recovery")continue;
                float center = recovery.transform.localPosition.x;
                cat.GetType().GetMethod("RespawnAt").Invoke(cat,new object[] { new Vector2(center - 3.3f, recovery.bounds.max.y + .46f) });
                Physics2D.SyncTransforms();
                for (int tick = 0; tick < 65 && body.position.x < center + 3.3f; tick++) {
                    motor.GetType().GetMethod("SetMove").Invoke(motor,new object[] { 1f });
                    Call(mover,"FixedUpdate"); Call(cat,"FixedUpdate"); Physics2D.Simulate(Time.fixedDeltaTime);
                }
                Assert.That(body.position.x, Is.GreaterThanOrEqualTo(center + 3.3f), "Blocked passage in group " + group);
            }
        } finally { Physics2D.simulationMode = previous; }
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
        Assert.That(supports.Length, Is.EqualTo(4));
        Assert.That(course.GetComponentsInChildren<PlatformEffector2D>(), Is.Empty, "Course platforms must be solid from below.");
        var shaped = course.GetComponentsInChildren<PolygonCollider2D>();
        Assert.That(shaped.Length, Is.GreaterThan(20));
        // This contact-profile assertion applies to suspended/main-route platforms, not solid cave volumes.
        foreach (var polygon in shaped.Where(p => !p.isTrigger && (p.name=="Standable platform" ||
            p.name=="Recovery terrace" || p.name=="Late ruin recovery" || p.name.StartsWith("Finale recovery ")))) {
            var bounds = polygon.bounds;
            Assert.That(polygon.OverlapPoint(new Vector2(bounds.center.x, bounds.max.y - 0.03f)), Is.True,
                "The registered landing surface must remain solid.");
            Assert.That(polygon.OverlapPoint(new Vector2(bounds.min.x + 0.01f, bounds.min.y + 0.01f)), Is.False,
                "Empty lower corners must not remain rectangular collision blocks.");
        }
        var finale=(Vector3[])RuntimeType("FinaleRoute").GetProperty("Platforms").GetValue(null);
        foreach(var rubble in shaped.Where(p=>p.name=="Solid fallen masonry")) {
            var bounds=rubble.bounds;
            Assert.That(rubble.OverlapPoint(new Vector2(bounds.center.x,bounds.min.y+.03f)),Is.True,"Grounded rubble must have a solid resting base.");
            Assert.That(rubble.OverlapPoint(new Vector2(bounds.min.x+.01f,bounds.max.y-.03f)),Is.False,"Short rubble side stones must not become an invisible full-width cap.");
            var bank=finale.Single(p=>Mathf.Abs(p.x+1.7f-rubble.transform.localPosition.x)<.01f && p.z>=10);
            float ground=course.transform.TransformPoint(new Vector3(bank.x,bank.y+.75f)).y;
            Assert.That(bounds.min.y,Is.EqualTo(ground).Within(.06f),"Rubble must rest on its terrace rather than float above it.");
            var physicsMode=Physics2D.simulationMode;
            Physics2D.simulationMode=SimulationMode2D.Script;
            try {
                var body=cat.GetComponent<Rigidbody2D>();
                var motion=cat.GetType().GetField("Motion").GetValue(cat);
                foreach(float offset in new[]{-.95f,0f,.95f}) {
                    cat.GetType().GetMethod("RespawnAt").Invoke(cat,new object[]{new Vector2(bounds.center.x+offset,bounds.max.y+1)});
                    Physics2D.SyncTransforms();
                    for(int tick=0;tick<100;tick++) { Call(cat,"FixedUpdate"); Physics2D.Simulate(Time.fixedDeltaTime); }
                    var resting=body.position;
                    for(int tick=0;tick<50;tick++) { Call(cat,"FixedUpdate"); Physics2D.Simulate(Time.fixedDeltaTime); }
                    Assert.That(Vector2.Distance(body.position,resting),Is.LessThan(.03f),"No idle slide after landing on rubble at offset "+offset);
                    Assert.That((bool)cat.GetType().GetProperty("Grounded").GetValue(cat),Is.True);
                }
                foreach(float direction in new[]{-1f,1f}) {
                    cat.GetType().GetMethod("RespawnAt").Invoke(cat,new object[]{new Vector2(bounds.center.x-direction*3,ground+.46f)});
                    Physics2D.SyncTransforms();
                    for(int tick=0;tick<10;tick++) { Call(cat,"FixedUpdate"); Physics2D.Simulate(Time.fixedDeltaTime); }
                    motion.GetType().GetMethod("SetMove").Invoke(motion,new object[]{direction});
                    cat.GetType().GetMethod("PressJump").Invoke(cat,null);
                    for(int tick=0;tick<60;tick++) { Call(cat,"FixedUpdate"); Physics2D.Simulate(Time.fixedDeltaTime); }
                    Assert.That((body.position.x-bounds.center.x)*direction,Is.GreaterThan(2),"Held movement must cross the rubble without rebound or locking.");
                }
            } finally { Physics2D.simulationMode=physicsMode; }

        }
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
            foreach (int bankIndex in new[] { 20, 50, 65, 75 }) {
                var bank = ascent[bankIndex]; var shelf = ascent[bankIndex + 1];
                cat.GetType().GetMethod("RespawnAt").Invoke(cat, new object[] {
                    bankIndex == 75 ? new Vector2(shelf.x + 2, 11.86f) : new Vector2(shelf.x, bank.y - .29f) });
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
                Assert.That(recovered, Is.True, "Recovery terrace must permit a short return to bank " + bankIndex);
            }
            cat.transform.localPosition = new Vector3(900, 14);
            var follow = course.GetComponentInChildren(RuntimeType("CatCamera"));
            follow.GetType().GetMethod("Snap").Invoke(follow, null); Call(follow, "LateUpdate");
            Assert.That(follow.transform.position.y, Is.EqualTo(11.4f).Within(0.01f));
            course.GetType().GetMethod("RestartRun").Invoke(course, null);
            Call(follow, "LateUpdate");
            Assert.That(follow.transform.position.y, Is.EqualTo(1).Within(0.01f));
            cat.transform.localPosition = new Vector3(500, 3.5f);
            for (int tick = 0; tick < 30; tick++) Call(follow, "LateUpdate");
            Assert.That(follow.transform.position.y, Is.EqualTo(1).Within(0.01f), "Ordinary jumps stay inside the resting zone.");
            cat.transform.localPosition = new Vector3(500, 4.2f);
            for (int tick = 0; tick < 60; tick++) Call(follow, "LateUpdate");
            Assert.That(follow.transform.position.y, Is.EqualTo(1.6f).Within(0.03f));
            cat.transform.localPosition = new Vector3(500, 1f);
            for (int tick = 0; tick < 30; tick++) Call(follow, "LateUpdate");
            Assert.That(follow.transform.position.y, Is.EqualTo(1.6f).Within(0.03f), "Landing must not pull the camera straight back down.");
            cat.transform.localPosition = new Vector3(500, -1f);
            for (int tick = 0; tick < 60; tick++) Call(follow, "LateUpdate");
            Assert.That(follow.transform.position.y, Is.EqualTo(1.2f).Within(0.03f), "Falls engage the lower edge.");
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
