using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class StompCombatTests
{
    Type type;
    object combat;
    object Call(string name, params object[] args) => type.GetMethod(name).Invoke(combat, args);
    T Read<T>(string name) => (T)type.GetProperty(name).GetValue(combat);
    [SetUp] public void SetUp()
    {
        type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("StompCombat")).First(t => t != null);
        combat = Activator.CreateInstance(type);
    }
    [Test] public void OnlyRecoveryAllowsOneTopHitPerCycle()
    {
        Assert.That(Call("TryStomp", true), Is.False);
        Call("Advance", 1.2f); Assert.That(Read<object>("Phase").ToString(), Is.EqualTo("Attack"));
        Assert.That(Call("TryStomp", true), Is.False);
        Call("Advance", .35f);
        Assert.That(Call("TryStomp", false), Is.False);
        Assert.That(Call("TryStomp", true), Is.True);
        Assert.That(Read<int>("BossHealth"), Is.EqualTo(2));
        Assert.That(Call("TryStomp", true), Is.False);
    }
    [Test] public void DamageProtectionExpiresAndRetryRestoresEverything()
    {
        Assert.That(Call("TryDamage"), Is.True);
        Assert.That(Call("TryDamage"), Is.False);
        Call("Advance", 1.11f);
        Assert.That(Call("TryDamage"), Is.True);
        Call("Advance", 1.11f); Call("TryDamage");
        Assert.That(Read<bool>("Finished"), Is.True);
        Call("Reset");
        Assert.That(Read<int>("PlayerHealth"), Is.EqualTo(3));
        Assert.That(Read<int>("BossHealth"), Is.EqualTo(3));
        Assert.That(Read<float>("Invulnerability"), Is.Zero);
        Assert.That(Read<float>("PhaseTime"), Is.Zero);
        Assert.That(Read<bool>("Finished"), Is.False);
    }
    [Test] public void BossDefeatDisablesFurtherDamageAndHits()
    {
        for (int i = 0; i < 3; i++) { Call("Advance", 1.551f); Assert.That(Call("TryStomp", true), Is.True); }
        Assert.That(Read<bool>("Finished"), Is.True);
        Assert.That(Call("TryDamage"), Is.False);
        Assert.That(Call("TryStomp", true), Is.False);
    }
    [TestCase(0, 3, 0, 0, true)]
    [TestCase(0, 0, 0, 3, false)]
    [TestCase(2, 1, 0, 1, false)]
    [TestCase(3, 3, 3, 0, false)]
    public void SweptFeetRejectSideUndersideAndMisses(float x0, float y0, float x1, float y1, bool expected)
    {
        Assert.That(Call("CrossedHead", new Vector2(x0, y0), new Vector2(x1, y1),
            Vector2.one * .45f, new Rect(-.85f, 0, 1.7f, 1.35f)), Is.EqualTo(expected));
    }
    [Test] public void AutomaticBounceIgnoresReleasedJumpAndResetClearsIt()
    {
        var motionType = type.Assembly.GetType("CatMotion");
        var motion = Activator.CreateInstance(motionType);
        motionType.GetMethod("Launch").Invoke(motion, new object[] { new Vector2(0, 10) });
        var velocity = (Vector2)motionType.GetMethod("Step").Invoke(motion, new object[] { .02f, false, new Vector2(0, 10) });
        Assert.That(velocity.y, Is.EqualTo(9.4f).Within(.001f));
        motionType.GetMethod("Reset").Invoke(motion, null);
        velocity = (Vector2)motionType.GetMethod("Step").Invoke(motion, new object[] { .02f, false, new Vector2(0, 10) });
        Assert.That(velocity.y, Is.LessThan(5));
    }
    [Test] public void RetryAndRestartRestoreArenaAndUnpausePhysics()
    {
        var root = new GameObject("Stomp lifecycle test");
        try {
            var arenaType = type.Assembly.GetType("StompArena");
            var arena = root.AddComponent(arenaType);
            arenaType.GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(arena, null);
            var state = arenaType.GetField("Combat").GetValue(arena);
            var cat = (Component)arenaType.GetProperty("Cat").GetValue(arena);
            cat.GetType().GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(cat, null);
            var input = cat.GetComponent(type.Assembly.GetType("CatInput"));
            input.GetType().GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(input, null);
            type.GetMethod("TryDamage").Invoke(state, null);
            arenaType.GetMethod("TogglePause").Invoke(arena, null);
            Assert.That(cat.GetComponent<Rigidbody2D>().simulated, Is.False);
            arenaType.GetMethod("Retry").Invoke(arena, null);
            Assert.That(cat.GetComponent<Rigidbody2D>().simulated, Is.True);
            Assert.That(type.GetProperty("PlayerHealth").GetValue(state), Is.EqualTo(3));
            Assert.That(cat.GetComponent<Rigidbody2D>().linearVelocity, Is.EqualTo(Vector2.zero));
            arenaType.GetMethod("Restart").Invoke(arena, null);
            Assert.That(cat.transform.localPosition.x, Is.EqualTo(-4).Within(.001));
        } finally { UnityEngine.Object.DestroyImmediate(root); }
    }
}
