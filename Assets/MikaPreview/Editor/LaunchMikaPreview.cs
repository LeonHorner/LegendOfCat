using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class LaunchMikaPreview
{
    static LaunchMikaPreview()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool("LaunchMikaLevel", false)) return;
            SessionState.EraseBool("LaunchMikaLevel");
            EditorApplication.delayCall += () =>
            {
                var menu = UnityEngine.Object.FindAnyObjectByType<StarterScreen>();
                if (menu)
                    typeof(StarterScreen).GetMethod("OpenLevelOne",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                        ?.Invoke(menu, null);
            };
        };
    }

    [MenuItem("Tools/Play Mika in Level One")]
    public static void Start()
    {
        EditorSceneManager.OpenScene("Assets/Game/Scenes/Main.unity");
        EditorApplication.ExecuteMenuItem("Window/General/Game");
        SessionState.SetBool("LaunchMikaLevel", true);
        EditorApplication.isPlaying = true;
    }
}
