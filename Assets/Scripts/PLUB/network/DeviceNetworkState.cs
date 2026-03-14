using System.Collections.Generic;

[System.Serializable]
public class DeviceNetworkState
{
    public string guid;
    public string ip;
    public List<PortState> ports = new List<PortState>();

    public DeviceNetworkState(string id)
    {
        guid = id;
        ip = "";
    }
}
