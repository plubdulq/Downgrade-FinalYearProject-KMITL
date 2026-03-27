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

    [Header("Auto Bind")]
    public bool autoBindOnAwake = true;
    public bool debugLogs = true;

    bool _eventsBound = false;
    bool _didInitialAutoBindLog = false;

    void Reset()
    {
        if (fireAlarmSystem == null)
            fireAlarmSystem = GetComponent<FireAlarmSystem>();
    }

    void Awake()
    {
        if (autoBindOnAwake)
            TryAutoBindAll();

        HideOptionalUi();
    }

    void OnEnable()
    {
        TryAutoBindAll();
        BindEventsIfPossible();
        HideOptionalUi();
    }

    void OnDisable()
    {
        UnbindEvents();
    }

    public void TryAutoBindAll()
    {
        bool changed = false;

        if (fireAlarmSystem == null)
        {
            fireAlarmSystem = GetComponent<FireAlarmSystem>();

            if (fireAlarmSystem == null)
                fireAlarmSystem = FindFirstObjectByType<FireAlarmSystem>(FindObjectsInactive.Include);

            if (fireAlarmSystem != null)
                changed = true;
        }

        if (vrHudCountdown == null)
        {
            vrHudCountdown = FindFirstObjectByType<VRHUDCountdown>(FindObjectsInactive.Include);

            if (vrHudCountdown != null)
                changed = true;
        }

        if (vrHudCountdown != null)
        {
            Transform hudRoot = vrHudCountdown.transform;

            if (dischargeActivatedUI == null)
            {
                Transform t = FindChildRecursiveByName(hudRoot, "HUD_DischargeActivatedText");
                if (t != null)
                {
                    dischargeActivatedUI = t.gameObject;
                    changed = true;
                }
            }

            if (waitingForResetUI == null)
            {
                Transform t = FindChildRecursiveByName(hudRoot, "HUD_WaitingForResetText");
                if (t != null)
                {
                    waitingForResetUI = t.gameObject;
                    changed = true;
                }
            }
        }

        if (!_didInitialAutoBindLog || changed)
        {
            LogBindSummary();
            _didInitialAutoBindLog = true;
        }
    }

    Transform FindChildRecursiveByName(Transform root, string targetName)
    {
        if (root == null)
            return null;

        if (root.name == targetName)
            return root;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform result = FindChildRecursiveByName(root.GetChild(i), targetName);
            if (result != null)
                return result;
        }

        return null;
    }

    void LogBindSummary()
    {
        if (!debugLogs)
            return;

        Debug.Log(
            "[VRHUDCountdownBinder] Auto-bind summary -> " +
            $"FireAlarmSystem: {(fireAlarmSystem ? fireAlarmSystem.name : "NULL")}, " +
            $"VRHUDCountdown: {(vrHudCountdown ? vrHudCountdown.name : "NULL")}, " +
            $"DischargeUI: {(dischargeActivatedUI ? dischargeActivatedUI.name : "NULL")}, " +
            $"WaitingUI: {(waitingForResetUI ? waitingForResetUI.name : "NULL")}",
            this
        );
    }

    void BindEventsIfPossible()
    {
        if (_eventsBound || fireAlarmSystem == null)
            return;

        fireAlarmSystem.OnPreDischargeStart += HandlePreDischargeStart;
        fireAlarmSystem.OnDischarge += HandleDischarge;
        fireAlarmSystem.OnDischargeComplete += HandleDischargeComplete;
        fireAlarmSystem.OnReset += HandleReset;

        _eventsBound = true;
    }

    void UnbindEvents()
    {
        if (!_eventsBound)
            return;

        if (fireAlarmSystem != null)
        {
            fireAlarmSystem.OnPreDischargeStart -= HandlePreDischargeStart;
            fireAlarmSystem.OnDischarge -= HandleDischarge;
            fireAlarmSystem.OnDischargeComplete -= HandleDischargeComplete;
            fireAlarmSystem.OnReset -= HandleReset;
        }

        _eventsBound = false;
    }

    void HandlePreDischargeStart(int duration)
    {
        if (vrHudCountdown == null || dischargeActivatedUI == null || waitingForResetUI == null)
            TryAutoBindAll();

        HideOptionalUi();

        if (vrHudCountdown == null)
        {
            Debug.LogWarning("[VRHUDCountdownBinder] vrHudCountdown is NULL. Cannot start HUD countdown.", this);
            return;
        }

        int seconds;

        if (useDurationFromFireAlarm)
            seconds = duration;
        else if (useRoomSizeFallback)
            seconds = GetCountdownFromRoomSize(roomWidth, roomLength);
        else
            seconds = duration;

        vrHudCountdown.StartCountdown(seconds);
    }

    void HandleDischarge()
    {
        if (vrHudCountdown == null || dischargeActivatedUI == null || waitingForResetUI == null)
            TryAutoBindAll();

        if (vrHudCountdown != null)
            vrHudCountdown.StopCountdownKeepHudVisible();

        SetActiveSafe(dischargeActivatedUI, true);
        SetActiveSafe(waitingForResetUI, false);
    }

    void HandleDischargeComplete()
    {
        if (vrHudCountdown == null || dischargeActivatedUI == null || waitingForResetUI == null)
            TryAutoBindAll();

        SetActiveSafe(dischargeActivatedUI, false);
        SetActiveSafe(waitingForResetUI, true);
    }

    void HandleReset()
    {
        if (vrHudCountdown == null || dischargeActivatedUI == null || waitingForResetUI == null)
            TryAutoBindAll();

        if (vrHudCountdown != null)
            vrHudCountdown.StopCountdown();

        HideOptionalUi();
    }

    void HideOptionalUi()
    {
        SetActiveSafe(dischargeActivatedUI, false);
        SetActiveSafe(waitingForResetUI, false);
    }

    void SetActiveSafe(GameObject go, bool state)
    {
        if (go != null)
            go.SetActive(state);
    }

    int GetCountdownFromRoomSize(float width, float length)
    {
        float area = width * length;

        if (area <= 36f)
            return 15;
        if (area <= 64f)
            return 20;

        return 30;
    }
}