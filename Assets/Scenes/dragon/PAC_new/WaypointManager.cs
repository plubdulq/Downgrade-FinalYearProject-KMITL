using System.Collections.Generic;
using UnityEngine;

namespace WaypointSystem
{
    public class WaypointManager : MonoBehaviour
    {
        public static WaypointManager Instance;

        public Waypoint waypoint;
        public List<WaypointSlot> slots = new List<WaypointSlot>();

        [Header("Auto Bind")]
        public bool autoFindSlotsFromChildren = true;
        public bool debugLogs = true;

        void Awake()
        {
            Instance = this;
            AutoBind();
            RefreshSlots();
        }

        void OnValidate()
        {
            AutoBind();
        }

        void AutoBind()
        {
            if (autoFindSlotsFromChildren)
            {
                slots = new List<WaypointSlot>(GetComponentsInChildren<WaypointSlot>(true));
            }
        }

        public void RefreshSlots()
        {
            if (slots == null)
                slots = new List<WaypointSlot>();

            foreach (var slot in slots)
            {
                if (slot == null) continue;

                slot.hasDevice = false;

                if (slot.waypoint == null)
                    slot.TryAutoBindWaypoint();
            }

            if (debugLogs)
                Debug.Log($"🔍 {name} Slots = {slots.Count}");
        }

        public WaypointSlot GetSlotByWaypoint(Waypoint wp)
        {
            foreach (var slot in slots)
            {
                if (slot == null || wp == null) continue;

                float dist = Vector3.Distance(slot.transform.position, wp.transform.position);

                if (dist < 0.01f)
                {
                    if (debugLogs)
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
                Debug.LogWarning("[WaypointManager] device is null.");
                return;
            }

            foreach (var slot in slots)
            {
                if (slot == null) continue;

                if (debugLogs)
                    Debug.Log($"Checking Slot: {slot.name} | hasDevice = {slot.hasDevice}");

                if (!slot.hasDevice)
                {
                    slot.hasDevice = true;

                    if (wp != null)
                        slot.waypoint = wp;

                    device.transform.position = slot.transform.position;

                    if (debugLogs)
                        Debug.Log($"✅ ADD {(wp != null ? wp.name : "NULL")} -> {slot.name}");

                    return;
                }
            }

            Debug.LogWarning("❌ No empty slot!");
        }
    }
}