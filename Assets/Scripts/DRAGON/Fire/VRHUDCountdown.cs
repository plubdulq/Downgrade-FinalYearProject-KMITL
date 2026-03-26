using System.Collections;
using TMPro;
using UnityEngine;

public class VRHUDCountdown : MonoBehaviour
{
    [Header("HUD References")]
    public GameObject hudRoot;
    public TMP_Text countdownText;

    private Coroutine countdownRoutine;

    private void Awake()
    {
        if (countdownText != null)
            countdownText.text = "";

        if (hudRoot != null)
            hudRoot.SetActive(false);
    }

    public void StartCountdown(int seconds)
    {
        Debug.Log("[VRHUDCountdown] StartCountdown called with: " + seconds);

        if (hudRoot == null)
        {
            Debug.LogWarning("[VRHUDCountdown] hudRoot is NULL");
            return;
        }

        if (countdownText == null)
        {
            Debug.LogWarning("[VRHUDCountdown] countdownText is NULL");
            return;
        }

        if (countdownRoutine != null)
            StopCoroutine(countdownRoutine);

        hudRoot.SetActive(true);
        countdownRoutine = StartCoroutine(CountdownRoutine(seconds));
    }

    public void StopCountdown()
    {
        Debug.Log("[VRHUDCountdown] StopCountdown called");

        if (countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
            countdownRoutine = null;
        }

        if (countdownText != null)
            countdownText.text = "";

        if (hudRoot != null)
            hudRoot.SetActive(false);
    }

    private IEnumerator CountdownRoutine(int seconds)
    {
        int remaining = seconds;

        while (remaining > 0)
        {
            countdownText.text = $"Please leave the room in {remaining}";
            yield return new WaitForSeconds(1f);
            remaining--;
        }

        countdownText.text = "Discharge activated";
        yield return new WaitForSeconds(1f);

        countdownText.text = "";
        hudRoot.SetActive(false);
        countdownRoutine = null;
    }
}