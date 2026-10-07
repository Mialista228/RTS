using UnityEngine;

public interface IDamageReceiver
{
    float CurrentHP { get; }
    float MaxHP { get; }
    void Heal(HealInfo healInfo);
    void TakeDamage(DamageInfo damageInfo, IAttackable attacker);
    bool isAlive { get; }
}