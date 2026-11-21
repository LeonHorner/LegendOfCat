// Copy into Assets/Game/Scripts in an isolated checkout to time a Linux development player.
#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public sealed class CaveRenderTiming : MonoBehaviour
{
    string output;
    int height;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void StartRequestedReview()
    {
        var args = Environment.GetCommandLineArgs();
        int index = Array.IndexOf(args, "--cave-render-timing");
        if (index < 0) return;
        if (index + 2 >= args.Length || !int.TryParse(args[index + 2], out int height) || (height != 720 && height != 1080)) {
            UnityEngine.Debug.LogError("Expected --cave-render-timing OUTPUT_DIRECTORY 720|1080");
            Application.Quit(1);
            return;
        }
        var review = new GameObject("Cave render timing").AddComponent<CaveRenderTiming>();
        review.output = args[index + 1];
        review.height = height;
    }

    IEnumerator Start()
    {
        MovementCourse course = null;
        for (int frame = 0; frame < 600 && !course; frame++) {
            yield return null;
            course = FindFirstObjectByType<MovementCourse>();
        }
        if (!course || SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) {
            UnityEngine.Debug.LogError("Cave timing requires an active level and a graphics device.");
            Application.Quit(1);
            yield break;
        }

        Directory.CreateDirectory(output);
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = -1;
        var follow = course.GetComponentInChildren<CatCamera>();
        var camera = follow.GetComponent<Camera>();
        var scenery = course.GetComponentInChildren<SceneryRenderer>();
        var cave = course.GetComponentInChildren<CaveEntryStudy>();
        int width = height * 16 / 9;
        var target = new RenderTexture(width, height, 24);
        target.Create();
        var pixel = new Texture2D(1, 1, TextureFormat.RGB24, false);
        var timer = new Stopwatch();
        var names = new[] { "surface", "entrance", "fountain", "falls", "spillway", "exit" };
        var surface = MovementCourse.AscentPlatforms[4];
        var points = new[] {
            new Vector2(surface.x, surface.y + 1.22f), CaveRoute.Entrance,
            CaveRoute.ToCourse(new Vector2(48, -12.16f)), CaveRoute.ToCourse(new Vector2(98.7f, -19.5f)),
            CaveRoute.ToCourse(new Vector2(135, -15.58f)), CaveRoute.ToCourse(new Vector2(208, -5.45f))
        };
        var results = new StringBuilder("round,view,frame,ms\n");
        try {
            for (int round = 0; round < 3; round++) {
                for (int view = 0; view < points.Length; view++) {
                    int index = round % 2 == 0 ? view : points.Length - 1 - view;
                    course.Cat.RespawnAt(points[index]);
                    for (int step = 0; step < 12; step++) yield return new WaitForFixedUpdate();
                    follow.Snap();
                    for (int step = 0; step < 90; step++) follow.Follow(.02f);
                    scenery.ApplyView(camera.transform.position.x, camera.transform.position.y);
                    scenery.SetWaterTime(12);
                    camera.targetTexture = target;
                    camera.rect = new Rect(0, 0, 1, 1);
                    for (int frame = -12; frame < 45; frame++) {
                        cave.AdvanceVisuals(.016f);
                        timer.Restart();
                        camera.Render();
                        RenderTexture.active = target;
                        // A one-pixel read waits for this camera draw to finish on the GPU.
                        pixel.ReadPixels(new Rect(0, 0, 1, 1), 0, 0, false);
                        timer.Stop();
                        if (frame >= 0) results.AppendFormat(CultureInfo.InvariantCulture,
                            "{0},{1},{2},{3:F4}\n", round, names[index], frame, timer.Elapsed.TotalMilliseconds);
                    }
                    if (round == 0) {
                        var image = new Texture2D(width, height, TextureFormat.RGB24, false);
                        image.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
                        File.WriteAllBytes(Path.Combine(output, names[index] + ".png"), image.EncodeToPNG());
                        Destroy(image);
                    }
                    camera.targetTexture = null;
                    RenderTexture.active = null;
                }
            }
            File.WriteAllText(Path.Combine(output, "render.csv"), results.ToString());
            UnityEngine.Debug.Log("Cave render timing complete on " + SystemInfo.graphicsDeviceName + " using " + SystemInfo.graphicsDeviceType);
        } finally {
            camera.targetTexture = null;
            RenderTexture.active = null;
            target.Release();
            Destroy(target);
            Destroy(pixel);
        }
        Application.Quit();
    }
}
#endif
