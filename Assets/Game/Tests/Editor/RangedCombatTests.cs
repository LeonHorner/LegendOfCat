using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class RangedCombatTests : InputTestFixture
{
    const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    static Type Runtime(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).First(t => t != null);
    object combat;
    object Call(string method, params object[] args) => combat.GetType().GetMethod(method).Invoke(combat, args);
    T Read<T>(string property) => (T)combat.GetType().GetProperty(property).GetValue(combat);
    IList Shots => (IList)Read<object>("Projectiles");
    Vector2 Velocity(int index) => (Vector2)Shots[index].GetType().GetField("Velocity").GetValue(Shots[index]);
    void Step(float seconds) { for (float t = 0; t < seconds - .0001f; t += .02f) Call("Advance", .02f); }
    [SetUp] public void CreateCombat() => combat = Activator.CreateInstance(Runtime("RangedCombat"));

    [Test] public void PulseIsImmediateAndDoesNotRepeatWithoutAnotherPress()
    {
        Assert.That(Call("PressPulse", new Vector2(-4, .6f)), Is.True);
        Assert.That(Velocity(0), Is.EqualTo(new Vector2(15, 0)));
        Assert.That(Call("PressPulse", Vector2.zero), Is.False);
        Step(.3f);
        Assert.That(Shots.Count, Is.EqualTo(1));
        Call("SetFacing", -1f);
        Assert.That(Call("PressPulse", new Vector2(4, .6f)), Is.True);
        Assert.That(Velocity(1).x, Is.EqualTo(-15));
    }
    [Test] public void ChargeOnlyChangesUpwardSpeedAndUsesReleaseFacing()
    {
        Call("PressLob"); Call("ReleaseLob", new Vector2(-4, .6f));
        Assert.That(Velocity(0), Is.EqualTo(new Vector2(7, 5)));
        Call("Reset"); Call("PressLob"); Step(1.2f);
        Assert.That(Read<float>("Charge"), Is.EqualTo(1));
        Assert.That(Shots.Count, Is.Zero);
        Assert.That(Call("PressPulse", Vector2.zero), Is.False, "No interleaved pulse while charging.");
        Call("SetFacing", -1f); Call("ReleaseLob", new Vector2(4, .6f));
        Assert.That(Velocity(0), Is.EqualTo(new Vector2(-7, 15)));
        Assert.That(Call("ReleaseLob", Vector2.zero), Is.False);
        Assert.That(Call("PressPulse", Vector2.zero), Is.False, "Both attacks share recovery.");
    }
    [Test] public void PauseFreezesChargeProjectilesAndTargetAndReleaseCannotGhostFire()
    {
        Call("PressPulse", new Vector2(-4, .6f)); Step(.3f); Call("PressLob"); Step(.2f);
        float elapsed = Read<float>("Elapsed"), charge = Read<float>("Charge");
        Vector2 position = (Vector2)Shots[0].GetType().GetField("Position").GetValue(Shots[0]);
        Call("SetPaused", true); Step(1);
        Assert.That(Read<float>("Elapsed"), Is.EqualTo(elapsed));
        Assert.That(Read<float>("Charge"), Is.EqualTo(charge));
        Assert.That(Shots[0].GetType().GetField("Position").GetValue(Shots[0]), Is.EqualTo(position));
        Assert.That(Call("ReleaseLob", Vector2.zero), Is.False);
        Call("SetPaused", false);
        Assert.That(Call("ReleaseLob", Vector2.zero), Is.False);
        Call("CancelTransient"); Assert.That(Shots.Count, Is.Zero);
        Call("Reset"); Assert.That(Read<int>("Health"), Is.EqualTo(6));
        Assert.That(Read<float>("Elapsed"), Is.Zero);
    }
    [Test] public void ShieldBlocksAndOpenTargetTakesExactlyOneHitPerProjectile()
    {
        Call("PressPulse", Read<Vector2>("Target")); Step(.02f);
        Assert.That(Read<int>("Health"), Is.EqualTo(6));
        Assert.That(Read<float>("BlockFlash"), Is.GreaterThan(0));
        Step(1.5f);
        Assert.That(Read<bool>("Vulnerable"), Is.True);
        Call("PressPulse", Read<Vector2>("Target")); Step(.02f);
        Assert.That(Read<int>("Health"), Is.EqualTo(5));
        Assert.That(Read<float>("HitFlash"), Is.GreaterThan(0));
        Assert.That(Shots.Count, Is.Zero);
        Step(.2f); Assert.That(Read<int>("Health"), Is.EqualTo(5));
    }
    [Test] public void DefeatClearsShotsAndPreventsFurtherAttacks()
    {
        Step(1.6f);
        for (int i = 0; i < 6; i++) { Call("PressPulse", Read<Vector2>("Target")); Step(.28f); }
        Assert.That(Read<bool>("Finished"), Is.True);
        Assert.That(Shots.Count, Is.Zero);
        Call("PressLob"); Assert.That(Read<bool>("Charging"), Is.False);
        Assert.That(Call("PressPulse", Vector2.zero), Is.False);
    }
    [Test] public void ProjectilesExpireAtFloorWallsAndLifetime()
    {
        Call("PressPulse", new Vector2(7.9f, 1)); Step(.02f); Assert.That(Shots.Count, Is.Zero);
        Call("Reset"); Call("PressLob"); Call("ReleaseLob", new Vector2(-7, .6f)); Step(1.2f);
        Assert.That(Shots.Count, Is.Zero);
        Call("Reset"); Call("PressPulse", new Vector2(-100, 7)); Step(3.2f);
        Assert.That(Shots.Count, Is.Zero);
    }
    [Test] public void RelativeSweepCatchesFastProjectileAndMovingTarget()
    {
        var method = Runtime("RangedCombat").GetMethod("CrossesTarget");
        Assert.That(method.Invoke(null, new object[] { new Vector2(-2, 0), new Vector2(2, 0), Vector2.up, Vector2.down }), Is.True);
        Assert.That(method.Invoke(null, new object[] { new Vector2(-2, 3), new Vector2(2, 3), Vector2.up, Vector2.down }), Is.False);
    }
    [Test] public void GroundedChargedOrbCanHitTheTargetWhileItIsHigh()
    {
        bool highHit = false;
        for (int holdSteps = 10; holdSteps <= 40 && !highHit; holdSteps++) {
            Call("Reset");
            Step(1.6f);
            Call("PressLob"); Step(holdSteps * .02f);
            Call("ReleaseLob", new Vector2(-3, .6f));
            for (int step = 0; step < 100; step++) {
                Call("Advance", .02f);
                if (Read<int>("Health") < 6) { highHit = Read<Vector2>("Target").y > 4.5f; break; }
            }
        }
        Assert.That(highHit, Is.True, "A ground-level charged throw must reach the high flying target without auto-aim.");
    }
    [Test] public void RealActionsAllowMoveJumpAndChargeAndCancelWithoutReleaseShot()
    {
        var keyboard = InputSystem.AddDevice<Keyboard>();
        var pad = InputSystem.AddDevice<Gamepad>();
        var host = new GameObject("Ranged input fixture");
        Component controller = host.AddComponent(Runtime("CatController"));
        Component input = host.AddComponent(Runtime("CatInput"));
        void Lifecycle(Component component, string method, params object[] args) => component.GetType().GetMethod(method, Private).Invoke(component, args);
        try {
            Lifecycle(controller, "Awake");
            input.GetType().GetField("controller").SetValue(input, controller); Lifecycle(input, "Awake");
            input.GetType().GetField("LobPressed").SetValue(input, (Action)(() => Call("PressLob")));
            input.GetType().GetField("LobReleased").SetValue(input, (Action)(() => Call("ReleaseLob", new Vector2(-4, .6f))));
            input.GetType().GetField("PulsePressed").SetValue(input, (Action)(() => Call("PressPulse", new Vector2(-4, .6f))));
            input.GetType().GetField("CombatCanceled").SetValue(input, (Action)(() => Call("CancelTransient")));
            Lifecycle(input, "OnApplicationFocus", true);
            controller.GetType().GetProperty("Grounded").SetValue(controller, true);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D, Key.Space, Key.K)); InputSystem.Update();
            Assert.That(Read<bool>("Charging"), Is.True);
            Assert.That(host.GetComponent<Rigidbody2D>().linearVelocity.y, Is.GreaterThan(0), "Jump and charge share the same input update.");
            Lifecycle(controller, "FixedUpdate");
            Assert.That(host.GetComponent<Rigidbody2D>().linearVelocity.x, Is.GreaterThan(0));
            Step(.4f);
            Lifecycle(input, "OnApplicationFocus", false);
            Assert.That(Read<bool>("Charging"), Is.False); Assert.That(Shots.Count, Is.Zero);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); InputSystem.Update();
            Lifecycle(input, "OnApplicationFocus", true);
            InputSystem.QueueStateEvent(pad, new GamepadState { rightTrigger = 1 }); InputSystem.Update();
            Assert.That(Read<bool>("Charging"), Is.True);
            Step(.4f);
            InputSystem.QueueStateEvent(pad, new GamepadState()); InputSystem.Update();
            Assert.That(Shots.Count, Is.EqualTo(1));
            Lifecycle(input, "OnDisable"); Assert.That(Shots.Count, Is.Zero);
        } finally { UnityEngine.Object.DestroyImmediate(host); }
    }
}

public class RangedIntegrationTests
{
    static Type Runtime(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).First(t => t != null);
    static object Invoke(object target, string name, params object[] args) => target.GetType()
        .GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(target, args);
    static object Property(object target, string name) => target.GetType().GetProperty(name).GetValue(target);
    [UnityEngine.TestTools.UnityTest]
    public IEnumerator MenuEntryRetryPauseReturnAndReentryKeepOneFreshArena()
    {
        UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene);
        yield return new UnityEngine.TestTools.EnterPlayMode();
        var host = new GameObject("Ranged menu integration");
        var menu = host.AddComponent(Runtime("StarterScreen"));
        Invoke(menu, "OpenRangedArena");
        var arena = UnityEngine.Object.FindFirstObjectByType(Runtime("RangedArena"));
        Assert.That(arena, Is.Not.Null);
        var combat = arena.GetType().GetField("Combat").GetValue(arena);
        var cat = (Component)Property(arena, "Cat");
        Invoke(combat, "PressLob"); Invoke(combat, "Advance", .4f);
        Invoke(arena, "TogglePause");
        Assert.That(cat.GetComponent<Rigidbody2D>().simulated, Is.False);
        Assert.That(Property(combat, "Paused"), Is.True);
        Invoke(arena, "Retry");
        Assert.That(cat.GetComponent<Rigidbody2D>().simulated, Is.True);
        Assert.That(Property(combat, "Charging"), Is.False);
        Assert.That(Property(combat, "Elapsed"), Is.EqualTo(0));
        Invoke(combat, "PressPulse", new Vector2(-4, .6f));
        Invoke(arena, "Restart");
        Assert.That(((IList)Property(combat, "Projectiles")).Count, Is.Zero);
        Assert.That(cat.transform.localPosition.x, Is.EqualTo(-4).Within(.001));
        var oldInput = cat.GetComponent(Runtime("CatInput"));
        Invoke(menu, "CloseRangedArena");
        Assert.That(((InputActionMap)Property(oldInput, "Actions")).enabled, Is.False);
        yield return null;
        Invoke(menu, "OpenRangedArena");
        Assert.That(UnityEngine.Object.FindObjectsByType(Runtime("RangedArena"), FindObjectsSortMode.None).Length, Is.EqualTo(1));
        Invoke(menu, "CloseRangedArena");
        UnityEngine.Object.Destroy(host);
        yield return null;
        yield return new UnityEngine.TestTools.ExitPlayMode();
    }
}
