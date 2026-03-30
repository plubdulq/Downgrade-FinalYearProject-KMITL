using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace ByteSize.KnobsXR.Samples
{
    public class GetXRInteractionManagerReference : MonoBehaviour
    {
        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

        private void Awake()
        {
            interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();

            interactable.interactionManager = FindInteractionManager();
        }

        private XRInteractionManager FindInteractionManager()
        {
            return FindObjectOfType<XRInteractionManager>();
        }
    }
}
