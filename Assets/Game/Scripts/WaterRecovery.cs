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
    const float SharkLaunchTime = 0.72f, SharkBiteTime = 1.93f;
    const float SharkSubmergeTime = 2.30f, SharkSecondLaunchTime = 4.35f;
    const float SharkLungeTime = SharkSecondLaunchTime + 0.20f;
    const float SharkBlackoutTime = SharkLungeTime + 0.34f;
    bool HasSharkEncounter => sharkEncounter && shark;
    float FadeStart => HasSharkEncounter ? SharkBlackoutTime - 0.06f : 1.65f;
    float RetryTime => HasSharkEncounter ? SharkBlackoutTime + 0.27f : RespawnTime;
    public float SequenceDuration => HasSharkEncounter ? RetryTime + 0.40f : Duration;
    public bool Active { get; private set; }
    public bool CameraHeld => Active && !respawned;
    public float Elapsed { get; private set; }
    public float Fade => !Active ? 0 : Elapsed < RetryTime
        ? Mathf.InverseLerp(FadeStart, FadeStart + (HasSharkEncounter ? 0.06f : 0.30f), Elapsed)
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
    SpriteRenderer shark;
    Sprite[] sharkFrames;
    MeshRenderer breachingShark;
    Mesh breachMesh;
    Vector3[] breachVertices;
    Vector2[] breachUVs;
    float breachHeight;
    const int BreachColumns = 24;
    LineRenderer[] bubbles;
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
            splashBack.sortingOrder = 2;
            splashFront = new GameObject("Shark splash overlay").AddComponent<SpriteRenderer>();
            splashFront.transform.SetParent(effects.transform, false);
            splashFront.sortingOrder = 3;
        }
        foamMaterial = new Material(Shader.Find("Sprites/Default"));
        for (int i = 0; i < rings.Length; i++) rings[i] = Line("Expanding surface ripple", 49, true);
        for (int i = 0; i < droplets.Length; i++) droplets[i] = Line("Splash droplet", 2, false);
        splash = Line("Breaking shoulder splash", 33, false);
        neckFoam = Line("Water washing over neck", 25, false);
        if (shark) {
            bubbles = new LineRenderer[22];
            for (int i = 0; i < bubbles.Length; i++) bubbles[i] = Line("Shark bubble", 17, false);
        }
    }

    void PrepareShark()
    {
        var texture = Resources.Load<Texture2D>("WaterRecovery/shark-lunge-frames");
        if (!texture) return;

        shark = new GameObject("Shark").AddComponent<SpriteRenderer>();
        shark.transform.SetParent(effects.transform, false);
        shark.sharedMaterial = submerge;
        sharkFrames = new Sprite[3];
        float width = texture.width / 3f;
        for (int i = 0; i < sharkFrames.Length; i++)
            sharkFrames[i] = Sprite.Create(texture, new Rect(i * width, 0, width, texture.height),
                new Vector2(0.5f, 0.42f), width / 1.65f, 0, SpriteMeshType.FullRect);
        PrepareSharkBreach();
    }

    void PrepareSharkBreach()
    {
        var texture = Resources.Load<Texture2D>("WaterRecovery/shark-breach-frames");
        if (!texture) return;

        breachingShark = new GameObject("Shark side-view breach").AddComponent<MeshRenderer>();
        breachingShark.transform.SetParent(effects.transform, false);
        breachingShark.sharedMaterial = submerge;
        breachingShark.sortingOrder = 5;
        var properties = new MaterialPropertyBlock();
        properties.SetTexture("_MainTex", texture);
        breachingShark.SetPropertyBlock(properties);

        breachMesh = new Mesh { name = "Shark breach surface" };
        breachMesh.MarkDynamic();
        breachVertices = new Vector3[2 * (BreachColumns + 1)];
        breachUVs = new Vector2[breachVertices.Length];
        breachHeight = 3.1f * texture.height / (texture.width / 3f);
        var colors = new Color32[breachVertices.Length];
        var triangles = new int[BreachColumns * 6];
        for (int i = 0; i <= BreachColumns; i++) {
            float u = Mathf.Lerp(0.001f, 0.999f, i / (float)BreachColumns) / 3f;
            breachUVs[2 * i] = new Vector2(u, 0);
            breachUVs[2 * i + 1] = new Vector2(u, 1);
            colors[2 * i] = colors[2 * i + 1] = new Color32(255, 255, 255, 255);
            if (i == BreachColumns) break;
            int vertex = i * 2;
            int triangle = i * 6;
            triangles[triangle] = vertex;
            triangles[triangle + 1] = vertex + 1;
            triangles[triangle + 2] = vertex + 2;
            triangles[triangle + 3] = vertex + 2;
            triangles[triangle + 4] = vertex + 1;
            triangles[triangle + 5] = vertex + 3;
        }
        breachMesh.vertices = breachVertices;
        breachMesh.uv = breachUVs;
        breachMesh.colors32 = colors;
        breachMesh.triangles = triangles;
        breachingShark.gameObject.AddComponent<MeshFilter>().sharedMesh = breachMesh;
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
        if (impactBody) {
            impactBody.enabled = Elapsed < 0.14f;
            impactBody.transform.position = impactPosition + foregroundShift - Vector3.up * (Elapsed * 2.3f) * scale;
        }
        submerge.SetFloat("_Surface", visualEntry.y);
        submerge.SetFloat("_SurfaceScale", scale);
        float breachFraming = HasSharkEncounter
            ? Mathf.SmoothStep(0, 1, Mathf.InverseLerp(SharkLaunchTime, 1.26f, Elapsed))
                * (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(2.02f, 2.48f, Elapsed)))
            : 0;
        view.orthographicSize = Mathf.Lerp(cameraSize, (1.65f + 0.85f * breachFraming) * scale, zoom);
        Vector3 close = entry + new Vector3(facing * 0.12f * scale,
            (0.28f + breachFraming * 0.95f - ForegroundInset) * scale, -10);
        float rumble = HasSharkEncounter && Elapsed < SharkSecondLaunchTime
            ? Mathf.InverseLerp(SharkSubmergeTime, SharkSecondLaunchTime, Elapsed) : 0;
        Vector3 shake = new Vector3(Mathf.Sin(Elapsed * 75), Mathf.Sin(Elapsed * 91 + 0.8f), 0);
        view.transform.position = Vector3.Lerp(cameraStart, close, zoom) + shake * (0.065f * scale * rumble);
        PresentShark(visualEntry);
        PresentBubbles(visualEntry, rumble);
        // A small foreground wash overlaps the softened neck edge; the wider
        // expanding rings alone leave the drawing looking detached from the water.
        float wash = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.12f, 0.24f, Elapsed))
            * (1 - Mathf.InverseLerp(1.0f, 1.35f, Elapsed));
        neckFoam.enabled = wash > 0 && reaction.enabled;
        neckFoam.startWidth = neckFoam.endWidth = 0.028f * scale;
        neckFoam.startColor = neckFoam.endColor = new Color(0.74f, 0.90f, 0.86f, wash * 0.7f);
        for (int p = 0; p < 25; p++) {
            float angle = p * Mathf.PI / 24;
            float x = facing * 0.30f + Mathf.Cos(angle) * 0.29f;
            float y = -Mathf.Sin(angle) * 0.035f + Mathf.Sin(x * 19 + Elapsed * 5) * 0.01f;
            neckFoam.SetPosition(p, visualEntry + new Vector3(x, y, -0.13f) * scale);
        }
        bool sharkSplash = HasSharkEncounter && Elapsed >= SharkLaunchTime;
        float splashStart = Elapsed >= SharkSecondLaunchTime ? SharkSecondLaunchTime
            : Elapsed >= SharkSubmergeTime - 0.17f ? SharkSubmergeTime - 0.17f : SharkLaunchTime;
        float splashTime = sharkSplash ? Elapsed - splashStart : Elapsed;
        float splashX = splashStart == SharkLaunchTime ? -1.55f
            : splashStart == SharkSecondLaunchTime ? 0.30f : 0.98f;
        Vector3 splashOrigin = visualEntry + Vector3.right * (sharkSplash ? facing * splashX * scale : 0);
        float crestTime = sharkSplash ? 0.65f : 0.4f;
        float breakAmount = Mathf.Sin(Mathf.Clamp01(splashTime / crestTime) * Mathf.PI);
        PresentSharkSplash(sharkSplash, splashTime, splashOrigin);
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

    void PresentSharkSplash(bool active, float age, Vector3 origin)
    {
        if (!splashFront) return;
        splashFront.enabled = splashBack.enabled = active && age < 0.88f;
        if (!splashFront.enabled) return;

        float frame = Mathf.Lerp(0, 2, Mathf.InverseLerp(0.06f, 0.62f, age));
        int first = Mathf.FloorToInt(frame);
        float blend = frame - first;
        float opacity = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0, 0.07f, age))
            * (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.53f, 0.88f, age)));
        float width = Mathf.Lerp(0.48f, 1.20f, Mathf.InverseLerp(0, 0.68f, age));
        float height = Mathf.Lerp(0.48f, 1.02f, Mathf.InverseLerp(0, 0.12f, age));
        height *= Mathf.Lerp(1f, 0.52f, Mathf.InverseLerp(0.2f, 0.7f, age));
        splashBack.sprite = splashFrames[first];
        splashFront.sprite = splashFrames[Mathf.Min(first + 1, 2)];
        splashBack.color = new Color(1, 1, 1, opacity * (1 - blend));
        splashFront.color = new Color(1, 1, 1, opacity * blend);
        splashBack.transform.position = splashFront.transform.position = origin;
        splashBack.transform.localScale = splashFront.transform.localScale = new Vector3(width, height, 1);
    }

    void PresentBubbles(Vector3 origin, float rumble)
    {
        if (bubbles == null) return;
        float age = Elapsed - SharkSubmergeTime;
        for (int i = 0; i < bubbles.Length; i++) {
            var bubble = bubbles[i];
            float lifetime = 0.35f + (i % 6) * 0.055f;
            float progress = Mathf.Repeat(age - i * 0.039f, lifetime) / lifetime;
            bubble.enabled = rumble > 0 && age >= i * 0.034f && progress < 0.94f;
            if (!bubble.enabled) continue;

            float x = facing * 0.30f + ((i * 17 % 23) / 22f - 0.5f) * 1.6f;
            x += Mathf.Sin(progress * 6 + i) * 0.065f;
            float y = Mathf.Lerp(-0.28f, 0.22f, progress);
            float radius = (0.022f + (i % 5) * 0.010f) * (0.75f + 0.55f * progress);
            float alpha = Mathf.Sin(progress * Mathf.PI) * (0.5f + 0.45f * rumble);
            bubble.startWidth = bubble.endWidth = 0.010f * scale;
            bubble.startColor = bubble.endColor = new Color(0.83f, 0.97f, 0.95f, alpha);
            for (int p = 0; p < 17; p++) {
                float angle = p / 16f * 2 * Mathf.PI;
                bubble.SetPosition(p, origin + new Vector3(x + Mathf.Cos(angle) * radius,
                    y + Mathf.Sin(angle) * radius, -0.1f) * scale);
            }
        }
    }

    void PresentShark(Vector3 visualEntry)
    {
        if (!shark) return;
        bool encounter = HasSharkEncounter;
        bool secondBreach = encounter && Elapsed >= SharkSecondLaunchTime && Elapsed < SharkBlackoutTime;
        shark.enabled = secondBreach;
        PresentSharkBreach(visualEntry, encounter);
        if (!shark.enabled) return;

        Vector3 bite = visualEntry + new Vector3(facing * 0.30f, 0.32f, 0) * scale;
        float lift = Mathf.InverseLerp(SharkSecondLaunchTime, SharkLungeTime, Elapsed);
        float lunge = Mathf.InverseLerp(SharkLungeTime, SharkBlackoutTime, Elapsed);
        Vector3 airborne = bite + Vector3.up * (Mathf.Lerp(-1.1f, 0.55f, lift * lift) * scale);
        float depth = Vector3.Dot(airborne - view.transform.position, view.transform.forward);
        Vector3 screenCenter = view.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, depth));
        shark.transform.position = Vector3.Lerp(airborne, screenCenter, Mathf.SmoothStep(0, 1, lunge));
        shark.sharedMaterial = lunge > 0 ? foamMaterial : submerge;
        shark.sortingOrder = lunge > 0 ? 20 : 5;
        float screenHeight = 2 * view.orthographicSize / scale;
        float fullFrameScale = 3.5f * screenHeight * Mathf.Max(1, view.aspect) / sharkFrames[2].bounds.size.x;
        shark.transform.localScale = Vector3.one * Mathf.Lerp(1, fullFrameScale, lunge * lunge);
        shark.sprite = sharkFrames[lift < 0.48f ? 0 : lunge < 0.16f ? 1 : 2];
    }

    void PresentSharkBreach(Vector3 origin, bool encounter)
    {
        if (!breachingShark) return;
        breachingShark.enabled = encounter && Elapsed >= SharkLaunchTime && Elapsed < SharkSubmergeTime;
        if (!breachingShark.enabled) return;

        bool diving = Elapsed >= SharkBiteTime;
        float travel = diving ? Mathf.InverseLerp(SharkBiteTime, SharkSubmergeTime, Elapsed)
            : Mathf.InverseLerp(SharkLaunchTime, SharkBiteTime, Elapsed);
        float x = diving ? Mathf.Lerp(0.30f, 0.98f, travel) : Mathf.Lerp(-1.55f, 0.30f, travel);
        float y = diving ? Mathf.Lerp(0.29f, -1.4f, travel)
            : Mathf.Lerp(-0.12f, 0.29f, travel) + 10.8f * travel * (1 - travel);
        float pitch = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.25f, 0.86f, travel));
        float angle = diving ? -72f : Mathf.Lerp(74f, -72f, pitch);
        breachingShark.transform.SetPositionAndRotation(origin + new Vector3(facing * x, y, 0) * scale,
            Quaternion.Euler(0, 0, facing * angle));

        // A single arch peaks as the shark turns over and relaxes into the catch.
        // Bend around the jaw to keep the bite aligned with Mika.
        float arch = diving ? 0 : Mathf.Sin(Mathf.PI * Mathf.InverseLerp(0.20f, 0.97f, travel));
        float curvature = -70f * Mathf.Deg2Rad * arch / 3.1f;
        bool curved = Mathf.Abs(curvature) > 0.0001f;
        int jaw = diving || travel < 0.54f ? 0 : travel < 0.72f ? 1 : 2;
        for (int i = 0; i <= BreachColumns; i++) {
            float u = i / (float)BreachColumns;
            float localX = (u - 0.91f) * 3.1f;
            float turn = localX * curvature;
            float sin = Mathf.Sin(turn);
            float cos = Mathf.Cos(turn);
            float centerX = curved ? sin / curvature : localX;
            float centerY = curved ? (1 - cos) / curvature : 0;
            for (int row = 0; row < 2; row++) {
                float localY = (row - 0.43f) * breachHeight;
                int index = 2 * i + row;
                breachVertices[index] = new Vector3(
                    facing * (centerX - localY * sin), centerY + localY * cos, 0);
                breachUVs[index] = new Vector2((jaw + Mathf.Lerp(0.001f, 0.999f, u)) / 3f, row);
            }
        }
        breachMesh.vertices = breachVertices;
        breachMesh.uv = breachUVs;
        breachMesh.RecalculateBounds();
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
        if (sharkFrames != null) foreach (var frame in sharkFrames) if (frame) Destroy(frame);
        if (breachMesh) Destroy(breachMesh);
        if (submerge) Destroy(submerge);
        if (foamMaterial) Destroy(foamMaterial);
    }
}
