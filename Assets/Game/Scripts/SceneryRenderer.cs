using UnityEngine;

// Painted parallax, terrain and water renderer for the playable course.
[DefaultExecutionOrder(100)]
public sealed class SceneryRenderer : MonoBehaviour
{
    public const float SkyTravel = 0.05f, ValleyTravel = 0.14f, CanopyTravel = 0.08f;
    public Camera View { get; private set; }
    public const float CameraFloor = 1f;
    public static bool IsFloating(int index) => index == 3 || index == 4 || index == 8 || index == 12;
    public CatController Cat { get; private set; }
    // Optional course-owned styling; null retains the original Movement Lab selection.
    public int[] FloatingPlatforms, RootPlatforms;
    public bool RouteLandmarks;
    readonly Transform[] landmarks = new Transform[3];
    public static readonly float[] LandmarkAnchors = { 84, 216, 342 };
    Material landmarkMaterial;
    readonly Transform[] easterEggs = new Transform[4];
    float fishClock, waterCaptureTime = -1;
    Rigidbody2D catBody;
    readonly Transform[] layers = new Transform[3];
    readonly Material[] materials = new Material[3];
    readonly System.Collections.Generic.List<Mesh> terrainMeshes = new System.Collections.Generic.List<Mesh>();
    Material terrainMaterial, cornerMaterial, riverFrontMaterial;
    Transform riverFront;
    Mesh quadMesh;
    float waterRunAmount, waterRunOffset;
    bool initialized;
    Vector3[] coursePlatforms;
    static readonly float[] TravelFactors = { SkyTravel, ValleyTravel, CanopyTravel };

    public void InitializeForCourse(Camera camera, CatController cat, Vector3[] platforms)
    {
        View = camera;
        Cat = cat;
        catBody = cat.GetComponent<Rigidbody2D>();
        coursePlatforms = platforms;
        Initialize();
    }

    static Material PaintedMaterial(string name, string shader)
    {
        var source = Resources.Load<Material>("BackgroundStudy/" + name);
        return source ? new Material(source) : new Material(Shader.Find(shader));
    }

    // Camera-relative travel, calculated absolutely: reversals and teleports cannot accumulate drift.
    public static float Offset(float cameraX, float speed) => cameraX * (1 - speed);
    public static float TileCenter(float cameraX, float speed, float width) =>
        Mathf.Floor(cameraX * speed / width + 0.5f) * width;

    void Initialize()
    {
        if (initialized) return;
        initialized = true;
        // Render-only unit quad: never create a primitive's temporary MeshCollider.
        quadMesh = new Mesh { name = "Scenery unit quad" };
        quadMesh.vertices = new[] { new Vector3(-0.5f, -0.5f), new Vector3(0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f), new Vector3(0.5f, 0.5f) };
        quadMesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.up, Vector2.one };
        quadMesh.triangles = new[] { 0, 2, 1, 1, 2, 3 };
        quadMesh.RecalculateBounds();
        var atlas = Resources.Load<Texture2D>("BackgroundStudy/painted-atlas");
        var textures = new[] { atlas, Resources.Load<Texture2D>("BackgroundStudy/recessed-valley-v4"),
            Resources.Load<Texture2D>("BackgroundStudy/distant-canopy-v3") };
        for (int i = 0; i < 3; i++) {
            layers[i] = new GameObject("Painted depth " + i).transform;
            layers[i].SetParent(transform, false);
            materials[i] = PaintedMaterial("background-material", "LegendOfCat/BackgroundStudy");
            materials[i].mainTexture = textures[i];
            materials[i].SetFloat("_Cutout", i == 0 ? 0 : 1);
            if (i == 1) { materials[i].SetFloat("_Fog", 0.32f); materials[i].SetFloat("_Saturation", 0.8f); }
            if (i == 2) { materials[i].SetFloat("_Fog", 0.42f); materials[i].SetFloat("_Saturation", 0.75f); }
            if (RouteLandmarks && i > 0) {
                // One authored strip covers the whole route's parallax travel, without recycling.
                materials[i].mainTexture = Resources.Load<Texture2D>("GoldenRiver/" + (i == 1 ? "valley-panorama-v3" : "canopy-panorama-v3"));
                float panoramaWidth = i == 1 ? 104 : 80;
                float crop = i == 1 ? 0.10f : 0.12f;
                var panorama = Quad();
                panorama.name = i == 1 ? "Continuous valley panorama" : "Continuous canopy panorama";
                panorama.transform.SetParent(layers[i], false);
                panorama.transform.localPosition = new Vector3(i == 1 ? 35 : 20, i == 1 ? 10 : 7, i == 1 ? 16 : 24);
                panorama.transform.localScale = new Vector3(panoramaWidth, panoramaWidth / 3, 1);
                // Crop lower terrain below the camera while retaining the painting's native aspect.
                var mesh = Instantiate(quadMesh);
                mesh.name = panorama.name + " mesh";
                mesh.uv = new[] { new Vector2(0, crop), new Vector2(1, crop),
                    new Vector2(0, 1 + crop), new Vector2(1, 1 + crop) };
                terrainMeshes.Add(mesh);
                panorama.GetComponent<MeshFilter>().sharedMesh = mesh;
                panorama.GetComponent<MeshRenderer>().sharedMaterial = materials[i];
                continue;
            }
            float width = LayerWidth(i);
            for (int tile = -1; tile <= 1; tile++) {
                var panel = Quad();
                panel.name = "Mirrored scenery tile " + tile;
                panel.transform.SetParent(layers[i], false);
                panel.transform.localPosition = new Vector3(tile * width, i == 0 ? 0 : -1,
                    i == 0 ? 30 : i == 1 ? 16 : 24);
                // Preserve the painting's proportions instead of stretching thin atlas strips.
                panel.transform.localScale = new Vector3(width, i == 0 ? width / 4.5f : width / 3, 1);
                panel.GetComponent<MeshRenderer>().sharedMaterial = materials[i];
            }
        }
        // Independent foreground water: no texture, geometry or perspective join to the valley.
        riverFront = new GameObject("Foreground water").transform;
        riverFront.SetParent(transform, false);
        riverFrontMaterial = PaintedMaterial("water-material", "LegendOfCat/StudyWater");
        riverFrontMaterial.mainTexture = Resources.Load<Texture2D>(RouteLandmarks ? "GoldenRiver/river-water-v2" : "BackgroundStudy/foreground-river");
        riverFrontMaterial.SetFloat("_Fog", 0.20f);
        riverFrontMaterial.SetFloat("_Saturation", 0.70f);
        // Keep transparent headroom: clipping at the old wave height cuts new crests into a rectangle.
        // Retain the original world/UV mapping for both the lab and route paintings.
        var waterMesh = new Mesh { name = "Visible foreground water" };
        waterMesh.vertices = new[] { new Vector3(-0.5f, -0.5f), new Vector3(0.5f, -0.5f),
            new Vector3(-0.5f, 0), new Vector3(0.5f, 0) };
        waterMesh.uv = new[] { Vector2.zero, Vector2.right, new Vector2(0, 0.5f), new Vector2(1, 0.5f) };
        waterMesh.triangles = new[] { 0, 2, 1, 1, 2, 3 };
        waterMesh.RecalculateBounds(); terrainMeshes.Add(waterMesh);
        for (int tile = 0; tile < 3; tile++) {
            var front = Quad();
            front.name = "Foreground water tile " + tile;
            front.transform.SetParent(riverFront, false);
            front.transform.localPosition = new Vector3((tile - 1) * 16, RouteLandmarks ? -0.64f : -0.32f, -2);
            front.transform.localScale = new Vector3(16, 16 / 1.5f, 1);
            front.GetComponent<MeshRenderer>().sharedMaterial = riverFrontMaterial;
            front.GetComponent<MeshFilter>().sharedMesh = waterMesh;
        }
        terrainMaterial = PaintedMaterial("terrain-material", "LegendOfCat/StudyTerrain");
        terrainMaterial.mainTexture = Resources.Load<Texture2D>("BackgroundStudy/painted-pillar-v2");
        var platforms = coursePlatforms;
        for (int index = 0; index < platforms.Length; index++) {
            var p = platforms[index];
            Terrain(p, FloatingPlatforms == null ? IsFloating(index) : System.Array.IndexOf(FloatingPlatforms, index) >= 0, index);
        }
        if (RouteLandmarks) {
            CreateLandmarks();
            CreateEasterEggs();
            CreateRiverRocks();
        }
        cornerMaterial = PaintedMaterial("background-material", "LegendOfCat/BackgroundStudy");
        cornerMaterial.mainTexture = Resources.Load<Texture2D>("BackgroundStudy/foreground-corners");
        cornerMaterial.SetFloat("_Cutout", 1); cornerMaterial.SetFloat("_Fog", 0);
        cornerMaterial.SetFloat("_Corner", 1);
        float halfView = View.orthographicSize * 16 / 9;
        for (int side = 0; side < 2; side++) {
            var corners = Quad();
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
        ApplyViewport(Screen.width, Screen.height);
        ApplyView(6, CameraFloor);
    }

    void Terrain(Vector3 platform, bool floating, int variant)
    {
        // Artwork owns its silhouette; the collider owns the exact standable span.
        bool bank = platform.z >= 6;
        float contact = platform.y + 0.75f;
        // Register contact independently from the authored grass tufts above it.
        bool variedCourse = FloatingPlatforms != null;
        bool distinctShelf = floating && variedCourse;
        bool rootShelf = distinctShelf && System.Array.IndexOf(RootPlatforms, variant) >= 0;
        bool compactShelf = distinctShelf && platform.z <= (rootShelf ? 4.25f : 4.5f);
        string texturePath;
        float contactUV, landingFraction;
        if (distinctShelf) {
            texturePath = "GoldenRiver/" + (rootShelf ? "shelf-root" : "shelf-masonry") + (compactShelf ? "-compact-v3" : "");
            contactUV = rootShelf ? (compactShelf ? 0.70f : 0.68f) : 0.66f;
            landingFraction = rootShelf ? 0.90f : 0.96f;
        } else if (variedCourse) {
            string form = platform.z >= 18 ? "wide" : platform.z >= 10 ? "medium" : "narrow";
            texturePath = "GoldenRiver/bank-" + form + "-v3";
            contactUV = form == "narrow" ? 0.66f : 0.587f;
            landingFraction = form == "narrow" ? 0.94f : 0.89f;
        } else {
            texturePath = floating ? "BackgroundStudy/painted-shelf-v3" : bank ? "BackgroundStudy/painted-bank-v2" : "BackgroundStudy/painted-pillar-v2";
            contactUV = floating ? 0.66f : bank ? 0.587f : 0.748f;
            landingFraction = floating ? 0.90f : bank ? 0.89f : 0.76f;
        }
        float width = platform.z / landingFraction;
        float sourceAspect = compactShelf ? 2 : 3;
        float bottom = distinctShelf ? contact - width / sourceAspect * contactUV : floating ? contact - 0.90f : -4.5f;
        float crown = distinctShelf ? contact + width / sourceAspect * (1 - contactUV) : contact + (floating ? 0.35f : 0.60f);
        float half = width * 0.5f;
        var vertices = new[] {
            new Vector3(-half, bottom, 2), new Vector3(half, bottom, 2),
            new Vector3(-half, contact, 2), new Vector3(half, contact, 2),
            new Vector3(-half, crown, 2), new Vector3(half, crown, 2)
        };
        bool mirror = !variedCourse && variant % 3 == 1;
        float left = mirror ? 1 : 0, right = 1 - left;
        var mesh = new Mesh { name = distinctShelf ? (rootShelf ? "Root island" : "Masonry shelf") + (compactShelf ? " compact" : " long") : variedCourse ? "Painted grounded bank" : floating ? "Painted floating rock" : "Painted grounded cliff" };
        mesh.vertices = vertices;
        mesh.uv = new[] { new Vector2(left, 0), new Vector2(right, 0),
            new Vector2(left, contactUV), new Vector2(right, contactUV),
            new Vector2(left, 1), new Vector2(right, 1) };
        mesh.triangles = new[] { 0, 2, 1, 1, 2, 3, 2, 4, 3, 3, 4, 5 };
        mesh.RecalculateBounds(); terrainMeshes.Add(mesh);
        var visual = new GameObject(mesh.name); visual.transform.SetParent(transform, false);
        visual.transform.localPosition = new Vector3(platform.x, 0, 0);
        visual.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = visual.AddComponent<MeshRenderer>(); renderer.sharedMaterial = terrainMaterial;
        var parameters = new MaterialPropertyBlock();
        parameters.SetTexture("_MainTex", Resources.Load<Texture2D>(texturePath));
        renderer.SetPropertyBlock(parameters);
        if (!variedCourse && !floating && (variant == 0 || variant == 2 || variant == 6 || variant == 10 || variant == 13))
            ShoreFoot(platform, variant == 0 || variant == 10 ? -1 : 1);
    }

    void CreateLandmarks()
    {
        landmarkMaterial = PaintedMaterial("background-material", "LegendOfCat/BackgroundStudy");
        landmarkMaterial.SetFloat("_Cutout", 1);
        landmarkMaterial.SetFloat("_Fog", 0.24f);
        landmarkMaterial.SetFloat("_Saturation", 0.72f);
        var names = new[] { "waterwheel", "viaduct", "willow" };
        var brightness = new[] { 0.87f, 0.89f, 0.72f };
        var fog = new[] { 0.24f, 0.28f, 0.16f };
        for (int i = 0; i < names.Length; i++) {
            var item = Quad();
            item.name = "Unique landmark " + names[i];
            item.transform.SetParent(transform, false);
            item.transform.localPosition = new Vector3(LandmarkAnchors[i], -0.5f, 8);
            // The opaque hillside continues below the viewport; its sides descend under the river.
            item.transform.localScale = new Vector3(30, 20, 1);
            item.GetComponent<MeshRenderer>().sharedMaterial = landmarkMaterial;
            var properties = new MaterialPropertyBlock();
            properties.SetTexture("_MainTex", Resources.Load<Texture2D>("GoldenRiver/landmark-" + names[i] + "-hill-v2"));
            properties.SetFloat("_Brightness", brightness[i]);
            properties.SetFloat("_Fog", fog[i]);
            item.GetComponent<MeshRenderer>().SetPropertyBlock(properties);
            landmarks[i] = item.transform;
        }
    }

    void CreateEasterEggs()
    {
        // Review anchor only: the statue is painted into the valley so it shares its depth and masonry.
        var king = new GameObject("Easter egg cat king");
        king.transform.SetParent(transform, false);
        easterEggs[0] = king.transform;
        var fish = new GameObject("Easter egg watching fish");
        fish.transform.SetParent(transform, false);
        fish.transform.localPosition = new Vector3(96, -3.38f, -2.2f);
        easterEggs[1] = fish.transform;
        for (int i = 0; i < 2; i++) {
            var item = Quad();
            item.name = i == 0 ? "Drifting fish left" : "Drifting fish right";
            item.transform.SetParent(fish.transform, false);
            item.transform.localScale = new Vector3(0.70f, 0.70f, 1);
            var mesh = Instantiate(quadMesh);
            mesh.name = item.name + " mesh";
            float left = i * 0.5f, right = left + 0.5f;
            mesh.uv = new[] { new Vector2(left, 0), new Vector2(right, 0), new Vector2(left, 1), new Vector2(right, 1) };
            terrainMeshes.Add(mesh);
            item.GetComponent<MeshFilter>().sharedMesh = mesh;
            item.GetComponent<MeshRenderer>().sharedMaterial = landmarkMaterial;
            var properties = new MaterialPropertyBlock();
            properties.SetTexture("_MainTex", Resources.Load<Texture2D>("GoldenRiver/egg-watching-fish"));
            properties.SetFloat("_Fog", 0.02f); properties.SetFloat("_Brightness", 0.95f);
            properties.SetFloat("_Submerged", 1);
            item.GetComponent<MeshRenderer>().SetPropertyBlock(properties);
        }
        // These two belong after the introduction, without blocking the playable plane.
        easterEggs[2] = Egg("no cats sign", "egg-no-cats-sign", new Vector3(440, 1.54f, 3), new Vector2(2.2f, 2.2f), true);
        easterEggs[3] = Egg("cat rock", "egg-cat-rock", new Vector3(499, -0.55f, 1.5f), new Vector2(1.25f, 1.25f), true);
        UpdateFish(fishClock);
    }

    Transform Egg(string name, string source, Vector3 position, Vector2 size, bool foreground)
    {
        var item = Quad();
        item.name = "Easter egg " + name;
        item.transform.SetParent(transform, false);
        item.transform.localPosition = position;
        item.transform.localScale = new Vector3(size.x, size.y, 1);
        item.GetComponent<MeshRenderer>().sharedMaterial = foreground ? terrainMaterial : landmarkMaterial;
        var properties = new MaterialPropertyBlock();
        properties.SetTexture("_MainTex", Resources.Load<Texture2D>("GoldenRiver/" + source));
        properties.SetFloat("_Fog", 0.28f); properties.SetFloat("_Brightness", 0.90f);
        item.GetComponent<MeshRenderer>().SetPropertyBlock(properties);
        return item.transform;
    }

    void UpdateFish(float seconds)
    {
        if (!easterEggs[1]) return;
        easterEggs[1].localPosition = new Vector3(96 + 0.18f * Mathf.Sin(seconds * 0.7f), -3.38f, -2.2f);
        for (int i = 0; i < 2; i++) {
            float phase = seconds * 1.6f + i * 1.8f;
            var fish = easterEggs[1].GetChild(i);
            // The current slowly wins, then a short swimming stroke recovers the lost ground.
            float cycle = Mathf.Repeat(seconds / 2.4f + i * 0.43f, 1);
            float recovery = Mathf.SmoothStep(0, 1, Mathf.Clamp01((cycle - 0.76f) / 0.24f));
            float drift = 0.16f * (cycle - recovery);
            fish.localPosition = new Vector3((i == 0 ? -0.34f : 0.34f) + drift,
                0.055f * Mathf.Sin(phase), 0);
            Vector3 target = Cat ? transform.InverseTransformPoint(Cat.transform.position) : Vector3.zero;
            Vector3 position = easterEggs[1].localPosition + fish.localPosition;
            bool watching = Cat && Mathf.Abs(target.x - 96) < 9 && Vector2.Distance(target, position) < 11;
            float facing = watching ? (target.x >= position.x ? 1 : -1) : -1;
            // Both painted heads look upward; turn the silhouette and incline the head toward Mika.
            float nativeFacing = -1;
            fish.localScale = new Vector3(0.70f * facing * nativeFacing, 0.70f, 1);
            float gaze = watching ? Mathf.Clamp((target.x - position.x) * -2, -12, 12) : 0;
            float stroke = Mathf.Sin(recovery * Mathf.PI) * 6;
            fish.localRotation = Quaternion.Euler(0, 0, gaze + facing * stroke + 3 * Mathf.Sin(phase * 0.8f));
        }
    }

    void CreateRiverRocks()
    {
        float[] anchors = { 22, 78, 163, 273, 394, 487 };
        for (int i = 0; i < anchors.Length; i++) {
            var rock = Quad();
            rock.name = "Unique river rock group " + i;
            rock.transform.SetParent(transform, false);
            rock.transform.localPosition = new Vector3(anchors[i], -4.47f, -2.3f);
            rock.transform.localScale = new Vector3(2.2f, 2.2f, 1);
            var mesh = Instantiate(quadMesh);
            mesh.name = rock.name + " mesh";
            float left = (i % 3) / 3f, right = left + 1f / 3;
            float bottom = i < 3 ? 0.5f : 0, top = bottom + 0.5f;
            mesh.uv = new[] { new Vector2(left, bottom), new Vector2(right, bottom),
                new Vector2(left, top), new Vector2(right, top) };
            terrainMeshes.Add(mesh);
            rock.GetComponent<MeshFilter>().sharedMesh = mesh;
            rock.GetComponent<MeshRenderer>().sharedMaterial = landmarkMaterial;
            var properties = new MaterialPropertyBlock();
            properties.SetTexture("_MainTex", Resources.Load<Texture2D>("GoldenRiver/river-rock-groups-v1"));
            properties.SetFloat("_Fog", 0.08f);
            properties.SetFloat("_Brightness", 0.9f);
            rock.GetComponent<MeshRenderer>().SetPropertyBlock(properties);
        }
    }

    void ShoreFoot(Vector3 platform, int side)
    {
        // A low decorative buttress joins the cliff to its bank below the landing plane.
        var foot = Quad();
        foot.name = "Painted shore footing"; foot.transform.SetParent(transform, false);
        foot.transform.localPosition = new Vector3(platform.x + side * (platform.z * 0.5f + 1.25f), -2.65f, 3);
        foot.transform.localScale = new Vector3(4.8f * side, 2.6f, 1);
        var renderer = foot.GetComponent<MeshRenderer>(); renderer.sharedMaterial = terrainMaterial;
        var parameters = new MaterialPropertyBlock();
        parameters.SetTexture("_MainTex", Resources.Load<Texture2D>("BackgroundStudy/painted-shore-v2"));
        renderer.SetPropertyBlock(parameters);
    }
    GameObject Quad()
    {
        var item = new GameObject("Scenery quad", typeof(MeshFilter), typeof(MeshRenderer));
        item.GetComponent<MeshFilter>().sharedMesh = quadMesh;
        return item;
    }

    static float LayerWidth(int layer) => layer == 0 ? 64 : layer == 1 ? 40 : 48;

    public void ApplyView(float x, float y)
    {
        // Keep the damp-foot tint registered with the placed shore/water grid too.
        terrainMaterial.SetFloat("_CourseOriginY", transform.position.y);
        terrainMaterial.SetFloat("_CourseScale", transform.lossyScale.y);
        View.transform.position = new Vector3(x, y, -10);
        var localView = transform.InverseTransformPoint(new Vector3(x, y, 0));
        // Distant scenery has no physical join to the world-space terrain or foreground water.
        for (int i = 0; i < 3; i++) {
            if (RouteLandmarks && i > 0) {
                layers[i].localPosition = new Vector3(Offset(localView.x, TravelFactors[i]), localView.y, 0);
                continue;
            }
            float width = LayerWidth(i);
            float center = TileCenter(localView.x, TravelFactors[i], width);
            layers[i].localPosition = new Vector3(Offset(localView.x, TravelFactors[i]) + center, localView.y, 0);
            int tileIndex = Mathf.RoundToInt(center / width);
            for (int j = 0; j < 3; j++) {
                var panel = layers[i].GetChild(j);
                var scale = panel.localScale;
                scale.x = ((tileIndex + j - 1) & 1) == 0 ? width : -width;
                panel.localScale = scale;
            }
        }
        if (RouteLandmarks) {
            for (int i = 0; i < landmarks.Length; i++) {
                var position = landmarks[i].localPosition;
                // Each landmark has one fixed anchor, never participates in tile recycling.
                position.x = LandmarkAnchors[i] + (localView.x - LandmarkAnchors[i]) * 0.65f;
                landmarks[i].localPosition = position;
            }
            // Register the small statue with the right-hand battlement in the valley painting.
            easterEggs[0].localPosition = new Vector3(Offset(localView.x, ValleyTravel) + 34.9f, localView.y + 4.37f, 12);
            UpdateFish(waterCaptureTime >= 0 ? waterCaptureTime : fishClock);
        }
        PositionForegroundWater(localView.x);
    }

    void PositionForegroundWater(float cameraX)
    {
        float center = TileCenter(cameraX, 1, 16);
        riverFront.localPosition = new Vector3(center, 0, 0);
        int tileIndex = Mathf.RoundToInt(center / 16);
        for (int tile = 0; tile < 3; tile++) {
            var scale = riverFront.GetChild(tile).localScale;
            scale.x = ((tileIndex + tile - 1) & 1) == 0 ? 16 : -16;
            riverFront.GetChild(tile).localScale = scale;
        }
    }

    public void SetWaterTime(float seconds)
    {
        waterCaptureTime = seconds;
        UpdateFish(seconds >= 0 ? seconds : fishClock);
        riverFrontMaterial.SetFloat("_WaterTime", seconds);
        riverFrontMaterial.SetFloat("_UseCaptureTime", seconds >= 0 ? 1 : 0);
    }

    public void ApplyViewport(int width, int height)
    {
        View.rect = MenuLayout.Letterbox16By9(width, height);
        View.aspect = 16f / 9f;
    }

    void FixedUpdate()
    {
        if (!Cat || !catBody || !riverFrontMaterial) return;
        if (RouteLandmarks && !Cat.Paused) {
            fishClock += Time.fixedDeltaTime;
            if (waterCaptureTime < 0) UpdateFish(fishClock);
        }
        float target = Cat.Grounded && catBody.simulated
            ? Mathf.Clamp01(Mathf.Abs(catBody.linearVelocity.x) / CatMotion.Speed) : 0;
        waterRunAmount = Mathf.MoveTowards(waterRunAmount, target, 2.5f * Time.fixedDeltaTime);
        // Add travel time rather than multiplying the clock, avoiding phase jumps on start/stop.
        waterRunOffset += waterRunAmount * 1.2f * Time.fixedDeltaTime;
        riverFrontMaterial.SetFloat("_RunAmount", waterRunAmount);
        riverFrontMaterial.SetFloat("_RunTimeOffset", waterRunOffset);
    }

    void LateUpdate()
    {
        if (!View) return;
        ApplyViewport(Screen.width, Screen.height);
        ApplyView(View.transform.position.x, View.transform.position.y);
    }

    void OnDestroy()
    {
        foreach (var material in materials) Release(material);
        foreach (var mesh in terrainMeshes) Release(mesh);
        Release(quadMesh);
        Release(terrainMaterial); Release(cornerMaterial); Release(riverFrontMaterial); Release(landmarkMaterial);
    }

    static void Release(Object owned)
    {
        if (!owned) return;
        if (Application.isPlaying) Destroy(owned);
        else DestroyImmediate(owned); // Edit-mode previews have no deferred destruction loop.
    }
}
