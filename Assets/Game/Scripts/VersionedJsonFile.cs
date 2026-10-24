using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// File schemas supply their own defaults, validation and one migration per old version.
public sealed class VersionedJsonFile
{
    readonly string path;
    readonly int currentVersion;
    readonly Action<JObject> validate;
    string expectedContents;
    public JObject Data { get; private set; }
    public bool CanWrite { get; private set; }
    public string Warning { get; private set; } = "";

    public VersionedJsonFile(string path, int currentVersion, Func<JObject> defaults,
        Action<JObject> validate, params Action<JObject>[] migrations)
    {
        if (currentVersion < 0 || migrations.Length != currentVersion)
            throw new ArgumentException("Provide one migration for every preceding schema version, starting at zero.");
        this.path = Path.GetFullPath(path);
        this.currentVersion = currentVersion;
        this.validate = validate;
        Data = defaults();
        Data["schemaVersion"] = currentVersion;
        validate(Data);
        try
        {
            if (!File.Exists(this.path)) { CanWrite = true; return; }
            expectedContents = File.ReadAllText(this.path);
            var loaded = JObject.Parse(expectedContents, new JsonLoadSettings {
                DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error
            });
            var version = ReadVersion(loaded);
            if (version > currentVersion) {
                Warning = "This file uses a newer data format. Defaults are temporary; the original file will not be overwritten.";
                return;
            }
            int originalVersion = version;
            while (version < currentVersion) {
                migrations[version](loaded);
                loaded["schemaVersion"] = ++version;
            }
            validate(loaded);
            if (ReadVersion(loaded) != currentVersion) throw new InvalidDataException("Invalid migrated schema version.");
            if (originalVersion < currentVersion) {
                EnsureUnchanged();
                // Retain exact original bytes. A unique backup cannot overwrite an earlier recovery copy.
                File.Copy(this.path, this.path + ".v" + originalVersion + "." + Guid.NewGuid().ToString("N") + ".bak");
                WriteAtomic(loaded);
            }
            Data = loaded;
            CanWrite = true;
        }
        catch (Exception error)
        {
            Warning = "The data file could not be loaded or migrated. Defaults are temporary; the original file is preserved. " + error.Message;
        }
    }

    static int ReadVersion(JObject data)
    {
        var token = data["schemaVersion"];
        if (token == null || token.Type != JTokenType.Integer)
            throw new InvalidDataException("schemaVersion must be a non-negative integer.");
        long value = token.Value<long>();
        if (value < 0 || value > int.MaxValue) throw new InvalidDataException("Invalid schemaVersion.");
        return (int)value;
    }

    public void Save()
    {
        if (!CanWrite) throw new InvalidOperationException("This data file is protected from writes.");
        if (ReadVersion(Data) != currentVersion) throw new InvalidDataException("Unexpected schemaVersion while saving.");
        validate(Data);
        WriteAtomic(Data);
    }

    void EnsureUnchanged()
    {
        if (expectedContents == null ? File.Exists(path) :
            !File.Exists(path) || File.ReadAllText(path) != expectedContents)
            throw new IOException("The data file changed since it was loaded. Reload it before saving.");
    }

    void WriteAtomic(JObject data)
    {
        EnsureUnchanged();
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        var contents = data.ToString(Formatting.Indented);
        try {
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None)) {
                var bytes = new UTF8Encoding(false).GetBytes(contents);
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }
            EnsureUnchanged();
            if (expectedContents == null) File.Move(temporary, path);
            else File.Replace(temporary, path, null);
            expectedContents = contents;
        }
        finally {
            try { if (File.Exists(temporary)) File.Delete(temporary); } catch (IOException) { }
        }
    }
}

public static class MenuSettings
{
    public const int CurrentVersion = 0;

    public static VersionedJsonFile Open(string path) => new VersionedJsonFile(path, CurrentVersion,
        () => new JObject { ["lastOfferedVersion"] = "" },
        data => {
            if (data["lastOfferedVersion"] == null) data["lastOfferedVersion"] = "";
            if (data["lastOfferedVersion"].Type != JTokenType.String)
                throw new InvalidDataException("lastOfferedVersion must be a string.");
        });
}

public static class DisplaySettings
{
    public const int CurrentVersion = 0;

    public static VersionedJsonFile Open(string path) => new VersionedJsonFile(path, CurrentVersion,
        () => new JObject { ["width"] = 1280, ["height"] = 720, ["fullscreen"] = false },
        data => {
            if (data["width"] == null) data["width"] = 1280;
            if (data["height"] == null) data["height"] = 720;
            if (data["fullscreen"] == null) data["fullscreen"] = false;
            if (data["width"].Type != JTokenType.Integer || data["height"].Type != JTokenType.Integer ||
                data["fullscreen"].Type != JTokenType.Boolean)
                throw new InvalidDataException("Invalid display preference types.");
            int width = data["width"].Value<int>(), height = data["height"].Value<int>();
            if (width < 320 || width > 16384 || height < 180 || height > 16384)
                throw new InvalidDataException("Display dimensions are outside safe bounds.");
        });
}
