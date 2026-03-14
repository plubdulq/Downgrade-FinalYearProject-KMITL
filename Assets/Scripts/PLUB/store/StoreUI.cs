using UnityEngine;

public class StoreUI : MonoBehaviour
{
    public GameObject deviceCardPrefab;
    public Transform storeParent;

    public void ClearStore()
    {
        foreach (Transform child in storeParent)
        {
            Destroy(child.gameObject);
        }
    }

    public void CreateDeviceCard(DeviceData deviceData)
    {
        GameObject deviceCard = Instantiate(deviceCardPrefab, storeParent);
        //spawn prefab button
        var deviceName = deviceCard.GetComponent<SpawnPrefab>();
        deviceName.SetUp(deviceData.DeviceName);
        //DeviceCardUI deviceCardUI = deviceCard.GetComponent<DeviceCardUI>();
        DeviceCardUI deviceCardUI = deviceCard.GetComponentInChildren<DeviceCardUI>();
        deviceCardUI.SetDeviceInfo(deviceData);
    }

    public void CreateCableCard(CableModelData cableData)
    {
        GameObject deviceCard = Instantiate(deviceCardPrefab, storeParent);
        //spawn prefab button
        var deviceName = deviceCard.GetComponent<SpawnPrefab>();
        deviceName.SetUp(cableData.CableType);
        //DeviceCardUI deviceCardUI = deviceCard.GetComponent<DeviceCardUI>();
        DeviceCardUI deviceCardUI = deviceCard.GetComponentInChildren<DeviceCardUI>();
        deviceCardUI.SetCableInfo(cableData);
    }
}
