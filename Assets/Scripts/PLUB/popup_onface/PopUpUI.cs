using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PopupUI : MonoBehaviour
{
    public TextMeshProUGUI messageText;
    public Button okButton;

    private void Start()
    {
        okButton.onClick.AddListener(Close);
    }

    public void SetMessage(string message)
    {
        messageText.text = message;
    }

    void Close()
    {
        AlertPopupManager.Instance.ClosePopup();
    }
}