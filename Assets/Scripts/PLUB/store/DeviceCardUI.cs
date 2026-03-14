using UnityEngine;
using TMPro;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.CompilerServices;

public class DeviceCardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text deviceNameText;
    [SerializeField] private TMP_Text devicePriceText;
    
    [SerializeField] private TMP_Text cableTypeText;

    public void SetDeviceInfo(DeviceData data)
    {
        deviceNameText.text = data.DeviceName + "\n" + data.Price.ToString() + " $";
        //devicePriceText.text = data.Price.ToString() + " $";
    }

    public void SetCableInfo(CableModelData data)
    {
        cableTypeText.text = data.CableType + "\n" + data.Price.ToString() + " $";
        //devicePriceText.text = data.Price.ToString() + " $";
    }
}
