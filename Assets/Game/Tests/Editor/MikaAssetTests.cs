using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class MikaAssetTests
{
    [Test]
    public void ProductionCharacterHasNoRejectedAssetDependenciesOrMissingScripts()
    {
        const string prefabPath = "Assets/Game/Resources/MikaOriginal.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        Assert.That(prefab, Is.Not.Null);
        foreach (var child in prefab.GetComponentsInChildren<Transform>(true))
            Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject), Is.Zero);
        var dependencies = AssetDatabase.GetDependencies(prefabPath, true);
        Assert.That(dependencies.Any(p => p.Contains("rig-part") || p.Contains("mika-rig")
            || p.Contains("movement-smooth") || p.Contains("movement-refined") || p.Contains("candidate-frame-")), Is.False);
        Assert.That(AssetDatabase.FindAssets("MikaGait t:MonoScript"), Is.Empty);
        Assert.That(AssetDatabase.FindAssets("MikaRigPresentation t:MonoScript"), Is.Empty);
    }

    [Test]
    public void ArchivedExperimentsAndMetadataMatchTheirOriginalHashes()
    {
        string root = Path.GetDirectoryName(Application.dataPath);
        string archive = Path.Combine(root, "docs/design/mika-animation/archive");
        var manifest = JArray.Parse(File.ReadAllText(Path.Combine(archive, "manifest.json")));
        Assert.That(manifest.Count, Is.GreaterThan(200));
        using (var hash = SHA256.Create())
            foreach (var entry in manifest) {
                string relative = (string)entry["original"];
                string path = Path.GetFullPath(Path.Combine(archive, relative));
                Assert.That(path.StartsWith(Path.GetFullPath(archive) + Path.DirectorySeparatorChar), Is.True);
                Assert.That(File.Exists(path), Is.True, relative);
                string actual = System.BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(path)))
                    .Replace("-", "").ToLowerInvariant();
                Assert.That(actual, Is.EqualTo((string)entry["sha256"]), relative);
                if ((string)entry["operation"] == "move") Assert.That(File.Exists(Path.Combine(root, relative)), Is.False, relative);
            }
    }
}
