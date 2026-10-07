using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Player : EruBehaviour
{
    [SerializeField][Range(0f, 100f)] private float gapToMove = 1f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Camera inputCamera;
    [SerializeField] private bool lockMovingCameraWhenMouseOffscreen;
    [Min(2)][SerializeField] private int controllingTeam = 2;
    [SerializeField] private LayerMask hoverableLayers;
    [SerializeField] private LayerMask groundLayer;

    public static Player Instance { get; private set; }

    public UnityAction<IUnit> OnUnitSelected;

    public static int ControllingTeam => Instance.controllingTeam;

    public IUnit MouseEnteredSelectable { get; private set; }

    public readonly List<IUnit> selected = new List<IUnit>();

    public Vector2 CurrentMousePosition { get; private set; }

    int currentAimingAction = -1;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void OnTick(float deltaInterval)
    {
        CurrentMousePosition = Input.mousePosition;
        Ray ray = inputCamera.ScreenPointToRay(CurrentMousePosition);

        UpdateHoveredSelectable(ray);

        if (currentAimingAction != -1)
        {
            if (Input.GetMouseButtonDown(0))
            {
                switch (currentAimingAction)
                {
                    case 0:
                        if (Physics.Raycast(ray, out RaycastHit hit, 500F, groundLayer))
                        {
                            ProcessMoveCommand(false, hit.point);
                        }
                        break;
                    case 3:
                        if (Physics.Raycast(ray, out hit, 500F, hoverableLayers))
                        {
                            Unit targetUnit = hit.collider.GetComponentInParent<Unit>();
                            if (targetUnit != null)
                                ProcessAttackCommand(targetUnit, false);
                        }
                        break;
                    case 4:
                        if (Physics.Raycast(ray, out hit, 500F, groundLayer))
                        {
                            ProcessPatrolCommand(false, hit.point);
                        }
                        break;
                    default:
                        break;
                }
                currentAimingAction = -1;
                HUD.Instance.StopAim();
            }
            return;
        }

        UpdateCameraMovement(deltaInterval, CurrentMousePosition);

        ProcessSelection();

        ProcessCommands(ray);
    }

    public static bool IsPlayerUnit(Unit target)
    {
        return target.Team == Instance.controllingTeam;
    }

    void UpdateCameraMovement(float deltaInterval, Vector2 mousePos)
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        Vector3 moveDirection = Vector3.zero;

        bool isMouseInsideWindow = mousePos.x >= 0 && mousePos.x <= screenWidth &&
                                   mousePos.y >= 0 && mousePos.y <= screenHeight;


        if (!isMouseInsideWindow && lockMovingCameraWhenMouseOffscreen) return;

        float borderWidth = screenWidth * gapToMove / 100f;
        float borderHeight = screenHeight * gapToMove / 100f;

        if (mousePos.y <= borderHeight)
        {
            moveDirection.y = -1f;
        }
        else if (mousePos.y >= screenHeight - borderHeight)
        {
            moveDirection.y = 1f;
        }

        if (mousePos.x <= borderWidth)
        {
            moveDirection.x = -1f;
        }
        else if (mousePos.x >= screenWidth - borderWidth)
        {
            moveDirection.x = 1f;
        }

        if (moveDirection.sqrMagnitude > 0f)
        {
            moveDirection.Normalize();

            Vector3 forward = inputCamera.transform.forward;
            Vector3 right = inputCamera.transform.right;

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 desiredMove = (forward * moveDirection.y + right * moveDirection.x) * moveSpeed * deltaInterval;
            inputCamera.transform.position += desiredMove;
        }
    }

    void UpdateHoveredSelectable(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit, 500F, hoverableLayers))
        {
            if (hit.collider.TryGetComponent<IUnit>(out var selectable))
            {
                SetHoveredSelectable(selectable);
                return;
            }
        }

        SetHoveredSelectable(null);
    }

    private void ProcessSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SelectSingleUnit(MouseEnteredSelectable);
        }
    }

    public void SelectSingleUnit(IUnit selectable)
    {
        if (selectable == null)
            return;

        bool isShiftPressed = Input.GetKey(KeyCode.LeftShift);

        if (!isShiftPressed)
        {
            DeselectAll();
        }

        if (selected.Contains(selectable))
        {
            if (isShiftPressed)
            {
                selectable.Deselect();
                selected.Remove(selectable);
            }
        }
        else
        {
            selectable.Select();
            selected.Add(selectable);
            OnUnitSelected?.Invoke(selectable);
        }
    }

    public void SelectUnits(List<IUnit> selectables, bool clearPrevious = true)
    {
        if (clearPrevious)
        {
            DeselectAll();
        }

        if (selectables == null) return;

        for (int i = 0; i < selectables.Count; i++)
        {
            IUnit unit = selectables[i];
            if (unit != null && !selected.Contains(unit))
            {
                unit.Select();
                selected.Add(unit);
                OnUnitSelected?.Invoke(unit);
            }
        }
    }

    public void DeselectAll()
    {
        for (int i = 0; i < selected.Count; i++)
        {
            selected[i]?.Deselect();
        }
        OnUnitSelected?.Invoke(null);
        selected.Clear();
    }

    public void AimDefaultActions(int actionIndex)
    {
        currentAimingAction = actionIndex;
        HUD.Instance.StartAim(() => { HUD.Instance.StopAim(); currentAimingAction = -1; });

        if (actionIndex == 1)
        {
            HUD.Instance.StopAim();
            currentAimingAction = -1;
            ProcessStopCommand();
        }
        if (actionIndex == 2)
        {
            HUD.Instance.StopAim();
            currentAimingAction = -1;
        }
    }

    public void ProcessStopCommand()
    {
        for (int i = 0; i < selected.Count; i++)
        {
            if (!(selected[i] is ICommandExecutor commandExecutor))
                continue;

            commandExecutor.Clear();
        }
    }

    public void ProcessPatrolCommand(bool queueCommands, Vector3 hitPoint)
    {
        for (int i = 0; i < selected.Count; i++)
        {
            if (!(selected[i] is ICommandExecutor commandExecutor))
                continue;

            if (queueCommands == false)
                commandExecutor.Clear();
            commandExecutor.Enqueue(new PatrolCommand(hitPoint));
        }
    }

    public void ProcessMoveCommand(bool queueCommands, Vector3 hitPoint)
    {
        for (int i = 0; i < selected.Count; i++)
        {
            if (!(selected[i] is ICommandExecutor commandExecutor))
                continue;

            if (queueCommands == false)
                commandExecutor.Clear();
            commandExecutor.Enqueue(new MoveCommand(hitPoint));
        }
    }

    public void ProcessAttackCommand(Unit targetUnit, bool queueCommands)
    {
        if (targetUnit == null)
            return;

        for (int i = 0; i < selected.Count; i++)
        {
            if (!(selected[i] is ICommandExecutor commandExecutor))
                continue;

            if (commandExecutor is Unit unit && unit == targetUnit)
                continue;

            if (queueCommands == false)
                commandExecutor.Clear();

            commandExecutor.Enqueue(new AttackCommand(targetUnit));
        }
    }

    public void ProcessFollowCommand(Transform targetTransform, bool queueCommands)
    {
        for (int i = 0; i < selected.Count; i++)
        {
            if (!(selected[i] is ICommandExecutor commandExecutor))
                continue;

            if (queueCommands == false)
                commandExecutor.Clear();
            commandExecutor.Enqueue(new FollowCommand(targetTransform));
        }
    }

    private void ProcessCommands(Ray ray)
    {
        if (selected.Count == 0)
            return;

        if (selected[0].Team != controllingTeam)
            return;

        if (Input.GetMouseButtonDown(1))
        {
            bool queueCommands = Input.GetKey(KeyCode.LeftShift);
            if (Physics.Raycast(ray, out RaycastHit hit, 500F, hoverableLayers))
            {
                Unit targetUnit = hit.collider.GetComponentInParent<Unit>();

                for (int i = 0; i < selected.Count; i++)
                {
                    if (!(selected[i] is ICommandExecutor commandExecutor)) continue;

                    if (!queueCommands) commandExecutor.Clear();

                    if (targetUnit != null)
                    {
                        Unit ownerUnit = selected[i] as Unit;

                        if (ownerUnit != null && GameplayManager.AreEnemies(ownerUnit.Team, targetUnit.Team))
                        {
                            commandExecutor.Enqueue(new AttackCommand(targetUnit));
                        }
                        else
                        {
                            commandExecutor.Enqueue(new FollowCommand(targetUnit.transform));
                        }
                    }
                    else
                    {
                        commandExecutor.Enqueue(new FollowCommand(hit.collider.transform));
                    }
                }
                return;
            }
            if (Physics.Raycast(ray, out hit, 500F, groundLayer))
            {
                ProcessMoveCommand(queueCommands, hit.point);
                return;
            }
        }
    }

    private void SetHoveredSelectable(IUnit newSelectable)
    {
        if (MouseEnteredSelectable == newSelectable)
            return;

        MouseEnteredSelectable?.OnHoverExit();
        MouseEnteredSelectable = newSelectable;
        MouseEnteredSelectable?.OnHoverEnter();
    }
}