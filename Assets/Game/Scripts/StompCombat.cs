using UnityEngine;

// Prototype tuning, not final boss rules. No input-device or presentation dependencies.
public sealed class StompCombat
{
    public enum AttackPhase { Anticipation, Attack, Recovery, Defeated }
    public AttackPhase Phase { get; private set; }
    public int BossHealth { get; private set; }
    public int PlayerHealth { get; private set; }
    public float PhaseTime { get; private set; }
    public float Invulnerability { get; private set; }
    public bool Finished => BossHealth == 0 || PlayerHealth == 0;
    public const float Anticipation = 1.2f, Active = 0.35f, Recovery = 1.8f;
    public StompCombat() => Reset();
    public void Reset()
    {
        BossHealth = PlayerHealth = 3;
        Phase = AttackPhase.Anticipation;
        PhaseTime = Invulnerability = 0;
    }
    public void Advance(float dt)
    {
        if (float.IsNaN(dt) || float.IsInfinity(dt) || dt < 0)
            throw new System.ArgumentOutOfRangeException(nameof(dt));
        Invulnerability = Mathf.Max(0, Invulnerability - dt);
        if (Finished) return;
        PhaseTime += dt;
        while (PhaseTime >= Duration) {
            PhaseTime -= Duration;
            Phase = Phase == AttackPhase.Anticipation ? AttackPhase.Attack
                : Phase == AttackPhase.Attack ? AttackPhase.Recovery : AttackPhase.Anticipation;
        }
    }
    public float Duration => Phase == AttackPhase.Anticipation ? Anticipation
        : Phase == AttackPhase.Attack ? Active : Recovery;

    // Swept feet crossing excludes ascending, side and underside contacts.
    public static bool CrossedHead(Vector2 previous, Vector2 current, Vector2 halfSize, Rect head)
    {
        float before = previous.y - halfSize.y, after = current.y - halfSize.y;
        if (after >= before || before < head.yMax || after > head.yMax) return false;
        float x = Mathf.Lerp(previous.x, current.x, (before - head.yMax) / (before - after));
        return x + halfSize.x > head.xMin && x - halfSize.x < head.xMax;
    }
    public bool TryStomp(bool topHit)
    {
        if (Finished || !topHit || Phase != AttackPhase.Recovery) return false;
        BossHealth--;
        PhaseTime = 0;
        Phase = BossHealth == 0 ? AttackPhase.Defeated : AttackPhase.Anticipation;
        return true;
    }
    public bool TryDamage()
    {
        if (Finished || Invulnerability > 0) return false;
        PlayerHealth--;
        Invulnerability = 1.1f;
        return true;
    }
}
