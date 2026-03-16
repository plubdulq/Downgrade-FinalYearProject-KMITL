using System;
using System.Collections;
using UnityEngine;

public class FireAlarmSystem : MonoBehaviour
{
    public enum TriggerReason
    {
        Unknown = 0,
        ManualCallPoint = 1,
        Smoke = 2,
        Heat = 3,
        Test = 4
    }

    public bool IsAlarmOn { get; private set; }
    public bool IsPreDischargeActive { get; private set; }
    public bool IsDischarged { get; private set; }
    public TriggerReason LastReason { get; private set; } = TriggerReason.Unknown;

    [Header("Timing")]
    public bool usePreDischarge = true;
    public float preDischargeDuration = 10f;

    [Tooltip("ถ้าเปิด จะปล่อย Discharge อัตโนมัติหลัง Pre-Discharge")]
    public bool autoDischarge = false;
    public float dischargeDelay = 0f;

    // Events (ไฟล์อื่นจะ subscribe)
    public event Action OnAlarm;
    public event Action<TriggerReason> OnAlarmWithReason;
    public event Action<float> OnPreDischargeStart;
    public event Action OnDischarge;
    public event Action OnReset;

    // Exit event (รองรับ RoomExitZone)
    public event Action<GameObject> OnUserExitedRoom;

    Coroutine _flow;

    // ---- Public API ----

    // Overload กันพัง: เรียกได้แบบไม่ส่ง reason
    public void TriggerAlarm()
    {
        TriggerAlarm(TriggerReason.Unknown);
    }

    public void TriggerAlarm(TriggerReason reason)
    {
        if (IsAlarmOn) return;

        IsAlarmOn = true;
        LastReason = reason;

        Debug.Log($"[FireAlarmSystem] Alarm TRIGGERED (Reason: {reason})");

        OnAlarm?.Invoke();
        OnAlarmWithReason?.Invoke(reason);

        if (_flow != null) StopCoroutine(_flow);
        _flow = StartCoroutine(AlarmFlow());
    }

    public void ResetAlarm()
    {
        if (_flow != null) StopCoroutine(_flow);
        _flow = null;

        IsAlarmOn = false;
        IsPreDischargeActive = false;
        IsDischarged = false;
        LastReason = TriggerReason.Unknown;

        Debug.Log("[FireAlarmSystem] Alarm RESET");
        OnReset?.Invoke();
    }

    public void ForceDischargeNow()
    {
        if (IsDischarged) return;

        // ถ้าบังคับปล่อยโดยตรง แต่ยังไม่ได้ trigger alarm ก็ถือว่า alarm on
        if (!IsAlarmOn) IsAlarmOn = true;

        if (_flow != null) StopCoroutine(_flow);
        _flow = null;

        DoDischarge();
    }

    // รองรับ RoomExitZone.cs ที่เรียกเมธอดนี้
    public void NotifyUserExitedRoom(GameObject who = null)
    {
        Debug.Log("[FireAlarmSystem] User exited room" + (who ? $" : {who.name}" : ""));
        OnUserExitedRoom?.Invoke(who);
    }

    // ---- Internal Flow ----
    IEnumerator AlarmFlow()
    {
        if (usePreDischarge && preDischargeDuration > 0f)
        {
            IsPreDischargeActive = true;
            OnPreDischargeStart?.Invoke(preDischargeDuration);

            float t = 0f;
            while (t < preDischargeDuration)
            {
                if (!IsAlarmOn) yield break;
                t += Time.deltaTime;
                yield return null;
            }

            IsPreDischargeActive = false;
        }

        if (autoDischarge)
        {
            if (dischargeDelay > 0f)
                yield return new WaitForSeconds(dischargeDelay);

            DoDischarge();
        }

        _flow = null;
    }

    void DoDischarge()
    {
        if (IsDischarged) return;

        IsDischarged = true;
        Debug.Log("[FireAlarmSystem] DISCHARGE");
        OnDischarge?.Invoke();
    }
}