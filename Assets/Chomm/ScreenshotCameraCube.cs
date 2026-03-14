using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNG;

public class ScreenshotCameraCube : GrabbableEvents
{
    [Tooltip("Sound to play when screenshot is taken")]
    public AudioClip ShutterSound;

    [Tooltip("Particle effect to play when screenshot is taken")]
    public ParticleSystem FlashEffect;

    public override void OnTriggerDown()
    {
        base.OnTriggerDown();

        // 1. Play Feedback
        if (ShutterSound != null)
        {
            VRUtils.Instance.PlaySpatialClipAt(ShutterSound, transform.position, 1f);
        }

        if (FlashEffect != null)
        {
            FlashEffect.Play();
        }

        // 2. Take Screenshot
        // We use the last loaded save name
        string nameToUse = PlayerPrefs.GetString("LastLoadedSaveName", "");
        
        // If empty, it means we are in a temporary room. Do NOT save screenshot.
        if (string.IsNullOrEmpty(nameToUse))
        {
             Debug.Log("Screenshot skipped: Room is temporary (not saved).");
             return;
        }

        ScreenshotManager.Instance.CaptureAndSave(nameToUse);
        
        Debug.Log("Camera Cube triggering screenshot for: " + nameToUse);
    }
}
