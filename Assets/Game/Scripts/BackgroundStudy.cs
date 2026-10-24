using UnityEngine;

// Bounded art decision lab; never loaded by the campaign or the release menu.
public sealed class BackgroundStudy : MonoBehaviour
{
    public enum Concept { ThreeLayer, Panorama, Subtle }
    public Concept concept;
    [Range(0, 1)] public float farSpeed = 0.08f;
    [Range(0, 1)] public float middleSpeed = 0.28f;
    [Range(0, 1)] public float nearSpeed = 0.55f;
    public bool tour;
    public bool showControls = true;
    public Camera View { get; private set; }
    public CatController Cat { get; private set; }
    readonly Transform[] layers = new Transform[3];
    readonly Material[] materials = new Material[3];
    Texture2D pixel;
    Sprite square;
    float tourTime;
    Vector3 cameraVelocity;

    // Camera-relative travel, calculated absolutely: reversals and teleports cannot accumulate drift.
    public static float Offset(float cameraX, float speed) => cameraX * (1 - speed);
    public static float TileCenter(float cameraX, float speed, float width) =>
        Mathf.Floor(cameraX * speed / width + 0.5f) * width;

    void Start() => Initialize();
    public void Initialize()
    {
        if (View) return;
        pixel = new Texture2D(1, 1);
        pixel.SetPixel(0, 0, Color.white); pixel.Apply();
        square = Sprite.Create(pixel, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1);
        View = new GameObject("Study camera").AddComponent<Camera>();
        View.transform.SetParent(transform, false);
        View.orthographic = true; View.orthographicSize = 5.5f;
        View.transform.position = new Vector3(6, 3, -10);
        View.clearFlags = CameraClearFlags.SolidColor;
        View.backgroundColor = new Color(0.8f, 0.79f, 0.7f);
        var atlas = Resources.Load<Texture2D>("BackgroundStudy/painted-atlas");
        for (int i = 0; i < 3; i++) {
            layers[i] = new GameObject("Painted depth " + i).transform;
            layers[i].SetParent(transform, false);
            materials[i] = new Material(Shader.Find("LegendOfCat/BackgroundStudy"));
            materials[i].mainTexture = atlas;
            materials[i].SetFloat("_Strip", i);
            materials[i].SetFloat("_Fade", i == 0 ? 0 : 1);
            for (int tile = -1; tile <= 1; tile++) {
                var panel = GameObject.CreatePrimitive(PrimitiveType.Quad);
                DestroyImmediate(panel.GetComponent<Collider>());
                panel.name = "Mirrored scenery tile " + tile;
                panel.transform.SetParent(layers[i], false);
                panel.transform.localPosition = new Vector3(tile * 64, i == 0 ? 4 : i == 1 ? 1.5f : -0.5f, 20 - i * 2);
                panel.transform.localScale = new Vector3(64, i == 0 ? 15 : i == 1 ? 10 : 8, 1);
                panel.GetComponent<MeshRenderer>().sharedMaterial = materials[i];
            }
        }
        // Reuse the approved motor and presentation, but own every object in this lab.
        foreach (var p in MovementCourse.Platforms) {
            var ground = Block("World-space ground", new Vector2(p.x, p.y), new Vector2(p.z, 1.5f), new Color(0.19f, 0.23f, 0.2f));
            ground.AddComponent<BoxCollider2D>();
            Block("Crisp landing edge", new Vector2(p.x, p.y + 0.72f), new Vector2(p.z, 0.06f), new Color(0.94f, 0.8f, 0.48f));
        }
        var player = new GameObject("Mika existing prototype");
        player.transform.SetParent(transform, false); player.transform.position = new Vector3(-3, 0.48f);
        player.AddComponent<Rigidbody2D>();
        player.AddComponent<BoxCollider2D>().size = new Vector2(0.9f, 0.9f);
        Cat = player.AddComponent<CatController>();
        var input = player.AddComponent<CatInput>(); input.controller = Cat;
        input.RetryCheckpoint = () => Cat.ReturnToStart();
        input.RestartRun = () => Cat.ReturnToStart();
        var original = Resources.Load<GameObject>("MikaOriginal");
        if (original) Instantiate(original, player.transform).GetComponent<MikaSpritePresentation>().controller = Cat;
        else Debug.LogError("Background study requires the existing MikaOriginal prefab.");
        ApplyView(6, 3);
    }

    GameObject Block(string label, Vector2 position, Vector2 size, Color color)
    {
        var item = new GameObject(label); item.transform.SetParent(transform, false);
        item.transform.position = position; item.transform.localScale = size;
        var renderer = item.AddComponent<SpriteRenderer>(); renderer.sprite = square; renderer.color = color;
        renderer.sortingOrder = 1; return item;
    }

    public void ApplyView(float x, float y)
    {
        View.transform.position = new Vector3(x, y, -10);
        float[] speeds = concept == Concept.Panorama ? new[] { 0.08f, 0.08f, 0.08f }
            : concept == Concept.Subtle ? new[] { 0.05f, 0.14f, 0.24f } : new[] { farSpeed, middleSpeed, nearSpeed };
        for (int i = 0; i < 3; i++) {
            float center = TileCenter(x, speeds[i], 64);
            layers[i].position = new Vector3(Offset(x, speeds[i]) + center, y * (1 - speeds[i]), 0);
            int tileIndex = Mathf.RoundToInt(center / 64);
            for (int j = 0; j < 3; j++) {
                var panel = layers[i].GetChild(j);
                var scale = panel.localScale;
                scale.x = ((tileIndex + j - 1) & 1) == 0 ? 64 : -64;
                panel.localScale = scale;
            }
        }
    }

    void LateUpdate()
    {
        if (!View) return;
        if (tour) {
            tourTime += Time.deltaTime;
            ApplyView(Mathf.PingPong(tourTime * 5, 48) - 2, 3);
        } else {
            float half = View.orthographicSize * View.aspect;
            float x = Mathf.Clamp(Cat.transform.position.x + 2, half - 5, 47 - half);
            var desired = new Vector3(x, Mathf.Clamp(Cat.transform.position.y + 1.2f, 3, 5), -10);
            var position = Vector3.SmoothDamp(View.transform.position, desired, ref cameraVelocity, 0.16f);
            ApplyView(position.x, position.y);
        }
    }

    void OnGUI()
    {
        if (!showControls) return;
        GUILayout.BeginArea(new Rect(10, 10, Mathf.Min(Screen.width - 20, 620), 215), GUI.skin.box);
        GUILayout.Label("GOLDEN RIVER / BACKGROUND DECISION LAB");
        GUILayout.Label("A/D: move | Space: jump | R: retry. Scenery has no collision.");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("A: Three layers")) concept = Concept.ThreeLayer;
        if (GUILayout.Button("B: Panorama")) concept = Concept.Panorama;
        if (GUILayout.Button("C: Subtle layers")) concept = Concept.Subtle;
        GUILayout.EndHorizontal();
        tour = GUILayout.Toggle(tour, "Camera-only tour (includes reversal; return to play to follow Mika)");
        GUILayout.Label("Camera travel factors (0 = camera-locked, 1 = world-space)");
        farSpeed = Slider("Far", farSpeed); middleSpeed = Slider("Middle", middleSpeed); nearSpeed = Slider("Near", nearSpeed);
        GUILayout.Label("Candidate: " + concept + " | Prototype only; production choice pending.");
        GUILayout.EndArea();
    }
    static float Slider(string label, float value)
    {
        GUILayout.BeginHorizontal(); GUILayout.Label(label + " " + value.ToString("F2"), GUILayout.Width(95));
        value = GUILayout.HorizontalSlider(value, 0, 1); GUILayout.EndHorizontal(); return value;
    }
    void OnDestroy()
    {
        foreach (var material in materials) if (material) DestroyImmediate(material);
        if (square) DestroyImmediate(square); if (pixel) DestroyImmediate(pixel);
    }
}
