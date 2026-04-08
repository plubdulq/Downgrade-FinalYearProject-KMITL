using UnityEngine;
using MyProject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DeviceHeat : MonoBehaviour
{
    public DevicePowerModel myData;

    public float wattPerCelsius = 0.3f;
    private float deviceTemperature;
    public int onPlug = 0;
    public int powerState = 0; // 0 = off, 1 = on

    public bool IsDataReady { get; private set; } = false;

    private async Task LoadDeviceData()
    {
        IsDataReady = false;

        QueryPowerDB db = new QueryPowerDB();
        List<DevicePowerModel> devices = await db.GetDevicePower();

        // 1) หา NetworkDevice จาก object นี้ก่อน
        NetworkDevice networkDevice = GetComponent<NetworkDevice>();

        // 2) ถ้าไม่เจอ ค่อยหาใน parent
        if (networkDevice == null)
        {
            networkDevice = GetComponentInParent<NetworkDevice>();
        }

        string localDeviceId = null;
        if (networkDevice != null)
        {
            localDeviceId = networkDevice.device_id;
        }

        string localDeviceName = gameObject.name.Replace("(Clone)", "").Trim();

        // 3) match ด้วย device_id ก่อน
        if (!string.IsNullOrWhiteSpace(localDeviceId))
        {
            foreach (DevicePowerModel device in devices)
            {
                if (device.DeviceID == localDeviceId)
                {
                    myData = device;

                    Debug.Log(
                        $"[DeviceHeat] Match by device_id success | " +
                        $"SceneObject={gameObject.name} | " +
                        $"device_id={localDeviceId} | " +
                        $"DBName={myData.DeviceName}"
                    );

                    IsDataReady = true;
                    return;
                }
            }

            Debug.LogWarning(
                $"[DeviceHeat] ไม่พบข้อมูลใน DB ที่ตรงกับ device_id: {localDeviceId} " +
                $"ของ object: {gameObject.name}"
            );
        }

        // 4) fallback: match ด้วยชื่อ object
        foreach (DevicePowerModel device in devices)
        {
            if (device.DeviceName == localDeviceName)
            {
                myData = device;

                Debug.Log(
                    $"[DeviceHeat] Match by DeviceName fallback success | " +
                    $"SceneObject={gameObject.name} | " +
                    $"DBName={myData.DeviceName}"
                );

                IsDataReady = true;
                return;
            }
        }

        Debug.LogWarning(
            $"[DeviceHeat] ไม่พบข้อมูล deviceModel ทั้งจาก device_id และ DeviceName | " +
            $"Object={gameObject.name}"
        );
    }

    private async void Start()
    {
        await LoadDeviceData();
    }

    public float GetHeatOutput()
    {
        if (!IsDataReady || myData == null)
        {
            Debug.LogWarning($"[{gameObject.name}] DB ยังโหลดไม่เสร็จ หรือยังไม่มีข้อมูล device");
            return 0f;
        }

        return onPlug * powerState *
               (myData.HeatCalculation.MinPower +
               ((myData.HeatCalculation.MaxPower - myData.HeatCalculation.MinPower) * myData.UserLoadRatio)) *
               myData.PlacementFactor * Time.deltaTime;
    }

    public float GetIdlePower()
    {
        if (!IsDataReady || myData == null)
        {
            Debug.LogWarning($"[{gameObject.name}] ยังไม่มีข้อมูลสำหรับคำนวณ idle power");
            return 0f;
        }

        return myData.HeatCalculation.MinPower;
    }

    public float GetMaxPower()
    {
        if (!IsDataReady || myData == null)
        {
            Debug.LogWarning($"[{gameObject.name}] ยังไม่มีข้อมูลสำหรับคำนวณ max power");
            return 0f;
        }

        return myData.HeatCalculation.MaxPower;
    }

    public float GetPowerConsume()
    {
        if (!IsDataReady || myData == null)
        {
            Debug.LogWarning($"[{gameObject.name}] DB ยังโหลดไม่เสร็จ หรือยังไม่มีข้อมูล device");
            return 0f;
        }

        float power =
            (myData.HeatCalculation.MinPower +
            ((myData.HeatCalculation.MaxPower - myData.HeatCalculation.MinPower) * myData.UserLoadRatio)) *
            myData.PlacementFactor;

        Debug.Log($"Calculating Power of {gameObject.name} : {power} Watt with {myData.UserLoadRatio * 100}% load ratio");
        return power * powerState * onPlug;
    }

    public void UpdateAmbientHeat(float heat)
    {
        if (!IsDataReady || myData == null) return;

        myData.AmbientHeat = heat;

        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = myData.AmbientHeat > 1000 ? Color.red : Color.white;
        }

        float power =
            (myData.HeatCalculation.MinPower +
            ((myData.HeatCalculation.MaxPower - myData.HeatCalculation.MinPower) * myData.UserLoadRatio)) *
            myData.PlacementFactor;

        Debug.Log($"Calculating Power of {gameObject.name} : {power} Watt with {myData.UserLoadRatio * 100}% load ratio");
    }

    public float GetDeviceTemperature(float airInTemperature)
    {
        if (!IsDataReady || myData == null) return 0f;

        deviceTemperature = airInTemperature + (GetHeatOutput() / 1200f);
        return deviceTemperature;
    }

    public void SetUserLoadRatio(float ratio)
    {
        if (!IsDataReady || myData == null) return;
        myData.UserLoadRatio = ratio;
    }

    public void PowerButtonManager()
    {
        powerState = (powerState == 0) ? 1 : 0;
    }
}