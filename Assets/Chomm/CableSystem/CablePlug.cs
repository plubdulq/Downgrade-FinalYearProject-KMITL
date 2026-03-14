using BNG;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;

namespace Chomm.CableSystem
{
    public class CablePlug : GrabbableEvents
    {
        public PlugType plugType;
        [Tooltip("The tip of the plug (Connection point)")]
        public Transform Start1;

        [Tooltip("The exit point where the cable wire starts")]
        public Transform Start2;

        [Tooltip("The visuals of the plug to hide when in an outer port (holster)")]
        public GameObject PlugModel;
        public GameObject canvas;
        private Cable parentCable;
        private SnapZone hoveredZone; // Track the zone we are currently touching
        public Cable GetParentCable() => parentCable;
        bool uiActive = false;
        private Grabber[] sceneGrabbers;
        private float lastToggleTime;
        private int lastToggleFrame = -1;

        private void Awake()
        {
            base.Awake(); // Critical: Initialize GrabbableEvents (finds the 'grab' component)
             // Try to find the cable we belong to IMMEDIATELY before any re-parenting happens
            parentCable = GetComponentInParent<Cable>();
        }
        private void Start()
        {
            if(canvas ==  null && CableDataUI.Instance != null)
                canvas = CableDataUI.Instance.gameObject.transform.parent.gameObject;
            
            if (canvas != null)
            {
                canvas.SetActive(false);
            }
            
            // Backup check
            if (parentCable == null) parentCable = GetComponentInParent<Cable>();

            // Cache grabbers for distance check when snapped
            sceneGrabbers = FindObjectsOfType<Grabber>();
        }

        [Tooltip("Distance from the plug to create the auto-anchor when connected to an Outer Port")]
        public float autoAnchorDistance = 0.2f;

        public override void OnGrab(Grabber grabber)
        {
            base.OnGrab(grabber);
            if (CableDataUI.Instance != null)
            {
                CableDataUI.Instance.UpdateUI(this);
            }
            if (OutlineManager.Instance != null)
            {
                OutlineManager.Instance.DisableAll();
            }
            canvas.SetActive(false);
            PlayerInv.Instance.CheckPlug(this);
            PlugModel.SetActive(true);

        }

        public override void OnBecomesClosestGrabbable(Grabber touchingGrabber)
        {
            base.OnBecomesClosestGrabbable(touchingGrabber);
            if (canvas != null)
            {
                //canvas.SetActive(true);
            }
        }

        public override void OnNoLongerClosestGrabbable(Grabber touchingGrabber)
        {
            base.OnNoLongerClosestGrabbable(touchingGrabber);
            if (canvas != null)
            {
                //canvas.SetActive(false);
            }
        }

        public override void OnSnapZoneEnter()
        {
            base.OnSnapZoneEnter();
            Debug.Log("Snap Zone");
            // Check if we are in a SnapZone
            if (grab != null && grab.transform.parent != null)
            {
                SnapZone zone = grab.transform.parent.GetComponent<SnapZone>();
                if (zone != null)
                {
                    // Existing logic: Hide visual if outer port
                    //switch (zone.plugType)
                    //{
                    //    case PlugType.Rj45:
                    //        IsOuterPort(zone);
                    //        break;
                    //    case PlugType.Dsl:
                    //        IsOuterPort(zone);
                    //        break;
                    //    case PlugType.PowerIn:
                    //        IsOuterPort(zone);
                    //        break;
                    //    case PlugType.PowerOut:
                    //        Debug.Log("Power Plug");
                    //        IsOuterPort(zone);
                    //        break;
                    //    case PlugType.IECC13:
                    //    case PlugType.IECC14:
                    //    case PlugType.IECC19:
                    //    case PlugType.IECC20:
                    //    case PlugType.StandardPlug:
                    //    case PlugType.FiberLCSinglemode:
                    //    case PlugType.FiberLCMultimode:
                    //    case PlugType.None:

                    //        Debug.Log("Power Plug");
                    //        IsOuterPort(zone);
                    //        break;
                    //}
                            IsOuterPort(zone);
                }
            }

            // 2. Adjust Slack based on distance
            if (parentCable != null && parentCable.PlugA != null && parentCable.PlugB != null)
            {
                // Ensure both ends are valid for distance check
                if (parentCable.PlugA.Start2 != null && parentCable.PlugB.Start2 != null)
                {
                    float dist = Vector3.Distance(parentCable.PlugA.Start2.position, parentCable.PlugB.Start2.position);
                    
                    // Logic:
                    // 0.2 -> 0.2
                    // 0.4 -> 0.6
                    // 0.6+ -> 0.8
                    if (dist <= 0.3f)
                    {
                        parentCable.Sag = 0.2f;
                    }
                    else if (dist <= 0.5f)
                    {
                        parentCable.Sag = 0.6f;
                    }
                    else
                    {
                    }
                }
            }

            if (CableDataUI.Instance != null)
            {
                CableDataUI.Instance.UpdateUI(this);
            }
        }
        void IsOuterPort(SnapZone zone)
        {
            if (zone.isOuterPort)
            {
                if (PlugModel != null)
                {
                    PlugModel.SetActive(false);
                }

                // Add Auto Anchor Logic
                if (parentCable != null && Start1 != null && Start2 != null)
                {
                    if (zone.CustomCableAnchors != null && zone.CustomCableAnchors.Count > 0)
                    {
                        // Use existing points from the Zone
                        parentCable.SetAutoAnchor(this, zone.CustomCableAnchors);
                    }
                }
            }

        }

        public override void OnSnapZoneExit()
        {
            base.OnSnapZoneExit();

            if (PlugModel != null)
            {
                PlugModel.SetActive(true);
            }

            // Remove Auto Anchor if we are disconnecting
            if (parentCable != null)
            {
                parentCable.RemoveAutoAnchor(this);
                // Reset Slack to default when unsnapped
                parentCable.Sag = 0.5f;

            }

            // Debug Collider Status
            Collider[] cols = GetComponentsInChildren<Collider>();
            foreach(var c in cols)
            {
                Debug.Log($"[CablePlug] Unsnap Exit: Collider {c.name} Enabled={c.enabled}, IsTrigger={c.isTrigger}, GO Active={c.gameObject.activeInHierarchy}");
            }
        }

        public override void OnRelease()
        {
            base.OnRelease();
            
            // Fix: Force enable colliders on release to prevent them from staying disabled
            Collider[] cols = GetComponentsInChildren<Collider>(true); // Include inactive just in case
            foreach(var c in cols)
            {
                if(c != null && !c.enabled)
                {
                    c.enabled = true;
                    Debug.Log($"[CablePlug] Fixed Collider: Re-enabling {c.name}");
                }
                
                // Also ensure it's not a trigger (unless it's supposed to be? Usually main collider is not trigger)
                // If the plug relies on physics, it must not be a trigger.
                // But check if it was originally a trigger? 
                // For now, let's assume standard Grabbable behavior: Colliders should be enabled.
            }
        }

       

        private void OnTriggerEnter(Collider other)
        {
            SnapZone zone = other.GetComponent<SnapZone>();
            if(zone != null && zone.isAnchor)
            {
                Debug.Log($"[CablePlug] Entered Anchor Zone: {zone.name}");
                hoveredZone = zone;
            }
        }

        private void OnTriggerExit(Collider other)
        {
             SnapZone zone = other.GetComponent<SnapZone>();
             if(zone != null && zone == hoveredZone)
             {
                 hoveredZone = null;
             }
        }

        private void OnTriggerStay(Collider other)
        {
            RemoteGrabber remoteGrabber = other.GetComponent<RemoteGrabber>();
            if (remoteGrabber != null && remoteGrabber.ParentGrabber != null)
            {
                Grabber g = remoteGrabber.ParentGrabber.GetComponent<Grabber>();
                if (g != null)
                {
                    if (InputBridge.Instance.GetGrabbedControllerBinding(GrabbedControllerBinding.TriggerDown, g.HandSide))
                    {
                        PlayerInv.Instance.CheckPlug(this);
                        ShowUI();
                    }
                }
            }
        }
        public void UIClose()
        {
            print("Call");
            uiActive = false;
            OutlineManager.Instance.ActiveOL(parentCable, uiActive);
            canvas.SetActive(uiActive);
        }
        public void ShowUI()
        {
            if (Time.time - lastToggleTime < 0.5f) return;
            lastToggleTime = Time.time;
            uiActive = !uiActive;
            OutlineManager.Instance.ActiveOL(parentCable, uiActive);
            CableDataUI.Instance.UpdateUI(this);
            canvas.transform.position = transform.position;
            canvas.SetActive(uiActive);
            Camera cam = Camera.main;
            canvas.transform.LookAt(
            canvas.transform.position + cam.transform.rotation * Vector3.forward,
            cam.transform.rotation * Vector3.up
        );
        }
        private void Update()
        {
            if(Time.frameCount % 60 == 0)
            {
                 Debug.Log($"[CablePlug] Update Tick - Grab: {(grab != null ? "Valid" : "Null")}, Held: {(grab != null ? grab.BeingHeld.ToString() : "N/A")}, Hovered: {(hoveredZone != null ? hoveredZone.name : "null")}");
            }

            // Check for input if being held
            if (grab != null && grab.BeingHeld)
            {
                InputBridge bridge = InputBridge.Instance;
                ControllerHand hand = ControllerHand.None;

                if(grab.HeldByGrabbers != null && grab.HeldByGrabbers.Count > 0)
                {
                     hand = grab.HeldByGrabbers[0].HandSide;
                }

                // 1. Add Point Logic (Must hover valid zone)
                if (hand != ControllerHand.None && bridge.GetGrabbedControllerBinding(GrabbedControllerBinding.TriggerDown, hand))
                {
                    ShowUI();
                }

                // Debug persistent hover state
                if(Time.frameCount % 60 == 0)
                {
                     Debug.Log($"[CablePlug] Status Update - Held: {grab.BeingHeld}, HoveredZone: {(hoveredZone != null ? hoveredZone.name : "null")}, Hand: {hand}");
                }

                if (hoveredZone != null)
                {
                    // Debug Raw Input
                    if(Time.frameCount % 60 == 0)
                    {
                        Debug.Log($"[CablePlug] Input Status - Hand: {hand}, L_Trig: {bridge.LeftTrigger}, R_Trig: {bridge.RightTrigger}");
                    }
                    
                    // ... existing input check ...

                    // Debug Input
                    if (hand == ControllerHand.None)
                    {
                         // Rate limit log
                         if(Time.frameCount % 60 == 0) Debug.LogWarning($"[CablePlug] Hovering {hoveredZone.name} but Hand is None! Held: {grab.BeingHeld}");
                    }
                    else
                    {
                        bool triggerDown = bridge.GetGrabbedControllerBinding(GrabbedControllerBinding.TriggerDown, hand);
                        if(triggerDown)
                        {
                            Debug.Log($"[CablePlug] Trigger DOWN detected on {hand}");
                             if (parentCable != null)
                            {
                                parentCable.AddIntermediatePoint(this, hoveredZone.transform);
                            }
                            else
                            {
                                Debug.LogError("[CablePlug] ParentCable is NULL!");
                            }
                        }
                    }
                }

                if(bridge.LeftThumbNear)
                {
                     if (parentCable != null)
                     {
                         parentCable.RemoveIntermediatePoint(this);
                     }
                }

                if (bridge.BButtonDown)
                {
                    if (parentCable != null)
                    {
                        Debug.Log("[CablePlug] B Button Pressed -> Destroying Cable");
                        MeshRenderer mesh = parentCable.gameObject.GetComponent<MeshRenderer>();
                        if (mesh != null)
                        {
                            mesh.enabled = false;
                        }
                    }
                }
            }
        }
        
        
        private void OnDrawGizmosSelected()
        {
             if (Start1 != null && Start2 != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(Start1.position, Start2.position);
                Gizmos.DrawSphere(Start1.position, 0.01f);
                Gizmos.DrawSphere(Start2.position, 0.01f);
            }
        }
    }
}
