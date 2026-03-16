using System.Collections;
using UnityEngine;

public class SmokeDetector : MonoBehaviour
{
    public FireAlarmSystem fireAlarm;
    public string smokeTag = "Smoke";

    [Header("Beep Sound")]
    public AudioSource audioSource;
    public AudioClip beepClip;          // ถ้ามีไฟล์เสียง drag มาใส่ได้เลย
    public float beepInterval = 0.5f;   // ติ๊ดทุกกี่วินาที
    public int beepCount = 5;           // ติ๊ดกี่ครั้ง

    bool _triggered;
    Coroutine _beepRoutine;

    void OnEnable()
    {
        if (fireAlarm) fireAlarm.OnReset += HandleReset;
    }

    void OnDisable()
    {
        if (fireAlarm) fireAlarm.OnReset -= HandleReset;
    }

    void HandleReset()
    {
        _triggered = false;
        if (_beepRoutine != null) StopCoroutine(_beepRoutine);
        _beepRoutine = null;
        Debug.Log("[SmokeDetector] Reset");
    }

    public void NotifyParticleEntered()
    {
        if (_triggered) return;
        if (!fireAlarm) return;

        Trigger();
    }

    void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag(smokeTag)) return;
        if (!fireAlarm) return;

        Debug.Log($"[SmokeDetector] Collider entered — {other.name}");
        Trigger();
    }

    void Trigger()
    {
        _triggered = true;
        Debug.Log("[SmokeDetector] ✅ ALARM TRIGGERED by Smoke!");
        fireAlarm.TriggerAlarm(FireAlarmSystem.TriggerReason.Smoke);

        // เริ่มเสียงติ๊ด
        if (_beepRoutine != null) StopCoroutine(_beepRoutine);
        _beepRoutine = StartCoroutine(BeepRoutine());
    }

    IEnumerator BeepRoutine()
    {
        for (int i = 0; i < beepCount; i++)
        {
            PlayBeep();
            Debug.Log($"[SmokeDetector] Beep {i + 1}/{beepCount}");
            yield return new WaitForSeconds(beepInterval);
        }
    }

    void PlayBeep()
    {
        if (!audioSource) return;

        if (beepClip)
        {
            // มีไฟล์เสียง → เล่นปกติ
            audioSource.PlayOneShot(beepClip);
        }
        else
        {
            // ไม่มีไฟล์ → generate เสียง beep เอง
            audioSource.PlayOneShot(GenerateBeep(0.05f, 880f));
        }
    }

    // Generate เสียง beep แบบ procedural ไม่ต้องใช้ไฟล์เสียง
    AudioClip GenerateBeep(float duration, float frequency)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            // Sine wave + envelope fade out เพื่อกันกึกตอนหยุด
            float envelope = 1f - (t / duration);
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.5f;
        }

        AudioClip clip = AudioClip.Create("Beep", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}