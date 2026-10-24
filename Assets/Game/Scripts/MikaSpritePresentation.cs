using UnityEngine;

// Opt-in until character acceptance and PR #36 integration. Attach to a visual child only.
[RequireComponent(typeof(SpriteRenderer))]
public sealed class MikaSpritePresentation : MonoBehaviour
{
    public CatController controller;
    public Sprite[] frames;
    public bool originalFrames;
    public readonly MikaAnimation Animation = new MikaAnimation();
    SpriteRenderer target;
    Vector3 previousPosition;
    BoxCollider2D shape;
    readonly RaycastHit2D[] contacts = new RaycastHit2D[8];

    void OnEnable()
    {
        target = GetComponent<SpriteRenderer>();
        Animation.OriginalFrames = originalFrames;
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
        ApplyFrame();
        AlignPaws();
    }

    void AlignPaws()
    {
        if (!shape) return;
        float floor = controller.transform.position.y + shape.offset.y - shape.size.y * 0.5f;
        if (controller.Grounded) {
            int count = shape.Cast(Vector2.down, new ContactFilter2D { useTriggers = false }, contacts, 0.08f);
            float nearest = float.PositiveInfinity;
            for (int i = 0; i < count; i++) {
                if (contacts[i].normal.y > 0.7f && contacts[i].distance < nearest) {
                    nearest = contacts[i].distance;
                    floor = contacts[i].point.y;
                }
            }
        }
        var position = transform.position;
        // The sprite pivot is the opaque sole. Correct physics contact skin on the visual child only.
        position.y = floor - 0.005f;
        transform.position = position;
    }

    void LateUpdate()
    {
        if (!controller) return;
        Vector3 position = controller.transform.position;
        Animation.Step(Time.deltaTime, controller.Grounded, controller.Motion.Velocity,
            position.x - previousPosition.x);
        previousPosition = position;
        ApplyFrame();
        AlignPaws();
    }

    void ApplyFrame()
    {
        if (frames != null && frames.Length == (originalFrames ? 16 : MikaAnimation.FrameCount)) target.sprite = frames[Animation.Frame];
        target.flipX = Animation.Facing < 0;
    }

    void OnDisable()
    {
        if (controller) controller.ReturnedToStart -= ResetPose;
        Animation.Reset();
    }
}
