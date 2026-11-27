using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

// Records real frame times during play when the game starts with --frame-log <folder>.
// Each frame keeps Mika's position, so cave and surface pacing can be compared afterwards.
public sealed class FrameTimeLog : MonoBehaviour
{
    public Transform Cat;
    public System.Func<Vector2, bool> InCave;
    readonly List<float> milliseconds = new List<float>();
    readonly List<Vector2> places = new List<Vector2>();
    string folder;
    bool saved;

    public static string RequestedFolder()
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i + 1 < args.Length; i++) if (args[i] == "--frame-log") return args[i + 1];
        return null;
    }
    void Awake() { folder = RequestedFolder(); enabled = folder != null; }
    void Update()
    {
        if (!Cat || Time.timeScale == 0) return;
        milliseconds.Add(Time.unscaledDeltaTime * 1000);
        places.Add(Cat.localPosition);
    }
    void OnApplicationQuit() => Save();
    void OnDestroy() => Save();

    void Save()
    {
        if (saved || folder == null || milliseconds.Count == 0) return;
        saved = true;
        Directory.CreateDirectory(folder);
        var csv = new StringBuilder("frame,milliseconds,x,y,cave\n");
        var cave = new List<float>(); var surface = new List<float>();
        for (int i = 0; i < milliseconds.Count; i++) {
            bool inside = InCave != null && InCave(places[i]);
            (inside ? cave : surface).Add(milliseconds[i]);
            csv.Append(i).Append(',').Append(milliseconds[i].ToString("F3", System.Globalization.CultureInfo.InvariantCulture))
                .Append(',').Append(places[i].x.ToString("F2", System.Globalization.CultureInfo.InvariantCulture))
                .Append(',').Append(places[i].y.ToString("F2", System.Globalization.CultureInfo.InvariantCulture))
                .Append(',').Append(inside ? 1 : 0).Append('\n');
        }
        File.WriteAllText(Path.Combine(folder, "frame-times.csv"), csv.ToString());
        string summary = Summary("Cave", cave) + Summary("Surface", surface);
        File.WriteAllText(Path.Combine(folder, "frame-summary.txt"), summary);
        Debug.Log("Frame time log written to " + folder + "\n" + summary);
    }
    // Hitches are frames over 25 ms (a visible stutter at 60 Hz) and over 34 ms (a missed 30 Hz frame).
    public static string Summary(string label, List<float> values)
    {
        if (values.Count == 0) return label + ": no frames\n";
        var sorted = new List<float>(values); sorted.Sort();
        float Percentile(float p) => sorted[Mathf.Clamp(Mathf.CeilToInt(p * sorted.Count) - 1, 0, sorted.Count - 1)];
        int over25 = 0, over34 = 0;
        foreach (float v in values) { if (v > 25) over25++; if (v > 34) over34++; }
        var c = System.Globalization.CultureInfo.InvariantCulture;
        return string.Format(c, "{0}: {1} frames, median {2:F2} ms, 95th {3:F2} ms, 99th {4:F2} ms, max {5:F2} ms, over 25 ms {6}, over 34 ms {7}\n",
            label, values.Count, Percentile(.5f), Percentile(.95f), Percentile(.99f), sorted[sorted.Count - 1], over25, over34);
    }
}
