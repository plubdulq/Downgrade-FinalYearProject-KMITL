using UnityEngine;

public class CameraUIHandler : MonoBehaviour
{
    private Canvas canvas;
    private Camera uiEventCamera;

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        //uiEventCamera = rightHandPointer.GetComponentInChildren<Camera>(true);
    }

    void Start()
    {
        Setup();
    }

    public void Setup()
    {
        if (canvas == null)
        {
            canvas = GetComponentInChildren<Canvas>(true);
            if (canvas == null)
            {
                Debug.LogError("Canvas component not found in children.");
                return;
            }
        }

        canvas.renderMode = RenderMode.WorldSpace;
        //canvas.worldCamera = uiEventCamera;
        canvas.worldCamera = UICameraManager.Instance.GetUIEventCamera();
    }
}