using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : EruBehaviour
{
    public static GameplayManager Instance;

    public static uint UnitNextID;

    private static readonly Dictionary<uint, Unit> AllUnitsById = new Dictionary<uint, Unit>();

    public static Material GetUnitCircleMaterial(int teamA, int teamB)
    {
        Material result = null;

        if (teamA == teamB)
        {
            result = Instance.playerMaterial;
        }
        else if (IsAlly(teamA, teamB))
        {
            result = Instance.allyMaterial;
        }
        else
        {
            result = Instance.enemyMaterial;
        }

        return result;
    }

    [SerializeField] private Material enemyMaterial;
    [SerializeField] private Material allyMaterial;
    [SerializeField] private Material playerMaterial;

    [SerializeField] private List<Team> teams = new List<Team>(2);
    [SerializeField] private bool multiplayerMode;
    [SerializeField] private bool unifiedUnits;

    [System.Serializable]
    public class Team
    {
        [HideInInspector]
        public string name;
        public ushort clan;
        public Color teamColor = Color.white;
        public List<Unit> units = new List<Unit>();
        public uint gold = 500;
        public uint lumbers = 150;
        public uint food = 12;
        public uint GetReservedFoods()
        {
            uint totalFood = 0;
            for (int i = 0; i < units.Count; i++)
            {
                Unit u = units[i];

                if (u != null && u.UnitData != null)
                {
                    totalFood += u.UnitData.foodCost;
                }
            }
            return totalFood;
        }

        public Team()
        {
            teamColor = Color.white;
            gold = 500;
            lumbers = 150;
            food = 12;
        }
    }

    public const int NeutralPassiveTeamIndex = 0;
    public const int NeutralHostileTeamIndex = 1;

    public const ushort NeutralClanID = 0;
    public const ushort MonstersClanID = 65535;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (teams == null) return;

        for (int i = 0; i < teams.Count; i++)
        {
            if (teams[i] == null) continue;

            if (i == NeutralPassiveTeamIndex)
            {
                teams[i].name = $"Team {i} (Neutral Passive)";
                teams[i].clan = NeutralClanID;
            }
            else if (i == NeutralHostileTeamIndex)
            {
                teams[i].name = $"Team {i} (Neutral Hostile)";
                teams[i].clan = MonstersClanID;
            }
            else
            {
                teams[i].name = $"Team {i - 1}";
                if (teams[i].clan == NeutralClanID || teams[i].clan == MonstersClanID)
                {
                    teams[i].clan = (ushort)i;
                }
            }
        }
    }
#endif

    public static bool UnifiedUnits => Instance != null && Instance.unifiedUnits;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public static Team GetTeam(int team)
    {
        if (Instance == null || Instance.teams == null)
            return null;
        return team < Instance.teams.Count && team >= 0 ? Instance.teams[team] : null;
    }

    public static Color GetTeamColor(int team)
    {
        Team t = GetTeam(team);
        return t != null ? t.teamColor : Color.black;
    }

    public static bool IsAlly(int yourTeam, int otherTeam)
    {
        Team t1 = GetTeam(yourTeam);
        Team t2 = GetTeam(otherTeam);

        if (t1 == null || t2 == null) return false;

        if (t1.clan == NeutralClanID || t2.clan == NeutralClanID)
            return true;

        if (t1.clan == MonstersClanID || t2.clan == MonstersClanID)
            return t1.clan == MonstersClanID && t2.clan == MonstersClanID;

        return t1.clan == t2.clan;
    }

    public static bool CanControl(int yourTeam, int targetTeam)
    {
        if (yourTeam == targetTeam) return true;

        if (UnifiedUnits && IsAlly(yourTeam, targetTeam)) return true;

        return false;
    }

    public static void RegisterUnit(Unit unit, uint customID = 0)
    {
        if (unit == null)
            return;

        Team t = GetTeam(unit.Team);

        if (t == null)
        {
            Debug.LogError("Не удалось зарегистрировать юнит! Возможно отсутствие команды или синглтон менеджера.");
            return;
        }

        uint unitID = (customID != 0) ? customID : UnitNextID++;
        unit.AssignId(unitID);

        AllUnitsById[unitID] = unit;
        if (!t.units.Contains(unit))
        {
            t.units.Add(unit);
        }

        Debug.Log($"Успешно зарегистрирован юнит по идентификатору {unitID}!");
    }

    public static void UnregisterUnit(Unit unit)
    {
        if (unit == null) return;

        Team t = GetTeam(unit.Team);
        if (t != null && t.units.Contains(unit))
        {
            t.units.Remove(unit);
        }
        if (AllUnitsById.ContainsKey(unit.Id))
        {
            AllUnitsById.Remove(unit.Id);
        }
    }

    public static bool TrySpendResources(int buyerTeamIndex, uint goldCost, uint lumberCost)
    {
        Team buyerTeam = GetTeam(buyerTeamIndex);
        if (buyerTeam == null) return false;

        if (buyerTeam.gold >= goldCost && buyerTeam.lumbers >= lumberCost)
        {
            buyerTeam.gold -= goldCost;
            buyerTeam.lumbers -= lumberCost;
            return true;
        }

        return false;
    }

    public static bool AreEnemies(ushort ownerClanID, ushort targetClanID)
    {
        if (ownerClanID == 0 || targetClanID == 0)
            return false;
        return ownerClanID != targetClanID;
    }

    public static bool AreEnemies(int ownerTeam, int targetTeam)
    {
        Team t1 = GetTeam(ownerTeam);
        Team t2 = GetTeam(targetTeam);
        if (t1 == null || t2 == null)
            return false;
        return AreEnemies(t1.clan, t2.clan);
    }
}