using System.Collections.Generic;
using System.Linq.Expressions;
//using System.Reflection.PortableExecutable;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;

    public Dictionary<string, DeviceNetworkState> devices = new Dictionary<string, DeviceNetworkState>();

    //public List<CableConnectionData> connections = new List<CableConnectionData>();
    

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RegisterDevice(DeviceNetworkState state)
    {
        Debug.Log($"Registering device {state.guid} with {state.ports.Count} ports");
        Debug.Log($"devices count: {devices.Count}");
        devices.Add(state.guid, state);
    }

    public void RegisterConnection(string guid, string portName, string cableGuid) //portName คือ portNumber ที่เป็น string เพราะ test อยู่ จึงใช้ portName เป็น portNumber
    {
        Debug.Log("Registering connection");
        DeviceNetworkState device = devices[guid];

        foreach (var port in device.ports)
        {
            if (port.portNumber.ToString() == portName)
            {
                port.cable_guid = cableGuid;
                Debug.Log($"Port {port.portNumber} on device {guid} is now connected to cable {cableGuid}");
                return;
            }
        }

        Debug.LogError($"Port {portName} not found on device {guid}");
    }

    public void DebugDevices()
    {
        foreach (var device in devices)
        {
            Debug.Log($"Device GUID: {device.Key}");

            DeviceNetworkState state = device.Value;

            foreach (var port in state.ports)
            {
                Debug.Log($"Port {port.portNumber}: Type={port.portType}, Speed={port.speed}");
            }
        }
    }

    public DeviceNetworkState GetDeviceByGuid(string guid)
    {
        if (string.IsNullOrEmpty(guid))
            return null;

        if (devices.TryGetValue(guid, out DeviceNetworkState device))
            return device;

        return null;
    }

    public PortState GetOtherDevicePort(DeviceNetworkState device, int portIndex)
    {
        if (device == null || portIndex < 0 || portIndex >= device.ports.Count)
            return null;

        PortState myPort = device.ports[portIndex];
        Debug.Log($"My port: {myPort.portNumber}, cable_guid: {myPort.cable_guid}");
        if (string.IsNullOrEmpty(myPort.cable_guid))
            {
                Debug.Log("No cable connected to this port");
                return null;
            }
                
        string otherGuid = CableManager.Instance.GetOtherDeviceGuid(myPort.cable_guid, device.guid);
        Debug.Log($"Other device GUID: {otherGuid}");

        if (string.IsNullOrEmpty(otherGuid))
            return null;
        
        Debug.Log($"Found other device: {otherGuid}");

        if (!devices.TryGetValue(otherGuid, out DeviceNetworkState otherDevice))
            return null;

        // 🔥 หา port ฝั่งนั้น
        foreach (var port in otherDevice.ports)
        {
            if (port.cable_guid == myPort.cable_guid)
            {
                return port; // ✅ return port ตรง ๆ
            }
        }
        return null;
    }

    // 🔍 GET IP
    public string GetPortIP(string deviceGuid, int portIndex)
    {
        if (!devices.TryGetValue(deviceGuid, out DeviceNetworkState device))
        {
            Debug.LogWarning($"Device {deviceGuid} not found");
            return null;
        }

        if (device.ports == null || portIndex < 0 || portIndex >= device.ports.Count)
        {
            Debug.LogWarning($"Invalid port index {portIndex}");
            return null;
        }

        return device.ports[portIndex].my_ip;
    }

    // ✏️ UPDATE IP
    public void UpdatePortIP(string deviceGuid, int portIndex, string newIP)
    {
        if (!devices.TryGetValue(deviceGuid, out DeviceNetworkState device))
        {
            Debug.LogWarning($"Device {deviceGuid} not found");
            return;
        }

        if (device.ports == null || portIndex < 0 || portIndex >= device.ports.Count)
        {
            Debug.LogWarning($"Invalid port index {portIndex}");
            return;
        }

        device.ports[portIndex].my_ip = newIP;

        Debug.Log($"Updated {deviceGuid} port {portIndex} → {newIP}");

        // 🔥 refresh UI
        //RefreshAllPortUI();
    }

    // void RefreshAllPortUI()
    // {
    //     var all = FindObjectsOfType<PortCardUI>(true);
    //     foreach (var p in all)
    //     {
    //         p.Refresh();
    //     }
    // }


}