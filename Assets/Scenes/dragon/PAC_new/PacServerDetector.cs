using System.Collections.Generic;
using UnityEngine;
using WaypointSystem;
using BNG;

public class PacServerDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    public float rayDistance = 10f;
    public LayerMask serverLayer = 0;

    [Header("Auto Bind")]
    public bool autoDetectAllLayersIfMaskEmpty = true;

    [Header("Debug")]
    public bool showDebug = true;

    public List<ServerState> detectedServers = new List<ServerState>();

    void Start()
    {
        AutoBind();
        Invoke(nameof(CheckServers), 0.2f);
    }

    void OnValidate()
    {
        AutoBindEditorSafe();
    }

    void AutoBind()
    {
        if (serverLayer.value == 0 && autoDetectAllLayersIfMaskEmpty)
        {
            serverLayer = ~0;
        }
    }

    void AutoBindEditorSafe()
    {
        if (!Application.isPlaying && serverLayer.value == 0 && autoDetectAllLayersIfMaskEmpty)
        {
            serverLayer = ~0;
        }
    }

    public void CheckServers()
    {
        detectedServers.Clear();

        DetectSide(-transform.right, ServerSide.Left);
        DetectSide(transform.right, ServerSide.Right);

        if (ServerRoomTempSimulation.Instance != null)
        {
            ServerRoomTempSimulation.Instance.SetServers(detectedServers);
        }
        else if (showDebug)
        {
            Debug.LogWarning("[PacServerDetector] ServerRoomTempSimulation.Instance not found.");
        }
    }

    void DetectSide(Vector3 dir, ServerSide side)
    {
        Ray ray = new Ray(transform.position, dir);

        RaycastHit[] hits = Physics.RaycastAll(ray, rayDistance, serverLayer);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            if (showDebug)
                Debug.Log($"Hit: {hit.collider.name} | Dist: {hit.distance}");

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
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (-transform.right * rayDistance));

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + (transform.right * rayDistance));
    }
}