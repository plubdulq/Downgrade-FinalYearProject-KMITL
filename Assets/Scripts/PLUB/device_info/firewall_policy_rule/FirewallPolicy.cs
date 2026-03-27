using System;
using System.Collections.Generic;

[System.Serializable]
public class FirewallPolicy
{
    public string policyName;

    public List<FirewallRule> rules = new List<FirewallRule>();

    public FirewallPolicy(string name)
    {
        policyName = name;
    }

    public void AddRule(FirewallRule rule)
    {
        rules.Add(rule);
    }
}