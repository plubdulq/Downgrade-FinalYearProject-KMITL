using UnityEngine;
using WaypointSystem;

public class FlowPointTrigger : MonoBehaviour
{
    public float searchRadius = 2f;
    public LayerMask waypointLayer;
    public WaypointManager waypointManager;
    void Start()
    {
        Invoke(nameof(FindAndAssign), 0.2f);
    }

    void FindAndAssign()
    {

        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, waypointLayer);

        WaypointAutoLink nearest = null;
        float bestDistance = Mathf.Infinity;

        foreach (var hit in hits)
        {
            WaypointAutoLink link = hit.GetComponent<WaypointAutoLink>();
            if (link == null) continue;

            float dist = Vector3.Distance(transform.position, link.transform.position);

            if (dist < bestDistance)
            {
                nearest = link;
                bestDistance = dist;
            }
        }

        if (nearest != null)
        {
            Waypoint target = nearest.FindBest();

            if (target != null)
            {
               // WaypointSystem.WaypointManager.Instance.AssignToSlot(target, this);
                waypointManager.AssignToEmptySlot(target, this);

                Debug.Log($"✅ {name} Assigned to {target.name}");
            }
            else
            {
                Debug.LogWarning($"❌ {name} No target waypoint");
            }
        }
        else
        {
            Debug.LogWarning($"❌ {name} No WaypointAutoLink found");
        }
    }
}
