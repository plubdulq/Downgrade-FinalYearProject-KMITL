using BNG;
using UnityEngine;
using System.Collections.Generic;

public class EquipmentData : MonoBehaviour
{
    public string equipmentDataName;
    public string uniqueID;
    [SerializeField] List<SnapZone> ports;

    [ContextMenu("GenerateID")]
    public void GenerateID()
    {
        uniqueID = System.Guid.NewGuid().ToString();
    }
    
    private void Awake()
    {
        if (string.IsNullOrEmpty(uniqueID))
        {
            GenerateID();
        }
        ports.Clear();
        if (ports == null || ports.Count == 0)
        {
            ports = new List<SnapZone>(GetComponentsInChildren<SnapZone>());
        }
    }

    public List<SnapZone> GetPorts() => ports;

    public SnapZone GetPortByIndex(int index)
    {
        if (ports != null && index >= 0 && index < ports.Count)
            return ports[index];
        return null;
    }
}
