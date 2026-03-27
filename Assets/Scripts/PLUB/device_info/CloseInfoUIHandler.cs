using UnityEngine;

public class CloseInfoUIHandler : MonoBehaviour
{
    [SerializeField] private GameObject deviceInfoPrefab;

    public void Close()
    {
        deviceInfoPrefab.SetActive(false);
    }
}