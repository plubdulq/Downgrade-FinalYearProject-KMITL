using UnityEngine;
using TMPro;
using System.Collections;

public class FirewallInfoSetValue : MonoBehaviour
{
    [SerializeField] private TMP_Text deviceNameText;
    [SerializeField] private TMP_Text deviceConsumePowerText;

    private DeviceHeat deviceHeat;
    private Coroutine updateRoutine;

    private void Awake()
    {
        deviceHeat = GetComponentInParent<DeviceHeat>();
    }

    private void SetStaticInfo()
    {
        deviceNameText.text = $"Firewall - {this.gameObject.name}";
    }

    public void OnEnable()
    {
        SetStaticInfo();
        //TempUpdate();
    }

    public void OnDisable()
    {
        if (updateRoutine != null)
            StopCoroutine(updateRoutine);
    }

    private void TempUpdate()
    {
        deviceConsumePowerText.text = $"{deviceHeat.GetPowerConsume().ToString("F2")} Watt";
    }
}
