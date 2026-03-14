using UnityEngine;
using Firebase.Firestore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.CompilerServices;


public class StoreQuery
{
    FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

    public async Task<List<DeviceData>> GetDevices(string category = null)
    {
        Query query = db.Collection("device_asset");

        if (!string.IsNullOrEmpty(category))
        {
            query = query.WhereEqualTo("category", category);
            Debug.Log($"Filtering devices by category: {category}");
        }
        QuerySnapshot snapshot = await query.GetSnapshotAsync();
        List<DeviceData> device_lists = new List<DeviceData>();
        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            //DeviceData deviceData = document.ConvertTo<DeviceData>();
            device_lists.Add(document.ConvertTo<DeviceData>());
            //Debug.Log($"Device found: {document.Id} => {device_lists[^1].DeviceName} Price: {device_lists[^1].Price.ToString()}");
        }
        return device_lists;
    }

    public async Task<List<CableModelData>> GetCables()
    {
        //NEW CABLE QUERY
        Query cableQuery = db.Collection("cable_asset");
        QuerySnapshot cableSnapshot = await cableQuery.GetSnapshotAsync();
        List<CableModelData> cable_lists = new List<CableModelData>();
        foreach (DocumentSnapshot document in cableSnapshot.Documents)
        {
            cable_lists.Add(document.ConvertTo<CableModelData>());
            //Debug.Log($"Cable found: {document.Id} => {cable_lists[^1].CableType} Price: {cable_lists[^1].Price.ToString()}");
        }
        return cable_lists; // Return null or an empty list since we're only interested in devices here
        //END OF CABLE QUERY
    }
}
