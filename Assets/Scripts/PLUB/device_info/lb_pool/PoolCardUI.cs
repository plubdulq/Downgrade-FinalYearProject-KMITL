using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PoolCardUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text poolNameText;
    public TMP_Text algoText;

    public Transform serverList;        // container ของ server
    public GameObject serverCardPrefab; // prefab card
    public LayoutElement layoutElement;
    public float collapsedHeight = 40f;
    public float expandedHeight = 250f;
 
    [Header("Dropdown")]
    public GameObject dropdownContent;  // panel ที่จะ show/hide

    private int serverCount = 0;

    private bool isExpanded = false;

    public void Setup(PoolConfig pool)
    {
        serverCount = 0;
        poolNameText.text = pool.poolName;
        algoText.text = pool.algorithm.ToString();

        // 👉 เริ่มต้น: ซ่อน dropdown
        dropdownContent.SetActive(false);
        isExpanded = false;

        // clear ของเก่า
        foreach (Transform child in serverList)
        {
            Destroy(child.gameObject);
        }

        // สร้าง server list
        foreach (var guid in pool.serverGuids)
        {
            var device = NetworkManager.Instance.GetDeviceByGuid(guid);
            if (device == null) continue;

            GameObject card = Instantiate(serverCardPrefab, serverList);
            serverCount++;

            // ✅ ไม่ใช้ ServerCardUI แล้ว → set text ตรงๆ
            TMP_Text text = card.GetComponentInChildren<TMP_Text>();

            if (text != null)
                //text.text = device.deviceName;
                text.text = $"({device.device_type} - {device.guid})";
        }
    }

    public void ToggleDropdown()
    {
        isExpanded = !isExpanded;

        dropdownContent.SetActive(isExpanded);

        if (isExpanded)
        {
            // 🔥 คำนวณจำนวนแถว (2 ตัวต่อแถว)
            int rowCount = Mathf.CeilToInt(serverCount / 2f);

            // 🔥 ความสูง = จำนวนแถว * 40
            float dynamicHeight = rowCount * 40f;

            layoutElement.preferredHeight = collapsedHeight + dynamicHeight;
        }
        else
        {
            layoutElement.preferredHeight = collapsedHeight;
        }

        // 🔥 force layout update
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent);
    }
}