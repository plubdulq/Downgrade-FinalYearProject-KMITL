using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDoor : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip openSfx;
    public AudioClip closeSfx;
    private AudioSource audioSource;
   public string openParameter= "Open";
   public string closeParameter= "Close";
   public string idleOpenParameter = "IdleOpen";
   public string idleCloseParameter = "IdleClose";
    public Collider col;
    public string keyTag;
     [Header("Door")]
    public Animator doorAnimator; // ใส่ Animator ประตู
    void Start()
    {
        doorAnimator.SetBool(idleCloseParameter, true);

    }
    void OpenDoor()
    {
        if (doorAnimator != null)
        {
            doorAnimator.SetBool(idleCloseParameter, false);
            doorAnimator.SetBool(closeParameter,  false);
            doorAnimator.SetBool(openParameter, true);
            StartCoroutine(IdleOpenAnim());
            Play(openSfx);
        }
    }
    IEnumerator IdleOpenAnim()
    {
        yield return new WaitForSeconds(2);
        doorAnimator.SetBool(openParameter, false);
        doorAnimator.SetBool(idleOpenParameter, true);
    }
 void closeDoor()
    {
        if (doorAnimator != null)
        {
            doorAnimator.SetBool(idleOpenParameter, false);
            doorAnimator.SetBool(openParameter, false);
            doorAnimator.SetBool(closeParameter, true);
            StartCoroutine(IdleCloseAnim());
            Play(closeSfx);
        }
    }
    IEnumerator IdleCloseAnim()
    {
        yield return new WaitForSeconds(2);
         doorAnimator.SetBool(closeParameter, false);
        doorAnimator.SetBool(idleCloseParameter, true);
        
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(keyTag))
        {
            if (col != null)
                col.enabled = true;
                OpenDoor();

        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(keyTag))
        {
            if (col != null)
                col.enabled = false;
                closeDoor();

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
