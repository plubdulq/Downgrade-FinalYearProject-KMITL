using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScandKeySystem : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip ScanASfx;
    private AudioSource audioSource;
    public Collider col;
    public string keyTag;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(keyTag))
        {
            if (col != null)
                col.enabled = true;
                Play(ScanASfx);

        }
    }
    void Play(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    void PlayOneShot(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    }
