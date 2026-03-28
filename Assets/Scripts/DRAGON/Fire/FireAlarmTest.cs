using UnityEngine;

public class FireAlarmTest : MonoBehaviour
{
    public FireAlarmSystem fireAlarm;
    public SmokeRiser smokeRiser;

    void Update()
    {
        if (!fireAlarm) return;

        if (Input.GetKeyDown(KeyCode.Y))
        {
            Debug.Log("[FireAlarmTest] Y pressed — trying to start smoke");
            if (smokeRiser)
            {
                smokeRiser.StartRising();
            }
            else
            {
                Debug.LogError("[FireAlarmTest] smokeRiser is NULL — ยังไม่ได้ drag SmokeTest มาใส่!");
            }
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            fireAlarm.TriggerAlarm(FireAlarmSystem.TriggerReason.Test);
            Debug.Log("[FireAlarmTest] T pressed — manual alarm");
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            fireAlarm.ResetAlarm();
            if (smokeRiser) smokeRiser.ResetPosition();
            Debug.Log("[FireAlarmTest] R pressed — full reset");
        }
    }
}