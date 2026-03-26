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

    private bool _used;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void OnEnable()
    {
        if (fireAlarm) fireAlarm.OnReset += HandleReset;
    }

    private void OnDisable()
    {
        if (fireAlarm) fireAlarm.OnReset -= HandleReset;
    }

    private void HandleReset()
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

    // เก็บ method นี้ไว้กัน script อื่น reference อยู่
    // แต่ตาม flow ใหม่ MCP จะไม่ทำ Reset แล้ว
    public void PressResetButton()
    {
        if (debugLogs)
            Debug.Log("[MCP] PressResetButton() called, but MCP reset is disabled in the new flow.");
    }

    private void OnTriggerEnter(Collider other)
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