using System.Collections.Generic;
using UnityEngine;
using BNG;
using Chomm.CableSystem;

public class ServerPlugScanner : MonoBehaviour
{
    public List<SnapZone> allPorts = new List<SnapZone>();

    void Awake()
    {
        // 🔥 Auto หา SnapZone ทั้งหมดในลูก
        allPorts.Clear();
        allPorts.AddRange(GetComponentsInChildren<SnapZone>());
 

        Debug.Log("Found Ports: " + allPorts.Count);
    }
  
    public List<SnapZone> GetMatchPorts(PlugType plugType)
    {
        List<SnapZone> result = new List<SnapZone>();

        foreach (var port in allPorts)
        {
            if (port.plugType == plugType)
            {
                result.Add(port);
            }
            
        }

        Debug.Log("Match Ports: " + result.Count);
        return result;
    }
}