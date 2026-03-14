using UnityEngine;
using Firebase.Firestore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

public class QueryPowerDB
{
    FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

    public async Task<List<DevicePowerModel>> GetDevicePower(string category = null)
    {
        Query query = db.Collection("device_asset");
        QuerySnapshot snapshot = await query.GetSnapshotAsync();
        List<DevicePowerModel> device_power_lists = new List<DevicePowerModel>();
        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            //DeviceData deviceData = document.ConvertTo<DeviceData>();
            device_power_lists.Add(document.ConvertTo<DevicePowerModel>());
            //Debug.Log($"Device found: {document.Id} => {device_power_lists[^1].DeviceName}");
        }
        return device_power_lists;
    }
}
