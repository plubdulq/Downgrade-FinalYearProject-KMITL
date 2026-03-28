using System.Collections;
using UnityEngine;

public class TriggerAir : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayer;
    public Material newMaterial;
    public float DelayChange = 3f;
    public Renderer rend;
    public Material originalMaterial;

    [Header("Auto Bind")]
    public bool autoBindRenderer = true;
    public bool autoUseDeviceLayerIfEmpty = false;
    public bool debugLogs = true;

    void Start()
    {
        AutoBind();

        if (rend != null)
            originalMaterial = rend.material;
    }

    void OnValidate()
    {
        if (!Application.isPlaying)
            AutoBind();
    }

    void AutoBind()
    {
        if (autoBindRenderer && rend == null)
        {
            rend = GetComponentInChildren<Renderer>(true);
        }

        if (targetLayer.value == 0 && autoUseDeviceLayerIfEmpty)
        {
            int deviceLayer = LayerMask.NameToLayer("Device");
            if (deviceLayer >= 0)
                targetLayer = 1 << deviceLayer;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (debugLogs)
            Debug.Log("Trigger Hit: " + other.name);

        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            if (debugLogs)
                Debug.Log("Layer Matched!");

            if (rend != null && newMaterial != null)
                rend.material = newMaterial;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            StartCoroutine(delayChang());
        }
    }

    IEnumerator delayChang()
    {
        yield return new WaitForSeconds(DelayChange);

        if (rend != null && originalMaterial != null)
        {
            rend.material = originalMaterial;
        }
    }
}