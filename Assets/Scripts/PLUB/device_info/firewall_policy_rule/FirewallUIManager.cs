using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class FirewallUIManager : MonoBehaviour
{
    [Header("Target Device")]
    public string deviceGuid;

    [Header("Rule Card")]
    public Transform cardContainer;
    public GameObject ruleCardPrefab;

    [Header("Policy Card")]
    public Transform policyContainer;
    public GameObject policyCardPrefab;

    public TMP_Text policyNameInput;

    // toggle mode ตรงนี้
    [SerializeField] private bool autoGenerateName = true;
    //private int policyCounter = 1;

    private List<GameObject> spawnedCards = new List<GameObject>();

    private FirewallPolicy currentPolicy;

    //To Get Current Guid
    public GameObject targetDevice;

    // =========================
    // INIT (ตอนเปิด UI)
    // =========================
    public void Init(string guid)
    {
        deviceGuid = targetDevice.GetComponent<NetworkDevice>().guid;

        FirewallManager.Instance.SetCurrentDevice(deviceGuid);

        RefreshPolicyCards();
    }

    // =========================
    // POLICY
    // =========================
    public void SelectPolicy(FirewallPolicy policy)
    {
        currentPolicy = policy;

        RefreshRuleCards();
    }

    // =========================
    // RULE UI
    // =========================
    public void RefreshRuleCards()
    {
        ClearCards();

        if (currentPolicy == null) return;

        foreach (var rule in currentPolicy.rules)
        {
            GameObject card = Instantiate(ruleCardPrefab, cardContainer);

            var ui = card.GetComponent<FirewallRuleCardUI>();
            ui.Bind(rule, this); // 🔥 ส่ง context เข้าไป

            spawnedCards.Add(card);
        }
    }

    private void ClearCards()
    {
        foreach (var card in spawnedCards)
        {
            Destroy(card);
        }

        spawnedCards.Clear();
    }

    // =========================
    // RULE ACTION
    // =========================
    public void AddRule(string source, string dest, string port, string protocol, string action)
    {
        if (currentPolicy == null) return;

        var rule = new FirewallRule(source, dest, port, protocol, action);
        currentPolicy.AddRule(rule);

        RefreshRuleCards();
    }

    public void RemoveRule(FirewallRule rule)
    {
        if (currentPolicy == null) return;

        currentPolicy.rules.Remove(rule);

        RefreshRuleCards();
    }

    public void RefreshPolicyCards()
    {
        foreach (Transform child in policyContainer)
        {
            Destroy(child.gameObject);
        }

        var device = FirewallManager.Instance.GetCurrentDevice();

        foreach (var policy in device.policies)
        {
            GameObject card = Instantiate(policyCardPrefab, policyContainer);

            var ui = card.GetComponent<PolicyCardUI>();
            ui.Init(policy, this);
        }
    }

    public void CreatePolicyFromUI()
    {
        string policyName;

        if (autoGenerateName)
        {
            int count = FirewallManager.Instance.GetPolicies(deviceGuid).Count + 1;
            policyName = $"Policy {count}";
        }
        else
        {
            if (string.IsNullOrEmpty(policyNameInput.text))
            {
                Debug.LogWarning("Policy name empty");
                return;
            }

            policyName = policyNameInput.text;
        }

        // 🔥 ระบุ device ชัดเจน
        FirewallManager.Instance.CreatePolicy(deviceGuid, policyName);

        RefreshPolicyCards();

        if (!autoGenerateName)
        {
            policyNameInput.text = "";
        }
    }
}