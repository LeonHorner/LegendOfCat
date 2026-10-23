using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

var cases = new (string Name, Func<Task> Run)[] {
    ("history success runs check after history", HistorySuccess),
    ("HTTP 403 on history still runs update check", RateLimitedHistory),
    ("HTTP 429 on history still runs update check", TooManyRequests),
    ("history timeout still runs update check", HistoryTimeout),
    ("malformed history still runs update check", InvalidHistory),
    ("history warning logger failure never blocks update check", WarningLogFailure),
    ("update check errors still propagate", UpdateFailurePropagates),
    ("Linux launches only after game exits and does not await launcher", LinuxHandoff),
    ("Windows awaits both game and launcher exit", WindowsHandoff),
    ("failed game shutdown blocks update handoff", GameExitFailure),
    ("installer launch error propagates", InstallerFailure),
};

var failed = 0;
foreach (var (name, run) in cases) {
    try {
        await run();
        Console.WriteLine("PASS " + name);
    } catch (Exception e) {
        failed++;
        Console.Error.WriteLine("FAIL " + name + ": " + e);
    }
}
Console.WriteLine($"{cases.Length - failed}/{cases.Length} updater behavior tests passed");
return failed == 0 ? 0 : 1;

static async Task HistorySuccess()
{
    var events = new List<string>();
    await UpdateWorkflow.CheckAsync(
        () => { events.Add("history"); return Task.CompletedTask; },
        () => { events.Add("check"); return Task.CompletedTask; },
        _ => throw new Exception("no warning expected"));
    Equal("history,check", string.Join(",", events));
}

static async Task RateLimitedHistory() =>
    await HistoryHttpFailure(HttpStatusCode.Forbidden);

static async Task TooManyRequests() =>
    await HistoryHttpFailure(HttpStatusCode.TooManyRequests);

static async Task HistoryHttpFailure(HttpStatusCode status)
{
    using var client = new HttpClient(new StubHistoryHandler(status));
    var checkedUpdate = false;
    Exception? warning = null;
    await UpdateWorkflow.CheckAsync(
        async () => { await client.GetFromJsonAsync<JsonElement[]>("https://example.invalid/releases"); },
        () => { checkedUpdate = true; return Task.CompletedTask; },
        error => { warning = error; return Task.CompletedTask; });
    Check(checkedUpdate, "A failed optional history HTTP call must not skip update check");
    Check(warning is HttpRequestException { StatusCode: var code } && code == status,
        "Warning must include the originating HTTP status");
}

static async Task HistoryTimeout()
{
    var checkedUpdate = false;
    await UpdateWorkflow.CheckAsync(
        () => throw new TaskCanceledException("Simulated history request timeout"),
        () => { checkedUpdate = true; return Task.CompletedTask; },
        error => {
            Check(error is TaskCanceledException, "Expected cancellation warning");
            return Task.CompletedTask;
        });
    Check(checkedUpdate, "History timeout must not prevent update check");
}

static async Task InvalidHistory()
{
    var checkedUpdate = false;
    await UpdateWorkflow.CheckAsync(
        () => throw new JsonException("Invalid response"),
        () => { checkedUpdate = true; return Task.CompletedTask; },
        error => {
            Check(error is JsonException, "Expected JSON warning");
            return Task.CompletedTask;
        });
    Check(checkedUpdate, "Malformed history must not prevent update check");
}

static async Task WarningLogFailure()
{
    var checkedUpdate = false;
    await UpdateWorkflow.CheckAsync(
        () => throw new HttpRequestException("403", null, HttpStatusCode.Forbidden),
        () => { checkedUpdate = true; return Task.CompletedTask; },
        _ => throw new IOException("Simulated unwritable diagnostic log"));
    Check(checkedUpdate, "Even diagnostic errors must not block updates");
}

static async Task UpdateFailurePropagates()
{
    await Throws<InvalidOperationException>(() => UpdateWorkflow.CheckAsync(
        () => Task.CompletedTask,
        () => throw new InvalidOperationException("Feed invalid"),
        _ => Task.CompletedTask));
}

static async Task LinuxHandoff()
{
    var gameExit = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    var events = new List<string>();
    var handoff = UpdateWorkflow.ApplyAsync(
        true,
        async () => { events.Add("wait game"); await gameExit.Task; events.Add("game exited"); },
        () => throw new Exception("Linux must not wait for launcher exit before handoff"),
        () => events.Add("apply"));
    Equal("wait game", string.Join(",", events));
    Check(!handoff.IsCompleted, "Cannot start updater before game exit");
    gameExit.SetResult(true);
    await handoff;
    Equal("wait game,game exited,apply", string.Join(",", events));
}

static async Task WindowsHandoff()
{
    var gameExit = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    var launcherExit = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    var events = new List<string>();
    var handoff = UpdateWorkflow.ApplyAsync(
        false,
        async () => { events.Add("wait game"); await gameExit.Task; events.Add("game exited"); },
        async () => { events.Add("wait launcher"); await launcherExit.Task; events.Add("launcher exited"); },
        () => events.Add("apply"));
    Equal("wait game", string.Join(",", events));
    gameExit.SetResult(true);
    // RunContinuationsAsynchronously means the waiting callback resumes on the pool.
    await Until(() => events.Contains("wait launcher"));
    Check(!handoff.IsCompleted, "Windows must await launcher exit");
    launcherExit.SetResult(true);
    await handoff;
    Equal("wait game,game exited,wait launcher,launcher exited,apply", string.Join(",", events));
}

static async Task GameExitFailure()
{
    var applied = false;
    await Throws<IOException>(() => UpdateWorkflow.ApplyAsync(
        true,
        () => throw new IOException("Simulated process wait failure"),
        () => Task.CompletedTask,
        () => applied = true));
    Check(!applied, "Failed game exit must not launch installer");
}

static async Task InstallerFailure()
{
    await Throws<InvalidOperationException>(() => UpdateWorkflow.ApplyAsync(
        true,
        () => Task.CompletedTask,
        () => Task.CompletedTask,
        () => throw new InvalidOperationException("Unable to spawn installer")));
}

static async Task Throws<T>(Func<Task> operation) where T : Exception
{
    try { await operation(); }
    catch (T) { return; }
    throw new Exception($"Expected {typeof(T).Name}");
}

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

static void Equal(string expected, string actual)
{
    if (expected != actual) throw new Exception($"Expected [{expected}], got [{actual}]");
}

static async Task Until(Func<bool> ready)
{
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    while (!ready()) {
        timeout.Token.ThrowIfCancellationRequested();
        await Task.Delay(10, timeout.Token);
    }
}

sealed class StubHistoryHandler(HttpStatusCode status) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken) =>
        Task.FromResult(new HttpResponseMessage(status) {
            Content = new StringContent("{\"message\":\"API rate limit exceeded\"}")
        });
}
