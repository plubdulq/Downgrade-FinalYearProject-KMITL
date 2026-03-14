using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlacementPreview : MonoBehaviour
{
    public static PlacementPreview Instance;
    
    // [SerializeField]
    // private GameObject mouseIndicator;
    [SerializeField]
    private InputManager inputManager;
    [SerializeField]
    private Material greenMat;
    [SerializeField]
    private Material redMat;
    [SerializeField]
    private float gridSize;

    //KEYBIND PLACING
    [SerializeField] private InputActionReference placeObjectAction;

    //FIX INSPECT UI
    [SerializeField] private Transform rightHandPointer;
    private Camera uiEventCamera;

    private GameObject currentPreview;   // ตัว preview ตอนลาก
    private GameObject selectedPrefab;   // prefab ที่จะสร้างจริง
    private bool isPlacing = false;      // กำลังอยู่ในโหมด placement ไหม

    void Start()
    {
        uiEventCamera = rightHandPointer.GetComponentInChildren<Camera>(true);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        if (!isPlacing) return;

        // ใช้ InputManager หาตำแหน่ง mouse
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        mousePosition.x = Mathf.Round(mousePosition.x / gridSize) * gridSize;
        mousePosition.y = Mathf.Round(mousePosition.y / gridSize) * gridSize;
        mousePosition.z = Mathf.Round(mousePosition.z / gridSize) * gridSize;
        currentPreview.transform.position = mousePosition;
        //Debug.Log("Preview Position: " + currentPreview.transform.position);

        // ==== Check Collision ====
        Collider col = currentPreview.GetComponentInChildren<Collider>();
        if (col != null)
        {
            Vector3 halfExtents = col.bounds.extents;
            Vector3 center = col.bounds.center;

            Collider[] overlaps = Physics.OverlapBox(center, halfExtents, currentPreview.transform.rotation,~0);
            bool isColliding = false;
            Renderer[] allRenderers = currentPreview.GetComponentsInChildren<Renderer>();
            foreach (Collider c in overlaps)
            {
                if (!c.transform.IsChildOf(currentPreview.transform) && c.gameObject.name != "Plane")
                {
                    //Renderer[] allRenderers = currentPreview.GetComponentsInChildren<Renderer>();
                    foreach (Renderer rend in allRenderers)
                    {
                        rend.material = redMat;
                    }
                    isColliding = true;
                    Debug.Log(currentPreview.gameObject.name + "--> colliding with: " + c.gameObject.name);
                    break;
                }

                foreach (Renderer rend in allRenderers)
                {
                    rend.material = greenMat;
                }
            }

            var renderer = currentPreview.GetComponentInChildren<Renderer>();
            renderer.material = isColliding ? redMat : greenMat;

            // ถ้าคลิกซ้าย → วางจริง
            if (placeObjectAction.action.WasPerformedThisFrame() && !isColliding)
            {
                Debug.Log("Placing Object");
                PlaceObject();
            }
        }
    }
    
    public async Task BeginPlacement(GameObject prefab)
    {
        Vector3 fixedPos = new Vector3(-395.679f, 1f, 79f);
        if (currentPreview != null) Destroy(currentPreview);

        selectedPrefab = prefab;
        currentPreview = Instantiate(selectedPrefab, fixedPos, Quaternion.identity);

        //FIX INSPECT UI
        Canvas[] canvases = currentPreview.GetComponentsInChildren<Canvas>(true);

        foreach (var canvas in canvases)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = uiEventCamera;

            Debug.Log("Assigned camera to: " + canvas.name);
        }
        currentPreview = null;
    
        //currentPreview = Instantiate(selectedPrefab, inputManager.GetSelectedMapPosition(), Quaternion.identity);
        //isPlacing = true;
    }

    public async Task GhostObject(GameObject prefab)
    {
        if (currentPreview != null) Destroy(currentPreview);

        selectedPrefab = prefab;
        currentPreview = Instantiate(selectedPrefab, inputManager.GetSelectedMapPosition(), Quaternion.identity);
        isPlacing = true;
    }

    // วางจริง
    private void PlaceObject()
    {
        GameObject placed = Instantiate(selectedPrefab, currentPreview.transform.position, currentPreview.transform.rotation);
        placed.GetComponentInChildren<Renderer>().material = greenMat;
        
        Destroy(currentPreview);
        currentPreview = null;
        isPlacing = false;
    }

}
