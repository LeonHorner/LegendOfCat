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

    // Windows-only: Linux uses the mount-independent native supervisor,
    // which must be started BEFORE authorizing Unity shutdown.
    internal static async Task ApplyAfterShutdownAsync(
        Func<Task> waitForGameExit,
        Func<Task> waitForLauncherExit,
        Action launchUpdater)
    {
        ArgumentNullException.ThrowIfNull(waitForGameExit);
        ArgumentNullException.ThrowIfNull(waitForLauncherExit);
        ArgumentNullException.ThrowIfNull(launchUpdater);

        await waitForGameExit();
        await waitForLauncherExit();
        launchUpdater();
    }
}
