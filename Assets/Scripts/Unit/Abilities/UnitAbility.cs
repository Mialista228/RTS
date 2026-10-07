using UnityEngine;

public abstract class UnitAbility : ScriptableObject
{
    [Header("Base Info")]
    public string abilityName;
    public Sprite icon;
    public bool abilityPassive;
    public string abilityCustomBinding;
    [Min(1)] public int diceCount = 1;
    [Min(2)] public int diceSides = 2;
    public bool chargableAbility;
    public int maxCharges;
    public int maxLevel = 3;
    public float cooldown = 5f;
    public float manaCost = 20f;
    public float chargeCooldown = 6f;
    public bool needToAim;
    public string aimName;
    public float waitTime = 0.5f;
    public bool holdPlayerAfterCast;

    public abstract void Init(Unit caster);
    public abstract void Activate(Unit caster);
    public abstract void Aim(Unit caster, Vector3 pos);
}
