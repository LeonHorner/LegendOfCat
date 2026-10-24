using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// Reproducible visual evidence using the same course/prefab as the actual menu.
public static class MikaMovementReview
{
    [MenuItem("LegendOfCat/Mika/Record integrated movement review")]
    public static void Record()
    {
        SessionState.SetBool("MikaIntegratedReview", true);
        Install();
        if (EditorApplication.isPlaying) StateChanged(PlayModeStateChange.EnteredPlayMode);
        else EditorApplication.EnterPlaymode();
    }

    [InitializeOnLoadMethod]
    static void Install()
    {
        EditorApplication.playModeStateChanged -= StateChanged;
        EditorApplication.playModeStateChanged += StateChanged;
    }

    static void StateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool("MikaIntegratedReview", false)) return;
        SessionState.SetBool("MikaIntegratedReview", false);
        int exitCode = 0;
        try { Capture(); }
        catch (System.Exception error) { Debug.LogException(error); exitCode = 1; }
        if (Application.isBatchMode) EditorApplication.Exit(exitCode);
        else EditorApplication.ExitPlaymode();
    }

    static void Capture()
    {
        const string output = "Logs/mika-integrated-review";
        System.IO.Directory.CreateDirectory(output);
        var previous = SceneManager.GetActiveScene();
        var scene = SceneManager.CreateScene("Integrated Mika review", new CreateSceneParameters(LocalPhysicsMode.Physics2D));
        SceneManager.SetActiveScene(scene);
        var course = Object.Instantiate(Resources.Load<GameObject>("MovementCourse"));
        var cat = course.GetComponent<MovementCourse>().Cat;
        cat.GetComponent<CatInput>().enabled = false;
        cat.GetComponent<Rigidbody2D>().interpolation = RigidbodyInterpolation2D.None;
        var presentation = cat.GetComponentInChildren<MikaSpritePresentation>();
        var camera = System.Array.Find(course.GetComponentsInChildren<Camera>(), c => c.name == "Course camera");
        var target = new RenderTexture(960, 540, 24);
        var pixels = new Texture2D(960, 540, TextureFormat.RGB24, false);
        var oldTarget = RenderTexture.active;
        float oldFixed = Time.fixedDeltaTime;
        var csv = new System.Text.StringBuilder("frame,phase,grounded,sprite,root_x,root_y,visual_y\n");
        try {
            Time.fixedDeltaTime = 1f / 60;
            camera.targetTexture = target;
            for (int frame = 0; frame < 360; frame++) {
                // Idle, a four-pose run, reversal, a full jump/landing, then retry.
                cat.Motion.SetMove(frame >= 30 && frame < 70 ? 1 : frame >= 70 && frame < 100 ? -1 : 0);
                if (frame == 130) cat.Motion.PressJump();
                if (frame == 185) cat.Motion.ReleaseJump();
                if (frame == 270) cat.ReturnToStart();
                cat.SendMessage("FixedUpdate");
                scene.GetPhysicsScene2D().Simulate(Time.fixedDeltaTime);
                presentation.RenderPose(Time.fixedDeltaTime);
                camera.transform.position = new Vector3(cat.transform.position.x,
                    2f + Mathf.Max(0, cat.transform.position.y - 0.46f) * 0.7f, -10);
                camera.aspect = 16f / 9;
                camera.rect = new Rect(0, 0, 1, 1);
                camera.Render();
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, 960, 540), 0, 0); pixels.Apply();
                System.IO.File.WriteAllBytes($"{output}/frame-{frame:0000}.png", pixels.EncodeToPNG());
                csv.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,
                    "{0},{1},{2},{3},{4:F4},{5:F4},{6:F4}\n", frame, presentation.Animation.Current,
                    cat.Grounded, presentation.Animation.Frame, cat.transform.position.x,
                    cat.transform.position.y, presentation.transform.position.y);
            }
            System.IO.File.WriteAllText($"{output}/playback.csv", csv.ToString());
            Debug.Log("Recorded 360 integrated Mika frames at 60 Hz: original course, controller, prefab and sprite presentation.");
        }
        finally {
            Time.fixedDeltaTime = oldFixed;
            camera.targetTexture = null; RenderTexture.active = oldTarget;
            target.Release(); Object.Destroy(target); Object.Destroy(pixels);
            SceneManager.SetActiveScene(previous);
            SceneManager.UnloadSceneAsync(scene);
        }
    }
}
