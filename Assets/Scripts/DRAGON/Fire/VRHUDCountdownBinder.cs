using UnityEngine;
using TMPro;

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
    public bool includeInactiveWhenSearching = true;
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
        // กันกรณี object ถูก enable ใหม่ หรือ scene โหลดลำดับแปลก
        TryAutoBindAll();
        BindEventsIfPossible();
        HideOptionalUi();
    }

    void OnDisable()
    {
        UnbindEvents();
    }

    // --------------------------------------------------
    // Auto Bind
    // --------------------------------------------------
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
            vrHudCountdown = FindHudCountdownInScene();

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

    VRHUDCountdown FindHudCountdownInScene()
    {
        // 1) หาแบบตรง ๆ ก่อน
        VRHUDCountdown found = FindFirstObjectByType<VRHUDCountdown>(FindObjectsInactive.Include);
        if (found != null)
            return found;

        // 2) fallback: เผื่อชื่อ HUD ตรง แต่ component ยังหาไม่เจอจากลำดับแปลก
        GameObject hudGo = FindGameObjectByName("HUD_CountdownCanvas");
        if (hudGo != null)
            return hudGo.GetComponent<VRHUDCountdown>();

        return null;
    }

    GameObject FindGameObjectByName(string targetName)
    {
        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();

        for (int i = 0; i < allTransforms.Length; i++)
        {
            Transform t = allTransforms[i];

            if (t == null)
                continue;

            if (t.hideFlags != HideFlags.None)
                continue;

            if (t.name == targetName)
                return t.gameObject;
        }

        return null;
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

        string fireAlarmName = fireAlarmSystem ? fireAlarmSystem.name : "NULL";
        string hudName = vrHudCountdown ? vrHudCountdown.name : "NULL";
        string dischargeName = dischargeActivatedUI ? dischargeActivatedUI.name : "NULL";
        string waitingName = waitingForResetUI ? waitingForResetUI.name : "NULL";

        Debug.Log(
            "[VRHUDCountdownBinder] Auto-bind summary -> " +
            $"FireAlarmSystem: {fireAlarmName}, " +
            $"VRHUDCountdown: {hudName}, " +
            $"DischargeUI: {dischargeName}, " +
            $"WaitingUI: {waitingName}",
            this
        );

        if (fireAlarmSystem == null)
            Debug.LogWarning("[VRHUDCountdownBinder] FireAlarmSystem not found. Binder cannot subscribe to events.", this);

        if (vrHudCountdown == null)
            Debug.LogWarning("[VRHUDCountdownBinder] VRHUDCountdown not found in scene. HUD countdown will not display.", this);

        if (dischargeActivatedUI == null)
            Debug.LogWarning("[VRHUDCountdownBinder] HUD_DischargeActivatedText not found under VRHUDCountdown.", this);

        if (waitingForResetUI == null)
            Debug.LogWarning("[VRHUDCountdownBinder] HUD_WaitingForResetText not found under VRHUDCountdown.", this);
    }

    // --------------------------------------------------
    // Event Binding
    // --------------------------------------------------
    void BindEventsIfPossible()
    {
        if (_eventsBound)
            return;

        if (fireAlarmSystem == null)
            return;

        fireAlarmSystem.OnPreDischargeStart += HandlePreDischargeStart;
        fireAlarmSystem.OnDischarge += HandleDischarge;
        fireAlarmSystem.OnDischargeComplete += HandleDischargeComplete;
        fireAlarmSystem.OnReset += HandleReset;

        _eventsBound = true;

        if (debugLogs)
            Debug.Log("[VRHUDCountdownBinder] Event binding complete.", this);
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

    // --------------------------------------------------
    // Event Handlers
    // --------------------------------------------------
    void HandlePreDischargeStart(int duration)
    {
        if (debugLogs)
            Debug.Log("[VRHUDCountdownBinder] OnPreDischargeStart received: " + duration, this);

        // กันกรณี scene โหลด HUD ช้ากว่า prefab
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

    void HandleDischarge()
    {
        if (debugLogs)
            Debug.Log("[VRHUDCountdownBinder] OnDischarge received", this);

        if (vrHudCountdown == null || dischargeActivatedUI == null || waitingForResetUI == null)
            TryAutoBindAll();

        if (vrHudCountdown != null)
            vrHudCountdown.StopCountdownKeepHudVisible();

        SetActiveSafe(dischargeActivatedUI, true);
        SetActiveSafe(waitingForResetUI, false);
    }

    void HandleDischargeComplete()
    {
        if (debugLogs)
            Debug.Log("[VRHUDCountdownBinder] OnDischargeComplete received", this);

        if (vrHudCountdown == null || dischargeActivatedUI == null || waitingForResetUI == null)
            TryAutoBindAll();

        SetActiveSafe(dischargeActivatedUI, false);
        SetActiveSafe(waitingForResetUI, true);
    }

    void HandleReset()
    {
        if (debugLogs)
            Debug.Log("[VRHUDCountdownBinder] OnReset received", this);

        if (vrHudCountdown == null || dischargeActivatedUI == null || waitingForResetUI == null)
            TryAutoBindAll();

        if (vrHudCountdown != null)
            vrHudCountdown.StopCountdown();

        HideOptionalUi();
    }

    // --------------------------------------------------
    // Helpers
    // --------------------------------------------------
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