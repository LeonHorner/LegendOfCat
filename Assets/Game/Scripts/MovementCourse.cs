using UnityEngine;

// A self-contained greybox: reusable from any menu, with no campaign/save dependencies.
public sealed class MovementCourse : MonoBehaviour
{
    public static readonly Vector3[] Platforms = {
        new Vector3(0, -0.75f, 10), new Vector3(9, -0.75f, 4),
        new Vector3(15, 0.25f, 4), new Vector3(22.5f, 0.25f, 3),
        new Vector3(28.5f, 1.25f, 3), new Vector3(34, 0.25f, 4),
        new Vector3(42, -0.75f, 10)
    };
    public CatController Cat { get; private set; }
    public System.Action BackToMenu;
    Sprite square;
    Texture2D texture;
    Camera courseCamera;
    Transform visual;
    Transform[] feet = new Transform[4];
    float stride, facing = 1;
    string previousPose;
    float landing;
    bool originalMika;

    void Awake()
    {
        texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white); texture.Apply();
        square = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1);
        foreach (var p in Platforms) {
            var platform = Block("Standable platform", new Vector2(p.x, p.y), new Vector2(p.z, 1.5f), new Color(0.22f, 0.28f, 0.28f));
            platform.AddComponent<BoxCollider2D>();
            Block("Continuous landing edge", new Vector2(p.x, p.y + 0.72f), new Vector2(p.z, 0.06f), new Color(0.78f, 0.8f, 0.66f));
        }
        // Low-contrast background shapes never intersect or bridge playable edges.
        for (int i = 0; i < 10; i++) {
            var hill = Block("Background silhouette", new Vector2(i * 7 - 7, -2), new Vector2(5, 8 + i % 3), new Color(0.13f, 0.18f, 0.23f));
            hill.GetComponent<SpriteRenderer>().sortingOrder = -10;
        }
        var player = new GameObject("Mika prototype");
        player.transform.SetParent(transform, false);
        player.transform.localPosition = new Vector3(-3, 0.48f);
        player.AddComponent<Rigidbody2D>();
        player.AddComponent<BoxCollider2D>().size = new Vector2(0.9f, 0.9f);
        Cat = player.AddComponent<CatController>();
        var input = player.AddComponent<CatInput>();
        input.controller = Cat;
        input.BackToMenu = () => BackToMenu?.Invoke();
        visual = new GameObject("Temporary quadruped silhouette").transform;
        visual.SetParent(player.transform, false);
        var original = Resources.Load<GameObject>("MikaOriginal");
        if (original) {
            Destroy(visual.gameObject);
            visual = Instantiate(original, player.transform).transform;
            visual.GetComponent<MikaSpritePresentation>().controller = Cat;
            originalMika = true;
        }
        if (!originalMika) {
        Part("Body", new Vector2(-0.04f, 0.03f), new Vector2(0.94f, 0.4f), new Color(0.95f, 0.53f, 0.2f));
        Part("Chest", new Vector2(0.3f, 0), new Vector2(0.22f, 0.38f), new Color(1, 0.86f, 0.62f));
        Part("Head", new Vector2(0.4f, 0.25f), new Vector2(0.35f, 0.32f), new Color(0.95f, 0.53f, 0.2f));
        Part("Ear", new Vector2(0.32f, 0.46f), new Vector2(0.13f, 0.18f), new Color(0.95f, 0.53f, 0.2f));
        Part("Eye", new Vector2(0.5f, 0.29f), new Vector2(0.045f, 0.055f), new Color(0.1f, 0.12f, 0.12f));
        var tail = Part("Tail", new Vector2(-0.65f, 0.18f), new Vector2(0.45f, 0.12f), new Color(0.8f, 0.36f, 0.12f));
        tail.localRotation = Quaternion.Euler(0, 0, -30);
        for (int i = 0; i < 4; i++) feet[i] = Part("Leg", new Vector2(-0.32f + i * 0.2f, -0.27f), new Vector2(0.1f, 0.32f), new Color(1, 0.79f, 0.5f));
        }
        courseCamera = new GameObject("Course camera").AddComponent<Camera>();
        courseCamera.transform.SetParent(transform, false);
        courseCamera.orthographic = true;
        courseCamera.orthographicSize = 5;
        courseCamera.clearFlags = CameraClearFlags.SolidColor;
        courseCamera.backgroundColor = new Color(0.09f, 0.13f, 0.18f);
        courseCamera.depth = 1;
        // A lower camera clears the whole surface; the gameplay camera only clears its viewport.
        var bars = new GameObject("Viewport bars").AddComponent<Camera>();
        bars.transform.SetParent(transform, false);
        bars.depth = 0.5f;
        bars.clearFlags = CameraClearFlags.SolidColor;
        bars.backgroundColor = Color.black;
        bars.cullingMask = 0;
        var follow = courseCamera.gameObject.AddComponent<CatCamera>();
        follow.target = player.transform;
        Cat.ReturnedToStart += follow.Snap;
        Label("EASY / 2 m gap", new Vector2(4, -2));
        Label("RISE / 2 m + 1 m up", new Vector2(11, -2));
        Label("PRECISION / 4 m gap", new Vector2(19, -2));
        Label("RISE / 3 m + 1 m up", new Vector2(25, -2));
        Label("FINISH / return or retry", new Vector2(42, 1));
    }
    GameObject Block(string name, Vector2 position, Vector2 size, Color color)
    {
        var item = new GameObject(name);
        item.transform.SetParent(transform, false);
        item.transform.localPosition = position;
        item.transform.localScale = size;
        var renderer = item.AddComponent<SpriteRenderer>();
        renderer.sprite = square; renderer.color = color;
        return item;
    }
    Transform Part(string name, Vector2 position, Vector2 size, Color color)
    {
        var part = Block(name, Vector2.zero, size, color).transform;
        part.SetParent(visual, false); part.localPosition = position;
        part.GetComponent<SpriteRenderer>().sortingOrder = 3;
        return part;
    }
    void Label(string text, Vector2 position)
    {
        var label = new GameObject(text).AddComponent<TextMesh>();
        label.transform.SetParent(transform, false); label.transform.localPosition = position;
        label.text = text; label.characterSize = 0.13f; label.fontSize = 32;
        label.anchor = TextAnchor.MiddleCenter; label.color = new Color(0.85f, 0.87f, 0.85f);
    }
    void LateUpdate()
    {
        if (courseCamera != null) {
            courseCamera.rect = MenuLayout.Letterbox16By9(Screen.width, Screen.height);
            courseCamera.aspect = 16f / 9f;
        }
        var velocity = Cat.Motion.Velocity;
        if (originalMika) return;
        if (Mathf.Abs(velocity.x) > 0.1f) facing = Mathf.Sign(velocity.x);
        visual.localScale = new Vector3(facing, 1, 1);
        stride += Mathf.Abs(velocity.x) * Time.deltaTime * 2;
        if (previousPose == "Fall" && Cat.Grounded) landing = 0.1f;
        landing = Mathf.Max(0, landing - Time.deltaTime);
        for (int i = 0; i < feet.Length; i++) {
            float angle = Cat.Grounded ? Mathf.Sin(stride * 6 + i * Mathf.PI) * Mathf.Min(Mathf.Abs(velocity.x) * 5, 28)
                : (velocity.y > 0 ? -25 : 20);
            feet[i].localRotation = Quaternion.Euler(0, 0, angle);
        }
        previousPose = Cat.Pose;
    }
    void OnGUI()
    {
        float width = Mathf.Min(680, Screen.width - 24);
        GUILayout.BeginArea(new Rect(12, 12, width, 160), GUI.skin.box);
        GUILayout.Label("MIKA / MOVEMENT LAB PROTOTYPE — temporary art");
        GUILayout.Label("A/D or arrows: run | Space/W/Up: jump (hold for height) | R: retry | Esc: menu");
        GUILayout.Label("Controller: stick/D-pad, South: jump, Select: retry, Start: menu");
        GUILayout.Label((landing > 0 ? "Landing" : Cat.Pose) + " | Clear the gaps; falls return to the start.");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Retry course", GUILayout.Height(28))) Cat.ReturnToStart();
        if (GUILayout.Button("Back to menu", GUILayout.Height(28))) BackToMenu?.Invoke();
        GUILayout.EndHorizontal(); GUILayout.EndArea();
    }
    void OnDestroy() { Destroy(square); Destroy(texture); }
}
