using UnityEngine;

public class ManualCallPoint : MonoBehaviour
{
    public FireAlarmSystem fireAlarm;

    [Header("Options")]
    public bool oneShot = true;

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
    }

    void OnTriggerEnter(Collider other)
    {
        if (!fireAlarm) return;
        if (oneShot && _used) return;

        _used = true;
        fireAlarm.TriggerAlarm(FireAlarmSystem.TriggerReason.ManualCallPoint);
        Debug.Log("[MCP] Triggered by: " + other.name);
    }
}