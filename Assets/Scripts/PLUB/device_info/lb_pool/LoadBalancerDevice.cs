using UnityEngine;

public class LoadBalancerDevice : MonoBehaviour
{
    public string deviceGuid;

    void Start()
    {
        // register config ตอนเริ่ม
        EquipmentData device = GetComponent<EquipmentData>();
        deviceGuid = device.uniqueID;
        //deviceGuid = GetComponent<EquipmentData>().uniqueID;
        LoadBalancerManager.Instance.GetOrCreateConfig(deviceGuid);
    }

    // 🔥 UI เรียก
    public void CreatePool(string poolName)
    {
        LoadBalancerManager.Instance.AddPool(deviceGuid, poolName, LoadBalancingAlgorithm.RoundRobin);
    }

    public void AddServer(string poolName, string serverGuid)
    {
        LoadBalancerManager.Instance.AddServerToPool(deviceGuid, poolName, serverGuid);
    }

    public void SetAlgo(string poolName, LoadBalancingAlgorithm algo)
    {
        LoadBalancerManager.Instance.SetAlgorithm(deviceGuid, poolName, algo);
    }

    // 🔥 ใช้ตอน simulate
    // public string GetServer(string poolName)
    // {
    //     return LoadBalancerManager.Instance.GetNextServer(deviceGuid, poolName);
    // }
}