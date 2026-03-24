using UnityEngine;

public class MCPButtonTrigger : MonoBehaviour
{
    public ManualCallPoint manualCallPoint;
    public bool isResetButton = false;

    [Header("Options")]
    public float pressCooldown = 0.4f;
    public float startupIgnoreTime = 1.0f;
    public string requiredTag = "";

    [Header("Debug")]
    public bool debugLogs = true;

    float _lastPressTime = -999f;
    float _readyTime;

    void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void Start()
    {
        _readyTime = Time.time + startupIgnoreTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (Time.time < _readyTime)
            return;

        if (Time.time < _lastPressTime + pressCooldown)
            return;

        if (!manualCallPoint)
        {
            Debug.LogWarning("[MCPButtonTrigger] ManualCallPoint is not assigned.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(requiredTag) && !other.CompareTag(requiredTag))
            return;

        if (debugLogs)
            Debug.Log("[MCPButtonTrigger] Trigger entered by: " + other.name);

        _lastPressTime = Time.time;

        if (isResetButton)
        {
            if (debugLogs)
                Debug.Log("[MCPButtonTrigger] RESET pressed by: " + other.name);

            manualCallPoint.PressResetButton();
        }
        else
        {
            if (debugLogs)
                Debug.Log("[MCPButtonTrigger] ALARM pressed by: " + other.name);

            manualCallPoint.PressAlarmButton();
        }
    }
}