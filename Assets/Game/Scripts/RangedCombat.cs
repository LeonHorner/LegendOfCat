using System;
using System.Collections.Generic;
using UnityEngine;

// Session-only prototype rules. Input devices and visual effects belong to adapters.
public sealed class RangedCombat
{
    public sealed class Projectile
    {
        public Vector2 Position, Velocity;
        public float Age;
        public bool Lob;
    }
    public enum Weapon { Pulse, Orb }
    public const float ChargeSeconds = .8f, Gravity = 18;
    public const float PulsePause = .2f, OrbPause = .4f;
    public const float PulseReload = 1, OrbReload = 1.5f;
    public const int PulseCapacity = 8, OrbCapacity = 3;
    readonly int[] ammo = { PulseCapacity, OrbCapacity };
    readonly float[] cooldowns = new float[2];
    public Weapon ActiveWeapon { get; private set; }
    public int Ammo => ammo[(int)ActiveWeapon];
    public int Capacity => ActiveWeapon == Weapon.Pulse ? PulseCapacity : OrbCapacity;
    public float ReloadDuration => ActiveWeapon == Weapon.Pulse ? PulseReload : OrbReload;
    public float ReloadRemaining { get; private set; }
    public bool Reloading => ReloadRemaining > 0;
    public const int MaximumProjectiles = 8;
    readonly List<Projectile> shots = new List<Projectile>();
    public IReadOnlyList<Projectile> Projectiles => shots;
    public float Elapsed { get; private set; }
    public float Charge { get; private set; }
    public float Cooldown => cooldowns[(int)ActiveWeapon];
    public bool Charging { get; private set; }
    public bool Paused { get; private set; }
    public int Health { get; private set; } = 6;
    public bool Finished => Health == 0;
    public bool Vulnerable => Elapsed % 5 >= 1.5f && !Finished;
    public Vector2 Target => new Vector2(2.8f * Mathf.Sin(Elapsed * .65f),
        3.3f + 2.1f * Mathf.Sin(Elapsed * .85f));
    public Vector2 LastContact { get; private set; }
    public float HitFlash { get; private set; }
    public float BlockFlash { get; private set; }
    public int Facing { get; private set; } = 1;

    public void SetFacing(float direction) { if (!Paused && direction != 0) Facing = direction < 0 ? -1 : 1; }
    bool PrepareAttack(Weapon weapon)
    {
        if (Paused || Finished) return false;
        if (ActiveWeapon != weapon) {
            CancelCharge();
            ReloadRemaining = 0;
            ActiveWeapon = weapon;
        }
        if (Reloading || Cooldown > 0) return false;
        if (Ammo == 0) { Reload(); return false; }
        return true;
    }
    public bool PressPulse(Vector2 origin) => PrepareAttack(Weapon.Pulse) && Fire(origin, false, 0);
    public bool PressLob()
    {
        if (!PrepareAttack(Weapon.Orb) || Charging) return false;
        Charging = true; Charge = 0;
        return true;
    }
    public bool ReleaseLob(Vector2 origin)
    {
        if (Paused) { CancelCharge(); return false; }
        if (!Charging || ActiveWeapon != Weapon.Orb) return false;
        float charge = Charge;
        CancelCharge();
        return Fire(origin, true, charge);
    }
    public int AmmoFor(Weapon weapon) => ammo[(int)weapon];
    public void Reload()
    {
        if (Paused || Finished || Reloading || Ammo == Capacity) return;
        CancelCharge();
        ReloadRemaining = ReloadDuration;
    }
    bool Fire(Vector2 origin, bool lob, float charge)
    {
        if (Paused || Finished || Reloading || Ammo == 0 || Cooldown > 0 || shots.Count >= MaximumProjectiles) return false;
        shots.Add(new Projectile { Position = origin, Lob = lob,
            Velocity = new Vector2(Facing * (lob ? 7 : 15), lob ? Mathf.Lerp(5, 15, charge) : 0) });
        ammo[(int)ActiveWeapon]--;
        cooldowns[(int)ActiveWeapon] = lob ? OrbPause : PulsePause;
        if (Ammo == 0) Reload();
        return true;
    }
    public void CancelCharge() { Charging = false; Charge = 0; }
    public void CancelTransient() { CancelCharge(); shots.Clear(); }
    public void SetPaused(bool paused) { Paused = paused; }
    public void Reset()
    {
        CancelTransient(); Elapsed = HitFlash = BlockFlash = ReloadRemaining = 0;
        ActiveWeapon = Weapon.Pulse;
        ammo[0] = PulseCapacity; ammo[1] = OrbCapacity;
        cooldowns[0] = cooldowns[1] = 0;
        Health = 6; Facing = 1; Paused = false;
    }
    public static bool CrossesTarget(Vector2 start, Vector2 end, Vector2 targetStart, Vector2 targetEnd)
    {
        Vector2 a = start - targetStart, b = end - targetEnd, delta = b - a;
        float t = delta.sqrMagnitude > 0 ? Mathf.Clamp01(-Vector2.Dot(a, delta) / delta.sqrMagnitude) : 0;
        return (a + t * delta).sqrMagnitude <= .65f * .65f;
    }
    public void Advance(float dt)
    {
        if (float.IsNaN(dt) || float.IsInfinity(dt) || dt < 0) throw new ArgumentOutOfRangeException(nameof(dt));
        if (Paused || Finished) return;
        Vector2 previousTarget = Target;
        bool open = Vulnerable;
        cooldowns[0] = Mathf.Max(0, cooldowns[0] - dt);
        cooldowns[1] = Mathf.Max(0, cooldowns[1] - dt);
        if (Reloading) {
            ReloadRemaining = Mathf.Max(0, ReloadRemaining - dt);
            if (!Reloading) ammo[(int)ActiveWeapon] = Capacity;
        }
        HitFlash = Mathf.Max(0, HitFlash - dt); BlockFlash = Mathf.Max(0, BlockFlash - dt);
        if (Charging) Charge = Mathf.Clamp01(Charge + dt / ChargeSeconds);
        Elapsed += dt;
        for (int i = shots.Count - 1; i >= 0; i--) {
            var shot = shots[i];
            Vector2 before = shot.Position;
            Vector2 acceleration = shot.Lob ? Vector2.down * Gravity : Vector2.zero;
            shot.Position += shot.Velocity * dt + acceleration * (.5f * dt * dt);
            shot.Velocity += acceleration * dt;
            shot.Age += dt;
            if (CrossesTarget(before, shot.Position, previousTarget, Target)) {
                LastContact = shot.Position;
                if (open) { Health--; HitFlash = .18f; } else BlockFlash = .18f;
                shots.RemoveAt(i);
                if (Finished) { CancelTransient(); break; }
            } else if (shot.Age >= 3 || Mathf.Abs(shot.Position.x) >= 8 || shot.Position.y <= .12f || shot.Position.y >= 8)
                shots.RemoveAt(i);
        }
    }
}
