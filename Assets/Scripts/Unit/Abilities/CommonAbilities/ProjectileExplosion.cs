using UnityEngine;

public abstract class ProjectileExplosion : EruBehaviour
{
    protected Unit caster;
    [SerializeField] protected float radius;
    [SerializeField] protected LayerMask unitLayer;
    protected readonly Collider[] cols = new Collider[20];
    public void Initialize(Unit caster)
    {
        this.caster = caster;
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