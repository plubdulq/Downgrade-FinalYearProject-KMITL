using UnityEngine;
using TMPro;
using System.Collections;

public class ServerInfoSetValue : MonoBehaviour
{
    [SerializeField] private TMP_Text deviceNameText;
    [SerializeField] private TMP_Text deviceIdlePowerText;
    [SerializeField] private TMP_Text deviceMaxPowerText;
    [SerializeField] private TMP_Text deviceConsumePowerText;

    private DeviceHeat deviceHeat;
    private Coroutine updateRoutine;

    private void Awake()
    {
        deviceHeat = GetComponentInParent<DeviceHeat>();
        if(deviceHeat == null)
        {
            deviceHeat = GetComponent<DeviceHeat>();
        }
    }

    void Start()
    {
        int index = SelectionPortCardManager.Instance.currentPortIndex;
        Debug.Log($"ServerInfoSetValue Start, current port index: {index}");
    }

    private void SetStaticInfo()
    {
        deviceNameText.text = this.gameObject.name;
    }

    public void OnEnable() // เปลี่ยนชื่อก็ดี เพราะมันเรียกตั้งแต่ frame แรกเลย อาจจะทำให้ข้อมูลยังไม่พร้อม
    {
        SetStaticInfo();
        //updateRoutine = StartCoroutine(UpdatePower());
        TempUpdate();
    }

    public void OnDisable()
    {
        if (updateRoutine != null)
            StopCoroutine(updateRoutine);
    }

    private void TempUpdate()
    {
        deviceConsumePowerText.text = $"{deviceHeat.GetPowerConsume()} Watt";
        deviceIdlePowerText.text = $"{deviceHeat.GetIdlePower()} Watt";
        deviceMaxPowerText.text = $"{deviceHeat.GetMaxPower()} Watt";
    }

    private IEnumerator UpdatePower()
    {
        while (true)
        {
            deviceConsumePowerText.text = $"{deviceHeat.GetPowerConsume()} Watt";

            yield return new WaitForSeconds(0.2f); // update ทุก 0.2 วิ
        }
    }

    private void SetUserLoadRatio(DevicePowerModel data)
    {
        data.UserLoadRatio = 0.5f;
    }
}
