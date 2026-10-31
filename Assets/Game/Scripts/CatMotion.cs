using UnityEngine;

// Device-independent fixed-step rules. Presentation and collision queries live outside this class.
public sealed class CatMotion
{
    public const float Speed = 7, Acceleration = 55, Braking = 80;
    public const float JumpSpeed = 12, Gravity = 30, FallSpeed = 20;
    public const float CoyoteTime = 0.10f, BufferTime = 0.12f, ReleaseSpeed = 5;
    public Vector2 Velocity { get; private set; }
    public bool Jumped { get; private set; }
    float move, pulse, grace, buffer, recoil;
    bool held, externalRise;

    public void SetMove(float value) { move = Mathf.Clamp(value, -1, 1); if (move != 0) pulse = move; }
    public void PressJump() { buffer = BufferTime; held = true; }
    public void ReleaseJump() => held = false;
    public void Reset() { Velocity = Vector2.zero; move = pulse = grace = buffer = recoil = 0; held = externalRise = false; }

    public Vector2 Launch(Vector2 velocity)
    {
        buffer = grace = recoil = 0;
        externalRise = true;
        return Velocity = velocity;
    }

    public Vector2 Knockback(Vector2 velocity)
    {
        Launch(velocity);
        recoil = velocity.x;
        return velocity;
    }

    public bool StartBufferedJump(bool grounded, Vector2 actualVelocity)
    {
        if (buffer <= 0 || !grounded && grace <= 0) return false;
        actualVelocity.y = JumpSpeed;
        Velocity = actualVelocity;
        buffer = grace = 0;
        Jumped = true;
        return true;
    }

    public Vector2 Step(float dt, bool grounded, Vector2 actualVelocity)
    {
        Jumped = false;
        grace = grounded ? CoyoteTime : Mathf.Max(0, grace - dt);
        var velocity = actualVelocity;
        float axis = move != 0 ? move : pulse;
        pulse = 0;
        // Decay the external impulse separately so normal braking does not erase it.
        // Movement input continues to accelerate the controllable part of velocity.
        velocity.x -= recoil;
        recoil = Mathf.MoveTowards(recoil, 0, 25 * dt);
        velocity.x = Mathf.MoveTowards(velocity.x, axis * Speed,
            (Mathf.Abs(axis) < 0.01f || axis * velocity.x < 0 ? Braking : Acceleration) * dt);
        velocity.x += recoil;
        if (grounded && velocity.y < 0) velocity.y = 0;
        if (StartBufferedJump(grounded, velocity)) velocity = Velocity;
        if (velocity.y <= 0 || grounded) externalRise = false;
        if (!held && !externalRise && velocity.y > ReleaseSpeed) velocity.y = ReleaseSpeed;
        velocity.y = Mathf.Max(velocity.y - Gravity * dt, -FallSpeed);
        buffer = Mathf.Max(0, buffer - dt);
        return Velocity = velocity;
    }
}
