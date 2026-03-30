using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    public string saveName;
    public string timestamp;
    public string sceneName; // "PlayScene" (Custom) or "ServerRoom1" (Template)
    
    // Room Config
    public float roomWidth = 10f;
    public float roomLength = 10f;
    public float roomHeight = 3f;

    public List<EquipmentSaveData> equipments = new List<EquipmentSaveData>();
    public List<CableSaveData> cables = new List<CableSaveData>();
    public NetworkSaveData networkData;
}

[System.Serializable]
public class EquipmentSaveData
{
    public string uniqueID;
    public string prefabName;
    public Vector3 position;
    public Quaternion rotation;
    public string parentID; // uniqueID of parent Equipment OR GameObject name if static
}

[System.Serializable]
public class CableSaveData
{
    //plub start
    public string cableGuid;
    //plub end
    public string cableType;
    public string startEquipmentID;
    public int startPortIndex;
    public string endEquipmentID;
    public int endPortIndex;
    public List<CablePointSaveData> intermediatePoints;
    public float sag;
}

[System.Serializable]
public class CablePointSaveData
{
    public Vector3 position; // Local position if parented, World if not
    public string parentID; // uniqueID of parent Equipment
    public string parentName; // Name of parent GameObject (fallback)
    public bool isAnchorPoint;
}

//plub start
[System.Serializable]
public class NetworkSaveData
{
    public List<DeviceNetworkState> devices = new List<DeviceNetworkState>();
    public List<CableDataModel> cables = new List<CableDataModel>();
}
//plub end