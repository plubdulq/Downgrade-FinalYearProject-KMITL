using System.Collections.Generic;
using UnityEngine;

namespace WaypointSystem
{

    public class WaypointManager : MonoBehaviour
    {
        public static WaypointManager Instance;
        public Waypoint waypoint;  // 🔥 ลากใส่ใน Inspector
       // public WaypointSlot[] slots;
        public List<WaypointSlot> slots = new List<WaypointSlot>();
        void Awake()
        {
    // 🔥 หา slot ของตัวเองเท่านั้น (ในลูก)
        slots = new List<WaypointSlot>(GetComponentsInChildren<WaypointSlot>());

        Debug.Log($"🔍 {name} Slots = {slots.Count}");

        foreach (var slot in slots)
        {
            if (slot == null) continue;

            slot.hasDevice = false;
           // slot.waypoint = null;
        }
    }

      public WaypointSlot GetSlotByWaypoint(Waypoint wp)
        {
            foreach (var slot in slots)
            {
                if (slot == null) continue;

                // 🔥 ใช้ตำแหน่งแทน object
                float dist = Vector3.Distance(slot.transform.position, wp.transform.position);

                if (dist < 0.01f) // ปรับได้
                {
                    Debug.Log($"✅ MATCH SLOT: {slot.name}");
                    return slot;
                }
            }

            Debug.LogError($"❌ SLOT NOT FOUND for {wp.name}");
            return null;
        }

       public void AssignToEmptySlot(Waypoint wp, FlowPointTrigger device)
        {
            Debug.Log($"🎯 Assign {wp.name} → Device {device.name}");

            foreach (var slot in slots)
            {

                Debug.Log($"Checking Slot: {slot.name} | hasDevice = {slot.hasDevice}");

                if (!slot.hasDevice)
                {
                    slot.hasDevice = true;
                   // slot.waypoint = wp;

                    // 🔥 ย้าย device เข้า slot (สำคัญ)
                    device.transform.position = slot.transform.position;

                    Debug.Log($"✅ ADD {wp.name} -> {slot.name}");

                    return;
                }
            }

            Debug.LogWarning("❌ No empty slot!");
        }
    }
}