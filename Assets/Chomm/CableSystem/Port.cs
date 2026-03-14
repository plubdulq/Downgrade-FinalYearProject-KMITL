using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Chomm.CableSystem
{
    public enum PortType
    {
        Power,
        Data,
        // Add more as needed
    }

    public enum JunctionAxis
    {
        Vertical,
        Horizontal
    }

    [RequireComponent(typeof(XRBaseInteractable))]
    public class Port : MonoBehaviour
    {
        public string ID;
        public PortType type;
        public System.Collections.Generic.List<Cable> connectedCables = new System.Collections.Generic.List<Cable>();
        public bool isJunction = false;
        public JunctionAxis axis = JunctionAxis.Vertical;
        public float junctionLength = 0.2f;


        public bool IsConnected
        {
            get
            {
                if (isJunction) return connectedCables.Count >= 2; // Junctions accept 2 lines (In/Out)
                return connectedCables.Count >= 1; // Normal ports accept 1
            }
        }

        public Vector3 GetJunctionOffset(bool isInput)
        {
            float direction = isInput ? -0.5f : 0.5f; // Input comes from one side, output goes to other
            Vector3 localDir = (axis == JunctionAxis.Vertical) ? Vector3.up : Vector3.right;
            return localDir * (junctionLength * direction);
        }

        public void ToggleAxis()
        {
            if (axis == JunctionAxis.Vertical) axis = JunctionAxis.Horizontal;
            else axis = JunctionAxis.Vertical;
            
            Debug.Log($"[Port] Toggled Axis to {axis}");
        }

        private XRBaseInteractable interactable;

        private void Awake()
        {
            interactable = GetComponent<XRBaseInteractable>();
        }

        private void OnEnable()
        {
            if (interactable != null)
            {
                interactable.selectEntered.AddListener(OnVRSelect);
            }
        }

        private void OnDisable()
        {
            if (interactable != null)
            {
                interactable.selectEntered.RemoveListener(OnVRSelect);
            }
        }

        private void OnVRSelect(SelectEnterEventArgs args)
        {
             // Pass the interactor's transform (the hand/controller) to the manager
             if(args.interactorObject != null)
             {
                 // Delegate all logic to Manager. 
                 // Manager will decide whether to Connect, Disconnect, or Ignore based on state.
                 //CableManager.Instance.OnPortSelected(this, args.interactorObject.transform);
             }
        }

        public void OnHoverEnter()
        {
            // Optional: Highlight logic
        }

        public void OnHoverExit()
        {
            // Optional: Remove highlight
        }

        private void OnMouseDown()
        {
            //CableManager.Instance.OnPortClicked(this);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * 0.5f);
        }
    }
}
