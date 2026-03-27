using UnityEngine;
using MyProject.Models;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics.Contracts;

public class DeviceHeat : MonoBehaviour
{
    //private DeviceDetail myData;
    public DevicePowerModel myData;

    public float wattPerCelsius = 0.3f; // watt per degree Celsius, ปรับค่าตามความเหมาะสม
    private float deviceTemperature;

    public int powerState = 0; // 0 = off, 1 = on

    async void LoadDeviceData()
    {
        QueryPowerDB db = new QueryPowerDB();
        List<DevicePowerModel> devices = await db.GetDevicePower();

        string deviceTag = this.gameObject.tag;
        string deviceName = this.gameObject.name;

        foreach (DevicePowerModel device in devices)
        {
            if (device.DeviceName == deviceName)
            {
                myData = device;

                Debug.Log(
                    $"Device found: {myData.DeviceName} " +
                    $"Min={myData.HeatCalculation.MinPower} " +
                    $"Max={myData.HeatCalculation.MaxPower} " +
                    $"Load={myData.UserLoadRatio} " +
                    $"Ambient={myData.AmbientHeat}"
                );

                break;
            }
        }

        if (myData == null)
        {
            Debug.LogWarning($"ไม่พบ deviceModel ตรงกับ tag: {this.gameObject.tag}");
        }
    }

    public float GetHeatOutput()
    {
        if (myData == null)
        {
            Debug.LogWarning("ยังไม่มีข้อมูล device สำหรับคำนวณ heat");
            return 0f;
        }
        return powerState * (myData.HeatCalculation.MinPower + ((myData.HeatCalculation.MaxPower - myData.HeatCalculation.MinPower) * myData.UserLoadRatio)) * myData.PlacementFactor * Time.deltaTime;
    }

    public float GetIdlePower()
    {
        if (myData == null)
        {
            Debug.LogWarning("ยังไม่มีข้อมูล myData,HeatCalculation สำหรับคำนวณ idle power");
            return 0f;
        }
        return myData.HeatCalculation.MinPower;
    }

    public float GetMaxPower()
    {
        if (myData == null)
        {
            Debug.LogWarning("ยังไม่มีข้อมูล myData,HeatCalculation สำหรับคำนวณ max power");
            return 0f;
        }
        return myData.HeatCalculation.MaxPower;
    }

    public float GetPowerConsume()
    {
        if (myData == null)
        {
            Debug.LogWarning("ยังไม่มีข้อมูล device สำหรับคำนวณ heat");
            return 0f;
        }
        //return (myData.basePower + ((myData.maxPower - myData.basePower) * myData.userLoadRatio)) * myData.placementFactor * Time.deltaTime;
        float power = (myData.HeatCalculation.MinPower + ((myData.HeatCalculation.MaxPower - myData.HeatCalculation.MinPower) * myData.UserLoadRatio)) * myData.PlacementFactor;
        Debug.Log($"Calculating Power of {this.gameObject.name} : {power} Watt with {myData.UserLoadRatio * 100}% load ratio");
        return power * powerState;
    }

    private void Start()
    {
        LoadDeviceData();
    }

    public void UpdateAmbientHeat(float heat)
    {
        //Debug.Log($"Device {this.gameObject}   {gameObject.name} ambient heat: {myData.ambientHeat}K");
        if (myData == null) return;
        myData.AmbientHeat = heat;

        Debug.Log($"Device {this.gameObject.name} ambient heat updated to: {myData.AmbientHeat}K");

        // Visual feedback
        if (myData.AmbientHeat > 1000)
        {
            GetComponent<Renderer>().material.color = Color.red;
        }
        else
        {
            GetComponent<Renderer>().material.color = Color.white;
        }
        float power = (myData.HeatCalculation.MinPower + ((myData.HeatCalculation.MaxPower - myData.HeatCalculation.MinPower) * myData.UserLoadRatio)) * myData.PlacementFactor;
        Debug.Log($"Calculating Power of {this.gameObject.name} : {power} Watt with {myData.UserLoadRatio * 100}% load ratio");
    }

    public float GetDeviceTemperature(float airInTemperature)
    {
        if (myData == null) return 0f;
        Debug.Log($"Old Device Temperature of {this.transform.parent.name}: {deviceTemperature}°C");
        deviceTemperature = airInTemperature + (GetHeatOutput() / 1200f); //อย่าลืมเช็คสูตรอีกรอบ
        Debug.Log($"New Device Temperature of {this.transform.parent.name}: {deviceTemperature}°C");
        return deviceTemperature;
    }

    public void SetUserLoadRatio(float ratio)
    {
        if (myData == null) return;
        myData.UserLoadRatio = ratio;


    }


    //TEST power on
    public void PowerButtonManager()
    {
        //if (myData == null) return;
        if (powerState == 0)
        {
            powerState = 1;
        }
        else
        {
            powerState = 0;
        }
    }
}
