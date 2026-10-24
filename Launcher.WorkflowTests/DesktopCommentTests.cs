using System.Text;

internal static class DesktopCommentTests
{
    internal static Task MigratesGeneratedKdeComment()
    {
        WithEntry((entry, image) => {
            File.WriteAllText(entry,
                "[Desktop Entry]\nType=Application\nName=LegendOfCat\n" +
                "Comment=LegendOfCat 0.5.0\n" +
                "Exec=" + image + "\n" +
                "Icon=legendofcat\n");
            Check(LinuxDesktopComment.RewriteGeneratedComment(entry, image), "Expected generated comment migration");
            var result = File.ReadAllText(entry);
            Check(result.Contains("Comment=A little cat, a big adventure\n"),
                "Expected fixed description");
            Check(result.Contains("Icon=legendofcat\n") && result.Contains("Exec=" + image + "\n"),
                "Unrelated entry keys must not change");
            Check(!LinuxDesktopComment.RewriteGeneratedComment(entry, image), "Must be idempotent");
        });
        return Task.CompletedTask;
    }

    internal static Task RespectsUserComment()
    {
        WithEntry((entry, image) => {
            var initial = "[Desktop Entry]\nName=LegendOfCat\nComment=My personal cat game\nExec=" + image + "\n";
            File.WriteAllText(entry, initial);
            Check(!LinuxDesktopComment.RewriteGeneratedComment(entry, image),
                "Custom comments are user-owned");
            Check(File.ReadAllText(entry) == initial, "Must preserve the original entry");
        });
        return Task.CompletedTask;
    }

    internal static Task RequiresCorrectAppImage()
    {
        WithEntry((entry, image) => {
            var other = Path.Combine(Path.GetDirectoryName(entry)!, "Other.AppImage");
            File.WriteAllText(other, "other binary");
            var initial = "[Desktop Entry]\nName=LegendOfCat\nComment=LegendOfCat 0.5.0\nExec=" + other + "\n";
            File.WriteAllText(entry, initial);
            Check(!LinuxDesktopComment.RewriteGeneratedComment(entry, image),
                "Must not touch an entry for a different installation");
            Check(File.ReadAllText(entry) == initial, "Unrelated entry modified");
        });
        return Task.CompletedTask;
    }

    internal static Task RequiresExactApplicationName()
    {
        WithEntry((entry, image) => {
            var initial = "[Desktop Entry]\nName=OtherGame\nComment=LegendOfCat 0.5.0\nExec=" + image + "\n";
            File.WriteAllText(entry, initial);
            Check(!LinuxDesktopComment.RewriteGeneratedComment(entry, image), "Wrong app name should not migrate");
            Check(File.ReadAllText(entry) == initial, "Wrong app modified");
        });
        return Task.CompletedTask;
    }

    internal static Task HandlesQuotedPathsAndLocales()
    {
        WithEntry((entry, image) => {
            var initial = "[Desktop Entry]\r\nName=LegendOfCat\r\n" +
                "Comment=LegendOfCat 0.6.0\r\n" +
                "Comment[de]=Benutzerdefinierter Text\r\n" +
                "Exec=\"" + image + "\" %U\r\n";
            File.WriteAllText(entry, initial);
            Check(LinuxDesktopComment.RewriteGeneratedComment(entry, image), "Quoted filename should migrate");
            var changed = File.ReadAllText(entry);
            Check(changed.Contains("Comment=A little cat, a big adventure\r\n"), "CRLF must be preserved");
            Check(changed.Contains("Comment[de]=Benutzerdefinierter Text\r\n"), "Localized comment modified");
            Check(changed.Contains("Exec=\"" + image + "\" %U\r\n"), "Exec was modified");
        }, imageContainsSpaces: true);
        return Task.CompletedTask;
    }

    internal static Task RejectsCommandWrappers()
    {
        WithEntry((entry, image) => {
            var initial = "[Desktop Entry]\nName=LegendOfCat\nComment=LegendOfCat 0.5.0\n" +
                "Exec=sh -c '" + image + "'\n";
            File.WriteAllText(entry, initial);
            Check(!LinuxDesktopComment.RewriteGeneratedComment(entry, image),
                "Unknown Exec commands must remain unchanged");
            Check(File.ReadAllText(entry) == initial, "Wrapped launcher modified");
        });
        return Task.CompletedTask;
    }

    internal static Task SkipsSymlinkedEntries()
    {
        if (!OperatingSystem.IsLinux()) return Task.CompletedTask;
        WithEntry((entry, image) => {
            var target = Path.Combine(Path.GetDirectoryName(entry)!, "target.desktop");
            var initial = "[Desktop Entry]\nName=LegendOfCat\n" +
                "Comment=LegendOfCat 0.5.0\nExec=" + image + "\n";
            File.WriteAllText(target, initial);
            File.CreateSymbolicLink(entry, target);
            Check(!LinuxDesktopComment.RewriteGeneratedComment(entry, image), "Symbolic links must be skipped");
            Check(File.ReadAllText(target) == initial, "Linked target modified");
        });
        return Task.CompletedTask;
    }

    private static void WithEntry(Action<string, string> verify, bool imageContainsSpaces = false)
    {
        var dir = Directory.CreateTempSubdirectory("legendofcat-kde-test-");
        try {
            var image = Path.Combine(dir.FullName,
                imageContainsSpaces ? "Legend Of Cat.AppImage" : "LegendOfCat.AppImage");
            File.WriteAllText(image, "AppImage placeholder");
            verify(Path.Combine(dir.FullName, "legendofcat.desktop"), image);
        } finally {
            dir.Delete(recursive: true);
        }
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
