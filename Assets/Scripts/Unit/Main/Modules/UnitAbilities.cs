using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnitAbilities : UnitModule
{
    [Header("Module Settings")]
    [SerializeField] private float mana;
    [SerializeField] private float maxMana;
    [SerializeField] private float manaRegenSpeed = 1f;
    [SerializeField] private float manaRegenPercentSpeed = 0.1f;
    [SerializeField] private Transform shotPoint;
    [SerializeField] private AbilityObject[] customAbilityObjects;
    [SerializeField] private AbilityAimObject[] abilityAimObjects;
    [SerializeField] private AbilityDefaultRuntimeProperties[] abilityRuntimeProperties;
    [SerializeField] private int maxAbilitySlots = 4;
    [SerializeField] private List<UnitAbility> abilities = new List<UnitAbility>();

    [System.Serializable]
    public struct AbilityObject
    {
        public string objectName;
        public GameObject targetObject;
    }

    [System.Serializable]
    public struct AbilityAimObject
    {
        public string objectName;
        public bool needDirection;
        public GameObject targetObject;
    }

    [System.Serializable]
    public struct AbilityDefaultRuntimeProperties
    {
        [Min(0f)] public float abilityRuntime;
        [Min(0)] public int level;
        [Min(0f)] public float cooldown;
        [Min(0)] public int charges;
        [Min(0f)] public float chargeTimer;
    }

    public GameObject GetCustomAbilityObject(string name) => registeredCustomAbilityObjects.ContainsKey(name) ? registeredCustomAbilityObjects[name] : null;
    public AbilityAimObject GetAbilityAimObject(string name) => registeredAbilityAimObjects.ContainsKey(name) ? registeredAbilityAimObjects[name] : default;

    public UnityAction<UnitAbility, Vector3> onAbilityCasted;

    private List<UnityAction<Transform>> onUnitAttack;
    private List<UnityAction<Transform>> onUnitAttackBeforeFrame;

    private Dictionary<string, GameObject> registeredCustomAbilityObjects;
    private Dictionary<string, AbilityAimObject> registeredAbilityAimObjects;
    private Dictionary<string, UnitAbility> registeredAbilities;

    private bool isAiming;

    private float deltaTimeRegen = 1f;

    public Transform ShotPoint => shotPoint;

    public AbilityDefaultRuntimeProperties[] RegisteredAbilities => abilityRuntimeProperties;

    public List<UnitAbility> Abilities => abilities;

    public int RequiredLevel { get; private set; }

    public int CurrentUsedLevel { get { int result = 0; if (abilityRuntimeProperties != null || abilityRuntimeProperties.Length > 0) for (int i = 0; i < abilityRuntimeProperties.Length; i++) result += abilityRuntimeProperties[i].level; return result; } }

    public float Mana => mana;

    public float MaxMana => maxMana;

    public GameObject SpawnObject(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        return Instantiate(prefab, pos, rot);
    }

    int abilitiesCount => Mathf.Min(maxAbilitySlots, abilities.Count);

    public override void OnAwake(Unit unit)
    {
        base.OnAwake(unit);

        onUnitAttack = new List<UnityAction<Transform>>(abilitiesCount);
        onUnitAttackBeforeFrame = new List<UnityAction<Transform>>(abilitiesCount);

        registeredCustomAbilityObjects = new Dictionary<string, GameObject>();

        maxMana = unit.MaxMana;
        mana = maxMana;

        manaRegenSpeed = unit.ManaRegen;
        manaRegenPercentSpeed = unit.ManaRegenPercent;

        for (int i = 0; i < customAbilityObjects.Length; i++)
        {
            registeredCustomAbilityObjects.Add(customAbilityObjects[i].objectName, customAbilityObjects[i].targetObject);
        }

        registeredAbilityAimObjects = new Dictionary<string, AbilityAimObject>();

        for (int i = 0; i < abilityAimObjects.Length; i++)
        {
            registeredAbilityAimObjects.Add(abilityAimObjects[i].objectName, abilityAimObjects[i]);
        }

        registeredAbilities = new Dictionary<string, UnitAbility>();

        if (abilityRuntimeProperties == null || abilityRuntimeProperties.Length == 0)
            abilityRuntimeProperties = new AbilityDefaultRuntimeProperties[abilitiesCount];

        for (int i = 0; i < abilitiesCount; i++)
        {
            int index = i;

            UnityAction<Transform> attackSlot = (target) => onUnitAttack[index]?.Invoke(target);
            UnityAction<Transform> beforeFrameSlot = (target) => onUnitAttackBeforeFrame[index]?.Invoke(target);

            onUnitAttack.Add(null);
            onUnitAttackBeforeFrame.Add(null);

            if (unit.Attack != null)
            {
                unit.Attack.onAttack += attackSlot;
                unit.Attack.onAttackBeforeFrame += beforeFrameSlot;
            }

            if (abilities[i] != null)
            {
                abilities[i].Init(unit);
            }

            registeredAbilities.Add(abilities[i].abilityName, abilities[i]);

            RequiredLevel += abilities[i].maxLevel;
        }
    }

    public void SetAbilityAttackAction(UnitAbility ability, UnityAction<Transform> action)
    {
        int index = abilities.IndexOf(ability);
        if (index != -1 && index < onUnitAttack.Count)
        {
            onUnitAttack[index] = action;
        }
    }
    public void SetAbilityBeforeFrameAction(UnitAbility ability, UnityAction<Transform> action)
    {
        int index = abilities.IndexOf(ability);
        if (index != -1 && index < onUnitAttackBeforeFrame.Count)
        {
            onUnitAttackBeforeFrame[index] = action;
        }
    }

    public void ClearAbilityAttackActions(UnitAbility ability)
    {
        int index = abilities.IndexOf(ability);
        if (index != -1)
        {
            if (index < onUnitAttack.Count) onUnitAttack[index] = null;
            if (index < onUnitAttackBeforeFrame.Count) onUnitAttackBeforeFrame[index] = null;
        }
    }

    AbilityAimObject currentAimObject;

    UnityAction<Vector3> onEndAim;

    public void StartAim(string aimName, UnityAction<Vector3> onEndAim)
    {
        isAiming = true;
        this.onEndAim = onEndAim;
        if (!string.IsNullOrEmpty(aimName))
        {
            currentAimObject = GetAbilityAimObject(aimName);

            if (currentAimObject.targetObject != null)
                currentAimObject.targetObject.SetActive(true);
        }
    }

    public void EndAim(Vector3 direction)
    {
        isAiming = false;
        onEndAim?.Invoke(direction);
        onEndAim = null;

        if (currentAimObject.targetObject != null)
        {
            currentAimObject.targetObject.SetActive(false);
            currentAimObject = default;
        }
    }

    public void CancelAim()
    {
        isAiming = false;
        onEndAim = null;

        if (currentAimObject.targetObject != null)
        {
            currentAimObject.targetObject.SetActive(false);
            currentAimObject = default;
        }
    }

    public override void OnStart() { }
    void RegenMana(float deltaInterval)
    {
        if (mana < maxMana)
        {
            deltaTimeRegen -= deltaInterval;
            if (deltaTimeRegen <= 0f)
            {
                FillMana(manaRegenSpeed + maxMana * manaRegenPercentSpeed / 100f);
                deltaTimeRegen = 1f;
            }
        }
    }
    public void FillMana(float amount)
    {
        mana += amount;
    }
    public override void OnTick(float deltaInterval)
    {
        ReloadAbility(deltaInterval);
        ChargableAbility(deltaInterval);
        RegenMana(deltaInterval);

        if (!isAiming) return;

        Ray ray = Camera.main.ScreenPointToRay(Player.Instance.CurrentMousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 direction = hitPoint - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                direction.Normalize();

                Quaternion targetRotation = Quaternion.LookRotation(direction);

                if (currentAimObject.targetObject != null)
                    if (currentAimObject.needDirection)
                        currentAimObject.targetObject.transform.rotation = Quaternion.Slerp(
                            currentAimObject.targetObject.transform.rotation,
                            targetRotation,
                            deltaInterval * 25f
                        );
                    else
                        currentAimObject.targetObject.transform.position = hitPoint;

                if (Input.GetMouseButtonDown(0))
                {
                    EndAim(currentAimObject.needDirection ? direction : hitPoint);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelAim();
        }
    }

    public void ReloadAbility(float deltaInterval)
    {
        for (int i = 0; i < abilityRuntimeProperties.Length; i++)
        {
            if (abilityRuntimeProperties[i].cooldown > 0f)
            {
                abilityRuntimeProperties[i].cooldown -= deltaInterval;
            }
        }
    }

    public void ChargableAbility(float deltaInterval)
    {
        for (int i = 0; i < abilityRuntimeProperties.Length; i++)
        {
            if (abilities[i].chargableAbility)
            {
                if (abilityRuntimeProperties[i].level > 0)
                {
                    if (abilityRuntimeProperties[i].charges < abilities[i].maxCharges)
                    {
                        abilityRuntimeProperties[i].chargeTimer += deltaInterval;
                        if (abilityRuntimeProperties[i].chargeTimer > abilities[i].chargeCooldown)
                        {
                            abilityRuntimeProperties[i].chargeTimer = 0f;
                            abilityRuntimeProperties[i].charges++;
                        }
                    }
                }
            }
        }
    }

    public void SetDelayedAction(UnityAction act, float delay)
    {
        StartCoroutine(SetDelayedActionCoroutine(act, delay));
    }

    IEnumerator SetDelayedActionCoroutine(UnityAction act, float delay)
    {
        yield return new WaitForSeconds(delay);
        act?.Invoke();
    }

    public void AbilityUsed(string abilityName, float waitTime)
    {
        for (int i = 0; i < abilitiesCount; i++)
        {
            if (abilities[i].abilityName == abilityName)
            {
                abilityRuntimeProperties[i].cooldown = abilities[i].cooldown;
                if (abilities[i].chargableAbility)
                {
                    abilityRuntimeProperties[i].charges--;
                }
            }
        }
        if (unit.Attack is UnitAttack attack)
        {
            attack.LockAttack(waitTime);
        }
        onAbilityCasted?.Invoke(null, Vector3.zero);
    }

    public void CastAbility(string abilityName)
    {
        for (int i = 0; i < abilitiesCount; i++)
        {
            if (abilities[i].abilityName == abilityName)
            {
                if (abilities[i].abilityPassive == false)
                {
                    if (abilities[i].manaCost < mana)
                    {
                        if (abilityRuntimeProperties[i].cooldown <= 0f)
                        {
                            if (abilities[i].chargableAbility == false || (abilities[i].chargableAbility && abilityRuntimeProperties[i].charges > 0))
                                if (abilities[i].needToAim)
                                    AimAbility(abilities[i]);
                                else
                                    abilities[i].Activate(unit);
                        }
                    }
                }
            }
        }
    }

    public void ReduceMana(float value)
    {
        mana -= value;
    }
    public void AimAbility(UnitAbility ability)
    {
        StartAim(ability.aimName, (direction) => { ability.Aim(unit, direction); });
    }
}