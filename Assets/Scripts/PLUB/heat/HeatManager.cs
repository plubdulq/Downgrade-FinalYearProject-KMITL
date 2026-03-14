using UnityEngine;
using System.Linq;
using System.Collections.Generic;


public class HeatManager : MonoBehaviour
{
    public GameObject[] allDevices;
    public float k = 0.024f;

    public float roomTemperature = 25f;
    public float roomVolume = 8f; // ปริมาตรห้องเป็นลูกบาศก์เมตร
    public float totalCoolingCapacity = 0f; // ความสามารถในการระบายความร้อนของระบบปร
    public float C; // air density (1.2 kg/m³) * roomVolume * specific heat capacity of air (1000 J/kg°C)

    void Start()
    {
        // allDevices = GameObject.FindObjectsOfType<DeviceHeat>() // อันนี้ใช้เพื่อดึง GameObject ที่มี Component DeviceHeat
        //                .Select(d => d.gameObject)
        //                .ToArray();
        C = 1.2f * roomVolume * 1000f; // ปรับค่าตามความเหมาะสม
    }

    void Update()
    {
        allDevices = GameObject.FindObjectsOfType<DeviceHeat>() // อย่าลืมเอาออกจาก Update เพื่อ performance
                       .Select(d => d.gameObject)
                       .ToArray();
        roomTemperature = UpdateRoomTemperature(allDevices.ToList());
        foreach (GameObject deviceA in allDevices)
        {
            DeviceHeat statsA = deviceA.GetComponent<DeviceHeat>();
            if (statsA == null) continue;

            float heatOutput = statsA.GetHeatOutput();

            float totalAmbientHeat = 0f;

            foreach (GameObject deviceB in allDevices)
            {
                if (deviceA == deviceB) continue;
                
                DeviceHeat statsB = deviceB.GetComponent<DeviceHeat>();
                float distance = Vector3.Distance(deviceA.transform.position, deviceB.transform.position)/100;
                //float ambient = heatOutput / (4 * Mathf.PI * (distance * distance));
                //totalAmbientHeat += ambient;

                //float decay = Mathf.Exp(-k * distance); // อย่าลืมปรับสูตรเป็นแบบใน doc

                float decay = 1 - distance/2f;
                float externalHeat = statsB.GetHeatOutput() * decay;

                totalAmbientHeat += externalHeat;
            }

            // Apply heat result to deviceA
            statsA.UpdateAmbientHeat(totalAmbientHeat);
            statsA.GetDeviceTemperature(roomTemperature + (totalAmbientHeat - totalCoolingCapacity*Time.deltaTime)/1000f );
        }
    }

    public float UpdateRoomTemperature(List<GameObject> allDevices)
    {
        float totalPower = 0f;
        foreach (GameObject device in allDevices)
        {
            DeviceHeat stats = device.GetComponent<DeviceHeat>();
            if (stats == null) continue;
            totalPower += stats.GetPowerConsume();
        }

        float newRoomTemperature = roomTemperature + (totalPower - totalCoolingCapacity) * Time.deltaTime / C; // ปรับค่าตามความเหมาะสม
        Debug.Log($"old Temp: {roomTemperature}°C, New Temp: {newRoomTemperature}°C, totalPower: {totalPower}W, Cooling: {totalCoolingCapacity}W");
        return newRoomTemperature;
    }
}
