using UnityEngine;
using TMPro;

public class FirewallInputUI : MonoBehaviour
{
    public FirewallUIManager parentUI;

    public TMP_Text sourceInput;
    public TMP_Text destinationInput;
    public TMP_Text portInput;

    public TMP_Dropdown protocolDropdown;
    public TMP_Dropdown actionDropdown;

    public void OnClickApply()
    {
        string source = sourceInput.text;
        string destination = destinationInput.text;
        string port = portInput.text;

        string protocol = protocolDropdown.options[protocolDropdown.value].text;
        string action = actionDropdown.options[actionDropdown.value].text;

        parentUI.AddRule(source, destination, port, protocol, action);
        
        ClearInputs();
    }

    private void ClearInputs()
    {
        sourceInput.text = "";
        destinationInput.text = "";
        portInput.text = "";
    }
}