using UnityEngine;

// Kinematic motion and rider velocity share the physics clock and interpolation.
[DefaultExecutionOrder(-100)]
public sealed class MovingSupport : MonoBehaviour
{
    public CatController Cat;
    public Transform Visual;
    public Vector2 Axis = Vector2.right;
    public float Amplitude = 0.8f;
    public Collider2D RecoveryFloor { get; internal set; }
    public Vector2 Velocity { get; private set; }
    public float Phase => clock;
    Rigidbody2D body;
    Vector3 origin, visualOrigin;
    float clock;
    bool ready;
    void Awake()
    {
        body = gameObject.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
    }
    void Start()
    {
        if (ready) return;
        origin = transform.localPosition;
        if (Visual) visualOrigin = Visual.localPosition;
        ready = true;
    }
    public void ResetPhase()
    {
        if (!ready) return;
        clock = 0; Velocity = Vector2.zero;
        body.linearVelocity = Vector2.zero;
        body.position = transform.parent.TransformPoint(origin);
        transform.localPosition = origin;
        if (Visual) Visual.localPosition = visualOrigin;
        Physics2D.SyncTransforms();
    }
    void LateUpdate()
    {
        if (!ready || !Visual) return;
        Visual.localPosition = visualOrigin + transform.localPosition - origin;
    }
    void FixedUpdate()
    {
        if (!ready || !Cat) return;
        if (Cat.Paused) { body.linearVelocity = Vector2.zero; Velocity = Vector2.zero; return; }
        clock += Time.fixedDeltaTime;
        var offset = (Vector3)(Axis * (Amplitude * Mathf.Sin(clock * Mathf.PI * 2 / 5)));
        Vector2 destination = transform.parent.TransformPoint(origin + offset);
        Velocity = (destination - body.position) / Time.fixedDeltaTime;
        body.MovePosition(destination);
    }
}
