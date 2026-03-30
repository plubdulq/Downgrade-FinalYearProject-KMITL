using System;
using System.Collections.Generic;

[Serializable]
public class DeviceRoutingTable
{
    public string deviceGuid;

    // 🔥 key = networkCIDR
    public List<RouteEntry> routes = new List<RouteEntry>();

    public DeviceRoutingTable(string guid)
    {
        deviceGuid = guid;
    }

}