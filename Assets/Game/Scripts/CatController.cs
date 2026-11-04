using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public sealed class CatController : MonoBehaviour
{
    // Optional placement frame for the course's fall boundary; motor units stay world-space.
    public Transform fallReference;
    public readonly CatMotion Motion = new CatMotion();
    public bool Grounded { get; private set; }
    public string Pose => Grounded ? (Mathf.Abs(Motion.Velocity.x) > 0.1f ? "Run" : "Idle")
        : body.linearVelocity.y > 0 ? "Jump" : "Fall";
    Rigidbody2D body;
    BoxCollider2D shape;
    readonly RaycastHit2D[] hits = new RaycastHit2D[8];
    int leaveGroundSteps;
    Vector2 supportVelocity;
    Vector2 spawn;
    PhysicsMaterial2D material;
    public event System.Action ReturnedToStart;
    public event System.Action FellOutOfBounds;
    public bool Paused { get; private set; }

    public void PressJump()
    {
        Motion.PressJump();
        // Give the current render frame the takeoff state, without advancing physics.
        // Airborne requests keep the existing coyote/buffer rules in FixedUpdate.
        if (Paused || !Grounded || !Motion.StartBufferedJump(true, body.linearVelocity - supportVelocity)) return;
        supportVelocity = Vector2.zero;
        body.linearVelocity = Motion.Velocity;
        Grounded = false;
        leaveGroundSteps = 2;
    }

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
        Vector2 nextSupportVelocity = Vector2.zero;
        if (leaveGroundSteps == 0 && body.linearVelocity.y - supportVelocity.y <= 0.1f)
            for (int i = 0; i < count; i++) {
                var hit = hits[i];
                if (hit.normal.y <= 0.7f) continue;
                // Casts do not apply one-way collision rules. An overlapping underside is not ground.
                if (hit.collider.usedByEffector && shape.bounds.min.y < hit.collider.bounds.max.y - 0.07f) continue;
                Grounded = true;
                var moving = hit.collider.GetComponent<MovingSupport>();
                if (moving) nextSupportVelocity = moving.Velocity;
                break;
            }
        if (leaveGroundSteps > 0) leaveGroundSteps--;
        var relativeVelocity = body.linearVelocity - supportVelocity;
        body.linearVelocity = Motion.Step(Time.fixedDeltaTime, Grounded, relativeVelocity);
        supportVelocity = Grounded && !Motion.Jumped ? nextSupportVelocity : Vector2.zero;
        body.linearVelocity += supportVelocity;
        if (Motion.Jumped) { Grounded = false; leaveGroundSteps = 2; }
        float fallY = fallReference ? fallReference.InverseTransformPoint(body.position).y : body.position.y;
        if (fallY < -7) {
            if (FellOutOfBounds != null) FellOutOfBounds();
            else ReturnToStart();
        }
    }
    public void Launch(Vector2 velocity)
    {
        supportVelocity = Vector2.zero;
        body.linearVelocity = Motion.Launch(velocity);
        Grounded = false;
        leaveGroundSteps = 2;
    }

    public void ReturnToStart() => RespawnAt(spawn);

    // The course chooses the spawn; the controller always resets motor and physics.
    public void RespawnAt(Vector2 position)
    {
        SetPaused(false);
        Motion.Reset();
        supportVelocity = Vector2.zero;
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
        // Freeze motor timers and preserve held input along with physics momentum.
        // Respawn and focus loss still clear input explicitly.
        body.simulated = !value;
    }
    void OnDisable()
    {
        Motion.Reset();
        supportVelocity = Vector2.zero;
        if (body) { body.linearVelocity = Vector2.zero; body.simulated = true; }
        Paused = false;
    }
    void OnDestroy()
    {
        if (!material) return;
        if (Application.isPlaying) Destroy(material);
        else DestroyImmediate(material);
    }
}
