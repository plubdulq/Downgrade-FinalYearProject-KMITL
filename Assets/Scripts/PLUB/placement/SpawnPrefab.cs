using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class SpawnPrefab : MonoBehaviour
{
    //public GameObject prefabToSpawn; // Reference ไปยัง Prefab
    // public Vector3 spawnPosition = new Vector3(0, 0, 0); // ตำแหน่งที่ต้องการให้เกิด
    private string deviceId;
    
    // // ฟังก์ชันนี้จะถูกเรียกเมื่อปุ่มถูกกด
    // public void Spawn()
    // {
    //     if (prefabToSpawn != null)
    //     {
    //         //Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
    //         PlacementPreview.Instance.BeginPlacement(prefabToSpawn);
    //     }
    //     else
    //     {
    //         Debug.LogError("Prefab to spawn is not assigned!");
    //     }
    // }
    public void SetUp(string deviceName)
    {
        deviceId = deviceName;
    }

    public void Spawn()
    {
        GameObject prefab = PrefabRegistry.Instance.GetPrefab(deviceId);
        if (prefab != null)
        {
            if (deviceId == "rack42U_nodoor" || deviceId == "supermicro 4029GP-TRT2")
            {
                Debug.Log(deviceId);
                PlacementPreview.Instance.GhostObject(prefab);
                return;
            }
            PlacementPreview.Instance.BeginPlacement(prefab);
            
        }
        else
        {
            Debug.LogError("Prefab to spawn is not assigned!");
        }
    }


}