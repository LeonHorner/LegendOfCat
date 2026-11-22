using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Unity.Collections;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

public class GoldenRiverAtmosphereTests
{
    static int exits;
    static void CountExit() => exits++;
    const BindingFlags Methods = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    static Type Runtime(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).First(t => t != null);
    static object Read(object item, string property) => item.GetType().GetProperty(property).GetValue(item);
    static object Call(object item, string method, params object[] args) => item.GetType().GetMethod(method, Methods).Invoke(item, args);
    static object Field(object item, string name) => item.GetType().GetField(name, Methods).GetValue(item);
    static float Energy()
    {
        using (var samples = new NativeArray<float>(AudioRenderer.GetSampleCountForCaptureFrame() * 2, Allocator.Temp)) {
            Assert.That(AudioRenderer.Render(samples), Is.True);
            return samples.Sum(x => x * x);
        }
    }
    static Color32[] RenderCourse(Camera view)
    {
        var target = new RenderTexture(320, 180, 24);
        var pixels = new Texture2D(320, 180, TextureFormat.RGB24, false);
        var previous = RenderTexture.active;
        var previousTarget = view.targetTexture;
        var previousRect = view.rect;
        try {
            view.targetTexture = target; view.rect = new Rect(0, 0, 1, 1); view.Render();
            RenderTexture.active = target;
            pixels.ReadPixels(new Rect(0, 0, 320, 180), 0, 0); pixels.Apply();
            return pixels.GetPixels32();
        } finally {
            RenderTexture.active = previous; view.targetTexture = previousTarget; view.rect = previousRect;
            target.Release(); UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(pixels);
        }
    }

    [Test]
    public void VolumeSliderUpdatesMixerImmediatelyAndSavesAfterDragging()
    {
        string directory = Path.Combine(Path.GetTempPath(), "LegendOfCat-slider-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "settings.json");
        var host = new GameObject("Volume slider regression");
        var menu = host.AddComponent(Runtime("StarterScreen"));
        float previousVolume = AudioListener.volume;
        try {
            File.WriteAllText(path, "{\"schemaVersion\":0,\"masterVolume\":0.7,\"unknown\":42}");
            var settings = Runtime("MenuSettings").GetMethod("Open").Invoke(null, new object[] { path });
            menu.GetType().GetField("settingsFile", Methods).SetValue(menu, settings);
            Call(menu, "SetMasterVolume", .374f);
            Assert.That(AudioListener.volume, Is.EqualTo(.37f).Within(.001));
            Assert.That(Read(menu, "MasterVolume"), Is.EqualTo(.37f));
            Assert.That((float)JObject.Parse(File.ReadAllText(path))["masterVolume"], Is.EqualTo(.7f),
                "Dragging changes the mixer without writing on every pointer event.");
            Call(menu, "OnApplicationFocus", false);
            var saved = JObject.Parse(File.ReadAllText(path));
            Assert.That((float)saved["masterVolume"], Is.EqualTo(.37f));
            Assert.That((int)saved["unknown"], Is.EqualTo(42));
            Call(menu, "SetMasterVolume", -1f);
            Assert.That(AudioListener.volume, Is.Zero);
            Call(menu, "SetMasterVolume", 2f);
            Assert.That(AudioListener.volume, Is.EqualTo(1));
            Call(menu, "SaveVolumePreference");
            Assert.That((float)JObject.Parse(File.ReadAllText(path))["masterVolume"], Is.EqualTo(1));
        } finally {
            AudioListener.volume = previousVolume;
            UnityEngine.Object.DestroyImmediate(host);
            Directory.Delete(directory, true);
        }
    }

    [Test]
    public void VolumeDefaultsRoundTripsAndProtectsInvalidAndFutureSettings()
    {
        string directory = Path.Combine(Path.GetTempPath(), "LegendOfCat-audio-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "settings.json");
        object Open() => Runtime("MenuSettings").GetMethod("Open").Invoke(null, new object[] { path });
        try {
            File.WriteAllText(path, "{\"schemaVersion\":0,\"lastOfferedVersion\":\"0.2.0\",\"unknown\":42}");
            var settings = Open();
            var data = (JObject)Read(settings, "Data");
            Assert.That((float)data["masterVolume"], Is.EqualTo(.7f));
            data["masterVolume"] = 0;
            Call(settings, "Save");
            data = (JObject)Read(Open(), "Data");
            Assert.That((float)data["masterVolume"], Is.Zero);
            Assert.That((int)data["unknown"], Is.EqualTo(42));
            Assert.That((string)data["lastOfferedVersion"], Is.EqualTo("0.2.0"));
            foreach (string contents in new[] {
                "{\"schemaVersion\":0,\"masterVolume\":1.2}",
                "{\"schemaVersion\":0,\"masterVolume\":\"quiet\"}",
                "{\"schemaVersion\":99,\"masterVolume\":0}" }) {
                File.WriteAllText(path, contents);
                settings = Open();
                Assert.That(Read(settings, "CanWrite"), Is.False);
                Assert.That(File.ReadAllText(path), Is.EqualTo(contents));
            }
        } finally { Directory.Delete(directory, true); }
    }

    [UnityTest]
    public IEnumerator RealExitAudioPauseRetryReplayAndImmediateCleanup()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        yield return new EnterPlayMode();
        var host = new GameObject("Background audio pause regression");
        var menu = (Behaviour)host.AddComponent(Runtime("StarterScreen"));
        menu.enabled = false;
        Call(menu, "OpenLevel", "golden-river");
        var course = (Behaviour)Field(menu, "course");
        yield return null;
        var cat = (Behaviour)Read(course, "Cat");
        var run = Read(course, "Run");
        Assert.That(cat, Is.Not.Null, "The course must initialize its player.");
        var audio = (Behaviour)course.GetComponent(Runtime("GoldenRiverAudio"));
        Assert.That(audio, Is.Not.Null, "Selected audio is installed for the owner-authorized in-game review.");
        Assert.That(course.GetComponentsInChildren<AudioSource>().Length, Is.EqualTo(3));
        Assert.That(Resources.Load<AudioClip>("GoldenRiverAudio/jump"), Is.Null);
        Assert.That(Resources.Load<AudioClip>("GoldenRiverAudio/complete"), Is.Null,
            "Rejected movement/completion clips must stay excluded.");
        exits = 0;
        course.GetType().GetEvent("ExitReached").AddEventHandler(course, (Action)CountExit);
        course.enabled = cat.enabled = audio.enabled = false;
        ((Behaviour)cat.GetComponent(Runtime("CatInput"))).enabled = false;
        var river = course.transform.Find("River ambience").GetComponent<AudioSource>();
        var wind = course.transform.Find("Passing wind").GetComponent<AudioSource>();
        var landmark = course.transform.Find("Checkpoint sound").GetComponent<AudioSource>();
        int captureRate = Time.captureFramerate;
        float volume = AudioListener.volume;
        var mode = Physics2D.simulationMode;
        bool recording = false;
        try {
            AudioListener.volume = 1;
            Time.captureFramerate = 50;
            Physics2D.simulationMode = SimulationMode2D.Script;
            recording = AudioRenderer.Start();
            Assert.That(recording, Is.True);
            // Re-enable after intentional shutdown, then drive simulation explicitly.
            audio.enabled = true;
            Call(audio, "ResetSounds");
            float energy = 0;
            for (int i = 0; i < 3; i++) { yield return null; energy += Energy(); }
            Assert.That(energy, Is.GreaterThan(0), "River ambience must reach the real mixer.");
            Call(menu, "OnApplicationFocus", false);
            Assert.That(river.isPlaying, Is.False, "Focus loss must pause audio before another frame can run.");
            Call(menu, "OnApplicationPause", true);
            Call(menu, "OnApplicationFocus", true);
            Call(menu, "OnApplicationPause", false);
            Assert.That(Read(run, "Paused"), Is.True, "Returning focus must require explicit resume.");
            Assert.That(cat.GetComponent<Rigidbody2D>().simulated, Is.False);
            int sample = river.timeSamples;
            for (int i = 0; i < 3; i++) { yield return null; Energy(); }
            Assert.That(river.isPlaying, Is.False);
            Assert.That(river.timeSamples, Is.EqualTo(sample));
            Call(course, "TogglePause"); Call(audio, "LateUpdate");
            for (int i = 0; i < 3; i++) { yield return null; Energy(); }
            Assert.That(river.isPlaying, Is.True);
            Assert.That(river.timeSamples, Is.GreaterThan(sample));

            // Advancing the environment, not player movement, produces bounded passing gusts.
            Call(audio, "ResetSounds");
            Assert.That(wind.isPlaying, Is.False);
            Call(audio, "AdvanceAmbience", 4f);
            var bank = (AudioClip[])Field(audio, "gusts");
            Assert.That(bank.Length, Is.EqualTo(4));
            Assert.That(bank.All(clip => clip && clip.length >= 11 && clip.length <= 14.1f), Is.True);
            Assert.That(bank.Distinct().Count(), Is.EqualTo(4));
            Assert.That(bank, Does.Contain(wind.clip));
            var firstGust = wind.clip;
            Assert.That(wind.isPlaying, Is.True);
            Assert.That(wind.volume, Is.GreaterThan(river.volume * 4));
            river.Pause();
            float gustEnergy = 0;
            for (int i = 0; i < 30; i++) { yield return null; gustEnergy += Energy(); }
            Assert.That(gustEnergy, Is.GreaterThan(0), "The selected wind must reach the mixer on its own.");
            river.UnPause();
            for (int i = 0; i < 3; i++) { yield return null; Energy(); }
            Call(menu, "OnApplicationFocus", false);
            Assert.That(wind.isPlaying, Is.False, "Backgrounding must immediately pause an active gust.");
            int windSample = wind.timeSamples;
            float gustAge = (float)Field(audio, "gustAge");
            Call(audio, "AdvanceAmbience", 30f);
            for (int i = 0; i < 3; i++) { yield return null; Energy(); }
            Assert.That(wind.timeSamples, Is.EqualTo(windSample));
            Assert.That(Field(audio, "gustAge"), Is.EqualTo(gustAge));
            Call(course, "TogglePause"); Call(audio, "LateUpdate");
            for (int i = 0; i < 3; i++) { yield return null; Energy(); }
            Assert.That(wind.isPlaying, Is.True);
            Assert.That(wind.timeSamples, Is.GreaterThan(windSample));
            Call(audio, "AdvanceAmbience", wind.clip.length);
            Assert.That(wind.isPlaying, Is.False);
            float gap = (float)Field(audio, "windWait");
            Assert.That(gap, Is.InRange(10f, 24f));
            Call(audio, "AdvanceAmbience", gap - .1f);
            Assert.That(wind.isPlaying, Is.False, "A quiet interval separates complete gusts.");
            Call(audio, "AdvanceAmbience", .2f);
            Assert.That(wind.clip, Is.Not.SameAs(firstGust));
            Assert.That(wind.isPlaying, Is.True);
            for (int i = 0; i < 20; i++) {
                var previous = wind.clip;
                Call(audio, "AdvanceAmbience", previous.length);
                Assert.That(wind.isPlaying, Is.False);
                gap = (float)Field(audio, "windWait");
                Assert.That(gap, Is.InRange(10f, 24f));
                Call(audio, "AdvanceAmbience", gap - .1f);
                Assert.That(wind.isPlaying, Is.False);
                Call(audio, "AdvanceAmbience", .2f);
                Assert.That(bank, Does.Contain(wind.clip));
                Assert.That(wind.clip, Is.Not.SameAs(previous), "Consecutive wind gestures must differ.");
            }
            Call(course, "RetryCheckpoint");
            Assert.That(wind.isPlaying, Is.False);
            Assert.That(Field(audio, "windWait"), Is.EqualTo(4f));
            Assert.That(river.loop, Is.True);
            Assert.That(river.clip.name, Is.EqualTo("river-loop"));
            Assert.That(river.volume, Is.LessThanOrEqualTo(.14f));
            Call(course, "RestartRun");

            var spawn = ((Vector2[])Runtime("MovementCourse").GetField("LevelOneCheckpointSpawns").GetValue(null))[0];
            Call(run, "TryActivateCheckpoint", 1, spawn, true);
            Call(audio, "LateUpdate");
            Assert.That(landmark.clip.name, Is.EqualTo("checkpoint"));
            for (int i = 0; i < 3; i++) { yield return null; Energy(); }
            sample = landmark.timeSamples;
            Call(audio, "LateUpdate");
            Assert.That(landmark.timeSamples, Is.GreaterThanOrEqualTo(sample), "Lingering at a checkpoint cannot restart its cue.");

            var exit = (Vector2)Runtime("MovementCourse").GetField("LevelOneExit").GetValue(null);
            var body = cat.GetComponent<Rigidbody2D>();
            Call(cat, "RespawnAt", exit);
            Physics2D.SyncTransforms();
            for (int i = 0; i < 12; i++) { Call(cat, "FixedUpdate"); Physics2D.Simulate(.02f); Call(course, "Update"); }
            Assert.That(Read(course, "HasReachedExit"), Is.True);
            var result = Read(course, "Completion");
            Assert.That(Read(result, "LevelId"), Is.EqualTo("golden-river"));
            Call(menu, "OnApplicationFocus", false);
            Call(menu, "OnApplicationPause", true);
            Call(menu, "OnApplicationFocus", true);
            Assert.That(Read(course, "Completion"), Is.SameAs(result), "Backgrounding cannot reopen a completed run.");
            Assert.That(body.simulated, Is.False, "Success freezes Mika at the real exit.");
            Assert.That(landmark.isPlaying, Is.False, "Finish stops the checkpoint cue; no unselected completion sound.");
            Assert.That(river.isPlaying, Is.False);
            Call(course, "Update");
            Assert.That(Read(course, "Completion"), Is.SameAs(result));
            Assert.That(exits, Is.EqualTo(1));
            Call(course, "TogglePause");
            Assert.That(body.simulated, Is.False, "Pause cannot dismiss success or resume movement.");
            Call(course, "RetryCheckpoint");
            Assert.That(Read(course, "Completion"), Is.Null);
            Assert.That(body.simulated, Is.True);
            Assert.That(landmark.isPlaying, Is.False);
            Assert.That(wind.isPlaying, Is.False);
            Assert.That(river.isPlaying, Is.True);
            Call(cat, "RespawnAt", exit);
            Physics2D.SyncTransforms();
            for (int i = 0; i < 12; i++) { Call(cat, "FixedUpdate"); Physics2D.Simulate(.02f); Call(course, "Update"); }
            Assert.That(Read(course, "Completion"), Is.Not.SameAs(result));
            Assert.That(Read(Read(course, "Completion"), "Retries"), Is.EqualTo(1));
            Assert.That(exits, Is.EqualTo(2));
            Call(course, "RestartRun");
            Assert.That(Read(course, "Completion"), Is.Null);
            Assert.That(Read(run, "ElapsedSeconds"), Is.Zero);
            course.gameObject.SetActive(false);
            Assert.That(new[] { river, wind, landmark }.All(s => !s.isPlaying), Is.True);
            Assert.That(Energy(), Is.LessThan(.0001f), "Disabled level sources must leave no mixed sound.");
        } finally {
            if (recording) AudioRenderer.Stop();
            Time.captureFramerate = captureRate;
            AudioListener.volume = volume;
            Physics2D.simulationMode = mode;
            UnityEngine.Object.Destroy(course.gameObject);
            UnityEngine.Object.Destroy(host);
        }
        yield return new ExitPlayMode();
    }

    [UnityTest]
    public IEnumerator BackgroundingPausesEveryPrototypeWithoutTogglingOrAutoResume()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        yield return new EnterPlayMode();
        var host = new GameObject("Background pause menu regression");
        var menu = (Behaviour)host.AddComponent(Runtime("StarterScreen"));
        menu.enabled = false;
        // Exercise the player bindings even when the headless Editor has no focused Game view.
        // Restore both host input-routing settings after the test.
        var backgroundBehavior = InputSystem.settings.backgroundBehavior;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        var editorBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        var keyboard = InputSystem.AddDevice<Keyboard>();
        try {
            Call(menu, "OnApplicationFocus", false);
            Call(menu, "OnApplicationPause", true);
            foreach (string mode in new[] { "course", "rangedArena", "stompArena" }) {
                if (mode == "course") Call(menu, "OpenLevel", "movement-lab");
                else Call(menu, mode == "rangedArena" ? "OpenRangedArena" : "OpenStompArena");
                var owner = (Component)Field(menu, mode);
                var run = Read(owner, "Run");
                var cat = (Component)Read(owner, "Cat");
                var input = cat.GetComponent(Runtime("CatInput"));
                Call(run, "Advance", 2f);
                float elapsed = (float)Read(run, "ElapsedSeconds");
                Call(menu, "OnApplicationFocus", false);
                Call(input, "OnApplicationFocus", false);
                Call(menu, "OnApplicationPause", true);
                Call(menu, "OnApplicationFocus", false);
                Assert.That(Read(run, "Paused"), Is.True, mode);
                Assert.That(cat.GetComponent<Rigidbody2D>().simulated, Is.False, mode);
                Call(run, "Advance", 10f);
                Assert.That(Read(run, "ElapsedSeconds"), Is.EqualTo(elapsed), mode);
                Call(menu, "OnApplicationFocus", true);
                Call(input, "OnApplicationFocus", true);
                Call(menu, "OnApplicationPause", false);
                Assert.That(Read(run, "Paused"), Is.True, "Focus return cannot resume " + mode);
                var actions = (InputActionMap)Read(input, "Actions");
                var pauseAction = actions.FindAction("Pause", true);
                Assert.That(actions.enabled, Is.True, "Focus return must enable input in " + mode);
                Assert.That(pauseAction.controls.Count, Is.GreaterThan(0));
                Assert.That(pauseAction.controls.Contains(keyboard.pKey), Is.True,
                    "P must resolve to the test keyboard: " + string.Join(", ", pauseAction.controls.Select(c => c.path)));
                Assert.That(pauseAction.enabled, Is.True);
                int pausePresses = 0;
                pauseAction.performed += _ => pausePresses++;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.P));
                InputSystem.Update();
                Assert.That(keyboard.pKey.isPressed, Is.True, "The test keyboard must receive P.");
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                InputSystem.Update();
                Assert.That(pausePresses, Is.EqualTo(1), "One P press must perform once in " + mode);
                Assert.That(Read(run, "Paused"), Is.False, mode);
                Assert.That(cat.GetComponent<Rigidbody2D>().simulated, Is.True, mode);
                foreach (bool paused in new[] { true, false, true }) {
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Escape)); InputSystem.Update();
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState()); InputSystem.Update();
                    Assert.That(Field(menu, mode), Is.SameAs(owner), "Escape must not leave " + mode);
                    Assert.That(Read(run, "Paused"), Is.EqualTo(paused), "Escape must toggle the existing pause in " + mode);
                    Assert.That(cat.GetComponent<Rigidbody2D>().simulated, Is.EqualTo(!paused));
                }
                Call(menu, "OnApplicationFocus", false);
                Assert.That(Read(run, "Paused"), Is.True, "An existing manual pause must stay paused.");
                Call(menu, mode == "course" ? "CloseCourse" : mode == "rangedArena" ? "CloseRangedArena" : "CloseStompArena");
                Call(menu, "OnApplicationFocus", false);
                yield return null;
            }
        } finally {
            InputSystem.RemoveDevice(keyboard);
            InputSystem.settings.backgroundBehavior = backgroundBehavior;
            InputSystem.settings.editorInputBehaviorInPlayMode = editorBehavior;
            UnityEngine.Object.Destroy(host);
        }
        yield return new ExitPlayMode();
    }

    [UnityTest]
    public IEnumerator WaterEntryAudioPlaysOnceAndPausesAndStopsWithTheRecovery()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        yield return new EnterPlayMode();
        var host = new GameObject("Water entry audio regression");
        var menu = (Behaviour)host.AddComponent(Runtime("StarterScreen")); menu.enabled = false;
        Call(menu, "OpenLevel", "golden-river");
        var course = (Behaviour)Field(menu, "course");
        var cat = (Behaviour)Read(course, "Cat");
        var recovery = cat.GetComponent(Runtime("WaterRecovery"));
        course.enabled = cat.enabled = false;
        ((Behaviour)course.GetComponent(Runtime("GoldenRiverAudio"))).enabled = false;
        ((Behaviour)cat.GetComponent(Runtime("CatInput"))).enabled = false;
        int captureRate = Time.captureFramerate;
        float volume = AudioListener.volume;
        bool recording = false;
        try {
            Time.captureFramerate = 50; AudioListener.volume = 1;
            recording = AudioRenderer.Start(); Assert.That(recording, Is.True);
            void Fall() {
                Call(cat, "RespawnAt", (Vector2)course.transform.TransformPoint(new Vector2(5, -3.1f)));
                Physics2D.SyncTransforms();
                Assert.That(Call(recovery, "TryEnter"), Is.True);
            }
            Fall();
            var splash = (AudioSource)Field(recovery, "entryAudio");
            Assert.That(splash.clip.name, Is.EqualTo("water-entry"));
            Assert.That(splash.loop, Is.False);
            float energy = 0;
            for (int i = 0; i < 10; i++) { yield return null; energy += Energy(); }
            Assert.That(energy, Is.GreaterThan(0), "The approved splash must reach the real mixer.");
            int sample = splash.timeSamples;
            Assert.That(Call(recovery, "TryEnter"), Is.False);
            Assert.That(splash.timeSamples, Is.EqualTo(sample), "A repeated water check cannot restart the splash.");
            Call(menu, "OnApplicationFocus", false);
            Assert.That(splash.isPlaying, Is.False, "Focus loss pauses the splash immediately.");
            sample = splash.timeSamples;
            for (int i = 0; i < 4; i++) { yield return null; Energy(); }
            Assert.That(splash.timeSamples, Is.EqualTo(sample));
            Call(menu, "OnApplicationFocus", true);
            Assert.That(splash.isPlaying, Is.False);
            Call(course, "TogglePause");
            for (int i = 0; i < 4; i++) { yield return null; Energy(); }
            Assert.That(splash.isPlaying, Is.True);
            Assert.That(splash.timeSamples, Is.GreaterThan(sample));
            Call(course, "RetryCheckpoint");
            Assert.That(splash.isPlaying, Is.False);
            Assert.That(Energy(), Is.LessThan(.0001f));
            Fall(); Assert.That(splash.isPlaying, Is.True);
            Call(menu, "CloseCourse");
            Assert.That(splash.isPlaying, Is.False, "Menu return stops the splash synchronously.");
        } finally {
            if (recording) AudioRenderer.Stop();
            Time.captureFramerate = captureRate; AudioListener.volume = volume;
            if (Field(menu, "course") != null) Call(menu, "CloseCourse");
            UnityEngine.Object.Destroy(host);
        }
        yield return new ExitPlayMode();
    }

    [UnityTest]
    public IEnumerator FocusAndManualPauseFreezeTheRenderedWorldAndAllMovingSupports()
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            Assert.Ignore("The complete pause regression requires real shader rendering.");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        yield return new EnterPlayMode();
        var host = new GameObject("Full world focus-pause regression");
        var menu = (Behaviour)host.AddComponent(Runtime("StarterScreen")); menu.enabled = false;
        Call(menu, "OpenLevel", "golden-river");
        var course = (Component)Field(menu, "course");
        var cat = (Component)Read(course, "Cat");
        var scenery = course.GetComponentInChildren(Runtime("SceneryRenderer"));
        var view = (Camera)Read(scenery, "View");
        var supports = course.GetComponentsInChildren(Runtime("MovingSupport"));
        int captureRate = Time.captureFramerate;
        try {
            Time.captureFramerate = 50;
            Assert.That(supports.Length, Is.GreaterThan(0));
            Call(cat, "RespawnAt", new Vector2(415, .48f));
            for (int i = 0; i < 10; i++) yield return null;
            var live = RenderCourse(view);
            for (int i = 0; i < 20; i++) yield return null;
            Assert.That(RenderCourse(view).SequenceEqual(live), Is.False, "The rendered environment must animate while playing.");
            foreach (bool focusPause in new[] { true, false }) {
                if (focusPause) Call(menu, "OnApplicationFocus", false);
                else Call(course, "TogglePause");
                Call(menu, "OnApplicationFocus", true);
                yield return null;
                var paused = RenderCourse(view);
                var positions = supports.Select(s => s.GetComponent<Rigidbody2D>().position).ToArray();
                var phases = supports.Select(s => (float)Read(s, "Phase")).ToArray();
                Assert.That(supports.All(s => !s.GetComponent<Rigidbody2D>().simulated), Is.True);
                for (int i = 0; i < 25; i++) yield return null;
                Assert.That(RenderCourse(view).SequenceEqual(paused), Is.True,
                    "Water, foliage, fish, Mika and camera must freeze for both focus and manual pause.");
                Assert.That(supports.Select(s => s.GetComponent<Rigidbody2D>().position), Is.EqualTo(positions));
                Assert.That(supports.Select(s => (float)Read(s, "Phase")), Is.EqualTo(phases));
                Call(course, "TogglePause");
                Assert.That(supports.All(s => s.GetComponent<Rigidbody2D>().simulated), Is.True);
                for (int i = 0; i < 10; i++) yield return null;
                Assert.That(supports.Select(s => (float)Read(s, "Phase")).SequenceEqual(phases), Is.False,
                    "Platforms must continue from their paused phase after explicit resume.");
            }
            Call(menu, "OnApplicationFocus", false);
            Call(course, "RetryCheckpoint");
            Assert.That(supports.All(s => s.GetComponent<Rigidbody2D>().simulated), Is.True,
                "Retry from a background pause must not leave platforms disabled.");
        } finally {
            Time.captureFramerate = captureRate;
            Call(menu, "CloseCourse"); UnityEngine.Object.Destroy(host);
        }
        yield return new ExitPlayMode();
    }
}
