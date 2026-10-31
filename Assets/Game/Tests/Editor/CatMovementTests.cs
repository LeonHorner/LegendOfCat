using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class CatMovementTests
{
    Type motionType;
    object motion;
    Vector2 velocity;
    void Call(string method, params object[] args) => motionType.GetMethod(method).Invoke(motion, args);
    Vector2 Step(bool grounded = false, float dt = 0.02f)
        => velocity = (Vector2)motionType.GetMethod("Step").Invoke(motion, new object[] { dt, grounded, velocity });
    bool Jumped => (bool)motionType.GetProperty("Jumped").GetValue(motion);
    [SetUp] public void SetUp()
    {
        motionType = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("CatMotion")).First(t => t != null);
        motion = Activator.CreateInstance(motionType); velocity = Vector2.zero;
    }
    [Test] public void PressAndReleaseBetweenPhysicsStepsStillJumpWithShortHeight()
    {
        Call("PressJump"); Call("ReleaseJump"); Step(true);
        Assert.That(Jumped, Is.True); Assert.That(velocity.y, Is.InRange(4, 5));
        Step(false); Assert.That(Jumped, Is.False);
    }
    [Test] public void ShortMovementTapSurvivesUntilOnePhysicsStep()
    {
        Call("SetMove", 1f); Call("SetMove", 0f); Step(true);
        Assert.That(velocity.x, Is.GreaterThan(0));
        Step(true); Assert.That(velocity.x, Is.Zero);
    }
    [Test]
    public void EarlyTakeoffPreservesTheFixedStepArcAndShortMovementPulse()
    {
        Call("SetMove", 1f); Call("SetMove", 0f); Call("PressJump");
        var expected = Step(true);
        var expectedPositions = new float[45];
        float height = expected.y * 0.02f;
        for (int i = 0; i < expectedPositions.Length; i++) {
            if (i == 3) Call("ReleaseJump");
            height += Step().y * 0.02f;
            expectedPositions[i] = height;
        }
        SetUp();
        Call("SetMove", 1f); Call("SetMove", 0f); Call("PressJump");
        Assert.That((bool)motionType.GetMethod("StartBufferedJump").Invoke(motion, new object[] { true, velocity }), Is.True);
        velocity = (Vector2)motionType.GetProperty("Velocity").GetValue(motion);
        Assert.That(velocity.y, Is.EqualTo(12));
        Assert.That(Step(), Is.EqualTo(expected), "Early feedback must not consume movement pulses or add a physics tick.");
        height = velocity.y * 0.02f;
        for (int i = 0; i < expectedPositions.Length; i++) {
            if (i == 3) Call("ReleaseJump");
            height += Step().y * 0.02f;
            Assert.That(height, Is.EqualTo(expectedPositions[i]).Within(0.0001f));
        }
    }
    [Test] public void CoyoteJumpWorksShortlyAfterLeavingButExpires()
    {
        Step(true); Step(); Step(); Call("PressJump"); Step(); Assert.That(Jumped, Is.True);
        SetUp(); Step(true); for (int i = 0; i < 6; i++) Step();
        Call("PressJump"); Step(); Assert.That(Jumped, Is.False);
    }
    [Test] public void BufferedJumpFiresOnLandingOnceAndHoldingDoesNotRepeat()
    {
        Call("PressJump"); Step(); Step(); Step(true);
        Assert.That(Jumped, Is.True);
        for (int i = 0; i < 10; i++) { Step(true); Assert.That(Jumped, Is.False); }
    }
    [Test] public void OldAirbornePressDoesNotFireOnLanding()
    {
        Call("PressJump"); for (int i = 0; i < 8; i++) Step();
        Step(true); Assert.That(Jumped, Is.False);
    }
    [Test] public void ResetClearsInputAndJumpGrace()
    {
        Step(true); Call("SetMove", 1f); Call("PressJump"); Call("Reset");
        Step(); Assert.That(Jumped, Is.False); Assert.That(velocity.x, Is.Zero);
    }
    [TestCase(1f)] [TestCase(-1f)] public void HorizontalAccelerationAndBrakingUseElapsedTime(float direction)
    {
        Call("SetMove", direction); for (int i = 0; i < 4; i++) Step(true, 0.02f);
        float at50Hz = velocity.x;
        SetUp(); Call("SetMove", direction); for (int i = 0; i < 8; i++) Step(true, 0.01f);
        Assert.That(velocity.x, Is.EqualTo(at50Hz).Within(0.001f));
        Assert.That(Mathf.Abs(velocity.x), Is.EqualTo(4.4f).Within(0.001f));
        Call("SetMove", 0f); for (int i = 0; i < 15; i++) Step(true, 0.01f);
        Assert.That(velocity.x, Is.Zero);
    }
    [Test] public void FallingSpeedIsBoundedAndAirCannotDoubleJump()
    {
        Call("PressJump"); Step(true); Step(); Call("PressJump"); Step();
        Assert.That(Jumped, Is.False);
        for (int i = 0; i < 200; i++) Step();
        Assert.That(velocity.y, Is.EqualTo(-20));
    }
    [Test] public void HeldJumpReachesDocumentedApexAndFlightDistance()
    {
        Call("SetMove", 1f); for (int i = 0; i < 20; i++) Step(true);
        Call("PressJump"); float height = 0, apex = 0, distance = 0;
        do { Step(); height += velocity.y * 0.02f; distance += velocity.x * 0.02f; apex = Mathf.Max(apex, height); } while (height >= 0);
        Assert.That(apex, Is.InRange(2.2f, 2.5f));
        Assert.That(distance, Is.InRange(5.2f, 5.7f));
    }
}
