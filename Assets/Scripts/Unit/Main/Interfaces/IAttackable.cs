using UnityEngine;
using UnityEngine.Events;

public interface IAttackable
{
    IDamageReceiver Target { get; }
    DamageInfo GetDamageInfo();
    UnityAction<Transform> onAttack { get; set; }
    UnityAction<Transform> onAttackBeforeFrame { get; set; }
    void Attack(IDamageReceiver target);
    void Hit(IDamageReceiver target);
    void SetTarget(IDamageReceiver target);
}