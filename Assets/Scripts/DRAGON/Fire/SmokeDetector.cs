using System.Collections;
using UnityEngine;

public class SmokeDetector : MonoBehaviour
{
    [Header("References")]
    public FireAlarmSystem fireAlarm;
    public string smokeTag = "Smoke";

    [Header("Beep Sound")]
    public AudioSource audioSource;
    public AudioClip beepClip;
    public float beepInterval = 0.5f;
    public int beepCount = 5;

    [Header("Auto Bind")]
    public bool autoBindOnAwake = true;
    public bool debugLogs = true;

    bool _triggered;
    Coroutine _beepRoutine;
    bool _eventsBound;

    void Awake()
    {
        if (autoBindOnAwake)
            TryAutoBind();
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

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = GetComponentInChildren<AudioSource>(true);

        if (debugLogs)
        {
            Debug.Log(
                "[SmokeDetector] Auto-bind summary -> " +
                $"FireAlarm: {(fireAlarm ? fireAlarm.name : "NULL")}, " +
                $"AudioSource: {(audioSource ? audioSource.name : "NULL")}",
                this
            );
        }
    }

    void BindEventsIfPossible()
    {
        if (_eventsBound || fireAlarm == null)
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
        _triggered = false;

        if (_beepRoutine != null)
            StopCoroutine(_beepRoutine);

        _beepRoutine = null;

        if (debugLogs)
            Debug.Log("[SmokeDetector] Reset", this);
    }

    public void NotifyParticleEntered()
    {
        if (_triggered)
            return;

        if (fireAlarm == null)
            TryAutoBind();

        if (!fireAlarm)
            return;

        Trigger();
    }

    void OnTriggerEnter(Collider other)
    {
        if (_triggered)
            return;

        if (!other.CompareTag(smokeTag))
            return;

        if (fireAlarm == null)
            TryAutoBind();

        if (!fireAlarm)
            return;

        if (debugLogs)
            Debug.Log($"[SmokeDetector] Collider entered — {other.name}", this);

        Trigger();
    }

    void Trigger()
    {
        _triggered = true;

        if (debugLogs)
            Debug.Log("[SmokeDetector] ALARM TRIGGERED by Smoke!", this);

        fireAlarm.TriggerAlarm(FireAlarmSystem.TriggerReason.Smoke);

        if (_beepRoutine != null)
            StopCoroutine(_beepRoutine);

        _beepRoutine = StartCoroutine(BeepRoutine());
    }

    IEnumerator BeepRoutine()
    {
        for (int i = 0; i < beepCount; i++)
        {
            PlayBeep();

            if (debugLogs)
                Debug.Log($"[SmokeDetector] Beep {i + 1}/{beepCount}", this);

            yield return new WaitForSeconds(beepInterval);
        }

        _beepRoutine = null;
    }

    void PlayBeep()
    {
        if (!audioSource)
            return;

        if (beepClip)
        {
            audioSource.PlayOneShot(beepClip);
        }
        else
        {
            audioSource.PlayOneShot(GenerateBeep(0.05f, 880f));
        }
    }

    AudioClip GenerateBeep(float duration, float frequency)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - (t / duration);
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.5f;
        }

        AudioClip clip = AudioClip.Create("Beep", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}