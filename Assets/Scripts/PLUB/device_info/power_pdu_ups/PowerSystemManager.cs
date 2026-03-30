using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Remoting;

public class PowerSystemManager : MonoBehaviour
{
    public static PowerSystemManager Instance;

    public Dictionary<string, DeviceNetworkState> deviceDict;
    public Dictionary<string, CableDataModel> cableDict;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (NetworkManager.Instance == null)
        {
            Debug.LogError("NetworkManager not found!");
            return;
        }

        deviceDict = NetworkManager.Instance.devices;
        cableDict = CableManager.Instance.cableDict;
    }

    public void RecalculatePower()
    {
        foreach (var device in deviceDict.Values)
        {
            Debug.Log($"[RecalculatePower] deviceDict count = {deviceDict.Count}, cableDict count = {cableDict.Count}");
            bool hasPower = CheckIfDeviceHasPower(device.guid);

            GameObject obj = DeviceMapManager.Instance.GetDevice(device.guid);

            if (obj != null)
            {
                var heat = obj.GetComponent<DeviceHeat>();
                if (heat != null)
                {
                    heat.onPlug = hasPower ? 1 : 0;
                }
            }
        }
    }

    bool CheckIfDeviceHasPower(string deviceGuid)
    {
        var visited = new HashSet<string>();

        return DFS(deviceGuid, visited);
    }

    bool DFS(string currentGuid, HashSet<string> visited)
    {
        if (visited.Contains(currentGuid)) return false;
        visited.Add(currentGuid);

        var device = deviceDict[currentGuid];

        // 🔥 ถ้าเป็น UPS หรือ Power Source → มีไฟ
        if (device.device_type == "ups")
        {
            return true;
        }

        foreach (var port in device.ports)
        {
            if (string.IsNullOrEmpty(port.cable_guid)) continue;

            var cable = cableDict[port.cable_guid];

            string nextGuid = null;

            if (cable.connectedDeviceGuid1 == currentGuid)
                nextGuid = cable.connectedDeviceGuid2;
            else
                nextGuid = cable.connectedDeviceGuid1;

            if (!string.IsNullOrEmpty(nextGuid))
            {
                if (DFS(nextGuid, visited))
                    return true;
            }
        }

        return false;
    }


}