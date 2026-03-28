using UnityEngine;
using WaypointSystem;
using BNG;

namespace WaypointSystem
{
    public class WaypointSlot : MonoBehaviour
    {
        public bool hasDevice = false;
        public Waypoint waypoint;

        [Header("Auto Bind")]
        public bool autoBindWaypoint = true;
        public bool debugLogs = false;

        void Awake()
        {
            TryAutoBindWaypoint();
        }

        void OnValidate()
        {
            TryAutoBindWaypoint();
        }

        public void TryAutoBindWaypoint()
        {
            if (!autoBindWaypoint) return;

            if (waypoint == null)
            {
                waypoint = GetComponent<Waypoint>();

                if (waypoint == null)
                    waypoint = GetComponentInChildren<Waypoint>(true);

                if (waypoint == null)
                    waypoint = GetComponentInParent<Waypoint>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Device"))
            {
                hasDevice = true;

                if (debugLogs)
                    Debug.Log($"[WaypointSlot] {name} occupied by {other.name}");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Device"))
            {
                hasDevice = false;

                if (debugLogs)
                    Debug.Log($"[WaypointSlot] {name} released by {other.name}");
            }
        }
    }
}