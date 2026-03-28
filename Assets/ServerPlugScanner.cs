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

    public void RegisterFromSnapZone(SnapZone snapZone)
    {
        if (snapZone == null) return;

        // 🔥 เอาของที่ถูกเสียบอยู่
        var held = snapZone.HeldItem;
        Debug.Log($"[Scanner] RegisterFromSnapZone: HeldItem={held?.name}");
        if (held == null) return;

        // 🔥 หา SnapZone (port) ข้างใน object ที่เสียบเข้ามา
        var childPorts = held.GetComponentsInChildren<SnapZone>();
        Debug.Log($"[Scanner] Found {childPorts.Length} child ports in HeldItem");
        foreach (var port in childPorts)
        {
            // ❗ กันซ้ำ
            if (!allPorts.Contains(port))
            {
                allPorts.Add(port);
                Debug.Log($"[Scanner] Added Port from HeldItem: {port.name}");
            }
        }
    }

    public void UnregisterFromSnapZone(SnapZone snapZone)
    {
        if (snapZone == null) return;

        var held = snapZone.HeldItem;
        if (held == null) return;

        var childPorts = held.GetComponentsInChildren<SnapZone>();

        foreach (var port in childPorts)
        {
            if (allPorts.Contains(port))
            {
                allPorts.Remove(port);
                Debug.Log($"[Scanner] Removed Port from HeldItem: {port.name}");
            }
        }
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