using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class StompReview
{
    // Uses the actual runtime arena, shared character and state-driven presentation.
    public static void Record()
    {
        SessionState.SetBool("StompReview", true);
        Install();
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
        if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool("StompReview", false)) return;
        SessionState.SetBool("StompReview", false);
        int result = 0;
        try { Capture(); }
        catch (System.Exception error) { Debug.LogException(error); result = 1; }
        if (Application.isBatchMode) EditorApplication.Exit(result);
        else EditorApplication.ExitPlaymode();
    }
    static void Capture()
    {
        var scene = SceneManager.CreateScene("Stomp review");
        SceneManager.SetActiveScene(scene);
        var root = new GameObject("Stomp review arena");
        var arena = root.AddComponent<StompArena>();

        var target = new RenderTexture(960, 540, 24);
        var pixels = new Texture2D(960, 540, TextureFormat.RGB24, false);
        var previous = RenderTexture.active;
        Directory.CreateDirectory("docs/design/stomp-prototype");
        try {
            arena.ArenaCamera.targetTexture = target;
            foreach (var phase in new[] { "anticipation", "attack", "recovery" }) {
                root.SendMessage("Present");
                arena.ArenaCamera.Render();
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, 960, 540), 0, 0); pixels.Apply();
                File.WriteAllBytes("docs/design/stomp-prototype/" + phase + ".png", pixels.EncodeToPNG());
                arena.Combat.Advance(phase == "anticipation" ? StompCombat.Anticipation : StompCombat.Active);
            }
        } finally {
            RenderTexture.active = previous;
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(pixels); Object.DestroyImmediate(target);
        }
        AssetDatabase.SaveAssets();
    }
}
