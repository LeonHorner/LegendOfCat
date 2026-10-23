using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildGame
{
    const string ScenePath = "Assets/Game/Scenes/Main.unity";

    [MenuItem("LegendOfCat/Create starter scene")]
    public static void CreateStarterScene()
    {
        if (!File.Exists(ScenePath))
        {
            Directory.CreateDirectory("Assets/Game/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Camera").AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5;
            camera.transform.position = new Vector3(0, 0, -10);
            camera.backgroundColor = new Color(0.08f, 0.1f, 0.16f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            new GameObject("Game").AddComponent<StarterScreen>();
            EditorSceneManager.SaveScene(scene, ScenePath);
        }
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        EditorSceneManager.OpenScene(ScenePath);
    }

    public static void Build()
    {
        CreateStarterScene();
        PlayerSettings.companyName = "LeonHorner";
        PlayerSettings.productName = "LegendOfCat";
        PlayerSettings.bundleVersion = Argument("-releaseVersion");
        var output = Argument("-releaseOutput");
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new[] { ScenePath },
            target = EditorUserBuildSettings.activeBuildTarget,
            locationPathName = output,
            options = BuildOptions.None
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new BuildFailedException("Unity player build failed.");
    }

    static string Argument(string key)
    {
        var args = Environment.GetCommandLineArgs();
        var index = Array.IndexOf(args, key);
        if (index < 0 || index + 1 >= args.Length) throw new BuildFailedException("Missing " + key);
        return args[index + 1];
    }
}
