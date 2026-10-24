using UnityEngine;

// Cosmetic state only. Physics, input, attack timing and the player's transform remain authoritative.
public sealed class MikaAnimation
{
    public enum Pose { Idle, Run, Turn, Jump, Fall, Landing }
    public Pose Current { get; private set; }
    public int Facing { get; private set; } = 1;
    public int Frame { get; private set; }
    public const int FrameCount = 78;
    public const float StrideLength = 3.6f;
    public const float LandingDuration = 0.24f;
    float elapsed, distance;
    bool initialized, wasGrounded;

    public void Reset()
    {
        Current = Pose.Idle;
        Facing = 1;
        Frame = 0;
        elapsed = distance = 0;
        initialized = wasGrounded = false;
    }

    public void Step(float deltaTime, bool grounded, Vector2 velocity, float travelled)
    {
        float dt = Mathf.Max(0, deltaTime);
        elapsed += dt;
        bool moving = Mathf.Abs(velocity.x) > 0.1f;
        int direction = moving ? (velocity.x < 0 ? -1 : 1) : Facing;
        bool turned = direction != Facing;
        bool landed = initialized && !wasGrounded && grounded;
        initialized = true;
        wasGrounded = grounded;
        Facing = direction;

        // Airborne state always interrupts ground flourishes, including a buffered landing jump.
        Pose next;
        if (!grounded) next = velocity.y > 0 ? Pose.Jump : Pose.Fall;
        else if (landed) next = Pose.Landing;
        else if (turned || Current == Pose.Turn && elapsed < 0.10f) next = Pose.Turn;
        else if (Current == Pose.Landing && elapsed < LandingDuration) next = Pose.Landing;
        else next = moving ? Pose.Run : Pose.Idle;

        if (next != Current || turned && grounded || landed) {
            Current = next;
            elapsed = 0;
        }
        // Drive the gait from measured displacement, not wall-clock time or input intent.
        // Reset on retry/teleport through the adapter; never animate a stationary run.
        if (grounded && moving) distance = Mathf.Repeat(distance + Mathf.Abs(travelled), StrideLength);
        switch (Current) {
            case Pose.Run:
                int phase = Mathf.Min(31, (int)(distance / StrideLength * 32));
                Frame = phase % 4 == 0 ? 6 + phase / 4 : 24 + phase / 4 * 3 + phase % 4 - 1;
                break;
            case Pose.Turn: Frame = elapsed < 0.05f ? 4 : 5; break;
            case Pose.Jump:
                Frame = elapsed < 0.06f ? Between(14, 15, elapsed / 0.06f)
                    : Between(15, 16, Mathf.Clamp01((11 - velocity.y) / 11));
                break;
            case Pose.Fall:
                float descent = Mathf.Clamp(-velocity.y / 6, 0, 3);
                int stage = Mathf.Min(2, (int)descent);
                Frame = Between(16 + stage, 17 + stage, descent - stage);
                break;
            case Pose.Landing:
                float recovery = Mathf.Clamp(elapsed / LandingDuration * 4, 0, 3.99f);
                int landingStage = (int)recovery;
                Frame = Between(20 + landingStage, landingStage == 3 ? 0 : 21 + landingStage,
                    recovery - landingStage);
                break;
            default: Frame = 0; break;
        }
    }

    static int Between(int first, int second, float fraction)
    {
        int step = Mathf.Clamp((int)(fraction * 4), 0, 4);
        if (step == 0) return first;
        if (step == 4) return second;
        int pair = first >= 20 ? 14 + first - 20 : 8 + first - 14;
        return 24 + pair * 3 + step - 1;
    }
}
