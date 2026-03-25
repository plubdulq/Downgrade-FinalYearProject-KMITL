using System.Collections.Generic;
using UnityEngine;

public class NetworkSimulationManager : MonoBehaviour
{
    public List<NetworkFlow> flows = new List<NetworkFlow>();
    public static NetworkSimulationManager Instance;

    void Awake()
    {
        Instance = this;
    }

    [Header("Simulation Settings")]
    public float tickRate = 10f;

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= 1f / tickRate)
        {
            SimulateTick(timer);
            timer = 0f;
        }
    }

    void SimulateTick(float deltaTime)
    {
        var cableDict = CableManager.Instance.cableDict;

        foreach (var flow in flows)
        {
            float bytesThisTick = flow.BytesPerSecond * deltaTime;

            foreach (var cableGuid in flow.cablePath)
            {
                if (cableDict.TryGetValue(cableGuid, out var cable))
                {
                    cable.ProcessFlow(bytesThisTick, deltaTime);
                }
            }
        }
    }
}