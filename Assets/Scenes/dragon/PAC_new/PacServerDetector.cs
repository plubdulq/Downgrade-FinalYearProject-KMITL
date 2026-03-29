using System.Collections.Generic;
using UnityEngine;
using WaypointSystem;
using BNG;

public class PacServerDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    public float rayDistance = 10f;
    public LayerMask serverLayer;

    [Header("Debug")]
    public bool showDebug = true;

    public List<ServerState> detectedServers = new List<ServerState>();

    void Start()
    {
        Invoke(nameof(CheckServers), 0.2f);
    }

    public void CheckServers()
    {
        detectedServers.Clear();

        DetectSide(-transform.right, ServerSide.Left);
        DetectSide(transform.right, ServerSide.Right);

        // 🔥 ส่งเข้า Temp Simulation
        if (ServerRoomTempSimulation.Instance != null)
        {
            ServerRoomTempSimulation.Instance.SetServers(detectedServers);
        }
    }

    void DetectSide(Vector3 dir, ServerSide side)
    {
        Ray ray = new Ray(transform.position, dir);

        // 🔥 ยิงทะลุทั้งหมด
        RaycastHit[] hits = Physics.RaycastAll(ray, rayDistance, serverLayer);

        // 🔥 เรียงจากใกล้ → ไกล
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            if (showDebug)
                Debug.Log($"Hit: {hit.collider.name} | Dist: {hit.distance}");

            // 🔥 สำคัญมาก: เผื่อ collider อยู่ child
            ServerState server = hit.collider.GetComponentInParent<ServerState>();

            if (server != null)
            {
                if (!detectedServers.Contains(server))
                {
                    server.SetSide(side);
                    detectedServers.Add(server);

                    if (showDebug)
                        Debug.Log($"✅ ADD Server: {server.name} | Side: {side}");
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // ซ้าย
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (-transform.right * rayDistance));

        // ขวา
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + (transform.right * rayDistance));
    }
}