using UnityEngine;

public class UICameraManager : MonoBehaviour
{
    public static UICameraManager Instance;

    public Transform rightHandPointer;

    [Header("UI Camera")]
    public Camera uiEventCamera;

    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        uiEventCamera = rightHandPointer.GetComponentInChildren<Camera>(true);
        //Debug.Log($"UICameraManager initialized with camera: {uiEventCamera.name}");
    }

    public Camera GetUIEventCamera()
    {
        return uiEventCamera;
    }
}