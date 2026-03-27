using System.Collections.Generic;

[System.Serializable]
public class FirewallDeviceData
{
    public string deviceGuid;

    public List<FirewallPolicy> policies = new List<FirewallPolicy>();

    public FirewallDeviceData(string guid)
    {
        deviceGuid = guid;
    }

    public FirewallPolicy CreatePolicy(string name)
    {
        var policy = new FirewallPolicy(name);
        policies.Add(policy);
        return policy;
    }
}