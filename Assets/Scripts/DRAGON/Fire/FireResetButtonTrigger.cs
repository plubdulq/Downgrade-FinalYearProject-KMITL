using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FireResetButtonTrigger : MonoBehaviour
{
    [Header("Reference")]
    public FireAlarmSystem fireAlarm;

    [Header("Trigger Settings")]
    public string requiredTag = "";
    public float pressCooldown = 0.5f;

    [Header("Auto Bind")]
    public bool autoBindOnAwake = true;

    [Header("Debug")]
    public bool debugLogs = true;

    float lastPressTime = -999f;

    void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    void Awake()
    {
        if (autoBindOnAwake)
            TryAutoBind();
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
                "[FireResetButtonTrigger] Auto-bind summary -> " +
                $"FireAlarm: {(fireAlarm ? fireAlarm.name : "NULL")}",
                this
            );
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (Time.time - lastPressTime < pressCooldown)
            return;

        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag))
            return;

        if (fireAlarm == null)
            TryAutoBind();

        if (fireAlarm == null)
        {
            if (debugLogs)
                Debug.LogWarning("[FireResetButtonTrigger] FireAlarmSystem reference is missing.", this);
            return;
        }

        lastPressTime = Time.time;

        if (debugLogs)
            Debug.Log("[FireResetButtonTrigger] Reset triggered by: " + other.name, this);

        fireAlarm.ResetAlarm();
    }
}