using UnityEngine;

// Runs after the motor; each physics sample uses the same state as its visible warnings.
[DefaultExecutionOrder(100)]
public sealed class StompArena : MonoBehaviour
{
    public readonly StompCombat Combat = new StompCombat();
    public CatController Cat { get; private set; }
    public RunLifecycle Run { get; private set; }
    public System.Action BackToMenu;
    public Camera ArenaCamera { get; private set; }
    Sprite square;
    Texture2D texture;
    SpriteRenderer boss, head, warning, impact;
    SpriteRenderer playerSprite;
    Color playerColor;
    TextMesh phaseLabel;
    readonly System.Collections.Generic.List<SpriteRenderer> stripes = new System.Collections.Generic.List<SpriteRenderer>();
    Rigidbody2D body;
    Vector2 previous;
    float flash, damageFlash, previousTargetX;
    Material damageMaterial;
    AudioSource damageAudio;
    Mesh burstMesh;
    Material burstCream, burstBrown;
    Transform damageBurst;
    MeshRenderer burstFill, burstOutline;
    readonly MeshRenderer[] sparks = new MeshRenderer[3];
    float burstTime;
    public const float DamageBurstDuration = .15f;
    static readonly Rect BossBounds = new Rect(-0.85f, 0, 1.7f, 1.35f);
    static readonly Rect Hazard = new Rect(-2.8f, 0, 5.6f, 0.35f);
    static readonly Vector2 Half = new Vector2(0.45f, 0.45f);

    void Awake()
    {
        texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white); texture.Apply();
        square = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1);
        Block("Arena floor", new Vector2(0, -0.5f), new Vector2(16, 1), new Color(.2f, .25f, .28f)).gameObject.AddComponent<BoxCollider2D>();
        Block("Landing edge", new Vector2(0, -.03f), new Vector2(16, .06f), new Color(.8f, .83f, .75f));
        foreach (float x in new[] { -8.25f, 8.25f })
            Block("Arena boundary", new Vector2(x, 2), new Vector2(.5f, 5), new Color(.25f, .3f, .33f)).gameObject.AddComponent<BoxCollider2D>();
        boss = Block("Target body / contact damage", new Vector2(0, .675f), new Vector2(1.7f, 1.35f), Color.gray);
        head = Block("Head vulnerability marker", new Vector2(0, 1.35f), new Vector2(1.7f, .12f), Color.white);
        warning = Block("Ground attack extent", new Vector2(0, .175f), new Vector2(5.6f, .35f), Color.white);
        warning.sortingOrder = 2;
        for (int i = 0; i < 12; i++) {
            var stripe = Block("Warning stripe", new Vector2(-2.6f + i * .47f, .175f), new Vector2(.09f, .35f), Color.white);
            stripe.sortingOrder = 3;
            stripes.Add(stripe);
        }
        impact = Block("Impact flash", Vector2.zero, Vector2.one * .3f, Color.white);
        impact.sortingOrder = 5;
        phaseLabel = new GameObject("Phase label").AddComponent<TextMesh>();
        phaseLabel.transform.SetParent(transform, false);
        phaseLabel.transform.localPosition = new Vector3(0, 2.1f);
        phaseLabel.anchor = TextAnchor.MiddleCenter;
        phaseLabel.fontSize = 36; phaseLabel.characterSize = .09f;
        var player = new GameObject("Mika stomp prototype");
        player.transform.SetParent(transform, false);
        player.transform.localPosition = new Vector3(-4, .5f);
        body = player.AddComponent<Rigidbody2D>();
        player.AddComponent<BoxCollider2D>().size = Half * 2;
        Cat = player.AddComponent<CatController>();
        Cat.fallReference = transform;
        var visual = Instantiate(Resources.Load<GameObject>("MikaOriginal"), player.transform);
        visual.GetComponent<MikaSpritePresentation>().controller = Cat;
        playerSprite = visual.GetComponent<SpriteRenderer>();
        playerColor = playerSprite.color;
        damageMaterial = new Material(Resources.Load<Shader>("StompDamage"));
        playerSprite.sharedMaterial = damageMaterial;
        damageAudio = player.AddComponent<AudioSource>();
        damageAudio.playOnAwake = false;
        damageAudio.loop = false;
        damageAudio.spatialBlend = 0;
        damageAudio.volume = .5f;
        damageAudio.clip = Resources.Load<AudioClip>("StompDamageVoice");
        CreateDamageBurst();
        Run = new RunLifecycle(new Vector2(-4, .5f));
        Run.ResetTransientState += () => { Combat.Reset(); flash = damageFlash = burstTime = previousTargetX = 0; damageAudio.Stop(); };
        Cat.FellOutOfBounds += Retry;
        var input = player.AddComponent<CatInput>();
        input.controller = Cat;
        input.RetryCheckpoint = Retry;
        input.RestartRun = Restart;
        input.TogglePause = TogglePause;
        input.BackToMenu = () => BackToMenu?.Invoke();
        ArenaCamera = new GameObject("Stomp arena camera").AddComponent<Camera>();
        ArenaCamera.transform.SetParent(transform, false);
        ArenaCamera.transform.localPosition = new Vector3(0, 2.4f, -10);
        ArenaCamera.orthographic = true; ArenaCamera.orthographicSize = 5;
        ArenaCamera.depth = 2;
        if (!FindFirstObjectByType<AudioListener>()) ArenaCamera.gameObject.AddComponent<AudioListener>();
        ArenaCamera.backgroundColor = new Color(.08f, .11f, .15f);
        previous = transform.InverseTransformPoint(body.position);
        Present();
    }
    SpriteRenderer Block(string name, Vector2 position, Vector2 size, Color color)
    {
        var item = new GameObject(name);
        item.transform.SetParent(transform, false);
        item.transform.localPosition = position; item.transform.localScale = size;
        var renderer = item.AddComponent<SpriteRenderer>();
        renderer.sprite = square; renderer.color = color;
        return renderer;
    }
    bool SweptContact(Rect rect, Vector2 current, out Vector2 contact)
    {
        var bounds = new Bounds(new Vector3(rect.center.x, rect.center.y, 0),
            new Vector3(rect.width + Half.x * 2, rect.height + Half.y * 2, 2));
        Vector2 before = previous - Vector2.right * previousTargetX;
        current -= Vector2.right * Combat.TargetX;
        Vector2 delta = current - before;
        float fraction = 0;
        contact = Vector2.zero;
        if (!bounds.Contains(before)) {
            if (delta.sqrMagnitude == 0 || !bounds.IntersectRay(new Ray(before, delta.normalized), out float distance)
                || distance > delta.magnitude) return false;
            fraction = distance / delta.magnitude;
        }
        Vector2 center = Vector2.Lerp(before, current, fraction);
        contact = new Vector2(Mathf.Clamp(center.x, rect.xMin, rect.xMax), Mathf.Clamp(center.y, rect.yMin, rect.yMax));
        if (rect.Contains(center)) {
            float nearest = Mathf.Min(center.x - rect.xMin, rect.xMax - center.x, center.y - rect.yMin, rect.yMax - center.y);
            if (nearest == center.x - rect.xMin) contact.x = rect.xMin;
            else if (nearest == rect.xMax - center.x) contact.x = rect.xMax;
            else if (nearest == center.y - rect.yMin) contact.y = rect.yMin;
            else contact.y = rect.yMax;
        }
        contact.x += Mathf.Lerp(previousTargetX, Combat.TargetX, fraction);
        return true;
    }
    void FixedUpdate()
    {
        if (Run.Paused) return;
        damageFlash = Mathf.Max(0, damageFlash - Time.fixedDeltaTime);
        burstTime = Mathf.Max(0, burstTime - Time.fixedDeltaTime);
        if (Combat.Finished) { Present(); return; }
        Vector2 current = transform.InverseTransformPoint(body.position);
        bool top = StompCombat.CrossedHead(previous - Vector2.right * previousTargetX, current - Vector2.right * Combat.TargetX, Half, BossBounds);
        // Resolve the completed simulation against the state displayed for that step.
        if (Combat.TryStomp(top)) {
            current.y = BossBounds.yMax + Half.y + .02f;
            body.position = transform.TransformPoint(current);
            Cat.Launch(new Vector2(body.linearVelocity.x, 10));
            impact.transform.localPosition = new Vector3(current.x, BossBounds.yMax);
            flash = .16f;
        } else {
            bool bodyHit = SweptContact(BossBounds, current, out Vector2 contact);
            bool groundHit = false;
            if (!bodyHit && Combat.Phase == StompCombat.AttackPhase.Attack)
                groundHit = SweptContact(Hazard, current, out contact);
            if ((bodyHit || groundHit) && Combat.TryDamage()) {
                Cat.Launch(new Vector2(body.linearVelocity.x, 7));
                damageFlash = .08f;
                burstTime = DamageBurstDuration;
                damageBurst.localPosition = contact;
                if (Application.isPlaying) damageAudio.Play();
            }
        }
        previous = current;
        if (Combat.Finished) Cat.SetPaused(true);
        previousTargetX = Combat.TargetX;
        Combat.Advance(Time.fixedDeltaTime);
        Run.Advance(Time.fixedDeltaTime);
        flash = Mathf.Max(0, flash - Time.fixedDeltaTime);
        Present();
    }
    public void Retry() { Cat.RespawnAt(transform.TransformPoint(Run.Retry())); previous = transform.InverseTransformPoint(body.position); Present(); }
    public void Restart() { Cat.RespawnAt(transform.TransformPoint(Run.Restart())); previous = transform.InverseTransformPoint(body.position); Present(); }
    public void TogglePause()
    {
        if (Combat.Finished) return;
        Cat.SetPaused(Run.TogglePause());
        if (Run.Paused) damageAudio.Pause();
        else damageAudio.UnPause();
    }
    void Present()
    {
        float x = Combat.TargetX;
        boss.transform.localPosition = new Vector3(x, .675f);
        head.transform.localPosition = new Vector3(x, 1.35f);
        warning.transform.localPosition = new Vector3(x, .175f);
        phaseLabel.transform.localPosition = new Vector3(x, 2.1f);
        for (int i = 0; i < stripes.Count; i++) stripes[i].transform.localPosition = new Vector3(x - 2.6f + i * .47f, .175f);
        damageMaterial.SetFloat("_Flash", .35f * damageFlash / .08f);
        PresentDamageBurst();
        var color = playerColor;
        if (!Combat.Finished && Combat.Invulnerability > 0) {
            float elapsed = StompCombat.DamageProtection - Combat.Invulnerability;
            color.a *= .75f + .25f * Mathf.Cos(elapsed * Mathf.PI * 2 / .25f);
        }
        playerSprite.color = color;
        bool open = Combat.Phase == StompCombat.AttackPhase.Recovery;
        bool active = Combat.Phase == StompCombat.AttackPhase.Attack;
        boss.enabled = head.enabled = Combat.BossHealth > 0;
        boss.color = active ? new Color(.9f, .3f, .23f) : new Color(.4f, .46f, .5f);
        head.color = open ? Color.white : new Color(.2f, .23f, .25f);
        warning.enabled = !Combat.Finished && !open;
        warning.color = active ? new Color(1, .35f, .2f) : new Color(.7f, .65f, .4f, .35f);
        foreach (var stripe in stripes) stripe.enabled = warning.enabled && !active;
        impact.enabled = flash > 0;
        phaseLabel.text = Combat.Finished ? (Combat.BossHealth == 0 ? "TARGET DEFEATED" : "TRY AGAIN")
            : open ? "OPEN HEAD / JUMP NOW" : active ? "ATTACK / GROUND UNSAFE" : "WARNING / GROUND ATTACK";
    }
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(12, 12, Mathf.Min(620, Screen.width - 24), 175), GUI.skin.box);
        GUILayout.Label("STOMP LAB / provisional rules for review");
        GUILayout.Label("Move and jump with the shared controls. Descend onto the white head during recovery.");
        GUILayout.Label("Dark head and sides hurt. Striped ground warning becomes a solid active attack.");
        GUILayout.Label("Mika " + Combat.PlayerHealth + "/3 | Target " + Combat.BossHealth + "/3" +
            (Combat.Invulnerability > 0 ? " | DAMAGE PROTECTION" : "") + (Run.Paused ? " | PAUSED" : ""));
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Retry (R)")) Retry();
        if (GUILayout.Button("Restart (Backspace)")) Restart();
        if (GUILayout.Button(Run.Paused ? "Resume (P)" : "Pause (P)")) TogglePause();
        if (GUILayout.Button("Menu (Esc)")) BackToMenu?.Invoke();
        GUILayout.EndHorizontal(); GUILayout.EndArea();
    }
    void CreateDamageBurst()
    {
        burstMesh = new Mesh { name = "Original six-point contact star" };
        burstCream = new Material(Shader.Find("Sprites/Default"));
        burstBrown = new Material(Shader.Find("Sprites/Default"));
        var vertices = new Vector3[13];
        var triangles = new int[36];
        // Uneven six-point outline: original geometry, not a borrowed explosion sprite.
        for (int i = 0; i < 12; i++) {
            float angle = (i * 30 + 9) * Mathf.Deg2Rad;
            float radius = i % 2 == 0 ? .48f - (i % 3) * .025f : .19f;
            vertices[i + 1] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = (i + 1);
            triangles[i * 3 + 2] = ((i + 1) % 12 + 1);
        }
        burstMesh.vertices = vertices;
        burstMesh.triangles = triangles;
        burstMesh.uv = new Vector2[vertices.Length];
        burstMesh.RecalculateBounds();
        damageBurst = new GameObject("Damage contact star").transform;
        damageBurst.SetParent(transform, false);
        burstOutline = BurstPart("Warm outline", 6);
        burstOutline.sharedMaterial = burstBrown;
        burstOutline.transform.localScale = Vector3.one * 1.12f;
        burstFill = BurstPart("Cream impact star", 7);
        for (int i = 0; i < sparks.Length; i++) sparks[i] = BurstPart("Small impact spark", 7);
    }
    MeshRenderer BurstPart(string name, int order)
    {
        var part = new GameObject(name);
        part.transform.SetParent(damageBurst, false);
        part.AddComponent<MeshFilter>().sharedMesh = burstMesh;
        var renderer = part.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = burstCream;
        renderer.sortingOrder = order;
        return renderer;
    }
    void PresentDamageBurst()
    {
        damageBurst.gameObject.SetActive(burstTime > 0);
        if (burstTime <= 0) return;
        float t = 1 - burstTime / DamageBurstDuration;
        float alpha = 1 - t * t;
        damageBurst.localScale = Vector3.one * Mathf.Lerp(.38f, .66f, Mathf.Sqrt(t));
        burstCream.color = new Color(1, .91f, .68f, alpha);
        burstBrown.color = new Color(.48f, .31f, .16f, alpha * .8f);
        for (int i = 0; i < sparks.Length; i++) {
            float angle = (i * 125 + 25) * Mathf.Deg2Rad;
            sparks[i].transform.localPosition = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * Mathf.Lerp(.34f, .68f, t);
            sparks[i].transform.localScale = Vector3.one * Mathf.Lerp(.14f, .04f, t);
        }
    }
    void OnDisable()
    {
        if (damageAudio) damageAudio.Stop();
    }
    void OnDestroy()
    {
        if (Application.isPlaying) { Destroy(square); Destroy(texture); Destroy(damageMaterial); Destroy(burstMesh); Destroy(burstCream); Destroy(burstBrown); }
        else { DestroyImmediate(square); DestroyImmediate(texture); DestroyImmediate(damageMaterial); DestroyImmediate(burstMesh); DestroyImmediate(burstCream); DestroyImmediate(burstBrown); }
    }
}
