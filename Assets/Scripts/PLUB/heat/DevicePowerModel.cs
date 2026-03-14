using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using System.Security.Cryptography.X509Certificates;

[FirestoreData]
public class DevicePowerModel
{

    [FirestoreProperty("device_id")]
    public string DeviceID { get; set; }

    [FirestoreProperty("device_name")]
    public string DeviceName { get; set; }

    [FirestoreProperty("model_calculation")]
    public HeatCalculationModel HeatCalculation { get; set; }

    // [FirestoreProperty("ports_schema")]
    // public Dictionary<string, object> PortsSchema { get; set; }
    public float AmbientHeat { get; set; } = 0.0f;
    public float UserLoadRatio { get; set; } = 0.7f;
    public float PlacementFactor { get; set; } = 1.0f;
}

[FirestoreData]
public class HeatCalculationModel
{
    [FirestoreProperty("min_power")]
    public float MinPower { get; set; }

    [FirestoreProperty("max_power")]
    public float MaxPower { get; set; }

    [FirestoreProperty("cooling_factor")]
    public string CoolingFactor { get; set; }
}

// [FirestoreData]
// public class PortModel
// {
//     [FirestoreProperty("ethernet")]
//     public float MinPower { get; set; }

//     [FirestoreProperty("max_power")]
//     public float MaxPower { get; set; }

//     [FirestoreProperty("cooling_factor")]
//     public string CoolingFactor { get; set; }
// }

