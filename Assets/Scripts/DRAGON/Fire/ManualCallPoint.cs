using UnityEngine;

public class ManualCallPoint : MonoBehaviour
{
    [Header("References")]
    public FireAlarmSystem fireAlarm;

    [Header("Mode")]
    public bool usePhysicalButtonsOnly = true;

    [Header("Options")]
    public bool oneShot = true;

    [Header("Auto Bind")]
    public bool autoBindOnAwake = true;

    [Header("Debug")]
    public bool debugLogs = true;

    bool _used;
    bool _eventsBound;

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void Awake()
    {
        if (autoBindOnAwake)
            TryAutoBind();
    }

    void OnEnable()
    {
        TryAutoBind();
        BindEventsIfPossible();
    }

    void OnDisable()
    {
        UnbindEvents();
    }

    public void TryAutoBind()
    {
        if (fireAlarm == null)
        {
            fireAlarm = GetComponentInParent<FireAlarmSystem>();

            if (fireAlarm == null)
                fireAlarm = FindFirstObjectByType<FireAlarmSystem>(FindObjectsInactive.Include);
        }

        if (debugLogs)
        {
            Debug.Log(
                "[ManualCallPoint] Auto-bind summary -> " +
                $"FireAlarm: {(fireAlarm ? fireAlarm.name : "NULL")}",
                this
            );
        }
    }

    void BindEventsIfPossible()
    {
        if (_eventsBound || fireAlarm == null)
            return;

        fireAlarm.OnReset += HandleReset;
        _eventsBound = true;
    }

    void UnbindEvents()
    {
        if (!_eventsBound)
            return;

        if (fireAlarm != null)
            fireAlarm.OnReset -= HandleReset;

        _eventsBound = false;
    }

    void HandleReset()
    {
        _used = false;

        if (debugLogs)
            Debug.Log("[MCP] Reset received from FireAlarmSystem. MCP ready again.", this);
    }

    public void PressAlarmButton()
    {
        if (fireAlarm == null)
            TryAutoBind();

        if (!fireAlarm)
        {
            Debug.LogWarning("[MCP] FireAlarmSystem is not assigned.", this);
            return;
        }

        if (oneShot && _used)
        {
            if (debugLogs)
                Debug.Log("[MCP] Alarm button pressed, but MCP is already used.", this);
            return;
        }

        _used = true;
        fireAlarm.TriggerAlarm(FireAlarmSystem.TriggerReason.ManualCallPoint);

        if (debugLogs)
            Debug.Log("[MCP] Alarm button pressed.", this);
    }

    public void PressResetButton()
    {
        if (debugLogs)
            Debug.Log("[MCP] PressResetButton() called, but MCP reset is disabled in the new flow.", this);
    }

    void OnTriggerEnter(Collider other)
    {
        if (usePhysicalButtonsOnly)
            return;

        if (fireAlarm == null)
            TryAutoBind();

        if (!fireAlarm)
        {
            Debug.LogWarning("[MCP] FireAlarmSystem is not assigned.", this);
            return;
        }

        if (oneShot && _used)
            return;

        _used = true;
        fireAlarm.TriggerAlarm(FireAlarmSystem.TriggerReason.ManualCallPoint);

        if (debugLogs)
            Debug.Log("[MCP] Triggered by collider: " + other.name, this);
    }
}