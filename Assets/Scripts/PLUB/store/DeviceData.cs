using Firebase.Firestore;

[FirestoreData]
public class DeviceData
{
    [FirestoreProperty("device_name")]
    public string DeviceName { get; set; }

    [FirestoreProperty("price")]
    public int Price { get; set; }
}
