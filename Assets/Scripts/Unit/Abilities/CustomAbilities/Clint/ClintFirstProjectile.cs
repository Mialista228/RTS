using UnityEngine;

public class ClintFirstProjectile : EruBehaviour
{
    [SerializeField] private float radius = 0.5f;
    [SerializeField] private float speed = 25f;
    private GameObject hitEffect;
    private DamageInfo damageInfo;
    private Unit caster;
    private Unit target;
    private bool init;

    public void Initialize(Unit caster, Unit target, DamageInfo damageInfo, GameObject hitEffect)
    {
        this.caster = caster;
        this.target = target;
        this.damageInfo = damageInfo;
        this.hitEffect = hitEffect;
        init = true;
    }


    public override void OnTick(float deltaInterval)
    {
        if (init == false)
            return;

        if (caster == null || target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * deltaInterval);
        transform.LookAt(target.transform);

        if (Vector3.Distance(transform.position, target.transform.position) < radius)
        {
            target.DamageReceiver.TakeDamage(damageInfo, caster.Attack);
            Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
