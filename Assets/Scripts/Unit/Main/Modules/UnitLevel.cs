using UnityEngine;
using UnityEngine.Events;

public class UnitLevel : UnitModule
{
    [Header("Module Settings")]
    [Min(1)][SerializeField] private int currentLevel = 1;
    [SerializeField] private int maxLevel = 15;
    [SerializeField] private float currentXP;
    public UnityEvent<int, float, float> OnXPChanged;
    public UnityEvent<int> OnLevelUp;

    public int CurrentLevel => currentLevel;
    public float CurrentExp => currentXP;
    private float RequiredXPForNextLevel => 100 * currentLevel + 100;
    public override void OnAwake(Unit unit)
    {
        base.OnAwake(unit);
    }

    public override void OnTick(float deltaInterval)
    {

    }

    public void AddXP(float xp)
    {
        currentXP += xp;
        OnXPChanged?.Invoke(currentLevel, currentXP, RequiredXPForNextLevel);
        if (maxLevel > currentLevel && currentXP > RequiredXPForNextLevel)
        {
            currentXP -= RequiredXPForNextLevel;
            currentLevel++;
            OnLevelUp?.Invoke(currentLevel);
        }
    }
}
