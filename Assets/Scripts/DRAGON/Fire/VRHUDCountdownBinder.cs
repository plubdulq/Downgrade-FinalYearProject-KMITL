using UnityEngine;

public class VRHUDCountdownBinder : MonoBehaviour
{
    [Header("References")]
    public FireAlarmSystem fireAlarmSystem;
    public VRHUDCountdown vrHudCountdown;

    [Header("Countdown Source")]
    public bool useDurationFromFireAlarm = true;

    [Header("Fallback Room Size Logic")]
    public bool useRoomSizeFallback = false;
    public float roomWidth = 5f;
    public float roomLength = 5f;

    [Header("Optional UI Objects")]
    public GameObject dischargeActivatedUI;
    public GameObject waitingForResetUI;

    private void Reset()
    {
        if (fireAlarmSystem == null)
            fireAlarmSystem = GetComponent<FireAlarmSystem>();
    }

    private void OnEnable()
    {
        if (fireAlarmSystem == null)
            fireAlarmSystem = GetComponent<FireAlarmSystem>();

        if (fireAlarmSystem != null)
        {
            fireAlarmSystem.OnPreDischargeStart += HandlePreDischargeStart;
            fireAlarmSystem.OnDischarge += HandleDischarge;
            fireAlarmSystem.OnDischargeComplete += HandleDischargeComplete;
            fireAlarmSystem.OnReset += HandleReset;
        }
        else
        {
            Debug.LogWarning("[VRHUDCountdownBinder] fireAlarmSystem is NULL");
        }

        HideOptionalUi();
    }

    private void OnDisable()
    {
        if (fireAlarmSystem != null)
        {
            fireAlarmSystem.OnPreDischargeStart -= HandlePreDischargeStart;
            fireAlarmSystem.OnDischarge -= HandleDischarge;
            fireAlarmSystem.OnDischargeComplete -= HandleDischargeComplete;
            fireAlarmSystem.OnReset -= HandleReset;
        }
    }

    private void HandlePreDischargeStart(int duration)
    {
        Debug.Log("[VRHUDCountdownBinder] OnPreDischargeStart received: " + duration);

        HideOptionalUi();

        if (vrHudCountdown == null)
        {
            Debug.LogWarning("[VRHUDCountdownBinder] vrHudCountdown is NULL");
            return;
        }

        int seconds;

        if (useDurationFromFireAlarm)
        {
            seconds = duration;
        }
        else if (useRoomSizeFallback)
        {
            seconds = GetCountdownFromRoomSize(roomWidth, roomLength);
        }
        else
        {
            seconds = duration;
        }

        vrHudCountdown.StartCountdown(seconds);
    }

    private void HandleDischarge()
    {
        Debug.Log("[VRHUDCountdownBinder] OnDischarge received");

        if (vrHudCountdown != null)
            vrHudCountdown.StopCountdown();

        SetActiveSafe(dischargeActivatedUI, true);
        SetActiveSafe(waitingForResetUI, false);
    }

    private void HandleDischargeComplete()
    {
        Debug.Log("[VRHUDCountdownBinder] OnDischargeComplete received");

        // ตอนนี้ข้อความ Discharge Activated อยู่มาครบช่วง effect แล้ว
        SetActiveSafe(dischargeActivatedUI, false);
        SetActiveSafe(waitingForResetUI, true);
    }

    private void HandleReset()
    {
        Debug.Log("[VRHUDCountdownBinder] OnReset received");

        if (vrHudCountdown != null)
            vrHudCountdown.StopCountdown();

        HideOptionalUi();
    }

    private void HideOptionalUi()
    {
        SetActiveSafe(dischargeActivatedUI, false);
        SetActiveSafe(waitingForResetUI, false);
    }

    private void SetActiveSafe(GameObject go, bool state)
    {
        if (go != null)
            go.SetActive(state);
    }

    private int GetCountdownFromRoomSize(float width, float length)
    {
        float area = width * length;

        if (area <= 36f)
            return 15;
        if (area <= 64f)
            return 20;

        return 30;
    }
}