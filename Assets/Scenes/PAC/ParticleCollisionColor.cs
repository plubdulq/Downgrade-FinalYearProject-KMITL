using UnityEngine;
using System.Collections.Generic;

public class ParticleCollisionBounce : MonoBehaviour
{
    [Header("===== Color =====")]
    public Color firstColor = Color.red;
    public Color secondColor = Color.yellow;

    [Header("===== Timing =====")]
    public float secondColorDelay = 1f;

    [Header("===== Physics =====")]
    public float bouncePower = 1.2f;
    public float hitDistance = 0.2f;

    [Header("===== Lifetime (On Hit) =====")]
    public float newLifetime = 2f;
    public float minNewLifetime = 0.1f;
    public float maxNewLifetime = 10f;

    [Header("===== Start Lifetime =====")]
    public float startLifetimeValue = 3f;
    public float minStartLifetime = 0.1f;
    public float maxStartLifetime = 10f;

    [Header("===== Emission Rate =====")]
    public float minRate = 0;
    public float maxRate = 200;

    [Header("===== Custom Max Particle =====")]
    public int customMaxParticles = 500;
    public int minParticles = 10;
    public int maxParticles = 5000;

    [Header("===== Layer =====")]
    public LayerMask meshLayer;

    Dictionary<uint, float> hitTimes = new Dictionary<uint, float>();
    List<ParticleCollisionEvent> collisionEvents;

    ParticleSystem ps;
    ParticleSystem.Particle[] particles;

    ParticleSystem.EmissionModule emission;
    ParticleSystem.MainModule main;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();

        collisionEvents = new List<ParticleCollisionEvent>();

        emission = ps.emission;
        main = ps.main;

        ApplyStartLifetime();
        ApplyMaxParticles();
    }

    void Update()
    {
        int particleCount = ps.particleCount;

        if (particleCount == 0) return;

        if (particles == null || particles.Length < particleCount)
            particles = new ParticleSystem.Particle[particleCount];

        ps.GetParticles(particles);

        for (int i = 0; i < particleCount; i++)
        {
            uint id = particles[i].randomSeed;

            if (hitTimes.ContainsKey(id))
            {
                float t = Time.time - hitTimes[id];

                if (t > secondColorDelay)
                {
                    particles[i].startColor = secondColor;
                }
            }
        }

        ps.SetParticles(particles, particleCount);
    }

    void OnParticleCollision(GameObject other)
    {
        if (((1 << other.layer) & meshLayer) == 0)
            return;

        int eventCount = ps.GetCollisionEvents(other, collisionEvents);

        int particleCount = ps.particleCount;

        if (particles == null || particles.Length < particleCount)
            particles = new ParticleSystem.Particle[particleCount];

        ps.GetParticles(particles);

        for (int i = 0; i < eventCount; i++)
        {
            Vector3 hitPos = collisionEvents[i].intersection;
            Vector3 normal = collisionEvents[i].normal;

            for (int j = 0; j < particleCount; j++)
            {
                float dist = Vector3.Distance(particles[j].position, hitPos);

                if (dist < hitDistance)
                {
                    particles[j].startColor = firstColor;

                    // lifetime เฉพาะตอนชน
                    particles[j].remainingLifetime = newLifetime;

                    // bounce
                    particles[j].velocity =
                        Vector3.Reflect(particles[j].velocity, normal) * bouncePower;

                    uint id = particles[j].randomSeed;

                    if (!hitTimes.ContainsKey(id))
                        hitTimes.Add(id, Time.time);
                    else
                        hitTimes[id] = Time.time;
                }
            }
        }

        ps.SetParticles(particles, particleCount);
    }

    // ===============================
    // START LIFETIME CONTROL
    // ===============================

    public void SetStartLifetime(float value)
    {
        startLifetimeValue = Mathf.Clamp(value, minStartLifetime, maxStartLifetime);
        ApplyStartLifetime();
    }

    public void IncreaseStartLifetime(float value)
    {
        startLifetimeValue = Mathf.Clamp(startLifetimeValue + value, minStartLifetime, maxStartLifetime);
        ApplyStartLifetime();
    }

    public void DecreaseStartLifetime(float value)
    {
        startLifetimeValue = Mathf.Clamp(startLifetimeValue - value, minStartLifetime, maxStartLifetime);
        ApplyStartLifetime();
    }

    void ApplyStartLifetime()
    {
        main.startLifetime = startLifetimeValue;
    }

    // ===============================
    // HIT LIFETIME CONTROL
    // ===============================

    public void SetHitLifetime(float value)
    {
        newLifetime = Mathf.Clamp(value, minNewLifetime, maxNewLifetime);
    }

    public void IncreaseHitLifetime(float value)
    {
        newLifetime = Mathf.Clamp(newLifetime + value, minNewLifetime, maxNewLifetime);
    }

    public void DecreaseHitLifetime(float value)
    {
        newLifetime = Mathf.Clamp(newLifetime - value, minNewLifetime, maxNewLifetime);
    }

    // ===============================
    // EMISSION CONTROL
    // ===============================

    public void SetParticleRate(float amount)
    {
        emission.rateOverTime = Mathf.Clamp(amount, minRate, maxRate);
    }

    public void IncreaseParticleRate(float value)
    {
        emission.rateOverTime =
            Mathf.Clamp(emission.rateOverTime.constant + value, minRate, maxRate);
    }

    public void DecreaseParticleRate(float value)
    {
        emission.rateOverTime =
            Mathf.Clamp(emission.rateOverTime.constant - value, minRate, maxRate);
    }

    // ===============================
    // CUSTOM MAX PARTICLES CONTROL
    // ===============================

    public void SetMaxParticles(int value)
    {
        int newMax =  Mathf.RoundToInt(value);
        customMaxParticles = Mathf.Clamp(newMax, minParticles, maxParticles);
        ApplyMaxParticles();
    }

    public void IncreaseMaxParticles(int value)
    {
        customMaxParticles = Mathf.Clamp(customMaxParticles + value, minParticles, maxParticles);
        ApplyMaxParticles();
    }

    public void DecreaseMaxParticles(int value)
    {
        customMaxParticles = Mathf.Clamp(customMaxParticles - value, minParticles, maxParticles);
        ApplyMaxParticles();
    }

    void ApplyMaxParticles()
    {
        main.maxParticles = customMaxParticles;
    }
}