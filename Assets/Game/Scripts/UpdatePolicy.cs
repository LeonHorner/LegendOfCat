using System;

// Shared by Unity and the updater so UI and installation use the same rules.
public static class UpdatePolicy
{
    public static bool IsStableRelease(bool draft, bool prerelease) => !draft && !prerelease;

    public static bool IsNewerStableVersion(string candidate, string installed)
    {
        return Version.TryParse((candidate ?? "").TrimStart('v'), out var next) &&
            Version.TryParse((installed ?? "").TrimStart('v'), out var current) && next > current;
    }

    public static bool ShouldOffer(string available, string lastOffered)
        => !string.IsNullOrEmpty(available) && available != lastOffered;

    public static bool IsReleaseUrl(string url) => url != null &&
        url.StartsWith("https://github.com/LeonHorner/LegendOfCat/releases/", StringComparison.Ordinal);
}
