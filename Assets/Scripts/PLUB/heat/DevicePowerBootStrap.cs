using UnityEngine;
using Firebase.Firestore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

public class DevicePowerBootStrap : MonoBehaviour
{
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        //Debug.Log("▶ DevicePowerBootstrap start");

        // QueryPowerDB db = new QueryPowerDB();
        // List<DevicePowerModel> devices = await db.GetDevicePower();

        // SaveToFile(devices);

        // Debug.Log($"✅ Device power calculation generated");
    }

    private void SaveToFile(List<DevicePowerModel> devices)   
    {
        string path = Path.Combine(
            Application.persistentDataPath,
            "devices_calculation.json"
        );

        string json = JsonUtility.ToJson(
            new DevicePowerListWrapper { devices = devices },
            true
        );

        File.WriteAllText(path, json);
    }
}