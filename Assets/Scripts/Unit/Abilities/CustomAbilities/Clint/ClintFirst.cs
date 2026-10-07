using System.Collections.Generic;
using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "NewClintFirstAbility", menuName = "RTS/Abilities/Clint/First")]
public class ClintFirst : UnitAbility
{
    [Header("Module Settings")]
    [SerializeField] private string shotAnimation = "First";
    [SerializeField] private string shotEffectName = "First";
    [SerializeField] private string shotSoundName = "First";
    [SerializeField] private string hitSoundName = "FirstHit";
    [SerializeField] private int bulletsPerAbility = 5;
    [SerializeField] private LayerMask unitLayer;
    [SerializeField] private bool canCrit = false;
    [SerializeField] private UnitStat baseDamage = 30;
    [SerializeField] private float casterDamageMultiplier = 0.7f;
    [SerializeField] private float damageMultiplierPerShotOneTarget = 0.5f;
    [SerializeField] private float maxDistance = 5f;
    [SerializeField] private float angle = 45f;
    [SerializeField] private float[] shotsTiming = new float[] { 0.1f, 0.05f, 0.1f, 0.1f, 0.1f };
    [SerializeField] private GameObject projectile;
    [SerializeField] private GameObject firstTimeHitEffect;
    [SerializeField] private GameObject hitEffect;
    public override void Init(Unit caster)
    {

    }
    public override void Activate(Unit caster, UnitAbilities.AbilityDefaultRuntimeProperties runtimeProperties = default)
    {
        Collider[] cols = new Collider[20];
        int numFound = Physics.OverlapSphereNonAlloc(caster.transform.position, maxDistance, cols, unitLayer);
        List<Unit> validTargets = new List<Unit>();
        for (int i = 0; i < numFound; i++)
        {
            Collider col = cols[i];
            if (col == null) continue;

            Vector3 dirToTarget = col.transform.position - caster.transform.position;
            dirToTarget.y = 0f;

            if (Vector3.Angle(caster.transform.forward, dirToTarget) <= angle)
            {
                if (col.TryGetComponent(out Unit unit) && unit != caster)
                {
                    if (!validTargets.Contains(unit))
                    {
                        validTargets.Add(unit);
                    }
                }
            }
        }
        if (caster != null && caster.Movement != null)
        {
            caster.Movement.Stop();
        }
        if (caster.Movement != null)
        {
            if (caster.Movement is UnitMovement um)
                um.DelayedStop(.25f);
        }
        caster.Abilities.ReduceMana(manaCost.GetValue(runtimeProperties.level));
        DamageInfo damageInfo = default;
        if (caster.Attack is UnitAttack attack)
        {
            damageInfo = attack._DamageType == DamageType.Physical
                ? attack.GetDamageInfo(canCrit)
                : attack.GetDamageInfo(DamageType.Physical, canCrit);

            damageInfo.hitSound = null;

            damageInfo.damageAmount = baseDamage.GetValue(runtimeProperties.level) + (damageInfo.damageAmount * casterDamageMultiplier) + Dice.GetRandomValue(diceCount, diceSides);
        }
        AnimationVFX(caster);
        VFX(caster);
        SFX(caster);

        if (validTargets.Count > 0)
        {
            caster.StartCoroutine(OrderShot(caster, damageInfo, new Dictionary<Unit, int>(), validTargets, 0, 0));
            HitSFX(caster);
        }

        caster.Abilities.AbilityUsed(abilityName, waitTime);
    }

    void AnimationVFX(Unit caster)
    {
        if (string.IsNullOrEmpty(shotAnimation))
            return;

        if (caster.VFX)
            caster.VFX.PlayAnimation(shotAnimation);
    }

    void VFX(Unit caster)
    {
        if (string.IsNullOrEmpty(shotEffectName))
            return;

        if (caster.VFX)
            caster.VFX.PlayEffect(shotEffectName);
    }

    void SFX(Unit caster)
    {
        if (string.IsNullOrEmpty(shotSoundName))
            return;

        if (caster.SFX)
            caster.SFX.PlaySound(shotSoundName);
    }

    void HitSFX(Unit caster)
    {
        if (string.IsNullOrEmpty(hitSoundName))
            return;
        caster.StartCoroutine(DelayedHitSFX(caster));
    }

    IEnumerator DelayedHitSFX(Unit caster)
    {
        yield return new WaitForSeconds(0.1f);
        if (caster.SFX)
            caster.SFX.PlaySound(hitSoundName);
    }

    IEnumerator OrderShot(Unit caster, DamageInfo damageInfo, Dictionary<Unit, int> damaged, List<Unit> validTargets, int shotCount, int shotUnit)
    {
        if (shotCount >= bulletsPerAbility || validTargets.Count == 0)
        {
            yield break;
        }

        shotUnit = shotUnit % validTargets.Count;
        Unit unit = validTargets[shotUnit];

        if (unit == null)
        {
            validTargets.RemoveAt(shotUnit);
            if (validTargets.Count > 0)
            {
                caster.StartCoroutine(OrderShot(caster, damageInfo, damaged, validTargets, shotCount, shotUnit));
            }
            yield break;
        }

        float multiplier = 1f;
        GameObject hit = hitEffect;
        if (damaged.TryGetValue(unit, out int hits))
        {
            multiplier = Mathf.Pow(damageMultiplierPerShotOneTarget, hits);
            damaged[unit] = hits + 1;
        }
        else
        {
            damaged.Add(unit, 1);
            hit = firstTimeHitEffect;
        }

        DamageInfo rdi = damageInfo;
        rdi.damageAmount = rdi.damageAmount * multiplier;

        GameObject go = caster.Abilities.SpawnObject(projectile, caster.Abilities.ShotPoint.position, caster.Abilities.ShotPoint.rotation);
        go.GetComponent<ClintFirstProjectile>().Initialize(caster, unit, rdi, hit);

        yield return new WaitForSeconds(shotsTiming[shotCount]);
        caster.StartCoroutine(OrderShot(caster, damageInfo, damaged, validTargets, shotCount + 1, shotUnit + 1));
    }

    public override void Aim(Unit caster, Vector3 pos, UnitAbilities.AbilityDefaultRuntimeProperties runtimeProperties = default)
    {
        caster.transform.rotation = Quaternion.LookRotation(pos);
        Activate(caster, runtimeProperties);
    }
}