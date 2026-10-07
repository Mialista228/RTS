using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitData", menuName = "RTS/Unit Data")]
public class UnitData : ScriptableObject
{
    [System.Flags]
    public enum UnitSurface
    {
        Ground = 1 << 0,
        Water = 1 << 1,
        Air = 1 << 2
    }
    public enum UnitRace
    {
        Human,
        NightElf,
        Orc,
        Undead,
        Naga
    }
    public enum UnitBodyType
    {
        Flesh,
        Stone,
        Metal,
        Wood
    }
    public enum DamageAttribute { Strength, Agility, Intelligence }

    void OnValidate()
    {
        if (movementSurface == 0)
        {
            movementSurface = UnitSurface.Ground;
        }
    }

    [Header("Main")]
    public string unitName;
    public string unitNickname;
    public bool heroUnit;
    [TextArea] public string unitDescription;
    public UnitSurface movementSurface = UnitSurface.Ground;
    public UnitBodyType bodyType = UnitBodyType.Flesh;

    [Header("Costs")]
    public uint goldCost = 100;
    public uint lumberCost = 20;
    public uint foodCost = 1;

    [Header("Basic Unit Stats")]
    public int unitPriority = 0;
    public UnitStat moveSpeed = 5f;
    public UnitStat baseHitPoints = 800f;
    public UnitStat baseHitPointRegen = 1f;
    public UnitStat baseHitPointPercentRegen = 0.1f;
    public UnitStat baseMana = 200f;
    public UnitStat baseManaRegen = 1f;
    public UnitStat baseManaPercentRegen = 0.1f;
    public UnitStat basePhysicalDefense = 3f;
    public UnitStat baseMagicDefense = 3f;
    public float visionDistance = 5f;
    [Min(1)] public int diceCount;
    [Min(2)] public int diceSides;

    [Header("Attacks")]
    public UnitSurface unitsCanAttack;
    public SurfaceHitSound attackHitSound;
    public UnitStat attackRange = 5f;
    public UnitStat attackFrame = 0.2f;
    public UnitStat attackRecoverFrame = 0.5f;
    public bool canCrit = true;
    public UnitStat critChance = 0f;
    public UnitStat critDMG = 200f;
    public UnitProjectile projectile;
    public DamageType damageType = DamageType.Physical;
    [SerializeField] private UnitAttackStats physical;
    [SerializeField] private UnitAttackStats magical;
    [SerializeField] private UnitAttackStats pure;

    [Header("Hero Unit Stats")]
    public DamageAttribute damageAttribute;
    public UnitStat strength = 10f;
    public UnitStat agility = 10f;
    public UnitStat intelligence = 10f;

    [Header("Scaling Multipliers")]
    public float hpPerStrength = 25f;
    public float armorPerAgility = 0.3f;
    public float magicResistPerIntelligence = 0.1f;
    public float manaPerIntelligence = 20f;

    [Header("Prefab")]
    public GameObject unitPrefab;

    #region Dynamic Properties (Динамический расчет характеристик)

    public float PrimaryAttributeValue
    {
        get
        {
            if (!heroUnit) return 0f;
            switch (damageAttribute)
            {
                case DamageAttribute.Strength: return strength.GetValue();
                case DamageAttribute.Agility: return agility.GetValue();
                case DamageAttribute.Intelligence: return intelligence.GetValue();
                default: return 0f;
            }
        }
    }

    public float MaxHitPoints(int level = 1) => heroUnit ? baseHitPoints.GetValue(level) + (strength.GetValue(level) * hpPerStrength) : baseHitPoints.GetValue(level);

    public float MaxMana(int level = 1) => heroUnit ? baseMana.GetValue(level) + (intelligence.GetValue(level) * manaPerIntelligence) : baseMana.GetValue(level);

    public float PhysicalDefense => heroUnit ? basePhysicalDefense.GetValue() + (agility.GetValue() * armorPerAgility) : basePhysicalDefense.GetValue();

    public float MagicDefense => heroUnit ? baseMagicDefense.GetValue() + (intelligence.GetValue() * magicResistPerIntelligence) : baseMagicDefense.GetValue();

    public UnitAttackStats ActiveAttackStats
    {
        get
        {
            UnitAttackStats stats;
            stats = GetUnitAttackStats(damageType);

            stats.primaryAttribute = PrimaryAttributeValue;
            return stats;
        }
    }

    public UnitAttackStats GetUnitAttackStats(DamageType damageType)
    {
        UnitAttackStats stats;
        switch (damageType)
        {
            case DamageType.Physical: stats = physical; break;
            case DamageType.Magical: stats = magical; break;
            case DamageType.Pure: stats = pure; break;
            default: stats = physical; break;
        }

        stats.diceCount = diceCount;
        stats.diceSides = diceSides;
        return stats;
    }

    #endregion
}
[System.Serializable]
public struct UnitStat
{
    public float baseValue;
    public float flatPerLevel;
    public float percentPerLevel;

    public float GetValue(int level = 1)
    {
        int levelDelta = Mathf.Max(0, level - 1);

        float flatBonus = flatPerLevel * levelDelta;

        float percentBonus = baseValue * (percentPerLevel * levelDelta);

        return baseValue + flatBonus + percentBonus;
    }

    public static implicit operator UnitStat(float v)
    {
        return new UnitStat { baseValue = v, flatPerLevel = 0f, percentPerLevel = 0f };
    }

    public static implicit operator float(UnitStat stat)
    {
        return stat.GetValue();
    }
}
[System.Serializable]
public struct UnitAttackStats
{
    public UnitStat baseDamage;

    [System.NonSerialized]
    public float primaryAttribute;

    public float bonusDamage;
    [System.NonSerialized]
    [Min(1)] public int diceCount;
    [System.NonSerialized]
    [Min(2)] public int diceSides;
    public float penetration;
    [Range(0f, 100f)] public float percentPenetration;

    public float TotalFlatBonus => primaryAttribute + bonusDamage;

    public float MinDamage => baseDamage.GetValue() + Mathf.Max(0, diceCount) + TotalFlatBonus;

    public float MaxDamage => baseDamage.GetValue() + (Mathf.Max(0, diceCount) * Mathf.Max(1, diceSides)) + TotalFlatBonus;

    public float AverageDamage => baseDamage.GetValue() + (Mathf.Max(0, diceCount) * (Mathf.Max(1, diceSides) + 1f) * 0.5f) + TotalFlatBonus;

    public float CalculateDamage(int level = 1)
    {
        float diceResult = 0f;
        if (diceCount > 0 && diceSides > 0)
        {
            for (int i = 0; i < diceCount; i++)
            {
                diceResult += Random.Range(1, diceSides + 1);
            }
        }

        return baseDamage.GetValue(level) + diceResult + TotalFlatBonus;
    }
}
public class Dice
{
    public static int GetMinValue(int diceCount, int diceSides)
    {
        return diceCount;
    }
    public static int GetMaxValue(int diceCount, int diceSides)
    {
        return diceCount * diceSides;
    }
    public static int GetRandomValue(int diceCount, int diceSides)
    {
        int result = 0;
        for (int i = 0; i < diceCount; i++)
        {
            result += Random.Range(1, diceSides + 1);
        }

        return result;
    }
}