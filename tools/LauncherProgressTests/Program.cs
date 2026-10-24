using System.Net;
using System.Reflection;
using Velopack;

// Exercise the actual pinned-downloader override with in-memory HTTP responses.
// No external network, installation, update application or test framework is needed.
var snapshots = new List<(int Percent, long Received, long Total)>();
var type = typeof(DownloadProgress).Assembly.GetType("ReportingDownloader")!;
var report = new Action<int, long, long>((p, received, total) => snapshots.Add((p, received, total)));
var downloader = Activator.CreateInstance(type, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
    null, new object[] { report }, null)!;
var full = new VelopackAsset { FileName = "full.nupkg", Size = 1000 };
var a = new VelopackAsset { FileName = "a-delta.nupkg", Size = 200 };
var b = new VelopackAsset { FileName = "b-delta.nupkg", Size = 300 };
type.GetMethod("Configure")!.Invoke(downloader, new object[] { new UpdateInfo(full, false, null, new[] { a, b }) });
var root = Path.Combine(Path.GetTempPath(), "LegendOfCat-progress-" + Guid.NewGuid());
Directory.CreateDirectory(root);
try {
    await Task.WhenAll(Download(a), Download(b));
    Check(snapshots[^1].Received == 500 && snapshots[^1].Total == 500, "Concurrent Delta payloads must aggregate.");
    type.GetMethod("ReportOverall")!.Invoke(downloader, new object[] { 70 });
    int start = snapshots.Count;
    await Download(full, partial: true);
    Check(snapshots[start] == (0, 0L, 1000L), "Full fallback must switch totals and reset old progress.");
    Check(snapshots[^1].Received == 1000 && snapshots[^1].Total == 1000, "Full payload count must finish correctly.");
    start = snapshots.Count;
    await Download(full, partial: true);
    Check(snapshots[start].Received == 0 && snapshots[^1].Received == 1000, "A retry must restart its package count.");
    type.GetMethod("Configure")!.Invoke(downloader, new object[] { new UpdateInfo(full, false) });
    type.GetMethod("ReportOverall")!.Invoke(downloader, new object[] { 0 });
    Check(snapshots[^1] == (0, 0L, 0L), "New update attempts must clear stale byte metadata.");
    Console.WriteLine("Launcher download byte integration checks passed (chunked responses, concurrent Deltas, full fallback, retries and reset).");
} finally { Directory.Delete(root, true); }

async Task Download(VelopackAsset asset, bool partial = false)
{
    using var client = new HttpClient(new PayloadHandler(new byte[(int)asset.Size]));
    using var file = File.Create(Path.Combine(root, asset.FileName + (partial ? ".partial" : "")));
    var method = type.GetMethod("DownloadToStreamInternal", BindingFlags.Instance | BindingFlags.NonPublic)!;
    await (Task)method.Invoke(downloader, new object?[] { client, "https://unused.invalid/package", file, null, CancellationToken.None })!;
    Check(file.CanWrite && file.Length == asset.Size, "Observation must leave Velopack's destination intact and open.");
}
static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }

sealed class PayloadHandler(byte[] payload) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new UnknownLengthPayload(payload) });
}
sealed class UnknownLengthPayload(byte[] payload) : HttpContent
{
    protected override bool TryComputeLength(out long length) { length = 0; return false; }
    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        => stream.WriteAsync(payload, 0, payload.Length);
}
