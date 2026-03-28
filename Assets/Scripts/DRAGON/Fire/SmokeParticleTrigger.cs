using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class SmokeParticleTrigger : MonoBehaviour
{
    [Header("References")]
    public SmokeDetector smokeDetector;
    public ParticleSystem smokeParticles;

    [Header("Auto Bind")]
    public bool autoBindOnAwake = true;
    public bool debugLogs = true;

    void Awake()
    {
        if (autoBindOnAwake)
            AutoBind();
    }

    [ContextMenu("Auto Bind Now")]
    public void AutoBind()
    {
        if (!smokeParticles)
            smokeParticles = GetComponent<ParticleSystem>();

        if (!smokeDetector)
        {
            smokeDetector = GetComponentInParent<SmokeDetector>();

            if (!smokeDetector)
                smokeDetector = FindFirstObjectByType<SmokeDetector>(FindObjectsInactive.Include);
        }

        if (!smokeParticles || !smokeDetector)
        {
            if (debugLogs)
                Debug.LogWarning("[SmokeParticleTrigger] AutoBind failed. Missing ParticleSystem or SmokeDetector.", this);
            return;
        }

        Collider detectorCol = smokeDetector.GetComponent<Collider>();

        if (!detectorCol)
        {
            if (debugLogs)
                Debug.LogWarning("[SmokeParticleTrigger] SmokeDetector found, but no Collider on detector.", this);
            return;
        }

        var trigger = smokeParticles.trigger;
        trigger.enabled = true;
        trigger.SetCollider(0, detectorCol);

        trigger.inside = ParticleSystemOverlapAction.Ignore;
        trigger.outside = ParticleSystemOverlapAction.Ignore;
        trigger.enter = ParticleSystemOverlapAction.Callback;
        trigger.exit = ParticleSystemOverlapAction.Ignore;

        if (debugLogs)
            Debug.Log($"[SmokeParticleTrigger] Bound Trigger collider to: {detectorCol.name}", this);
    }

    void OnEnable()
    {
        if (!smokeParticles)
            smokeParticles = GetComponent<ParticleSystem>();

        if (autoBindOnAwake)
            AutoBind();
    }

    void OnParticleTrigger()
    {
        if (!smokeDetector || !smokeParticles)
            return;

        List<ParticleSystem.Particle> entered = new List<ParticleSystem.Particle>();
        int count = smokeParticles.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, entered);

        if (count <= 0)
            return;

        smokeDetector.NotifyParticleEntered();

        if (debugLogs)
            Debug.Log($"[SmokeParticleTrigger] Trigger ENTER detected: {count} particles", this);
    }
}