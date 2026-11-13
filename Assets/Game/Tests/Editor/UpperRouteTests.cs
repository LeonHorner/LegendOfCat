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
    [UnityTest]
    public IEnumerator WholeLevelCompletesWithTheOptionalLoopAndBothMainCheckpoints() => WholeLevel(true);
    [UnityTest] public IEnumerator WholeLevelCanSkipTheCave() => WholeLevel(false);
    IEnumerator WholeLevel(bool includeCave)
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
        var pilot = new UpperLevelPhysicsPilot(course,includeCave);
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
    public IEnumerator ConnectedCaveSupportsTraversalCatchReverseWaterAndLifecycle()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();
        Runtime("IntegratedCaveReview").GetMethod("Verify").Invoke(null,null);
        yield return new ExitPlayMode();
    }
    [UnityTest]
    public IEnumerator ShoreReplacementsAreUniqueAndWaterMaskEndsInsideSolidRock()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
        root=new GameObject("Permanent shore registration");root.SetActive(false);
        var course=root.AddComponent(Runtime("MovementCourse"));course.GetType().GetField("LevelOne").SetValue(course,true);root.SetActive(true);
        Runtime("IntegratedCaveReview").GetMethod("VerifyShoreRegistration").Invoke(null,new object[]{course});
        yield return new ExitPlayMode();
    }
    [UnityTearDown]
    public IEnumerator Cleanup()
    {
        if(Application.isPlaying){if(root)UnityEngine.Object.Destroy(root);yield return null;yield return new ExitPlayMode();}
    }
}
