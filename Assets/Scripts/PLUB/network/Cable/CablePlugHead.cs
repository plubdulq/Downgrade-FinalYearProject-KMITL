using System.Runtime.InteropServices;
using UnityEngine;

public class CablePlugHead : MonoBehaviour
{
    public string uniqueID;
    [SerializeField] private string connectedDeviceGuid;
    [SerializeField] private string headNumber;

    public void Start()
    {
        CableInfo device = GetComponentInParent<CableInfo>();
        if (device != null)
        {
            // หา guid จาก parent cable
            if (string.IsNullOrEmpty(uniqueID) && !NetworkManager.Instance.isLoading)
            {
                uniqueID = device.uniqueID;
                return;
            }
        }
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