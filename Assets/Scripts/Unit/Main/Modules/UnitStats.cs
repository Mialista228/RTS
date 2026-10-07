using UnityEngine;

public class UnitStats : UnitModule
{
    private UnitData unitData => unit ? unit.UnitData : null;
    private UnitLevel Level => unit ? unit.Level : null;
    private IDamageReceiver DamageReceiver => unit ? unit.DamageReceiver : null;
    private UnitAbilities Abilities => unit ? unit.Abilities : null;
    #region Unit Combat Stats

    public UnitAttackStats GetUnitAttackStats(DamageType damageType)
    {
        if (unitData == null)
        {
            return default;
        }
        UnitAttackStats stats = unitData.GetUnitAttackStats(damageType);

        if (damageType == unitData.damageType)
            stats.primaryAttribute += PrimaryAttributeValue;

        return stats;
    }

    public float PrimaryAttributeValue
    {
        get
        {
            if (unitData)
            {
                if (unitData.heroUnit)
                {
                    return unitData.damageAttribute switch
                    {
                        UnitData.DamageAttribute.Strength => Strength,
                        UnitData.DamageAttribute.Agility => Agility,
                        UnitData.DamageAttribute.Intelligence => Intelligence,
                        _ => 0f,
                    };
                }
            }
            return 0f;
        }
    }

    public UnitData.UnitSurface UnitsCanAttack => unitData ? unitData.unitsCanAttack : UnitData.UnitSurface.Ground;
    public float AttackRange
    {
        get
        {
            if (unitData == null)
                return 10f;
            return Level ? unitData.attackRange.GetValue(Level.CurrentLevel) : unitData.attackRange.GetValue();
        }
    }
    public float AttackFrame
    {
        get
        {
            if (unitData == null)
                return 0.2f;
            return Level ? unitData.attackFrame.GetValue(Level.CurrentLevel) : unitData.attackFrame.GetValue();
        }
    }
    public float AttackRecoverFrame
    {
        get
        {
            if (unitData == null)
                return 0.5f;
            return Level ? unitData.attackRecoverFrame.GetValue(Level.CurrentLevel) : unitData.attackRecoverFrame.GetValue();
        }
    }
    public bool CanCrit => unitData ? unitData.canCrit : true;
    public float CritChance
    {
        get
        {
            if (unitData == null)
                return 0f;
            if (Level)
                return unitData.critChance.GetValue(Level.CurrentLevel);
            return unitData.critChance.GetValue();
        }
    }
    public float CritDMG
    {
        get
        {
            if (unitData == null)
                return 0f;
            if (Level)
                return unitData.critDMG.GetValue(Level.CurrentLevel);
            return unitData.critDMG.GetValue();
        }
    }
    public UnitProjectile Projectile => unitData ? unitData.projectile : null;

    public float Strength
    {
        get
        {
            if (unitData == null)
                return 0f;
            if (unitData.heroUnit)
                return Level ? unitData.strength.GetValue(Level.CurrentLevel) : unitData.strength.GetValue();
            return 0f;
        }
    }
    public float Agility
    {
        get
        {
            if (unitData == null)
                return 0f;
            if (unitData.heroUnit)
                return Level ? unitData.agility.GetValue(Level.CurrentLevel) : unitData.agility.GetValue();
            return 0f;
        }
    }
    public float Intelligence
    {
        get
        {
            if (unitData == null)
                return 0f;
            if (unitData.heroUnit)
                return Level ? unitData.intelligence.GetValue(Level.CurrentLevel) : unitData.intelligence.GetValue();
            return 0f;
        }
    }

    #endregion

    #region Unit Health Stats

    public float Health => DamageReceiver != null ? DamageReceiver.CurrentHP : 0f;
    public float MaxHealth => unitData ? unitData.MaxHitPoints() : (DamageReceiver != null ? DamageReceiver.MaxHP : 0f);
    public float Mana => Abilities != null ? Abilities.Mana : 0f;
    public float MaxMana => unitData ? unitData.MaxMana() : (Abilities != null ? Abilities.MaxMana : 0f);
    public float HealthRegen => unitData ? unitData.baseHitPointRegen.GetValue(Level.CurrentLevel) : 1f;
    public float HealthRegenPercent => unitData ? unitData.baseHitPointPercentRegen.GetValue(Level.CurrentLevel) : .1f;
    public float ManaRegen => unitData ? unitData.baseManaRegen.GetValue(Level.CurrentLevel) : 1f;
    public float ManaRegenPercent => unitData ? unitData.baseManaPercentRegen.GetValue(Level.CurrentLevel) : .1f;

    public float PhysicalDefense
    {
        get
        {
            if (unitData == null)
                return 0f;
            if (unitData.heroUnit)
                if (Level)
                    return unitData.basePhysicalDefense.GetValue(Level.CurrentLevel) + (Agility * unitData.armorPerAgility);
                else
                    return unitData.basePhysicalDefense.GetValue() + (Agility * unitData.armorPerAgility);
            else
                return unitData.basePhysicalDefense.GetValue();
        }
    }

    public float MagicalDefense
    {
        get
        {
            if (unitData == null)
                return 0f;
            if (unitData.heroUnit)
                if (Level)
                    return unitData.baseMagicDefense.GetValue(Level.CurrentLevel) + (Intelligence * unitData.magicResistPerIntelligence);
                else
                    return unitData.baseMagicDefense.GetValue() + (Intelligence * unitData.magicResistPerIntelligence);
            else
                return unitData.baseMagicDefense.GetValue();
        }
    }

    #endregion
    public override void OnAwake(Unit unit)
    {
        base.OnAwake(unit);
    }

    public override void OnStart()
    {

    }

    public override void OnTick(float deltaInterval)
    {

    }
}
