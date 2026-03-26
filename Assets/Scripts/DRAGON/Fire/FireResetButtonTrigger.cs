using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FireResetButtonTrigger : MonoBehaviour
{
    [Header("Reference")]
    public FireAlarmSystem fireAlarm;

    [Header("Trigger Settings")]
    public string requiredTag = "";
    public float pressCooldown = 0.5f;

    [Header("Debug")]
    public bool debugLogs = true;

    private float lastPressTime = -999f;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time - lastPressTime < pressCooldown)
            return;

        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag))
            return;

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