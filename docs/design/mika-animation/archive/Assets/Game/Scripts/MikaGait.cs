using UnityEngine;

// Presentation targets only. Stance feet are world-locked; joints never drive physics.
public sealed class MikaGait
{
    public const float CycleDistance = 1.40f;
    public const float StanceFraction = 0.50f;
    public readonly Vector2[] Feet = new Vector2[4];
    public readonly bool[] Supporting = new bool[4];
    static readonly float[] RestX = { 0.40f, 0.30f, -0.43f, -0.53f };
    static readonly float[] Offset = { 0, 0.5f, 0.5f, 0 };
    readonly Vector2[] planted = new Vector2[4];
    readonly Vector2[] lift = new Vector2[4];
    readonly bool[] swinging = new bool[4];
    readonly float[] swingPhase = new float[4];
    readonly float[] swingTargetX = new float[4];
    readonly bool[] settling = new bool[4];
    float phase, impact, airBlend, runBlend;
    bool initialized, wasGrounded;
    int facing = 1;
    Vector2 previousOrigin;
    public int Facing => facing;
    public float HipHeight => 0.54f - runBlend * 0.14f - impact;
    public float AirBlend => airBlend;

    public void Reset(Vector2 origin)
    {
        initialized = true;
        wasGrounded = true;
        facing = 1;
        previousOrigin = origin;
        phase = StanceFraction * 0.5f;
        impact = airBlend = runBlend = 0;
        for (int i = 0; i < 4; i++) {
            Feet[i] = planted[i] = lift[i] = origin + new Vector2(RestX[i], 0);
            swinging[i] = false;
            settling[i] = false;
            Supporting[i] = true;
        }
    }

    public void Step(float dt, bool grounded, Vector2 velocity, Vector2 origin, float travelled)
    {
        if (!initialized) Reset(origin);
        dt = Mathf.Max(0, dt);
        int direction = Mathf.Abs(velocity.x) > 0.1f ? (velocity.x < 0 ? -1 : 1) : facing;
        if (direction != facing) {
            // A mirrored front leg occupies the old hind leg's side; preserve world contacts.
            for (int i = 0; i < 2; i++) {
                var foot = Feet[i]; Feet[i] = Feet[i + 2]; Feet[i + 2] = foot;
                planted[i] = Feet[i]; planted[i + 2] = Feet[i + 2];
                swinging[i] = swinging[i + 2] = false;
            }
            facing = direction;
            phase = StanceFraction * 0.5f;
        }
        bool landed = grounded && !wasGrounded;
        if (landed) {
            impact = 0.085f;
            for (int i = 0; i < 4; i++) {
                Feet[i].y = origin.y;
                planted[i] = Feet[i];
                swinging[i] = false;
            }
            phase = StanceFraction * 0.5f;
        }
        impact *= Mathf.Exp(-14 * dt);
        airBlend = Mathf.MoveTowards(airBlend, grounded ? 0 : 1, dt * 12);
        if (grounded) phase = Mathf.Repeat(phase + Mathf.Abs(travelled) / CycleDistance, 1);
        float reach = CycleDistance * StanceFraction * 0.5f;
        bool moving = Mathf.Abs(velocity.x) > 0.1f;
        bool atRest = true;
        for (int i = 0; i < 4; i++)
            if (swinging[i] || Mathf.Abs(Feet[i].x - (origin.x + RestX[i]*facing)) > 0.025f) atRest = false;
        if (moving || atRest) runBlend = Mathf.MoveTowards(runBlend,moving ? 1 : 0,dt*6);
        int settlePair = swinging[0] || swinging[3] ? 0 : swinging[1] || swinging[2] ? 1
            : Mathf.Abs(Feet[0].x-(origin.x+RestX[0]*facing)) > 0.025f
              || Mathf.Abs(Feet[3].x-(origin.x+RestX[3]*facing)) > 0.025f ? 0 : 1;
        for (int i = 0; i < 4; i++) {
            float rest = origin.x + RestX[i] * facing;
            if (!grounded) {
                // Carry airborne paws with the physics root; smooth only the local joint pose.
                // Smoothing world positions would trail behind fast jumps and stretch the legs.
                Feet[i] += origin - previousOrigin;
                // Fold gradually while rising, then extend continuously before contact.
                float tuck = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(-7, 4, velocity.y));
                var target = new Vector2(rest + (i < 2 ? -0.08f : 0.07f) * facing * tuck,
                    origin.y + Mathf.Lerp(0.015f, 0.22f, tuck));
                Feet[i] = Vector2.Lerp(Feet[i], target, 1 - Mathf.Exp(-24 * dt));
                Supporting[i] = false;
                swinging[i] = false;
                continue;
            }
            float p = Mathf.Repeat(phase + Offset[i], 1);
            bool wantsSwing = moving && p >= StanceFraction;
            bool resetStep = !moving && (i == 0 || i == 3 ? 0 : 1) == settlePair
                && Mathf.Abs(Feet[i].x-rest) > 0.025f;
            if (resetStep && !swinging[i]) {
                lift[i] = Feet[i]; swinging[i] = settling[i] = true;
                swingPhase[i] = 0; swingTargetX[i] = rest;
            }
            if (!moving && swinging[i] && !settling[i]) {
                lift[i] = Feet[i]; swingPhase[i] = 0;
                settling[i] = true; swingTargetX[i] = rest;
            }
            // Finish an existing step when input stops, then leave all four soles planted.
            if (!wantsSwing && moving && swinging[i]) {
                Feet[i] = new Vector2(rest + (reach - p * CycleDistance) * facing, origin.y);
                planted[i] = Feet[i];
                swinging[i] = false;
            }
            if (wantsSwing || !moving && swinging[i]) {
                if (!swinging[i]) {
                    lift[i] = Feet[i]; swinging[i] = true; settling[i] = false;
                    // Fix the future touchdown point now; zero-velocity easing then meets stance cleanly.
                    swingTargetX[i] = rest + (reach + (1-p)*CycleDistance)*facing;
                }
                float t = moving ? (p - StanceFraction) / (1 - StanceFraction)
                    : Mathf.MoveTowards(swingPhase[i], 1, dt * 10);
                swingPhase[i] = t;
                float ease = t * t * (3 - 2 * t);
                float x = Mathf.Lerp(lift[i].x, swingTargetX[i], ease);
                Feet[i] = new Vector2(x, origin.y + Mathf.Sin(Mathf.PI * t) * (moving ? 0.075f : 0.04f));
                planted[i] = new Vector2(x, origin.y);
                if (t >= 1) { swinging[i] = false; Feet[i] = planted[i]; }
            }
            else {
                Feet[i] = new Vector2(planted[i].x, origin.y);
            }
            Supporting[i] = !swinging[i];
        }
        wasGrounded = grounded;
        previousOrigin = origin;
    }

    public static Vector2 Knee(Vector2 hip, Vector2 ankle, float upper, float lower, int bend)
    {
        Vector2 delta = ankle - hip;
        float distance = Mathf.Clamp(delta.magnitude, 0.001f, upper + lower - 0.0001f);
        Vector2 direction = delta.sqrMagnitude > 0.000001f ? delta.normalized : Vector2.down;
        float along = (upper * upper - lower * lower + distance * distance) / (2 * distance);
        float across = Mathf.Sqrt(Mathf.Max(0, upper * upper - along * along));
        return hip + direction * along + new Vector2(-direction.y, direction.x) * across * bend;
    }
}
