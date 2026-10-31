using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

// Synchronous render + readback comparison, not an FPS or physical input-latency measurement.
public static class SceneryPerformanceReview
{
    public static void MeasureWater()
    {
        var scene = EditorSceneManager.NewPreviewScene();
        var root = new GameObject("Water render comparison");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, scene);
        var study = root.AddComponent<BackgroundStudy>();
        study.Initialize(); study.showControls = false; study.View.scene = scene;
        study.ApplyView(17, 1); study.SetWaterTime(0.4f);
        var filters = Array.FindAll(root.GetComponentsInChildren<MeshFilter>(), f => f.name.StartsWith("Foreground water tile"));
        var cropped = filters[0].sharedMesh;
        var full = new Mesh { name = "Previous full water quad" };
        full.vertices = new[] { new Vector3(-0.5f, -0.5f), new Vector3(0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f), new Vector3(0.5f, 0.5f) };
        full.uv = new[] { Vector2.zero, Vector2.right, Vector2.up, Vector2.one };
        full.triangles = new[] { 0, 2, 1, 1, 2, 3 }; full.RecalculateBounds();
        var target = new RenderTexture(1280, 720, 24);
        var pixels = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        var previous = RenderTexture.active;
        study.View.targetTexture = target; study.View.rect = new Rect(0, 0, 1, 1);
        var before = new double[12]; var after = new double[12];
        Color32[] original = null, optimized = null;
        try {
            // Alternate order to avoid assigning all warm-up/clock changes to one shader.
            for (int sample = -4; sample < 12; sample++) {
                for (int pass = 0; pass < 2; pass++) {
                    bool old = ((sample + pass) & 1) == 0;
                    foreach (var filter in filters) filter.sharedMesh = old ? full : cropped;
                    study.SetWaterTime(0.4f);
                    var watch = Stopwatch.StartNew();
                    study.View.Render(); RenderTexture.active = target;
                    pixels.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); pixels.Apply();
                    watch.Stop();
                    if (sample >= 0) (old ? before : after)[sample] = watch.Elapsed.TotalMilliseconds;
                    if (sample == 11) {
                        if (old) original = pixels.GetPixels32(); else optimized = pixels.GetPixels32();
                    }
                }
            }
            int maxDifference = 0;
            long totalDifference = 0;
            for (int i = 0; i < original.Length; i++) {
                int delta = Math.Abs(original[i].r - optimized[i].r)
                    + Math.Abs(original[i].g - optimized[i].g) + Math.Abs(original[i].b - optimized[i].b);
                maxDifference = Math.Max(maxDifference, delta); totalDifference += delta;
            }
            Array.Sort(before); Array.Sort(after);
            var result = new Result {
                baselineMedianMs = (before[5] + before[6]) * 0.5,
                optimizedMedianMs = (after[5] + after[6]) * 0.5,
                maxRgbSumDifference = maxDifference,
                meanChannelDifference = totalDifference / (original.Length * 3.0)
            };
            Directory.CreateDirectory("Logs/scenery-performance");
            File.WriteAllText("Logs/scenery-performance/render-comparison.json", JsonUtility.ToJson(result, true));
            UnityEngine.Debug.Log("Water render comparison completed; synchronous readback timings are not gameplay FPS.");
        } finally {
            foreach (var filter in filters) filter.sharedMesh = cropped;
            Object.DestroyImmediate(full);
            study.View.targetTexture = null; RenderTexture.active = previous;
            target.Release(); Object.DestroyImmediate(target); Object.DestroyImmediate(pixels);
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }

    [Serializable]
    sealed class Result
    {
        public double baselineMedianMs, optimizedMedianMs, meanChannelDifference;
        public int maxRgbSumDifference;
    }
}
