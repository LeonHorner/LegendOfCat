using System.Text.Json;

// Side-effect orchestration lives here so it can be tested without an installed
// AppImage, GitHub access, Unity, or Velopack process termination.
internal static class UpdateWorkflow
{
    internal static async Task CheckAsync(
        Func<Task> loadOptionalHistory,
        Func<Task> checkForUpdates,
        Func<Exception, Task> historyUnavailable)
    {
        ArgumentNullException.ThrowIfNull(loadOptionalHistory);
        ArgumentNullException.ThrowIfNull(checkForUpdates);
        ArgumentNullException.ThrowIfNull(historyUnavailable);

        try {
            await loadOptionalHistory();
        } catch (Exception error) when (error is HttpRequestException or TaskCanceledException or JsonException) {
            // Even a logging failure must not turn optional history into a
            // prerequisite for the actual update check.
            try { await historyUnavailable(error); } catch (Exception) { }
        }

        await checkForUpdates();
    }

    internal static async Task ApplyAsync(
        bool isLinux,
        Func<Task> waitForGameExit,
        Func<Task> waitForLauncherExit,
        Action launchUpdater)
    {
        ArgumentNullException.ThrowIfNull(waitForGameExit);
        ArgumentNullException.ThrowIfNull(waitForLauncherExit);
        ArgumentNullException.ThrowIfNull(launchUpdater);

        await waitForGameExit();
        // The Linux updater must launch before waiting for the AppImage
        // launcher to exit, since that may unmount the updater runtime.
        if (!isLinux) await waitForLauncherExit();
        launchUpdater();
    }
}
