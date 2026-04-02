using UnityEngine;

public class NetworkPort : MonoBehaviour
{
    public string portName;
    [SerializeField] private string ipInterface;
    [SerializeField] private string labelState;

    public string cable_guid;
    

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
        Debug.Log($"Port {portName} belongs to cable_guid {cable_guid}");
        return guid;
    }

    public string GetCableGuid()
    {
        CablePlugHead cableInfo = GetComponentInChildren<CablePlugHead>();

        if (cableInfo == null)
        {
            Debug.LogError("CableInfo not found");
            AlertPopupManager.Instance.ShowPopup("Cable info not found!!!\n Or\n Devices have collision issue");
            return null;
        }
        cable_guid = cableInfo.uniqueID;
        return cable_guid;
    }

    public void TriggerRegisterCable()
    {
        CablePlugHead plugHead = GetComponentInChildren<CablePlugHead>();
        if (plugHead != null)
        {
            plugHead.RegisterCable();
        }
        else
        {
            Debug.Log("CablePlugHead not found in children");
        }
    }

    public void Connect()
    {
        if (NetworkManager.Instance.isLoading)
        {
            Debug.Log("[Connect] Skipped (loading)");
            return;
        }
        
        string my_guid = PortIdentify();
        TriggerRegisterCable();
        cable_guid = GetCableGuid();
        if (cable_guid == null)
        {
            NetworkManager.Instance.RegisterConnection(
            my_guid,
            portName,
            cable_guid
            );
        }
        NetworkManager.Instance.DebugDevices();
        PowerSystemManager.Instance.RecalculatePower();
    }

}