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

    public static void ConfigureDesktop()
    {
        PlayerSettings.resizableWindow = true;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.allowFullscreenSwitch = false;
        var icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Game/Branding/icon.png");
        if (icon == null) throw new BuildFailedException("Missing generated application icon.");
        var importer = (TextureImporter)AssetImporter.GetAtPath("Assets/Game/Branding/icon.png");
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
        PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);
        var sizes = PlayerSettings.GetIconSizes(NamedBuildTarget.Standalone, IconKind.Any);
        PlayerSettings.SetIcons(NamedBuildTarget.Standalone, sizes.Select(_ => icon).ToArray(), IconKind.Any);
    }

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
        ConfigureDesktop();
        PlayerSettings.companyName = "LeonHorner";
        PlayerSettings.productName = "LegendOfCat";
        PlayerSettings.bundleVersion = Argument("-releaseVersion");
        var output = Argument("-releaseOutput");
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new[] { ScenePath },
            target = EditorUserBuildSettings.activeBuildTarget,
            locationPathName = output,
            // A fresh asset pack makes the packed-art audit complete, including cached resources.
            options = BuildOptions.CleanBuildCache | BuildOptions.DetailedBuildReport
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new BuildFailedException("Unity player build failed.");
        BuildBackgroundStudy.ValidatePackedArt(report);
        // GameCI CLI requires this exact success marker for custom build methods.
        // Report success only after Unity and the packed-asset audit have passed.
        Debug.Log("Build succeeded!");
    }

    // Local UI inspection only. No installer, update feed or release publication is produced.
    public static void BuildPreview()
    {
        CreateStarterScene();
        ConfigureDesktop();
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
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            var output = Argument("-previewOutput");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { ScenePath },
                target = BuildTarget.StandaloneWindows64,
                locationPathName = output,
                options = BuildOptions.Development | BuildOptions.CleanBuildCache | BuildOptions.DetailedBuildReport,
                extraScriptingDefines = new[] { "LEGEND_OF_CAT_PREVIEW" }
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException("Windows preview build failed.");
            BuildBackgroundStudy.ValidatePackedArt(report);
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
