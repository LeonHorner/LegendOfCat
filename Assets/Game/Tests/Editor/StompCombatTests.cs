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
        Call("Advance", 1.26f);
        Assert.That(Call("TryDamage"), Is.True);
        Call("Advance", 1.26f); Call("TryDamage");
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
    [TestCase(-1.1f, 7f)]
    [TestCase(1.1f, -7f)]
    [TestCase(0f, 0f)]
    public void DamageHopPreservesHorizontalMotionAndProtectionPulses(float x, float expectedVelocity)
    {
        var root = new GameObject("Stomp damage test");
        try {
            var arenaType = type.Assembly.GetType("StompArena");
            var arena = root.AddComponent(arenaType);
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            arenaType.GetMethod("Awake", flags).Invoke(arena, null);
            var cat = (Component)arenaType.GetProperty("Cat").GetValue(arena);
            cat.GetType().GetMethod("Awake", flags).Invoke(cat, null);
            var input = cat.GetComponent(type.Assembly.GetType("CatInput"));
            input.GetType().GetMethod("Awake", flags).Invoke(input, null);
            var body = cat.GetComponent<Rigidbody2D>();
            var start = new Vector2(x, .5f);
            body.position = start;
            body.linearVelocity = new Vector2(expectedVelocity, -2);
            arenaType.GetField("previous", flags).SetValue(arena, start);
            var fixedStep = arenaType.GetMethod("FixedUpdate", flags);
            var present = arenaType.GetMethod("Present", flags);
            var state = arenaType.GetField("Combat").GetValue(arena);
            fixedStep.Invoke(arena, null);
            Assert.That(body.position, Is.EqualTo(start));
            Assert.That(body.linearVelocity, Is.EqualTo(new Vector2(expectedVelocity, 7)));
            Assert.That(type.GetProperty("PlayerHealth").GetValue(state), Is.EqualTo(2));
            var burst = (Transform)arenaType.GetField("damageBurst", flags).GetValue(arena);
            Vector3 contact = burst.localPosition;
            float burstBefore = (float)arenaType.GetField("burstTime", flags).GetValue(arena);
            Assert.That(burst.gameObject.activeSelf, Is.True);
            var audio = cat.GetComponent<AudioSource>();
            Assert.That(audio.clip, Is.Not.Null);
            Assert.That(audio.clip.name, Is.EqualTo("StompDamageVoice"));
            Assert.That(audio.clip.length, Is.EqualTo(.9f).Within(.005f));
            Assert.That(audio.playOnAwake || audio.loop, Is.False);
            Assert.That(audio.spatialBlend, Is.Zero);
            body.linearVelocity = Vector2.zero;
            fixedStep.Invoke(arena, null);
            Assert.That(body.linearVelocity, Is.EqualTo(Vector2.zero), "Protected contact must not launch again");
            Assert.That(type.GetProperty("PlayerHealth").GetValue(state), Is.EqualTo(2));
            Assert.That(burst.localPosition, Is.EqualTo(contact), "The star stays at the original contact");
            Assert.That((float)arenaType.GetField("burstTime", flags).GetValue(arena), Is.LessThan(burstBefore));
            var sprite = cat.GetComponentInChildren<SpriteRenderer>();
            float initialAlpha = ((Color)arenaType.GetField("playerColor", flags).GetValue(arena)).a;
            type.GetMethod("Advance").Invoke(state, new object[] { .125f - 2 * Time.fixedDeltaTime });
            present.Invoke(arena, null);
            Assert.That(sprite.color.a, Is.EqualTo(initialAlpha * .5f).Within(.002f));
            arenaType.GetMethod("TogglePause").Invoke(arena, null);
            float remaining = (float)type.GetProperty("Invulnerability").GetValue(state);
            float alpha = sprite.color.a;
            float targetX = (float)type.GetProperty("TargetX").GetValue(state);
            float burstPaused = (float)arenaType.GetField("burstTime", flags).GetValue(arena);
            fixedStep.Invoke(arena, null);
            Assert.That(type.GetProperty("Invulnerability").GetValue(state), Is.EqualTo(remaining));
            Assert.That(sprite.color.a, Is.EqualTo(alpha));
            Assert.That(arenaType.GetField("burstTime", flags).GetValue(arena), Is.EqualTo(burstPaused));
            Assert.That(type.GetProperty("TargetX").GetValue(state), Is.EqualTo(targetX));
            arenaType.GetMethod("Retry").Invoke(arena, null);
            Assert.That(sprite.color.a, Is.EqualTo(initialAlpha));
            Assert.That(burst.gameObject.activeSelf, Is.False);
            Assert.That(audio.isPlaying, Is.False);
            Assert.That(type.GetProperty("Invulnerability").GetValue(state), Is.EqualTo(0));
        } finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    [Test] public void ProtectionLastsTheFullLegacyDuration()
    {
        Call("TryDamage");
        Call("Advance", 1.249f);
        Assert.That(Call("TryDamage"), Is.False);
        Call("Advance", .002f);
        Assert.That(Call("TryDamage"), Is.True);
    }
    [TestCase(-1f)] [TestCase(1f)]
    public void DamageHopKeepsHeldRunDirectionAndIgnoresJumpRelease(float direction)
    {
        var motionType = type.Assembly.GetType("CatMotion");
        var motion = Activator.CreateInstance(motionType);
        var velocity = new Vector2(direction * 7, 7);
        motionType.GetMethod("SetMove").Invoke(motion, new object[] { direction });
        motionType.GetMethod("Launch").Invoke(motion, new object[] { velocity });
        velocity = (Vector2)motionType.GetMethod("Step").Invoke(motion, new object[] { .02f, false, velocity });
        Assert.That(velocity.x, Is.EqualTo(direction * 7).Within(.001f));
        Assert.That(velocity.y, Is.EqualTo(6.4f).Within(.001f));
        motionType.GetMethod("Reset").Invoke(motion, null);
        velocity = (Vector2)motionType.GetMethod("Step").Invoke(motion, new object[] { .02f, false, Vector2.zero });
        Assert.That(velocity.x, Is.Zero);
    }

    [Test] public void TargetKeepsMovingThroughoutRecoveryAndResetsOnRetry()
    {
        Call("Advance", 1.56f);
        float before = Read<float>("TargetX");
        Assert.That(Read<object>("Phase").ToString(), Is.EqualTo("Recovery"));
        Call("Advance", .3f);
        Assert.That(Read<float>("TargetX"), Is.GreaterThan(before));
        Call("TryStomp", true);
        Assert.That(Read<float>("TargetX"), Is.GreaterThan(before), "A hit must not teleport the target");
        Call("Reset");
        Assert.That(Read<float>("TargetX"), Is.Zero);
    }
    [Test] public void MovingHeadCrossingUsesRelativeMotion()
    {
        // Stationary player's descending feet; target moves under the landing.
        var before = new Vector2(1.4f, 2.5f) - new Vector2(0, 0);
        var after = new Vector2(1.4f, 1.5f) - new Vector2(1, 0);
        Assert.That(Call("CrossedHead", before, after, Vector2.one * .45f,
            new Rect(-.85f, 0, 1.7f, 1.35f)), Is.True);
    }

    [Test] public void TargetMotionIsBoundedAndContinuousAcrossTurns()
    {
        float previousX = 0;
        for (int i = 0; i < 600; i++) {
            Call("Advance", .02f);
            float x = Read<float>("TargetX");
            Assert.That(x, Is.InRange(-2f, 2f));
            Assert.That(Mathf.Abs(x - previousX), Is.LessThanOrEqualTo(.0241f));
            previousX = x;
        }
    }
    [Test] public void SignalsFollowTheExactAttackAndRecoveryState()
    {
        var root = new GameObject("Stomp signal state test");
        try {
            var arenaType = type.Assembly.GetType("StompArena");
            var arena = root.AddComponent(arenaType);
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            arenaType.GetMethod("Awake", flags).Invoke(arena, null);
            var cat = (Component)arenaType.GetProperty("Cat").GetValue(arena);
            cat.GetType().GetMethod("Awake", flags).Invoke(cat, null);
            var input = cat.GetComponent(type.Assembly.GetType("CatInput"));
            input.GetType().GetMethod("Awake", flags).Invoke(input, null);
            var state = arenaType.GetField("Combat").GetValue(arena);
            var present = arenaType.GetMethod("Present", flags);
            var arrows = (SpriteRenderer[])arenaType.GetField("headArrows", flags).GetValue(arena);
            var locks = (SpriteRenderer[])arenaType.GetField("headLocks", flags).GetValue(arena);
            var marks = (SpriteRenderer[])arenaType.GetField("attackMarks", flags).GetValue(arena);
            Assert.That(locks.All(r => r.enabled), Is.True);
            Assert.That(arrows.Any(r => r.enabled) || marks.Any(r => r.enabled), Is.False);
            type.GetMethod("Advance").Invoke(state, new object[] { 1.2f });
            present.Invoke(arena, null);
            Assert.That(marks.All(r => r.enabled), Is.True);
            foreach (var mark in marks) {
                float half = mark.transform.localScale.y * .5f;
                Assert.That(mark.transform.localPosition.y - half, Is.GreaterThanOrEqualTo(0));
                Assert.That(mark.transform.localPosition.y + half, Is.LessThanOrEqualTo(.35f));
            }
            type.GetMethod("Advance").Invoke(state, new object[] { .351f });
            present.Invoke(arena, null);
            Assert.That(arrows.All(r => r.enabled), Is.True);
            Assert.That(locks.Any(r => r.enabled) || marks.Any(r => r.enabled), Is.False);
            arenaType.GetMethod("Retry").Invoke(arena, null);
            Assert.That(locks.All(r => r.enabled), Is.True);
            Assert.That(arrows.Any(r => r.enabled), Is.False);
        } finally { UnityEngine.Object.DestroyImmediate(root); }
    }

}
