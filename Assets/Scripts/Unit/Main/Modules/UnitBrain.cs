using System.Collections.Generic;
using UnityEngine;

public class UnitBrain : UnitModule, ICommandExecutor
{
    [Header("Module Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private bool showGizmos;
    [SerializeField] private UnitState currentState;
    public enum UnitState
    {
        Idle,
        Move,
        AttackMove,
        Attack,
        Patrol,
        Follow,
        HoldPosition,
        Stunned,
        CastAbility
    }
    public Transform Target => target;

    public bool ProcessingCommand { get; private set; }

    public UnitState CurrentState => currentState;
    private readonly Queue<ICommand> commandQueue = new Queue<ICommand>();
    private ICommand currentCommand;

    private IMoveable movement => unit.Movement;

    public void Enqueue(ICommand command)
    {
        if (command == null) return;

        commandQueue.Enqueue(command);

        if (currentCommand == null)
        {
            ExecuteNext();
        }
    }

    public void ExecuteNext()
    {
        if (commandQueue.Count > 0)
        {
            currentCommand = commandQueue.Dequeue();
            StartCommand(currentCommand);
        }
        else
        {
            currentCommand = null;
            currentState = UnitState.Idle;
            CommandStop();
        }
    }

    public void Clear()
    {
        commandQueue.Clear();
        if (currentCommand != null)
            currentCommand.Cancel(unit);
        currentState = UnitState.Idle;
        currentCommand = null;
        CommandStop();
    }

    void CommandStop()
    {

    }

    public bool RemoveAt(int index)
    {
        return false;
    }

    private void StartCommand(ICommand command)
    {
        switch (command)
        {
            case MoveCommand moveCmd:
                if (movement != null)
                {
                    currentState = UnitState.Move;
                    moveCmd.Start(unit);
                }
                else
                {
                    moveCmd.Cancel(unit);
                }
                break;

            case FollowCommand followCmd:
                if (movement != null)
                {
                    currentState = UnitState.Follow;
                    followCmd.Start(unit);
                }
                else
                {
                    followCmd.Cancel(unit);
                }
                break;

            case AttackMoveCommand attackMoveCmd:
                currentState = UnitState.AttackMove;
                attackMoveCmd.Start(unit);
                break;

            case AttackCommand attackCmd:
                currentState = UnitState.Attack;
                attackCmd.Start(unit);
                break;

            case CastAbilityCommand castCmd:
                currentState = UnitState.CastAbility;
                castCmd.Start(unit);
                break;

            case HoldPositionCommand holdCmd:
                currentState = UnitState.HoldPosition;
                holdCmd.Start(unit);
                break;

            case PatrolCommand patrolCmd:
                currentState = UnitState.Patrol;
                patrolCmd.Start(unit);
                break;

            case StopCommand stopCmd:
                currentState = UnitState.Idle;
                stopCmd.Start(unit);
                break;
        }
    }

    public override void OnAwake(Unit unit)
    {
        base.OnAwake(unit);
    }

    public override void OnStart()
    {

    }

    public override void OnTick(float deltaInterval)
    {
        ProcessingCommand = currentCommand != null;
        if (currentCommand == null) return;

        if (IsCurrentCommandFinished(deltaInterval))
        {
            ExecuteNext();
        }
    }

    private bool IsCurrentCommandFinished(float deltaInterval)
    {
        if (currentCommand == null) return true;

        currentCommand.OnTick(unit, deltaInterval);

        return currentCommand.IsFinished;
    }

    void OnDrawGizmosSelected()
    {
        if (!showGizmos)
            return;

        foreach (ICommand command in commandQueue)
        {
            if (command is MoveCommand mc)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(mc.GetDestination, 0.25f);
            }
        }
    }
}
