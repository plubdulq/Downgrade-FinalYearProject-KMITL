using UnityEngine;
using TMPro;

public class MyPortDetailUI : MonoBehaviour
{
    public static MyPortDetailUI Instance;

    [SerializeField] private TextMeshProUGUI ipText;

    void Awake()
    {
        Instance = this;
    }

    public void Refresh()
    {
        string guid = SelectionPortCardManager.Instance.currentDeviceGuid;
        int index = SelectionPortCardManager.Instance.currentPortIndex;

        string ip = NetworkManager.Instance.GetPortIP(guid, index);
  
        if (string.IsNullOrEmpty(ip))
        {
            ipText.text = "No IP";
        }
        else
        {
            ipText.text = ip;
        }
    }

    public void OnClickEditIP()
    {
        string guid = SelectionPortCardManager.Instance.currentDeviceGuid;
        int index = SelectionPortCardManager.Instance.currentPortIndex;
        Debug.Log($"Edit IP for {guid} port {index}");
        IPKeyboardUI.Instance.Open(guid, index);
    }
}