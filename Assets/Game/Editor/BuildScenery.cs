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
        if (paths.Any(p => p.StartsWith("Assets/Game/Art/BackgroundStudyArchive/", System.StringComparison.Ordinal)))
            throw new BuildFailedException("Historical background artwork must not be packed into the player.");
        foreach (string name in ActiveLayers)
            if (!paths.Contains(ResourcesDirectory + name + ".png"))
                throw new BuildFailedException("Missing selected scenery in player build: " + name);
        foreach (string name in new[] { "background-material", "terrain-material", "water-material" })
            if (!paths.Contains(ResourcesDirectory + name + ".mat"))
                throw new BuildFailedException("Missing scenery shader-reference material: " + name);
        Debug.Log("Packed scenery verified: nine active paintings and three shader-reference materials; historical art excluded.");
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
            // Retain crisp alpha-cutout grass/ivy in the fixed orthographic study.
            // Mips need a separate coverage/minification review, not a global toggle.
            importer.mipmapEnabled = false;
            importer.isReadable = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp; importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }
    }

}
