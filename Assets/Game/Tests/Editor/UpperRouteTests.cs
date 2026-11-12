using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class UpperRouteTests
{
    GameObject root;
    static Type Runtime(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).First(t => t != null);
    static object Get(object item, string name) => item.GetType().GetProperty(name).GetValue(item);
    static void Call(object item, string name, params object[] args) => item.GetType().GetMethod(name,
        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(item, args);
    static void PlaceForCamera(Component cat, Component garden, Component follow, Vector2 position)
    {
        Call(cat, "RespawnAt", position); Call(garden, "Refresh"); Call(follow, "Snap"); Call(follow, "Follow", .02f);
    }

    [UnityTest]
    public IEnumerator WholeLevelCompletesWithTheOptionalLoopAndBothMainCheckpoints()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        yield return new EnterPlayMode();
        root = new GameObject("Whole level optional integration"); root.SetActive(false);
        var course = root.AddComponent(Runtime("MovementCourse"));
        course.GetType().GetField("LevelOne").SetValue(course, true); root.SetActive(true);
        var cat = (Component)Get(course, "Cat");
        ((Behaviour)cat.GetComponent(Runtime("CatInput"))).enabled = false;
        var movers = root.GetComponentsInChildren(Runtime("MovingSupport"));
        foreach (var mover in movers) Call(mover, "Start");
        var pilot = new UpperLevelPhysicsPilot(course);
        var oldMode = Physics2D.simulationMode; Physics2D.simulationMode = SimulationMode2D.Script;
        try {
            Physics2D.SyncTransforms();
            for (int tick = 0; tick < 25000 && !pilot.Complete; tick++) {
                foreach (var mover in movers) Call(mover, "FixedUpdate");
                pilot.Command(); Call(cat, "FixedUpdate"); Physics2D.Simulate(Time.fixedDeltaTime); Call(course, "Update");
            }
            Assert.That(pilot.Complete, Is.True, "Whole level stopped at " + cat.transform.position);
            Assert.That((int)Get(Get(course, "Run"), "Checkpoint"), Is.EqualTo(2));
        } finally { Physics2D.simulationMode = oldMode; }
        yield return new ExitPlayMode();
    }

    [UnityTest]
    public IEnumerator OptionalLoopUsesRealCollidersAndPreservesMainRecovery()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        yield return new EnterPlayMode();
        root = new GameObject("Optional route collision regression"); root.SetActive(false);
        var course = root.AddComponent(Runtime("MovementCourse"));
        course.GetType().GetField("LevelOne").SetValue(course, true); root.SetActive(true);
        var cat = (Component)Get(course, "Cat");
        ((Behaviour)cat.GetComponent(Runtime("CatInput"))).enabled = false;
        var body = cat.GetComponent<Rigidbody2D>();
        body.interpolation = RigidbodyInterpolation2D.None;
        var run = Get(course, "Run");
        var entry = (Vector2)Runtime("UpperRoute").GetProperty("Entrance").GetValue(null);
        var steps = (Vector3[])Runtime("UpperRoute").GetField("Platforms").GetValue(null);
        var spawns = (Vector2[])course.GetType().GetField("LevelOneCheckpointSpawns").GetValue(null);
        run.GetType().GetMethod("TryActivateCheckpoint").Invoke(run, new object[] { 1, spawns[0], true });
        var oldMode = Physics2D.simulationMode; Physics2D.simulationMode = SimulationMode2D.Script;
        void Tick() { Call(cat, "FixedUpdate"); Physics2D.Simulate(Time.fixedDeltaTime); Call(course, "Update"); }
        try {
            Call(cat, "RespawnAt", entry);
            Physics2D.SyncTransforms();
            for (int i = 0; i < 10; i++) Tick();
            var pilot = new UpperRoutePhysicsPilot(course);
            int elapsedTicks = 0;
            for (int tick = 0; tick < 8000 && !pilot.Complete; tick++) {
                elapsedTicks++;
                pilot.Command(); Tick();
                Assert.That((int)Get(run, "Retries"), Is.Zero, "Fall while approaching optional step " + pilot.Next);
            }
            Debug.Log("Descent simulation: " + (elapsedTicks*Time.fixedDeltaTime).ToString("F2") + " seconds; step " + pilot.Next + " position " + body.position);
            Assert.That(pilot.Complete, Is.True, "Stopped before step " + pilot.Next + " at " + body.position);
            Assert.That((int)Get(run, "Checkpoint"), Is.EqualTo(1));
            Assert.That(root.GetComponentsInChildren(Runtime("CheckpointMarker")), Has.Length.EqualTo(2));
            Assert.That(root.GetComponentsInChildren<Transform>().Any(t => t.name == "FutureCollectibleAnchor"), Is.True);
            int entryTurn = (int)Runtime("UpperRoute").GetField("DescentTurnIndex").GetValue(null);
            var reverseEntry = new Vector3[entryTurn + 1];
            for (int i = 0; i < entryTurn; i++) reverseEntry[i] = steps[entryTurn - i - 1];
            reverseEntry[entryTurn] = new Vector3(entry.x - 2, -.75f, 8);
            Call(cat, "RespawnAt", new Vector2(steps[entryTurn].x, steps[entryTurn].y + 1.22f));
            Physics2D.SyncTransforms();
            for (int i = 0; i < 10; i++) Tick();
            var reversePilot = new UpperRoutePhysicsPilot(course, reverseEntry, steps[entryTurn]);
            for (int tick = 0; tick < 2500 && !reversePilot.Complete; tick++) { reversePilot.Command(); Tick(); }
            Assert.That(reversePilot.Complete, Is.True, "Entry cannot be reversed at step " + reversePilot.Next + " position " + body.position);
            var catches = (Vector3[])Runtime("UpperRoute").GetField("Recoveries").GetValue(null);
            var returnSteps = new Vector3[catches.Length];
            Array.Copy(catches, 1, returnSteps, 0, catches.Length - 1);
            returnSteps[returnSteps.Length - 1] = steps[(int)Runtime("UpperRoute").GetField("RiskIndex").GetValue(null)-1];
            Call(cat, "RespawnAt", new Vector2(catches[0].x, catches[0].y + 1.22f));
            Physics2D.SyncTransforms();
            for (int i = 0; i < 10; i++) Tick();
            var rescuePilot = new UpperRoutePhysicsPilot(course, returnSteps, catches[0]);
            for (int tick = 0; tick < 2500 && !rescuePilot.Complete; tick++) { rescuePilot.Command(); Tick(); }
            Assert.That(rescuePilot.Complete, Is.True, "Rescue stopped at step " + rescuePilot.Next + " position " + body.position);
            // An actual fall from each shelf must either catch safe route geometry or retry checkpoint 1.
            foreach (var p in steps) {
                Call(cat, "RespawnAt", new Vector2(p.x + p.z / 2 + .7f, p.y + 1.22f));
                Physics2D.SyncTransforms();
                for (int tick = 0; tick < 180; tick++) Tick();
                Assert.That((bool)Get(cat, "Grounded"), Is.True, "No recovery beneath " + p);
                Assert.That((int)Get(run, "Checkpoint"), Is.EqualTo(1));
                Call(course, "RetryCheckpoint");
                Assert.That(body.position, Is.EqualTo(spawns[0]));
            }
            Call(cat, "RespawnAt", new Vector2(steps[5].x, steps[5].y + 1.22f));
            Physics2D.SyncTransforms();
            Call(course, "TogglePause"); var paused = body.position;
            for (int i = 0; i < 20; i++) Tick();
            Assert.That(body.position, Is.EqualTo(paused));
            Call(course, "TogglePause"); Call(course, "RetryCheckpoint");
            Assert.That(body.position, Is.EqualTo(spawns[0]));
            Call(course, "RestartRun");
            Assert.That((int)Get(run, "Checkpoint"), Is.Zero);
            Assert.That(body.position.x, Is.EqualTo(-3));
            Debug.Log("Optional route scripted collision traversal and recovery verified; not a human difficulty measurement.");
        } finally { Physics2D.simulationMode = oldMode; }
        yield return new ExitPlayMode();
    }
    [UnityTearDown]
    public IEnumerator Cleanup()
    {
        if (Application.isPlaying) { if (root) UnityEngine.Object.Destroy(root); yield return null; yield return new ExitPlayMode(); }
    }
}
