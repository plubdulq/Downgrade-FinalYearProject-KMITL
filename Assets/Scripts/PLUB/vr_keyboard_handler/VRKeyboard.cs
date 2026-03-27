using UnityEngine;
using TMPro;

public class VRKeyboard : MonoBehaviour
{
    public TMP_Text previewText;

    private TMP_InputField targetField;
    private string currentText = "";

    public void SetTarget(TMP_InputField field)
    {
        targetField = field;
    }

    public void SetText(string text)
    {
        currentText = text;
        previewText.text = currentText;
    }

    public void SetPlaceholder(string placeholder)
    {
        currentText = "";
        previewText.text = placeholder;
    }

    // ปุ่มกดตัวอักษร
    public void Type(string character)
    {
        currentText += character;
        previewText.text = currentText;
    }

    public void Backspace()
    {
        if (currentText.Length > 0)
        {
            currentText = currentText.Substring(0, currentText.Length - 1);
            previewText.text = currentText;
        }
    }

    public void Apply()
    {
        if (targetField != null)
        {
            targetField.text = currentText;
        }

        Close();
    }

    public void Close()
    {
        Destroy(gameObject);
    }
}