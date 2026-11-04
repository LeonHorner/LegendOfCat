using UnityEngine;

// Carries only a standing rider. Jump velocity remains owned by CatMotion.
[DefaultExecutionOrder(-100)]
public sealed class MovingSupport : MonoBehaviour
{
    public CatController Cat;
    public Transform Visual;
    public Vector2 Axis = Vector2.right;
    public float Amplitude = 0.8f;
    Vector3 origin, visualOrigin, previousOffset, currentOffset;
    float clock;
    bool ready;
    public float Phase => clock;
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
        clock = 0;
        previousOffset = currentOffset = Vector3.zero;
        transform.localPosition = origin;
        if (Visual) Visual.localPosition = visualOrigin;
        Physics2D.SyncTransforms();
    }
    void LateUpdate()
    {
        if (!ready || !Visual || Cat && Cat.Paused) return;
        float blend = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
        Visual.localPosition = visualOrigin + Vector3.Lerp(previousOffset, currentOffset, blend);
    }
    void FixedUpdate()
    {
        if (!ready || !Cat || Cat.Paused) return;
        var body = Cat.GetComponent<Rigidbody2D>();
        var bounds = GetComponent<BoxCollider2D>().bounds;
        var feet = Cat.GetComponent<BoxCollider2D>().bounds;
        bool riding = Cat.Grounded && body.linearVelocity.y <= 0.1f &&
            Mathf.Abs(feet.min.y - bounds.max.y) < 0.12f &&
            feet.max.x > bounds.min.x && feet.min.x < bounds.max.x;
        clock += Time.fixedDeltaTime;
        var offset = (Vector3)(Axis * (Amplitude * Mathf.Sin(clock * Mathf.PI * 2 / 5)));
        previousOffset = currentOffset; currentOffset = offset;
        var previous = transform.position;
        transform.localPosition = origin + offset;
        if (Visual) Visual.localPosition = visualOrigin + offset;
        if (riding) body.position += (Vector2)(transform.position - previous);
        Physics2D.SyncTransforms();
    }
}
