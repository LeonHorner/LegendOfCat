using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class UpdateTests
{
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
