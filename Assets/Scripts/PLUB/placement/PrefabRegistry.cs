using System.Collections.Generic;
using UnityEngine;

public class PrefabRegistry : MonoBehaviour
{
    public static PrefabRegistry Instance;

    [System.Serializable]
    public class PrefabEntry
    {
        public string deviceId;
        public GameObject prefab;
    }

    public List<PrefabEntry> prefabs;

    private Dictionary<string, GameObject> prefabMap;

    void Awake()
    {
        Instance = this;
        prefabMap = new Dictionary<string, GameObject>();

        foreach (var entry in prefabs)
        {
            prefabMap[entry.deviceId] = entry.prefab;
        }
    }

    public GameObject GetPrefab(string deviceId)
    {
        if (prefabMap.TryGetValue(deviceId, out var prefab))
            return prefab;

        Debug.LogError($"Prefab not found for deviceId: {deviceId}");
        return null;
    }
}
