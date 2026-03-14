using BNG;
using Chomm.CableSystem;
using System.Text;
using TMPro;
using UnityEngine;

public class CableDataUI : MonoBehaviour
{
    public static CableDataUI Instance;
    [SerializeField] TextMeshProUGUI cableTypeText;
    [SerializeField] TextMeshProUGUI device1Text;
    [SerializeField] TextMeshProUGUI device2Text;
    public Cable holdingCable;
    public CablePlug holdingPlug;
    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //UpdateUI();
    }

    public void UpdateUI(CablePlug holdingPlugIn = null)
    {
        if (holdingPlugIn != null)
        {
            holdingCable = holdingPlugIn.GetParentCable();
            holdingPlug = holdingPlugIn;
        }
        if (holdingPlug == null || holdingCable == null) { return; }

        holdingPlug.autoAnchorDistance = 0;
        
        // 1. Get Cable Type
        string plugType = GetPlugTypeString(holdingPlug.plugType);
        cableTypeText.text = GetCableNameString(holdingCable);
        // 2. Get Device Info for both ends
        string device1Info = GetDeviceInfo(holdingCable.PlugA, "Device 1");
        string device2Info = GetDeviceInfo(holdingCable.PlugB, "Device 2");

        // 3. Get Length
        float length = holdingCable.GetCableLength();

        // 4. Update UI
        // Device 1 Panel
        StringBuilder sb1 = new StringBuilder();
        sb1.Append(device1Info).AppendLine().AppendLine().AppendLine();
        sb1.AppendLine().Append($"Length : {length:F2} m  Sag : {holdingCable.sag:F1}");
        device1Text.text = sb1.ToString();

        // Device 2 Panel
        device2Text.text = device2Info;
    }

    private string GetDeviceInfo(CablePlug plug, string label)
    {
        if (plug == null) return $"{label}\nUnknown\nport : -/-";

        string deviceName = "name"; // Default placeholder
        string portInfo = "port : -/-";

        // Find connected port / equipment
        // Logic: Plug is a child of a SnapZone?, or we check what the plug is connected TO?
        // In Cable.cs logic, PlugA/B are the ENDS of the cable. 
        // We typically check plug.transform.parent to see if it's snapped into a SnapZone.
        
        SnapZone zone = plug.GetComponentInParent<SnapZone>();
        if (zone != null)
        {
            EquipmentData eqData = zone.GetComponent<EquipmentData>();
            if(eqData ==  null) 
            eqData = zone.GetComponentInParent<EquipmentData>();
            if (eqData != null)
            {
                deviceName = eqData.equipmentDataName;
                
                // Find Port Index
                var ports = eqData.GetPorts();
                if (ports != null)
                {
                    int index = ports.IndexOf(zone) + 1;
                    int total = ports.Count;
                    portInfo = $"port : {index}/{total}";
                }
            }
        }

        return $"{label}\n{deviceName}\n{portInfo}";
    }

    private string GetPlugTypeString(PlugType type)
    {
         switch (type)
        {
            case PlugType.Rj45: return "Ethernet cable(RJ45)";
            case PlugType.Dsl: return "Dsl";
            case PlugType.PowerIn: return "Power In";
            case PlugType.PowerOut: return "Power Out";
            case PlugType.IECC13: return "C13";
            case PlugType.IECC14: return "C14";
            case PlugType.IECC19: return "C19";
            case PlugType.IECC20: return "C20";
            case PlugType.StandardPlug: return "Plug";
            case PlugType.FiberLCSinglemode: return "Fiber LC Singlemode";
            case PlugType.FiberLCMultimode: return "Fiber LC Multimode";
            default: return "Unknown Cable";
        }
    }
    private string GetCableNameString(Cable cable)
    {
        if (cable == null || cable.PlugA == null || cable.PlugB == null) return "Unknown Cable";
        
        PlugType a = cable.PlugA.plugType;
        PlugType b = cable.PlugB.plugType;

        if (a == PlugType.FiberLCSinglemode || b == PlugType.FiberLCSinglemode) return "Fiber LC to LC (Singlemode)";
        if (a == PlugType.FiberLCMultimode || b == PlugType.FiberLCMultimode) return "Fiber LC to LC (Multimode)";
        if (a == PlugType.Rj45 || b == PlugType.Rj45) return "Ethernet cable (RJ45)";
        if (a == PlugType.Dsl || b == PlugType.Dsl) return "DSL to DSL";

        string nameA = GetPlugTypeString(a);
        string nameB = GetPlugTypeString(b);

        bool aIsIEC = a == PlugType.IECC13 || a == PlugType.IECC14 || a == PlugType.IECC19 || a == PlugType.IECC20;
        bool bIsIEC = b == PlugType.IECC13 || b == PlugType.IECC14 || b == PlugType.IECC19 || b == PlugType.IECC20;

        if (aIsIEC || bIsIEC || a == PlugType.StandardPlug || b == PlugType.StandardPlug)
        {
            string prefixObj = "IEC ";
            if (!aIsIEC && !bIsIEC) prefixObj = "Power ";
            return $"{prefixObj}{nameA} to {nameB}";
        }
        
        if (a == PlugType.PowerIn || b == PlugType.PowerIn || a == PlugType.PowerOut || b == PlugType.PowerOut) return "Power cable";

        return "Unknown Cable";
    }
}
