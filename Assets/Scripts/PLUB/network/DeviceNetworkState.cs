using System.Collections.Generic;

[System.Serializable]
public class DeviceNetworkState
{
    public string guid;
    public string device_type;
    public List<PortState> ports = new List<PortState>();

    // 🔥 เพิ่มตรงนี้
    public HashSet<string> flowsPassed = new HashSet<string>();

    public DeviceNetworkState(string id, string type)
    {
        guid = id;
        device_type = type;
    }


    public void RegisterFlow(string flowGuid)
    {
        flowsPassed.Add(flowGuid);
    }
}
