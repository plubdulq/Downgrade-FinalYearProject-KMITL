using System.Collections;
using UnityEngine;

public class DischargeNozzleFX : MonoBehaviour
{
    [Header("References")]
    public FireAlarmSystem fireAlarm;

    [Header("Particles")]
    public ParticleSystem dischargeParticles;

    [Header("Sequence")]
    public bool useDurationFromFireAlarm = true;
    public float dischargeDuration = 8f;

    [Header("Auto Bind")]
    public bool autoBindOnAwake = true;
    public bool debugLogs = true;

    Coroutine _spray;
    bool _eventsBound;

    void Reset()
    {
        if (dischargeParticles == null)
            dischargeParticles = GetComponentInChildren<ParticleSystem>(true);
    }

    void Awake()
    {
        if (autoBindOnAwake)
            TryAutoBind();

        if (dischargeParticles == null)
            dischargeParticles = GetComponentInChildren<ParticleSystem>(true);

        if (dischargeParticles != null)
            dischargeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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
            fireAlarm = GetComponentInParent<FireAlarmSystem>();

            if (fireAlarm == null)
                fireAlarm = FindFirstObjectByType<FireAlarmSystem>(FindObjectsInactive.Include);
        }

        if (dischargeParticles == null)
            dischargeParticles = GetComponentInChildren<ParticleSystem>(true);

        if (debugLogs)
        {
            Debug.Log(
                "[DischargeNozzleFX] Auto-bind summary -> " +
                $"FireAlarm: {(fireAlarm ? fireAlarm.name : "NULL")}, " +
                $"Particles: {(dischargeParticles ? dischargeParticles.name : "NULL")}",
                this
            );
        }
    }

    void BindEventsIfPossible()
    {
        if (_eventsBound || fireAlarm == null)
            return;

        fireAlarm.OnDischarge += HandleDischarge;
        fireAlarm.OnReset += HandleReset;
        _eventsBound = true;
    }

    void UnbindEvents()
    {
        if (!_eventsBound)
            return;

        if (fireAlarm != null)
        {
            fireAlarm.OnDischarge -= HandleDischarge;
            fireAlarm.OnReset -= HandleReset;
        }

        _eventsBound = false;
    }

    void HandleDischarge()
    {
        if (_spray != null)
            StopCoroutine(_spray);

        _spray = StartCoroutine(SprayRoutine());
    }

    void HandleReset()
    {
        StopAllFx();
    }

    IEnumerator SprayRoutine()
    {
        float duration = dischargeDuration;

        if (useDurationFromFireAlarm && fireAlarm != null)
            duration = fireAlarm.dischargeEffectDuration;

        if (dischargeParticles != null)
            dischargeParticles.Play();

        if (duration > 0f)
            yield return new WaitForSeconds(duration);

        if (dischargeParticles != null)
            dischargeParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        _spray = null;
    }

    void StopAllFx()
    {
        if (_spray != null)
        {
            StopCoroutine(_spray);
            _spray = null;
        }

        if (dischargeParticles != null)
            dischargeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}