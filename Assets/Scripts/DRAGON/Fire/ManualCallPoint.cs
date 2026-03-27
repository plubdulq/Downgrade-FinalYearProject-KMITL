using System.Collections;
using UnityEngine;

public class ManualCallPoint : MonoBehaviour
{
    [Header("References")]
    public FireAlarmSystem fireAlarm;
    public SmokeRiser smokeRiser;

    [Header("Mode")]
    public bool usePhysicalButtonsOnly = true;

    [Header("Options")]
    public bool oneShot = true;

    [Header("MCP Action")]
    public bool startSmokeInsteadOfTriggerAlarm = true;
    public float smokeStartDelay = 0f;

    [Header("Auto Bind")]
    public bool autoBindOnAwake = true;

    [Header("Debug")]
    public bool debugLogs = true;

    bool _used;
    bool _smokeStarted;
    bool _eventsBound;
    Coroutine _startSmokeRoutine;

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
            fireAlarm = FindFromSameFireSystemRoot<FireAlarmSystem>();

        if (fireAlarm == null)
            fireAlarm = FindFirstObjectByType<FireAlarmSystem>(FindObjectsInactive.Include);

        if (smokeRiser == null)
            smokeRiser = FindFromSameFireSystemRoot<SmokeRiser>();

        if (smokeRiser == null)
            smokeRiser = FindFirstObjectByType<SmokeRiser>(FindObjectsInactive.Include);

        if (debugLogs)
        {
            Debug.Log(
                "[ManualCallPoint] Auto-bind summary -> " +
                $"FireAlarm: {(fireAlarm ? fireAlarm.name : "NULL")}, " +
                $"SmokeRiser: {(smokeRiser ? smokeRiser.name : "NULL")}",
                this
            );
        }
    }

    T FindFromSameFireSystemRoot<T>() where T : Component
    {
        Transform root = transform.root;
        if (root == null) return null;
        return root.GetComponentInChildren<T>(true);
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
        _smokeStarted = false;

        if (_startSmokeRoutine != null)
        {
            StopCoroutine(_startSmokeRoutine);
            _startSmokeRoutine = null;
        }

        if (debugLogs)
            Debug.Log("[MCP] Reset received from FireAlarmSystem. MCP ready again.", this);
    }

    public void PressAlarmButton()
    {
        if (startSmokeInsteadOfTriggerAlarm)
        {
            StartSmokeFlow();
            return;
        }

        TriggerAlarmDirectly();
    }

    void StartSmokeFlow()
    {
        if (smokeRiser == null)
            TryAutoBind();

        if (smokeRiser == null)
        {
            Debug.LogWarning("[MCP] SmokeRiser is not assigned.", this);
            return;
        }

        if (oneShot && _smokeStarted)
        {
            if (debugLogs)
                Debug.Log("[MCP] Smoke already started. Ignored.", this);
            return;
        }

        _used = true;
        _smokeStarted = true;

        if (_startSmokeRoutine != null)
            StopCoroutine(_startSmokeRoutine);

        _startSmokeRoutine = StartCoroutine(StartSmokeRoutine());

        if (debugLogs)
            Debug.Log("[MCP] Alarm button pressed -> start smoke flow.", this);
    }

    IEnumerator StartSmokeRoutine()
    {
        if (smokeStartDelay > 0f)
            yield return new WaitForSeconds(smokeStartDelay);

        if (smokeRiser != null)
            smokeRiser.StartRising();

        _startSmokeRoutine = null;
    }

    void TriggerAlarmDirectly()
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
            Debug.Log("[MCP] Alarm button pressed -> direct alarm trigger.", this);
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

        PressAlarmButton();

        if (debugLogs)
            Debug.Log("[MCP] Triggered by collider: " + other.name, this);
    }
}