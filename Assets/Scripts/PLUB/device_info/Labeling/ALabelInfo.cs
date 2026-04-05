using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ALabelInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text portNameText;
    [SerializeField] private TMP_Text otherDeviceNameText;
    [SerializeField] private TMP_Text otherDeviceIPText;

    public int portIndex;
    public DeviceNetworkState device;
    private Coroutine updateRoutine;

    public void SetCardConnectionInfo()
    {
        PortState myPort = device.ports[portIndex];

        portNameText.text = $"Port {myPort.portNumber}";
        
        var otherPort = NetworkManager.Instance.GetOtherDevicePort(device, portIndex);

        if (otherPort == null)
        {
            // otherDeviceNameText.text = "No Connection";
            // otherDeviceIPText.text = "";
            return;
        }

        string otherGuid = CableManager.Instance.GetOtherDeviceGuid(myPort.cable_guid, device.guid);

        //otherDeviceNameText.text = otherGuid ?? "Unknown Device";
        Debug.Log($"otherPort_ip: {otherPort.my_ip}");
        otherDeviceIPText.text = otherPort.my_ip ?? "To: No IP";
    }

}
