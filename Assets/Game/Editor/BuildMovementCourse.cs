using System.IO;
using UnityEditor;
using UnityEngine;

public static class BuildMovementCourse
{
    [MenuItem("LegendOfCat/Create movement course prefab")]
    public static void Create()
    {
        Directory.CreateDirectory("Assets/Game/Resources");
        var root = new GameObject("MovementCourse");
        root.AddComponent<MovementCourse>();
        PrefabUtility.SaveAsPrefabAsset(root, "Assets/Game/Resources/MovementCourse.prefab");
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
