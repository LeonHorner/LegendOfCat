using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System;
using System.Linq;

public class ProjectTests
{
    [Test]
    public void DesktopBuildConfigurationIncludesTheOriginalDerivedIcon()
    {
        var build = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("BuildGame")).First(t => t != null);
        build.GetMethod("ConfigureDesktop").Invoke(null, null);
        Assert.That(PlayerSettings.resizableWindow, Is.True);
        Assert.That(PlayerSettings.defaultScreenWidth, Is.EqualTo(1280));
        Assert.That(PlayerSettings.defaultScreenHeight, Is.EqualTo(720));
        Assert.That(PlayerSettings.fullScreenMode, Is.EqualTo(FullScreenMode.Windowed));
        Assert.That(PlayerSettings.allowFullscreenSwitch, Is.False);
        var icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Game/Branding/icon.png");
        Assert.That(icon, Is.Not.Null);
        Assert.That(icon.width, Is.EqualTo(256));
        var icons = PlayerSettings.GetIcons(UnityEditor.Build.NamedBuildTarget.Standalone, IconKind.Any);
        Assert.That(icons, Is.Not.Empty);
        Assert.That(icons.All(item => item == icon), Is.True);
    }
    [Test]
    public void StarterSceneCanBeCreatedAndContainsAnOrthographicCamera()
    {
        Assert.That(EditorApplication.ExecuteMenuItem("LegendOfCat/Create starter scene"), Is.True);
        Assert.That(EditorSceneManager.GetActiveScene().path, Is.EqualTo("Assets/Game/Scenes/Main.unity"));
        var camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
        Assert.That(camera, Is.Not.Null);
        Assert.That(camera.orthographic, Is.True);
        Assert.That(GameObject.Find("Game"), Is.Not.Null);
        Assert.That(EditorBuildSettings.scenes.Length, Is.EqualTo(1));
        Assert.That(EditorBuildSettings.scenes[0].enabled, Is.True);
    }
}
