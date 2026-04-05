using UnityEngine;
using BNG;

public class PowerSnapHandler : MonoBehaviour
{
    private SnapZone snapZone;

    private void Start()
    {
        snapZone = GetComponent<SnapZone>();

        if (snapZone == null)
        {
            Debug.LogWarning($"[{name}] ไม่มี SnapZone component");
        }
    }

    public void ApplyHeatState()
    {
        if (snapZone == null)
        {
            Debug.LogWarning($"[{name}] snapZone เป็น null");
            return;
        }

        if (snapZone.HeldItem == null)
        {
            Debug.LogWarning($"[{name}] ไม่มี HeldItem ใน SnapZone");
            return;
        }

        // เช็ก plug type ก่อน
        string plugTypeName = snapZone.plugType.ToString();

        bool isSupportedPlug =
            plugTypeName.Contains("13") ||
            plugTypeName.Contains("14") ||
            plugTypeName.Contains("19") ||
            plugTypeName.Contains("20");

        if (!isSupportedPlug)
        {
            Debug.Log($"[{name}] plugType ไม่รองรับการจ่ายไฟ: {plugTypeName}");
            return;
        }

        // หา DeviceHeat ที่ object หรือ parent
        DeviceHeat heat = snapZone.HeldItem.GetComponent<DeviceHeat>();

        if (heat == null)
        {
            heat = snapZone.HeldItem.GetComponentInParent<DeviceHeat>();
        }

        if (heat != null)
        {
            heat.onPlug = 1;
            Debug.Log($"Set onPlug = 1 ให้กับ {snapZone.HeldItem.name} | plugType = {plugTypeName}");
        }
        else
        {
            Debug.LogWarning($"ไม่มี DeviceHeat บน HeldItem หรือ Parent ของ {snapZone.HeldItem.name}");
        }
    }
}