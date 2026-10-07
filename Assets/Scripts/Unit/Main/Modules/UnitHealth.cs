using UnityEngine;
using UnityEngine.Events;

public class UnitHealth : UnitModule, IDamageReceiver
{
    [Header("Module Settings")]
    [SerializeField] private float hitPoints = 800f;
    [SerializeField] private float maxHitPoints = 800f;
    [SerializeField] private float hitPointsRegenSpeed = 1f;
    [SerializeField] private float hitPointsPercentRegenSpeed = 0.1f;
    [SerializeField] private float physicalDefense = 10f;
    [SerializeField] private float magicDefense = 10f;
    [SerializeField] private float damageReduction = 0f;
    [SerializeField] private float healMultiplier = 1f;
    [SerializeField] private UnityEvent<IAttackable> OnDie;

    public float CurrentHP => hitPoints;

    public float MaxHP => maxHitPoints;

    public float PhysicalDefense => physicalDefense;

    public float MagicDefense => magicDefense;

    public bool isAlive => hitPoints > 0f;

    private float regenDeltaTime = 1f;

    public UnityAction<IAttackable> onDie;
    public UnityAction<IAttackable> onDamageTaken;

    public override void OnAwake(Unit unit)
    {
        base.OnAwake(unit);

        maxHitPoints = unit.MaxHealth;
        hitPoints = maxHitPoints;

        physicalDefense = unit.PhysicalDefense;
        magicDefense = unit.MagicalDefense;
    }

    public override void OnTick(float deltaInterval)
    {
        if (isAlive)
        {
            if (hitPoints < maxHitPoints)
            {
                regenDeltaTime -= deltaInterval;
                if (regenDeltaTime <= 0f)
                {
                    Heal(new HealInfo(hitPointsRegenSpeed + (hitPointsPercentRegenSpeed / 100f * hitPoints)));
                    regenDeltaTime = 1f;
                }
            }
        }
    }

    public void Heal(HealInfo healInfo)
    {
        if (!isAlive) return;
        hitPoints += healInfo.healAmount * healMultiplier;
    }

    public void TakeDamage(DamageInfo damageInfo, IAttackable attacker)
    {
        float rawDefense = 0f;
        switch (damageInfo.damageType)
        {
            case DamageType.Physical:
                rawDefense = physicalDefense;
                break;
            case DamageType.Magical:
                rawDefense = magicDefense;
                break;
            case DamageType.Pure:
                rawDefense = 0f;
                break;
        }
        float netDefense = rawDefense * (1f - (damageInfo.percentPenetration / 100f)) - damageInfo.penetration;
        netDefense = Mathf.Max(0f, netDefense);

        float armorMultiplier = 1f;
        if (netDefense > 0f)
        {
            float x = 0.06f * netDefense;
            armorMultiplier = 1f - (x / (1f + x));
        }

        float reductionMultiplier = 1f - (damageReduction / 100f);

        float finalDamage = damageInfo.damageAmount * damageInfo.critCoefficient * armorMultiplier * reductionMultiplier;

        hitPoints = Mathf.Max(0f, hitPoints - finalDamage);

        onDamageTaken?.Invoke(attacker);

        Debug.Log($"Damage Taken {unit.gameObject} - {finalDamage}");

        HUD.DamageTaken(transform, new DamageShortInfo(finalDamage, damageInfo.critCoefficient, damageInfo.damageType));

        if (!isAlive)
        {
            OnDeath(attacker);
        }
    }

    private void OnDeath(IAttackable killer)
    {
        OnDie?.Invoke(killer);
        onDie?.Invoke(killer);
    }
}
