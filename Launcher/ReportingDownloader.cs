using System.Diagnostics;
using Velopack;
using Velopack.Sources;

// Only observe Velopack's existing HTTP stream. Selection, retries, verification and patching stay with Velopack.
sealed class ReportingDownloader(Action<int, long, long> report) : HttpClientFileDownloader
{
    readonly object gate = new();
    readonly DownloadProgress bytes = new();
    Dictionary<string, long> deltas = new();
    KeyValuePair<string, long> full;
    string mode = "";
    int percent;
    long lastReport;

    public void Configure(UpdateInfo update)
    {
        lock (gate) {
            full = new(update.TargetFullRelease.FileName, update.TargetFullRelease.Size);
            deltas = update.DeltasToTarget.ToDictionary(x => x.FileName, x => x.Size);
            mode = ""; percent = 0; lastReport = 0;
            bytes.Reset(Array.Empty<KeyValuePair<string, long>>());
        }
    }
    public void ReportOverall(int value)
    {
        lock (gate) { percent = Math.Clamp(value, 0, 100); Emit(true); }
    }
    void Emit(bool force)
    {
        long now = Stopwatch.GetTimestamp();
        if (!force && now - lastReport < Stopwatch.Frequency / 4) return;
        lastReport = now;
        report(percent, bytes.DownloadedBytes, bytes.TotalBytes);
    }
    protected override async Task DownloadToStreamInternal(HttpClient client, string requestUri, Stream destination,
        Action<int>? progress = null, CancellationToken cancelToken = default)
    {
        string name = destination is FileStream file ? Path.GetFileName(file.Name) : "";
        if (name.EndsWith(".partial", StringComparison.Ordinal)) name = name[..^8];
        lock (gate) {
            if (name == full.Key && mode != "full") {
                mode = "full"; percent = 0; bytes.Reset(new[] { full });
            } else if (deltas.ContainsKey(name) && mode == "") {
                mode = "delta"; percent = 0; bytes.Reset(deltas);
            }
            bytes.Report(name, 0); // A transport retry restarts this file, not the other concurrent deltas.
            Emit(true);
        }
        using var observed = new ProgressWriteStream(destination, count => {
            lock (gate) { bytes.Report(name, count); Emit(false); }
        });
        await base.DownloadToStreamInternal(client, requestUri, observed, progress, cancelToken).ConfigureAwait(false);
        lock (gate) { Emit(true); }
    }
}
