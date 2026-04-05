// using UnityEngine;
// using System.Collections.Generic;
// using System.Security.Cryptography.X509Certificates;
// using System.ComponentModel;
// using System.Runtime.CompilerServices;
// using System.Diagnostics.Tracing;
// using System.Threading.Tasks;

// public class StoreController : MonoBehaviour

// {
//     public StoreUI storeUI;
//     private StoreQuery repository;

//     async void Start()
//     {
//         repository = new StoreQuery();
//         List<DeviceData> device_lists = await repository.GetDevices();
//         List<CableModelData> cable_lists = await repository.GetCables();
//         foreach (var d in device_lists)
//         {
//             storeUI.CreateDeviceCard(d);
//         }
//         foreach (var c in cable_lists)
//         {
//             storeUI.CreateCableCard(c);
//         }
//         // Waiting for the cable data to be fetched and displayed at start
//     }
    
//     public async Task UpdateStoreByCategory(string categoryName)
//     {
//         // Clear existing store items
//         storeUI.ClearStore();

//         // Fetch and display devices for the selected category
//         if (categoryName == "cable")
//         {
//             List<CableModelData> cable_lists = await repository.GetCables();
//             foreach (var c in cable_lists)
//             {
//                 storeUI.CreateCableCard(c);
//             }
//         }
//         else
//         {
//             List<DeviceData> device_lists = await repository.GetDevices(categoryName);
//             foreach (var d in device_lists)
//             {
//                 storeUI.CreateDeviceCard(d);
//             }
//         }
        
//     }
// }
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public class StoreController : MonoBehaviour
{
    public StoreUI storeUI;
    private StoreQuery repository;

    private IEnumerator Start()
    {
        while (!FirebaseInitializer.IsReady)
        {
            yield return null;
        }

        repository = new StoreQuery();

        Task<List<DeviceData>> deviceTask = repository.GetDevices();
        yield return new WaitUntil(() => deviceTask.IsCompleted);

        if (deviceTask.Exception != null)
        {
            Debug.LogError("Failed to load devices: " + deviceTask.Exception);
            yield break;
        }

        Task<List<CableModelData>> cableTask = repository.GetCables();
        yield return new WaitUntil(() => cableTask.IsCompleted);

        if (cableTask.Exception != null)
        {
            Debug.LogError("Failed to load cables: " + cableTask.Exception);
            yield break;
        }

        List<DeviceData> device_lists = deviceTask.Result;
        List<CableModelData> cable_lists = cableTask.Result;

        foreach (var d in device_lists)
        {
            storeUI.CreateDeviceCard(d);
        }

        foreach (var c in cable_lists)
        {
            storeUI.CreateCableCard(c);
        }
    }

    public async Task UpdateStoreByCategory(string categoryName)
    {
        if (!FirebaseInitializer.IsReady)
        {
            Debug.LogError("Firebase is not ready yet.");
            return;
        }

        if (repository == null)
        {
            repository = new StoreQuery();
        }

        storeUI.ClearStore();

        if (categoryName == "cable")
        {
            List<CableModelData> cable_lists = await repository.GetCables();
            foreach (var c in cable_lists)
            {
                storeUI.CreateCableCard(c);
            }
        }
        else
        {
            List<DeviceData> device_lists = await repository.GetDevices(categoryName);
            foreach (var d in device_lists)
            {
                storeUI.CreateDeviceCard(d);
            }
        }
    }
}