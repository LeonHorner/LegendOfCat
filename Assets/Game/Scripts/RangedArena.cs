using UnityEngine;

[DefaultExecutionOrder(100)]
public sealed class RangedArena : MonoBehaviour
{
    public readonly RangedCombat Combat = new RangedCombat();
    public CatController Cat { get; private set; }
    public RunLifecycle Run { get; private set; }
    public Camera ArenaCamera { get; private set; }
    public System.Action BackToMenu;
    Texture2D texture;
    Sprite square, disc;
    SpriteRenderer target, shield, charge, feedback, playerSprite;
    readonly SpriteRenderer[] projectiles = new SpriteRenderer[RangedCombat.MaximumProjectiles];
    TextMesh targetLabel;
    Rigidbody2D body;
    GUIStyle labelStyle;
    public Vector2 Muzzle => (Vector2)transform.InverseTransformPoint(body.position) + new Vector2(Combat.Facing * .55f, .05f);

    void Awake()
    {
        texture = new Texture2D(64, 64) { filterMode = FilterMode.Bilinear };
        var pixels = new Color[64 * 64];
        for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
            pixels[y * 64 + x] = new Color(1, 1, 1, Mathf.Clamp01((31 - Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f))) * 2));
        texture.SetPixels(pixels); texture.Apply();
        disc = Sprite.Create(texture, new Rect(0, 0, 64, 64), Vector2.one * .5f, 64);
        square = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * .5f, 1);
        Part("Solid floor", new Vector2(0, -.5f), new Vector2(16, 1), new Color(.18f, .24f, .25f), false).gameObject.AddComponent<BoxCollider2D>();
        Part("Landing edge", new Vector2(0, -.025f), new Vector2(16, .05f), new Color(.76f, .8f, .68f), false);
        foreach (float x in new[] { -8.25f, 8.25f })
            Part("Arena wall", new Vector2(x, 3.5f), new Vector2(.5f, 8), new Color(.18f, .24f, .25f), false).gameObject.AddComponent<BoxCollider2D>();
        shield = Part("Closed shield silhouette", Vector2.zero, Vector2.one * 1.3f, new Color(.35f, .43f, .49f), false);
        target = Part("Flying practice target", Vector2.zero, Vector2.one * 1.1f, new Color(1, .86f, .55f), true);
        target.sortingOrder = 2;
        targetLabel = new GameObject("Target vulnerability label").AddComponent<TextMesh>();
        targetLabel.transform.SetParent(transform, false);
        targetLabel.anchor = TextAnchor.MiddleCenter; targetLabel.fontSize = 40; targetLabel.characterSize = .055f;
        feedback = Part("Impact feedback", Vector2.zero, Vector2.one * .35f, Color.white, false);
        feedback.sortingOrder = 5;
        var player = new GameObject("Mika ranged prototype");
        player.transform.SetParent(transform, false); player.transform.localPosition = new Vector2(-4, .5f);
        body = player.AddComponent<Rigidbody2D>();
        player.AddComponent<BoxCollider2D>().size = Vector2.one * .9f;
        Cat = player.AddComponent<CatController>(); Cat.fallReference = transform;
        var visual = Instantiate(Resources.Load<GameObject>("MikaOriginal"), player.transform);
        visual.GetComponent<MikaSpritePresentation>().controller = Cat;
        playerSprite = visual.GetComponent<SpriteRenderer>();
        charge = Part("Charged light orb", Vector2.zero, Vector2.one * .2f, new Color(1, .79f, .35f), true);
        for (int i = 0; i < projectiles.Length; i++) {
            projectiles[i] = Part("Player projectile", Vector2.zero, Vector2.one, Color.white, true);
            projectiles[i].sortingOrder = 4;
        }
        Run = new RunLifecycle(new Vector2(-4, .5f));
        Run.ResetTransientState += Combat.Reset;
        Cat.FellOutOfBounds += Retry;
        var input = player.AddComponent<CatInput>(); input.controller = Cat;
        input.UseCombatReload = true;
        input.PulsePressed = () => Combat.PressPulse(Muzzle);
        input.LobPressed = () => Combat.PressLob();
        input.LobReleased = () => Combat.ReleaseLob(Muzzle);
        input.ReloadPressed = Combat.Reload;
        input.FacingChanged = Combat.SetFacing;
        input.CombatCanceled = Combat.CancelTransient;
        input.RetryCheckpoint = Retry; input.RestartRun = Restart;
        input.TogglePause = TogglePause; input.BackToMenu = () => BackToMenu?.Invoke();
        ArenaCamera = new GameObject("Ranged arena camera").AddComponent<Camera>();
        ArenaCamera.transform.SetParent(transform, false);
        ArenaCamera.transform.localPosition = new Vector3(0, 3.7f, -10);
        ArenaCamera.orthographic = true; ArenaCamera.orthographicSize = 4.8f;
        ArenaCamera.depth = 2; ArenaCamera.clearFlags = CameraClearFlags.SolidColor;
        ArenaCamera.backgroundColor = new Color(.07f, .12f, .16f);
        if (!FindFirstObjectByType<AudioListener>()) ArenaCamera.gameObject.AddComponent<AudioListener>();
        Present(); LateUpdate();
    }
    SpriteRenderer Part(string name, Vector2 position, Vector2 size, Color color, bool round)
    {
        var item = new GameObject(name); item.transform.SetParent(transform, false);
        item.transform.localPosition = position; item.transform.localScale = size;
        var renderer = item.AddComponent<SpriteRenderer>();
        renderer.sprite = round ? disc : square; renderer.color = color;
        return renderer;
    }
    void FixedUpdate()
    {
        if (!Combat.Finished) Run.Advance(Time.fixedDeltaTime);
        Combat.Advance(Time.fixedDeltaTime);
        if (Combat.Finished) Cat.SetPaused(true);
        Present();
    }
    public void Retry() { Cat.RespawnAt(transform.TransformPoint(Run.Retry())); Present(); }
    public void Restart() { Cat.RespawnAt(transform.TransformPoint(Run.Restart())); Present(); }
    public void TogglePause()
    {
        if (Combat.Finished) return;
        bool paused = Run.TogglePause(); Combat.SetPaused(paused); Cat.SetPaused(paused);
    }
    void Present()
    {
        target.transform.localPosition = shield.transform.localPosition = Combat.Target;
        target.enabled = !Combat.Finished;
        target.color = Combat.HitFlash > 0 ? Color.white : new Color(1, .86f, .55f);
        shield.enabled = !Combat.Vulnerable && !Combat.Finished;
        targetLabel.transform.localPosition = Combat.Target + Vector2.up * .95f;
        targetLabel.text = Combat.Finished ? "TARGET DEFEATED" : Combat.BlockFlash > 0 ? "BLOCKED" : Combat.HitFlash > 0 ? "HIT!" : Combat.Vulnerable ? "OPEN" : "SHIELDED";
        charge.enabled = Combat.Charging;
        charge.transform.localPosition = Muzzle;
        charge.transform.localScale = Vector3.one * Mathf.Lerp(.15f, .48f, Combat.Charge);
        feedback.enabled = Combat.HitFlash > 0 || Combat.BlockFlash > 0;
        feedback.transform.localPosition = Combat.LastContact;
        feedback.transform.localRotation = Quaternion.Euler(0, 0, Combat.BlockFlash > 0 ? 45 : 0);
        feedback.color = Combat.BlockFlash > 0 ? new Color(.65f, .7f, .75f) : Color.white;
        for (int i = 0; i < projectiles.Length; i++) {
            var renderer = projectiles[i]; renderer.enabled = i < Combat.Projectiles.Count;
            if (!renderer.enabled) continue;
            var shot = Combat.Projectiles[i];
            renderer.transform.localPosition = shot.Position;
            renderer.transform.localScale = shot.Lob ? Vector3.one * .28f : new Vector3(.42f, .13f, 1);
            renderer.color = shot.Lob ? new Color(1, .8f, .35f) : new Color(.7f, .93f, 1);
        }
    }
    void LateUpdate()
    {
        ArenaCamera.rect = MenuLayout.Letterbox16By9(Screen.width, Screen.height); ArenaCamera.aspect = 16f / 9f;
        playerSprite.flipX = Combat.Facing < 0;
    }
    void OnGUI()
    {
        if (!ArenaCamera) return;
        if (labelStyle == null) labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, wordWrap = true, margin = new RectOffset(2, 2, 0, 0) };
        float width = Mathf.Max(1, Mathf.Min(440, Screen.width - 16));
        var area = new Rect((Screen.width - width) * .5f, Screen.height - 40, width, 40);
        GUI.Box(area, GUIContent.none);
        GUI.BeginGroup(area);
        float slotWidth = (width - 20) * .5f;
        DrawWeaponHud(RangedCombat.Weapon.Pulse, new Rect(8, 2, slotWidth, 36));
        DrawWeaponHud(RangedCombat.Weapon.Orb, new Rect(12 + slotWidth, 2, slotWidth, 36));
        GUI.EndGroup();
    }
    void DrawWeaponHud(RangedCombat.Weapon weapon, Rect slot)
    {
        bool orb = weapon == RangedCombat.Weapon.Orb;
        bool recent = Combat.ActiveWeapon == weapon;
        bool reloading = recent && Combat.Reloading;
        Color previous = GUI.color;
        GUI.color = orb ? new Color(1, .8f, .35f) : new Color(.7f, .93f, 1);
        if (orb) GUI.DrawTexture(new Rect(slot.x + 4, slot.y, 12, 12), texture);
        else {
            GUI.DrawTexture(new Rect(slot.x + 5, slot.y + 7, 10, 8), texture);
            GUI.DrawTexture(new Rect(slot.x + 1, slot.y + 3, 5, 6), texture);
            GUI.DrawTexture(new Rect(slot.x + 5, slot.y - 1, 5, 6), texture);
            GUI.DrawTexture(new Rect(slot.x + 11, slot.y, 5, 6), texture);
            GUI.DrawTexture(new Rect(slot.x + 15, slot.y + 4, 5, 6), texture);
            GUI.color = new Color(1, .89f, .65f);
            GUI.DrawTexture(new Rect(slot.x + 19, slot.y - 1, 1, 5), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(slot.x + 17, slot.y + 1, 5, 1), Texture2D.whiteTexture);
        }
        GUI.color = previous;
        string name = orb ? "ORB  K / RMB" : "PAW  J / LMB";
        string status = reloading ? "  Reload " + Mathf.RoundToInt(100 * (1 - Combat.ReloadRemaining / Combat.ReloadDuration)) + "%"
            : orb && Combat.Charging ? "  Charge " + Mathf.RoundToInt(100 * Combat.Charge) + "%" : "";
        GUI.Label(new Rect(slot.x + 26, slot.y - 1, slot.width - 26, 18), name + status, labelStyle);
        int capacity = orb ? RangedCombat.OrbCapacity : RangedCombat.PulseCapacity;
        for (int i = 0; i < capacity; i++) {
            var dot = new Rect(slot.x + i * 14, slot.y + 20, 10, 10);
            GUI.color = new Color(1, .89f, .65f);
            GUI.DrawTexture(dot, texture);
            if (i >= Combat.AmmoFor(weapon)) {
                GUI.color = new Color(.07f, .12f, .16f);
                GUI.DrawTexture(new Rect(dot.x + 1.5f, dot.y + 1.5f, 7, 7), texture);
            }
        }
        if (reloading) {
            var track = new Rect(slot.x, slot.y + 33, slot.width - 4, 2);
            GUI.color = new Color(.35f, .43f, .49f); GUI.DrawTexture(track, Texture2D.whiteTexture);
            GUI.color = new Color(1, .89f, .65f);
            track.width *= Mathf.Clamp01(1 - Combat.ReloadRemaining / Combat.ReloadDuration);
            GUI.DrawTexture(track, Texture2D.whiteTexture);
        }
        GUI.color = previous;
    }
    void OnDisable() => Combat.CancelTransient();
    void OnDestroy()
    {
        if (Application.isPlaying) { Destroy(square); Destroy(disc); Destroy(texture); }
        else { DestroyImmediate(square); DestroyImmediate(disc); DestroyImmediate(texture); }
    }
}
