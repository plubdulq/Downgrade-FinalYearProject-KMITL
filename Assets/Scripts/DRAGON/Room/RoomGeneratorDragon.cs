using UnityEngine;

public class RoomGeneratorDragon : MonoBehaviour
{
    [System.Serializable]
    public class RoomConfig
    {
        public float width;   // X
        public float length;  // Z
        public float height;  // Y
        public RoomPreset.RoomShape shape;
        public InteriorContent interiorContent;
    }

    [Header("Debug / Wizard Hook")]
    public RoomPreset debugPreset;
    public InteriorContent debugInterior = InteriorContent.EmptyRoom;

    // ฟังก์ชันนี้จะถูกเรียกจากปุ่ม
    public void GenerateDebugRoom()
    {
        Debug.Log("[RoomGenerator] GenerateDebugRoom() CLICKED");

        if (debugPreset == null)
        {
            Debug.LogWarning("[RoomGenerator] debugPreset is null");
            return;
        }

        GenerateFromPreset(debugPreset, debugInterior);
    }


    [Header("Size Limits (meters)")]
    public float minWidth = 3f;
    public float maxWidth = 20f;

    public float minLength = 3f;
    public float maxLength = 20f;

    public float minHeight = 2.5f;
    public float maxHeight = 5f;

    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject ceilingPrefab;

    [Header("Parent / Origin")]
    public Transform roomParent;        // เช่น RoomRoot
    public bool autoCenterAtOrigin = true;

    private GameObject currentRoomRoot;
    private RoomConfig currentConfig;

    // =============================
    //  PUBLIC ENTRY POINTS
    // =============================

    // ใช้ตอนสร้างจาก RoomPreset + ตัวเลือก “ภายในห้อง”
    public void GenerateFromPreset(
        RoomPreset preset,
        InteriorContent interior,
        float? overrideWidth = null,
        float? overrideLength = null,
        float? overrideHeight = null)
    {
        if (preset == null)
        {
            Debug.LogWarning("[RoomGenerator] Preset is null");
            return;
        }

        RoomConfig config = new RoomConfig
        {
            width = overrideWidth ?? preset.width,
            length = overrideLength ?? preset.length,
            height = overrideHeight ?? preset.height,
            shape = preset.shape,
            interiorContent = interior
        };

        GenerateRoom(config);
    }

    // ใช้ตอนโหลดจาก RoomSaveData (จากหน้า My Server Room)
    public void GenerateFromSaveData(RoomSaveData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[RoomGenerator] SaveData is null");
            return;
        }

        RoomConfig config = new RoomConfig
        {
            width = data.width,
            length = data.length,
            height = data.height,
            shape = data.shape,
            interiorContent = data.interiorContent
        };

        GenerateRoom(config);
    }

    // ลบห้องเก่า
    public void ClearRoom()
    {
        if (currentRoomRoot != null)
        {
#if UNITY_EDITOR
            DestroyImmediate(currentRoomRoot);
#else
            Destroy(currentRoomRoot);
#endif
            currentRoomRoot = null;
        }
    }

    // =============================
    //  INTERNAL GENERATION LOGIC
    // =============================

    private void GenerateRoom(RoomConfig config)
    {
        if (config == null) return;

        config.width = Mathf.Clamp(config.width, minWidth, maxWidth);
        config.length = Mathf.Clamp(config.length, minLength, maxLength);
        config.height = Mathf.Clamp(config.height, minHeight, maxHeight);

        currentConfig = config;

        ClearRoom();

        currentRoomRoot = new GameObject("ProceduralRoom");
        if (roomParent != null)
        {
            currentRoomRoot.transform.SetParent(roomParent, false);
        }

        if (autoCenterAtOrigin)
        {
            currentRoomRoot.transform.localPosition = Vector3.zero;
            currentRoomRoot.transform.localRotation = Quaternion.identity;
        }

        switch (config.shape)
        {
            case RoomPreset.RoomShape.Rectangle:
                BuildRectangleRoom(config);
                break;

            case RoomPreset.RoomShape.LShape:
            case RoomPreset.RoomShape.Modular:
                Debug.Log("[RoomGenerator] L-Shape/Modular not implemented yet, using rectangle bounding.");
                BuildRectangleRoom(config);
                break;
        }
        // PlaceRoomForPreview(currentRoomRoot.transform, config, 1.0f, 1.6f);
        FocusCameraOutsideRoom(currentRoomRoot.transform, config);



        Debug.Log($"[RoomGenerator] Generated room {config.width} x {config.length} x {config.height} (m) " +
                  $"Shape={config.shape}, Interior={config.interiorContent}");
    }

    private void BuildRectangleRoom(RoomConfig config)
    {
        float halfW = config.width / 2f;
        float halfL = config.length / 2f;
        float h = config.height;

        // Floor
        if (floorPrefab != null)
        {
            GameObject floor = Instantiate(floorPrefab, currentRoomRoot.transform);
            floor.name = "Floor";
            floor.transform.localPosition = Vector3.zero;
            floor.transform.localRotation = Quaternion.identity;
            floor.transform.localScale = new Vector3(config.width, 1f, config.length);
        }

        // Ceiling
        if (ceilingPrefab != null)
        {
            GameObject ceiling = Instantiate(ceilingPrefab, currentRoomRoot.transform);
            ceiling.name = "Ceiling";
            ceiling.transform.localPosition = new Vector3(0f, h, 0f);
            ceiling.transform.localRotation = Quaternion.identity;
            ceiling.transform.localScale = new Vector3(config.width, 1f, config.length);
        }

        // Walls
        if (wallPrefab != null)
        {
            // North (+Z)
            GameObject wallNorth = Instantiate(wallPrefab, currentRoomRoot.transform);
            wallNorth.name = "Wall_North";
            wallNorth.transform.localPosition = new Vector3(0f, h / 2f, halfL);
            wallNorth.transform.localRotation = Quaternion.identity;
            wallNorth.transform.localScale = new Vector3(config.width, h, 0.1f);

            // South (-Z)
            GameObject wallSouth = Instantiate(wallPrefab, currentRoomRoot.transform);
            wallSouth.name = "Wall_South";
            wallSouth.transform.localPosition = new Vector3(0f, h / 2f, -halfL);
            wallSouth.transform.localRotation = Quaternion.identity;
            wallSouth.transform.localScale = new Vector3(config.width, h, 0.1f);

            // East (+X)
            GameObject wallEast = Instantiate(wallPrefab, currentRoomRoot.transform);
            wallEast.name = "Wall_East";
            wallEast.transform.localPosition = new Vector3(halfW, h / 2f, 0f);
            wallEast.transform.localRotation = Quaternion.identity;
            wallEast.transform.localScale = new Vector3(0.1f, h, config.length);

            // West (-X)
            GameObject wallWest = Instantiate(wallPrefab, currentRoomRoot.transform);
            wallWest.name = "Wall_West";
            wallWest.transform.localPosition = new Vector3(-halfW, h / 2f, 0f);
            wallWest.transform.localRotation = Quaternion.identity;
            wallWest.transform.localScale = new Vector3(0.1f, h, config.length);
        }

        // TODO: ภายหลังค่อยต่อ logic วางอุปกรณ์อัตโนมัติ
        // ถ้า config.interiorContent == InteriorContent.SampleEquipments
        // → เรียกระบบ spawn rack/PAC ตัวอย่าง
    }

    private void FocusCameraOutsideRoom(Transform roomRoot, RoomConfig cfg)
    {
        Camera cam = Camera.main;
        if (cam == null) cam = FindFirstObjectByType<Camera>();
        if (cam == null)
        {
            Debug.LogWarning("[RoomGenerator] FocusCameraOutsideRoom: No camera found.");
            return;
        }

        // ถ้ามี OVRCameraRig ให้ขยับทั้ง rig
        Transform rig = null;
        var ovr = FindFirstObjectByType<OVRCameraRig>();
        if (ovr != null) rig = ovr.transform;
        if (rig == null) rig = cam.transform.parent != null ? cam.transform.parent : cam.transform;

        // สร้าง bounds ของห้องจากค่าที่เราสร้างจริง
        Bounds b = new Bounds(
            roomRoot.position + new Vector3(0f, cfg.height * 0.5f, 0f),
            new Vector3(cfg.width, cfg.height, cfg.length)
        );

        float padding = 2.0f; // เพิ่มระยะเผื่อ
        Vector3 size = b.size + Vector3.one * padding;

        // ใช้ radius เพื่อให้กล้องถอยพอที่จะเห็นทั้งก้อน
        float vFov = Mathf.Max(30f, cam.fieldOfView) * Mathf.Deg2Rad;
        float radius = 0.5f * size.magnitude;
        float dist = radius / Mathf.Sin(vFov * 0.5f);

        // วางกล้องแบบเฉียง จะเห็นห้องง่ายกว่า “ถอยตรงๆ”
        Vector3 dir = new Vector3(-1f, 0.35f, -1f).normalized;
        Vector3 pos = b.center + dir * dist;

        // กันต่ำเกินไป
        pos.y = Mathf.Max(1.6f, cfg.height * 0.8f);

        rig.position = pos;

        // VR มักโดน head tracking แย่ง rotation → เราหมุนเฉพาะแกน Y ก็พอ
        Vector3 look = (b.center - pos);
        look.y = 0f;
        if (look.sqrMagnitude > 0.001f)
            rig.rotation = Quaternion.LookRotation(look.normalized, Vector3.up);

        // กันห้องยาวแล้วโดน far clip ตัด (สำคัญมากถ้าขยายใหญ่)
        cam.farClipPlane = Mathf.Max(cam.farClipPlane, dist + 200f);
    }



    private void PlaceRoomForPreview(Transform roomRoot, RoomConfig cfg, float insideBuffer = 0.8f, float headHeight = 1.6f)
    {
        Camera cam = Camera.main;
        if (cam == null) cam = FindFirstObjectByType<Camera>();
        if (cam == null)
        {
            Debug.LogWarning("[RoomGenerator] PlaceRoomForPreview: No camera found.");
            return;
        }

        float halfL = cfg.length * 0.5f;

        // ให้กล้องอยู่ "ข้างใน" ใกล้ประตู/ขอบห้อง แต่ไม่ชนผนัง
        float forwardOffset = Mathf.Max(halfL - insideBuffer, 0.5f);

        roomRoot.position =
            cam.transform.position
            + cam.transform.forward * forwardOffset
            - Vector3.up * headHeight;

        roomRoot.rotation = Quaternion.Euler(0f, cam.transform.eulerAngles.y, 0f);

        Debug.Log($"[RoomGenerator] Preview inside room: cam={cam.name}, forwardOffset={forwardOffset}, roomPos={roomRoot.position}");
    }



}
