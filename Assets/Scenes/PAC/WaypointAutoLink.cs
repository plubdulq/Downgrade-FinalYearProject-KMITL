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

        void Awake()
        {
            if (current == null)
                current = GetComponent<Waypoint>();
        }

        void Start()
        {
            Invoke(nameof(FindBest), 0.2f);
        }
   
       public Waypoint FindBest()
        {
            Vector3 dir = (side == Side.Left) ? -transform.right : transform.right;

            RaycastHit[] hits = Physics.RaycastAll(transform.position, dir, searchRadius, waypointLayer);

            Debug.Log($"Hits = {hits.Length}");

            foreach (var hit in hits)
            {
                Debug.Log($"Hit: {hit.collider.name}");

                Waypoint wp = hit.collider.GetComponentInParent<Waypoint>();

                if (wp != null && wp != current)
                {
                    Debug.Log($"FOUND WAYPOINT: {wp.name}");
                    return wp;
                }
            }

            return null;
        }
    }
}
 