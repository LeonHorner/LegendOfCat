using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class WaterRecoveryTests
{
    GameObject root, player, cameraObject;
    Component cat, recovery;
    Rigidbody2D body;
    int retries;
    readonly Vector2 checkpoint = new Vector2(9, 0.52f);
    static Type Find(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).First(t => t != null);
    static object Call(object target, string method, params object[] args) => target.GetType().GetMethod(method,
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Invoke(target, args);
    static T Read<T>(object target, string name) => (T)target.GetType().GetProperty(name).GetValue(target);

    [SetUp] public void SetUp()
    {
        root = new GameObject("Water recovery test course");
        root.transform.position = new Vector3(30, 2, 0);
        root.transform.localScale = Vector3.one * 1.2f;
        player = new GameObject("Test Mika");
        player.transform.SetParent(root.transform, false);
        body = player.AddComponent<Rigidbody2D>();
        player.AddComponent<BoxCollider2D>().size = Vector2.one * 0.9f;
        cat = player.AddComponent(Find("CatController")); Call(cat, "Awake");
        cameraObject = new GameObject("Test camera");
        var camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true; camera.orthographicSize = 6.6f;
        recovery = player.AddComponent(Find("WaterRecovery"));
        Action retry = () => { retries++; Call(cat, "RespawnAt", (Vector2)root.transform.TransformPoint(checkpoint)); };
        Call(recovery, "Initialize", cat, camera, root.transform, retry);
    }

    void Position(float y, float velocity = -20)
    {
        body.position = root.transform.TransformPoint(new Vector3(6, y));
        player.transform.position = body.position;
        body.linearVelocity = new Vector2(0, velocity);
        Physics2D.SyncTransforms();
    }

    [TearDown] public void TearDown()
    {
        // Let runtime destruction release its generated meshes/materials in normal Play;
        // EditMode tests explicitly release transient objects after cancellation.
        Call(recovery, "Cancel");
        foreach (string field in new[] { "effects", "submerge", "foamMaterial", "drawings" }) {
            var info = recovery.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            var value = info.GetValue(recovery);
            if (value is Sprite[] sprites) foreach (var sprite in sprites) UnityEngine.Object.DestroyImmediate(sprite);
            else if (value is UnityEngine.Object owned) UnityEngine.Object.DestroyImmediate(owned);
            info.SetValue(recovery, null);
        }
        var material = player.GetComponent<BoxCollider2D>().sharedMaterial;
        cat.GetType().GetField("material", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(cat, null);
        UnityEngine.Object.DestroyImmediate(material);
        UnityEngine.Object.DestroyImmediate(root); UnityEngine.Object.DestroyImmediate(cameraObject);
        retries = 0;
    }

    [Test] public void EntryStopsFallingAndRetriesExactlyOnceAtCheckpointAfterBlackout()
    {
        Position(-2); Assert.That((bool)Call(recovery, "TryEnter"), Is.False);
        Position(-3.1f); Call(cat, "FixedUpdate");
        Assert.That(Read<bool>(recovery, "Active"), Is.True);
        Assert.That(body.simulated, Is.False);
        Call(cat, "FixedUpdate"); Call(cat, "PressJump");
        Assert.That((bool)Call(recovery, "TryEnter"), Is.False);
        Call(recovery, "Advance", 2f);
        Assert.That(retries, Is.Zero);
        Assert.That(Read<float>(recovery, "Fade"), Is.EqualTo(1));
        Call(recovery, "Advance", 0.11f);
        Assert.That(retries, Is.EqualTo(1));
        Assert.That(body.position, Is.EqualTo((Vector2)root.transform.TransformPoint(checkpoint)));
        Assert.That(Read<bool>(recovery, "CameraHeld"), Is.False);
        Call(recovery, "Advance", 1f); Call(recovery, "Advance", 10f);
        Assert.That(retries, Is.EqualTo(1));
        Assert.That(Read<bool>(recovery, "Active"), Is.False);
        Assert.That(body.simulated, Is.True);
        Assert.That(Read<float>(recovery, "Fade"), Is.Zero);
    }

    [Test] public void PauseFreezesSinkingAndManualRetryRestoresCameraAndPhysics()
    {
        Position(-3.1f); Assert.That((bool)Call(recovery, "TryEnter"), Is.True);
        Call(recovery, "Advance", 0.5f); Call(recovery, "LateUpdate");
        Assert.That(cameraObject.GetComponent<Camera>().orthographicSize, Is.LessThan(6.6f));
        Call(cat, "SetPaused", true); Call(recovery, "Advance", 10f);
        Assert.That(Read<float>(recovery, "Elapsed"), Is.EqualTo(0.5f));
        Assert.That(retries, Is.Zero);
        Call(cat, "SetPaused", false);
        Assert.That(body.simulated, Is.False, "Resuming must not resume falling during submersion.");
        Call(cat, "RespawnAt", checkpoint);
        Assert.That(Read<bool>(recovery, "Active"), Is.False);
        Assert.That(body.simulated, Is.True);
        Assert.That(cameraObject.GetComponent<Camera>().orthographicSize, Is.EqualTo(6.6f));
        Assert.That(Read<float>(recovery, "Fade"), Is.Zero);
    }

    [Test] public void UpwardMotionCannotTriggerWaterEntry()
    {
        Position(-3.1f, 2); Assert.That((bool)Call(recovery, "TryEnter"), Is.False);
        Assert.That(body.simulated, Is.True);
    }
}
