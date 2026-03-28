using System.Collections.Generic;
using UnityEngine;
using WaypointSystem;
using BNG;

namespace WaypointSystem
{
    public class WaypointAttach : MonoBehaviour
    {
        [Header("Detection Settings")]
        public float rayDistance = 10f;
        public LayerMask WayPointLayer;

        [Header("Root Waypoint (จุดเริ่มต้น)")]
        public Waypoint current;

        [Header("Debug")]
        public bool showDebug = true;

        public enum Side { Left, Right }
        public Side side;

        [Header("Detected Slots")]
        public List<WaypointSlot> detectedWayPoint = new List<WaypointSlot>();

        [Header("Auto Bind")]
        public bool autoBindCurrent = true;
        public bool autoFindChildSlots = true;
        public bool autoUseWaypointSlotLayerIfEmpty = true;

        void Awake()
        {
            AutoBind();
            ResetDetectedSlotsState();
        }

        void Start()
        {
            Invoke(nameof(SetupAll), 0.2f);
        }

        void OnValidate()
        {
            AutoBind();
        }

        void AutoBind()
        {
            if (autoBindCurrent && current == null)
            {
                current = GetComponent<Waypoint>();
                if (current == null)
                    current = GetComponentInChildren<Waypoint>(true);
            }

            if (autoFindChildSlots)
            {
                detectedWayPoint = new List<WaypointSlot>(GetComponentsInChildren<WaypointSlot>(true));
            }

            if (autoUseWaypointSlotLayerIfEmpty && WayPointLayer.value == 0)
            {
                int layer = LayerMask.NameToLayer("WayPointSlot");
                if (layer >= 0)
                    WayPointLayer = 1 << layer;
            }
        }

        void ResetDetectedSlotsState()
        {
            if (detectedWayPoint == null)
                detectedWayPoint = new List<WaypointSlot>();

            if (showDebug)
                Debug.Log($"🔍 {name} detectedWayPoint = {detectedWayPoint.Count}");

            foreach (var slot in detectedWayPoint)
            {
                if (slot == null) continue;

                slot.hasDevice = false;

                if (slot.waypoint == null)
                    slot.TryAutoBindWaypoint();
            }
        }

        public void Relink()
        {
            SetupAll();
        }

        void SetupAll()
        {
            CheckWayPoint();
            SortSlotsByDistance();
            LinkWaypointsChain();
        }

        public void CheckWayPoint()
        {
            detectedWayPoint.Clear();

            if (side == Side.Left)
                DetectSide(-transform.right);
            else if (side == Side.Right)
                DetectSide(transform.right);
        }

        void DetectSide(Vector3 dir)
        {
            Ray ray = new Ray(transform.position, dir);
            RaycastHit[] hits = Physics.RaycastAll(ray, rayDistance, WayPointLayer);

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var hit in hits)
            {
                WaypointSlot slot = hit.collider.GetComponent<WaypointSlot>();

                if (slot != null)
                {
                    Vector3 dirToHit = (hit.point - transform.position).normalized;
                    float dot = Vector3.Dot(transform.right, dirToHit);

                    if (side == Side.Left && dot > 0) continue;
                    if (side == Side.Right && dot < 0) continue;

                    if (!detectedWayPoint.Contains(slot))
                    {
                        if (slot.waypoint == null)
                            slot.TryAutoBindWaypoint();

                        detectedWayPoint.Add(slot);

                        if (showDebug)
                            Debug.Log($"✅ ADD Slot: {slot.name}");
                    }
                }
            }
        }

        void SortSlotsByDistance()
        {
            detectedWayPoint.Sort((a, b) =>
            {
                float distA = Vector3.Distance(transform.position, a.transform.position);
                float distB = Vector3.Distance(transform.position, b.transform.position);
                return distA.CompareTo(distB);
            });

            if (showDebug)
            {
                Debug.Log("📊 Sorted Slots:");
                foreach (var s in detectedWayPoint)
                    Debug.Log($" - {s.name}");
            }
        }

        void LinkWaypointsChain()
        {
            Debug.Log("=== LINK START ===");

            if (detectedWayPoint.Count == 0)
            {
                Debug.LogError("❌ detectedWayPoint EMPTY");
                return;
            }

            if (current == null)
            {
                Debug.LogError("❌ current ROOT is NULL");
                return;
            }

            current.next = null;

            foreach (var slot in detectedWayPoint)
            {
                if (slot == null) continue;

                if (slot.waypoint == null)
                {
                    slot.TryAutoBindWaypoint();

                    if (slot.waypoint == null)
                    {
                        Debug.LogError($"❌ {slot.name} has NO Waypoint");
                        continue;
                    }
                }

                slot.waypoint.next = null;
                slot.waypoint.previous = null;

                var db = slot.waypoint.GetComponent<destroyBall>();
                if (db != null)
                    db.enabled = false;
            }

            Waypoint prev = current;
            Waypoint last = null;

            foreach (var slot in detectedWayPoint)
            {
                if (slot?.waypoint == null) continue;

                Waypoint wp = slot.waypoint;

                prev.next = wp;
                wp.previous = prev;

                Debug.Log($"🔗 LINK {prev.name} -> {wp.name}");

                prev = wp;
                last = wp;
            }

            if (last != null)
            {
                if (last.GetComponent<destroyBall>() == null)
                {
                    last.gameObject.AddComponent<destroyBall>();
                    Debug.Log($"💥 ADD destroyBall on {last.name}");
                }
                else
                {
                    Debug.Log($"⚠️ destroyBall already exists on {last.name}");
                }
            }

            Debug.Log("=== LINK END ===");
        }

        public WaypointSlot GetSlotByWaypoint(Waypoint wp)
        {
            foreach (var slot in detectedWayPoint)
            {
                if (slot == null || wp == null) continue;

                float dist = Vector3.Distance(slot.transform.position, wp.transform.position);

                if (dist < 0.01f)
                {
                    Debug.Log($"✅ MATCH SLOT: {slot.name}");
                    return slot;
                }
            }

            Debug.LogError($"❌ SLOT NOT FOUND for {(wp != null ? wp.name : "NULL")}");
            return null;
        }

        public void AssignToEmptySlot(Waypoint wp, FlowPointTrigger device)
        {
            Debug.Log($"🎯 Assign {(wp != null ? wp.name : "NULL")} → Device {(device != null ? device.name : "NULL")}");

            if (device == null)
            {
                Debug.LogWarning("[WaypointAttach] device is null.");
                return;
            }

            foreach (var slot in detectedWayPoint)
            {
                if (slot == null) continue;

                if (!slot.hasDevice)
                {
                    slot.hasDevice = true;

                    if (wp != null)
                        slot.waypoint = wp;

                    device.transform.position = slot.transform.position;

                    Debug.Log($"✅ ADD {(wp != null ? wp.name : "NULL")} -> {slot.name}");
                    return;
                }
            }

            Debug.LogWarning("❌ No empty slot!");
        }

        void OnDrawGizmosSelected()
        {
            if (side == Side.Left)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, transform.position + (-transform.right * rayDistance));
            }
            else if (side == Side.Right)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, transform.position + (transform.right * rayDistance));
            }
        }
    }
}