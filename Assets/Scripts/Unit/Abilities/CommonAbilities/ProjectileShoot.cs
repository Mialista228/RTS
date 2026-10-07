using UnityEngine;

[CreateAssetMenu(fileName = "NewProjectileShootAbility", menuName = "RTS/Abilities/Common/ProjectileShoot")]
public class ProjectileShoot : UnitAbility
{
    [Header("Custom Properties")]
    [SerializeField] private bool stopWhenCast;
    [SerializeField] private string shotAnimation;
    [SerializeField] private string shotVFX;
    [SerializeField] private string shotSFX;
    [SerializeField] private float animationShotDelay;
    [SerializeField] private float animationVFXDelay;
    [SerializeField] private float animationSFXDelay;
    [SerializeField] private bool rotateUnitTowardsAim;
    [SerializeField] private float damageAmount = 40f;
    [SerializeField] private GameObject projectile;

    public override void Init(Unit caster)
    {

    }
    public override void Activate(Unit caster, UnitAbilities.AbilityDefaultRuntimeProperties runtimeProperties = default)
    {

    }
    public void DelayedShoot(Unit caster, Vector3 direction, UnitAbilities.AbilityDefaultRuntimeProperties runtimeProperties = default)
    {
        if (stopWhenCast && caster != null && caster.Movement != null)
        {
            if (caster.Movement is UnitMovement um)
                um.DelayedStop(.25f);
        }
        caster.Abilities.ReduceMana(manaCost.GetValue(runtimeProperties.level));
        caster.Abilities.AbilityUsed(abilityName, waitTime);
        if (caster.Movement is UnitMovement movement) movement.DelayedStop(waitTime);

        if (rotateUnitTowardsAim)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            caster.transform.rotation = targetRotation;
        }

        if (!string.IsNullOrEmpty(shotAnimation))
        {
            if (caster.VFX)
            {
                caster.VFX.PlayAnimation(shotAnimation);
            }
        }

        caster.Abilities.SetDelayedAction(() => { SFX(caster); }, animationSFXDelay);
        caster.Abilities.SetDelayedAction(() => { VFX(caster); }, animationVFXDelay);
        caster.Abilities.SetDelayedAction(() => { Shoot(caster, direction, runtimeProperties); }, animationShotDelay);
    }
    public void SFX(Unit caster)
    {
        if (!string.IsNullOrEmpty(shotSFX))
        {
            if (caster.SFX)
            {
                caster.SFX.PlaySound(shotSFX);
            }
        }
    }
    public void VFX(Unit caster)
    {
        if (!string.IsNullOrEmpty(shotVFX))
        {
            if (caster.VFX)
            {
                caster.VFX.PlayEffect(shotVFX);
            }
        }
    }
    public void Shoot(Unit caster, Vector3 direction, UnitAbilities.AbilityDefaultRuntimeProperties runtimeProperties = default)
    {
        direction.y = 0f;

        Quaternion spawnRotation = caster.Abilities.ShotPoint.rotation;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            if (rotateUnitTowardsAim)
            {
                caster.transform.rotation = targetRotation;
                spawnRotation = caster.Abilities.ShotPoint.rotation;
            }
            else
            {
                spawnRotation = targetRotation;
            }
        }

        GameObject projectileGO = caster.Abilities.SpawnObject(
            projectile,
            caster.Abilities.ShotPoint.position,
            spawnRotation
        );

        if (projectileGO.TryGetComponent<AbilityProjectile>(out var projScript))
        {
            projScript.Initialize(caster);
        }
    }

    public override void Aim(Unit caster, Vector3 pos, UnitAbilities.AbilityDefaultRuntimeProperties runtimeProperties = default)
    {
        DelayedShoot(caster, pos, runtimeProperties);
    }
}