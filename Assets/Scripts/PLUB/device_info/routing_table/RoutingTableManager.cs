using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoutingTableManager : MonoBehaviour
{
    public static RoutingTableManager Instance;

    public Dictionary<string, DeviceRoutingTable> deviceTables = new();

    void Awake()
    {
        Instance = this;
    }

    // =============================
    // 🟢 INIT
    // =============================
    public void InitDevice(string deviceGuid)
    {
        if (!deviceTables.ContainsKey(deviceGuid))
        {
            deviceTables.Add(deviceGuid, new DeviceRoutingTable(deviceGuid));
        }
    }

    // =============================
    // 🟢 CONNECTED ROUTE (AUTO)
    // =============================
    public void RegisterConnectedRoute(string deviceGuid, int portIndex, string cidr)
    {
        InitDevice(deviceGuid);

        var table = deviceTables[deviceGuid];
        string newNetwork = NetworkUtils.NormalizeToCIDR(cidr);

        // ❗ ป้องกัน subnet ซ้ำข้าม port
        foreach (var r in table.routes)
        {
            if (r.type == RouteType.Connected &&
                r.networkCIDR == newNetwork &&
                r.portIndex != portIndex)
            {
                Debug.LogError($"[ROUTE] Subnet overlap: {newNetwork}");
                return;
            }
        }

        // 🔥 ลบ connected route เก่าของ port นี้
        table.routes.RemoveAll(r =>
            r.type == RouteType.Connected &&
            r.portIndex == portIndex
        );

        // 🔥 เพิ่มใหม่
        table.routes.Add(new RouteEntry(
            newNetwork,
            null,
            portIndex,
            RouteType.Connected
        ));

        Debug.Log($"[ROUTE][C] {deviceGuid} port {portIndex} → {newNetwork}");
    }

    // =============================
    // 🔴 STATIC ROUTE (USER)
    // =============================
    public void AddStaticRoute(string deviceGuid, string networkCIDR, string nextHopIP, int portIndex)
    {
        InitDevice(deviceGuid);

        var table = deviceTables[deviceGuid];

        table.routes.Add(new RouteEntry(
            networkCIDR,
            nextHopIP,
            portIndex,
            RouteType.Static
        ));

        Debug.Log($"[ROUTE][S] {deviceGuid} {networkCIDR} via {nextHopIP} port {portIndex}");
    }

    public void RemoveStaticRoute(string deviceGuid, string networkCIDR, string nextHopIP)
    {
        if (!deviceTables.ContainsKey(deviceGuid)) return;

        var table = deviceTables[deviceGuid];

        table.routes.RemoveAll(r =>
            r.type == RouteType.Static &&
            r.networkCIDR == networkCIDR &&
            r.nextHopIP == nextHopIP
        );
    }

    // =============================
    // 🔍 FIND ROUTE (Longest Prefix Match)
    // =============================
    public RouteEntry FindBestRoute(string deviceGuid, string destIP)
    {
        if (!deviceTables.ContainsKey(deviceGuid))
            return null;

        var table = deviceTables[deviceGuid];

        RouteEntry best = null;
        int bestPrefix = -1;

        Debug.Log($"[ROUTE] Finding best route for {destIP} on device {deviceGuid} with {table.routes.Count} routes"); 

        foreach (var route in table.routes)
        {
            Debug.Log($"[ROUTE] Checking route {route.networkCIDR} for destination {destIP} on device {deviceGuid}");
            if (NetworkUtils.IsIPInSubnet(destIP, route.networkCIDR))
            {
                Debug.Log($"[ROUTE] route = [{route.networkCIDR}]");
                int prefix = GetPrefix(route.networkCIDR);

                if (prefix > bestPrefix)
                {
                    Debug.Log("Inside FindBestRoute: " + route.networkCIDR + " matches " + destIP + " with prefix " + prefix);
                    bestPrefix = prefix;
                    best = route;
                }
            }
        }
        Debug.Log(best == null
            ? $"[ROUTE] No route found for {destIP} on {deviceGuid}"
            : $"[ROUTE] Best route for {destIP} on {deviceGuid} is {best.networkCIDR} via port {best.portIndex}");
        return best;
    }

    private int GetPrefix(string cidr)
    {
        var parts = cidr.Split('/');
        if (parts.Length != 2) return 0;

        int.TryParse(parts[1], out int prefix);
        return prefix;
    }

    // =============================
    // 📦 GET ROUTES (for UI)
    // =============================
    public List<RouteEntry> GetRoutes(string deviceGuid)
    {
        if (!deviceTables.ContainsKey(deviceGuid))
            return new List<RouteEntry>();

        return deviceTables[deviceGuid].routes;
    }

    // =============================
    // 📊 GET ROUTES SORTED (for UI)
    // =============================
    public List<RouteEntry> GetRoutesSorted(string deviceGuid)
    {
        if (!deviceTables.ContainsKey(deviceGuid))
            return new List<RouteEntry>();

        return deviceTables[deviceGuid].routes
            .OrderBy(r => r.portIndex)
            .ThenByDescending(r => GetPrefix(r.networkCIDR))
            .ToList();
    }
}