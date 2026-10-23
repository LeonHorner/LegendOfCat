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
        PlayerSettings.resizableWindow = true;
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

    // Local UI inspection only. No installer, update feed or release publication is produced.
    public static void BuildPreview()
    {
        CreateStarterScene();
        PlayerSettings.resizableWindow = true;
        var company = PlayerSettings.companyName;
        var product = PlayerSettings.productName;
        var version = PlayerSettings.bundleVersion;
        var fullscreen = PlayerSettings.fullScreenMode;
        var width = PlayerSettings.defaultScreenWidth;
        var height = PlayerSettings.defaultScreenHeight;
        try {
            PlayerSettings.companyName = "LeonHorner";
            PlayerSettings.productName = "LegendOfCat Preview";
            PlayerSettings.bundleVersion = "0.0.0-preview";
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.defaultScreenWidth = 1100;
            PlayerSettings.defaultScreenHeight = 800;
            var output = Argument("-previewOutput");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { ScenePath },
                target = BuildTarget.StandaloneWindows64,
                locationPathName = output,
                options = BuildOptions.Development,
                extraScriptingDefines = new[] { "LEGEND_OF_CAT_PREVIEW" }
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException("Windows preview build failed.");
        }
        finally {
            PlayerSettings.companyName = company;
            PlayerSettings.productName = product;
            PlayerSettings.bundleVersion = version;
            PlayerSettings.fullScreenMode = fullscreen;
            PlayerSettings.defaultScreenWidth = width;
            PlayerSettings.defaultScreenHeight = height;
        }
    }

    static string Argument(string key)
    {
        var args = Environment.GetCommandLineArgs();
        var index = Array.IndexOf(args, key);
        if (index < 0 || index + 1 >= args.Length) throw new BuildFailedException("Missing " + key);
        return args[index + 1];
    }
}
