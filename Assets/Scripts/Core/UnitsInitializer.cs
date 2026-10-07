using UnityEngine;

public class UnitsInitializer : EruBehaviour
{
    [SerializeField] private Unit[] units;

    void Awake()
    {
        if (units == null)
            return;

        for (int i = 0; i < units.Length; i++)
        {
            if (units[i] != null)
                units[i].Initialize();
        }
    }
}