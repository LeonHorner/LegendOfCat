using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class DesktopBehaviorTests : InputTestFixture
{
    static Type Runtime(string name) => AppDomain.CurrentDomain.GetAssemblies()
        .Select(a => a.GetType(name)).First(t => t != null);
    static object Call(object state, string method, params object[] args) => state.GetType().GetMethod(method).Invoke(state, args);
    static object Property(object state, string name) => state.GetType().GetProperty(name).GetValue(state);
    static object State() => Activator.CreateInstance(Runtime("DisplayState"), 1280, 720, false);

    [Test]
    public void KeyboardTakeoffFeedbackIsImmediateAndReleaseReachesTheNextPhysicsStep()
    {
        var keyboard = InputSystem.AddDevice<Keyboard>();
        var host = new GameObject("Keyboard takeoff fixture");
        var controller = host.AddComponent(Runtime("CatController"));
        var input = host.AddComponent(Runtime("CatInput"));
        var body = host.GetComponent<Rigidbody2D>();
        InputActionMap actions = null;
        PhysicsMaterial2D material = null;
        void Lifecycle(Component target, string name, params object[] args) => target.GetType()
            .GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, args);
        try {
            Lifecycle(controller, "Awake");
            material = host.GetComponent<BoxCollider2D>().sharedMaterial;
            // The real course/physics fixture separately establishes grounded contact.
            controller.GetType().GetProperty("Grounded").SetValue(controller, true);
            input.GetType().GetField("controller").SetValue(input, controller);
            Lifecycle(input, "Awake"); Lifecycle(input, "OnApplicationFocus", true);
            actions = (InputActionMap)Property(input, "Actions");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D, Key.Space));
            InputSystem.Update();
            Assert.That(Property(controller, "Grounded"), Is.False);
            Assert.That(Property(controller, "Pose"), Is.EqualTo("Jump"));
            Assert.That(body.linearVelocity.y, Is.EqualTo(12), "Takeoff state must be visible before FixedUpdate.");
            Assert.That(body.position, Is.EqualTo(Vector2.zero), "Input must not advance physics or teleport the body.");
            Lifecycle(controller, "FixedUpdate");
            Assert.That(body.linearVelocity.x, Is.GreaterThan(1));
            Assert.That(body.linearVelocity.y, Is.EqualTo(11.4f).Within(0.001f));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update(); Lifecycle(controller, "FixedUpdate");
            Assert.That(body.linearVelocity.x, Is.Zero);
            Assert.That(body.linearVelocity.y, Is.LessThan(5));
        } finally {
            actions?.Dispose();
            host.GetComponent<BoxCollider2D>().sharedMaterial = null;
            controller.GetType().GetField("material", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(controller, null);
            UnityEngine.Object.DestroyImmediate(host);
            if (material) UnityEngine.Object.DestroyImmediate(material);
            InputSystem.RemoveDevice(keyboard);
        }
    }

    [Test]
    public void ResizeDebouncesAndFlushesTheLastWindowSize()
    {
        var state = State();
        Assert.That(Call(state, "Observe", 960, 540, false, 0f, false), Is.False);
        Assert.That(Call(state, "Observe", 1000, 600, false, 1f, false), Is.False);
        Assert.That(Call(state, "Observe", 1000, 600, false, 2f, false), Is.False);
        Assert.That(Call(state, "Observe", 1000, 600, false, 3f, false), Is.True);
        Assert.That(Property(state, "Width"), Is.EqualTo(1000));
        Assert.That(Call(state, "Observe", 1000, 600, false, 8f, false), Is.False);
        Assert.That(Call(state, "Observe", 960, 540, false, 9f, true), Is.True);
        Assert.That(Property(state, "Width"), Is.EqualTo(960));
    }

    [Test]
    public void FullscreenWaitsForUnityAndPreservesWindowDimensions()
    {
        var state = State();
        Call(state, "Request", true, 0f);
        Assert.That(Call(state, "Observe", 1280, 720, false, 0.1f, false), Is.False);
        Assert.That(Call(state, "Observe", 1920, 1080, true, 0.2f, false), Is.False);
        Call(state, "Observe", 1920, 1080, true, 0.3f, false);
        Call(state, "Observe", 1920, 1080, true, 1f, false);
        Assert.That(Property(state, "Pending"), Is.True, "Persistence remains pending until the observed mode is stable.");
        Assert.That(Call(state, "Observe", 1920, 1080, true, 2f, false), Is.True);
        Assert.That(Property(state, "Fullscreen"), Is.True);
        Assert.That(Property(state, "Width"), Is.EqualTo(1280));
        Call(state, "Request", false, 3f);
        Assert.That(Call(state, "Observe", 1920, 1080, true, 3.1f, true), Is.False);
        Call(state, "Observe", 1280, 720, false, 3.2f, false);
        Call(state, "Observe", 1280, 720, false, 3.3f, false);
        Assert.That(Call(state, "Observe", 1280, 720, false, 5f, false), Is.True);
        Assert.That(Property(state, "Fullscreen"), Is.False);
    }

    [Test]
    public void RejectedRequestTimesOutWithoutPersistingTheRequestedMode()
    {
        var state = State();
        Call(state, "Request", true, 0f);
        Call(state, "Observe", 1280, 720, false, 0.1f, false);
        Call(state, "Observe", 1280, 720, false, 1f, false);
        Assert.That(Call(state, "Observe", 1280, 720, false, 6f, false), Is.False);
        Assert.That(Property(state, "Pending"), Is.False);
        Assert.That(Property(state, "Fullscreen"), Is.False);
    }

    static Array Parse(string text) => (Array)Runtime("ReleaseHistory").GetMethod("Parse").Invoke(null, new object[] { text });
    static string Text(object block) => (string)block.GetType().GetField("Text").GetValue(block);

    [TestCase("0.2.0 (2026-10-23)", "v0.2.0", true)]
    [TestCase("0.2.0", "v0.2.0", true)]
    [TestCase("Bug Fixes", "v0.2.0", false)]
    public void RepeatedVersionHeadingsUseTheLaunchersTagFormat(string heading, string version, bool expected)
    {
        Assert.That(Runtime("ReleaseHistory").GetMethod("IsVersionHeading").Invoke(null, new object[] { heading, version }), Is.EqualTo(expected));
    }

    [Test]
    public void DownloadSectionEndsAtConventionalReleaseHeading()
    {
        var blocks = Parse("## Download LegendOfCat\n- [installer](https://example.org/setup)\n> Early development\n# [0.2.0](https://example.org)\n### Bug Fixes\n* **menu:** preserve *settings* ([abcdef123](https://example.org/commit))");
        Assert.That(blocks.Length, Is.EqualTo(3));
        Assert.That(Text(blocks.GetValue(0)), Is.EqualTo("0.2.0"));
        Assert.That(blocks.GetValue(1).GetType().GetField("Heading").GetValue(blocks.GetValue(1)), Is.True);
        Assert.That(Text(blocks.GetValue(2)), Is.EqualTo("• menu: preserve settings"));
    }

    [TestCase("1. [Readable link](https://example.org/a_(b))", "• Readable link")]
    [TestCase("> A `code` and **bold** note", "A code and bold note")]
    [TestCase("- [x] Completed task", "• Completed task")]
    [TestCase("https://example.org/long/path", "")]
    [TestCase("[abcdef123](https://example.org/commit)", "")]
    [TestCase("Fix layout ([#36](https://example.org/pull/36))", "Fix layout")]
    [TestCase("### Fixes ###", "Fixes")]
    [TestCase("Commit abcdef123", "Commit")]
    public void CompactMarkdownHasReadableText(string markdown, string expected)
    {
        var blocks = Parse(markdown);
        Assert.That(blocks.Length == 0 ? "" : Text(blocks.GetValue(0)), Is.EqualTo(expected));
    }

    [TestCase(
        "* integrate selected Golden River scenery into movement prototype ([#54](https://github.com/LeonHorner/LegendOfCat/issues/54)) ([7706517](https://github.com/LeonHorner/LegendOfCat/commit/7706517fd1b79802a4e29d825f0fb758b7c06163)), closes [#51](https://github.com/LeonHorner/LegendOfCat/issues/51) [#53](https://github.com/LeonHorner/LegendOfCat/issues/53) [#50](https://github.com/LeonHorner/LegendOfCat/issues/50) [#16](https://github.com/LeonHorner/LegendOfCat/issues/16) [#16](https://github.com/LeonHorner/LegendOfCat/issues/16)",
        "• integrate selected Golden River scenery into movement prototype")]
    [TestCase(
        "* complete Mika's grounded character animation ([#45](https://github.com/LeonHorner/LegendOfCat/issues/45)) ([d01d470](https://github.com/LeonHorner/LegendOfCat/commit/d01d470ff75714101e2a94808d715f71e414692b)), closes [#10](https://github.com/LeonHorner/LegendOfCat/issues/10) [#10](https://github.com/LeonHorner/LegendOfCat/issues/10) [#41](https://github.com/LeonHorner/LegendOfCat/issues/41)",
        "• complete Mika's grounded character animation")]
    [TestCase("* **linux:** preserve AppImage update handoff ([#42](https://github.com/LeonHorner/LegendOfCat/issues/42)) ([e1f088c](https://github.com/LeonHorner/LegendOfCat/commit/e1f088c5bb0ffd14da451d87b67e315eec2001e2)), closes [#40](https://github.com/LeonHorner/LegendOfCat/issues/40)",
        "• linux: preserve AppImage update handoff")]
    [TestCase("- Fix checkpoint retry, closes #31 #31, #42", "• Fix checkpoint retry")]
    [TestCase("- Fix checkpoint retry, ReSoLvEs [#31](https://github.com/issues/31)", "• Fix checkpoint retry")]
    [TestCase("- Fix checkpoint retry, fixes #42", "• Fix checkpoint retry")]
    [TestCase("- Fix checkpoint retry, closes the settings window", "• Fix checkpoint retry, closes the settings window")]
    [TestCase("- The door closes #51 when the player leaves the room", "• The door closes #51 when the player leaves the room")]
    [TestCase("- The player closes a door", "• The player closes a door")]
    [TestCase("- Fix start, closes [#10](https://github.com/issues/10) in the next version", "• Fix start, closes in the next version")]
    [TestCase("- Internal **rendering** work ([abcdef123](https://github.com/example/commit/abcdef123))", "• Internal rendering work")]
    [TestCase("### ⚠ BREAKING CHANGES", "⚠ BREAKING CHANGES")]
    [TestCase("- **save:** old files require manual migration", "• save: old files require manual migration")]
    [TestCase("> BREAKING CHANGE: Older saves cannot load this version.", "BREAKING CHANGE: Older saves cannot load this version.")]
    public void IngameHistoryHidesOnlyTrailingGitHubMetadata(string markdown, string expected)
    {
        var blocks = Parse(markdown);
        Assert.That(blocks.Length, Is.EqualTo(1), markdown);
        Assert.That(Text(blocks.GetValue(0)), Is.EqualTo(expected));
    }

    [Test]
    public void EmptyAndLongHistoriesAreBounded()
    {
        Assert.That(Parse(null).Length, Is.Zero);
        Assert.That(Parse("```\ncode\n```\n[ref]: https://example.org\n| table |").Length, Is.Zero);
        Assert.That(Parse(string.Join("\n", Enumerable.Repeat("- Entry", 10000))).Length, Is.LessThanOrEqualTo(121));
    }
    static bool Shortcut(Keyboard keyboard) => (bool)Runtime("StarterScreen")
        .GetMethod("FullscreenShortcutPressed", BindingFlags.Static | BindingFlags.NonPublic)
        .Invoke(null, new object[] { keyboard });

    [TestCase(Key.LeftAlt, Key.Enter)]
    [TestCase(Key.RightAlt, Key.Enter)]
    [TestCase(Key.LeftAlt, Key.NumpadEnter)]
    [TestCase(Key.RightAlt, Key.NumpadEnter)]
    public void FullscreenShortcutRequiresAnEnterEdgeAndDoesNotRepeat(Key alt, Key enter)
    {
        var keyboard = InputSystem.AddDevice<Keyboard>();
        try {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(alt));
            InputSystem.Update();
            Assert.That(Shortcut(keyboard), Is.False);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(alt, enter));
            InputSystem.Update();
            Assert.That(Shortcut(keyboard), Is.True);
            InputSystem.Update();
            Assert.That(Shortcut(keyboard), Is.False, "Holding the chord must not toggle every frame.");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(alt));
            InputSystem.Update();
            Assert.That(Shortcut(keyboard), Is.False);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(alt, enter));
            InputSystem.Update();
            Assert.That(Shortcut(keyboard), Is.True, "Release and repress permits the next toggle.");
        } finally { InputSystem.RemoveDevice(keyboard); }
    }

    [TestCase(Key.Enter)]
    [TestCase(Key.NumpadEnter)]
    public void OrdinaryEnterAndAddingAltToHeldEnterDoNotToggle(Key enter)
    {
        var keyboard = InputSystem.AddDevice<Keyboard>();
        try {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(enter));
            InputSystem.Update();
            Assert.That(Shortcut(keyboard), Is.False);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(enter, Key.LeftAlt));
            InputSystem.Update();
            Assert.That(Shortcut(keyboard), Is.False);
        } finally { InputSystem.RemoveDevice(keyboard); }
    }

    [Test]
    public void FullscreenShortcutWithoutAKeyboardIsHarmless()
        => Assert.That(Shortcut(null), Is.False);
    [TestCase(false)]
    [TestCase(true)]
    public void GameplayDevicesReleaseAndRecoverAfterFocusLoss(bool gamepad)
    {
        var keyboard = InputSystem.AddDevice<Keyboard>();
        var pad = InputSystem.AddDevice<Gamepad>();
        var host = new GameObject("Input adapter test");
        var controller = host.AddComponent(Runtime("CatController"));
        var input = host.AddComponent(Runtime("CatInput"));
        var type = input.GetType();
        void Lifecycle(string name, params object[] args) => type.GetMethod(name,
            BindingFlags.Instance | BindingFlags.NonPublic).Invoke(input, args);
        var motion = controller.GetType().GetField("Motion").GetValue(controller);
        Vector2 Step(bool grounded, Vector2 velocity) => (Vector2)Call(motion, "Step", 0.02f, grounded, velocity);
        void State(bool pressed) {
            if (gamepad) InputSystem.QueueStateEvent(pad, pressed
                ? new GamepadState { leftStick = Vector2.right }.WithButton(GamepadButton.South)
                    .WithButton(GamepadButton.Select).WithButton(GamepadButton.Start)
                : new GamepadState());
            else InputSystem.QueueStateEvent(keyboard, pressed
                ? new KeyboardState(Key.D, Key.Space, Key.R, Key.Escape) : new KeyboardState());
            InputSystem.Update();
        }
        InputActionMap actions = null;
        try {
            type.GetField("controller").SetValue(input, controller);
            Lifecycle("Awake");
            actions = (InputActionMap)type.GetProperty("Actions").GetValue(input);
            int retries = 0, menus = 0;
            type.GetField("RetryCheckpoint").SetValue(input, (Action)(() => retries++));
            type.GetField("BackToMenu").SetValue(input, (Action)(() => menus++));
            Lifecycle("OnApplicationFocus", true);
            State(true);
            var velocity = Step(true, Vector2.zero);
            Assert.That(velocity.x, Is.GreaterThan(0));
            Assert.That(velocity.y, Is.GreaterThan(10));
            Assert.That(retries, Is.EqualTo(1));
            Assert.That(menus, Is.EqualTo(1));
            State(false);
            var released = Step(false, velocity);
            Assert.That(released.y, Is.LessThan(5), "Jump release cuts ascent.");
            Assert.That(released.x, Is.Zero, "Movement release must clear the axis.");
            Call(motion, "Reset");
            Assert.That(Step(true, Vector2.zero).x, Is.Zero);
            State(true);
            Lifecycle("OnApplicationFocus", false);
            Assert.That(actions.enabled, Is.False);
            Assert.That(Step(true, Vector2.zero).x, Is.Zero);
            Assert.That((bool)Property(motion, "Jumped"), Is.False);
            State(false);
            Lifecycle("OnApplicationFocus", true);
            InputSystem.Update();
            Assert.That(Step(true, Vector2.zero).x, Is.Zero, "Released background input must not stick.");
            State(true);
            Assert.That(Step(true, Vector2.zero).y, Is.GreaterThan(10));
            Assert.That(retries, Is.EqualTo(3));
            Assert.That(menus, Is.EqualTo(3));
            Lifecycle("OnDisable");
            Assert.That(actions.enabled, Is.False);
            Assert.That(Step(true, Vector2.zero).x, Is.Zero);
        } finally {
            actions?.Dispose();
            UnityEngine.Object.DestroyImmediate(host);
            InputSystem.RemoveDevice(pad);
            InputSystem.RemoveDevice(keyboard);
        }
    }
    [Test]
    public void F11TogglesWithoutAltOnlyOnNewPresses()
    {
        var keyboard = InputSystem.AddDevice<Keyboard>();
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.F11));
        InputSystem.Update();
        Assert.That(Shortcut(keyboard), Is.True);
        InputSystem.Update();
        Assert.That(Shortcut(keyboard), Is.False);
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        InputSystem.Update();
        Assert.That(Shortcut(keyboard), Is.False);
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.F11));
        InputSystem.Update();
        Assert.That(Shortcut(keyboard), Is.True);
    }

    [Test]
    public void ConfirmedModeCanReverseBeforePersistenceDebounceWithoutLosingWindowSize()
    {
        var state = State();
        Call(state, "Request", true, 0f);
        Assert.That(Property(state, "Switching"), Is.True);
        Call(state, "Observe", 1280, 720, false, 0.01f, false);
        Assert.That(Property(state, "Switching"), Is.True);
        Call(state, "Observe", 1920, 1080, true, 0.02f, false);
        Call(state, "Observe", 1920, 1080, true, 0.03f, false);
        Assert.That(Property(state, "Switching"), Is.True);
        Assert.That(Call(state, "Observe", 1920, 1080, true, 0.04f, false), Is.False);
        Assert.That(Property(state, "Switching"), Is.False, "Controls should unlock after mode confirmation.");
        Assert.That(Property(state, "Pending"), Is.True, "Saving still debounces.");
        Call(state, "Request", false, 0.05f);
        Call(state, "Observe", 1920, 1080, true, 0.06f, true);
        Assert.That(Property(state, "Switching"), Is.True);
        // Some compositors report the old desktop dimensions briefly on exit.
        Call(state, "Observe", 1920, 1080, false, 0.07f, false);
        Call(state, "Observe", 1280, 720, false, 0.08f, false);
        Call(state, "Observe", 1280, 720, false, 0.09f, false);
        Call(state, "Observe", 1280, 720, false, 0.10f, false);
        Assert.That(Property(state, "Switching"), Is.False);
        Assert.That(Property(state, "Width"), Is.EqualTo(1280));
        Call(state, "Observe", 1280, 720, false, 2f, false);
        Assert.That(Property(state, "Pending"), Is.False);
        Assert.That(Property(state, "Fullscreen"), Is.False);
        Assert.That(Property(state, "Width"), Is.EqualTo(1280));
        Assert.That(Property(state, "Height"), Is.EqualTo(720));
    }
}
