using UnityEngine;
//using TMPro;
using UnityEngine.UI;

public class IPKeyboardUI : MonoBehaviour
{
    public static IPKeyboardUI Instance;

    private string currentDeviceGuid;
    private int currentPortIndex;

    [SerializeField] private InputField inputField;
    [SerializeField] private Text inputDisplay;

    void Awake()
    {
        Instance = this;
        //gameObject.SetActive(false);
    }

    // 👇 อันที่คุณถาม
    public void Open(string deviceGuid, int portIndex)
    {
        currentDeviceGuid = deviceGuid;
        currentPortIndex = portIndex;

        //inputField.text = "";
        //gameObject.SetActive(true);
    }

    public void OnPressKey(string key)
    {
        inputField.text += key;
    }

    public void OnBackspace()
    {
        if (inputField.text.Length > 0)
        {
            inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
        }
    }

    public void OnConfirm()
    {
        //string newIP = inputField.text;
        string newIP = inputDisplay.text;
        Debug.Log($"Updating IP for {currentDeviceGuid} port {currentPortIndex} to {newIP}");

        NetworkManager.Instance.UpdatePortIP(
            currentDeviceGuid,
            currentPortIndex,
            newIP
        );
        //gameObject.SetActive(false);
    }

    public void OnConfirmForRouter()
    {
        string newIP = inputDisplay.text;
        Debug.Log($"Updating IP for {currentDeviceGuid} port {currentPortIndex} to {newIP}");

        if (currentDeviceGuid == null || currentPortIndex < 0)
        {
            Debug.Log("No port selected for router IP update");
            AlertPopupManager.Instance.ShowPopup("No port selected for router IP update!!!\n Must select a port to edit IP");
            return;
        }

        NetworkManager.Instance.UpdatePortIP(
            currentDeviceGuid,
            currentPortIndex,
            newIP
        );

        RoutingTableManager.Instance.RegisterConnectedRoute(
            currentDeviceGuid,
            currentPortIndex,
            newIP
        );
        //gameObject.SetActive(false);
    }

    public void OnCancel()
    {
        gameObject.SetActive(false);
    }
} 