using UnityEngine;
using System.Collections.Generic;

public class DeviceMapManager : MonoBehaviour
{
    public static DeviceMapManager Instance;

    private Dictionary<string, GameObject> deviceMap = new();

    void Awake()
    {
        Instance = this;
    }

    public void Register(string guid, GameObject obj)
    {
        deviceMap[guid] = obj;
    }

    public GameObject GetDevice(string guid)
    {
        deviceMap.TryGetValue(guid, out var obj);
        return obj;
    }
}