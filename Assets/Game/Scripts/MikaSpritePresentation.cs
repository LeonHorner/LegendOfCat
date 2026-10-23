using UnityEngine;

// Opt-in until character acceptance and PR #36 integration. Attach to a visual child only.
[RequireComponent(typeof(SpriteRenderer))]
public sealed class MikaSpritePresentation : MonoBehaviour
{
    public CatController controller;
    public Sprite[] frames;
    public readonly MikaAnimation Animation = new MikaAnimation();
    SpriteRenderer target;
    Vector3 previousPosition;

    void OnEnable()
    {
        target = GetComponent<SpriteRenderer>();
        if (!controller) controller = GetComponentInParent<CatController>();
        if (controller) {
            controller.ReturnedToStart += ResetPose;
            ResetPose();
        }
    }

    void ResetPose()
    {
        Animation.Reset();
        previousPosition = controller.transform.position;
        ApplyFrame();
    }

    void LateUpdate()
    {
        if (!controller) return;
        Vector3 position = controller.transform.position;
        Animation.Step(Time.deltaTime, controller.Grounded, controller.Motion.Velocity,
            position.x - previousPosition.x);
        previousPosition = position;
        ApplyFrame();
    }

    void ApplyFrame()
    {
        if (frames != null && frames.Length == 16) target.sprite = frames[Animation.Frame];
        target.flipX = Animation.Facing < 0;
    }

    void OnDisable()
    {
        if (controller) controller.ReturnedToStart -= ResetPose;
        Animation.Reset();
    }
}
