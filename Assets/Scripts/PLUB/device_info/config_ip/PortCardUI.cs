using UnityEngine;
using UnityEngine.UI;

public class PortCardUI : MonoBehaviour
{
    public string deviceGuid;
    public int portIndex;

    void Start()
    {
        // ดึงจาก component ที่มีอยู่แล้ว
        ALabelInfo labelInfo = GetComponent<ALabelInfo>();

        if (labelInfo != null)
        {
            deviceGuid = labelInfo.device.guid;
            portIndex = labelInfo.portIndex;
        }
    }

    // 👇 ผูกกับ pointer click
    public void ViewMyPort()
    {
        SelectionPortCardManager.Instance.SelectPort(deviceGuid, portIndex);
    }
}