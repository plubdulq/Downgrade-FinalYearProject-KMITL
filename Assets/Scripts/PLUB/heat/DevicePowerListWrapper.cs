using UnityEngine;
using Firebase.Firestore;
using System.Threading.Tasks;
using System.Collections.Generic;

[System.Serializable]
public class DevicePowerListWrapper
{
    public List<DevicePowerModel> devices;
}
