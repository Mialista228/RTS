public struct DamageInfo
{
    public float damageAmount;
    public float critCoefficient;
    public float penetration;
    public float percentPenetration;
    public DamageType damageType;

    public DamageInfo(float damageAmount, float critCoefficient, float penetration, float percentPenetration, DamageType damageType)
    {
        this.damageAmount = damageAmount;
        this.critCoefficient = critCoefficient;
        this.penetration = penetration;
        this.percentPenetration = percentPenetration;
        this.damageType = damageType;
    }
}

public struct HealInfo
{
    public float healAmount;

    public HealInfo(float healAmount)
    {
        this.healAmount = healAmount;
    }
}