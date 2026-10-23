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
    static Type RuntimeType(string name) => AppDomain.CurrentDomain.GetAssemblies()
        .Select(a => a.GetType(name)).First(t => t != null);
    static object Field(object target, string name) => target.GetType()
        .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
    static void Call(object target, string name) => target.GetType()
        .GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, null);

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
        Call(screen, "OpenCourse");
        var next = (Component)Field(screen, "course");
        Assert.That(next, Is.Not.Null);
        Assert.That(next, Is.Not.SameAs(course), "Re-entry must create a new course.");
        var nextCat = (Component)next.GetType().GetProperty("Cat").GetValue(next);
        Assert.That(nextCat.GetComponent<Rigidbody2D>().position.x, Is.EqualTo(spawnX).Within(0.01f),
            "Re-entry must start at the original spawn.");
        Call(screen, "CloseCourse");
        yield return null;
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
