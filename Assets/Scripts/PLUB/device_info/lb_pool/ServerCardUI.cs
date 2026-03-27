using TMPro;
using UnityEngine;

public class ServerCardUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text ipText;
    public TMP_Text typeText;

    private string guid;
    private LBPoolUI parentUI;

    private bool isSelected = false;

    public void Setup(DeviceNetworkState device, LBPoolUI ui)
    {
        guid = device.guid;
        parentUI = ui;

        typeText.text = "Server";
    }

    public void OnClickCard()
    {
        isSelected = !isSelected;

        parentUI.ToggleServer(guid);

        // 🔥 optional: เปลี่ยนสี
        GetComponent<UnityEngine.UI.Image>().color = isSelected
            ? Color.green
            : Color.white;
    }
}