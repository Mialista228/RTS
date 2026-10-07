using UnityEngine;

public class ProjectileDamagingExplosion : ProjectileExplosion
{
    [SerializeField][Min(1)] private int diceCount = 1;
    [SerializeField][Min(2)] private int diceSides = 2;
    [SerializeField] private UnitStat damageAmount = 40f;
    [SerializeField] private bool canCrit;
    [SerializeField] private float casterPhysicalDamageMultiplier = 0.5f;
    [SerializeField] private float casterMagicalDamageMultiplier = 0f;
    [SerializeField] private float casterPureDamageMultiplier = 0f;
    [SerializeField] private DamageType damageType;
    public override void Explosion()
    {
        int numFound = GetUnitsInRadius();

        DamageInfo damageInfo = default;
        if (caster.Attack is UnitAttack attack)
        {
            DamageInfo physicalInfo = default;
            DamageInfo magicalInfo = default;
            DamageInfo pureInfo = default;

            DamageInfo adaptedInfo = attack.GetDamageInfo(canCrit);
            switch (attack._DamageType)
            {
                case DamageType.Physical:
                    physicalInfo = attack.GetDamageInfo();
                    magicalInfo = attack.GetDamageInfo(DamageType.Magical);
                    pureInfo = attack.GetDamageInfo(DamageType.Pure);
                    break;
                case DamageType.Magical:
                    physicalInfo = attack.GetDamageInfo(DamageType.Physical);
                    magicalInfo = attack.GetDamageInfo();
                    pureInfo = attack.GetDamageInfo(DamageType.Pure);
                    break;
                case DamageType.Pure:
                    physicalInfo = attack.GetDamageInfo(DamageType.Physical);
                    magicalInfo = attack.GetDamageInfo(DamageType.Magical);
                    pureInfo = attack.GetDamageInfo();
                    break;
            }
            damageInfo.damageAmount = damageAmount.GetValue(runtimeProperties.level) + Dice.GetRandomValue(diceCount, diceSides) + physicalInfo.damageAmount * casterPhysicalDamageMultiplier
            + magicalInfo.damageAmount * casterMagicalDamageMultiplier + pureInfo.damageAmount * casterPureDamageMultiplier;

            damageInfo.critCoefficient = adaptedInfo.critCoefficient;
            damageInfo.penetration = adaptedInfo.penetration;
            damageInfo.percentPenetration = adaptedInfo.percentPenetration;
        }
        for (int i = 0; i < numFound; i++)
        {
            if (cols[i].TryGetComponent(out Unit unit))
            {
                if (unit == caster) continue;

                unit.DamageReceiver.TakeDamage(damageInfo, caster.Attack);
            }
        }
    }
}
