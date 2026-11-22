using System;
using UnityEditor;
using UnityEngine;

// Preserve the old prefab/GUID for editor regression fixtures, outside player Resources.
public static class MovementLabArchive
{
    public const string FixturePath = "Assets/Game/Tests/Editor/Fixtures/MovementCourse.prefab";
    public static void Prepare()
    {
        const string source = "Assets/Game/Resources/MovementCourse.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(source)) {
            string error = AssetDatabase.MoveAsset(source, FixturePath);
            if (!string.IsNullOrEmpty(error)) throw new InvalidOperationException(error);
            AssetDatabase.SaveAssets();
        }
        if (!AssetDatabase.LoadAssetAtPath<GameObject>(FixturePath))
            throw new InvalidOperationException("Missing editor movement fixture.");
    }
}
