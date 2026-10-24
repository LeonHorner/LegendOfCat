using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;

public class UpdateTests
{
    [TestCase(2500000L, 10000000L, "7.5 MB remaining / 10.0 MB total")]
    [TestCase(1L, 100000L, "0.1 MB remaining / 0.1 MB total")]
    [TestCase(12000000L, 10000000L, "0.0 MB remaining / 10.0 MB total")]
    [TestCase(-50L, 1000000L, "1.0 MB remaining / 1.0 MB total")]
    [TestCase(0L, 0L, "")]
    public void DownloadSizesAreReadableBoundedAndDoNotRoundRemainingBytesToZero(long downloaded, long total, string expected)
        => Assert.That(RuntimeType("DownloadProgress").GetMethod("Format").Invoke(null, new object[] { downloaded, total }), Is.EqualTo(expected));

    [Test]
    public void ConcurrentDeltaCountsAndRetriesResetCleanlyForFullFallback()
    {
        var type = RuntimeType("DownloadProgress");
        var tracker = Activator.CreateInstance(type);
        void Plan(Dictionary<string, long> files) => type.GetMethod("Reset").Invoke(tracker, new object[] { files });
        void Report(string file, long count) => type.GetMethod("Report").Invoke(tracker, new object[] { file, count });
        long Value(string field) => (long)type.GetProperty(field).GetValue(tracker);
        Plan(new Dictionary<string, long> { ["a.delta"] = 200, ["b.delta"] = 300 });
        Report("a.delta", 150); Report("b.delta", 100);
        Assert.That(Value("DownloadedBytes"), Is.EqualTo(250));
        Assert.That(Value("TotalBytes"), Is.EqualTo(500));
        Report("a.delta", 0); // This package restarts after a failed HTTP attempt.
        Assert.That(Value("DownloadedBytes"), Is.EqualTo(100));
        Plan(new Dictionary<string, long> { ["full"] = 1000 });
        Report("a.delta", 200); // A stale package must not inflate the new full download.
        Assert.That(Value("DownloadedBytes"), Is.Zero);
        Report("full", 1500);
        Assert.That(Value("DownloadedBytes"), Is.EqualTo(1000));
        Assert.That(Value("TotalBytes"), Is.EqualTo(1000));
    }

    [Test]
    public void ByteObservationCountsSyncAndAsyncWritesAndLeavesVelopacksStreamOpen()
    {
        using (var destination = new MemoryStream()) {
            long received = 0;
            var observed = (Stream)Activator.CreateInstance(RuntimeType("ProgressWriteStream"), destination, new Action<long>(value => received = value));
            observed.Write(new byte[80], 0, 80);
            observed.WriteAsync(new byte[20], 0, 20, CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(received, Is.EqualTo(100));
            Assert.That(destination.Length, Is.EqualTo(100));
            observed.Dispose();
            Assert.That(destination.CanWrite, Is.True);
        }
    }

    [Test]
    public void DownloadByteMessagesAreOptionalAndResetForOlderHelpersAndErrors()
    {
        CreateScreen();
        Receive("{\"state\":\"downloading\",\"progress\":25,\"downloadedBytes\":2500000,\"totalBytes\":10000000}");
        string Status() => (string)screenType.GetMethod("StatusText", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(screen, null);
        Assert.That(Status(), Does.Contain("7.5 MB remaining / 10.0 MB total"));
        Receive("{\"state\":\"downloading\",\"progress\":70,\"downloadedBytes\":10000000,\"totalBytes\":10000000}");
        Assert.That(Status(), Does.Contain("Preparing update"));
        Receive("{\"state\":\"downloading\",\"progress\":40}");
        Assert.That(Status(), Does.Not.Contain("MB"));
        Receive("{\"state\":\"error\",\"message\":\"Interrupted\"}");
        Assert.That(Get("totalBytes"), Is.EqualTo(0L));
    }
    static Type RuntimeType(string name) => AppDomain.CurrentDomain.GetAssemblies()
        .Select(assembly => assembly.GetType(name)).First(type => type != null);
    static object Policy(string method, params object[] args) => RuntimeType("UpdatePolicy")
        .GetMethod(method).Invoke(null, args);

    [TestCase(false, false, true)]
    [TestCase(false, true, false)]
    [TestCase(true, false, false)]
    [TestCase(true, true, false)]
    public void HistoryExcludesDraftsAndPrereleases(bool draft, bool prerelease, bool expected)
        => Assert.That(Policy("IsStableRelease", draft, prerelease), Is.EqualTo(expected));

    [TestCase("1.10.0", "1.9.0", true)]
    [TestCase("v1.1.0", "1.0.0", true)]
    [TestCase("1.0.0", "1.0.0", false)]
    [TestCase("0.9.0", "1.0.0", false)]
    [TestCase("2.0.0-beta.1", "1.0.0", false)]
    [TestCase("invalid", "1.0.0", false)]
    public void OnlyNewerStableVersionsCanBeInstalled(string candidate, string installed, bool expected)
        => Assert.That(Policy("IsNewerStableVersion", candidate, installed), Is.EqualTo(expected));

    [TestCase(null, "", false)]
    [TestCase("", "", false)]
    [TestCase("1.1.0", "1.1.0", false)]
    [TestCase("1.2.0", "1.1.0", true)]
    public void ReminderIsOfferedOncePerVersion(string version, string offered, bool expected)
        => Assert.That(Policy("ShouldOffer", version, offered), Is.EqualTo(expected));

    [TestCase("https://github.com/LeonHorner/LegendOfCat/releases/tag/v1.0.0", true)]
    [TestCase("https://github.com/other/project/releases/tag/v1.0.0", false)]
    [TestCase("https://github.com.evil.example/LeonHorner/LegendOfCat/releases/", false)]
    [TestCase("file:///tmp/release", false)]
    public void ReleaseLinksStayInTheOfficialRepository(string url, bool expected)
        => Assert.That(Policy("IsReleaseUrl", url), Is.EqualTo(expected));

    GameObject gameObject;
    Component screen;
    Type screenType;
    string directory;

    void CreateScreen()
    {
        directory = Path.Combine(Path.GetTempPath(), "LegendOfCat-tests-" + Guid.NewGuid());
        Directory.CreateDirectory(directory);
        screenType = RuntimeType("StarterScreen");
        gameObject = new GameObject("Update test");
        screen = gameObject.AddComponent(screenType);
        Set("preferencesPath", Path.Combine(directory, "settings.json"));
        screenType.GetMethod("LoadPreferences", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(screen, null);
    }
    FieldInfo Field(string name) => screenType.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
    void Set(string name, object value) => Field(name).SetValue(screen, value);
    object Get(string name) => Field(name).GetValue(screen);
    void Receive(string json)
    {
        ((ConcurrentQueue<string>)Get("messages")).Enqueue(json);
        screenType.GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(screen, null);
    }

    [TearDown]
    public void Cleanup()
    {
        if (gameObject != null) UnityEngine.Object.DestroyImmediate(gameObject);
        if (directory != null && Directory.Exists(directory)) Directory.Delete(directory, true);
    }

    [Test]
    public void LaterSurvivesRestartAndANewerReleasePromptsAgain()
    {
        CreateScreen();
        Receive("{\"state\":\"available\",\"version\":\"1.1.0\"}");
        Assert.That(Get("popup"), Is.True);
        screenType.GetMethod("LoadPreferences", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(screen, null);
        Set("popup", false);
        Receive("{\"state\":\"available\",\"version\":\"1.1.0\"}");
        Assert.That(Get("popup"), Is.False);
        Assert.That(Get("available"), Is.EqualTo("1.1.0"));
        Receive("{\"state\":\"available\",\"version\":\"1.2.0\"}");
        Assert.That(Get("popup"), Is.True);
    }

    [Test]
    public void FailedDownloadKeepsMenuAndUpdateAvailableForRetry()
    {
        CreateScreen();
        Set("available", "1.1.0");
        Receive("{\"state\":\"downloading\",\"progress\":42}");
        Assert.That(Get("progress"), Is.EqualTo(42));
        Receive("{\"state\":\"error\",\"message\":\"Download interrupted\"}");
        Assert.That(Get("applying"), Is.False);
        Assert.That(Get("page"), Is.EqualTo("menu"));
        Assert.That(Get("available"), Is.EqualTo("1.1.0"));
        Assert.That(Get("error"), Is.EqualTo("Download interrupted"));
    }

    [Test]
    public void DownloadCompletionWithoutAnAvailableHelperCannotQuitTheGame()
    {
        CreateScreen();
        Receive("{\"state\":\"ready\"}");
        Assert.That(Get("state"), Is.EqualTo("error"));
        Assert.That(Get("applying"), Is.False);
    }

    [Test]
    public void UnsupportedPlatformKeepsSharedMenuWithoutSendingUpdates()
    {
        CreateScreen();
        Set("platform", Activator.CreateInstance(RuntimeType("UnavailableMenuPlatformService")));
        var supported = screenType.GetProperty("UpdatesSupported", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(supported.GetValue(screen), Is.False);
        screenType.GetMethod("Send", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(screen, new object[] { "check" });
        Assert.That(Get("page"), Is.EqualTo("menu"));
        Assert.That(Get("state"), Is.EqualTo("development"));
        Assert.That(Get("applying"), Is.False);
    }

    [Test]
    public void HistoryAndInstallationWarningDoNotReplaceUpdateState()
    {
        CreateScreen();
        Receive("{\"state\":\"available\",\"version\":\"1.1.0\"}");
        Receive("{\"state\":\"history\",\"releases\":[{\"version\":\"v1.0.0\",\"notes\":\"First release\"}]}");
        Receive("{\"state\":\"warning\",\"message\":\"Previous installation incomplete\"}");
        Assert.That(Get("state"), Is.EqualTo("available"));
        Assert.That(((Array)Get("releases")).Length, Is.EqualTo(1));
        Assert.That(Get("warning"), Is.EqualTo("Previous installation incomplete"));
    }
}
