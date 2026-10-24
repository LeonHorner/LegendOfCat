using System.Text;
using System.Text.RegularExpressions;

// Velopack 1.2.161 embeds "Comment=LegendOfCat <version>" in its AppImage.
// KDE copies that value into a user-owned .desktop entry on integration, but
// replacing the AppImage does not refresh the copied entry.
// Only migrate that generated value for an entry pointing to THIS AppImage.
internal static class LinuxDesktopComment
{
    internal const string Description = "A little cat, a big adventure";

    private static readonly Regex GeneratedComment = new(
        @"^Comment=LegendOfCat [0-9]+\.[0-9]+\.[0-9]+(?:[-+][0-9A-Za-z.-]+)?$",
        RegexOptions.CultureInvariant);

    internal static void RefreshCurrentInstallation()
    {
        if (!OperatingSystem.IsLinux()) return;

        var appImage = Environment.GetEnvironmentVariable("APPIMAGE");
        if (string.IsNullOrWhiteSpace(appImage) || !Path.IsPathFullyQualified(appImage) ||
            !File.Exists(appImage)) return;

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrEmpty(home)) return;
        var xdgDataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
        var dataHome = !string.IsNullOrWhiteSpace(xdgDataHome) &&
            Path.IsPathFullyQualified(xdgDataHome)
            ? xdgDataHome : Path.Combine(home, ".local", "share");
        var applications = Path.Combine(dataHome, "applications");
        if (!Directory.Exists(applications)) return;

        try {
            foreach (var entry in Directory.EnumerateFiles(applications, "*.desktop")) {
                if (!Path.GetFileName(entry).Contains("legendofcat", StringComparison.OrdinalIgnoreCase))
                    continue;
                try { RewriteGeneratedComment(entry, appImage); }
                catch (IOException) { /* Desktop integration is optional. */ }
                catch (UnauthorizedAccessException) { /* User may have protected their entry. */ }
            }
        } catch (IOException) { /* Never interrupt game startup. */ }
        catch (UnauthorizedAccessException) { /* Never interrupt game startup. */ }
    }

    internal static bool RewriteGeneratedComment(string desktopFile, string appImage)
    {
        if (!Path.IsPathFullyQualified(appImage) || !File.Exists(desktopFile) ||
            (File.GetAttributes(desktopFile) & FileAttributes.ReparsePoint) != 0)
            return false;

        var content = File.ReadAllText(desktopFile);
        var lines = content.Split('\n');
        var inDesktopSection = false;
        var nameMatches = false;
        var executableMatches = false;
        var generatedCommentIndex = -1;

        for (var i = 0; i < lines.Length; i++) {
            var line = lines[i].TrimEnd('\r');
            if (line.StartsWith('[')) {
                inDesktopSection = line == "[Desktop Entry]";
                continue;
            }
            if (!inDesktopSection) continue;

            if (line == "Name=LegendOfCat")
                nameMatches = true;
            else if (line.StartsWith("Exec=", StringComparison.Ordinal))
                executableMatches = MatchesExecutable(line[5..], appImage);
            else if (GeneratedComment.IsMatch(line))
                generatedCommentIndex = i;
        }

        if (!nameMatches || !executableMatches || generatedCommentIndex < 0)
            return false;

        var previous = lines[generatedCommentIndex];
        lines[generatedCommentIndex] = "Comment=" + Description +
            (previous.EndsWith('\r') ? "\r" : "");
        var changed = string.Join("\n", lines);

        // Replace atomically in the same directory. Preserve Linux access mode
        // and never follow a .desktop symlink or rewrite a custom comment.
        var temp = desktopFile + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try {
            File.WriteAllText(temp, changed, new UTF8Encoding(false));
            if (OperatingSystem.IsLinux())
                File.SetUnixFileMode(temp, File.GetUnixFileMode(desktopFile));
            File.Move(temp, desktopFile, overwrite: true);
        } finally {
            if (File.Exists(temp)) File.Delete(temp);
        }
        return true;
    }

    private static bool MatchesExecutable(string value, string appImage)
    {
        // A KDE launcher's Exec line can use a quoted path, or freedesktop's
        // \\s escaping for spaces, optionally followed by a field code.
        var path = Path.GetFullPath(appImage);
        foreach (var candidate in new[] {
            path,
            path.Replace(" ", "\\s", StringComparison.Ordinal),
            "\"" + path + "\""
        }) {
            if (value == candidate || value == candidate + " %U" ||
                value == candidate + " %u" || value == candidate + " %F" ||
                value == candidate + " %f")
                return true;
        }
        return false;
    }
}
