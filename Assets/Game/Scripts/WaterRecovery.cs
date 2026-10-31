using UnityEngine;

// A bounded water-entry presentation. Checkpoint ownership remains with the course.
[DefaultExecutionOrder(50)]
public sealed class WaterRecovery : MonoBehaviour
{
    public const float SurfaceY = -3.25f;
    public const float ForegroundInset = 0.65f;
    public const float RespawnTime = 2.1f, Duration = 2.5f;
    public bool Active { get; private set; }
    public bool CameraHeld => Active && !respawned;
    public float Elapsed { get; private set; }
    public float Fade => !Active ? 0 : Elapsed < 1.65f ? 0 : Elapsed < 1.95f
        ? Mathf.InverseLerp(1.65f, 1.95f, Elapsed) : Elapsed < RespawnTime ? 1
        : 1 - Mathf.InverseLerp(RespawnTime, Duration, Elapsed);
    CatController cat;
    Camera view;
    Transform course;
    Rigidbody2D body;
    System.Action retry;
    MikaSpritePresentation original;
    SpriteRenderer originalRenderer, reaction, impactBody;
    Sprite[] drawings;
    Material submerge, foamMaterial;
    GameObject effects;
    readonly LineRenderer[] rings = new LineRenderer[2];
    readonly LineRenderer[] droplets = new LineRenderer[12];
    LineRenderer splash;
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
        foamMaterial = new Material(Shader.Find("Sprites/Default"));
        for (int i = 0; i < rings.Length; i++) rings[i] = Line("Expanding surface ripple", 49, true);
        for (int i = 0; i < droplets.Length; i++) droplets[i] = Line("Splash droplet", 2, false);
        splash = Line("Breaking shoulder splash", 33, false);
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
        if (!respawned && Elapsed >= RespawnTime) {
            internalRespawn = true;
            try { retry(); } finally { internalRespawn = false; }
            respawned = true;
            RestorePresentation();
            cat.SetRecovering(true);
        }
        if (Elapsed >= Duration) Cancel();
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
        float sink = Elapsed * 0.1f + 0.65f * Mathf.Pow(Mathf.InverseLerp(0.9f, 1.6f, Elapsed), 1.5f);
        reaction.transform.position = visualEntry + new Vector3(facing * 0.30f, 0.40f - sink, 0) * scale;
        reaction.transform.localScale = Vector3.one;
        reaction.flipX = facing < 0;
        int frame = Elapsed < 0.25f ? 0 : Elapsed < 0.34f ? 1 : Elapsed < 0.43f ? 2
            : Elapsed < 0.52f ? 3 : Elapsed < 0.61f ? 4 : Elapsed < 0.85f ? 5 : Elapsed < 0.98f ? 6 : 7;
        reaction.sprite = drawings[frame];
        reaction.enabled = !impactBody || Elapsed >= 0.14f;
        if (impactBody) {
            impactBody.enabled = Elapsed < 0.14f;
            impactBody.transform.position = impactPosition + foregroundShift - Vector3.up * (Elapsed * 2.3f) * scale;
        }
        submerge.SetFloat("_Surface", visualEntry.y);
        view.orthographicSize = Mathf.Lerp(cameraSize, 1.65f * scale, zoom);
        // Follow the face only slightly as it sinks; keep the surface in the close view.
        Vector3 close = entry + new Vector3(facing * 0.12f * scale, (0.28f - ForegroundInset) * scale, -10);
        view.transform.position = Vector3.Lerp(cameraStart, close, zoom);
        float breakAmount = Mathf.Sin(Mathf.Clamp01(Elapsed / 0.4f) * Mathf.PI);
        splash.enabled = Elapsed < 0.4f;
        splash.startWidth = splash.endWidth = (0.035f + breakAmount * 0.11f) * scale;
        splash.startColor = splash.endColor = new Color(0.8f, 0.94f, 0.91f, 1 - Mathf.Clamp01(Elapsed / 0.4f));
        for (int p = 0; p < 33; p++) {
            float x = (p / 32f - 0.5f) * (0.9f + Elapsed * 1.2f);
            float envelope = Mathf.Sin(p / 32f * Mathf.PI);
            float crest = envelope * (0.10f + Mathf.Abs(Mathf.Sin(p * 1.7f)) * 0.22f) * breakAmount;
            splash.SetPosition(p, visualEntry + new Vector3(x, crest, -0.12f) * scale);
        }
        float alpha = 1 - Mathf.Clamp01(Elapsed / 1.75f);
        for (int r = 0; r < rings.Length; r++) {
            float radius = (0.25f + Elapsed * (0.7f + r * 0.25f)) * scale;
            rings[r].startWidth = rings[r].endWidth = 0.022f * scale;
            rings[r].startColor = rings[r].endColor = new Color(0.82f, 0.93f, 0.88f, alpha * 0.8f);
            for (int p = 0; p < 49; p++) {
                float angle = p * Mathf.PI * 2 / 48;
                rings[r].SetPosition(p, visualEntry + new Vector3(Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius * 0.12f, -0.1f));
            }
        }
        for (int i = 0; i < droplets.Length; i++) {
            float t = Mathf.Min(Elapsed, 0.65f);
            float vx = (i - 5.5f) * 0.38f;
            float vy = 2.4f + (i % 3) * 0.45f;
            var position = visualEntry + new Vector3(vx * t, vy * t - 6 * t * t, -0.1f) * scale;
            droplets[i].enabled = Elapsed < 0.65f && position.y >= visualEntry.y;
            droplets[i].startWidth = 0.035f * scale; droplets[i].endWidth = 0.012f * scale;
            droplets[i].startColor = droplets[i].endColor = new Color(0.8f, 0.93f, 0.9f, 1 - t);
            droplets[i].SetPosition(0, position);
            droplets[i].SetPosition(1, position + Vector3.up * 0.065f * scale);
        }
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
        if (submerge) Destroy(submerge);
        if (foamMaterial) Destroy(foamMaterial);
    }
}
