using UnityEngine;

public class PolicyCardUI : MonoBehaviour
{
    private FirewallPolicy policy;
    private FirewallUIManager parentUI;

    public void Init(FirewallPolicy p, FirewallUIManager ui)
    {
        policy = p;
        parentUI = ui;
    }

    public void OnClick()
    {
        parentUI.SelectPolicy(policy);
    }
}