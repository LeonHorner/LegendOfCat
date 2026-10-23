using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

public class MigrationTests
{
    string directory, path;
    static Type RuntimeType(string name) => AppDomain.CurrentDomain.GetAssemblies()
        .Select(assembly => assembly.GetType(name)).First(type => type != null);
    static object Property(object store, string name) => store.GetType().GetProperty(name).GetValue(store);
    static JObject Data(object store) => (JObject)Property(store, "Data");
    static void Save(object store) => store.GetType().GetMethod("Save").Invoke(store, null);
    object OpenSettings() => RuntimeType("MenuSettings").GetMethod("Open").Invoke(null, new object[] { path });
    string[] Backups() => Directory.GetFiles(directory, "*.bak");
    byte[] Fixture(string name)
    {
        var bytes = File.ReadAllBytes(Path.Combine(Application.dataPath, "Game/Tests/Editor/Fixtures", name));
        File.WriteAllBytes(path, bytes);
        return bytes;
    }
    [SetUp] public void Setup()
    {
        directory = Path.Combine(Path.GetTempPath(), "LegendOfCat-migrations-" + Guid.NewGuid());
        Directory.CreateDirectory(directory);
        path = Path.Combine(directory, "settings.json");
    }
    [TearDown] public void Cleanup() => Directory.Delete(directory, true);

    object OpenDisplay() => RuntimeType("DisplaySettings").GetMethod("Open").Invoke(null, new object[] { path });

    [Test]
    public void DisplayPreferencesRoundTripAndPreserveUnknownFields()
    {
        var store = OpenDisplay();
        Assert.That((int)Data(store)["width"], Is.EqualTo(1280));
        Assert.That((int)Data(store)["height"], Is.EqualTo(720));
        Assert.That((bool)Data(store)["fullscreen"], Is.False);
        Data(store)["width"] = 1440;
        Data(store)["fullscreen"] = true;
        Data(store)["custom"] = "keep";
        Save(store);
        var loaded = OpenDisplay();
        Assert.That((int)Data(loaded)["width"], Is.EqualTo(1440));
        Assert.That((bool)Data(loaded)["fullscreen"], Is.True);
        Assert.That((string)Data(loaded)["custom"], Is.EqualTo("keep"));
    }

    [TestCase("{\"schemaVersion\":99,\"width\":1280,\"height\":720,\"fullscreen\":false}")]
    [TestCase("{\"schemaVersion\":0,\"width\":200,\"height\":720,\"fullscreen\":false}")]
    [TestCase("{\"schemaVersion\":0,\"width\":1280,\"height\":720,\"fullscreen\":\"yes\"}")]
    public void UnsafeDisplayFilesRemainUntouched(string original)
    {
        File.WriteAllText(path, original);
        var store = OpenDisplay();
        Assert.That(Property(store, "CanWrite"), Is.False);
        Assert.That((string)Property(store, "Warning"), Is.Not.Empty);
        CollectionAssert.AreEqual(System.Text.Encoding.UTF8.GetBytes(original), File.ReadAllBytes(path));
    }

    [Test] public void InitialFormatStartsAtZeroWithoutAMigration()
    {
        var original = Fixture("settings-v0.json");
        var store = OpenSettings();
        Assert.That(Property(store, "CanWrite"), Is.True);
        Assert.That((int)Data(store)["schemaVersion"], Is.EqualTo(0));
        Assert.That((string)Data(store)["lastOfferedVersion"], Is.EqualTo("0.2.0"));
        Assert.That((bool)Data(store)["customOption"]["keep"], Is.True);
        CollectionAssert.AreEqual(original, File.ReadAllBytes(path));
        Save(store);
        OpenSettings();
        Assert.That(Backups(), Is.Empty);
    }

    [Test] public void SupportedRoundTripPreservesUnknownFields()
    {
        Fixture("settings-v0.json");
        var store = OpenSettings();
        Data(store)["lastOfferedVersion"] = "0.4.0";
        Save(store);
        var reloaded = OpenSettings();
        Assert.That((string)Data(reloaded)["lastOfferedVersion"], Is.EqualTo("0.4.0"));
        Assert.That((bool)Data(reloaded)["customOption"]["keep"], Is.True);
        Assert.That(Backups(), Is.Empty);
        Assert.That(Directory.GetFiles(directory, "*.tmp"), Is.Empty);
    }

    [TestCase("settings-future.json")]
    [TestCase("settings-v1.json")]
    [TestCase("settings-unversioned.json")]
    [TestCase("settings-corrupt.txt")]
    [TestCase("settings-invalid.json")]
    public void UnsupportedOrInvalidFilesRemainByteForByteAndCannotBeSaved(string fixture)
    {
        var original = Fixture(fixture);
        var store = OpenSettings();
        Assert.That(Property(store, "CanWrite"), Is.False);
        Assert.That((string)Property(store, "Warning"), Is.Not.Empty);
        Data(store)["lastOfferedVersion"] = "0.5.0";
        var exception = Assert.Throws<TargetInvocationException>(() => Save(store));
        Assert.That(exception.InnerException, Is.TypeOf<InvalidOperationException>());
        CollectionAssert.AreEqual(original, File.ReadAllBytes(path));
        Assert.That(Backups(), Is.Empty);
    }

    [TestCase("-1")]
    [TestCase("1.5")]
    [TestCase("\"1\"")]
    [TestCase("null")]
    [TestCase("2147483648")]
    public void InvalidSchemaVersionsAreProtected(string version)
    {
        var original = "{\"schemaVersion\":" + version + "}";
        File.WriteAllText(path, original);
        Assert.That(Property(OpenSettings(), "CanWrite"), Is.False);
        Assert.That(File.ReadAllText(path), Is.EqualTo(original));
    }

    [Test] public void MissingFileUsesDefaultsWithoutWritingUntilSave()
    {
        var store = OpenSettings();
        Assert.That(File.Exists(path), Is.False);
        Assert.That(Property(store, "CanWrite"), Is.True);
        Save(store);
        Assert.That((int)JObject.Parse(File.ReadAllText(path))["schemaVersion"], Is.EqualTo(0));
    }

    [Test] public void DuplicateVersionCannotHideAFutureSchema()
    {
        const string original = "{\"schemaVersion\":99,\"schemaVersion\":0}";
        File.WriteAllText(path, original);
        Assert.That(Property(OpenSettings(), "CanWrite"), Is.False);
        Assert.That(File.ReadAllText(path), Is.EqualTo(original));
    }

    [Test] public void ChangedFileIsNotOverwrittenByAStaleWriter()
    {
        Fixture("settings-v0.json");
        var store = OpenSettings();
        const string changed = "{\"schemaVersion\":99,\"changedExternally\":true}";
        File.WriteAllText(path, changed);
        var exception = Assert.Throws<TargetInvocationException>(() => Save(store));
        Assert.That(exception.InnerException, Is.TypeOf<IOException>());
        Assert.That(File.ReadAllText(path), Is.EqualTo(changed));
        Assert.That(Directory.GetFiles(directory, "*.tmp"), Is.Empty);
    }

    object OpenChain(Action<JObject> secondStep)
    {
        Func<JObject> defaults = () => new JObject { ["score"] = new JObject { ["value"] = 0 } };
        Action<JObject> validate = data => {
            if (!(data["score"] is JObject score) || score["value"]?.Type != JTokenType.Integer)
                throw new InvalidDataException("Invalid synthetic score.");
        };
        Action<JObject> first = data => {
            data["score"] = int.Parse((string)data["scoreText"]);
            data.Remove("scoreText");
        };
        return Activator.CreateInstance(RuntimeType("VersionedJsonFile"), new object[] {
            path, 2, defaults, validate, new Action<JObject>[] { first, secondStep }
        });
    }

    [Test] public void ChainedMigrationsRunInOrderAndOnlyBackUpTheOriginal()
    {
        var original = Fixture("chain-v0.json");
        var store = OpenChain(data => data["score"] = new JObject { ["value"] = (int)data["score"] });
        Assert.That(Property(store, "CanWrite"), Is.True);
        Assert.That((int)Data(store)["score"]["value"], Is.EqualTo(7));
        Assert.That((int)Data(store)["schemaVersion"], Is.EqualTo(2));
        Assert.That((string)Data(store)["preserved"], Does.StartWith("synthetic"));
        Assert.That(Backups().Length, Is.EqualTo(1));
        CollectionAssert.AreEqual(original, File.ReadAllBytes(Backups()[0]));
        OpenChain(data => data["score"] = new JObject { ["value"] = (int)data["score"] });
        Assert.That(Backups().Length, Is.EqualTo(1), "Current files must not migrate again.");
    }

    [Test] public void FailedMigrationOrValidationCannotWriteAPartialResult()
    {
        var original = Fixture("chain-v0.json");
        var store = OpenChain(data => { throw new InvalidOperationException("Migration failed"); });
        Assert.That(Property(store, "CanWrite"), Is.False);
        CollectionAssert.AreEqual(original, File.ReadAllBytes(path));
        store = OpenChain(data => data["score"] = "invalid final result");
        Assert.That(Property(store, "CanWrite"), Is.False);
        CollectionAssert.AreEqual(original, File.ReadAllBytes(path));
        Assert.That(Backups(), Is.Empty);
    }

    [Test] public void FailedNewFileWriteCleansTemporaryDataAndKeepsDestination()
    {
        var store = OpenSettings();
        Directory.CreateDirectory(path);
        Assert.Throws<TargetInvocationException>(() => Save(store));
        Assert.That(Directory.Exists(path), Is.True);
        Assert.That(Directory.GetFiles(directory, "*.tmp"), Is.Empty);
    }
}
