using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class RunLifecycleTests
{
    Type lifecycleType;
    object lifecycle;
    object Call(string method, params object[] args) => lifecycleType.GetMethod(method).Invoke(lifecycle, args);
    T Read<T>(string property) => (T)lifecycleType.GetProperty(property).GetValue(lifecycle);

    [SetUp] public void SetUp()
    {
        lifecycleType = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("RunLifecycle"))
            .First(t => t != null);
        lifecycle = Activator.CreateInstance(lifecycleType, new Vector2(-3, 0.48f));
    }

    [Test] public void OnlySafeForwardCheckpointsActivateAndRetryReturnsTheirFixedSpawn()
    {
        var first = new Vector2(9, 0.52f);
        var second = new Vector2(34, 1.52f);
        Assert.That((bool)Call("TryActivateCheckpoint", 1, first, false), Is.False);
        Assert.That(Read<int>("Checkpoint"), Is.Zero);
        Assert.That((bool)Call("TryActivateCheckpoint", 1, first, true), Is.True);
        Assert.That((bool)Call("TryActivateCheckpoint", 1, Vector2.zero, true), Is.False);
        Assert.That((bool)Call("TryActivateCheckpoint", 2, second, true), Is.True);
        Assert.That((bool)Call("TryActivateCheckpoint", 1, first, true), Is.False);
        Assert.That((Vector2)Call("Retry"), Is.EqualTo(second));
        Assert.That(Read<int>("Retries"), Is.EqualTo(1));
        Assert.That(Read<int>("Checkpoint"), Is.EqualTo(2));
    }

    [Test] public void RetryResetsEncounterAndProjectilesWithoutResettingTheRunClock()
    {
        bool projectileActive = true;
        int encounterPhase = 3;
        Action reset = () => { projectileActive = false; encounterPhase = 0; };
        lifecycleType.GetEvent("ResetTransientState").AddEventHandler(lifecycle, reset);
        Call("Advance", 5f);
        Assert.That((Vector2)Call("Retry"), Is.EqualTo(new Vector2(-3, 0.48f)));
        Assert.That(projectileActive, Is.False);
        Assert.That(encounterPhase, Is.Zero);
        Assert.That(Read<float>("ElapsedSeconds"), Is.EqualTo(5f));
    }

    [Test] public void PauseStopsTimeAndCheckpointActivationAndRetryExitsPause()
    {
        Call("Advance", 1f);
        Assert.That((bool)Call("TogglePause"), Is.True);
        Call("Advance", 3f);
        Assert.That((bool)Call("TryActivateCheckpoint", 1, new Vector2(9, 0.52f), true), Is.False);
        Assert.That(Read<float>("ElapsedSeconds"), Is.EqualTo(1f));
        Call("Retry");
        Assert.That(Read<bool>("Paused"), Is.False);
        Call("Advance", 2f);
        Assert.That(Read<float>("ElapsedSeconds"), Is.EqualTo(3f));
    }

    [Test] public void FullRestartClearsOnlySessionStateAndCallsTransientReset()
    {
        int resetCount = 0;
        Action reset = () => resetCount++;
        lifecycleType.GetEvent("ResetTransientState").AddEventHandler(lifecycle, reset);
        Call("TryActivateCheckpoint", 1, new Vector2(9, 0.52f), true);
        Call("Retry");
        Call("Advance", 8f);
        Call("TogglePause");
        Assert.That((Vector2)Call("Restart"), Is.EqualTo(new Vector2(-3, 0.48f)));
        Assert.That(Read<int>("Checkpoint"), Is.Zero);
        Assert.That(Read<int>("Retries"), Is.Zero);
        Assert.That(Read<float>("ElapsedSeconds"), Is.Zero);
        Assert.That(Read<bool>("Paused"), Is.False);
        Assert.That(resetCount, Is.EqualTo(2));
    }
}
