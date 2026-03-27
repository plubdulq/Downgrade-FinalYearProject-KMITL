using UnityEngine;
using TMPro;

public class VRKeyboardSpawner : MonoBehaviour
{
    public static VRKeyboardSpawner Instance;

    public GameObject keyboardPrefab;
    public Transform spawnPoint; // ตรงหน้ากล้อง

    private GameObject currentKeyboard;

    void Awake()
    {
        Instance = this;
    }

    public void OpenKeyboard(TMP_InputField targetField)
    {
        // ถ้ามี keyboard อยู่แล้ว ลบทิ้งก่อน
        if (currentKeyboard != null)
        {
            Destroy(currentKeyboard);
        }

        // spawn
        currentKeyboard = Instantiate(keyboardPrefab, spawnPoint.position, spawnPoint.rotation);

        // bind target
        VRKeyboard keyboard = currentKeyboard.GetComponent<VRKeyboard>();
        keyboard.SetTarget(targetField);

        // preload text
        if (!string.IsNullOrEmpty(targetField.text))
        {
            keyboard.SetText(targetField.text);
        }
        else
        {
            keyboard.SetPlaceholder("Enter Text");
        }
    }
}