using Firebase.Firestore;

[FirestoreData]
public class CableModelData
{
    [FirestoreProperty("cable_type")]
    public string CableType { get; set; }

    [FirestoreProperty("price_per_meter")]
    public int Price { get; set; }
}