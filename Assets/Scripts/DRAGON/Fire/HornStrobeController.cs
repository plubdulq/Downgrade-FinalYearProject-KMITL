using System.Collections;
using UnityEngine;

public class HornStrobeController : MonoBehaviour
{
    [Header("References")]
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

    [Header("Auto Bind")]
    public bool autoBindOnAwake = true;
    public bool debugLogs = true;

    Coroutine _blink;
    Coroutine _delayed;
    bool _eventsBound;

    void Reset()
    {
        if (siren == null)
            siren = GetComponent<AudioSource>();

        if (strobeLight == null)
            strobeLight = GetComponentInChildren<Light>(true);

        if (fireAlarm == null)
            fireAlarm = GetComponentInParent<FireAlarmSystem>();
    }

    void Awake()
    {
        if (autoBindOnAwake)
            TryAutoBind();

        if (siren != null)
        {
            siren.playOnAwake = false;
            siren.loop = loop;
            siren.spatialBlend = spatialBlend;
            siren.minDistance = minDistance;
            siren.maxDistance = maxDistance;

            if (sirenClip != null)
                siren.clip = sirenClip;

            siren.Stop();
        }

        if (strobeLight != null)
            strobeLight.enabled = false;
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

        if (siren == null)
            siren = GetComponent<AudioSource>();

        if (siren == null)
            siren = GetComponentInChildren<AudioSource>(true);

        if (strobeLight == null)
            strobeLight = GetComponentInChildren<Light>(true);

        if (debugLogs)
        {
            Debug.Log(
                "[HornStrobeController] Auto-bind summary -> " +
                $"FireAlarm: {(fireAlarm ? fireAlarm.name : "NULL")}, " +
                $"Siren: {(siren ? siren.name : "NULL")}, " +
                $"Strobe: {(strobeLight ? strobeLight.name : "NULL")}",
                this
            );
        }
    }

    void BindEventsIfPossible()
    {
        if (_eventsBound || fireAlarm == null)
            return;

        fireAlarm.OnAlarm += HandleAlarm;
        fireAlarm.OnReset += HandleReset;
        _eventsBound = true;
    }

    void UnbindEvents()
    {
        if (!_eventsBound)
            return;

        if (fireAlarm != null)
        {
            fireAlarm.OnAlarm -= HandleAlarm;
            fireAlarm.OnReset -= HandleReset;
        }

        _eventsBound = false;
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

        if (strobeLight != null)
            _blink = StartCoroutine(Blink());

        if (siren != null && !siren.isPlaying)
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

        if (siren != null)
            siren.Stop();

        if (strobeLight != null)
            strobeLight.enabled = false;
    }

    IEnumerator Blink()
    {
        while (true)
        {
            if (strobeLight != null)
                strobeLight.enabled = !strobeLight.enabled;

            yield return new WaitForSeconds(blinkInterval);
        }
    }
}