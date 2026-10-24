using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public sealed class CatController : MonoBehaviour
{
    public readonly CatMotion Motion = new CatMotion();
    public bool Grounded { get; private set; }
    public string Pose => Grounded ? (Mathf.Abs(body.linearVelocity.x) > 0.1f ? "Run" : "Idle")
        : body.linearVelocity.y > 0 ? "Jump" : "Fall";
    Rigidbody2D body;
    BoxCollider2D shape;
    readonly RaycastHit2D[] hits = new RaycastHit2D[8];
    int leaveGroundSteps;
    Vector2 spawn;
    PhysicsMaterial2D material;
    public event System.Action ReturnedToStart;
    public event System.Action FellOutOfBounds;
    public bool Paused { get; private set; }

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        shape = GetComponent<BoxCollider2D>();
        spawn = body.position;
        body.gravityScale = 0;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        material = new PhysicsMaterial2D("Cat frictionless contact") { friction = 0, bounciness = 0 };
        shape.sharedMaterial = material;
    }
    void FixedUpdate()
    {
        if (Paused) return;
        Grounded = false;
        var filter = new ContactFilter2D { useTriggers = false };
        var count = shape.Cast(Vector2.down, filter, hits, 0.04f);
        if (leaveGroundSteps == 0 && body.linearVelocity.y <= 0.1f)
            for (int i = 0; i < count; i++)
                if (hits[i].normal.y > 0.7f) Grounded = true;
        if (leaveGroundSteps > 0) leaveGroundSteps--;
        body.linearVelocity = Motion.Step(Time.fixedDeltaTime, Grounded, body.linearVelocity);
        if (Motion.Jumped) { Grounded = false; leaveGroundSteps = 2; }
        if (body.position.y < -7) {
            if (FellOutOfBounds != null) FellOutOfBounds();
            else ReturnToStart();
        }
    }
    public void ReturnToStart() => RespawnAt(spawn);

    // The course chooses the spawn; the controller always resets motor and physics.
    public void RespawnAt(Vector2 position)
    {
        SetPaused(false);
        Motion.Reset();
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0;
        body.position = position;
        transform.position = position;
        Grounded = false;
        leaveGroundSteps = 0;
        ReturnedToStart?.Invoke();
    }

    public void SetPaused(bool value)
    {
        if (Paused == value) return;
        Paused = value;
        Motion.Reset();
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0;
        body.simulated = !value;
    }
    void OnDisable()
    {
        Motion.Reset();
        if (body) { body.linearVelocity = Vector2.zero; body.simulated = true; }
        Paused = false;
    }
    void OnDestroy() { if (material) Destroy(material); }
}
