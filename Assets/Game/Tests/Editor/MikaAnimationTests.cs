using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class MikaAnimationTests
{
    object animation;
    Type type;
    object Read(string name) => type.GetProperty(name).GetValue(animation);
    void Step(float dt, bool grounded, float x, float y, float distance = 0) =>
        type.GetMethod("Step").Invoke(animation, new object[] { dt, grounded, new Vector2(x, y), distance });

    [SetUp]
    public void Create()
    {
        type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("MikaAnimation")).First(t => t != null);
        animation = Activator.CreateInstance(type);
    }

    [Test]
    public void OriginalRunUsesOnlyFourSourcePosesAndStopsAtRest()
    {
        type.GetProperty("OriginalFrames").SetValue(animation, true);
        var poses = new System.Collections.Generic.HashSet<int>();
        for (int i = 0; i < 40; i++) {
            Step(0.02f, true, 7, 0, 0.1f);
            poses.Add((int)Read("Frame"));
        }
        Assert.That(poses, Is.EquivalentTo(new[] { 4, 5, 6, 7 }));
        var frame = Read("Frame");
        Step(1, true, 7, 0, 0);
        Assert.That(Read("Frame"), Is.EqualTo(frame));
        Step(0.02f, true, 0, 0);
        Assert.That(Read("Frame"), Is.EqualTo(0));
        Step(0.02f, false, 0, 11);
        Assert.That(Read("Frame"), Is.EqualTo(10));
    }

    [Test]
    public void OriginalJumpBridgesBothSidesOfApexButNotLedgeFalls()
    {
        type.GetProperty("OriginalFrames").SetValue(animation, true);
        Step(0.02f, false, 2, -0.1f);
        Assert.That(Read("Frame"), Is.EqualTo(11), "A ledge fall has no takeoff.");
        Step(0.02f, false, 2, 11);
        Assert.That(Read("Frame"), Is.EqualTo(10));
        foreach (float speed in new[] { 2f, 0.1f, 0f, -0.1f, -2f }) {
            Step(0.02f, false, 2, speed);
            Assert.That(Read("Frame"), Is.EqualTo(16));
        }
        Step(0.02f, false, 2, -4);
        Assert.That(Read("Frame"), Is.EqualTo(11));
        Step(0.02f, true, 0, 0);
        Step(0.02f, false, 0, -0.1f);
        Assert.That(Read("Frame"), Is.EqualTo(11), "Landing clears apex history.");
        type.GetMethod("Reset").Invoke(animation, null);
        Step(0.02f, false, 0, -0.1f);
        Assert.That(Read("Frame"), Is.EqualTo(11));
    }

    [Test]
    public void BufferedJumpImmediatelyInterruptsLanding()
    {
        Step(0.02f, false, 0, -5);
        Step(0.02f, true, 0, 0);
        Assert.That(Read("Current").ToString(), Is.EqualTo("Landing"));
        Step(0.02f, false, 0, 11);
        Assert.That(Read("Current").ToString(), Is.EqualTo("Jump"));
        Assert.That(Read("Frame"), Is.EqualTo(14));
    }

    [Test]
    public void ReversalChangesFacingImmediatelyWithoutHoldingMovement()
    {
        Step(0.02f, true, 7, 0, 0.14f);
        Step(0.02f, true, -1, 0, -0.02f);
        Assert.That(Read("Facing"), Is.EqualTo(-1));
        Assert.That(Read("Current").ToString(), Is.EqualTo("Turn"));
        Step(0.12f, true, -7, 0, -0.84f);
        Assert.That(Read("Current").ToString(), Is.EqualTo("Run"));
    }

    [Test]
    public void BlockedMovementDoesNotAdvanceTheGait()
    {
        Step(0.02f, true, 7, 0, 0.7f);
        var frame = Read("Frame");
        Step(1, true, 7, 0, 0);
        Assert.That(Read("Frame"), Is.EqualTo(frame));
        Step(0.02f, true, 0, 0);
        Assert.That(Read("Current").ToString(), Is.EqualTo("Idle"));
    }

    [Test]
    public void RunPhaseDependsOnDistanceAcrossFrameRates()
    {
        Step(1f / 30, true, 7, 0, 1.3f);
        var frame = Read("Frame");
        type.GetMethod("Reset").Invoke(animation, null);
        for (int i = 0; i < 10; i++) Step(1f / 300, true, 7, 0, 0.13f);
        Assert.That(Read("Frame"), Is.EqualTo(frame));
    }

    [Test]
    public void RetryClearsLandingTurnAndGaitHistory()
    {
        Step(0.02f, true, -7, 0, -1);
        Step(0.02f, false, -7, -10);
        type.GetMethod("Reset").Invoke(animation, null);
        Step(0.02f, true, 0, 0);
        Assert.That(Read("Current").ToString(), Is.EqualTo("Idle"));
        Assert.That(Read("Facing"), Is.EqualTo(1));
        Assert.That(Read("Frame"), Is.EqualTo(0));
    }

    [Test]
    public void ApexDoesNotSnapDirectlyFromTuckedToExtendedPaws()
    {
        Step(0.02f, false, 2, 11);
        Step(0.08f, false, 2, 4);
        Assert.That((int)Read("Frame"), Is.InRange(51, 53));
        Step(0.02f, false, 2, -0.1f);
        Assert.That(Read("Frame"), Is.EqualTo(16));
        Step(0.10f, false, 2, -6);
        Assert.That(Read("Frame"), Is.EqualTo(17));
        Step(0.10f, false, 2, -12);
        Assert.That(Read("Frame"), Is.EqualTo(18));
    }

    [Test]
    public void LandingPlaysContactAbsorptionAndRecoveryBeforeIdle()
    {
        Step(0.02f, false, 0, -10);
        Step(0.02f, true, 0, 0);
        for (int i = 0; i < 4; i++) {
            Assert.That(Read("Frame"), Is.EqualTo(20 + i));
            Step(0.0601f, true, 0, 0);
        }
        Assert.That(Read("Current").ToString(), Is.EqualTo("Idle"));
    }
}
