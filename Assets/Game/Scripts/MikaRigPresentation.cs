using UnityEngine;

// Small cutout rig; no Animator/package dependency and no authority over player motion.
public sealed class MikaRigPresentation : MonoBehaviour
{
    public CatController controller;
    public Sprite[] parts;
    public readonly MikaGait Gait = new MikaGait();
    readonly Transform[] uppers = new Transform[4], lowers = new Transform[4], paws = new Transform[4];
    readonly RaycastHit2D[] contacts = new RaycastHit2D[8];
    Transform torso, tail;
    BoxCollider2D shape;
    Vector2 previous;
    bool built;
    float tailTime, angle;
    public float MaximumAnkleError { get; private set; }
    public Vector3 PawSole(int leg) => paws[leg].position;

    void OnEnable()
    {
        if (!controller) controller = GetComponentInParent<CatController>();
        if (!controller || parts == null || parts.Length != 8) return;
        shape = controller.GetComponent<BoxCollider2D>();
        // Editor domain reload preserves child objects but not readonly managed arrays.
        Build();
        controller.ReturnedToStart += ResetPose;
        ResetPose();
    }

    Transform Part(string label, int sprite, int order, float width = 1)
    {
        var item = transform.Find(label);
        if (!item) {
            item = new GameObject(label).transform;
            item.SetParent(transform, false);
        }
        item.localScale = new Vector3(width, 1, 1);
        var renderer = item.GetComponent<SpriteRenderer>();
        if (!renderer) renderer = item.gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = parts[sprite];
        renderer.sortingOrder = order;
        return item;
    }

    void Build()
    {
        tail = Part("Striped tail", 1, 2);
        MaximumAnkleError = 0;
        for (int i = 0; i < 4; i++) {
            bool front = i < 2;
            bool far = i % 2 == 1;
            int order = far ? 1 : 5;
            uppers[i] = Part($"Leg {i} upper", front ? 2 : 4, order + 1);
            lowers[i] = Part($"Leg {i} lower", front ? 3 : 5, order);
            paws[i] = Part($"Leg {i} paw", front ? 6 : 7, order);
            if (far) {
                Color shade = new Color(0.79f, 0.79f, 0.79f);
                uppers[i].GetComponent<SpriteRenderer>().color = shade;
                lowers[i].GetComponent<SpriteRenderer>().color = shade;
                paws[i].GetComponent<SpriteRenderer>().color = shade;
            }
        }
        torso = Part("Torso and head", 0, 8);
        built = true;
    }

    float Floor()
    {
        float floor = controller.transform.position.y + shape.offset.y - shape.size.y * 0.5f;
        if (controller.Grounded) {
            int count = shape.Cast(Vector2.down, new ContactFilter2D { useTriggers = false }, contacts, 0.08f);
            float closest = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
                if (contacts[i].normal.y > 0.7f && contacts[i].distance < closest) {
                    closest = contacts[i].distance;
                    floor = contacts[i].collider.ClosestPoint(new Vector2(controller.transform.position.x,shape.bounds.max.y)).y;
                }
        }
        return floor - 0.005f;
    }

    void ResetPose()
    {
        previous = controller.transform.position;
        Gait.Reset(new Vector2(previous.x, Floor()));
        angle = tailTime = 0;
    }

    void LateUpdate() => RenderPose(Time.deltaTime);

    public void RenderPose(float dt)
    {
        if (!built || !torso || !uppers[0]) return;
        Vector2 position = controller.transform.position;
        float floor = Floor();
        Vector2 origin = new Vector2(position.x, floor);
        Gait.Step(dt, controller.Grounded, controller.Motion.Velocity, origin, position.x - previous.x);
        previous = position;
        transform.position = origin;
        transform.localScale = new Vector3(Gait.Facing, 1, 1);
        float pitch = controller.Grounded ? 0 : Mathf.Clamp(controller.Motion.Velocity.y * 0.45f, -5, 5);
        angle = Mathf.LerpAngle(angle, pitch, 1 - Mathf.Exp(-14 * dt));
        float minBodyX = float.NegativeInfinity, maxBodyX = float.PositiveInfinity;
        for (int i = 0; i < 4; i++) {
            float rootX = (i < 2 ? 0.37f : -0.39f) - (i % 2 == 1 ? 0.10f : 0);
            Vector2 foot = transform.InverseTransformPoint(Gait.Feet[i]);
            float reach = (i < 2 ? 0.53f : 0.58f) - 0.004f;
            minBodyX = Mathf.Max(minBodyX,foot.x-rootX-reach);
            maxBodyX = Mathf.Min(maxBodyX,foot.x-rootX+reach);
        }
        float bodyX = minBodyX <= maxBodyX ? Mathf.Clamp(0,minBodyX,maxBodyX) : (minBodyX+maxBodyX)*0.5f;
        float hipHeight = Gait.HipHeight;
        for (int i = 0; i < 4; i++) {
            bool front = i < 2;
            float rootX = (front ? 0.37f : -0.39f) - (i % 2 == 1 ? 0.10f : 0);
            Vector2 foot = transform.InverseTransformPoint(Gait.Feet[i]);
            float reach = (front ? 0.53f : 0.58f) - 0.002f;
            float horizontal = foot.x - bodyX - Mathf.Cos(angle*Mathf.Deg2Rad)*rootX;
            float vertical = Mathf.Sqrt(Mathf.Max(0,reach*reach-horizontal*horizontal));
            hipHeight = Mathf.Min(hipHeight,foot.y+0.065f+vertical-Mathf.Sin(angle*Mathf.Deg2Rad)*rootX);
        }
        torso.localPosition = new Vector3(bodyX, hipHeight);
        torso.localRotation = Quaternion.Euler(0, 0, angle);
        tailTime += dt;
        tail.localPosition = torso.localPosition + new Vector3(-0.42f, 0.04f);
        tail.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(tailTime * 2.5f) * 4 - angle);
        MaximumAnkleError = 0;
        for (int i = 0; i < 4; i++) {
            bool front = i < 2;
            float rootX = front ? 0.37f : -0.39f;
            if (i % 2 == 1) rootX -= 0.10f;
            Vector2 root = torso.localPosition + torso.localRotation * new Vector3(rootX, 0);
            Vector2 foot = transform.InverseTransformPoint(Gait.Feet[i]);
            Vector2 ankle = foot + Vector2.up * 0.065f;
            float upper = front ? 0.27f : 0.30f, lower = front ? 0.26f : 0.28f;
            Vector2 knee = MikaGait.Knee(root, ankle, upper, lower, front ? -1 : 1);
            PlaceBone(uppers[i], root, knee);
            PlaceBone(lowers[i], knee, ankle);
            MaximumAnkleError = Mathf.Max(MaximumAnkleError,
                Mathf.Abs(Vector2.Distance(knee, ankle) - lower));
            paws[i].localPosition = foot;
            paws[i].localRotation = Quaternion.identity;
        }
    }

    static void PlaceBone(Transform bone, Vector2 start, Vector2 end)
    {
        bone.localPosition = start;
        Vector2 delta = end - start;
        bone.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg + 90);
    }

    void OnDisable() { if (controller) controller.ReturnedToStart -= ResetPose; }
}
