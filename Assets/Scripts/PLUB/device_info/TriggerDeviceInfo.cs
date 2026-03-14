using UnityEngine;

public class TriggerDeviceInfo : MonoBehaviour
{
    [SerializeField] private GameObject deviceInfoPrefab;

    public void ToggleUI()
    {
        deviceInfoPrefab.SetActive(!deviceInfoPrefab.activeSelf);
    }
}