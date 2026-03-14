using UnityEngine;
using System.Linq;
public class HeatManager01 : MonoBehaviour
{
    public GameObject[] allDevices;
    public float k = 0.024f;

    void Start()
    {
        allDevices = GameObject.FindObjectsOfType<DeviceHeat>() // อันนี้ใช้เพื่อดึง GameObject ที่มี Component DeviceHeat
                       .Select(d => d.gameObject)
                       .ToArray();
    }

    void Update()
    {
        foreach (GameObject deviceA in allDevices)
        {
            DeviceHeat statsA = deviceA.GetComponent<DeviceHeat>();
            if (statsA == null) continue;

            float heatOutput = statsA.GetHeatOutput();

            float totalAmbientHeat = 0f;

            foreach (GameObject deviceB in allDevices)
            {
                if (deviceA == deviceB) continue;

                float distance = Vector3.Distance(deviceA.transform.position, deviceB.transform.position)/100;
                float ambient = heatOutput / (4 * Mathf.PI * (distance * distance));
                totalAmbientHeat += ambient;
            }

            // Apply heat result to deviceA
            statsA.UpdateAmbientHeat(totalAmbientHeat);
        }
    }
}
