using UnityEngine;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics.Tracing;
using System.Threading.Tasks;

public class StoreController : MonoBehaviour

{
    public StoreUI storeUI;
    private StoreQuery repository;

    async void Start()
    {
        repository = new StoreQuery();
        List<DeviceData> device_lists = await repository.GetDevices();
        List<CableModelData> cable_lists = await repository.GetCables();
        foreach (var d in device_lists)
        {
            storeUI.CreateDeviceCard(d);
        }
        foreach (var c in cable_lists)
        {
            storeUI.CreateCableCard(c);
        }
        // Waiting for the cable data to be fetched and displayed at start
    }
    
    public async Task UpdateStoreByCategory(string categoryName)
    {
        // Clear existing store items
        storeUI.ClearStore();

        // Fetch and display devices for the selected category
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
