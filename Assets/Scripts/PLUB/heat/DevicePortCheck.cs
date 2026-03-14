using System.Runtime.CompilerServices;
using UnityEngine;
using Chomm.CableSystem;

public class DevicePortCheck : MonoBehaviour
{
    private float rj45_cable_speed = 1000f; // 1 Gbps ของสายเคเบิล
    private float rj45_port_speed = 1000f; // 1 Gbps ของ port device
    private float rj45_port_count = 12;

    private DeviceHeat deviceHeat;
    private float totalBandwidth;
    private float userLoadRatio;

    public void CheckPortConnection()
    {
        //SPEED ของสาย
        totalBandwidth = 0f; // รีเซ็ตก่อนคำนวณใหม่
        CablePlug[] connections = GetComponentsInChildren<CablePlug>();
        foreach (CablePlug cable in connections)
        {
            if (cable.plugType == PlugType.Rj45)
            {
                totalBandwidth += Mathf.Min(rj45_cable_speed, rj45_port_speed);
            }
        }
    }

    public void CalculateUserLoadRatio()
    {
        deviceHeat = GetComponentInChildren<DeviceHeat>();
        if (deviceHeat == null) return;
        
        float maxBandwidth = rj45_port_speed * rj45_port_count;
        userLoadRatio = Mathf.Clamp01(totalBandwidth / maxBandwidth);
        deviceHeat.SetUserLoadRatio(userLoadRatio);

        Debug.Log($"Total Bandwidth: {totalBandwidth} Mbps, User Load Ratio: {userLoadRatio * 100}%");
    }

    void Update()
    {
        CheckPortConnection();
        CalculateUserLoadRatio();
    }
}
