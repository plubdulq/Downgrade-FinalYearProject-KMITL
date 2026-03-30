using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LBPoolUI : MonoBehaviour
{
    [Header("UI - create/edit new pool")]
    public Transform cardContainer;
    public GameObject cardPrefab;
    public TMP_Text poolNameText;

    [Header("UI - normal")]
    public Transform cardContainer2;
    public GameObject cardPrefab2;

    private string currentLbGuid;
    private string currentPoolName;
    private LoadBalancerDevice currentLB;

    // 🔥 TEMP STATE
    private HashSet<string> selectedServers = new HashSet<string>();
    private LoadBalancingAlgorithm selectedAlgo = LoadBalancingAlgorithm.RoundRobin;

    public void OpenUI()
    {
        var networkDevice = GetComponentInParent<NetworkDevice>();
        currentLbGuid = networkDevice.guid;

        RenderExistingPools();
    }

    public void CreateNewPool()
    {
        var networkDevice = GetComponentInParent<NetworkDevice>();
        currentLbGuid = networkDevice.guid;

        currentLB = networkDevice.GetComponent<LoadBalancerDevice>();
        Debug.Log($"Creating pool for LB {currentLbGuid}");

        selectedServers.Clear(); // reset state

        GeneratePoolName();
        Debug.Log($"Generated pool name: {currentPoolName}");
        SpawnServerCards();
    }

    void GeneratePoolName()
    {
        int count = 0;

        if (LoadBalancerManager.Instance.lbConfigs.TryGetValue(currentLbGuid, out var lbConfig))
        {
            count = lbConfig.pools.Count;
        }

        currentPoolName = $"New Pool {count + 6}";
        poolNameText.text = currentPoolName;
    }

    void SpawnServerCards()
    {
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }
        var servers = NetworkUtils.FindReachableServers(currentLbGuid);
        Debug.Log($"Found {servers.Count} servers");

        foreach (var serverGuid in servers)
        {
            var device = NetworkManager.Instance.GetDeviceByGuid(serverGuid);
            if (device == null) continue;

            GameObject card = Instantiate(cardPrefab, cardContainer);
            Debug.Log($"Spawn card for {device.guid}");

            var cardUI = card.GetComponent<ServerCardUI>();

            // 🔥 ส่ง callback เข้าไป
            cardUI.Setup(device, this);
        }
    }

    // 🔥 ถูกเรียกจาก Card
    public void ToggleServer(string guid)
    {
        if (selectedServers.Contains(guid))
        {
            selectedServers.Remove(guid);
        }
        else
        {
            selectedServers.Add(guid);
        }

        Debug.Log($"Selected: {selectedServers.Count}");
    }

    // 🔥 Apply Button เรียกตัวนี้
    public void OnClickApply()
    {
        if (selectedServers.Count == 0)
        {
            Debug.LogWarning("No server selected");
            return;
        }

        // 1. สร้าง pool
        currentLB.CreatePool(currentPoolName);

        // 2. set algo
        currentLB.SetAlgo(currentPoolName, selectedAlgo);

        // 3. add server
        foreach (var guid in selectedServers)
        {
            currentLB.AddServer(currentPoolName, guid);
        }

        Debug.Log($"Pool {currentPoolName} created with {selectedServers.Count} servers");
    }

    public void RenderExistingPools()
    {
        // clear UI ฝั่งแสดงผล
        foreach (Transform child in cardContainer2)
        {
            Destroy(child.gameObject);
        }

        if (!LoadBalancerManager.Instance.lbConfigs.TryGetValue(currentLbGuid, out var lbConfig))
        {
            Debug.LogWarning("No LB config found");
            return;
        }

        foreach (var poolPair in lbConfig.pools)
        {
            PoolConfig pool = poolPair.Value;

            // 🧱 สร้าง pool card
            GameObject poolGO = Instantiate(cardPrefab2, cardContainer2);

            // ✅ ใช้ PoolCardUI แทนทั้งหมด
            var poolUI = poolGO.GetComponent<PoolCardUI>();

            if (poolUI == null)
            {
                Debug.LogError("PoolCardUI not found on prefab!");
                return;
            }

            poolUI.Setup(pool);
        }
    }
}