using UnityEngine;
using TMPro;

public class AlertPopupManager : MonoBehaviour
{
    public static AlertPopupManager Instance;

    public GameObject popupPrefab;
    public Transform playerCamera;

    private GameObject currentPopup;

    void Awake()
    {
        Instance = this;
    }

    public void ShowPopup(string message)
    {
        if (currentPopup != null)
            Destroy(currentPopup);

        currentPopup = Instantiate(popupPrefab);

        // 📍 วางหน้ากล้อง
        Transform cam = playerCamera;

        // 📍 parent กับกล้อง
        Vector3 spawnPos = cam.position + cam.forward * 0.8f;

        // 📍 วางใน world
        currentPopup.transform.position = spawnPos;

        // 📍 หันเข้าหากล้อง
        currentPopup.transform.rotation = Quaternion.LookRotation(spawnPos - cam.position);

        // 📝 ใส่ข้อความ
        var text = currentPopup.GetComponentInChildren<TextMeshProUGUI>();
        text.text = message;
    }

    public void ClosePopup()
    {
        if (currentPopup != null)
            Destroy(currentPopup);
    }
}