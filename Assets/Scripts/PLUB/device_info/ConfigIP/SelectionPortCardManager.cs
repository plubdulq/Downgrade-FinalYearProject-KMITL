using UnityEngine;

public class SelectionPortCardManager : MonoBehaviour
{
    public static SelectionPortCardManager Instance;

    public string currentDeviceGuid;
    public int currentPortIndex;

    void Awake()
    {
        Instance = this;
    }

    public void SelectPort(string guid, int index)
    {
        currentDeviceGuid = guid;
        currentPortIndex = index;

        Debug.Log($"Selected {guid} port {index}");

        // แจ้ง UI ตัวอื่น
        MyPortDetailUI.Instance?.Refresh();
    }
}