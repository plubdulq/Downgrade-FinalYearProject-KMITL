using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FirewallRuleCardUI : MonoBehaviour
{
    public TMP_Text sourceText;
    public TMP_Text destinationText;
    public TMP_Text portText;
    public TMP_Text protocolText;
    public TMP_Text actionText;

    public Button deleteButton;

    private FirewallRule boundRule;
    private FirewallUIManager parentUI;

    public void Bind(FirewallRule rule, FirewallUIManager ui)
    {
        boundRule = rule;
        parentUI = ui;

        sourceText.text = rule.sourceIP;
        destinationText.text = rule.destinationIP;
        portText.text = rule.port;
        protocolText.text = rule.protocol;
        actionText.text = rule.action;

        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(OnDeleteClicked);
    }

    private void OnDeleteClicked()
    {
        parentUI.RemoveRule(boundRule);
    }
}