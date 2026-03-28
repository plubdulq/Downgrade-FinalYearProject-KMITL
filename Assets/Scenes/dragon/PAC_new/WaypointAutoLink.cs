using System.Collections.Generic;
using UnityEngine;

namespace WaypointSystem
{
    public class WaypointAutoLink : MonoBehaviour
    {
        public Waypoint current;

        public float searchRadius = 2f;
        public LayerMask waypointLayer;

        public enum Side { Left, Right }
        public Side side;

        public destroyBall desB;
        public bool useDestroy;

        [Header("Auto Bind")]
        public bool autoBindCurrent = true;
        public bool autoBindDestroyBall = true;
        public bool autoUseWaypointSlotLayerIfEmpty = true;
        public bool debugLogs = true;

        void Awake()
        {
            AutoBind();
        }

        void Start()
        {
            Invoke(nameof(FindAndLink), 0.2f);
        }

        void OnValidate()
        {
            AutoBind();
        }

        void AutoBind()
        {
            if (autoBindCurrent && current == null)
                current = GetComponent<Waypoint>();

            if (autoBindDestroyBall && desB == null)
                desB = GetComponent<destroyBall>();

            if (autoUseWaypointSlotLayerIfEmpty && waypointLayer.value == 0)
            {
                int layer = LayerMask.NameToLayer("WayPointSlot");
                if (layer >= 0)
                    waypointLayer = 1 << layer;
            }
        }

        public void Relink()
        {
            FindAndLink();
        }

        public Waypoint FindBest()
        {
            Vector3 dir = (side == Side.Left) ? -transform.right : transform.right;

            RaycastHit[] hits = Physics.RaycastAll(transform.position, dir, searchRadius, waypointLayer);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            if (debugLogs)
                Debug.Log($"Hits = {hits.Length}");

            foreach (var hit in hits)
            {
                if (debugLogs)
                    Debug.Log($"Hit: {hit.collider.name}");

                Waypoint wp = hit.collider.GetComponentInParent<Waypoint>();

                if (wp != null && wp != current)
                {
                    if (debugLogs)
                        Debug.Log($"FOUND WAYPOINT: {wp.name}");

                    return wp;
                }
            }

            return null;
        }

        void FindAndLink()
        {
            if (current == null)
            {
                Debug.LogWarning($"[WaypointAutoLink] {name} current Waypoint is null.");
                return;
            }

            Waypoint best = FindBest();

            if (best != null)
            {
                useDestroy = false;

                if (best.previous != null && best.previous != current)
                {
                    if (debugLogs)
                        Debug.LogWarning($"⚠️ {best.name} already has previous");
                    return;
                }

                current.next = best;
                best.previous = current;

                if (desB != null)
                    desB.enabled = false;

                if (debugLogs)
                    Debug.Log($"✅ {name} Linked to {best.name}");
            }
            else
            {
                useDestroy = true;

                if (debugLogs)
                    Debug.LogWarning($"⚠️ {name} Not Found Target");

                if (desB != null)
                    desB.enabled = true;
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, searchRadius);

            Vector3 dir = (side == Side.Left) ? -transform.right : transform.right;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + dir * searchRadius);
        }
    }
}