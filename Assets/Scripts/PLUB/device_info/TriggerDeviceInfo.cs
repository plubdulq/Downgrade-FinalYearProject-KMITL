using UnityEngine;

public class TriggerDeviceInfo : MonoBehaviour
{
    [SerializeField] private GameObject deviceInfoPrefab;
    [SerializeField] private GameObject icon;

    public void ToggleUI()
    {
        deviceInfoPrefab.SetActive(!deviceInfoPrefab.activeSelf);
        // if (icon != null)
        // {
        //     icon.SetActive(!icon.activeSelf);
        // }
    }
}