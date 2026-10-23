using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class CatCollisionTests
{
    GameObject cat, floor;
    Component controller;
    Rigidbody2D body;
    object motion;
    SimulationMode2D previousMode;
    void Invoke(string name) => controller.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(controller, null);
    void Input(string name, params object[] args) => motion.GetType().GetMethod(name).Invoke(motion, args);
    void Tick()
    {
        Invoke("FixedUpdate"); Physics2D.Simulate(Time.fixedDeltaTime);
    }
    [SetUp] public void SetUp()
    {
        previousMode = Physics2D.simulationMode; Physics2D.simulationMode = SimulationMode2D.Script;
        floor = new GameObject("Test floor"); floor.transform.position = new Vector2(0, -0.5f);
        floor.AddComponent<BoxCollider2D>().size = new Vector2(20, 1);
        cat = new GameObject("Test cat"); cat.transform.position = new Vector2(0, 0.46f);
        body = cat.AddComponent<Rigidbody2D>(); cat.AddComponent<BoxCollider2D>().size = Vector2.one * 0.9f;
        var type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("CatController")).First(t => t != null);
        controller = cat.AddComponent(type); Invoke("Awake");
        motion = type.GetField("Motion").GetValue(controller);
        Physics2D.SyncTransforms();
        for (int i = 0; i < 10; i++) Tick();
    }
    [TearDown] public void TearDown()
    {
        // The runtime owns the transient material; detach it before EditMode destruction.
        var material = cat.GetComponent<BoxCollider2D>().sharedMaterial;
        cat.GetComponent<BoxCollider2D>().sharedMaterial = null;
        controller.GetType().GetField("material", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(controller, null);
        UnityEngine.Object.DestroyImmediate(cat); UnityEngine.Object.DestroyImmediate(floor);
        UnityEngine.Object.DestroyImmediate(material); Physics2D.simulationMode = previousMode;
    }
    [Test] public void CatLandsAndCanJumpAgainWithoutHeldAutoJump()
    {
        Assert.That((bool)controller.GetType().GetProperty("Grounded").GetValue(controller), Is.True);
        Input("PressJump"); Tick(); Assert.That(body.linearVelocity.y, Is.GreaterThan(10));
        for (int i = 0; i < 70; i++) Tick();
        Assert.That(body.position.y, Is.InRange(0.43f, 0.49f));
        Assert.That((bool)controller.GetType().GetProperty("Grounded").GetValue(controller), Is.True);
        Input("ReleaseJump"); Input("PressJump"); Tick(); Assert.That(body.linearVelocity.y, Is.GreaterThan(10));
    }
    // Verify actual rigidbody motion with explicit physics steps, independent of
    // headless Play Mode focus, fixed-update scheduling, and Transform interpolation.
    [TestCase(-1f)]
    [TestCase(1f)]
    public void ScriptedPhysicsMovesAndJumpsInBothDirections(float direction)
    {
        Assert.That((bool)controller.GetType().GetProperty("Grounded").GetValue(controller), Is.True,
            "The cat must start grounded for the jump to trigger.");
        var start = body.position;
        Input("SetMove", direction);
        Input("PressJump");
        for (int i = 0; i < 5; i++) Tick();

        var displacement = body.position - start;
        var state = $"Direction={direction}, start={start}, actual={body.position}, " +
            $"velocity={body.linearVelocity}, simulation={Physics2D.simulationMode}";
        Assert.That(displacement.x * direction, Is.GreaterThan(0.05f), state);
        Assert.That(displacement.y, Is.GreaterThan(0.05f), state);
    }
    [Test] public void RetryRestoresPositionAndClearsVelocityAndBufferedInput()
    {
        Input("SetMove", 1f); Input("PressJump"); Tick();
        controller.GetType().GetMethod("ReturnToStart").Invoke(controller, null);
        Assert.That(body.position.x, Is.EqualTo(0).Within(0.001));
        Assert.That(body.linearVelocity, Is.EqualTo(Vector2.zero));
        Tick(); Assert.That(body.linearVelocity.x, Is.Zero);
        Assert.That(body.linearVelocity.y, Is.LessThanOrEqualTo(0));
    }
}
