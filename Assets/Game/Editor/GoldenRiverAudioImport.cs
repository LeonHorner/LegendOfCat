using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class GoldenRiverAudioImport
{
    const string Folder = "Assets/Game/Resources/GoldenRiverAudio/";
    static readonly string[] Names = { "river-loop", "wind-01", "wind-02", "checkpoint" };

    public static void Prepare()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach (var name in Names) {
            string path = Folder + name + ".wav";
            var importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (!importer) throw new BuildFailedException("Missing selected level sound: " + path);
            var settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = name == "checkpoint" ? AudioCompressionFormat.PCM : AudioCompressionFormat.Vorbis;
            settings.quality = .8f;
            settings.preloadAudioData = true;
            importer.defaultSampleSettings = settings;
            importer.forceToMono = false;
            importer.SaveAndReimport();
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (!clip || clip.length <= 0) throw new BuildFailedException("Invalid selected level sound: " + path);
        }
    }

    public static void ValidatePackedAudio(BuildReport report)
    {
        var packed = report.packedAssets.SelectMany(asset => asset.contents)
            .Select(asset => asset.sourceAssetPath.Replace('\\', '/')).ToArray();
        foreach (var name in Names)
            if (!packed.Contains(Folder + name + ".wav", StringComparer.Ordinal))
                throw new BuildFailedException("Player package is missing selected sound: " + name);
        if (packed.Any(path => path.Contains("RejectedGoldenRiverAudio") || path.StartsWith("art/audio/", StringComparison.Ordinal)))
            throw new BuildFailedException("Player package contains rejected or review-only sound sources.");
        Debug.Log("Packed audio verified: four selected clips; rejected and review-only sources excluded.");
    }

    public static void BuildPreview()
    {
        Prepare();
        BuildGame.BuildPreview();
    }
}
