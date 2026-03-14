using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.InputSystem;

public class Keybind : MonoBehaviour
{
    public GameObject StorePrefab;
    public Transform cameraTransform;
    [SerializeField] private Transform rightHandPointer;
    private Camera uiEventCamera;

    public float spawnDistance = 2.0f;

    private GameObject currentStoreInstance;
    [SerializeField] private InputActionReference openStoreAction;

    void Start()
    {
        uiEventCamera = rightHandPointer.GetComponentInChildren<Camera>(true);
    }

    void Update()
        {
            StoreOpen();
        }
    void StoreOpen()
        {
            //if (Input.GetKeyDown(KeyCode.B))
            if (openStoreAction.action.WasPerformedThisFrame())
            {
                if (currentStoreInstance != null)
                {
                    Destroy(currentStoreInstance);
                    return;
                }
                Debug.Log("B key pressed");
                ToggleStoreUI();
            }
        }
    void ToggleStoreUI()
        {
            Vector3 spawnPos = cameraTransform.position + cameraTransform.forward * spawnDistance;

            spawnPos.y = cameraTransform.position.y;

            currentStoreInstance = Instantiate(StorePrefab, spawnPos, Quaternion.identity);

            // 🔥 FIX หลัก
            var canvas =
                currentStoreInstance.GetComponentInChildren<Canvas>(true);

            if (canvas != null)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = uiEventCamera;
            }
            
            Vector3 lookDir = currentStoreInstance.transform.position - cameraTransform.position;
            lookDir.y = 0;
            currentStoreInstance.transform.rotation = Quaternion.LookRotation(lookDir);
        }
}