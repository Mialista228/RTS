using UnityEngine;

public class AbilityProjectile : EruBehaviour
{
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float lifetimeDistance = 5f;
    [SerializeField] private float speed = 1f;
    [SerializeField] private float radius = 1f;
    [SerializeField] private LayerMask unitLayer;

    private Unit caster;
    private bool init;
    private readonly Collider[] hitColliders = new Collider[20];
    private float lifetimeDistanceSquared;
    private Vector3 startPos;
    private UnitAbilities.AbilityDefaultRuntimeProperties runtimeProperties;

    public void Initialize(Unit caster, UnitAbilities.AbilityDefaultRuntimeProperties runtimeProperties = default)
    {
        this.caster = caster;
        startPos = transform.position;
        lifetimeDistanceSquared = lifetimeDistance * lifetimeDistance;
        this.runtimeProperties = runtimeProperties;
        AddCustomUpdate();
        init = true;
    }

    public override void OnTick(float deltaInterval)
    {
        if (init == false)
            return;

        transform.Translate(0f, 0f, speed * deltaInterval, Space.Self);

        if ((transform.position - startPos).sqrMagnitude > lifetimeDistanceSquared)
        {
            Destroy(gameObject);
        }
    }

    public override void OnCustomTick(float deltaInterval)
    {
        if (init == false)
            return;

        int numFound = Physics.OverlapSphereNonAlloc(transform.position, radius, hitColliders, unitLayer);

        if (numFound > 0)
        {
            if (explosionPrefab)
            {
                GameObject go = Instantiate(explosionPrefab, transform.position, transform.rotation);
                go.GetComponent<ProjectileExplosion>().Initialize(caster, runtimeProperties);
            }
            Destroy(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}