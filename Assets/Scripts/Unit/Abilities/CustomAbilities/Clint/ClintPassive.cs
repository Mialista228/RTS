using UnityEngine;

[CreateAssetMenu(fileName = "ClintPassive", menuName = "RTS/Abilities/Clint/Passive")]
public class ClintPassive : UnitAbility
{
    [Header("Custom Properties")]
    [SerializeField] private string shotEffectName = "Passive";
    [SerializeField] private string shotSoundName = "Passive";
    [SerializeField] private string showEnhancedAttackEffectName = "PassiveActive";
    [SerializeField] private string hideEnhancedAttackEffectName = "PassiveDeactive";
    [SerializeField] private string hitSoundName = "PassiveHit";
    [SerializeField] private float baseDamage = 40f;
    [SerializeField] private float unitDamageMultiplier = 0.5f;
    [SerializeField] private LayerMask unitLayer;
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private float lineLength = 5f;

    public override void Init(Unit caster)
    {
        caster.Abilities.onAbilityCasted += (value1, value2) => PreparePassive(caster);
    }
    public override void Activate(Unit caster)
    {

    }
    public void PreparePassive(Unit caster)
    {
        if (caster.Abilities == null) return;

        if (caster.Attack is UnitAttack attack)
        {
            if (!attack.IsAttacking || attack.IsAttackLocked)
            {
                ApplyPassiveBuff(caster, attack);
            }
            else
            {
                caster.Abilities.SetAbilityAttackAction(this, (target) =>
                {
                    caster.Abilities.ClearAbilityAttackActions(this);

                    ApplyPassiveBuff(caster, attack);
                });
            }
        }
    }

    private void ApplyPassiveBuff(Unit caster, UnitAttack attack)
    {
        attack.SetEnhancedAttack(lineLength);

        caster.VFX.PlayEffect(showEnhancedAttackEffectName);

        if (caster.VFX != null)
        {
            caster.VFX.SetAttackIndex(2);
        }

        caster.Abilities.SetAbilityBeforeFrameAction(this, (target) =>
        {
            ShootSFX(caster);
        });

        caster.Abilities.SetAbilityAttackAction(this, (target) =>
        {
            Shoot(caster, target);
            if (caster.Movement is UnitMovement movement)
            {
                movement.DelayedStop(waitTime);
            }
            ShootVFX(caster);

            caster.Abilities.ClearAbilityAttackActions(this);
            attack.IsNextAttackEnhanced = false;

            if (caster.VFX != null)
            {
                caster.VFX.SetAttackIndex(0);
            }
        });
    }
    public void ShootSFX(Unit caster)
    {
        if (caster.SFX != null)
        {
            if (!string.IsNullOrEmpty(shotSoundName))
                caster.SFX.PlaySound(shotSoundName);
        }
    }
    public void HitSFX(Unit caster)
    {
        if (caster.SFX != null)
        {
            if (!string.IsNullOrEmpty(hitSoundName))
                caster.SFX.PlaySound(hitSoundName);
        }
    }
    public void ShootVFX(Unit caster)
    {
        if (caster.VFX != null)
        {
            if (!string.IsNullOrEmpty(shotEffectName))
                caster.VFX.PlayEffect(shotEffectName);
        }
    }
    public void Shoot(Unit caster, Transform target)
    {
        if (caster.Attack is UnitAttack attack)
        {
            Vector3 direction = (target.position - attack.ShotPoint.position).normalized;

            Ray ray = new Ray(attack.ShotPoint.position, direction);
            RaycastHit[] hitInfo = Physics.RaycastAll(ray, lineLength, unitLayer);
            DamageInfo casterDamageInfo = attack.GetDamageInfo(true);
            float calculatedDamage = baseDamage + Dice.GetRandomValue(diceCount, diceSides) + casterDamageInfo.damageAmount * unitDamageMultiplier;
            caster.VFX.PlayEffect(hideEnhancedAttackEffectName);
            DamageInfo damageInfo = new DamageInfo(calculatedDamage, casterDamageInfo.critCoefficient, casterDamageInfo.penetration, casterDamageInfo.percentPenetration, DamageType.Physical);
            for (int i = 0; i < hitInfo.Length; i++)
            {
                if (hitInfo[i].collider.gameObject == caster.gameObject) continue;

                if (hitInfo[i].collider.TryGetComponent(out Unit unit))
                {
                    unit.DamageReceiver.TakeDamage(damageInfo, caster.Attack);
                    caster.VFX.SpawnEffect(hitEffect, unit.transform.position, Quaternion.identity);
                }
            }
            attack.IsNextAttackEnhanced = false;
        }
    }

    public override void Aim(Unit caster, Vector3 pos)
    {

    }
}