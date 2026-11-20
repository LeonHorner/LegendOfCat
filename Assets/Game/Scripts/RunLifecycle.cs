using System;
using UnityEngine;

// Session-only rules; campaign records and ability choices are separate features.
// Start/Spawn use the caller's coordinate frame; MovementCourse supplies local coordinates.
public sealed class RunLifecycle
{
    public Vector2 Start { get; }
    public Vector2 Spawn { get; private set; }
    public int Checkpoint { get; private set; }
    public int Retries { get; private set; }
    public float ElapsedSeconds { get; private set; }
    bool completed;
    public bool Paused { get; private set; }

    // Encounter owners subscribe to remove projectiles and restore their initial state
    // before any respawn; the movement lab has no encounter yet.
    public event Action ResetTransientState;

    public RunLifecycle(Vector2 start)
    {
        Start = Spawn = start;
    }

    public bool TryActivateCheckpoint(int sequence, Vector2 safeSpawn, bool grounded)
    {
        // Grounded, forward-only checkpoints cannot replace safe recovery with an
        // airborne position or an earlier optional-route marker.
        if (Paused || !grounded || sequence <= Checkpoint || sequence < 1) return false;
        Checkpoint = sequence;
        Spawn = safeSpawn;
        return true;
    }

    public void Advance(float deltaSeconds)
    {
        if (float.IsNaN(deltaSeconds) || float.IsInfinity(deltaSeconds) || deltaSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
        if (!Paused && !completed) ElapsedSeconds += deltaSeconds;
    }

    public void Complete() => completed = true;

    public bool TogglePause() => Paused = !Paused;

    public Vector2 Retry()
    {
        ResetTransientState?.Invoke();
        Retries++;
        completed = false;
        Paused = false;
        return Spawn;
    }

    public Vector2 Restart()
    {
        ResetTransientState?.Invoke();
        Spawn = Start;
        Checkpoint = 0;
        Retries = 0;
        ElapsedSeconds = 0;
        completed = false;
        Paused = false;
        return Start;
    }
}
