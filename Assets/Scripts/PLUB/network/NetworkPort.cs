using UnityEngine;

public class NetworkPort : MonoBehaviour
{
    public string portName;
    public NetworkPort connectedPort;

    public string PortIdentify()
    {
        // หา device จาก parent
        EquipmentData device = GetComponentInParent<EquipmentData>();

        if (device == null)
        {
            Debug.Log("Device not found");
            return null;
        }

        string guid = device.uniqueID;
        Debug.Log($"Port {portName} belongs to device {guid}");
        return guid;
    }

    public void Connect()//NetworkPort otherPort
    {
        //connectedPort = otherPort;
        NetworkManager.Instance.RegisterConnection(
        PortIdentify(),
        portName
        );
        NetworkManager.Instance.DebugDevices();
    }

    public void Disconnect()
    {
        if (connectedPort == null)
            return;

        //NetworkManager.Instance.RemoveConnection(this, connectedPort); 

        connectedPort = null;
    }
}