using System.Runtime.InteropServices;
using UnityEngine;

public class CablePlugHead : MonoBehaviour
{
    public string uniqueID;
    [SerializeField] private string connectedDeviceGuid;
    [SerializeField] private string headNumber;

    void Start()
    {
        // หา guid จาก parent cable
        CableInfo device = GetComponentInParent<CableInfo>();
        uniqueID = device.uniqueID;
    }

    public void GetDeviceGuid()
    {
        EquipmentData device = GetComponentInParent<EquipmentData>();
        connectedDeviceGuid = device.uniqueID;
    }

    [ContextMenu("RegisterCable")]
    public void RegisterCable()
    {
        GetDeviceGuid();
        CableManager.Instance.RegisterCable(
            uniqueID,
            headNumber,
            connectedDeviceGuid
        );
    }
}