using Firebase.Firestore;
using System.Collections.Generic;

[FirestoreData]
public class PortSchemaDB
{
    [FirestoreProperty("ports_schema")]
    public Dictionary<string, PortSchema> ports_schema { get; set; }
}

[FirestoreData]
public class PortSchema
{
    [FirestoreProperty("connector")]
    public string connector { get; set; }

    [FirestoreProperty("count")]
    public int count { get; set; }

    [FirestoreProperty("speed")]
    public string speed { get; set; }
}