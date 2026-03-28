using System.Collections;
using TMPro;
using UnityEngine;

public class VRHUDCountdown : MonoBehaviour
{
    [Header("HUD References")]
    public GameObject hudRoot;
    public TMP_Text countdownText;

    [Header("Auto Bind")]
    public bool autoAssignOnAwake = true;
    public bool debugLogs = true;

    Coroutine countdownRoutine;

    void Awake()
    {
        if (autoAssignOnAwake)
            TryAutoAssignReferences();

        if (countdownText != null)
            countdownText.text = "";

        if (hudRoot != null)
            hudRoot.SetActive(false);
    }

    public void TryAutoAssignReferences()
    {
        if (hudRoot == null)
            hudRoot = gameObject;

        if (countdownText == null)
        {
            Transform t = FindChildRecursiveByName(transform, "HUD_CountdownText");
            if (t != null)
                countdownText = t.GetComponent<TMP_Text>();
        }

        if (debugLogs)
        {
            Debug.Log(
                "[VRHUDCountdown] Auto-assign summary -> " +
                $"hudRoot: {(hudRoot ? hudRoot.name : "NULL")}, " +
                $"countdownText: {(countdownText ? countdownText.name : "NULL")}",
                this
            );
        }
    }

    Transform FindChildRecursiveByName(Transform root, string targetName)
    {
        if (root == null)
            return null;

        if (root.name == targetName)
            return root;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform result = FindChildRecursiveByName(root.GetChild(i), targetName);
            if (result != null)
                return result;
        }

        return null;
    }

    public void StartCountdown(int seconds)
    {
        if (hudRoot == null || countdownText == null)
            TryAutoAssignReferences();

        if (hudRoot == null)
        {
            Debug.LogWarning("[VRHUDCountdown] hudRoot is NULL", this);
            return;
        }

        if (countdownText == null)
        {
            Debug.LogWarning("[VRHUDCountdown] countdownText is NULL", this);
            return;
        }

        if (countdownRoutine != null)
            StopCoroutine(countdownRoutine);

        hudRoot.SetActive(true);
        countdownRoutine = StartCoroutine(CountdownRoutine(seconds));
    }

    public void StopCountdown()
    {
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

    public void StopCountdownKeepHudVisible()
    {
        if (countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
            countdownRoutine = null;
        }

        if (countdownText != null)
            countdownText.text = "";

        if (hudRoot != null)
            hudRoot.SetActive(true);
    }

    IEnumerator CountdownRoutine(int seconds)
    {
        int remaining = seconds;

        while (remaining > 0)
        {
            countdownText.text = $"Please leave the room in {remaining}";
            yield return new WaitForSeconds(1f);
            remaining--;
        }

        countdownText.text = "";
        countdownRoutine = null;
    }
}