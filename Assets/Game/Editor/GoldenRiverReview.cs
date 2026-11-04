using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// Static visual checks use the actual runtime course and imported paintings.
public static class GoldenRiverReview
{
    [MenuItem("LegendOfCat/Golden River/Capture variety review")]
    public static void Record()
    {
        BuildScenery.ImportGoldenRiverArt();
        SessionState.SetBool("GoldenRiverReview", true);
        EditorApplication.EnterPlaymode();
    }

    [InitializeOnLoadMethod]
    static void Install()
    {
        EditorApplication.playModeStateChanged -= StateChanged;
        EditorApplication.playModeStateChanged += StateChanged;
    }

    static void StateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool("GoldenRiverReview", false)) return;
        SessionState.SetBool("GoldenRiverReview", false);
        int result = 0;
        try { Capture(); }
        catch (System.Exception error) { Debug.LogException(error); result = 1; }
        if (Application.isBatchMode) EditorApplication.Exit(result);
        else EditorApplication.ExitPlaymode();
    }

    static void Capture()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        int outputIndex = System.Array.IndexOf(args, "-reviewOutput");
        string output = outputIndex >= 0 && outputIndex + 1 < args.Length
            ? args[outputIndex + 1]
            : "docs/design/golden-river-main-route/variety-evidence";
        Directory.CreateDirectory(output);
        var previousScene = SceneManager.GetActiveScene();
        var scene = SceneManager.CreateScene("Golden River visual review", new CreateSceneParameters(LocalPhysicsMode.Physics2D));
        SceneManager.SetActiveScene(scene);
        var root = new GameObject("Golden River review course");
        root.SetActive(false);
        var course = root.AddComponent<MovementCourse>();
        course.LevelOne = true;
        root.SetActive(true);
        var cat = course.Cat;
        cat.GetComponent<CatInput>().enabled = false;
        cat.GetComponent<Rigidbody2D>().interpolation = RigidbodyInterpolation2D.None;
        var follow = root.GetComponentInChildren<CatCamera>();
        var camera = follow.GetComponent<Camera>();
        var scenery = root.GetComponentInChildren<SceneryRenderer>();
        var presentation = cat.GetComponentInChildren<MikaSpritePresentation>();
        var previousTarget = RenderTexture.active;
        var points = new[] { new Vector2(49, 1.52f), new Vector2(84, .52f), new Vector2(211, .27f), new Vector2(342, .27f), new Vector2(121, 1.52f), new Vector2(246, 1.52f), new Vector2(156, 1.52f), new Vector2(100, 1.02f), new Vector2(302.5f, -.48f), new Vector2(202, -.23f), new Vector2(-3, .52f), new Vector2(30, .52f), new Vector2(431, .52f), new Vector2(503, .52f), new Vector2(100, 1.02f), new Vector2(238, 2.27f), new Vector2(440, 1.02f), new Vector2(499, .52f), new Vector2(22, .52f), new Vector2(78, .52f), new Vector2(163, .52f), new Vector2(273, .52f), new Vector2(394, .52f), new Vector2(487, .52f), new Vector2(15.9f, .52f), new Vector2(16.1f, .52f), new Vector2(-5, .52f), new Vector2(507, .52f), new Vector2(55, 1.52f), new Vector2(138, .52f), new Vector2(235, 2.27f), new Vector2(303, -.48f), new Vector2(365, 2.27f), new Vector2(455, 1.52f) };
        var names = new[] { "suspended-steps", "watermill", "aqueduct", "willow", "watermill-edge", "aqueduct-edge", "masonry-sequence", "watermill-base", "low-root-crossing", "low-masonry-crossing", "panorama-start", "first-banks", "panorama-late", "panorama-exit", "watching-fish", "cat-king", "no-cats-sign", "cat-rock", "river-rock-0", "river-rock-1", "river-rock-2", "river-rock-3", "river-rock-4", "river-rock-5", "water-boundary-before", "water-boundary-after", "route-start-edge", "route-exit-edge", "river-rock-12", "river-rock-13", "river-rock-14", "river-rock-15", "river-rock-16", "river-rock-17" };
        if (System.Array.IndexOf(args, "-ascentReview") >= 0) {
            var ascent = MovementCourse.AscentPlatforms;
            points = new Vector2[6]; names = new string[6];
            for (int i = 0; i < 6; i++) {
                var p = ascent[new[] { 0, 10, 31, 45, 51, 75 }[i]];
                points[i] = new Vector2(p.x, p.y + 1.22f); names[i] = "ascent-" + i;
            }
        }
        try {
            for (int index = 0; index < points.Length; index++) {
                cat.RespawnAt(points[index]);
                Physics2D.SyncTransforms();
                for (int step = 0; step < 20; step++) {
                    cat.SendMessage("FixedUpdate");
                    scene.GetPhysicsScene2D().Simulate(Time.fixedDeltaTime);
                }
                cat.transform.position = cat.GetComponent<Rigidbody2D>().position;
                presentation.RenderPose(0);
                follow.Snap();
                follow.SendMessage("LateUpdate");
                scenery.ApplyView(camera.transform.position.x, camera.transform.position.y);
                scenery.SetWaterTime(12);
                foreach (int width in new[] { 960, 640 }) {
                    int height = width * 9 / 16;
                    var target = new RenderTexture(width, height, 24);
                    var pixels = new Texture2D(width, height, TextureFormat.RGB24, false);
                    try {
                        camera.targetTexture = target;
                        camera.rect = new Rect(0, 0, 1, 1);
                        camera.Render();
                        RenderTexture.active = target;
                        pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                        pixels.Apply();
                        File.WriteAllBytes($"{output}/{names[index]}-{width}.png", pixels.EncodeToPNG());
                        if (width == 640 && names[index] == "watching-fish") {
                            const string frames = "Logs/watching-fish-frames";
                            Directory.CreateDirectory(frames);
                            camera.orthographicSize = 2.25f;
                            scenery.ApplyView(96, -3.05f);
                            for (int frame = 0; frame < 60; frame++) {
                                scenery.SetWaterTime(frame / 12f);
                                camera.Render();
                                RenderTexture.active = target;
                                pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0); pixels.Apply();
                                File.WriteAllBytes($"{frames}/frame-{frame:000}.png", pixels.EncodeToPNG());
                            }
                            camera.orthographicSize = 5.5f;
                            follow.Snap(); follow.SendMessage("LateUpdate");
                            scenery.ApplyView(camera.transform.position.x, camera.transform.position.y);
                            scenery.SetWaterTime(12);
                            camera.Render(); RenderTexture.active = target;
                            pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0); pixels.Apply();
                        }
                        if (width == 640 && names[index] == "cat-king") {
                            var oldPosition = camera.transform.position;
                            var king = scenery.transform.Find("Easter egg cat king");
                            camera.orthographicSize = 1.6f;
                            camera.transform.position = new Vector3(king.position.x, king.position.y, oldPosition.z);
                            camera.Render(); RenderTexture.active = target;
                            pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0); pixels.Apply();
                            File.WriteAllBytes($"{output}/cat-king-detail.png", pixels.EncodeToPNG());
                            camera.orthographicSize = 5.5f; camera.transform.position = oldPosition;
                            camera.Render(); RenderTexture.active = target;
                            pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0); pixels.Apply();
                        }
                        if (width == 640 && System.Array.IndexOf(args, "-ascentReview") >= 0 && (index == 2 || index == 4)) {
                            var saved = pixels.GetPixels();
                            var movers = root.GetComponentsInChildren<MovingSupport>();
                            foreach (var mover in movers) { mover.SendMessage("Start"); mover.ResetPhase(); }
                            string frames = output + "/motion-" + index;
                            Directory.CreateDirectory(frames);
                            for (int frame = 0; frame < 60; frame++) {
                                for (int tick = 0; tick < 4; tick++) {
                                    foreach (var mover in movers) mover.SendMessage("FixedUpdate");
                                    cat.SendMessage("FixedUpdate");
                                    scene.GetPhysicsScene2D().Simulate(Time.fixedDeltaTime);
                                }
                                presentation.SendMessage("LateUpdate"); follow.SendMessage("LateUpdate");
                                scenery.ApplyView(camera.transform.position.x, camera.transform.position.y);
                                scenery.SetWaterTime(12 + frame / 12.5f);
                                camera.Render(); RenderTexture.active = target;
                                pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0); pixels.Apply();
                                File.WriteAllBytes($"{frames}/frame-{frame:000}.png", pixels.EncodeToPNG());
                            }
                            pixels.SetPixels(saved); pixels.Apply();
                        }
                        if (width == 640) {
                            var colors = pixels.GetPixels();
                            for (int i = 0; i < colors.Length; i++) {
                                float gray = colors[i].grayscale;
                                colors[i] = new Color(gray, gray, gray, 1);
                            }
                            pixels.SetPixels(colors); pixels.Apply();
                            File.WriteAllBytes($"{output}/{names[index]}-{width}-gray.png", pixels.EncodeToPNG());
                        }
                    } finally {
                        camera.targetTexture = null;
                        RenderTexture.active = previousTarget;
                        target.Release(); Object.Destroy(target); Object.Destroy(pixels);
                    }
                }
            }
        } finally {
            RenderTexture.active = previousTarget;
            Object.Destroy(root);
            SceneManager.SetActiveScene(previousScene);
            SceneManager.UnloadSceneAsync(scene);
        }
    }
}
