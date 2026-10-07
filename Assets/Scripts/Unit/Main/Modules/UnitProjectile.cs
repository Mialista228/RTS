using UnityEngine;

public class UnitProjectile : EruBehaviour
{
    [SerializeField] private IAttackable attacker;
    [SerializeField] private IDamageReceiver target;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float projectileLifetime = 5f;
    [Min(0.01f)][SerializeField] private float projectileRadius = 0.01f;
    private Transform targetTransform;
    private bool initialized;
    private float dieTime;

    public void Init(IAttackable attacker, IDamageReceiver target, Transform targetTransform)
    {
        this.target = target;
        this.attacker = attacker;
        this.targetTransform = targetTransform;
        initialized = true;
    }

    void Awake()
    {
        dieTime = Time.time + projectileLifetime;
    }

    public override void OnTick(float deltaInterval)
    {
        if (dieTime < Time.time)
            Destroy(gameObject);
        if (initialized == false)
            return;
        if (target == null || attacker == null)
        {
            Destroy(gameObject);
            return;
        }
        transform.position = Vector3.MoveTowards(transform.position, targetTransform.position, moveSpeed * deltaInterval);
        transform.LookAt(targetTransform);
        if (Vector3.Distance(transform.position, targetTransform.position) < projectileRadius)
        {
            attacker.Hit(target);
            Destroy(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, projectileRadius);
    }

}