using System.Collections.Generic;
using UnityEngine;

public class LoadBalancerManager : MonoBehaviour
{
    public static LoadBalancerManager Instance;

    // deviceGuid -> config
    public Dictionary<string, LoadBalancerConfigModel> lbConfigs = new Dictionary<string, LoadBalancerConfigModel>();

    void Awake()
    {
        Instance = this;
    }

    // 🔥 สร้าง config ถ้ายังไม่มี
    public LoadBalancerConfigModel GetOrCreateConfig(string deviceGuid)
    {
        if (!lbConfigs.ContainsKey(deviceGuid))
        {
            lbConfigs[deviceGuid] = new LoadBalancerConfigModel
            {
                deviceGuid = deviceGuid
            };
        }

        return lbConfigs[deviceGuid];
    }

    // 🔥 เพิ่ม pool
    public void AddPool(string deviceGuid, string poolName, LoadBalancingAlgorithm algo)
    {
        var config = GetOrCreateConfig(deviceGuid);

        if (!config.pools.ContainsKey(poolName))
        {
            config.pools[poolName] = new PoolConfig
            {
                poolName = poolName,
                algorithm = algo
            };
        }
    }

    // 🔥 เพิ่ม server เข้า pool
    public void AddServerToPool(string deviceGuid, string poolName, string serverGuid)
    {
        var config = GetOrCreateConfig(deviceGuid);

        if (config.pools.TryGetValue(poolName, out var pool))
        {
            if (!pool.serverGuids.Contains(serverGuid))
            {
                pool.serverGuids.Add(serverGuid);
            }
        }
        else
        {
            Debug.LogWarning($"Pool {poolName} not found on LB {deviceGuid}");
        }
    }

    // 🔥 เปลี่ยน algorithm
    public void SetAlgorithm(string deviceGuid, string poolName, LoadBalancingAlgorithm algo)
    {
        var config = GetOrCreateConfig(deviceGuid);

        if (config.pools.TryGetValue(poolName, out var pool))
        {
            pool.algorithm = algo;
        }
    }

    // 🔥 ดึง config
    public LoadBalancerConfigModel GetConfig(string deviceGuid)
    {
        lbConfigs.TryGetValue(deviceGuid, out var config);
        return config;
    }
}