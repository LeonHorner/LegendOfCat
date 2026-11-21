using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Unity.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

public class GoldenRiverAtmosphereTests
{
    static int exits;
    static void CountExit() => exits++;
    const BindingFlags Methods = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    static Type Runtime(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).First(t => t != null);
    static object Read(object item, string property) => item.GetType().GetProperty(property).GetValue(item);
    static object Call(object item, string method, params object[] args) => item.GetType().GetMethod(method, Methods).Invoke(item, args);
    static float Energy()
    {
        using (var samples = new NativeArray<float>(AudioRenderer.GetSampleCountForCaptureFrame() * 2, Allocator.Temp)) {
            Assert.That(AudioRenderer.Render(samples), Is.True);
            return samples.Sum(x => x * x);
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
        var course = (Behaviour)Runtime("LevelRegistry").GetMethod("Create").Invoke(null, new object[] { "golden-river" });
        yield return null;
        var cat = (Behaviour)Read(course, "Cat");
        var run = Read(course, "Run");
        Assert.That(cat, Is.Not.Null, "The course must initialize its player.");
        Assert.That(course.GetComponent(Runtime("GoldenRiverAudio")), Is.Null,
            "Unapproved sound candidates must not attach to the playable level.");
        Assert.That(course.GetComponentsInChildren<AudioSource>(), Is.Empty);
        Assert.That(Resources.Load<AudioClip>("GoldenRiverAudio/river"), Is.Null,
            "Rejected clips must not be included through Resources.");
        // Retain technical lifecycle coverage with explicitly installed Editor-only fixtures.
        course.gameObject.AddComponent<AudioListener>();
        var audio = (Behaviour)course.gameObject.AddComponent(Runtime("GoldenRiverAudio"));
        Call(audio, "Initialize", course);
        AudioClip Fixture(string name) => AssetDatabase.LoadAssetAtPath<AudioClip>(
            "Assets/Game/Tests/Editor/Fixtures/RejectedGoldenRiverAudio/" + name + ".wav");
        foreach (string cue in new[] { "jump", "land", "checkpoint", "complete" })
            audio.GetType().GetField(cue, Methods).SetValue(audio, Fixture(cue));
        exits = 0;
        course.GetType().GetEvent("ExitReached").AddEventHandler(course, (Action)CountExit);
        course.enabled = cat.enabled = audio.enabled = false;
        ((Behaviour)cat.GetComponent(Runtime("CatInput"))).enabled = false;
        var river = course.transform.Find("River ambience").GetComponent<AudioSource>();
        river.clip = Fixture("river");
        var movement = course.transform.Find("Movement sounds").GetComponent<AudioSource>();
        var landmark = course.transform.Find("Landmark sounds").GetComponent<AudioSource>();
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
            Call(course, "TogglePause"); Call(audio, "LateUpdate");
            int sample = river.timeSamples;
            for (int i = 0; i < 3; i++) { yield return null; Energy(); }
            Assert.That(river.isPlaying, Is.False);
            Assert.That(river.timeSamples, Is.EqualTo(sample));
            Call(course, "TogglePause"); Call(audio, "LateUpdate");
            for (int i = 0; i < 3; i++) { yield return null; Energy(); }
            Assert.That(river.isPlaying, Is.True);
            Assert.That(river.timeSamples, Is.GreaterThan(sample));

            Physics2D.SyncTransforms();
            for (int i = 0; i < 20; i++) {
                Call(cat, "FixedUpdate"); Physics2D.Simulate(.02f); Call(audio, "LateUpdate");
            }
            Assert.That(Read(cat, "Grounded"), Is.True);
            Assert.That(movement.isPlaying, Is.False, "The initial spawn must not play a landing cue.");
            Call(cat, "PressJump"); Call(audio, "LateUpdate");
            Assert.That(movement.clip.name, Is.EqualTo("jump"));
            for (int i = 0; i < 65; i++) {
                Call(cat, "FixedUpdate"); Physics2D.Simulate(.02f); Call(audio, "LateUpdate");
            }
            Assert.That(Read(cat, "Grounded"), Is.True);
            Assert.That(movement.clip.name, Is.EqualTo("land"));

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
            Assert.That(body.simulated, Is.False, "Success freezes Mika at the real exit.");
            Assert.That(landmark.clip.name, Is.EqualTo("complete"));
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
            Assert.That(movement.isPlaying, Is.False);
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
            Assert.That(new[] { river, movement, landmark }.All(s => !s.isPlaying), Is.True);
            Assert.That(Energy(), Is.LessThan(.0001f), "Disabled level sources must leave no mixed sound.");
        } finally {
            if (recording) AudioRenderer.Stop();
            Time.captureFramerate = captureRate;
            AudioListener.volume = volume;
            Physics2D.simulationMode = mode;
            UnityEngine.Object.Destroy(course.gameObject);
        }
        yield return new ExitPlayMode();
    }
}
