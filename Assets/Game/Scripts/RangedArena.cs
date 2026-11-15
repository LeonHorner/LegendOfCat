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
    SpriteRenderer target, shield, charge, facing, feedback, playerSprite;
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
        facing = Part("Facing indicator", Vector2.zero, new Vector2(.3f, .07f), new Color(.7f, .9f, 1), false);
        for (int i = 0; i < projectiles.Length; i++) {
            projectiles[i] = Part("Player projectile", Vector2.zero, Vector2.one, Color.white, true);
            projectiles[i].sortingOrder = 4;
        }
        Run = new RunLifecycle(new Vector2(-4, .5f));
        Run.ResetTransientState += Combat.Reset;
        Cat.FellOutOfBounds += Retry;
        var input = player.AddComponent<CatInput>(); input.controller = Cat;
        input.UseCombatReload = true;
        input.AttackPressed = () => Combat.PressAttack(Muzzle);
        input.AttackReleased = () => Combat.ReleaseAttack(Muzzle);
        input.WeaponSwitched = Combat.SwitchWeapon;
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
        facing.transform.localPosition = Muzzle;
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
        float width = Mathf.Max(1, Mathf.Min(360, Screen.width - 16));
        GUILayout.BeginArea(new Rect((Screen.width - width) * .5f, Screen.height - 38, width, 34), GUI.skin.box);
        string weapon = Combat.ActiveWeapon == RangedCombat.Weapon.Pulse ? "PULSE" : "LIGHT ORB";
        string status = Combat.Reloading ? " | Reload: " + Mathf.CeilToInt(100 * (1 - Combat.ReloadRemaining / Combat.ReloadDuration)) + "%"
            : Combat.Charging ? " | Charge: " + Mathf.RoundToInt(Combat.Charge * 100) + "%" : "";
        GUILayout.BeginHorizontal();
        Color hudColor = GUI.color;
        bool orbSelected = Combat.ActiveWeapon == RangedCombat.Weapon.Orb;
        var icon = GUILayoutUtility.GetRect(18, 20, GUILayout.Width(18), GUILayout.Height(20));
        GUI.color = orbSelected ? new Color(1, .8f, .35f) : new Color(.7f, .93f, 1);
        GUI.DrawTexture(orbSelected ? new Rect(icon.x + 3, icon.y + 2, 12, 12)
            : new Rect(icon.x, icon.y + 6, 18, 5), texture);
        GUILayout.Label(weapon, labelStyle, GUILayout.Width(70));
        GUI.color = hudColor;
        var rounds = GUILayoutUtility.GetRect(Combat.Capacity * 16, 20, GUILayout.Width(Combat.Capacity * 16), GUILayout.Height(20));
        Color previousColor = GUI.color;
        for (int i = 0; i < Combat.Capacity; i++) {
            var dot = new Rect(rounds.x + i * 16, rounds.y + 2, 12, 12);
            GUI.color = new Color(1, .89f, .65f);
            GUI.DrawTexture(dot, texture);
            if (i >= Combat.Ammo) {
                GUI.color = new Color(.07f, .12f, .16f);
                GUI.DrawTexture(new Rect(dot.x + 1.5f, dot.y + 1.5f, 9, 9), texture);
            }
        }
        if (Combat.Reloading) {
            var track = new Rect(rounds.x, rounds.y + 17, Combat.Capacity * 16 - 4, 2);
            GUI.color = new Color(.35f, .43f, .49f);
            GUI.DrawTexture(track, Texture2D.whiteTexture);
            GUI.color = new Color(1, .89f, .65f);
            track.width *= Mathf.Clamp01(1 - Combat.ReloadRemaining / Combat.ReloadDuration);
            GUI.DrawTexture(track, Texture2D.whiteTexture);
        }
        GUI.color = previousColor;
        GUILayout.Label(status, labelStyle);
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
    void OnDisable() => Combat.CancelTransient();
    void OnDestroy()
    {
        if (Application.isPlaying) { Destroy(square); Destroy(disc); Destroy(texture); }
        else { DestroyImmediate(square); DestroyImmediate(disc); DestroyImmediate(texture); }
    }
}
