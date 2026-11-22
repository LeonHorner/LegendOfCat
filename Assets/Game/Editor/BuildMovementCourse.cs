using System.IO;
using UnityEditor;
using UnityEngine;

public static class BuildMovementCourse
{
    [MenuItem("LegendOfCat/Create editor movement fixture")]
    public static void Create()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(MovementLabArchive.FixturePath));
        var root = new GameObject("MovementCourse");
        root.AddComponent<MovementCourse>();
        PrefabUtility.SaveAsPrefabAsset(root, MovementLabArchive.FixturePath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
