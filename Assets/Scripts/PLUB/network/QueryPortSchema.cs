using UnityEngine;
using Firebase.Firestore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.CompilerServices;
using System.Linq;

public class QueryPortSchema
{
    FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

    public async Task<PortSchemaDB> GetPortSchema(string deviceType) //deviceType == device_id in DB
    {
        //NEW PORT SCHEMA QUERY
        Query portSchemaQuery = db.Collection("device_asset").WhereEqualTo("device_id", deviceType);
        QuerySnapshot portSchemaSnapshot = await portSchemaQuery.GetSnapshotAsync();

        if (portSchemaSnapshot.Count == 0)
        {
            Debug.LogError(deviceType);
            Debug.LogError($"No port schema found for device type {deviceType}");
            return null;
        }
        Debug.Log($"Port schema found for device type {deviceType}, processing...");
        DocumentSnapshot document = portSchemaSnapshot.Documents.First();

        return document.ConvertTo<PortSchemaDB>();
        //END OF CABLE QUERY
    }
}
