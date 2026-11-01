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
        const string output = "docs/design/golden-river-main-route/variety-evidence";
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
        var points = new[] { new Vector2(49, 1.52f), new Vector2(84, .52f), new Vector2(211, 1.52f), new Vector2(342, .27f) };
        var names = new[] { "suspended-steps", "watermill", "aqueduct", "willow" };
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
