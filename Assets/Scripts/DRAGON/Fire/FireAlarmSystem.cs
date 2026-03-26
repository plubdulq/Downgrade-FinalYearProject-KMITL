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

    public bool IsAlarmOn { get; private set; }                  // ระบบฉุกเฉินยัง active
    public bool IsPreDischargeActive { get; private set; }       // อยู่ช่วง countdown
    public bool IsDischarged { get; private set; }               // discharge เริ่มแล้ว
    public bool IsDischargeEffectActive { get; private set; }    // effect กำลังทำงานอยู่
    public bool IsWaitingForReset { get; private set; }          // effect จบแล้ว แต่ยังรอ reset
    public TriggerReason LastReason { get; private set; } = TriggerReason.Unknown;

    [Header("Timing")]
    public bool usePreDischarge = true;
    public float preDischargeDuration = 10f;

    [Tooltip("ถ้าเปิด จะปล่อย Discharge อัตโนมัติหลัง Pre-Discharge")]
    public bool autoDischarge = true;

    [Header("Discharge")]
    public float dischargeDelay = 0f;
    public float dischargeEffectDuration = 5f;

    // Events
    public event Action OnAlarm;
    public event Action<TriggerReason> OnAlarmWithReason;
    public event Action<int> OnPreDischargeStart;
    public event Action OnDischarge;
    public event Action OnDischargeComplete;
    public event Action OnReset;

    // Exit event (รองรับ RoomExitZone)
    public event Action<GameObject> OnUserExitedRoom;

    private Coroutine _flow;

    // -------------------------
    // Public API
    // -------------------------
    public void TriggerAlarm()
    {
        TriggerAlarm(TriggerReason.Unknown);
    }

    public void TriggerAlarm(TriggerReason reason)
    {
        // ถ้าระบบยัง active อยู่แล้ว ไม่ต้อง trigger ซ้ำ
        if (IsAlarmOn)
        {
            Debug.Log($"[FireAlarmSystem] Trigger ignored. Alarm already active. Current reason: {LastReason}");
            return;
        }

        IsAlarmOn = true;
        IsPreDischargeActive = false;
        IsDischarged = false;
        IsDischargeEffectActive = false;
        IsWaitingForReset = false;
        LastReason = reason;

        Debug.Log($"[FireAlarmSystem] Alarm TRIGGERED (Reason: {reason})");

        OnAlarm?.Invoke();
        OnAlarmWithReason?.Invoke(reason);

        if (_flow != null)
            StopCoroutine(_flow);

        _flow = StartCoroutine(AlarmFlow());
    }

    public void ResetAlarm()
    {
        if (_flow != null)
        {
            StopCoroutine(_flow);
            _flow = null;
        }

        IsAlarmOn = false;
        IsPreDischargeActive = false;
        IsDischarged = false;
        IsDischargeEffectActive = false;
        IsWaitingForReset = false;
        LastReason = TriggerReason.Unknown;

        Debug.Log("[FireAlarmSystem] Alarm RESET");
        OnReset?.Invoke();
    }

    public void ForceDischargeNow()
    {
        if (IsDischarged)
        {
            Debug.Log("[FireAlarmSystem] ForceDischargeNow ignored. Already discharged.");
            return;
        }

        if (!IsAlarmOn)
        {
            IsAlarmOn = true;
            LastReason = TriggerReason.Unknown;
            Debug.Log("[FireAlarmSystem] ForceDischargeNow turned alarm on.");
            OnAlarm?.Invoke();
            OnAlarmWithReason?.Invoke(LastReason);
        }

        if (_flow != null)
        {
            StopCoroutine(_flow);
            _flow = null;
        }

        IsPreDischargeActive = false;
        StartCoroutine(DischargeSequence());
    }

    public void NotifyUserExitedRoom(GameObject who = null)
    {
        Debug.Log("[FireAlarmSystem] User exited room" + (who ? $" : {who.name}" : ""));
        OnUserExitedRoom?.Invoke(who);
    }

    // -------------------------
    // Internal Flow
    // -------------------------
    private IEnumerator AlarmFlow()
    {
        if (usePreDischarge && preDischargeDuration > 0f)
        {
            IsPreDischargeActive = true;

            int countdownSeconds = Mathf.CeilToInt(preDischargeDuration);
            OnPreDischargeStart?.Invoke(countdownSeconds);

            float t = 0f;
            while (t < preDischargeDuration)
            {
                if (!IsAlarmOn)
                    yield break;

                t += Time.deltaTime;
                yield return null;
            }

            IsPreDischargeActive = false;
        }

        if (!autoDischarge)
        {
            _flow = null;
            yield break;
        }

        yield return StartCoroutine(DischargeSequence());
        _flow = null;
    }

    private IEnumerator DischargeSequence()
    {
        if (dischargeDelay > 0f)
            yield return new WaitForSeconds(dischargeDelay);

        if (!IsAlarmOn)
            yield break;

        DoDischarge();

        IsDischargeEffectActive = true;
        IsWaitingForReset = false;

        if (dischargeEffectDuration > 0f)
        {
            float dischargeT = 0f;
            while (dischargeT < dischargeEffectDuration)
            {
                if (!IsAlarmOn)
                    yield break;

                dischargeT += Time.deltaTime;
                yield return null;
            }
        }

        IsDischargeEffectActive = false;
        IsWaitingForReset = true;

        Debug.Log("[FireAlarmSystem] Discharge effect complete. Waiting for manual reset.");
        OnDischargeComplete?.Invoke();
    }

    private void DoDischarge()
    {
        if (IsDischarged)
            return;

        IsDischarged = true;
        IsWaitingForReset = false;

        Debug.Log("[FireAlarmSystem] DISCHARGE");
        OnDischarge?.Invoke();
    }
}