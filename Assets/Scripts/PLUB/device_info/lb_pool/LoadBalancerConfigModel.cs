using System;
using System.Collections.Generic;

public enum LoadBalancingAlgorithm
{
    RoundRobin,
    LeastConnections,
    Random
}

[Serializable]
public class PoolConfig
{
    public string poolName;
    public LoadBalancingAlgorithm algorithm;

    // เก็บ guid ของ server
    public List<string> serverGuids = new List<string>();
}

[Serializable]
public class LoadBalancerConfigModel
{
    public string deviceGuid;

    // poolName -> PoolConfig
    public Dictionary<string, PoolConfig> pools = new Dictionary<string, PoolConfig>();
}