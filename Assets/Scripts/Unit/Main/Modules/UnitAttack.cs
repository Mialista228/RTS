using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class UnitAttack : UnitModule, IAttackable
{
    [Header("Module Settings")]
    [SerializeField] private Transform shotPoint;
    private UnitData.UnitSurface unitsCanAttack;
    private float attackRange = 5f;
    private float attackFrame = 0.2f;
    private float attackRecoverFrame = 0.5f;
    private bool canCrit = false;
    private float critChance = 0f;
    private float critDMG = 100f;
    private UnitProjectile projectile;
    private DamageType damageType = DamageType.Physical;
    private UnitAttackStats physicalStats;
    private UnitAttackStats magicalStats;
    private UnitAttackStats pureStats;

    [Header("Events")]
    public UnityEvent<Transform> OnAttack;
    public UnityEvent<Transform> OnAttackBeforeFrame;

    public UnityAction<Transform> onAttack { get; set; }
    public UnityAction<Transform> onAttackBeforeFrame { get; set; }

    public Transform ShotPoint => shotPoint;

    public DamageType _DamageType => damageType;
    public float AttackRange => attackRange;
    public float AttackFrame => attackFrame;
    public float AttackRecoverFrame => attackRecoverFrame;
    public bool CanCrit => canCrit;
    public float CritChance => critChance;
    public float CritDMG => critDMG;
    public IDamageReceiver Target => target;

    public bool IsNextAttackEnhanced { get; set; }
    public bool IsAttackLocked { get; private set; }
    public bool IsAttacking => isAttacking;

    public UnitAttackStats GetUnitAttackStats()
    {
        return GetUnitAttackStats(damageType);
    }

    public UnitAttackStats GetUnitAttackStats(DamageType damageType)
    {

        switch (damageType)
        {
            case DamageType.Physical:
                return physicalStats;
            case DamageType.Magical:
                return magicalStats;
            case DamageType.Pure:
                return pureStats;
        }
        return default;
    }

    public float GetCritCoefficient
    {
        get
        {
            if (critChance > 0f)
                return Random.Range(0f, 100f) < critChance ? critDMG : 0f;
            return 0f;
        }
    }

    public DamageType DamageType => damageType;

    public DamageInfo GetDamageInfo(DamageType damageType, bool crit = false)
    {
        return new DamageInfo(GetUnitAttackStats(damageType).CalculateDamage(), 1f + (crit ? GetCritCoefficient / 100f : 0f), GetUnitAttackStats(damageType).penetration, GetUnitAttackStats(damageType).percentPenetration, damageType);
    }
    public DamageInfo GetDamageInfo(bool crit = false)
    {
        return GetDamageInfo(damageType, crit);
    }
    public DamageInfo GetDamageInfo()
    {
        return GetDamageInfo(false);
    }

    private IDamageReceiver target;
    private Transform targetTransform;
    private float cooldownTimer;
    private float frameTimer;
    private bool isAttacking;
    private bool hasAttackedInCurrentCycle;

    public override void OnAwake(Unit unit)
    {
        base.OnAwake(unit);

        if (unit.UnitData)
        {
            unitsCanAttack = unit.UnitsCanAttack;
            attackRange = unit.AttackRange;
            attackFrame = unit.AttackFrame;
            attackRecoverFrame = unit.AttackRecoverFrame;
            canCrit = unit.CanCrit;
            critChance = unit.CritChance;
            critDMG = unit.CritDMG;
            projectile = unit.Projectile;
            damageType = unit.UnitData.damageType;
            physicalStats = unit.GetUnitAttackStats(DamageType.Physical);
            magicalStats = unit.GetUnitAttackStats(DamageType.Magical);
            pureStats = unit.GetUnitAttackStats(DamageType.Pure);
        }
    }

    public override void OnStart()
    {

    }

    [ContextMenu("Auto Create Shot Point")]
    private void AutoCreateShotPoint()
    {
        if (shotPoint == null)
        {
            shotPoint = new GameObject("ShotPoint").transform;
            shotPoint.SetParent(transform);
            shotPoint.localPosition = transform.forward;
        }
    }

    public override void OnTick(float deltaInterval)
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= deltaInterval;
        }

        if (target == null)
        {
            ResetAttackCycle();
            return;
        }

        targetTransform = (target as Component)?.transform;
        if (targetTransform == null)
        {
            target = null;
            ResetAttackCycle();
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);

        if (distanceToTarget > (IsNextAttackEnhanced ? enhancedRange : attackRange))
        {
            if (isAttacking && !hasAttackedInCurrentCycle)
            {
                ResetAttackCycle();
            }

            if (unit != null && unit.Movement != null)
            {
                unit.Movement.MoveTo(targetTransform.position);
            }
            return;
        }

        if (unit != null && unit.Movement != null)
        {
            unit.Movement.Stop();
        }

        RotateTowardsTarget();

        if (!isAttacking)
        {
            if (cooldownTimer <= 0f)
            {
                if (IsAttackLocked)
                    return;
                isAttacking = true;
                hasAttackedInCurrentCycle = false;
                frameTimer = attackFrame;
                onAttackBeforeFrame?.Invoke(targetTransform);
                OnAttackBeforeFrame?.Invoke(targetTransform);
            }
        }
        else
        {
            if (IsAttackLocked)
            {
                isAttacking = false;
                frameTimer = -attackRecoverFrame;
                hasAttackedInCurrentCycle = true;
            }
            frameTimer -= deltaInterval;

            if (frameTimer <= 0f && !hasAttackedInCurrentCycle)
            {
                hasAttackedInCurrentCycle = true;
                Attack(target);
            }

            if (frameTimer <= -attackRecoverFrame)
            {
                isAttacking = false;
                cooldownTimer = 0f;
            }
        }
    }

    public void SetTarget(IDamageReceiver target)
    {
        if (this.target != target)
        {
            this.target = target;
            targetTransform = (target as Component)?.transform;

            if (!isAttacking)
            {
                ResetAttackCycle();
            }
        }
    }

    public void LockAttack(float delay)
    {
        StartCoroutine(LockAttackDelayed(delay));
    }

    IEnumerator LockAttackDelayed(float delay)
    {
        IsAttackLocked = true;
        yield return new WaitForSeconds(delay);
        IsAttackLocked = false;
    }

    float enhancedRange = 0f;

    public void SetEnhancedAttack(float attackRange = 0f)
    {
        IsNextAttackEnhanced = true;
        if (attackRange > 0f)
        {
            enhancedRange = attackRange;
        }
        else
        {
            enhancedRange = this.attackRange;
        }
    }

    public void Attack(IDamageReceiver target)
    {
        if (IsAttackLocked)
        {
            isAttacking = false;
            cooldownTimer = 0f;
            frameTimer = -attackRecoverFrame;
            return;
        }

        if (target == null)
            return;

        if (targetTransform == null)
            return;

        if (IsNextAttackEnhanced == false)
        {
            if (projectile)
            {
                Vector3 spawnPosition = shotPoint != null ? shotPoint.position : transform.position;
                Quaternion spawnRotation = transform.rotation;
                GameObject projectileGO = Instantiate(projectile.gameObject, spawnPosition, spawnRotation);

                if (projectileGO.TryGetComponent<UnitProjectile>(out var unitProj))
                {
                    unitProj.Init(this, target, targetTransform);
                }
            }
            else
            {
                Hit(target);
            }
        }

        onAttack?.Invoke(targetTransform);
        OnAttack?.Invoke(targetTransform);
    }

    public void Hit(IDamageReceiver target)
    {
        target.TakeDamage(GetDamageInfo(canCrit), this);
    }

    private void ResetAttackCycle()
    {
        isAttacking = false;
        hasAttackedInCurrentCycle = false;
        frameTimer = 0f;
    }

    private void RotateTowardsTarget()
    {
        if (targetTransform == null) return;

        Vector3 direction = (targetTransform.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}