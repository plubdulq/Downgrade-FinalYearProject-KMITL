using System.Collections.Generic;
using UnityEngine;

// Script นี้ต้อง attach ที่ SmokeParticles GameObject เท่านั้น
public class SmokeParticleTrigger : MonoBehaviour
{
    public SmokeDetector smokeDetector;

    ParticleSystem _ps;
    List<ParticleSystem.Particle> _enter = new List<ParticleSystem.Particle>();

    void Awake()
    {
        _ps = GetComponent<ParticleSystem>();
    }

    void OnParticleTrigger()
    {
        if (!smokeDetector) return;

        int count = _ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, _enter);
        Debug.Log($"[SmokeParticleTrigger] OnParticleTrigger fired — {count} particles entered");

        if (count > 0)
        {
            smokeDetector.NotifyParticleEntered();
        }
    }
}