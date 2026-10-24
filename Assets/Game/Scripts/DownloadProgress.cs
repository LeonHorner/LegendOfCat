using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

// Payload bytes for the currently selected package plan, separate from patch preparation percent.
public sealed class DownloadProgress
{
    readonly Dictionary<string, long> sizes = new Dictionary<string, long>();
    readonly Dictionary<string, long> received = new Dictionary<string, long>();
    public long TotalBytes { get; private set; }
    public long DownloadedBytes { get; private set; }

    public void Reset(IEnumerable<KeyValuePair<string, long>> packages)
    {
        sizes.Clear(); received.Clear(); TotalBytes = DownloadedBytes = 0;
        foreach (var package in packages) {
            if (package.Value <= 0 || sizes.ContainsKey(package.Key)) continue;
            sizes.Add(package.Key, package.Value);
            received.Add(package.Key, 0);
            TotalBytes = checked(TotalBytes + package.Value);
        }
    }
    public void Report(string package, long bytes)
    {
        if (!sizes.TryGetValue(package, out var size)) return;
        bytes = Math.Max(0, Math.Min(bytes, size));
        DownloadedBytes += bytes - received[package];
        received[package] = bytes;
    }
    public static string Format(long downloaded, long total)
    {
        if (total <= 0) return "";
        downloaded = Math.Max(0, Math.Min(downloaded, total));
        // MB uses decimal bytes; one decimal place rounds up so remaining payload is not shown as zero early.
        string Mb(long bytes) => (Math.Ceiling(bytes / 100000d) / 10d).ToString("0.0", CultureInfo.InvariantCulture);
        return Mb(total - downloaded) + " MB remaining / " + Mb(total) + " MB total";
    }
}

// Leave the actual file owned by Velopack; observe completed writes without changing HTTP behavior.
public sealed class ProgressWriteStream : Stream
{
    readonly Stream destination;
    readonly Action<long> report;
    long written;
    public ProgressWriteStream(Stream destination, Action<long> report) { this.destination = destination; this.report = report; }
    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => destination.CanWrite;
    public override long Length => written;
    public override long Position { get => written; set => throw new NotSupportedException(); }
    public override void Flush() => destination.Flush();
    public override void Write(byte[] buffer, int offset, int count)
    { destination.Write(buffer, offset, count); written += count; report(written); }
    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    { await destination.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false); written += count; report(written); }
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
}
