using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using NaughtyAttributes;

public class HUD : EruBehaviour
{
    [Header("Core")]
    [SerializeField] private Camera viewCamera;
    [SerializeField] private Player player;
    [SerializeField] private string physicalHEX = "FFFFFFFF";
    [SerializeField] private string magicalHEX = "FFFFFFFF";
    [SerializeField] private Gradient healthColorGradient;

    [Space(10f)]

    [Foldout(prefabsGroup)][SerializeField] private GameObject damageText;

    [Foldout(groupElements)][SerializeField] private GameObject unitStats;
    [Foldout(groupElements)][SerializeField] private GameObject defaultUnitMainStats;
    [Foldout(groupElements)][SerializeField] private GameObject buildingUnitMainStats;
    [Foldout(groupElements)][SerializeField] private GameObject[] heroUnitStats;

    [Foldout(closedElement)][SerializeField] private GameObject inventoryClosed;
    [Foldout(closedElement)][SerializeField] private GameObject actionsClosed;
    [Foldout(closedElement)][SerializeField] private GameObject temporarilyDisabledActions;
    [Foldout(closedElement)][SerializeField] private Button cancelButton;

    [Foldout(portraitTextElements)][SerializeField] private TextMeshProUGUI health;
    [Foldout(portraitTextElements)][SerializeField] private TextMeshProUGUI mana;

    [Foldout(unitMainStats)][SerializeField] private TextMeshProUGUI unitName;
    [Foldout(unitMainStats)][SerializeField] private TextMeshProUGUI attack;
    [Foldout(unitMainStats)][SerializeField] private TextMeshProUGUI[] defense;
    [Foldout(unitMainStats)][SerializeField] private TextMeshProUGUI penetration;
    [Foldout(unitMainStats)][SerializeField] private TextMeshProUGUI crit;
    [Foldout(unitMainStats)][SerializeField] private TextMeshProUGUI food;

    [Foldout(unitHeroMainStats)][SerializeField] private Slider levelSlider;
    [Foldout(unitHeroMainStats)][SerializeField] private TextMeshProUGUI levelText;
    [Foldout(unitHeroMainStats)][SerializeField] private TextMeshProUGUI strength;
    [Foldout(unitHeroMainStats)][SerializeField] private TextMeshProUGUI agility;
    [Foldout(unitHeroMainStats)][SerializeField] private TextMeshProUGUI intelligence;

    [Header("UI Action Buttons Elements")]
    [SerializeField] private ActionButton[] actionButtons;

    [Space(10f)]

    private Unit selectedUnit;

    private bool upgradeSkills;

    private UnityAction onCancel;

    public static HUD Instance;

    private const string prefabsGroup = "UI Element Prefabs";
    private const string groupElements = "UI Group Elements";
    private const string closedElement = "UI Closed Element";
    private const string portraitTextElements = "UI Portrait Text Elements";
    private const string unitMainStats = "UI Default Unit Main Stats Text Elements";
    private const string unitHeroMainStats = "UI Hero Unit Main Stats Text Elements";

    [System.Serializable]
    public struct ActionButton
    {
        public Sprite defaultIcon;
        public Color defaultIconColor;

        public Button button;
        public Image icon;
        public GameObject executingBorder;
        public TextMeshProUGUI cooldownText;
        public Image cooldownFillImage;
        public GameObject chargesFillObject;
        public GameObject chargesTextObject;
        public TextMeshProUGUI chargesText;
        public Image chargesFillImage;
        public UnityAction onClick;

        public void SetActive(bool active)
        {
            button.gameObject.SetActive(active);
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        Reset();
        AwakeSetupButtons();
        cancelButton.onClick.AddListener(onCancel);
    }

    public static void DamageTaken(Transform target, DamageShortInfo damage)
    {
        GameObject go = Instantiate(Instance.damageText, Vector3.zero, Quaternion.identity);
        go.transform.SetParent(Instance.transform);
        go.GetComponent<DamageText>().Initialize(damage, target, Instance.viewCamera);
    }

    void Start()
    {
        Player.Instance.OnUnitSelected += (unit) => UnitStats();
    }

    void Reset()
    {
        inventoryClosed.SetActive(true);
        actionsClosed.SetActive(true);
        unitStats.SetActive(false);

        health.text = "";
        mana.text = "";
    }

    void AwakeSetupButtons()
    {
        for (int i = 0; i < actionButtons.Length; i++)
        {
            int index = i;
            actionButtons[i].button.onClick.RemoveAllListeners();
            actionButtons[i].button.onClick.AddListener(() => actionButtons[index].onClick?.Invoke());
            actionButtons[i].chargesFillObject.SetActive(false);
            actionButtons[i].chargesTextObject.SetActive(false);
            actionButtons[i].executingBorder.SetActive(false);
            actionButtons[i].SetActive(false);
        }
    }

    private static readonly int[] AbilitySlotIndices = { 8, 9, 10, 11, 5, 6 };

    void ResetButtons()
    {
        for (int i = 0; i < actionButtons.Length; i++)
        {
            actionButtons[i].onClick = null;
            actionButtons[i].chargesFillObject.SetActive(false);
            actionButtons[i].chargesTextObject.SetActive(false);
            actionButtons[i].executingBorder.SetActive(false);
            actionButtons[i].SetActive(false);
        }
    }

    void SetupButtons()
    {
        if (selectedUnit == null)
        {
            ResetButtons();
            return;
        }

        bool isPlayerUnit = Player.IsPlayerUnit(selectedUnit);

        if (!isPlayerUnit)
        {
            for (int i = 0; i < actionButtons.Length; i++)
                actionButtons[i].SetActive(false);
            return;
        }

        bool movementAvailable = selectedUnit.Movement != null;
        bool attackAvailable = selectedUnit.Attack != null;
        bool abilityAvailable = selectedUnit.Abilities != null;

        //Basic Commands
        actionButtons[0].SetActive(movementAvailable);                                            // ---> Перемещение
        actionButtons[0].onClick = () => { Player.Instance.AimDefaultActions(0); };

        actionButtons[1].SetActive(attackAvailable || movementAvailable || abilityAvailable);     // ---> Отставить
        actionButtons[1].onClick = () => { Player.Instance.AimDefaultActions(1); };

        actionButtons[2].SetActive(movementAvailable && attackAvailable);                         // ---> Удерживать позицию
        actionButtons[2].onClick = () => { Player.Instance.AimDefaultActions(2); };

        actionButtons[3].SetActive(attackAvailable);                                              // ---> Атака
        actionButtons[3].onClick = () => { Player.Instance.AimDefaultActions(3); };

        actionButtons[4].SetActive(movementAvailable);                                            // ---> Патрулирование
        actionButtons[4].onClick = () => { Player.Instance.AimDefaultActions(4); };

        //Ability Upgrader
        if (selectedUnit.Hero)
        {
            actionButtons[7].SetActive((selectedUnit.Abilities.CurrentUsedLevel - selectedUnit.Abilities.RequiredLevel) < 0);
            actionButtons[7].chargesText.text = (selectedUnit.Level.CurrentLevel - selectedUnit.Abilities.CurrentUsedLevel).ToString();
            actionButtons[7].chargesTextObject.SetActive((selectedUnit.Level.CurrentLevel - selectedUnit.Abilities.CurrentUsedLevel) > 0);
            actionButtons[7].onClick = UpgradeSkillsSection;
        }
        else
        {
            actionButtons[7].SetActive(false);
        }

        Debug.Log($"{selectedUnit.Abilities.CurrentUsedLevel} | {selectedUnit.Abilities.RequiredLevel}");

        //Abilities
        if (abilityAvailable)
        {
            UnitAbilities.AbilityDefaultRuntimeProperties[] abilities = selectedUnit.Abilities.RegisteredAbilities;
            for (int i = 0; i < Mathf.Min(abilities.Length, AbilitySlotIndices.Length); i++)
            {
                UnitAbility ability = selectedUnit.Abilities.Abilities[i];

                int buttonIndex = AbilitySlotIndices[i];

                bool isActive = abilities[i].level > 0;

                if (isActive)
                {
                    actionButtons[buttonIndex].icon.sprite = ability.icon;
                    if (ability.abilityPassive == false)
                    {
                        bool chargable = ability.chargableAbility;

                        actionButtons[buttonIndex].chargesFillObject.SetActive(chargable);
                        actionButtons[buttonIndex].chargesTextObject.SetActive(chargable);

                        if (chargable)
                        {
                            actionButtons[buttonIndex].chargesText.text = chargable ? abilities[i].charges.ToString() : "";

                            actionButtons[buttonIndex].chargesFillImage.fillAmount = Mathf.Clamp01(abilities[i].chargeTimer / ability.chargeCooldown);
                        }

                        actionButtons[buttonIndex].cooldownFillImage.fillAmount = Mathf.Clamp01(abilities[i].cooldown / ability.cooldown);
                    }
                    else
                    {
                        actionButtons[buttonIndex].chargesFillObject.SetActive(false);
                        actionButtons[buttonIndex].chargesTextObject.SetActive(false);
                    }
                }

                actionButtons[buttonIndex].executingBorder.SetActive(false);

                actionButtons[buttonIndex].onClick = isActive && ability.abilityPassive == false ? (() => selectedUnit.Abilities.CastAbility(ability.abilityName)) : null;

                actionButtons[buttonIndex].SetActive(isActive);
            }
        }
    }

    void UpgradeSkillsSection()
    {
        ResetButtons();
    }

    void UpdateActionButtons()
    {
        if (selectedUnit == null)
            return;
        actionButtons[0].executingBorder.SetActive(selectedUnit.Brain.CurrentState == UnitBrain.UnitState.Move);
        actionButtons[1].executingBorder.SetActive(selectedUnit.Brain.CurrentState == UnitBrain.UnitState.Idle);
        actionButtons[2].executingBorder.SetActive(selectedUnit.Brain.CurrentState == UnitBrain.UnitState.HoldPosition);
        actionButtons[3].executingBorder.SetActive(selectedUnit.Brain.CurrentState == UnitBrain.UnitState.Attack);
        actionButtons[4].executingBorder.SetActive(selectedUnit.Brain.CurrentState == UnitBrain.UnitState.Patrol || selectedUnit.Brain.CurrentState == UnitBrain.UnitState.Follow);
    }

    public override void OnTick(float deltaInterval)
    {
        UnitStats();
        UpdateActionButtons();
    }

    public void DisableActions(bool disable)
    {
        temporarilyDisabledActions.SetActive(!disable);
    }

    public void StartAim(UnityAction onCancel)
    {
        this.onCancel = onCancel;
        temporarilyDisabledActions.SetActive(true);
        cancelButton.gameObject.SetActive(true);
    }

    public void StopAim()
    {
        temporarilyDisabledActions.SetActive(false);
        cancelButton.gameObject.SetActive(false);
        onCancel = null;
    }

    void UnitStats()
    {
        selectedUnit = player.selected.Count > 0 ? player.selected[0] as Unit : null;
        if (player.selected.Count == 0)
        {
            inventoryClosed.SetActive(true);
            actionsClosed.SetActive(true);
            unitStats.SetActive(false);
            return;
        }
        actionsClosed.SetActive(false);
        SetupButtons();
        if (player.selected.Count > 2)
        {
            unitStats.SetActive(false);
            return;
        }
        unitStats.SetActive(true);
        for (int i = 0; i < heroUnitStats.Length; i++)
            heroUnitStats[i].SetActive(player.selected[0].UnitData ? player.selected[0].UnitData.heroUnit : false);
        if (selectedUnit)
        {
            UnitAttackStats physicalDamage = default;
            UnitAttackStats magicalDamage = default;
            if (selectedUnit.Attack != null)
            {
                physicalDamage = selectedUnit.GetUnitAttackStats(DamageType.Physical);
                magicalDamage = selectedUnit.GetUnitAttackStats(DamageType.Magical);
            }

            unitName.text = selectedUnit.Name;
            levelText.text = selectedUnit.UnitLevelText;
            attack.text = $"Атака\n<color=#{physicalHEX}>{physicalDamage.MinDamage} - {physicalDamage.MaxDamage}</color>\n<color=#{magicalHEX}>{magicalDamage.MinDamage} - {magicalDamage.MaxDamage}</color>";
            penetration.text = $"Проник.\n<color=#{physicalHEX}>{physicalDamage.penetration}|{physicalDamage.percentPenetration}%</color>\n<color=#{magicalHEX}>{magicalDamage.penetration}|{magicalDamage.percentPenetration}%</color>";
            crit.text = $"Криты\n{selectedUnit.CritChance}%|{selectedUnit.CritDMG}%";
            strength.text = $"Сила\n{selectedUnit.Strength}";
            agility.text = $"Ловкость\n{selectedUnit.Agility}";
            intelligence.text = $"Разум\n{selectedUnit.Intelligence}";

            health.text = $"{Mathf.Ceil(selectedUnit.Health)} / {Mathf.Ceil(selectedUnit.MaxHealth)}";
            health.color = healthColorGradient.Evaluate(selectedUnit.Health / selectedUnit.MaxHealth);
            mana.text = $"{Mathf.Ceil(selectedUnit.Mana)} / {Mathf.Ceil(selectedUnit.MaxMana)}";

            for (int i = 0; i < defense.Length; i++)
                defense[i].text = $"Защита\n<color=#{physicalHEX}>{selectedUnit.PhysicalDefense}</color>\n<color=#{magicalHEX}>{selectedUnit.MagicalDefense}</color>";
        }
    }
}
