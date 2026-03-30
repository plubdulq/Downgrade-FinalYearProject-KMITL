using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class RoutingTableUI : MonoBehaviour
{
    public Transform container;
    public GameObject rowPrefab;

    public string currentDeviceGuid;
    private NetworkDevice networkDevice;

    // =============================
    // 🔥 เรียกตอนเปิด UI
    // =============================
    public void Refresh()
    {
        Clear();

        networkDevice = GetComponentInParent<NetworkDevice>(); // อัพเดต port info ใน label card ก่อน
        currentDeviceGuid = networkDevice.guid;

        List<RouteEntry> routes = RoutingTableManager.Instance.GetRoutesSorted(currentDeviceGuid);

        foreach (var route in routes)
        {
            GameObject row = Instantiate(rowPrefab, container);

            var ui = row.GetComponent<RoutingRowUI>();
            ui.SetData(route);
        }
    }

    void Clear()
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
}