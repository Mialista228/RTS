using UnityEngine;

public class MoveCommand : ICommand
{
    private Vector3 destination;
    public Vector3 GetDestination => destination;
    public MoveCommand(Vector3 destination)
    {
        this.destination = destination;
    }
    public void Start(Unit unit)
    {
        unit.Movement.MoveTo(destination);
    }

    public bool IsFinished { get; private set; }
    public void Cancel(Unit unit)
    {
        //unit.Movement.Stop();
        IsFinished = true;
    }

    public void OnTick(Unit unit, float deltaInterval)
    {
        if (unit.Movement != null && unit.Movement.IsReachedDestination)
        {
            IsFinished = true;
        }
    }
}
public class AttackMoveCommand : ICommand
{
    public bool IsFinished { get; private set; }

    public AttackMoveCommand()
    {

    }

    public void Cancel(Unit unit)
    {

    }

    public void OnTick(Unit unit, float deltaInterval)
    {

    }

    public void Start(Unit unit)
    {

    }
}
public class AttackCommand : ICommand
{
    public bool IsFinished { get; private set; }

    public Unit Target;

    public AttackCommand(Unit targetUnit)
    {
        Target = targetUnit;
    }

    public void Start(Unit unit)
    {
        if (unit == null || Target == null)
        {
            IsFinished = true;
            return;
        }

        if (unit.Attack == null)
        {
            IsFinished = true;
            return;
        }

        if (Target.DamageReceiver.isAlive == false)
        {
            IsFinished = true;
            return;
        }

        if (!unit.UnitsCanAttack.HasFlag(Target.MovementSurface))
        {
            IsFinished = true;
            return;
        }

        if (Target.DamageReceiver != null)
        {
            unit.Attack.SetTarget(Target.DamageReceiver);
        }
        else
        {
            IsFinished = true;
        }
    }

    public void OnTick(Unit unit, float deltaInterval)
    {
        if (Target == null)
        {
            Finish(unit);
        }
        if (Target.DamageReceiver.isAlive == false)
        {
            Finish(unit);
            return;
        }
    }

    void Finish(Unit unit)
    {
        if (unit != null && unit.Attack != null)
        {
            unit.Attack.SetTarget(null);
        }
        IsFinished = true;
    }

    public void Cancel(Unit unit)
    {
        if (unit != null && unit.Attack != null)
        {
            unit.Attack.SetTarget(null);
        }

        IsFinished = true;
    }
}
public class CastAbilityCommand : ICommand
{
    public bool IsFinished { get; private set; }

    private string abilityName;
    private IDamageReceiver target;
    private bool attackAfterAbility;

    public CastAbilityCommand(string abilityName, IDamageReceiver target = null, bool attackAfterAbility = false)
    {
        this.abilityName = abilityName;
        this.target = target;
        this.attackAfterAbility = attackAfterAbility;
    }

    public void Cancel(Unit unit)
    {

    }

    public void OnTick(Unit unit, float deltaInterval)
    {

    }

    public void Start(Unit unit)
    {
        unit.Abilities.CastAbility(abilityName);
        if (attackAfterAbility && target != null)
            unit.Enqueue(new AttackCommand((target as UnitHealth)._Unit));
        IsFinished = true;
    }
}
public class HoldPositionCommand : ICommand
{
    public bool IsFinished { get; private set; }

    public void Cancel(Unit unit)
    {

    }

    public void OnTick(Unit unit, float deltaInterval)
    {

    }

    public void Start(Unit unit)
    {

    }
}
public class StopCommand : ICommand
{
    public bool IsFinished => true;

    public void Cancel(Unit unit)
    {

    }

    public void OnTick(Unit unit, float deltaInterval)
    {

    }

    public void Start(Unit unit)
    {
        unit.Clear();
        if (unit.Movement != null)
            unit.Movement.Stop();
    }
}
public class PatrolCommand : ICommand
{
    private Vector3 startPoint;
    private Vector3 endPoint;
    private Vector3 currentDestination;

    public bool IsFinished { get; private set; }

    public PatrolCommand(Vector3 destination)
    {
        this.endPoint = destination;
        this.IsFinished = false;
    }

    public void Start(Unit unit)
    {
        if (unit.Movement == null)
        {
            IsFinished = true;
            return;
        }

        startPoint = unit.transform.position;
        currentDestination = endPoint;

        unit.Movement.MoveTo(currentDestination);
    }

    public void OnTick(Unit unit, float deltaInterval)
    {
        if (IsFinished || unit.Movement == null)
            return;

        /*
        if (unit.Attack != null && unit.Attack.TryAcquireTargetInRadius())
        {
            return;
        }
        */

        if (unit.Movement.IsReachedDestination)
        {
            currentDestination = (currentDestination == endPoint) ? startPoint : endPoint;

            unit.Movement.MoveTo(currentDestination);
        }
    }

    public void Cancel(Unit unit)
    {
        if (unit.Movement != null)
        {
            unit.Movement.Stop();
        }

        IsFinished = true;
    }
}
public class FollowCommand : ICommand
{
    public Transform TargetTransform { get; private set; }
    public float StoppingDistance { get; }
    public bool IsFinished => TargetTransform == null;

    private Vector3 lastTargetPosition;
    private float updateTimer;
    private const float UPDATE_INTERVAL = 0.2f;

    public FollowCommand(Transform target, float stoppingDistance = 2f)
    {
        TargetTransform = target;
        StoppingDistance = stoppingDistance;
    }

    public void Start(Unit unit)
    {
        if (unit.Movement != null && TargetTransform != null)
        {
            unit.Movement.SetStoppingDistance(StoppingDistance);
            lastTargetPosition = TargetTransform.position;
            unit.Movement.MoveTo(lastTargetPosition);
        }
    }

    public void Cancel(Unit unit)
    {
        if (unit.Movement != null)
        {
            unit.Movement.ResetStoppingDistance();
            unit.Movement.Stop();
        }
        TargetTransform = null;
    }

    public void OnTick(Unit unit, float deltaInterval)
    {
        if (TargetTransform == null || unit.Movement == null) return;

        updateTimer += Time.deltaTime;

        if (updateTimer >= UPDATE_INTERVAL)
        {
            updateTimer = 0f;

            if (Vector3.SqrMagnitude(TargetTransform.position - lastTargetPosition) > 0.25f)
            {
                lastTargetPosition = TargetTransform.position;
                unit.Movement.MoveTo(lastTargetPosition);
            }
        }
    }
}