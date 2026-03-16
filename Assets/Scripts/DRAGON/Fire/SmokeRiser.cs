using UnityEngine;

public class SmokeRiser : MonoBehaviour
{
    [Header("Movement")]
    public float riseSpeed = 0.4f;
    public float maxHeight = 5f;

    [Header("Smoke Particles")]
    public ParticleSystem smokeParticles;

    Vector3 _startPos;
    bool _rising;

    void Start()
    {
        _startPos = transform.position;

        // ไม่ให้ออกเองตอน Play
        if (smokeParticles) smokeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    // เรียกจากข้างนอก (FireAlarmTest กด S)
    public void StartRising()
    {
        if (_rising) return;

        transform.position = _startPos;
        _rising = true;

        if (smokeParticles) smokeParticles.Play();
        Debug.Log("[SmokeRiser] Smoke started rising");
    }

    public void ResetPosition()
    {
        _rising = false;
        transform.position = _startPos;
        if (smokeParticles) smokeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        Debug.Log("[SmokeRiser] Reset");
    }

    void Update()
    {
        if (!_rising) return;

        transform.position += Vector3.up * riseSpeed * Time.deltaTime;

        if (transform.position.y >= _startPos.y + maxHeight)
        {
            _rising = false;
            Debug.Log("[SmokeRiser] Reached max height");
        }
    }
}