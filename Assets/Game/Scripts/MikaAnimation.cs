using UnityEngine;

// Cosmetic state only. Physics, input, attack timing and the player's transform remain authoritative.
public sealed class MikaAnimation
{
    public enum Pose { Idle, Run, Turn, Jump, Fall, Landing }
    public Pose Current { get; private set; }
    public int Facing { get; private set; } = 1;
    public int Frame { get; private set; }
    public const float StrideLength = 2.4f;
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
        else if (Current == Pose.Landing && elapsed < 0.12f) next = Pose.Landing;
        else next = moving ? Pose.Run : Pose.Idle;

        if (next != Current || turned && grounded || landed) {
            Current = next;
            elapsed = 0;
        }
        // Drive the gait from measured displacement, not wall-clock time or input intent.
        // Reset on retry/teleport through the adapter; never animate a stationary run.
        if (grounded && moving) distance = Mathf.Repeat(distance + Mathf.Abs(travelled), StrideLength);
        switch (Current) {
            case Pose.Run: Frame = 4 + Mathf.Min(3, (int)(distance / StrideLength * 4)); break;
            case Pose.Turn: Frame = elapsed < 0.05f ? 8 : 9; break;
            case Pose.Jump: Frame = 10; break;
            case Pose.Fall: Frame = 11; break;
            case Pose.Landing: Frame = elapsed < 0.06f ? 12 : 13; break;
            default: Frame = (int)(elapsed * 3) % 4; break;
        }
    }
}
