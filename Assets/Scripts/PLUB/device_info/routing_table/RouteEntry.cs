using System;

[Serializable]
public class RouteEntry
{
    public string networkCIDR;   // เช่น 192.168.1.0/24
    public string nextHopIP;     // null = connected
    public int portIndex;
    public RouteType type;

    public RouteEntry(string networkCIDR, string nextHopIP, int portIndex, RouteType type)
    {
        this.networkCIDR = networkCIDR;
        this.nextHopIP = nextHopIP;
        this.portIndex = portIndex;
        this.type = type;
    }
}

public enum RouteType
{
    Connected,
    Static
}