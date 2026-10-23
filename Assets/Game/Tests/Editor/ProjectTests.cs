using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class ProjectTests
{
    [Test]
    public void StarterSceneCanBeCreatedAndContainsAnOrthographicCamera()
    {
        Assert.That(EditorApplication.ExecuteMenuItem("LegendOfCat/Create starter scene"), Is.True);
        Assert.That(EditorSceneManager.GetActiveScene().path, Is.EqualTo("Assets/Game/Scenes/Main.unity"));
        var camera = Object.FindFirstObjectByType<Camera>();
        Assert.That(camera, Is.Not.Null);
        Assert.That(camera.orthographic, Is.True);
        Assert.That(GameObject.Find("Game"), Is.Not.Null);
        Assert.That(EditorBuildSettings.scenes.Length, Is.EqualTo(1));
        Assert.That(EditorBuildSettings.scenes[0].enabled, Is.True);
    }
}
