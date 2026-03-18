using System;
using UnityEngine;

public class CCTVDetectionZone : MonoBehaviour
{
    [Header("CCTV Info")]
    public string cameraName = "EntranceCCTV_01";

    [Header("Target Filter")]
    public string targetTag = "Player";

    [Header("State (Read Only)")]
    public bool isPlayerInside = false;
    public string lastEnterTime = "";
    public string lastExitTime = "";

    private void Start()
    {
        Debug.Log($"[CCTV] {cameraName} Detection Zone started on object: {gameObject.name}");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[CCTV DEBUG] ENTER by: {other.name}, tag: {other.tag}");

        if (!other.CompareTag(targetTag))
        {
            Debug.Log($"[CCTV DEBUG] Ignored {other.name} because tag is {other.tag}, not {targetTag}");
            return;
        }

        isPlayerInside = true;
        lastEnterTime = DateTime.Now.ToString("HH:mm:ss");
        Debug.Log($"[CCTV] {cameraName} detected PLAYER ENTER at {lastEnterTime}");
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"[CCTV DEBUG] EXIT by: {other.name}, tag: {other.tag}");

        if (!other.CompareTag(targetTag))
        {
            Debug.Log($"[CCTV DEBUG] Ignored {other.name} because tag is {other.tag}, not {targetTag}");
            return;
        }

        isPlayerInside = false;
        lastExitTime = DateTime.Now.ToString("HH:mm:ss");
        Debug.Log($"[CCTV] {cameraName} detected PLAYER EXIT at {lastExitTime}");
    }
}