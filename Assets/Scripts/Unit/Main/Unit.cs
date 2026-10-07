using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
#endif

[SelectionBase]
public class Unit : EruBehaviour, IUnit
{

    [Header("Unit Info")]
    [SerializeField] private uint id;
    [SerializeField] private int team;
    [SerializeField] private UnitData unitData;
    [SerializeField] private Circle circle;

    [Header("Unit Settings")]
    [SerializeField] private List<UnitModule> modules = new List<UnitModule>();

    [Header("Events")]
    [SerializeField] private UnityEngine.Events.UnityEvent onMouseEnter;
    [SerializeField] private UnityEngine.Events.UnityEvent onMouseExit;
    [SerializeField] private UnityEngine.Events.UnityEvent onSelected;
    [SerializeField] private UnityEngine.Events.UnityEvent onDeselected;

    public IMoveable Movement { get; private set; }
    public IAttackable Attack { get; private set; }
    public UnitHealth DamageReceiver { get; private set; }
    public UnitBrain Brain { get; private set; }
    public UnitData UnitData => unitData;
    public UnitVFX VFX { get; private set; }
    public UnitSFX SFX { get; private set; }
    public UnitAbilities Abilities { get; private set; }
    public UnitHero Hero { get; private set; }
    public UnitLevel Level { get; private set; }
    public UnitStats Stats { get; private set; }

    public UnitData.UnitSurface MovementSurface => unitData ? unitData.movementSurface : UnitData.UnitSurface.Ground;

    public string UnitLevelText
    {
        get
        {
            if (Level)
            {
                return $"Level {Level.CurrentLevel} {Nickname}";
            }
            return "";
        }
    }
    public float UnitXP
    {
        get
        {
            if (Level)
            {
                return Level.CurrentExp;
            }
            return 0f;
        }
    }

    public bool IsNeutralEnemyUnit => team == 1;

    #region Unit Combat Stats

    public UnitAttackStats GetUnitAttackStats(DamageType damageType)
    {
        if (unitData == null)
        {
            return default;
        }
        if (Stats != null)
        {
            return Stats.GetUnitAttackStats(damageType);
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
            if (Stats != null)
                return Stats.PrimaryAttributeValue;
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
            if (Stats != null)
                return Stats.AttackRange;
            return Level ? unitData.attackRange.GetValue(Level.CurrentLevel) : unitData.attackRange.GetValue();
        }
    }
    public float AttackFrame
    {
        get
        {
            if (unitData == null)
                return 0.2f;
            if (Stats != null)
                return Stats.AttackFrame;
            return Level ? unitData.attackFrame.GetValue(Level.CurrentLevel) : unitData.attackFrame.GetValue();
        }
    }
    public float AttackRecoverFrame
    {
        get
        {
            if (unitData == null)
                return 0.5f;
            if (Stats != null)
                return Stats.AttackRecoverFrame;
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
            if (Stats != null)
                return Stats.CritChance;
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
            if (Stats != null)
                return Stats.CritDMG;
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
            if (Stats != null)
                return Stats.Strength;
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
            if (Stats != null)
                return Stats.Agility;
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
            if (Stats != null)
                return Stats.Intelligence;
            if (unitData.heroUnit)
                return Level ? unitData.intelligence.GetValue(Level.CurrentLevel) : unitData.intelligence.GetValue();
            return 0f;
        }
    }

    #endregion

    #region Unit Health Stats

    public float Health => DamageReceiver != null ? DamageReceiver.CurrentHP : 0f;
    public float MaxHealth => unitData ? (Stats != null ? Stats.MaxHealth : unitData.MaxHitPoints) : (DamageReceiver != null ? DamageReceiver.MaxHP : 0f);
    public float Mana => 200f;
    public float MaxMana => 200f;

    public float PhysicalDefense
    {
        get
        {
            if (unitData == null)
                return 0f;
            if (Stats != null)
                return Stats.PhysicalDefense;
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
            if (Stats != null)
                return Stats.MagicalDefense;
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

    public uint Id => id;
    public string Name => unitData ? unitData.unitName : "Default Unit";
    public string Nickname => unitData ? unitData.unitNickname : "Default Unit Nickname";
    public string Description => unitData ? unitData.unitDescription : "Lorem...";
    public int Team => team;

    public bool IsSelected { get; private set; }

    private void OnValidate()
    {
        if (modules == null || modules.Count == 0)
            return;
        for (int i = 0; i < modules.Count; i++)
        {
            if (modules[i] == null)
                continue;
            if (modules[i].gameObject != gameObject)
            {
                Debug.LogError("Unit module is not on the same game object as the unit");
                modules[i] = null;
                continue;
            }
        }
    }
    public void Initialize()
    {
        GameplayManager.RegisterUnit(this);
    }
    public void AssignId(uint id)
    {
        this.id = id;
    }
    void Awake()
    {
        for (int i = 0; i < modules.Count; i++)
        {
            if (modules[i] != null)
            {
                if (modules[i] is UnitMovement movement)
                {
                    if (Movement != null)
                        Debug.LogError("Unit has more than one movement module");
                    else
                        Movement = movement;
                }
                if (modules[i] is UnitAttack attack)
                {
                    if (Attack != null)
                        Debug.LogError("Unit has more than one attack module");
                    else
                        Attack = attack;
                }
                if (modules[i] is UnitHealth damageReceiver)
                {
                    if (DamageReceiver != null)
                        Debug.LogError("Unit has more than one damage receiver module");
                    else
                        DamageReceiver = damageReceiver;
                }
                if (modules[i] is UnitBrain brain)
                {
                    if (Brain != null)
                        Debug.LogError("Unit has more than one brain module");
                    else
                        Brain = brain;
                }
                if (modules[i] is UnitAbilities abilities)
                {
                    if (Abilities != null)
                        Debug.LogError("Unit has more than one abilities module");
                    else
                        Abilities = abilities;
                }
                if (modules[i] is UnitVFX vfx)
                {
                    if (VFX != null)
                        Debug.LogError("Unit has more than one VFX module");
                    else
                        VFX = vfx;
                }
                if (modules[i] is UnitSFX sfx)
                {
                    if (SFX != null)
                        Debug.LogError("Unit has more than one VFX module");
                    else
                        SFX = sfx;
                }
                if (modules[i] is UnitLevel level)
                {
                    if (Level != null)
                        Debug.LogError("Unit has more than one level module");
                    else
                        Level = level;
                }
                if (modules[i] is UnitStats stats)
                {
                    if (Stats != null)
                        Debug.LogError("Unit has more than one stats module");
                    else
                        Stats = stats;
                }
                if (unitData != null && unitData.heroUnit)
                {
                    if (modules[i] is UnitHero hero)
                    {
                        if (Hero != null)
                            Debug.LogError("Unit has more than one hero module");
                        else
                            Hero = hero;
                    }
                }
                modules[i].OnAwake(this);
            }
        }
    }
    [ContextMenu("Auto Assign Creep Unit Modules")]
    private void CreepConfig()
    {
        if (modules.Count > 0)
        {
            bool[] availableModules = new bool[6];
            for (int i = 0; i < modules.Count; i++)
            {
                if (modules[i] is IAttackable)
                    availableModules[0] = true;
                if (modules[i] is IMoveable)
                    availableModules[1] = true;
                if (modules[i] is IDamageReceiver)
                    availableModules[2] = true;
                if (modules[i] is UnitBrain)
                    availableModules[3] = true;
                if (modules[i] is UnitStats)
                    availableModules[4] = true;
                if (modules[i] is UnitStatus)
                    availableModules[5] = true;
            }
            if (availableModules[0] == false)
                modules.Add(gameObject.AddComponent<UnitAttack>());
            if (availableModules[1] == false)
                modules.Add(gameObject.AddComponent<UnitMovement>());
            if (availableModules[2] == false)
                modules.Add(gameObject.AddComponent<UnitHealth>());
            if (availableModules[3] == false)
                modules.Add(gameObject.AddComponent<UnitBrain>());
            if (availableModules[4] == false)
                modules.Add(gameObject.AddComponent<UnitStats>());
            if (availableModules[5] == false)
                modules.Add(gameObject.AddComponent<UnitStatus>());
        }
        else
        {
            UnitAttack attack = gameObject.AddComponent<UnitAttack>();
            UnitMovement brain = gameObject.AddComponent<UnitMovement>();
            UnitHealth health = gameObject.AddComponent<UnitHealth>();
            UnitBrain movement = gameObject.AddComponent<UnitBrain>();
            UnitStats unitStats = gameObject.AddComponent<UnitStats>();
            UnitStatus unitStatus = gameObject.AddComponent<UnitStatus>();

            modules.AddRange(new List<UnitModule> { attack, brain, health, movement, unitStats, unitStatus });
        }


    }
    [ContextMenu("Auto Assign All Unit Modules Of Object")]
    private void AssignModules()
    {
        UnitModule[] cachedModules = GetComponents<UnitModule>();
        for (int i = 0; i < cachedModules.Length; i++)
        {
            if (!modules.Contains(cachedModules[i]))
            {
                modules.Add(cachedModules[i]);
            }
        }
    }
#if UNITY_EDITOR
    [ContextMenu("Auto Assign Unit FX Modules")]
    private void UnitVisualConfig()
    {
        // 1. Помечаем объект для Undo и сохранения
        Undo.RegisterCompleteObjectUndo(gameObject, "Auto Assign FX Modules");

        // 2. Безопасно получаем или добавляем SFX и VFX
        UnitSFX sfx = GetComponent<UnitSFX>();
        if (sfx == null) sfx = Undo.AddComponent<UnitSFX>(gameObject);

        UnitVFX vfx = GetComponent<UnitVFX>();
        if (vfx == null) vfx = Undo.AddComponent<UnitVFX>(gameObject);

        // Гарантируем их наличие в списке модулей без дубликатов
        if (!modules.Contains(sfx)) modules.Add(sfx);
        if (!modules.Contains(vfx)) modules.Add(vfx);

        // 3. Находим все модули атаки и привязываем события
        for (int i = 0; i < modules.Count; i++)
        {
            if (modules[i] is UnitAttack attack)
            {
                // Очищаем прошлые постоянные подписки, чтобы не плодить дубликаты
                CleanUpPersistentListeners(attack);

                // Добавляем постоянные (Persistent) подписки, которые сохраняются в сцену/префаб
                UnityEventTools.AddPersistentListener(attack.OnAttack, (value) => sfx.BasicAttack());
                UnityEventTools.AddPersistentListener(attack.OnAttack, (value) => vfx.BasicAttackEffect());
                UnityEventTools.AddPersistentListener(attack.OnAttackBeforeFrame, (value) => vfx.BasicAttackAnimation());

                EditorUtility.SetDirty(attack);
            }
        }

        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(gameObject);
        Debug.Log("[UnitVisualConfig] FX модули и события успешно авто-назначены!", this);
    }

    private void CleanUpPersistentListeners(UnitAttack attack)
    {
        // Очищаем события, созданные в редакторе, чтобы при повторном клике не спамить подписками
        while (attack.OnAttack.GetPersistentEventCount() > 0)
        {
            UnityEventTools.RemovePersistentListener(attack.OnAttack, 0);
        }
        while (attack.OnAttackBeforeFrame.GetPersistentEventCount() > 0)
        {
            UnityEventTools.RemovePersistentListener(attack.OnAttackBeforeFrame, 0);
        }
    }
#endif
    private void OnDestroy()
    {
        GameplayManager.UnregisterUnit(this);
    }
    void Start()
    {
        for (int i = 0; i < modules.Count; i++)
        {
            if (modules[i] != null)
            {
                modules[i].OnStart();
            }
        }
        if (circle)
            circle.SetActive(false);
    }

    public void SendCommand(ICommand command)
    {
        command.Start(this);
    }

    public override void OnTick(float deltaInterval)
    {
        for (int i = 0; i < modules.Count; i++)
        {
            if (modules[i] != null)
            {
                modules[i].OnTick(deltaInterval);
            }
        }
    }

    public void SetCircle(Circle circle)
    {
        this.circle = circle;
    }

    public void Select()
    {
        IsSelected = true;
        onSelected?.Invoke();
        if (circle)
            circle.SetActive(true);
    }

    public void Deselect()
    {
        IsSelected = false;
        onDeselected?.Invoke();
        if (circle)
            circle.SetActive(false);
    }

    public void OnHoverEnter()
    {
        onMouseEnter?.Invoke();
        if (circle)
            if (IsSelected == false)
                circle.SetActive(true);
    }

    public void OnHoverExit()
    {
        onMouseExit?.Invoke();
        if (circle)
            if (IsSelected == false)
                circle.SetActive(false);
    }

    public void Enqueue(ICommand command)
    {
        if (Brain != null)
        {
            Brain.Enqueue(command);
            return;
        }
    }

    public bool RemoveAt(int index)
    {
        return false;
    }

    public void ExecuteNext()
    {

    }

    public void Clear()
    {
        if (Brain != null)
        {
            Brain.Clear();
            return;
        }
    }


}
