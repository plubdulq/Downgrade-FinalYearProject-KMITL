using System.Collections.Generic;
using UnityEngine;

public class CableManager : MonoBehaviour
{
    public static CableManager Instance;

    public Dictionary<string, CableDataModel> cableDict = new Dictionary<string, CableDataModel>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RegisterCable(string guid, string head, string deviceGuid)
    {
        if (!cableDict.ContainsKey(guid))
        {
            cableDict[guid] = new CableDataModel();
            cableDict[guid].guid = guid;
        }

        CableDataModel data = cableDict[guid];
        Debug.Log($"Registering cable {guid} head {head} with device {deviceGuid}");

        if (head == "1")
        {
            data.connectedDeviceGuid1 = deviceGuid;
        }
        else if (head == "2")
        {
            data.connectedDeviceGuid2 = deviceGuid;
        }
        DebugCableInfo();
    }

    public void DebugCableInfo()
    {
        foreach (var cable in cableDict.Values)
        {
            Debug.Log($"Cable {cable.guid}: Device1={cable.connectedDeviceGuid1}, Device2={cable.connectedDeviceGuid2}");
        }
    }
    
    public string GetOtherDeviceGuid(string cableGuid, string myGuid)
    {
        if (string.IsNullOrEmpty(cableGuid) || string.IsNullOrEmpty(myGuid))
            return null;

        if (!cableDict.TryGetValue(cableGuid, out CableDataModel cable))
            return null;

        if (string.IsNullOrEmpty(cable.connectedDeviceGuid1) ||
            string.IsNullOrEmpty(cable.connectedDeviceGuid2))
            return null;

        if (cable.connectedDeviceGuid1 == myGuid)
            return cable.connectedDeviceGuid2;

        if (cable.connectedDeviceGuid2 == myGuid)
            return cable.connectedDeviceGuid1;

        return null;
    }

    // public List<string> GetConnectedCablesGuid(string deviceGuid)
    // {
    //     List<string> result = new List<string>();

    //     var device = NetworkManager.Instance.GetDeviceByGuid(deviceGuid);
    //     if (device == null)
    //         return result;

    //     foreach (var port in device.ports)
    //     {
    //         Debug.Log($"Checking port {port.portNumber} on device {deviceGuid} with cable {port.cable_guid}");
    //         if (!string.IsNullOrEmpty(port.cable_guid))
    //         {
    //             Debug.Log($"Device {deviceGuid} has cable {port.cable_guid} on port {port.portNumber}");
    //             var otherDeviceGuid = GetOtherDeviceGuid(port.cable_guid, deviceGuid);
    //             if (!string.IsNullOrEmpty(otherDeviceGuid))
    //             {
    //                 result.Add(otherDeviceGuid);
    //             }
    //         }
    //     }
    //     // foreach (var cable in cableDict.Values)
    //     // {
    //     //     if (cable.connectedDeviceGuid1 == deviceGuid)
    //     //     {
    //     //         result.Add(cable.connectedDeviceGuid2);
    //     //     }
    //     //     else if (cable.connectedDeviceGuid2 == deviceGuid)
    //     //     {
    //     //         result.Add(cable.connectedDeviceGuid1);
    //     //     }
    //     // }

    //     return result;
    // }

    public List<string> GetConnectedCablesGuid(string deviceGuid)
    {
        List<string> result = new List<string>();

        var device = NetworkManager.Instance.GetDeviceByGuid(deviceGuid);
        if (device == null)
            return result;

        foreach (var port in device.ports)
        {
            if (!string.IsNullOrEmpty(port.cable_guid))
            {
                result.Add(port.cable_guid); // ✅ เอา cableGuid จริง
            }
        }

        return result;
    }

    public List<string> GetConnectedDevicesGuid(string deviceGuid)
    {
        var cableGuids = GetConnectedCablesGuid(deviceGuid);
        List<string> result = new List<string>();
        foreach (var cableGuid in cableGuids)
        {
            var otherDeviceGuid = GetOtherDeviceGuid(cableGuid, deviceGuid);
            if (!string.IsNullOrEmpty(otherDeviceGuid))
            {
                result.Add(otherDeviceGuid);
            }
        }
        return result;
    }

}