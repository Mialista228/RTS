using UnityEngine;

public abstract class UnitBuff : ScriptableObject
{
    public string buffName;
    public Sprite buffIcon;
    public BuffType buffType;
    public bool isDebuff;
    public float buffLifetime = 3f;
    public BuffStat[] buffingStats;
    [Min(1)] public int diceCount = 1;
    [Min(2)] public int diceSides = 2;
    public bool stackingBuff;
    public int maxStacks;

    public abstract void OnAwake(Unit unit);

    public abstract void OnTick(Unit unit, float deltaInterval);

    public abstract void OnRemove(Unit unit);
}
[System.Serializable]
public struct BuffUnitStat
{
    public BuffStat buffStat;
    public float value;
    public float percentValue;
}
public enum BuffStat
{
    MoveSpeed,
    Health,
    Mana,
    PhysicalDefense,
    MagicDefense,
    DamageReduction,
    AttackRange,
    AttackSpeed,
    CritChance,
    CritDMG,
    Strength,
    Agility,
    Intelligence,
}