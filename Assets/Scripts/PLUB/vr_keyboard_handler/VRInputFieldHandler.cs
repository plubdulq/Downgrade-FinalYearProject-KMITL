using UnityEngine;
using TMPro;

public class VRInputFieldHandler : MonoBehaviour
{
    private TMP_InputField inputField;

    void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
    }

    public void OnClick()
    {
        VRKeyboardSpawner.Instance.OpenKeyboard(inputField);
    }
}