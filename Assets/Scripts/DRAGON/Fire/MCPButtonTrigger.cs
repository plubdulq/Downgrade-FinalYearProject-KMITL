using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MCPButtonTrigger : MonoBehaviour
{
    [Header("References")]
    public ManualCallPoint manualCallPoint;
    public bool isResetButton = false;

    [Header("Options")]
    public float pressCooldown = 0.4f;
    public float startupIgnoreTime = 1.0f;
    public string requiredTag = "";

    [Header("Auto Bind")]
    public bool autoBindOnAwake = true;

    [Header("Debug")]
    public bool debugLogs = true;

    float _lastPressTime = -999f;
    float _readyTime;

    void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void Awake()
    {
        if (autoBindOnAwake)
            TryAutoBind();
    }

    void Start()
    {
        _readyTime = Time.time + startupIgnoreTime;
    }

    public void TryAutoBind()
    {
        if (manualCallPoint == null)
            manualCallPoint = GetComponentInParent<ManualCallPoint>();

        if (debugLogs)
        {
            Debug.Log(
                "[MCPButtonTrigger] Auto-bind summary -> " +
                $"ManualCallPoint: {(manualCallPoint ? manualCallPoint.name : "NULL")}",
                this
            );
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (Time.time < _readyTime)
            return;

        if (Time.time < _lastPressTime + pressCooldown)
            return;

        if (manualCallPoint == null)
            TryAutoBind();

        if (!manualCallPoint)
        {
            Debug.LogWarning("[MCPButtonTrigger] ManualCallPoint is not assigned.", this);
            return;
        }

        if (!string.IsNullOrWhiteSpace(requiredTag) && !other.CompareTag(requiredTag))
            return;

        if (debugLogs)
            Debug.Log("[MCPButtonTrigger] Trigger entered by: " + other.name, this);

        _lastPressTime = Time.time;

        if (isResetButton)
        {
            if (debugLogs)
                Debug.Log("[MCPButtonTrigger] RESET pressed by: " + other.name, this);

            manualCallPoint.PressResetButton();
        }
        else
        {
            if (debugLogs)
                Debug.Log("[MCPButtonTrigger] ALARM pressed by: " + other.name, this);

            manualCallPoint.PressAlarmButton();
        }
    }
}