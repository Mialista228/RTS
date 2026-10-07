using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnitVFX : UnitModule
{
    [System.Serializable]
    public class UnitVFXCustomEffects
    {
        public string effectName;
        public UnityEvent effect;
    }
    [Header("Module Settings")]
    [SerializeField] private Animator targetAnimator;
    [SerializeField] private float dampTime = 0.1f;
    [SerializeField] private int attackAnimations = 2;
    [SerializeField] private ParticleSystem[] basicAttackParticles;
    [SerializeField] private UnitVFXCustomEffects[] effects;
    private Dictionary<string, UnityEvent> effectDictionary;
    private static readonly int MoveHash = Animator.StringToHash("Move");
    private static readonly int AttackHash = Animator.StringToHash("BasicAttack");
    private static readonly int AttackIndexHash = Animator.StringToHash("BasicAttackIndex");
    private static readonly int UnitSpeedHash = Animator.StringToHash("UnitSpeed");
    private int attackIndex;
    public override void OnAwake(Unit unit)
    {
        base.OnAwake(unit);

        if (targetAnimator == null)
            targetAnimator = GetComponentInChildren<Animator>();

        effectDictionary = new Dictionary<string, UnityEvent>();

        foreach (var vfx in effects)
        {
            if (string.IsNullOrEmpty(vfx.effectName)) continue;

            if (!effectDictionary.ContainsKey(vfx.effectName))
            {
                effectDictionary.Add(vfx.effectName, vfx.effect);
            }
            else
            {
                Debug.LogError($"Эффект под названием {vfx.effectName} уже присутствует");
            }
        }
    }
    float moveValue = 0f;
    public override void OnTick(float deltaInterval)
    {
        if (targetAnimator == null)
            return;

        if (unit.Movement != null)
        {
            float currentSpeed = unit.Movement.CurrentVelocity.magnitude;
            moveValue = Mathf.InverseLerp(0f, unit.Movement.MoveSpeed, currentSpeed);

            targetAnimator.SetFloat(UnitSpeedHash, moveValue);
            targetAnimator.SetFloat(MoveHash, moveValue, dampTime, deltaInterval);
        }
    }
    public void PlayEffect(string effectName)
    {
        if (effectDictionary.TryGetValue(effectName, out UnityEvent effect))
        {
            effect?.Invoke();
        }
        else
        {
            Debug.LogError("Давай по новой миша всё хуйня");
        }
    }
    public void PlayAnimation(string animation)
    {
        targetAnimator.Play(animation, 0, 0);
    }
    public void BasicAttackAnimation()
    {
        targetAnimator.SetFloat(AttackIndexHash, attackIndex);
        targetAnimator.Play(AttackHash, 0, 0f);
        attackIndex++;
        if (attackIndex > attackAnimations - 1)
            attackIndex = 0;
    }
    public void BasicAttackEffect()
    {
        foreach (ParticleSystem particle in basicAttackParticles)
        {
            particle.Play();
        }
    }
    public GameObject SpawnEffect(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        return Instantiate(prefab, pos, rot);
    }
    public void SetAttackIndex(int value)
    {
        attackIndex = value;
    }
}