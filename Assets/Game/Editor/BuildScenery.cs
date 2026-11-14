using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Production packed-art validation and authoring imports; historical captures live in docs.
public static class BuildScenery
{
    const string ResourcesDirectory = "Assets/Game/Resources/BackgroundStudy/";
    static readonly string[] ActiveLayers = {
        "painted-atlas", "foreground-river", "recessed-valley-v4", "foreground-corners",
        "painted-pillar-v2", "painted-bank-v2", "painted-shore-v2", "painted-shelf-v3", "distant-canopy-v3"
    };

    public static void ValidatePackedArt(BuildReport report)
    {
        var paths = new System.Collections.Generic.HashSet<string>(report.packedAssets.SelectMany(p => p.contents)
            .Select(p => p.sourceAssetPath));
        if (paths.Count == 0)
            throw new BuildFailedException("Packed-art auditing requires a complete clean build report.");
        if (paths.Any(p => p.StartsWith("Assets/Game/Art/BackgroundStudyArchive/", System.StringComparison.Ordinal)
            || p.StartsWith("Assets/Game/Art/GoldenRiverArchive/", System.StringComparison.Ordinal)))
            throw new BuildFailedException("Historical background artwork must not be packed into the player.");
        foreach (string name in ActiveLayers)
            if (!paths.Contains(ResourcesDirectory + name + ".png"))
                throw new BuildFailedException("Missing selected scenery in player build: " + name);
        foreach (string name in GoldenRiverArt)
            if (!paths.Contains("Assets/Game/Resources/GoldenRiver/" + name + ".png"))
                throw new BuildFailedException("Missing Golden River route art: " + name);
        foreach (string name in new[] { "background-material", "terrain-material", "water-material" })
            if (!paths.Contains(ResourcesDirectory + name + ".mat"))
                throw new BuildFailedException("Missing scenery shader-reference material: " + name);
        foreach (string name in new[] { "cave-rock-material", "cave-grass-material" })
            if (!paths.Contains("Assets/Game/Resources/GoldenRiver/Descent/" + name + ".mat"))
                throw new BuildFailedException("Missing cave shader-reference material: " + name);
        foreach(string name in CaveTerrainImport.MaskNames)
            if(!paths.Contains("Assets/Game/Resources/GoldenRiver/Descent/"+name+".png"))
                throw new BuildFailedException("Missing precomputed cave mask: "+name);
        long descentBytes = GoldenRiverArt.Where(n => n.StartsWith("Descent/", System.StringComparison.Ordinal))
            .Sum(n => UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(
                AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Game/Resources/GoldenRiver/" + n + ".png")));
        Debug.Log("Dry descent imported texture bytes: " + descentBytes);
        Debug.Log("Packed scenery verified: " + (ActiveLayers.Length + GoldenRiverArt.Length) + " active paintings and five shader-reference materials; historical art excluded.");
    }

    public static readonly string[] GoldenRiverArt = {
        "Descent/entry-module-0-v1", "Descent/entry-module-1-v1", "Descent/entry-module-2-v1",
        "Descent/entry-module-3-v1", "Descent/entry-module-4-v1", "Descent/entry-module-5-v1",
        "Descent/terrain-stone-v1", "Descent/discovery-floor-v2", "Descent/discovery-ceiling-v2", "Descent/discovery-rear-v1",
        "Descent/entry-study-left-v2", "Descent/entry-study-right-v2", "Descent/entry-study-floor-v1",
        "Descent/entry-study-rear-v1", "Descent/entry-study-valley-v1", "Descent/ruin-v1",
        "finale-watchtower-v1", "finale-rubble-v1", "finale-hillside-v1", "finale-gate-v1", "finale-guide-v1", "finale-vertical-guide-v1", "finale-wall-v1", "finale-pier-v1", "finale-garden-v1",
        "checkpoint-states-v1", "checkpoint-tree-v1", "checkpoint-arch-v1",
        "near-extended-panorama-v1", "far-extended-panorama-v1", "bank-tall-ascent-v1", "shelf-ascent-masonry-v1", "landmark-hillside-fortress-v1", "shelf-root", "shelf-masonry", "landmark-waterwheel-hill-v2", "landmark-viaduct-hill-v2", "landmark-willow-hill-v2",
        "valley-panorama-v3", "far-valley-panorama-v4", "bank-narrow-v3", "bank-medium-v3", "bank-wide-v3",
        "shelf-root-compact-v3", "shelf-masonry-compact-v3",
        "egg-watching-fish", "egg-no-cats-sign", "river-water-v2", "river-rock-groups-v1", "river-rock-groups-v2", "river-rock-groups-v3"
    };

    [MenuItem("LegendOfCat/Scenery/Import Golden River variety art")]
    public static void ImportGoldenRiverArt()
    {
        CaveTerrainImport.Prepare();
        const string rejectedRoof="Assets/Game/Resources/GoldenRiver/Descent/discovery-ceiling-v1.png";
        if(AssetDatabase.LoadAssetAtPath<Texture2D>(rejectedRoof)) {
            string error=AssetDatabase.MoveAsset(rejectedRoof,"Assets/Game/Art/GoldenRiverArchive/discovery-ceiling-v1.png");
            if(!string.IsNullOrEmpty(error))throw new System.InvalidOperationException(error);
        }
        const string rejectedFloor="Assets/Game/Resources/GoldenRiver/Descent/discovery-floor-v1.png";
        if(AssetDatabase.LoadAssetAtPath<Texture2D>(rejectedFloor)) {
            string error=AssetDatabase.MoveAsset(rejectedFloor,"Assets/Game/Art/GoldenRiverArchive/discovery-floor-v1.png");
            if(!string.IsNullOrEmpty(error))throw new System.InvalidOperationException(error);
        }
        const string entryMaterial="Assets/Game/Resources/GoldenRiver/Descent/entry-study-material.mat";
        if(!AssetDatabase.LoadAssetAtPath<Material>(entryMaterial))
            AssetDatabase.CreateAsset(new Material(Shader.Find("LegendOfCat/EntryStudyPaint")),entryMaterial);
        const string rejectedEntry="Assets/Game/Resources/GoldenRiver/Descent/entry-study-right-v1.png";
        if(AssetDatabase.LoadAssetAtPath<Texture2D>(rejectedEntry)) {
            string error=AssetDatabase.MoveAsset(rejectedEntry,"Assets/Game/Art/GoldenRiverArchive/entry-study-right-v1.png");
            if(!string.IsNullOrEmpty(error)) throw new System.InvalidOperationException(error);
        }
        const string rejectedLeft="Assets/Game/Resources/GoldenRiver/Descent/entry-study-left-v1.png";
        if(AssetDatabase.LoadAssetAtPath<Texture2D>(rejectedLeft)) {
            string error=AssetDatabase.MoveAsset(rejectedLeft,"Assets/Game/Art/GoldenRiverArchive/entry-study-left-v1.png");
            if(!string.IsNullOrEmpty(error)) throw new System.InvalidOperationException(error);
        }
        foreach (string name in new[] { "garden-court-v1", "garden-wall-v1" }) {
            string source = "Assets/Game/Resources/GoldenRiver/" + name + ".png";
            if (!AssetDatabase.LoadAssetAtPath<Texture2D>(source)) continue;
            const string archive = "Assets/Game/Art/GoldenRiverArchive";
            if (!AssetDatabase.IsValidFolder(archive)) AssetDatabase.CreateFolder("Assets/Game/Art", "GoldenRiverArchive");
            string error = AssetDatabase.MoveAsset(source, archive + "/" + name + ".png");
            if (!string.IsNullOrEmpty(error)) throw new System.InvalidOperationException(error);
        }
        const string checkpointMaterial = "Assets/Game/Resources/GoldenRiver/checkpoint-material.mat";
        if (!AssetDatabase.LoadAssetAtPath<Material>(checkpointMaterial))
            AssetDatabase.CreateAsset(new Material(Shader.Find("LegendOfCat/Checkpoint")), checkpointMaterial);
        ArchiveLandmarkCutouts();
        const string repeatingStones = "Assets/Game/Resources/GoldenRiver/river-stones-slab-v1.png";
        if (AssetDatabase.LoadAssetAtPath<Texture2D>(repeatingStones)) {
            string error = AssetDatabase.MoveAsset(repeatingStones, "Assets/Game/Art/GoldenRiverArchive/river-stones-slab-v1.png");
            if (!string.IsNullOrEmpty(error)) throw new System.InvalidOperationException(error);
        }
        const string rejectedKing = "Assets/Game/Resources/GoldenRiver/egg-cat-king.png";
        if (AssetDatabase.LoadAssetAtPath<Texture2D>(rejectedKing)) {
            string error = AssetDatabase.MoveAsset(rejectedKing, "Assets/Game/Art/GoldenRiverArchive/egg-cat-king.png");
            if (!string.IsNullOrEmpty(error)) throw new System.InvalidOperationException(error);
        }
        foreach (string name in GoldenRiverArt) {
            if(name.StartsWith("Descent/entry-module-", System.StringComparison.Ordinal)) continue;
            var importer = (TextureImporter)AssetImporter.GetAtPath("Assets/Game/Resources/GoldenRiver/" + name + ".png");
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.isReadable = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = name.Contains("extended-panorama") ? 16384 : name.Contains("panorama") || name == "checkpoint-states-v1" ? 4096 : 2048;
            if (name.StartsWith("Descent/", System.StringComparison.Ordinal)) {
                // Desktop block compression requires aligned dimensions. UV registration stays unchanged.
                importer.npotScale = TextureImporterNPOTScale.ToNearest;
                var platform = importer.GetPlatformTextureSettings("Standalone");
                platform.overridden = true;
                platform.maxTextureSize = 2048;
                platform.format = TextureImporterFormat.BC7;
                platform.compressionQuality = 100;
                importer.SetPlatformTextureSettings(platform);
            }
            importer.SaveAndReimport();
        }
        AssetDatabase.SaveAssets();
    }

    static void ArchiveLandmarkCutouts()
    {
        const string archive = "Assets/Game/Art/GoldenRiverArchive";
        foreach (string name in new[] { "waterwheel", "viaduct", "willow" }) {
            string source = "Assets/Game/Resources/GoldenRiver/landmark-" + name + ".png";
            if (!AssetDatabase.LoadAssetAtPath<Texture2D>(source)) continue;
            if (!AssetDatabase.IsValidFolder(archive)) AssetDatabase.CreateFolder("Assets/Game/Art", "GoldenRiverArchive");
            // Unity moves the asset and its GUID-bearing metadata together.
            string error = AssetDatabase.MoveAsset(source, archive + "/landmark-" + name + ".png");
            if (!string.IsNullOrEmpty(error)) throw new System.InvalidOperationException(error);
        }
    }

    // Resources materials retain shader references in player builds; Shader.Find alone does not.
    [MenuItem("LegendOfCat/Scenery/Prepare runtime materials")]
    public static void PrepareRuntimeMaterials()
    {
        foreach (var pair in new[] {
            new[] { "background-material", "LegendOfCat/BackgroundStudy" },
            new[] { "terrain-material", "LegendOfCat/StudyTerrain" },
            new[] { "water-material", "LegendOfCat/StudyWater" }
        }) {
            string path = ResourcesDirectory + pair[0] + ".mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(path)) continue;
            var shader = Shader.Find(pair[1]);
            if (!shader) throw new System.InvalidOperationException("Missing scenery shader: " + pair[1]);
            AssetDatabase.CreateAsset(new Material(shader), path);
        }
        AssetDatabase.SaveAssets();
    }

    [MenuItem("LegendOfCat/Scenery/Import painted layers")]
    public static void ImportLayers()
    {
        foreach (string name in ActiveLayers) {
            var importer = (TextureImporter)AssetImporter.GetAtPath($"Assets/Game/Resources/BackgroundStudy/{name}.png");
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            // Retain crisp alpha-cutout grass/ivy in the fixed orthographic course.
            // Mips need a separate coverage/minification review, not a global toggle.
            importer.mipmapEnabled = false;
            importer.isReadable = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp; importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }
    }
}
