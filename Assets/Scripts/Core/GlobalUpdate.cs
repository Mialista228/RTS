using UnityEngine;

public class GlobalUpdate : MonoBehaviour
{
    [SerializeField] private float customTicksPerSecond = 10f;

    private float customTickInterval;
    private float customTickTimer;

    private void Awake()
    {
        UpdateCustomTickInterval();
    }

    private void OnValidate()
    {
        UpdateCustomTickInterval();
    }

    private void UpdateCustomTickInterval()
    {
        customTickInterval = customTicksPerSecond > 0 ? 1f / customTicksPerSecond : 0.1f;
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        for (int i = 0; i < EruBehaviour.allUpdate.Count; i++)
            EruBehaviour.allUpdate[i].Tick(deltaTime);

        customTickTimer += deltaTime;

        int maxTicksPerFrame = 3;
        int ticksExecuted = 0;

        while (customTickTimer >= customTickInterval && ticksExecuted < maxTicksPerFrame)
        {
            customTickTimer -= customTickInterval;
            ticksExecuted++;

            for (int i = 0; i < EruBehaviour.allCustomUpdate.Count; i++)
                EruBehaviour.allCustomUpdate[i].CustomTick(customTickInterval);
        }

        if (customTickTimer > customTickInterval * 2)
        {
            customTickTimer = 0f;
        }
    }

    private void FixedUpdate()
    {
        float fixedDeltaTime = Time.fixedDeltaTime;
        for (int i = 0; i < EruBehaviour.allFixedUpdate.Count; i++)
            EruBehaviour.allFixedUpdate[i].FixedTick(fixedDeltaTime);
    }

    private void LateUpdate()
    {
        float deltaTime = Time.deltaTime;
        for (int i = 0; i < EruBehaviour.allLateUpdate.Count; i++)
            EruBehaviour.allLateUpdate[i].LateTick(deltaTime);
    }
}
