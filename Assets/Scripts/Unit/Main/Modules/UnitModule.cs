using UnityEngine;

public abstract class UnitModule : MonoBehaviour
{
    [SerializeField] private bool isEnabled = true;
    public bool IsEnabled => isEnabled;
    public void SetEnabled(bool value) => isEnabled = value;
    public Unit _Unit => unit;

    protected Unit unit;
    public virtual void OnAwake(Unit unit)
    {
        this.unit = unit;
    }
    public virtual void OnStart()
    {

    }
    public virtual void OnTick(float deltaInterval)
    {

    }
}