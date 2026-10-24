using UnityEngine;

// Cosmetic child only: never move the controller or change its collision geometry.
[RequireComponent(typeof(SpriteRenderer))]
public sealed class MikaSpritePresentation : MonoBehaviour
{
    public CatController controller;
    public Sprite[] frames;
    public readonly MikaAnimation Animation = new MikaAnimation();
    SpriteRenderer target;
    Vector3 previousPosition;
    BoxCollider2D shape;
    readonly RaycastHit2D[] contacts = new RaycastHit2D[8];
    float groundCorrection;
    const float MaximumCorrection = 0.04f;

    void OnEnable()
    {
        target = GetComponent<SpriteRenderer>();
        if (!controller) controller = GetComponentInParent<CatController>();
        if (controller) {
            shape = controller.GetComponent<BoxCollider2D>();
            controller.ReturnedToStart += ResetPose;
            ResetPose();
        }
    }

    void ResetPose()
    {
        Animation.Reset();
        previousPosition = controller.transform.position;
        groundCorrection = 0;
        ApplyFrame();
        AlignPaws(0, true);
    }

    void AlignPaws(float deltaTime, bool snap = false)
    {
        if (!shape) return;
        float floor = shape.transform.TransformPoint(shape.offset - Vector2.up * shape.size.y * 0.5f).y;
        float correction = 0;
        if (controller.Grounded) {
            int count = shape.Cast(Vector2.down, new ContactFilter2D { useTriggers = false }, contacts, MaximumCorrection);
            float nearest = float.PositiveInfinity;
            for (int i = 0; i < count; i++) {
                // A box cast may report a distant slope corner or platform side.
                // Only correct contact skin near the actual collider sole.
                float offset = contacts[i].point.y - floor;
                if (contacts[i].normal.y > 0.7f && Mathf.Abs(offset) <= MaximumCorrection
                    && contacts[i].distance < nearest) {
                    nearest = contacts[i].distance;
                    correction = offset;
                }
            }
        }
        groundCorrection = snap ? correction : Mathf.MoveTowards(groundCorrection, correction,
            0.4f * Mathf.Max(0, deltaTime));
        var position = transform.position;
        // Airborne and unsupported frames return continuously to the collider anchor.
        // Do not force a rigid drawing to conform to a slope by sinking the cat.
        position.y = floor + groundCorrection;
        transform.position = position;
    }

    void LateUpdate() => RenderPose(Time.deltaTime);

    public void RenderPose(float deltaTime)
    {
        if (!controller) return;
        Vector3 position = controller.transform.position;
        Animation.LandingSoon = false;
        if (shape && !controller.Grounded && controller.Motion.Velocity.y < 0) {
            int count = shape.Cast(Vector2.down, new ContactFilter2D { useTriggers = false }, contacts, 0.45f);
            for (int i = 0; i < count; i++)
                if (contacts[i].normal.y > 0.7f) Animation.LandingSoon = true;
        }
        Animation.Step(deltaTime, controller.Grounded, controller.Motion.Velocity,
            position.x - previousPosition.x);
        previousPosition = position;
        ApplyFrame();
        AlignPaws(deltaTime);
    }

    void ApplyFrame()
    {
        if (frames != null && frames.Length == MikaAnimation.FrameCount) target.sprite = frames[Animation.Frame];
        target.flipX = Animation.Facing < 0;
    }

    void OnDisable()
    {
        if (controller) controller.ReturnedToStart -= ResetPose;
        Animation.Reset();
    }
}
