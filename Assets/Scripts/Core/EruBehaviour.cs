using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EruBehaviour : MonoBehaviour
{
    public static List<EruBehaviour> allUpdate = new List<EruBehaviour>(10001);
    public static List<EruBehaviour> allFixedUpdate = new List<EruBehaviour>(10001);
    public static List<EruBehaviour> allLateUpdate = new List<EruBehaviour>(10001);
    public static List<EruBehaviour> allCustomUpdate = new List<EruBehaviour>(10001);

    private void OnEnable() => allUpdate.Add(this);
    private void OnDisable()
    {
        allUpdate.Remove(this);
        allFixedUpdate.Remove(this);
        allLateUpdate.Remove(this);
        allCustomUpdate.Remove(this);
    }

    private void OnDestroy()
    {
        allUpdate.Remove(this);
        allFixedUpdate.Remove(this);
        allLateUpdate.Remove(this);
        allCustomUpdate.Remove(this);
    }

    protected void AddFixedUpdate() => allFixedUpdate.Add(this);
    protected void RemoveFixedUpdate() => allFixedUpdate.Remove(this);
    protected void AddLateUpdate() => allLateUpdate.Add(this);
    protected void RemoveLateUpdate() => allLateUpdate.Remove(this);
    protected void AddCustomUpdate() => allCustomUpdate.Add(this);
    protected void RemoveCustomUpdate() => allCustomUpdate.Remove(this);

    public void Tick(float deltaInterval) => OnTick(deltaInterval);
    public void FixedTick(float deltaInterval) => OnFixedTick(deltaInterval);
    public void LateTick(float deltaInterval) => OnLateTick(deltaInterval);
    public void CustomTick(float deltaInterval) => OnCustomTick(deltaInterval);

    public virtual void OnTick(float deltaInterval) { }
    public virtual void OnFixedTick(float deltaInterval) { }
    public virtual void OnLateTick(float deltaInterval) { }
    public virtual void OnCustomTick(float deltaInterval) { }
}