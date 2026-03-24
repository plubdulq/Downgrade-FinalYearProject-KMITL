using UnityEngine;

public class ManualCallPoint : MonoBehaviour
{
    public FireAlarmSystem fireAlarm;

    [Header("Mode")]
    public bool usePhysicalButtonsOnly = true;

    [Header("Options")]
    public bool oneShot = true;

    [Header("Debug")]
    public bool debugLogs = true;

    bool _used;

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void OnEnable()
    {
        if (fireAlarm) fireAlarm.OnReset += HandleReset;
    }

    void OnDisable()
    {
        if (fireAlarm) fireAlarm.OnReset -= HandleReset;
    }

    void HandleReset()
    {
        _used = false;

        if (debugLogs)
            Debug.Log("[MCP] Reset received from FireAlarmSystem. MCP ready again.");
    }

    public void PressAlarmButton()
    {
        if (!fireAlarm)
        {
            Debug.LogWarning("[MCP] FireAlarmSystem is not assigned.");
            return;
        }

        if (oneShot && _used)
        {
            if (debugLogs)
                Debug.Log("[MCP] Alarm button pressed, but MCP is already used.");
            return;
        }

        _used = true;
        fireAlarm.TriggerAlarm(FireAlarmSystem.TriggerReason.ManualCallPoint);

        if (debugLogs)
            Debug.Log("[MCP] Alarm button pressed.");
    }

    public void PressResetButton()
    {
        if (!fireAlarm)
        {
            Debug.LogWarning("[MCP] FireAlarmSystem is not assigned.");
            return;
        }

        if (debugLogs)
            Debug.Log("[MCP] Reset button pressed.");

        fireAlarm.ResetAlarm();
    }

    void OnTriggerEnter(Collider other)
    {
        if (usePhysicalButtonsOnly)
            return;

        if (!fireAlarm)
        {
            Debug.LogWarning("[MCP] FireAlarmSystem is not assigned.");
            return;
        }

        if (oneShot && _used)
            return;

        _used = true;
        fireAlarm.TriggerAlarm(FireAlarmSystem.TriggerReason.ManualCallPoint);

        if (debugLogs)
            Debug.Log("[MCP] Triggered by collider: " + other.name);
    }
}