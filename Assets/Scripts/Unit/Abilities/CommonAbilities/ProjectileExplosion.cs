using UnityEngine;

public abstract class ProjectileExplosion : EruBehaviour
{
    protected Unit caster;
    protected UnitAbilities.AbilityDefaultRuntimeProperties runtimeProperties;
    [SerializeField] protected float radius;
    [SerializeField] protected LayerMask unitLayer;
    protected readonly Collider[] cols = new Collider[20];
    public void Initialize(Unit caster, UnitAbilities.AbilityDefaultRuntimeProperties runtimeProperties = default)
    {
        this.caster = caster;
        this.runtimeProperties = runtimeProperties;
        Explosion();
    }
    public abstract void Explosion();
    protected int GetUnitsInRadius()
    {
        int n = Physics.OverlapSphereNonAlloc(transform.position, radius, cols, unitLayer);
        return n;
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}