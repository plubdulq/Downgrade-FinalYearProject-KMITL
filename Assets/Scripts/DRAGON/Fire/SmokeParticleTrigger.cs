using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class SmokeParticleTrigger : MonoBehaviour
{
    [Header("References")]
    public SmokeDetector smokeDetector;

    [Header("Auto Bind")]
    public bool autoBindOnAwake = true;
    public bool debugLogs = true;

    ParticleSystem _ps;
    readonly List<ParticleSystem.Particle> _enter = new List<ParticleSystem.Particle>();

    void Awake()
    {
        _ps = GetComponent<ParticleSystem>();

        if (autoBindOnAwake)
            TryAutoBind();
    }

    public void TryAutoBind()
    {
        if (smokeDetector == null)
            smokeDetector = GetComponentInParent<SmokeDetector>();

        if (smokeDetector == null)
            smokeDetector = FindFirstObjectByType<SmokeDetector>(FindObjectsInactive.Include);

        if (debugLogs)
        {
            Debug.Log(
                "[SmokeParticleTrigger] Auto-bind summary -> " +
                $"SmokeDetector: {(smokeDetector ? smokeDetector.name : "NULL")}",
                this
            );
        }
    }

    void OnParticleTrigger()
    {
        if (smokeDetector == null)
            TryAutoBind();

        if (!smokeDetector || _ps == null)
            return;

        int count = _ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, _enter);

        if (debugLogs && count > 0)
            Debug.Log($"[SmokeParticleTrigger] OnParticleTrigger fired — {count} particles entered", this);

        if (count > 0)
            smokeDetector.NotifyParticleEntered();
    }
}