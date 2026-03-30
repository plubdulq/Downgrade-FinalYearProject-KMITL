using UnityEngine;
using BNG;

public class PowerSnapHandler : MonoBehaviour
{
    private SnapZone snapZone; // อ้างอิง SnapZone

    private void Start()
    {
        snapZone = GetComponent<SnapZone>();
    }

    // private void OnTriggerEnter(Collider other)
    // {
    //     ApplyHeatState();
    //     if (other.CompareTag("Snappable"))
    //     {
            
    //     }
    // }

    public void ApplyHeatState()
    {
        if (snapZone == null || snapZone.HeldItem == null) return;

        DeviceHeat heat = snapZone.HeldItem.GetComponent<DeviceHeat>();

        if (heat != null)
        {
            heat.onPlug = 1;
            Debug.Log("Set onPlug = 1 ให้กับ " + snapZone.HeldItem.name);
        }
        else
        {
            Debug.LogWarning("ไม่มี DeviceHeat บน HeldItem");
        }
    }
}