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
    public const float CameraFloor = 1f;
    public const float CourseLeft = -5, CourseRight = 104;
    public const float TourDistance = 100;
    // Keep the shared course untouched; this lab adds a second sequence for longer art review.
    public static readonly Vector3[] Platforms = {
        new Vector3(0, -0.75f, 10), new Vector3(9, -0.75f, 4),
        new Vector3(15, 0.25f, 4), new Vector3(22.5f, 0.25f, 3),
        new Vector3(28.5f, 1.25f, 3), new Vector3(34, 0.25f, 4),
        new Vector3(42, -0.75f, 10), new Vector3(52, -0.75f, 4),
        new Vector3(58.5f, 0.25f, 3), new Vector3(65, 0.25f, 4),
        new Vector3(73, -0.75f, 6), new Vector3(83, -0.75f, 8),
        new Vector3(92, 0.25f, 4), new Vector3(100, -0.75f, 8)
    };
    public static bool IsFloating(int index) => index == 3 || index == 4 || index == 8 || index == 12;
    public CatController Cat { get; private set; }
    readonly Transform[] layers = new Transform[3];
    readonly Material[] materials = new Material[3];
    readonly System.Collections.Generic.List<Mesh> terrainMeshes = new System.Collections.Generic.List<Mesh>();
    Material terrainMaterial, cornerMaterial, riverFrontMaterial;
    Transform riverFront;
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
        View.transform.position = new Vector3(6, CameraFloor, -10);
        View.depth = 1;
        View.clearFlags = CameraClearFlags.SolidColor;
        View.backgroundColor = new Color(0.8f, 0.79f, 0.7f);
        var bars = new GameObject("Study viewport bars").AddComponent<Camera>();
        bars.transform.SetParent(transform, false);
        bars.depth = 0; bars.clearFlags = CameraClearFlags.SolidColor;
        bars.backgroundColor = Color.black; bars.cullingMask = 0;
        var atlas = Resources.Load<Texture2D>("BackgroundStudy/painted-atlas");
        var textures = new[] { atlas, Resources.Load<Texture2D>("BackgroundStudy/connected-river"),
            Resources.Load<Texture2D>("BackgroundStudy/near-cutout") };
        for (int i = 0; i < 3; i++) {
            layers[i] = new GameObject("Painted depth " + i).transform;
            layers[i].SetParent(transform, false);
            materials[i] = new Material(Shader.Find(i == 1 ? "LegendOfCat/StudyRiver" : "LegendOfCat/BackgroundStudy"));
            materials[i].mainTexture = textures[i];
            materials[i].SetFloat("_Cutout", i == 0 ? 0 : 1);
            if (i == 1) materials[i].SetFloat("_Waterline", -3.2f);
            float width = i == 0 ? 64 : 24;
            for (int tile = -1; tile <= 1; tile++) {
                var panel = GameObject.CreatePrimitive(PrimitiveType.Quad);
                DestroyImmediate(panel.GetComponent<Collider>());
                panel.name = "Mirrored scenery tile " + tile;
                panel.transform.SetParent(layers[i], false);
                panel.transform.localPosition = new Vector3(tile * width, i == 0 ? 0 : i == 1 ? -1.5f : -2, 20 - i * 2);
                // Preserve the painting's proportions instead of stretching thin atlas strips.
                panel.transform.localScale = new Vector3(width, i == 0 ? width / 4.5f : width / 1.5f, 1);
                panel.GetComponent<MeshRenderer>().sharedMaterial = materials[i];
            }
        }
        // Register the foreground to the same painting; draw it after near-bank foliage.
        riverFront = new GameObject("Connected river foreground").transform;
        riverFront.SetParent(transform, false);
        riverFrontMaterial = new Material(materials[1]); riverFrontMaterial.SetFloat("_Foreground", 1);
        for (int tile = 0; tile < 3; tile++) {
            var source = layers[1].GetChild(tile);
            var front = GameObject.CreatePrimitive(PrimitiveType.Quad);
            DestroyImmediate(front.GetComponent<Collider>());
            front.name = "Connected river foreground tile " + tile;
            front.transform.SetParent(riverFront, false);
            front.transform.localPosition = new Vector3(source.localPosition.x, source.localPosition.y, -2);
            front.transform.localScale = source.localScale;
            front.GetComponent<MeshRenderer>().sharedMaterial = riverFrontMaterial;
        }
        // Reuse the approved motor and presentation, but own every object in this lab.
        terrainMaterial = new Material(Shader.Find("LegendOfCat/StudyTerrain"));
        terrainMaterial.mainTexture = Resources.Load<Texture2D>("BackgroundStudy/painted-pillar-v2");
        for (int index = 0; index < Platforms.Length; index++) {
            var p = Platforms[index];
            var ground = Block("World-space ground", new Vector2(p.x, p.y), new Vector2(p.z, 1.5f), new Color(0.19f, 0.23f, 0.2f));
            ground.GetComponent<SpriteRenderer>().enabled = false;
            ground.AddComponent<BoxCollider2D>();
            Terrain(p, IsFloating(index), index);
        }
        cornerMaterial = new Material(Shader.Find("LegendOfCat/BackgroundStudy"));
        cornerMaterial.mainTexture = Resources.Load<Texture2D>("BackgroundStudy/foreground-corners");
        cornerMaterial.SetFloat("_Cutout", 1); cornerMaterial.SetFloat("_Fog", 0);
        cornerMaterial.SetFloat("_Corner", 1);
        float halfView = View.orthographicSize * 16 / 9;
        for (int side = 0; side < 2; side++) {
            var corners = GameObject.CreatePrimitive(PrimitiveType.Quad);
            DestroyImmediate(corners.GetComponent<Collider>());
            corners.name = "Foreground corner foliage " + side;
            corners.transform.SetParent(View.transform, false);
            // Each half of the source retains its aspect ratio. Scaling the entire
            // painting leaves natural leaf contours instead of clipping a straight top.
            corners.transform.localPosition = new Vector3((side == 0 ? -1 : 1) * (halfView - 2.25f), -2.5f, 1);
            corners.transform.localScale = new Vector3(4.5f, 6, 1);
            var renderer = corners.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = cornerMaterial; renderer.sortingOrder = 10;
            var parameters = new MaterialPropertyBlock(); parameters.SetFloat("_CornerSide", side);
            renderer.SetPropertyBlock(parameters);
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
        ApplyViewport(Screen.width, Screen.height);
        ApplyView(6, CameraFloor);
    }

    void Terrain(Vector3 platform, bool floating, int variant)
    {
        // Artwork owns its silhouette; the collider owns the exact standable span.
        bool bank = platform.z >= 6;
        float contact = platform.y + 0.75f;
        // Register the load-bearing grass, not the brighter leaves above its edge.
        float contactUV = bank ? 0.587f : 0.748f;
        float landingFraction = bank ? 0.89f : 0.76f;
        float width = platform.z / landingFraction;
        float bottom = floating ? contact - 2.1f : -4.5f;
        float crown = contact + 0.60f;
        float half = width * 0.5f;
        var vertices = new[] {
            new Vector3(-half, bottom, 2), new Vector3(half, bottom, 2),
            new Vector3(-half, contact, 2), new Vector3(half, contact, 2),
            new Vector3(-half, crown, 2), new Vector3(half, crown, 2)
        };
        bool mirror = variant % 3 == 1;
        float left = mirror ? 1 : 0, right = 1 - left;
        var mesh = new Mesh { name = floating ? "Painted floating rock" : "Painted grounded cliff" };
        mesh.vertices = vertices;
        mesh.uv = new[] { new Vector2(left, 0), new Vector2(right, 0),
            new Vector2(left, contactUV), new Vector2(right, contactUV),
            new Vector2(left, 1), new Vector2(right, 1) };
        mesh.triangles = new[] { 0, 2, 1, 1, 2, 3, 2, 4, 3, 3, 4, 5 };
        mesh.RecalculateBounds(); terrainMeshes.Add(mesh);
        var visual = new GameObject(mesh.name); visual.transform.SetParent(transform, false);
        visual.transform.position = new Vector3(platform.x, 0, 0);
        visual.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = visual.AddComponent<MeshRenderer>(); renderer.sharedMaterial = terrainMaterial;
        var parameters = new MaterialPropertyBlock();
        parameters.SetTexture("_MainTex", Resources.Load<Texture2D>(
            bank ? "BackgroundStudy/painted-bank-v2" : "BackgroundStudy/painted-pillar-v2"));
        renderer.SetPropertyBlock(parameters);
        if (!floating && (variant == 0 || variant == 2 || variant == 6 || variant == 10 || variant == 13))
            ShoreFoot(platform, variant == 0 || variant == 10 ? -1 : 1);
    }

    void ShoreFoot(Vector3 platform, int side)
    {
        // A low decorative buttress joins the cliff to its bank below the landing plane.
        var foot = GameObject.CreatePrimitive(PrimitiveType.Quad);
        DestroyImmediate(foot.GetComponent<Collider>());
        foot.name = "Painted shore footing"; foot.transform.SetParent(transform, false);
        foot.transform.position = new Vector3(platform.x + side * (platform.z * 0.5f + 1.25f), -2.65f, 3);
        foot.transform.localScale = new Vector3(4.8f * side, 2.6f, 1);
        var renderer = foot.GetComponent<MeshRenderer>(); renderer.sharedMaterial = terrainMaterial;
        var parameters = new MaterialPropertyBlock();
        parameters.SetTexture("_MainTex", Resources.Load<Texture2D>("BackgroundStudy/painted-shore-v2"));
        renderer.SetPropertyBlock(parameters);
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
            float width = i == 0 ? 64 : 24;
            float center = TileCenter(x, speeds[i], width);
            // Only horizontal parallax is under evaluation. Lock vertical registration
            // to the camera so jump follow cannot reveal the outer panorama boundary.
            layers[i].position = new Vector3(Offset(x, speeds[i]) + center, y, 0);
            int tileIndex = Mathf.RoundToInt(center / width);
            for (int j = 0; j < 3; j++) {
                var panel = layers[i].GetChild(j);
                var scale = panel.localScale;
                scale.x = ((tileIndex + j - 1) & 1) == 0 ? width : -width;
                panel.localScale = scale;
            }
        }
        RegisterRiverForeground();
    }

    void RegisterRiverForeground()
    {
        riverFront.position = layers[1].position;
        for (int tile = 0; tile < 3; tile++)
            riverFront.GetChild(tile).localScale = layers[1].GetChild(tile).localScale;
    }

    public void ApplyViewport(int width, int height)
    {
        View.rect = MenuLayout.Letterbox16By9(width, height);
        View.aspect = 16f / 9f;
    }

    void LateUpdate()
    {
        if (!View) return;
        ApplyViewport(Screen.width, Screen.height);
        if (tour) {
            tourTime += Time.deltaTime;
            ApplyView(Mathf.PingPong(tourTime * 5, TourDistance) - 2, CameraFloor);
        } else {
            float half = View.orthographicSize * View.aspect;
            float x = Mathf.Clamp(Cat.transform.position.x + 2, half + CourseLeft, CourseRight - half);
            // The course's highest jump fits here; keep water and cliff feet in view.
            var desired = new Vector3(x, CameraFloor, -10);
            var position = Vector3.SmoothDamp(View.transform.position, desired, ref cameraVelocity, 0.16f);
            ApplyView(position.x, position.y);
        }
    }

    void OnGUI()
    {
        if (!showControls) return;
        GUILayout.BeginArea(new Rect(10, 10, Mathf.Min(Screen.width - 20, 620), 215), GUI.skin.box);
        GUILayout.Label("GOLDEN RIVER / BACKGROUND DECISION LAB");
        GUILayout.Label("A/D: move | Space: jump | R: retry. 109 m painted review course.");
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
        foreach (var mesh in terrainMeshes) if (mesh) DestroyImmediate(mesh);
        if (terrainMaterial) DestroyImmediate(terrainMaterial);
        if (cornerMaterial) DestroyImmediate(cornerMaterial);
        if (riverFrontMaterial) DestroyImmediate(riverFrontMaterial);
        if (square) DestroyImmediate(square); if (pixel) DestroyImmediate(pixel);
    }
}
