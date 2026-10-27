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
    public const float SurfaceFrontInset = 0.12f;
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
    Material terrainMaterial, cornerMaterial, waterMaterial, foliageMaterial;
    Transform water;
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
        var textures = new[] { atlas, Resources.Load<Texture2D>("BackgroundStudy/middle-cutout"),
            Resources.Load<Texture2D>("BackgroundStudy/near-cutout") };
        for (int i = 0; i < 3; i++) {
            layers[i] = new GameObject("Painted depth " + i).transform;
            layers[i].SetParent(transform, false);
            materials[i] = new Material(Shader.Find("LegendOfCat/BackgroundStudy"));
            materials[i].mainTexture = textures[i];
            materials[i].SetFloat("_Cutout", i == 0 ? 0 : 1);
            float width = i == 0 ? 64 : 24;
            for (int tile = -1; tile <= 1; tile++) {
                var panel = GameObject.CreatePrimitive(PrimitiveType.Quad);
                DestroyImmediate(panel.GetComponent<Collider>());
                panel.name = "Mirrored scenery tile " + tile;
                panel.transform.SetParent(layers[i], false);
                panel.transform.localPosition = new Vector3(tile * width, i == 0 ? 0 : i == 1 ? 1 : -2, 20 - i * 2);
                // Preserve the painting's proportions instead of stretching thin atlas strips.
                panel.transform.localScale = new Vector3(width, i == 0 ? width / 4.5f : width / 1.5f, 1);
                panel.GetComponent<MeshRenderer>().sharedMaterial = materials[i];
            }
        }
        // Reuse the approved motor and presentation, but own every object in this lab.
        terrainMaterial = new Material(Shader.Find("LegendOfCat/StudyTerrain"));
        terrainMaterial.mainTexture = Resources.Load<Texture2D>("BackgroundStudy/painted-terrain");
        terrainMaterial.SetTexture("_TopTex", Resources.Load<Texture2D>("BackgroundStudy/meadow-surface"));
        foliageMaterial = new Material(Shader.Find("LegendOfCat/BackgroundStudy"));
        foliageMaterial.mainTexture = Resources.Load<Texture2D>("BackgroundStudy/hanging-foliage");
        foliageMaterial.SetFloat("_Cutout", 1); foliageMaterial.SetFloat("_Fog", 0.04f);
        foliageMaterial.SetFloat("_AtlasCount", 3);
        for (int index = 0; index < Platforms.Length; index++) {
            var p = Platforms[index];
            var ground = Block("World-space ground", new Vector2(p.x, p.y), new Vector2(p.z, 1.5f), new Color(0.19f, 0.23f, 0.2f));
            ground.GetComponent<SpriteRenderer>().enabled = false;
            ground.AddComponent<BoxCollider2D>();
            Terrain(p, IsFloating(index), index);
        }
        water = new GameObject("Foreground river and small stones").transform;
        water.SetParent(transform, false);
        waterMaterial = new Material(Shader.Find("LegendOfCat/BackgroundStudy"));
        waterMaterial.mainTexture = Resources.Load<Texture2D>("BackgroundStudy/foreground-river");
        waterMaterial.SetFloat("_Cutout", 1); waterMaterial.SetFloat("_Fog", 0.18f);
        waterMaterial.SetFloat("_Saturation", 0.82f);
        for (int tile = -1; tile <= 1; tile++) {
            var panel = GameObject.CreatePrimitive(PrimitiveType.Quad);
            DestroyImmediate(panel.GetComponent<Collider>());
            panel.name = "Foreground river tile"; panel.transform.SetParent(water, false);
            // Lower, smaller river motifs connect to the scenery rather than forming a tall foreground curtain.
            panel.transform.localPosition = new Vector3(tile * 16, -0.5f, -1);
            panel.transform.localScale = new Vector3(16, 16 / 1.5f, 1);
            panel.GetComponent<MeshRenderer>().sharedMaterial = waterMaterial;
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
        float contact = platform.y + 0.75f;
        float top = contact - SurfaceFrontInset;
        float depth = floating ? 1.8f + variant % 3 * 0.25f : top + 10;
        // The exposed underside is irregular; the standable top remains flat and exact.
        var shape = floating ? new[] { new Vector2(0, 1), new Vector2(1, 1),
            new Vector2(0.93f, 0.42f), new Vector2(0.72f, 0.18f),
            new Vector2(0.38f + variant % 3 * 0.1f, 0), new Vector2(0.24f, 0.23f), new Vector2(0.06f, 0.48f) }
            : new[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0) };
        var vertices = new Vector3[shape.Length];
        var indices = new int[(shape.Length - 2) * 3];
        for (int i = 0; i < shape.Length; i++)
            vertices[i] = new Vector3((shape[i].x - 0.5f) * platform.z, top - depth + shape[i].y * depth, 2);
        for (int i = 0; i < shape.Length - 2; i++) {
            indices[i * 3] = 0; indices[i * 3 + 1] = i + 1; indices[i * 3 + 2] = i + 2;
        }
        var mesh = new Mesh { name = floating ? "Painted floating rock" : "Painted grounded cliff" };
        mesh.vertices = vertices; mesh.uv = shape; mesh.triangles = indices; mesh.RecalculateBounds();
        terrainMeshes.Add(mesh);
        var visual = new GameObject(mesh.name); visual.transform.SetParent(transform, false);
        visual.transform.position = new Vector3(platform.x, 0, 0);
        visual.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = visual.AddComponent<MeshRenderer>(); renderer.sharedMaterial = terrainMaterial;
        var parameters = new MaterialPropertyBlock();
        parameters.SetFloat("_Width", platform.z); parameters.SetFloat("_Depth", depth);
        parameters.SetFloat("_Seed", variant * 0.37f);
        parameters.SetColor("_Tint", variant % 3 == 0 ? new Color(1.02f, 1, 0.96f)
            : variant % 3 == 1 ? new Color(0.96f, 1, 0.94f) : new Color(1, 0.97f, 0.91f));
        renderer.SetPropertyBlock(parameters);
        // The contact line sits inside a narrow painted grass band, like the concept.
        // Keep the real collider unchanged and avoid broad trapezoids or box-like side faces.
        float back = SurfaceFrontInset + 0.10f + variant % 3 * 0.015f;
        var upper = new Mesh { name = "Painted upper surface" };
        float half = platform.z * 0.5f;
        upper.vertices = new[] { new Vector3(-half, top, 1.9f), new Vector3(half, top, 1.9f),
            new Vector3(half - 0.04f, top + back, 2.1f), new Vector3(-half + 0.04f, top + back, 2.1f) };
        upper.uv = new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };
        upper.triangles = new[] { 0, 1, 2, 0, 2, 3 }; upper.RecalculateBounds(); terrainMeshes.Add(upper);
        var topVisual = new GameObject(upper.name); topVisual.transform.SetParent(transform, false);
        topVisual.transform.position = new Vector3(platform.x, 0, 0);
        topVisual.AddComponent<MeshFilter>().sharedMesh = upper;
        var topRenderer = topVisual.AddComponent<MeshRenderer>(); topRenderer.sharedMaterial = terrainMaterial;
        parameters.SetFloat("_Surface", 1); topRenderer.SetPropertyBlock(parameters);
        for (int patch = 0; patch <= variant % 3; patch++) {
            float length = 1.3f + (variant + patch) % 4 * 0.45f;
            float fraction = patch == 0 ? 0.06f : patch == 1 ? 0.38f : 0.8f;
            var vine = GameObject.CreatePrimitive(PrimitiveType.Quad);
            DestroyImmediate(vine.GetComponent<Collider>());
            vine.name = "Hanging platform foliage"; vine.transform.SetParent(transform, false);
            vine.transform.position = new Vector3(platform.x - half + platform.z * fraction, top + 0.06f - length * 0.5f, 1.65f);
            vine.transform.localScale = new Vector3(length * 0.5f * ((variant + patch) % 2 == 0 ? 1 : -1), length, 1);
            var leaves = vine.GetComponent<MeshRenderer>(); leaves.sharedMaterial = foliageMaterial;
            var leafParameters = new MaterialPropertyBlock(); leafParameters.SetFloat("_AtlasIndex", (variant + patch) % 3);
            leaves.SetPropertyBlock(leafParameters);
        }
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
        if (water) {
            float center = TileCenter(x, 1, 16);
            water.position = new Vector3(center, 0, 0);
            int tileIndex = Mathf.RoundToInt(center / 16);
            for (int tile = 0; tile < 3; tile++) {
                var panel = water.GetChild(tile); var scale = panel.localScale;
                scale.x = ((tileIndex + tile - 1) & 1) == 0 ? 16 : -16;
                panel.localScale = scale;
            }
        }
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
            var desired = new Vector3(x, Mathf.Clamp(Cat.transform.position.y - 0.8f, CameraFloor, 3), -10);
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
        if (waterMaterial) DestroyImmediate(waterMaterial);
        if (foliageMaterial) DestroyImmediate(foliageMaterial);
        if (square) DestroyImmediate(square); if (pixel) DestroyImmediate(pixel);
    }
}
