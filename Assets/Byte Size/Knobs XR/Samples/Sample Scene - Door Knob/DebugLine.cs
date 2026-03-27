using UnityEngine;
using ByteSize.KnobsXR;

[RequireComponent(typeof(LineRenderer))]
public class DebugLine : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private KnobsXRInteractable interactable;

    private void Awake()
    {
        this.lineRenderer = GetComponent<LineRenderer>();
        this.interactable = GetComponent<KnobsXRInteractable>();
    }

    public void Update()
    {
        this.lineRenderer.SetPosition(0, this.transform.position);
        this.lineRenderer.SetPosition(1, this.interactable.ConstraintAxis + this.transform.position);
    }
}
