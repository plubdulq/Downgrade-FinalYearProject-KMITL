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
}