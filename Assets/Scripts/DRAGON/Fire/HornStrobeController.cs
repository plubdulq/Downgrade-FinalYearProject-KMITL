using System.Collections;
using UnityEngine;

public class HornStrobeController : MonoBehaviour
{
    public FireAlarmSystem fireAlarm;

    [Header("Audio")]
    public AudioSource siren;
    public AudioClip sirenClip;
    public bool loop = true;

    [Header("3D Sound")]
    [Range(0f, 1f)] public float spatialBlend = 1f;
    public float minDistance = 2f;
    public float maxDistance = 30f;

    [Header("Light")]
    public Light strobeLight;
    public float blinkInterval = 0.25f;

    [Header("Timing")]
    [Tooltip("หน่วง siren + strobe กี่วิ เพื่อให้ beep ดังก่อน")]
    public float sirenDelay = 3f;

    Coroutine _blink;
    Coroutine _delayed;

    void Awake()
    {
        if (siren)
        {
            siren.playOnAwake = false;
            siren.loop = loop;
            siren.spatialBlend = spatialBlend;
            siren.minDistance = minDistance;
            siren.maxDistance = maxDistance;

            if (sirenClip)
                siren.clip = sirenClip;

            siren.Stop();
        }

        if (strobeLight)
            strobeLight.enabled = false;
    }

    void OnEnable()
    {
        if (!fireAlarm) return;

        fireAlarm.OnAlarm += HandleAlarm;
        fireAlarm.OnReset += HandleReset;
    }

    void OnDisable()
    {
        if (!fireAlarm) return;

        fireAlarm.OnAlarm -= HandleAlarm;
        fireAlarm.OnReset -= HandleReset;
    }

    void HandleAlarm()
    {
        if (_delayed != null)
            StopCoroutine(_delayed);

        _delayed = StartCoroutine(DelayedAlarmFX());
    }

    IEnumerator DelayedAlarmFX()
    {
        if (sirenDelay > 0f)
            yield return new WaitForSeconds(sirenDelay);

        if (fireAlarm == null || !fireAlarm.IsAlarmOn)
            yield break;

        if (_blink != null)
            StopCoroutine(_blink);

        if (strobeLight)
            _blink = StartCoroutine(Blink());

        if (siren && !siren.isPlaying)
            siren.Play();
    }

    void HandleReset()
    {
        StopAllFx();
    }

    void StopAllFx()
    {
        if (_delayed != null)
        {
            StopCoroutine(_delayed);
            _delayed = null;
        }

        if (_blink != null)
        {
            StopCoroutine(_blink);
            _blink = null;
        }

        if (siren)
            siren.Stop();

        if (strobeLight)
            strobeLight.enabled = false;
    }

    IEnumerator Blink()
    {
        while (true)
        {
            if (strobeLight)
                strobeLight.enabled = !strobeLight.enabled;

            yield return new WaitForSeconds(blinkInterval);
        }
    }
}