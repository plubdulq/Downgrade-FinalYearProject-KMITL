using UnityEngine;

public class SmokeRiser : MonoBehaviour
{
    [Header("References")]
    public FireAlarmSystem fireAlarm;

    [Header("Movement")]
    public float riseSpeed = 0.4f;
    public float maxHeight = 5f;

    [Header("Smoke Particles")]
    public ParticleSystem smokeParticles;

    [Header("Auto Bind")]
    public bool autoBindOnAwake = true;
    public bool autoResetOnAlarmReset = true;
    public bool debugLogs = true;

    Vector3 _startPos;
    bool _rising;
    bool _eventsBound;

    void Awake()
    {
        _startPos = transform.position;

        if (autoBindOnAwake)
            TryAutoBind();

        if (smokeParticles == null)
            smokeParticles = GetComponentInChildren<ParticleSystem>(true);

        if (smokeParticles != null)
            smokeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void OnEnable()
    {
        TryAutoBind();
        BindEventsIfPossible();
    }

    void OnDisable()
    {
        UnbindEvents();
    }

    public void TryAutoBind()
    {
        if (fireAlarm == null)
        {
            Transform root = transform.root;
            if (root != null)
                fireAlarm = root.GetComponentInChildren<FireAlarmSystem>(true);
        }

        if (fireAlarm == null)
            fireAlarm = FindFirstObjectByType<FireAlarmSystem>(FindObjectsInactive.Include);

        if (smokeParticles == null)
            smokeParticles = GetComponentInChildren<ParticleSystem>(true);

        if (debugLogs)
        {
            Debug.Log(
                "[SmokeRiser] Auto-bind summary -> " +
                $"FireAlarm: {(fireAlarm ? fireAlarm.name : "NULL")}, " +
                $"SmokeParticles: {(smokeParticles ? smokeParticles.name : "NULL")}",
                this
            );
        }
    }

    void BindEventsIfPossible()
    {
        if (_eventsBound || !autoResetOnAlarmReset || fireAlarm == null)
            return;

        fireAlarm.OnReset += HandleReset;
        _eventsBound = true;
    }

    void UnbindEvents()
    {
        if (!_eventsBound)
            return;

        if (fireAlarm != null)
            fireAlarm.OnReset -= HandleReset;

        _eventsBound = false;
    }

    void HandleReset()
    {
        ResetPosition();
    }

    public void StartRising()
    {
        if (_rising)
            return;

        transform.position = _startPos;
        _rising = true;

        if (smokeParticles != null)
            smokeParticles.Play();

        if (debugLogs)
            Debug.Log("[SmokeRiser] Smoke started rising", this);
    }

    public void ResetPosition()
    {
        _rising = false;
        transform.position = _startPos;

        if (smokeParticles != null)
            smokeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (debugLogs)
            Debug.Log("[SmokeRiser] Reset", this);
    }

    void Update()
    {
        if (!_rising)
            return;

        transform.position += Vector3.up * riseSpeed * Time.deltaTime;

        if (transform.position.y >= _startPos.y + maxHeight)
        {
            _rising = false;

            if (debugLogs)
                Debug.Log("[SmokeRiser] Reached max height", this);
        }
    }
}