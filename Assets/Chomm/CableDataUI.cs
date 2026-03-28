using BNG;
using Chomm.CableSystem;
using System.Text;
using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CableDataUI : MonoBehaviour
{
    public static CableDataUI Instance;
    [SerializeField] TextMeshProUGUI cableTypeText;
    [SerializeField] TextMeshProUGUI device1Text;
    [SerializeField] TextMeshProUGUI device2Text;
    [SerializeField] TextMeshProUGUI cableUseTypeText;
    public Cable holdingCable;
    public CablePlug holdingPlug;
    private Dictionary<(string, string), string> cableMap;
    private void Awake()
    {
        Instance = this;
        cableMap = new Dictionary<(string, string), string>()
        {
            
            // ===== SRX340 =====
            { ("SRX340", "SRX340"), "Crossover" },
            { ("SRX340", "LB-3000B"), "Straight-Through" },
            { ("SRX340", "Cisco 2911"), "Straight-Through" },
            { ("SRX340", "C8500"), "" },
            { ("SRX340", "C2960"), "Straight-Through" },
            { ("SRX340", "c3560"), "Straight-Through" },
            { ("SRX340", "sg300"), "Straight-Through" },
            { ("SRX340", "Dell R260"), "Straight-Through" },
            { ("SRX340", "Dell R540"), "Straight-Through" },
            { ("SRX340", "sky-6420"), "Straight-Through" },
            { ("SRX340", "SRT2200"), "" },
            { ("SRX340", "Tripp lite 12-Outlet PDU"), "" },

            // ===== LB-3000B =====
            { ("LB-3000B", "Cisco 2911"), "Straight-Through" },
            { ("LB-3000B", "SRX340"), "Straight-Through" },
            { ("LB-3000B", "LB-3000B"), "Crossover" },
            { ("LB-3000B", "C8500"), "Straight-Through" },
            { ("LB-3000B", "C2960"), "Straight-Through" },
            { ("LB-3000B", "c3560"), "Straight-Through" },
            { ("LB-3000B", "sg300"), "Straight-Through" },
            { ("LB-3000B", "Dell R260"), "Straight-Through" },
            { ("LB-3000B", "Dell R540"), "Straight-Through" },
            { ("LB-3000B", "sky-6420"), "Straight-Through" },
            { ("LB-3000B", "SRT2200"), "" },
            { ("LB-3000B", "Tripp lite 12-Outlet PDU"), "" },

            // ===== Cisco 2911 =====
            { ("Cisco 2911", "Cisco 2911"), "Crossover" },
            { ("Cisco 2911", "SRX340"), "Straight-Through" },
            { ("Cisco 2911", "LB-3000B"), "Straight-Through" },
            { ("Cisco 2911", "C8500"), "Crossover" },
            { ("Cisco 2911", "C2960"), "Straight-Through" },
            { ("Cisco 2911", "c3560"), "Straight-Through" },
            { ("Cisco 2911", "sg300"), "Straight-Through" },
            { ("Cisco 2911", "Dell R260"), "Straight-Through" },
            { ("Cisco 2911", "Dell R540"), "Straight-Through" },
            { ("Cisco 2911", "sky-6420"), "Straight-Through" },
            { ("Cisco 2911", "SRT2200"), "" },
            { ("Cisco 2911", "Tripp lite 12-Outlet PDU"), "" },

            // ===== C8500 =====
            { ("C8500", "C8500"), "" },
            { ("C8500", "SRX340"), "" },
            { ("C8500", "LB-3000B"), "Straight-Through" },
            { ("C8500", "Cisco 2911"), "Crossover" },
            { ("C8500", "C2960"), "" },
            { ("C8500", "c3560"), "" },
            { ("C8500", "sg300"), "" },
            { ("C8500", "Dell R260"), "" },
            { ("C8500", "Dell R540"), "" },
            { ("C8500", "sky-6420"), "" },
            { ("C8500", "SRT2200"), "" },
            { ("C8500", "Tripp lite 12-Outlet PDU"), "" },

            // ===== C2960 =====
            { ("C2960", "C2960"), "Crossover" },
            { ("C2960", "SRX340"), "Straight-Through" },
            { ("C2960", "LB-3000B"), "Straight-Through" },
            { ("C2960", "Cisco 2911"), "Straight-Through" },
            { ("C2960", "C8500"), "" },
            { ("C2960", "c3560"), "Crossover" },
            { ("C2960", "sg300"), "Crossover" },
            { ("C2960", "Dell R260"), "Straight-Through" },
            { ("C2960", "Dell R540"), "Straight-Through" },
            { ("C2960", "sky-6420"), "Straight-Through" },
            { ("C2960", "SRT2200"), "" },
            { ("C2960", "Tripp lite 12-Outlet PDU"), "" },

            // ===== c3560 =====
            { ("c3560", "c3560"), "Crossover" },
            { ("c3560", "SRX340"), "Straight-Through" },
            { ("c3560", "LB-3000B"), "Straight-Through" },
            { ("c3560", "Cisco 2911"), "Straight-Through" },
            { ("c3560", "C8500"), "" },
            { ("c3560", "C2960"), "Crossover" },
            { ("c3560", "sg300"), "Crossover" },
            { ("c3560", "Dell R260"), "Straight-Through" },
            { ("c3560", "Dell R540"), "Straight-Through" },
            { ("c3560", "sky-6420"), "Straight-Through" },
            { ("c3560", "SRT2200"), "" },
            { ("c3560", "Tripp lite 12-Outlet PDU"), "" },

            // ===== sg300 =====
            { ("sg300", "sg300"), "Crossover" },
            { ("sg300", "SRX340"), "Straight-Through" },
            { ("sg300", "LB-3000B"), "Straight-Through" },
            { ("sg300", "Cisco 2911"), "Straight-Through" },
            { ("sg300", "C8500"), "" },
            { ("sg300", "C2960"), "Crossover" },
            { ("sg300", "c3560"), "Crossover" },
            { ("sg300", "Dell R260"), "Straight-Through" },
            { ("sg300", "Dell R540"), "Straight-Through" },
            { ("sg300", "sky-6420"), "Straight-Through" },
            { ("sg300", "SRT2200"), "" },
            { ("sg300", "Tripp lite 12-Outlet PDU"), "" },

            // ===== Dell R260 =====
            { ("Dell R260", "Dell R260"), "Crossover" },
            { ("Dell R260", "SRX340"), "Straight-Through" },
            { ("Dell R260", "LB-3000B"), "Straight-Through" },
            { ("Dell R260", "Cisco 2911"), "Straight-Through" },
            { ("Dell R260", "C8500"), "" },
            { ("Dell R260", "C2960"), "Straight-Through" },
            { ("Dell R260", "c3560"), "Straight-Through" },
            { ("Dell R260", "sg300"), "Straight-Through" },
            { ("Dell R260", "Dell R540"), "Crossover" },
            { ("Dell R260", "sky-6420"), "Crossover" },
            { ("Dell R260", "SRT2200"), "" },
            { ("Dell R260", "Tripp lite 12-Outlet PDU"), "" },

            // ===== Dell R540 =====
            { ("Dell R540", "Dell R540"), "Crossover" },
            { ("Dell R540", "SRX340"), "Straight-Through" },
            { ("Dell R540", "LB-3000B"), "Straight-Through" },
            { ("Dell R540", "Cisco 2911"), "Straight-Through" },
            { ("Dell R540", "C8500"), "" },
            { ("Dell R540", "C2960"), "Straight-Through" },
            { ("Dell R540", "c3560"), "Straight-Through" },
            { ("Dell R540", "sg300"), "Straight-Through" },
            { ("Dell R540", "Dell R260"), "Crossover" },
            { ("Dell R540", "sky-6420"), "Crossover" },
            { ("Dell R540", "SRT2200"), "" },
            { ("Dell R540", "Tripp lite 12-Outlet PDU"), "" },

            // ===== sky-6420 =====
            { ("sky-6420", "sky-6420"), "Crossover" },
            { ("sky-6420", "SRX340"), "Straight-Through" },
            { ("sky-6420", "LB-3000B"), "Straight-Through" },
            { ("sky-6420", "Cisco 2911"), "Straight-Through" },
            { ("sky-6420", "C8500"), "" },
            { ("sky-6420", "C2960"), "Straight-Through" },
            { ("sky-6420", "c3560"), "Straight-Through" },
            { ("sky-6420", "sg300"), "Straight-Through" },
            { ("sky-6420", "Dell R260"), "Crossover" },
            { ("sky-6420", "Dell R540"), "Crossover" },
            { ("sky-6420", "SRT2200"), "" },
            { ("sky-6420", "Tripp lite 12-Outlet PDU"), "" },

            // ===== SRT2200 =====
            { ("SRT2200", "SRT2200"), "" },
            { ("SRT2200", "SRX340"), "" },
            { ("SRT2200", "LB-3000B"), "" },
            { ("SRT2200", "Cisco 2911"), "" },
            { ("SRT2200", "C8500"), "" },
            { ("SRT2200", "C2960"), "" },
            { ("SRT2200", "c3560"), "" },
            { ("SRT2200", "sg300"), "" },
            { ("SRT2200", "Dell R260"), "" },
            { ("SRT2200", "Dell R540"), "" },
            { ("SRT2200", "sky-6420"), "" },
            { ("SRT2200", "Tripp lite 12-Outlet PDU"), "" },

            // ===== Tripp lite 12-Outlet PDU =====
            { ("Tripp lite 12-Outlet PDU", "Tripp lite 12-Outlet PDU"), "" },
            { ("Tripp lite 12-Outlet PDU", "SRX340"), "" },
            { ("Tripp lite 12-Outlet PDU", "LB-3000B"), "" },
            { ("Tripp lite 12-Outlet PDU", "Cisco 2911"), "" },
            { ("Tripp lite 12-Outlet PDU", "C8500"), "" },
            { ("Tripp lite 12-Outlet PDU", "C2960"), "" },
            { ("Tripp lite 12-Outlet PDU", "c3560"), "" },
            { ("Tripp lite 12-Outlet PDU", "sg300"), "" },
            { ("Tripp lite 12-Outlet PDU", "Dell R260"), "" },
            { ("Tripp lite 12-Outlet PDU", "Dell R540"), "" },
            { ("Tripp lite 12-Outlet PDU", "sky-6420"), "" },
            { ("Tripp lite 12-Outlet PDU", "SRT2200"), "" },
        };
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
    public string GetCable(string col1, string col2)
    {
        Debug.Log($"device_A{col1}");
        Debug.Log($"device_B{col2}");
        string device_A = col1.Trim();
        string device_B = col2.Trim();

        if (cableMap.TryGetValue((device_A, device_B), out string result))
        {
             Debug.Log($"result{result}");
             cableUseTypeText.text = result.ToString();
        }


        return "Unknown";
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
        string deviceA = GetDeviceName(holdingCable.PlugA);
        string deviceB = GetDeviceName(holdingCable.PlugB);
        if(deviceA != "Unknown"||deviceA != "Unknown")
        GetCable(deviceA, deviceB);
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
    private string GetDeviceName(CablePlug plug)
    {
        if (plug == null) return "Unknown";
         string deviceName = "name"; // Default placeholder
        SnapZone zone = plug.GetComponentInParent<SnapZone>();
        if (zone != null)
        {
            EquipmentData eqData = zone.GetComponent<EquipmentData>();
            if(eqData ==  null) 
            eqData = zone.GetComponentInParent<EquipmentData>();
            if (eqData != null)
            {
                deviceName = eqData.equipmentDataName;
                return eqData.equipmentDataName;
            }
        }

        return "Unknown";
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
                 Debug.Log($"deviceName{deviceName}");
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
