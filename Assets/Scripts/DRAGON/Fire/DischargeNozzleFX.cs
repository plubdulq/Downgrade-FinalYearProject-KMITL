using System.Collections;
using UnityEngine;

public class DischargeNozzleFX : MonoBehaviour
{
    public FireAlarmSystem fireAlarm;

    [Header("Particles")]
    public ParticleSystem dischargeParticles;

    [Header("Sequence")]
    public float dischargeDuration = 8f;

    Coroutine _spray;

    void Awake()
    {
        if (!dischargeParticles)
            dischargeParticles = GetComponentInChildren<ParticleSystem>(true);

        if (dischargeParticles)
            dischargeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void OnEnable()
    {
        if (!fireAlarm) return;
        fireAlarm.OnDischarge += HandleDischarge;
        fireAlarm.OnReset     += HandleReset;
    }

    void OnDisable()
    {
        if (!fireAlarm) return;
        fireAlarm.OnDischarge -= HandleDischarge;
        fireAlarm.OnReset     -= HandleReset;
    }

    void HandleDischarge()
    {
        if (_spray != null) StopCoroutine(_spray);
        _spray = StartCoroutine(SprayRoutine());
    }

    void HandleReset()
    {
        StopAll();
    }

    IEnumerator SprayRoutine()
    {
        if (dischargeParticles) dischargeParticles.Play();

        if (dischargeDuration > 0f)
            yield return new WaitForSeconds(dischargeDuration);

        if (dischargeParticles)
            dischargeParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    void StopAll()
    {
        if (_spray != null) StopCoroutine(_spray);
        _spray = null;

        if (dischargeParticles)
            dischargeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}