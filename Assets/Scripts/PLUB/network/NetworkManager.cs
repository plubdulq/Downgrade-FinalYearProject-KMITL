using System.Collections.Generic;
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

    public void RegisterConnection(string guid, string portName) //portName คือ portNumber ที่เป็น string เพราะ test อยู่ จึงใช้ portName เป็น portNumber
    {
        Debug.Log("Registering connection");
        DeviceNetworkState device = devices[guid];

        foreach (var port in device.ports)
        {
            if (port.portNumber.ToString() == portName)
            {
                port.portType = "CONNECTED"; // ใส่ข้อมูลจริงทีหลัง
                Debug.Log($"Port {port.portNumber} on device {guid} is now connected");
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

            Debug.Log($"IP: {state.ip}");

            foreach (var port in state.ports)
            {
                if (port.connection == null)
                {
                    Debug.Log($"Port {port.portNumber} ({port.portType}) -> EMPTY");
                }
                else
                {
                    Debug.Log($"Port {port.portNumber} ({port.portType}) -> {port.connection.targetGuid}:{port.connection.targetPort}");
                }
            }
        }
    }
}