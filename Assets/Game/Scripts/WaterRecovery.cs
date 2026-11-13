using UnityEngine;

// A bounded water-entry presentation. Checkpoint ownership remains with the course.
[DefaultExecutionOrder(50)]
public sealed class WaterRecovery : MonoBehaviour
{
    public const float SurfaceY = -3.25f;
    public const float ForegroundInset = 0.65f;
    public const float RespawnTime = 2.1f, Duration = 2.5f;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    const bool ForceSharkForDebug = true;
#else
    const bool ForceSharkForDebug = false;
#endif
    const float SharkLaunchTime = 0.72f, SharkBiteTime = 1.93f, SharkDiveEnd = 2.30f;
    bool HasSharkEncounter => sharkEncounter && shark;
    float FadeStart => HasSharkEncounter ? 2.55f : 1.65f;
    float RetryTime => HasSharkEncounter ? 3.0f : RespawnTime;
    public float SequenceDuration => HasSharkEncounter ? 3.4f : Duration;
    public bool Active { get; private set; }
    public bool CameraHeld => Active && !respawned;
    public float Elapsed { get; private set; }
    public float Fade => !Active ? 0 : Elapsed < RetryTime
        ? Mathf.InverseLerp(FadeStart, FadeStart + 0.30f, Elapsed)
        : 1 - Mathf.InverseLerp(RetryTime, SequenceDuration, Elapsed);
    CatController cat;
    Camera view;
    Transform course;
    Rigidbody2D body;
    System.Action retry;
    MikaSpritePresentation original;
    SpriteRenderer originalRenderer, reaction, impactBody;
    Sprite[] drawings, splashFrames;
    SpriteRenderer splashFront, splashBack;
    MeshRenderer shark;
    Mesh sharkMesh;
    Vector3[] sharkVertices;
    Vector2[] sharkUVs;
    float sharkHeight;
    const int SharkColumns = 28, SharkRows = 6;
    bool sharkEncounter;
    Material submerge, foamMaterial;
    GameObject effects;
    readonly LineRenderer[] rings = new LineRenderer[2];
    readonly LineRenderer[] droplets = new LineRenderer[44];
    LineRenderer splash, neckFoam;
    Vector3 impactPosition;
    Vector3 cameraStart, entry;
    float cameraSize, scale, facing;
    bool respawned, internalRespawn;

    public void Initialize(CatController controller, Camera camera, Transform courseRoot, System.Action checkpointRetry)
    {
        cat = controller; view = camera; course = courseRoot; retry = checkpointRetry;
        body = cat.GetComponent<Rigidbody2D>();
        original = cat.GetComponentInChildren<MikaSpritePresentation>();
        if (original) originalRenderer = original.GetComponent<SpriteRenderer>();
        cat.ReturnedToStart += OnRespawn;
    }

    public bool TryEnter()
    {
        if (Active || !cat || cat.Paused || !body.simulated || body.linearVelocity.y > 0) return false;
        var shape = cat.GetComponent<BoxCollider2D>();
        float sole = course.InverseTransformPoint(shape.bounds.min).y;
        if (sole > SurfaceY) return false;
        Begin();
        return true;
    }

    void Begin()
    {
        Active = true; respawned = false; Elapsed = 0;
        sharkEncounter = ForceSharkForDebug || SharkRollHits(Random.Range(0, 100));
        scale = course.lossyScale.y;
        cameraStart = view.transform.position; cameraSize = view.orthographicSize;
        facing = originalRenderer && originalRenderer.flipX ? -1 : 1;
        entry = new Vector3(body.position.x, course.TransformPoint(new Vector3(0, SurfaceY)).y, 0);
        cat.SetRecovering(true);
        PrepareEffects();
        if (impactBody) {
            impactBody.sprite = originalRenderer.sprite;
            impactBody.flipX = originalRenderer.flipX;
            impactPosition = originalRenderer.transform.position;
            impactBody.transform.localScale = originalRenderer.transform.lossyScale / scale;
        }
        if (original) original.enabled = false;
        if (originalRenderer) originalRenderer.enabled = false;
        effects.SetActive(true);
        Present();
    }

    static bool SharkRollHits(int roll) => roll == 0;

    void PrepareEffects()
    {
        if (effects) return;
        effects = new GameObject("Mika water reaction and splash");
        effects.transform.SetParent(course, false);
        var texture = Resources.Load<Texture2D>("WaterRecovery/turn-reactions");
        drawings = new Sprite[8];
        float width = texture.width / 4f, height = texture.height / 2f;
        for (int i = 0; i < drawings.Length; i++)
            drawings[i] = Sprite.Create(texture, new Rect(i % 4 * width, (1 - i / 4) * height, width, height),
                // Register the lower row's higher source baseline; keep one scale for all drawings.
                new Vector2(0.5f, i < 4 ? 0.5f : 0.588f), width / 0.9f, 0, SpriteMeshType.FullRect);
        reaction = new GameObject("Mika facial reaction").AddComponent<SpriteRenderer>();
        reaction.transform.SetParent(effects.transform, false);
        submerge = new Material(Resources.Load<Shader>("WaterRecovery/Submerge"));
        reaction.sharedMaterial = submerge; reaction.sortingOrder = 4;
        if (originalRenderer) {
            impactBody = new GameObject("Original fall through impact").AddComponent<SpriteRenderer>();
            impactBody.transform.SetParent(effects.transform, false);
            impactBody.sharedMaterial = submerge; impactBody.sortingOrder = 4;
        }
        PrepareShark();
        var splashTexture = Resources.Load<Texture2D>("WaterRecovery/shark-splash-frames");
        if (splashTexture) {
            splashFrames = new Sprite[3];
            float widthPerFrame = splashTexture.width / 3f;
            for (int i = 0; i < splashFrames.Length; i++)
                splashFrames[i] = Sprite.Create(splashTexture,
                    new Rect(i * widthPerFrame, 0, widthPerFrame, splashTexture.height),
                    new Vector2(0.5f, 0.15f), widthPerFrame / 3f, 0, SpriteMeshType.FullRect);
            splashBack = new GameObject("Shark splash underlay").AddComponent<SpriteRenderer>();
            splashBack.transform.SetParent(effects.transform, false);
            splashBack.sortingOrder = 6;
            splashFront = new GameObject("Shark splash overlay").AddComponent<SpriteRenderer>();
            splashFront.transform.SetParent(effects.transform, false);
            splashFront.sortingOrder = 7;
        }
        foamMaterial = new Material(Shader.Find("Sprites/Default"));
        for (int i = 0; i < rings.Length; i++) rings[i] = Line("Expanding surface ripple", 49, true);
        for (int i = 0; i < droplets.Length; i++) droplets[i] = Line("Splash droplet", 2, false);
        splash = Line("Breaking shoulder splash", 33, false);
        neckFoam = Line("Water washing over neck", 25, false);
    }

    void PrepareShark()
    {
        var texture = Resources.Load<Texture2D>("WaterRecovery/shark-breach-frames");
        if (!texture) return;

        shark = new GameObject("Shark").AddComponent<MeshRenderer>();
        shark.transform.SetParent(effects.transform, false);
        shark.sharedMaterial = submerge;
        shark.sortingOrder = 5;
        var properties = new MaterialPropertyBlock();
        properties.SetTexture("_MainTex", texture);
        shark.SetPropertyBlock(properties);

        sharkHeight = 3.1f * texture.height / (texture.width / 3f);
        sharkMesh = new Mesh { name = "Shark body grid" };
        sharkMesh.MarkDynamic();
        int stride = SharkColumns + 1;
        sharkVertices = new Vector3[stride * (SharkRows + 1)];
        sharkUVs = new Vector2[sharkVertices.Length];
        var colors = new Color32[sharkVertices.Length];
        for (int i = 0; i < colors.Length; i++) colors[i] = new Color32(255, 255, 255, 255);
        var triangles = new int[SharkColumns * SharkRows * 6];
        int index = 0;
        for (int row = 0; row < SharkRows; row++) {
            for (int col = 0; col < SharkColumns; col++) {
                int corner = row * stride + col;
                triangles[index++] = corner;
                triangles[index++] = corner + stride;
                triangles[index++] = corner + 1;
                triangles[index++] = corner + 1;
                triangles[index++] = corner + stride;
                triangles[index++] = corner + stride + 1;
            }
        }
        sharkMesh.vertices = sharkVertices;
        sharkMesh.colors32 = colors;
        sharkMesh.triangles = triangles;
        shark.gameObject.AddComponent<MeshFilter>().sharedMesh = sharkMesh;
    }

    LineRenderer Line(string name, int points, bool loop)
    {
        var line = new GameObject(name).AddComponent<LineRenderer>();
        line.transform.SetParent(effects.transform, false);
        line.sharedMaterial = foamMaterial; line.useWorldSpace = true;
        line.positionCount = points; line.loop = loop; line.sortingOrder = 5;
        line.numCapVertices = 4;
        return line;
    }

    void Update() => Advance(Time.deltaTime);

    public void Advance(float seconds)
    {
        if (!Active || cat.Paused) return;
        Elapsed += Mathf.Max(0, seconds);
        if (!respawned && Elapsed >= RetryTime) {
            internalRespawn = true;
            try { retry(); } finally { internalRespawn = false; }
            respawned = true;
            RestorePresentation();
            cat.SetRecovering(true);
        }
        if (Elapsed >= SequenceDuration) Cancel();
    }

    void LateUpdate()
    {
        if (CameraHeld) Present();
    }

    void Present()
    {
        float zoom = Mathf.SmoothStep(0, 1, Mathf.Clamp01(Elapsed / 0.55f));
        // Move the close-up into the painted river, away from its far shoreline.
        // The body enters at the physical surface; only presentation shifts with the zoom.
        Vector3 foregroundShift = Vector3.down * (ForegroundInset * scale * zoom);
        Vector3 visualEntry = entry + foregroundShift;
        // Continue the existing fall through impact, then reveal a submerged neck.
        // The head keeps descending during its authored turn; there is no floating hold.
        float sinkTime = HasSharkEncounter ? Mathf.Min(Elapsed, 0.80f) : Elapsed;
        float sink = sinkTime * 0.1f + 0.65f * Mathf.Pow(Mathf.InverseLerp(0.9f, 1.6f, sinkTime), 1.5f);
        reaction.transform.position = visualEntry + new Vector3(facing * 0.30f, 0.40f - sink, 0) * scale;
        reaction.transform.localScale = Vector3.one;
        reaction.flipX = facing < 0;
        float expressionTime = HasSharkEncounter && Elapsed < SharkBiteTime - 0.12f
            ? Mathf.Min(Elapsed, 0.80f) : Elapsed;
        int frame = expressionTime < 0.25f ? 0 : expressionTime < 0.34f ? 1 : expressionTime < 0.43f ? 2
            : expressionTime < 0.52f ? 3 : expressionTime < 0.61f ? 4 : expressionTime < 0.85f ? 5 : expressionTime < 0.98f ? 6 : 7;
        reaction.sprite = drawings[frame];
        reaction.enabled = (!impactBody || Elapsed >= 0.14f)
            && !(HasSharkEncounter && Elapsed >= SharkBiteTime);
        PresentShark(visualEntry);
        if (impactBody) {
            impactBody.enabled = Elapsed < 0.14f;
            impactBody.transform.position = impactPosition + foregroundShift - Vector3.up * (Elapsed * 2.3f) * scale;
        }
        submerge.SetFloat("_Surface", visualEntry.y);
        submerge.SetFloat("_SurfaceScale", scale);
        float breachFraming = HasSharkEncounter
            ? Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.48f, 1.00f, Elapsed)) : 0;
        view.orthographicSize = Mathf.Lerp(cameraSize, Mathf.Lerp(1.65f, 2.60f, breachFraming) * scale, zoom);
        // Follow the face only slightly as it sinks; keep the surface in the close view.
        Vector3 close = entry + new Vector3(facing * 0.12f * scale, (0.28f + breachFraming * 1.05f - ForegroundInset) * scale, -10);
        view.transform.position = Vector3.Lerp(cameraStart, close, zoom);
        // A small foreground wash overlaps the softened neck edge; the wider
        // expanding rings alone leave the drawing looking detached from the water.
        float wash = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.12f, 0.24f, Elapsed))
            * (1 - Mathf.InverseLerp(1.0f, 1.35f, Elapsed));
        neckFoam.enabled = wash > 0;
        neckFoam.startWidth = neckFoam.endWidth = 0.028f * scale;
        neckFoam.startColor = neckFoam.endColor = new Color(0.74f, 0.90f, 0.86f, wash * 0.7f);
        for (int p = 0; p < 25; p++) {
            float angle = p * Mathf.PI / 24;
            float x = facing * 0.30f + Mathf.Cos(angle) * 0.29f;
            float y = -Mathf.Sin(angle) * 0.035f + Mathf.Sin(x * 19 + Elapsed * 5) * 0.01f;
            neckFoam.SetPosition(p, visualEntry + new Vector3(x, y, -0.13f) * scale);
        }
        bool sharkSplash = HasSharkEncounter && Elapsed >= SharkLaunchTime;
        bool impact = Elapsed >= SharkBiteTime;
        float splashTime = sharkSplash ? Elapsed - (impact ? SharkBiteTime : SharkLaunchTime) : Elapsed;
        Vector3 splashOrigin = visualEntry + Vector3.right * (sharkSplash ? facing * (impact ? 0.30f : -1.55f) * scale : 0);
        float crestTime = sharkSplash ? 0.65f : 0.4f;
        float breakAmount = Mathf.Sin(Mathf.Clamp01(splashTime / crestTime) * Mathf.PI);
        PresentSharkSplash(sharkSplash, impact, splashTime, splashOrigin);
        splash.enabled = !sharkSplash && splashTime < crestTime;
        splash.startWidth = splash.endWidth = (0.035f + breakAmount * 0.11f) * scale;
        splash.startColor = splash.endColor = new Color(0.8f, 0.94f, 0.91f, 1 - Mathf.Clamp01(splashTime / crestTime));
        for (int p = 0; p < 33; p++) {
            float x = (p / 32f - 0.5f) * (0.9f + splashTime * 1.2f);
            float envelope = Mathf.Sin(p / 32f * Mathf.PI);
            float wave = Mathf.Abs(Mathf.Sin(p * 1.7f));
            float crest = envelope * (0.10f + wave * 0.22f) * breakAmount;
            splash.SetPosition(p, splashOrigin + new Vector3(x, crest, -0.12f) * scale);
        }
        float rippleTime = sharkSplash ? splashTime : Elapsed;
        float alpha = 1 - Mathf.Clamp01(rippleTime / (sharkSplash ? 0.85f : 1.75f));
        for (int r = 0; r < rings.Length; r++) {
            float radius = (0.25f + rippleTime * (0.7f + r * 0.25f)) * scale * (sharkSplash ? 2.2f : 1f);
            rings[r].enabled = !sharkSplash;
            rings[r].startWidth = rings[r].endWidth = 0.022f * scale;
            rings[r].startColor = rings[r].endColor = new Color(0.82f, 0.93f, 0.88f, alpha * 0.8f);
            for (int p = 0; p < 49; p++) {
                float angle = p * Mathf.PI * 2 / 48;
                rings[r].SetPosition(p, splashOrigin + new Vector3(Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius * 0.12f, -0.1f));
            }
        }
        for (int i = 0; i < droplets.Length; i++) {
            int count = sharkSplash ? droplets.Length : 12;
            float t = Mathf.Min(splashTime, 0.85f);
            float vx = (2f * i / (count - 1) - 1) * (sharkSplash ? 4.2f : 2.09f);
            float vy = sharkSplash ? 3.0f + (i * 7 % 13) * 0.24f : 2.4f + (i % 3) * 0.45f;
            var position = splashOrigin + new Vector3(vx * t, vy * t - 6 * t * t, -0.1f) * scale;
            droplets[i].enabled = i < count && splashTime < (sharkSplash ? 0.85f : 0.65f)
                && position.y >= visualEntry.y;
            float dropWidth = sharkSplash ? 0.014f + (i % 5) * 0.005f : 0.035f;
            droplets[i].startWidth = dropWidth * scale;
            droplets[i].endWidth = dropWidth * 0.25f * scale;
            droplets[i].startColor = droplets[i].endColor = new Color(0.8f, 0.93f, 0.9f, 1 - t);
            droplets[i].SetPosition(0, position);
            var streak = sharkSplash ? new Vector3(vx, vy - 12 * t, 0) * 0.018f : Vector3.up * 0.065f;
            droplets[i].SetPosition(1, position + streak * scale);
        }
    }

    void PresentSharkSplash(bool active, bool impact, float age, Vector3 origin)
    {
        if (!splashFront) return;
        splashFront.enabled = splashBack.enabled = active && age < 0.88f;
        if (!splashFront.enabled) return;

        float frame = Mathf.Lerp(0, 2, Mathf.InverseLerp(0.06f, 0.62f, age));
        int first = Mathf.FloorToInt(frame);
        float blend = frame - first;
        float opacity = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0, 0.07f, age))
            * (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.53f, 0.88f, age)));
        float width = Mathf.Lerp(0.48f, impact ? 1.46f : 1.20f, Mathf.InverseLerp(0, 0.68f, age));
        float height = Mathf.Lerp(0.48f, impact ? 1.12f : 0.98f, Mathf.InverseLerp(0, 0.12f, age));
        height *= Mathf.Lerp(1f, 0.52f, Mathf.InverseLerp(0.2f, 0.7f, age));
        splashBack.sprite = splashFrames[first];
        splashFront.sprite = splashFrames[Mathf.Min(first + 1, 2)];
        splashBack.color = new Color(1, 1, 1, opacity * (1 - blend));
        splashFront.color = new Color(1, 1, 1, opacity * blend);
        splashBack.transform.position = splashFront.transform.position = origin;
        splashBack.transform.localScale = splashFront.transform.localScale = new Vector3(width, height, 1);
    }

    void PresentShark(Vector3 visualEntry)
    {
        if (!shark) return;
        shark.enabled = HasSharkEncounter && Elapsed >= SharkLaunchTime && Elapsed < SharkDiveEnd;
        if (!shark.enabled) return;

        bool diving = Elapsed >= SharkBiteTime;
        float travel = diving ? Mathf.InverseLerp(SharkBiteTime, SharkDiveEnd, Elapsed)
            : Mathf.InverseLerp(SharkLaunchTime, SharkBiteTime, Elapsed);
        float x = diving ? Mathf.Lerp(0.30f, 0.94f, travel) : Mathf.Lerp(-1.55f, 0.30f, travel);
        float y = diving ? Mathf.Lerp(0.29f, -1.4f, travel)
            : Mathf.Lerp(-0.12f, 0.29f, travel) + 10.8f * travel * (1 - travel);
        // Carry the steep launch into the air before pitching over into the catch.
        float pitch = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.25f, 0.86f, travel));
        float angle = diving ? -72f : Mathf.Lerp(74f, -72f, pitch);
        shark.transform.SetPositionAndRotation(visualEntry + new Vector3(facing * x, y, 0) * scale,
            Quaternion.Euler(0, 0, facing * angle));
        int jaw = diving || travel < 0.54f ? 0 : travel < 0.72f ? 1 : 2;
        // A single airborne arch follows the pitch-over. Bend around the jaw
        // so the centerline retains its length and the catch stays registered.
        float kick = Mathf.Sin(Mathf.PI * Mathf.InverseLerp(0, 0.27f, travel));
        float arch = diving ? 0 : Mathf.Sin(Mathf.PI * Mathf.InverseLerp(0.27f, 0.96f, travel));
        float curvature = -28f * Mathf.Deg2Rad * arch / 3.1f;
        float tailAngle = diving ? 0 : 17f * kick * Mathf.Deg2Rad;
        const float tailRoot = 0.38f;
        float hingeX = (tailRoot - 0.91f) * 3.1f;
        float cos = Mathf.Cos(tailAngle);
        float sin = Mathf.Sin(tailAngle);

        for (int row = 0; row <= SharkRows; row++) {
            float v = row / (float)SharkRows;
            for (int col = 0; col <= SharkColumns; col++) {
                float u = col / (float)SharkColumns;
                float localX = (u - 0.91f) * 3.1f;
                float localY = (v - 0.43f) * sharkHeight;
                float weight = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.19f, tailRoot, u));
                float offsetX = localX - hingeX;
                float posedX = Mathf.Lerp(localX, hingeX + offsetX * cos - localY * sin, weight);
                float posedY = Mathf.Lerp(localY, offsetX * sin + localY * cos, weight);
                if (Mathf.Abs(curvature) > 0.0001f) {
                    float turn = posedX * curvature;
                    float sectionCos = Mathf.Cos(turn);
                    float sectionSin = Mathf.Sin(turn);
                    posedX = sectionSin / curvature - posedY * sectionSin;
                    posedY = (1 - sectionCos) / curvature + posedY * sectionCos;
                }
                int i = row * (SharkColumns + 1) + col;
                sharkVertices[i] = new Vector3(facing * posedX, posedY, 0);
                sharkUVs[i] = new Vector2((jaw + Mathf.Lerp(0.001f, 0.999f, u)) / 3, v);
            }
        }
        sharkMesh.vertices = sharkVertices;
        sharkMesh.uv = sharkUVs;
        sharkMesh.RecalculateBounds();
    }

    void OnRespawn() { if (!internalRespawn) Cancel(); }

    void RestorePresentation()
    {
        if (effects) effects.SetActive(false);
        if (originalRenderer) originalRenderer.enabled = true;
        if (original) original.enabled = true;
        view.orthographicSize = cameraSize;
        view.transform.position = cameraStart;
        var follow = view.GetComponent<CatCamera>();
        if (follow) follow.Snap();
    }

    public void Cancel()
    {
        if (!Active) return;
        Active = false;
        RestorePresentation();
        cat.SetRecovering(false);
    }

    void OnGUI()
    {
        if (Fade <= 0) return;
        int depth = GUI.depth; Color color = GUI.color;
        GUI.depth = -1000; GUI.color = new Color(0, 0, 0, Fade);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = color; GUI.depth = depth;
    }

    void OnDisable() => Cancel();
    void OnDestroy()
    {
        if (cat) cat.ReturnedToStart -= OnRespawn;
        if (effects) Destroy(effects);
        if (drawings != null) foreach (var drawing in drawings) if (drawing) Destroy(drawing);
        if (splashFrames != null) foreach (var frame in splashFrames) if (frame) Destroy(frame);
        if (sharkMesh) Destroy(sharkMesh);
        if (submerge) Destroy(submerge);
        if (foamMaterial) Destroy(foamMaterial);
    }
}
