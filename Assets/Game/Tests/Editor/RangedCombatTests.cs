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

    [Test] public void PulseMagazineAutomaticallyReloadsAndRequiresFreshPress()
    {
        for (int i = 0; i < 8; i++) {
            int before = Read<int>("Ammo");
            Call("PressAttack", new Vector2(7.9f, 1));
            Assert.That(Read<int>("Ammo"), Is.EqualTo(before - 1));
            if (i < 7) Step(.22f);
        }
        Assert.That(Read<bool>("Reloading"), Is.True);
        Assert.That(Read<float>("ReloadRemaining"), Is.EqualTo(1));
        Call("PressAttack", Vector2.zero); Assert.That(Read<int>("Ammo"), Is.Zero);
        Step(1.02f);
        Assert.That(Read<int>("Ammo"), Is.EqualTo(8));
        Assert.That(Shots.Count, Is.Zero, "Reload completion must not repeat a held attack.");
        Call("PressAttack", new Vector2(-4, .6f)); Assert.That(Read<int>("Ammo"), Is.EqualTo(7));
        Assert.That(Velocity(0), Is.EqualTo(new Vector2(15, 0)));
    }
    [Test] public void OrbChargeConsumesOneRoundOnReleaseAndHasItsOwnRecovery()
    {
        Call("SwitchWeapon"); Call("PressAttack", Vector2.zero);
        Step(1.2f); Assert.That(Read<float>("Charge"), Is.EqualTo(1));
        Assert.That(Read<int>("Ammo"), Is.EqualTo(3));
        Call("SetFacing", -1f); Call("ReleaseAttack", new Vector2(4, .6f));
        Assert.That(Velocity(0), Is.EqualTo(new Vector2(-7, 15)));
        Assert.That(Read<int>("Ammo"), Is.EqualTo(2));
        Assert.That(Read<float>("Cooldown"), Is.EqualTo(.4f));
        Call("PressAttack", Vector2.zero); Assert.That(Read<bool>("Charging"), Is.False);
        Step(.42f); Call("PressAttack", Vector2.zero); Call("ReleaseAttack", new Vector2(4, .6f));
        Step(.42f); Call("PressAttack", Vector2.zero); Call("ReleaseAttack", new Vector2(4, .6f));
        Assert.That(Read<int>("Ammo"), Is.Zero);
        Assert.That(Read<float>("ReloadRemaining"), Is.EqualTo(1.5f));
        Step(1.52f); Assert.That(Read<int>("Ammo"), Is.EqualTo(3));
    }
    [Test] public void SwitchingCancelsReloadAndEmptyWeaponRestartsFromFullDuration()
    {
        for (int i = 0; i < 8; i++) { Call("PressAttack", new Vector2(7.9f, 1)); if (i < 7) Step(.22f); }
        Step(.4f); Call("SwitchWeapon");
        Assert.That(Read<bool>("Reloading"), Is.False);
        Step(2); Call("SwitchWeapon");
        Assert.That(Read<int>("Ammo"), Is.Zero);
        Assert.That(Read<float>("ReloadRemaining"), Is.EqualTo(1));
        Step(1.02f); Assert.That(Read<int>("Ammo"), Is.EqualTo(8));
    }
    [Test] public void EarlyReloadKeepsAmmoUntilCompleteAndSwitchingDiscardsProgress()
    {
        Call("Reload"); Assert.That(Read<bool>("Reloading"), Is.False);
        Call("PressAttack", new Vector2(7.9f, 1)); Call("Reload"); Step(.4f);
        Assert.That(Read<int>("Ammo"), Is.EqualTo(7));
        Call("SwitchWeapon"); Step(2); Call("SwitchWeapon");
        Assert.That(Read<int>("Ammo"), Is.EqualTo(7));
        Assert.That(Read<bool>("Reloading"), Is.False);
        Call("Reload"); Step(1.02f); Assert.That(Read<int>("Ammo"), Is.EqualTo(8));
    }
    [Test] public void SwitchingCannotBypassCooldownAndChargeCancellationCostsNothing()
    {
        Call("PressAttack", new Vector2(-4, .6f));
        Call("SwitchWeapon"); Call("PressAttack", Vector2.zero); Step(.06f);
        Call("SwitchWeapon"); Assert.That(Read<int>("Ammo"), Is.EqualTo(7));
        Call("PressAttack", Vector2.zero); Assert.That(Read<int>("Ammo"), Is.EqualTo(7));
        Step(.16f); Call("PressAttack", new Vector2(-4, .6f)); Assert.That(Read<int>("Ammo"), Is.EqualTo(6));
        Call("SwitchWeapon"); Assert.That(Read<int>("Ammo"), Is.EqualTo(3));
        Assert.That(Read<bool>("Charging"), Is.False);
        Assert.That(Call("ReleaseAttack", Vector2.zero), Is.False);
    }
    [Test] public void PauseFreezesReloadAndFocusCancellationCannotFire()
    {
        Call("PressAttack", new Vector2(-4, .6f)); Call("Reload"); Step(.2f);
        float remaining = Read<float>("ReloadRemaining"), elapsed = Read<float>("Elapsed");
        Call("SetPaused", true); Step(1); Call("SwitchWeapon");
        Assert.That(Read<float>("ReloadRemaining"), Is.EqualTo(remaining));
        Assert.That(Read<float>("Elapsed"), Is.EqualTo(elapsed));
        Call("SetPaused", false); Call("SwitchWeapon"); Call("PressAttack", Vector2.zero); Step(.2f);
        Call("CancelTransient"); Assert.That(Call("ReleaseAttack", Vector2.zero), Is.False);
        Assert.That(Read<int>("Ammo"), Is.EqualTo(3)); Assert.That(Shots.Count, Is.Zero);
        Call("Reset"); Assert.That(Read<int>("Ammo"), Is.EqualTo(8));
        Assert.That(Read<bool>("Reloading"), Is.False); Assert.That(Read<float>("Cooldown"), Is.Zero);
    }
    [Test] public void ShieldBlocksOpenTargetTakesDamageAndDefeatStopsAttacks()
    {
        Call("PressAttack", Read<Vector2>("Target")); Step(.02f);
        Assert.That(Read<int>("Health"), Is.EqualTo(6)); Assert.That(Read<float>("BlockFlash"), Is.GreaterThan(0));
        Step(1.6f);
        for (int i = 0; i < 6; i++) { Call("PressAttack", Read<Vector2>("Target")); Step(.22f); }
        Assert.That(Read<bool>("Finished"), Is.True); Assert.That(Shots.Count, Is.Zero);
        int ammo = Read<int>("Ammo"); Call("PressAttack", Vector2.zero); Call("Reload"); Call("SwitchWeapon");
        Assert.That(Read<int>("Ammo"), Is.EqualTo(ammo));
    }
    [Test] public void ProjectilesExpireAtFloorAndWalls()
    {
        Call("PressAttack", new Vector2(7.9f, 1)); Step(.02f); Assert.That(Shots.Count, Is.Zero);
        Call("Reset"); Call("SwitchWeapon"); Call("PressAttack", Vector2.zero); Call("ReleaseAttack", new Vector2(-7, .6f)); Step(1.2f);
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
            Call("SwitchWeapon"); Call("PressAttack", Vector2.zero); Step(holdSteps * .02f);
            Call("ReleaseAttack", new Vector2(-3, .6f));
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
            input.GetType().GetField("AttackPressed").SetValue(input, (Action)(() => Call("PressAttack", new Vector2(-4, .6f))));
            input.GetType().GetField("AttackReleased").SetValue(input, (Action)(() => Call("ReleaseAttack", new Vector2(-4, .6f))));
            input.GetType().GetField("UseCombatReload").SetValue(input, true);
            input.GetType().GetField("WeaponSwitched").SetValue(input, (Action)(() => Call("SwitchWeapon")));
            input.GetType().GetField("ReloadPressed").SetValue(input, (Action)(() => Call("Reload")));
            Call("SwitchWeapon");
            input.GetType().GetField("CombatCanceled").SetValue(input, (Action)(() => Call("CancelTransient")));
            Lifecycle(input, "OnApplicationFocus", true);
            controller.GetType().GetProperty("Grounded").SetValue(controller, true);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D, Key.Space, Key.F)); InputSystem.Update();
            Assert.That(Read<bool>("Charging"), Is.True);
            Assert.That(host.GetComponent<Rigidbody2D>().linearVelocity.y, Is.GreaterThan(0), "Jump and charge share the same input update.");
            Lifecycle(controller, "FixedUpdate");
            Assert.That(host.GetComponent<Rigidbody2D>().linearVelocity.x, Is.GreaterThan(0));
            Step(.4f);
            Lifecycle(input, "OnApplicationFocus", false);
            Assert.That(Read<bool>("Charging"), Is.False); Assert.That(Shots.Count, Is.Zero);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); InputSystem.Update();
            Lifecycle(input, "OnApplicationFocus", true);
            InputSystem.QueueStateEvent(pad, new GamepadState().WithButton(GamepadButton.West)); InputSystem.Update();
            Assert.That(Read<bool>("Charging"), Is.True);
            Step(.4f);
            InputSystem.QueueStateEvent(pad, new GamepadState()); InputSystem.Update();
            Assert.That(Shots.Count, Is.EqualTo(1));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.R)); InputSystem.Update();
            Assert.That(Read<bool>("Reloading"), Is.True, "R starts reload rather than retry.");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Q)); InputSystem.Update();
            Assert.That(Read<bool>("Reloading"), Is.False);
            Assert.That(Read<int>("Capacity"), Is.EqualTo(8));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); InputSystem.Update();
            InputSystem.QueueStateEvent(pad, new GamepadState { rightTrigger = 1 }); InputSystem.Update();
            Assert.That(Read<int>("Capacity"), Is.EqualTo(3));
            InputSystem.QueueStateEvent(pad, new GamepadState()); InputSystem.Update();
            InputSystem.QueueStateEvent(pad, new GamepadState().WithButton(GamepadButton.North)); InputSystem.Update();
            Assert.That(Read<bool>("Reloading"), Is.True);
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
        Invoke(combat, "SwitchWeapon"); Invoke(combat, "PressAttack", Vector2.zero); Invoke(combat, "Advance", .4f);
        Invoke(arena, "TogglePause");
        Assert.That(cat.GetComponent<Rigidbody2D>().simulated, Is.False);
        Assert.That(Property(combat, "Paused"), Is.True);
        Invoke(arena, "Retry");
        Assert.That(cat.GetComponent<Rigidbody2D>().simulated, Is.True);
        Assert.That(Property(combat, "Charging"), Is.False);
        Assert.That(Property(combat, "Elapsed"), Is.EqualTo(0));
        Invoke(combat, "PressAttack", new Vector2(-4, .6f));
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
