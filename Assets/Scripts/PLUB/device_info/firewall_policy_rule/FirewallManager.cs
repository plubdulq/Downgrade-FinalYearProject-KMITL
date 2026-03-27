using System.Collections.Generic;
using UnityEngine;

public class FirewallManager : MonoBehaviour
{
    public static FirewallManager Instance;

    private Dictionary<string, FirewallDeviceData> firewallDevices
        = new Dictionary<string, FirewallDeviceData>();

    private FirewallDeviceData currentDevice;
    private FirewallPolicy currentPolicy;

    void Awake()
    {
        Instance = this;
    }

    // =========================
    // DEVICE
    // =========================
    public void SetCurrentDevice(string deviceGuid)
    {
        if (!firewallDevices.ContainsKey(deviceGuid))
        {
            firewallDevices[deviceGuid] = new FirewallDeviceData(deviceGuid);
        }

        currentDevice = firewallDevices[deviceGuid];
        currentPolicy = null; // reset selection
    }

    public FirewallDeviceData GetCurrentDevice()
    {
        return currentDevice;
    }

    // =========================
    // POLICY
    // =========================
    public FirewallPolicy CreatePolicy(string deviceGuid, string name)
    {
        if (!firewallDevices.ContainsKey(deviceGuid))
        {
            firewallDevices[deviceGuid] = new FirewallDeviceData(deviceGuid);
        }

        return firewallDevices[deviceGuid].CreatePolicy(name);
    }

    public List<FirewallPolicy> GetPolicies(string deviceGuid)
    {
        if (!firewallDevices.ContainsKey(deviceGuid))
            return new List<FirewallPolicy>();

        return firewallDevices[deviceGuid].policies;
    }

    public void SelectPolicy(FirewallPolicy policy)
    {
        if (currentDevice == null || policy == null)
            return;

        // กันกรณี policy ที่ส่งมา ไม่ได้อยู่ใน device นี้
        if (!currentDevice.policies.Contains(policy))
        {
            Debug.LogWarning("Policy not found in current device");
            return;
        }

        currentPolicy = policy;
    }

    public FirewallPolicy GetCurrentPolicy()
    {
        return currentPolicy;
    }

    // 🔥 เพิ่ม: ลบ policy แบบปลอดภัย
    public void RemovePolicy(FirewallPolicy policy)
    {
        if (currentDevice == null || policy == null)
            return;

        if (currentDevice.policies.Remove(policy))
        {
            // ถ้าลบตัวที่เลือกอยู่ → reset
            if (currentPolicy == policy)
                currentPolicy = null;
        }
    }

    // =========================
    // RULE
    // =========================
    public void AddRule(
        string source,
        string destination,
        string port,
        string protocol,
        string action)
    {
        if (currentPolicy == null)
        {
            Debug.LogError("No policy selected");
            return;
        }

        var rule = new FirewallRule(source, destination, port, protocol, action);
        currentPolicy.AddRule(rule);
    }

    public List<FirewallRule> GetRules()
    {
        if (currentPolicy == null)
            return new List<FirewallRule>();

        return currentPolicy.rules;
    }

    // 🔥 เพิ่ม: remove rule แบบไม่พัง
    public void RemoveRule(FirewallRule rule)
    {
        if (currentPolicy == null || rule == null)
            return;

        currentPolicy.rules.Remove(rule);
    }
}