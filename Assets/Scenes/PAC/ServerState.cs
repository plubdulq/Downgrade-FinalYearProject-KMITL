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

    // =========================
    // 🔁 SET SIDE (จาก Detector)
    // =========================
    public void SetSide(ServerSide newSide)
    {
        if (side == newSide) return;

        side = newSide;
        Debug.Log($"{name} → {side}");

        ApplyToWaypoints();
    }

    void ApplyToWaypoints()
    {
        foreach (var wp in targetWaypoints)
        {
            if (wp == null) continue;

            if (side == ServerSide.Left)
                wp.side = WaypointAutoLink.Side.Left;
            else if (side == ServerSide.Right)
                wp.side = WaypointAutoLink.Side.Right;
            else
                continue;

            //wp.Relink();
        }
    }

    public void ResetSide()
    {
        side = ServerSide.None;
    }

    // =========================
    // 🎨 Gizmos (Debug)
    // =========================
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

    // =========================
    // 🔥 PANEL SYSTEM
    // =========================

    // 🔒 ปิดฝา
    public void ClosePanel()
    {
        isPanelClosed = true;
        NotifyPAC();
    }

    // 🔓 เปิดฝา
    public void OpenPanel()
    {
        isPanelClosed = false;
        NotifyPAC();
    }

    // 🔁 Toggle
    public void TogglePanel()
    {
        isPanelClosed = !isPanelClosed;
        NotifyPAC();
    }

    // =========================
    // 🔥 DEVICE SYSTEM
    // =========================

    // ➕ ติดตั้ง
    public void InstallDevice()
    {
        hasDevice = true;
        NotifyPAC();
    }

    // ➖ ถอดออก
    public void RemoveDevice()
    {
        hasDevice = false;
        NotifyPAC();
    }

    // =========================
    // 📡 แจ้ง PAC (สำคัญมาก)
    // =========================
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

        // Debug สถานะ
        Debug.Log($"[{name}] Device:{hasDevice} | PanelClosed:{isPanelClosed}");
    }
}