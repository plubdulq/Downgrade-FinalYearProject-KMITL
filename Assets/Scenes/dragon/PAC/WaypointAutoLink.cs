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
        void Awake()
        {
            if (current == null)
                current = GetComponent<Waypoint>();
        }

        void Start()
        {
            Invoke(nameof(FindAndLink), 0.2f);
        }
        public void Relink()
        {
            FindAndLink();
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
 
        // 🔥 ใช้ภายในตัวเอง (เดิมของคุณ)
        void FindAndLink()
        {
            Waypoint best = FindBest();

            if (best != null)
            {
                useDestroy=false;
                if (best.previous != null && best.previous != current)
                {
                    Debug.LogWarning($"⚠️ {best.name} already has previous");
                    return;
                }
                current.next = best;
                
                Debug.Log($"✅ {name} Linked to {best.name}");
                best.previous = current;

            }
            else
            {
                useDestroy=true;
                Debug.LogWarning($"⚠️ {name} Not Found Target");
            }
                if (useDestroy==true)
                desB.GetComponent<destroyBall>().enabled = true;
                
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
        /*
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, searchRadius);
        }*/
   /*
        // 🔥 ใช้กับ FlowPointTrigger
        public Waypoint FindBest()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, waypointLayer);

            Waypoint best = null;
            float bestDistance = Mathf.Infinity;

            foreach (var hit in hits)
            {
                Waypoint wp = hit.GetComponent<Waypoint>();
                if (wp == null || wp == current)
                    continue;

                Vector3 dir = (wp.transform.position - transform.position).normalized;
                float dot = Vector3.Dot(transform.right, dir);
                float dist = Vector3.Distance(transform.position, wp.transform.position);

                if (side == Side.Left && dot > 0)
                {
                    if (dist < bestDistance)
                    {
                        best = wp;
                        bestDistance = dist;
                    }
                }
                else if (side == Side.Right && dot < 0)
                {
                    if (dist < bestDistance)
                    {
                        best = wp;
                        bestDistance = dist;
                    }
                }
            }

            return best;
        }
*/