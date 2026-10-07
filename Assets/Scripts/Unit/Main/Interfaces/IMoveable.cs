using UnityEngine;

public interface IMoveable
{
    float MoveSpeed { get; }
    float StoppingDistance { get; }
    bool IsReachedDestination { get; }
    Vector3 CurrentVelocity { get; }
    void MoveTo(Vector3 position);
    void SetStoppingDistance(float stoppingDistance);
    void ResetStoppingDistance();
    void Stop();
}