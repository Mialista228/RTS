using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitMovement : UnitModule, IMoveable
{
    [Header("Module Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 15f;
    [SerializeField] private float turnSpeed = 720f;
    [SerializeField] private float stoppingDistance = 0.25f;

    [Header("Path Smoothing")]
    [SerializeField] private float waypointThreshold = 0.8f;
    [SerializeField] private float lookAheadDistance = 1.5f;

    [Header("Avoidance")]
    [SerializeField] private float personalRadius = 0.5f;
    [SerializeField] private LayerMask unitLayer;

    public float MoveSpeed => moveSpeed;
    public float StoppingDistance => stoppingDistance;
    public Vector3 CurrentVelocity => currentVelocity;
    public bool IsReachedDestination => !isMoving;

    private readonly List<Vector3> currentWaypoints = new List<Vector3>();
    private int currentWaypointIndex = 0;
    private Vector3 currentVelocity;
    private bool isMoving = false;
    private bool editedStoppingDistance = false;
    private float customStoppingDistance = 0f;
    private float CurrentStoppingDistance => editedStoppingDistance ? customStoppingDistance : stoppingDistance;
    private bool canMove = true;

    public override void OnAwake(Unit unit)
    {
        base.OnAwake(unit);

        if (unit.UnitData)
            moveSpeed = unit.UnitData.moveSpeed;
    }

    public override void OnStart() { }

    public bool SetDestination(Vector3 destination)
    {
        NavMeshPath path = new NavMeshPath();

        if (NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, path))
        {
            if (path.status == NavMeshPathStatus.PathInvalid || path.corners.Length == 0)
                return false;

            currentWaypoints.Clear();
            currentWaypoints.AddRange(path.corners);

            currentWaypointIndex = currentWaypoints.Count > 1 ? 1 : 0;
            isMoving = true;
            return true;
        }

        return false;
    }

    public void SetStoppingDistance(float value)
    {
        customStoppingDistance = value;
        editedStoppingDistance = true;
    }

    public void ResetStoppingDistance()
    {
        customStoppingDistance = 0f;
        editedStoppingDistance = false;
    }

    public void MoveTo(Vector3 destination)
    {
        SetDestination(destination);
    }

    public void Stop()
    {
        currentWaypoints.Clear();
        currentWaypointIndex = 0;
        currentVelocity = Vector3.zero;
        isMoving = false;
    }

    public override void OnTick(float deltaInterval)
    {
        if (canMove == false)
        {
            currentVelocity = Vector3.zero;
            isMoving = false;
            return;
        }
        if (!isMoving || currentWaypoints.Count == 0)
            return;

        Vector3 finalTarget = currentWaypoints[currentWaypoints.Count - 1];
        Vector3 toFinal = finalTarget - transform.position;
        toFinal.y = 0f;
        float distanceToFinal = toFinal.magnitude;

        if (distanceToFinal <= CurrentStoppingDistance)
        {
            Stop();
            return;
        }

        AdvanceWaypoints();

        Vector3 smoothedTarget = GetSmoothedTargetPosition();

        Vector3 directionToTarget = smoothedTarget - transform.position;
        directionToTarget.y = 0f;

        Vector3 moveDirection = directionToTarget.normalized;

        float desiredSpeed = moveSpeed;
        if (distanceToFinal < 2f)
        {
            float maxSpeedForDistance = Mathf.Sqrt(2f * acceleration * distanceToFinal);
            desiredSpeed = Mathf.Min(moveSpeed, maxSpeedForDistance);
        }

        Vector3 separationForce = CalculateSeparation();
        if (distanceToFinal < CurrentStoppingDistance * 3f)
        {
            float stopProgress = Mathf.Clamp01(distanceToFinal / (CurrentStoppingDistance * 3f));
            separationForce *= stopProgress;
        }

        Vector3 finalDirection = (moveDirection + separationForce).normalized;
        Vector3 targetVelocity = finalDirection * desiredSpeed;

        currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, acceleration * deltaInterval);
        transform.position += currentVelocity * deltaInterval;

        Vector3 lookDirection = currentVelocity;
        lookDirection.y = 0f;
        if (lookDirection.sqrMagnitude > 0.05f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                turnSpeed * deltaInterval
            );
        }
    }

    private void AdvanceWaypoints()
    {
        while (currentWaypointIndex < currentWaypoints.Count - 1)
        {
            Vector3 waypoint = currentWaypoints[currentWaypointIndex];
            waypoint.y = transform.position.y;

            if (Vector3.Distance(transform.position, waypoint) <= waypointThreshold)
            {
                currentWaypointIndex++;
            }
            else
            {
                break;
            }
        }
    }

    public void DelayedStop(float time)
    {
        StartCoroutine(DelayedStopCoroutine(time));
    }

    IEnumerator DelayedStopCoroutine(float time)
    {
        canMove = false;
        Stop();
        yield return new WaitForSeconds(time);
        canMove = true;
    }

    private Vector3 GetSmoothedTargetPosition()
    {
        Vector3 currentTarget = currentWaypoints[currentWaypointIndex];

        if (currentWaypointIndex < currentWaypoints.Count - 1)
        {
            Vector3 segment = currentWaypoints[currentWaypointIndex] - transform.position;
            segment.y = 0f;

            if (segment.magnitude < lookAheadDistance)
            {
                float t = Mathf.Clamp01((lookAheadDistance - segment.magnitude) / lookAheadDistance);
                return Vector3.Lerp(currentWaypoints[currentWaypointIndex], currentWaypoints[currentWaypointIndex + 1], t);
            }
        }

        return currentTarget;
    }

    private Vector3 CalculateSeparation()
    {
        Vector3 separation = Vector3.zero;
        Collider[] neighbours = Physics.OverlapSphere(transform.position, personalRadius, unitLayer);

        int count = 0;
        foreach (var col in neighbours)
        {
            if (col.gameObject == gameObject) continue;

            Vector3 pushDir = transform.position - col.transform.position;
            pushDir.y = 0f;

            float distance = pushDir.magnitude;
            if (distance > 0f)
            {
                separation += pushDir.normalized / distance;
                count++;
            }
        }

        return count > 0 ? (separation / count) : Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        if (currentWaypoints != null && currentWaypoints.Count > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < currentWaypoints.Count - 1; i++)
            {
                Gizmos.DrawLine(currentWaypoints[i], currentWaypoints[i + 1]);
            }
        }
    }
}