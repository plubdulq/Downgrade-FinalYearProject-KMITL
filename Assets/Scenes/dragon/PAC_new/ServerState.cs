using UnityEngine;
using WaypointSystem;
using BNG;

public enum ServerSide
{
    None,
    Left,
    Right
}

public class ServerState : MonoBehaviour
{
    [SerializeField]
    private ServerSide side = ServerSide.None;

    public ServerSide Side => side;

    [Header("Waypoints ที่อยู่ด้านใน")]
    public WaypointAutoLink[] targetWaypoints;

    [Header("State")]
    public bool hasDevice = false;
    public bool isPanelClosed = false;

    [Header("Auto Bind")]
    public bool autoFindTargetWaypoints = true;
    public bool debugLogs = true;

    void Awake()
    {
        AutoBind();
    }

    void OnValidate()
    {
        AutoBind();
    }

    void AutoBind()
    {
        if (autoFindTargetWaypoints && (targetWaypoints == null || targetWaypoints.Length == 0))
        {
            targetWaypoints = GetComponentsInChildren<WaypointAutoLink>(true);
        }
    }

    public void SetSide(ServerSide newSide)
    {
        if (side == newSide) return;

        side = newSide;

        if (debugLogs)
            Debug.Log($"{name} → {side}");

        ApplyToWaypoints();
    }

    void ApplyToWaypoints()
    {
        if (targetWaypoints == null) return;

        foreach (var wp in targetWaypoints)
        {
            if (wp == null) continue;

            if (side == ServerSide.Left)
                wp.side = WaypointAutoLink.Side.Left;
            else if (side == ServerSide.Right)
                wp.side = WaypointAutoLink.Side.Right;
            else
                continue;

            wp.Relink();
        }
    }

    public void ResetSide()
    {
        side = ServerSide.None;
    }

    void OnDrawGizmos()
    {
        switch (side)
        {
            case ServerSide.Left:
                Gizmos.color = Color.red;
                break;
            case ServerSide.Right:
                Gizmos.color = Color.green;
                break;
            default:
                Gizmos.color = Color.gray;
                break;
        }

        Gizmos.DrawSphere(transform.position, 0.2f);
    }

    public void ClosePanel()
    {
        isPanelClosed = true;
        NotifyPAC();
    }

    public void OpenPanel()
    {
        isPanelClosed = false;
        NotifyPAC();
    }

    public void TogglePanel()
    {
        isPanelClosed = !isPanelClosed;
        NotifyPAC();
    }

    public void InstallDevice()
    {
        hasDevice = true;
        NotifyPAC();
    }

    public void RemoveDevice()
    {
        hasDevice = false;
        NotifyPAC();
    }

    void NotifyPAC()
    {
        if (ServerRoomTempSimulation.Instance != null)
        {
            ServerRoomTempSimulation.Instance.RecalculateFromServers();
        }
        else
        {
            Debug.LogWarning("❗ ServerRoomTempSimulation Instance NOT FOUND");
        }

        if (debugLogs)
            Debug.Log($"[{name}] Device:{hasDevice} | PanelClosed:{isPanelClosed}");
    }
}