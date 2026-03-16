using UnityEngine;

public class RoomExitZone : MonoBehaviour
{
    public FireAlarmSystem fireAlarm;
    public string playerTag = "Player";

    [Header("Optional")]
    public bool resetAlarmWhenExit = false;

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (!fireAlarm) return;

        fireAlarm.NotifyUserExitedRoom(other.gameObject);

        if (resetAlarmWhenExit)
            fireAlarm.ResetAlarm();
    }
}