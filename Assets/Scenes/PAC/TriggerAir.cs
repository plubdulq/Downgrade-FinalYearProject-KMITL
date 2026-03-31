using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerAir : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayer;
    public Material newMaterial;
    public float DelayChange = 3f;
    public Renderer rend;
    public Material originalMaterial;

    void Start()
    {
       
        rend = GetComponentInChildren<Renderer>();
        if (rend != null)
            originalMaterial = rend.material;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger Hit: " + other.name);

        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            Debug.Log("Layer Matched!");
            rend.GetComponentInChildren<Renderer>().material = newMaterial;
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
                rend.GetComponentInChildren<Renderer>().material = originalMaterial;
            }
    }
}