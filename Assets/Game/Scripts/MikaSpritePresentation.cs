using UnityEngine;

// Cosmetic child only: never move the controller or change its collision geometry.
[RequireComponent(typeof(SpriteRenderer))]
public sealed class MikaSpritePresentation : MonoBehaviour
{
    public CatController controller;
    public Sprite[] frames;
    public readonly MikaAnimation Animation = new MikaAnimation();
    // The long cave ramps allow a full paw stance on one continuous rock plane.
    public Collider2D RiverbankTerrain { private get; set; }
    SpriteRenderer target;
    float gradeAngle, gradeOffset;
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
        gradeAngle=gradeOffset=0;
        ApplyFrame();
        AlignPaws(0, true);
    }

    void AlignPaws(float deltaTime, bool snap = false)
    {
        if (!shape) return;
        float floor = shape.transform.TransformPoint(shape.offset - Vector2.up * shape.size.y * 0.5f).y;
        float correction = 0;
        float angle=0, centerOffset=0;
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
                    angle=centerOffset=0;
                    var hit=contacts[i];
                    if (hit.collider==RiverbankTerrain && hit.normal.y<.996f) {
                        float grade=-hit.normal.x/hit.normal.y;
                        float center=shape.bounds.center.x;
                        float height=hit.point.y+(center-hit.point.x)*grade;
                        float span=shape.bounds.extents.x*.8f;
                        // Both sides must still be on this plane. Keep the neutral stance on a ledge lip.
                        bool Supported(float side) {
                            var sole=new Vector2(center+side,height+side*grade);
                            return hit.collider.OverlapPoint(sole-Vector2.up*.045f) &&
                                !hit.collider.OverlapPoint(sole+Vector2.up*.045f);
                        }
                        if(Supported(-span) && Supported(span)) {
                            angle=Mathf.Atan(grade)*Mathf.Rad2Deg;
                            centerOffset=height-floor-offset;
                        }
                    }
                }
            }
        }
        groundCorrection = snap ? correction : Mathf.MoveTowards(groundCorrection, correction,
            0.4f * Mathf.Max(0, deltaTime));
        var position = transform.position;
        bool immediate=snap || deltaTime<=0;
        gradeAngle=immediate?angle:Mathf.MoveTowards(gradeAngle,angle,260f*deltaTime);
        gradeOffset=immediate?centerOffset:Mathf.MoveTowards(gradeOffset,centerOffset,5f*deltaTime);
        if (RiverbankTerrain) transform.localRotation=Quaternion.Euler(0,0,gradeAngle);
        position.y = floor + groundCorrection + gradeOffset;
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
        Animation.Step(deltaTime, controller.Grounded, controller.PresentationVelocity,
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
