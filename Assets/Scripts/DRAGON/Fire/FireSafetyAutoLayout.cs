using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FireSafetyAutoLayout : MonoBehaviour
{
    [Header("Anchors")]
    [SerializeField] private Transform roomCenter;
    [SerializeField] private Transform smokeDetectorsRoot;
    [SerializeField] private Transform dischargeNozzlesRoot;

    [Header("Optional Geometry Roots")]
    [SerializeField] private Transform floorsTransform;
    [SerializeField] private Transform ceilingsTransform;

    [Header("Prefabs")]
    [SerializeField] private GameObject smokeDetectorPrefab;
    [SerializeField] private GameObject dischargeNozzlePrefab;

    [Header("Room Size Fallback (meters)")]
    [SerializeField] private float fallbackRoomWidth = 5f;
    [SerializeField] private float fallbackRoomLength = 5f;

    [Header("Y Placement")]
    [SerializeField] private float fallbackCeilingBottomLoc = 2.75f;

    [Tooltip("ค่าชดเชย Y ของ Smoke Detector จากใต้เพดานโลกจริง (+ = สูงขึ้น, - = ต่ำลง)")]
    [SerializeField] private float smokeDetectorYOffset = -0.12f;

    [Tooltip("ค่าชดเชย Y ของ Discharge Nozzle จากใต้เพดานโลกจริง (+ = สูงขึ้น, - = ต่ำลง)")]
    [SerializeField] private float dischargeNozzleYOffset = -0.08f;

    [Header("Placement Rules")]
    [SerializeField] private float minWallOffset = 0.8f;
    [SerializeField] private float minSmokeToNozzleDistance = 1f;

    [Header("Device Rotation")]
    [SerializeField] private Vector3 smokeDetectorLocalEuler = new Vector3(-90f, 0f, 0f);
    [SerializeField] private Vector3 dischargeNozzleLocalEuler = new Vector3(90f, 0f, 0f);

    [Header("Runtime")]
    [SerializeField] private bool applyOnStart = true;
    [SerializeField] private bool clearOldChildrenBeforeSpawn = true;
    [SerializeField] private bool debugLogs = true;

    [Header("Runtime Debug (Read Only)")]
    [SerializeField] private float resolvedRoomWidth;
    [SerializeField] private float resolvedRoomLength;
    [SerializeField] private float resolvedRoomArea;
    [SerializeField] private float resolvedFloorCenterX;
    [SerializeField] private float resolvedFloorCenterZ;
    [SerializeField] private float resolvedUsableHalfX;
    [SerializeField] private float resolvedUsableHalfZ;
    [SerializeField] private float resolvedCeilingBottomY;

    private const float SmokeCoverageSqm = 64f;
    private const float NozzleCoverageSqm = 30f;

    private struct FloorFrame
    {
        public Vector3 center;
        public float usableHalfX;
        public float usableHalfZ;
    }

    private void Start()
    {
        if (applyOnStart)
            RebuildLayout();
    }

    [ContextMenu("Rebuild Layout")]
    public void RebuildLayout()
    {
        if (!ValidateRequiredRefs())
            return;

        if (clearOldChildrenBeforeSpawn)
        {
            ClearChildren(smokeDetectorsRoot);
            ClearChildren(dischargeNozzlesRoot);
        }

        Vector2 roomSize = ResolveRoomSizeFromSceneNameOrFallback();
        float roomWidth = roomSize.x;
        float roomLength = roomSize.y;
        float roomArea = roomWidth * roomLength;

        resolvedRoomWidth = roomWidth;
        resolvedRoomLength = roomLength;
        resolvedRoomArea = roomArea;

        int smokeCount = CalculateSmokeDetectorCount(roomArea);
        int nozzleCount = CalculateDischargeNozzleCount(roomArea);

        FloorFrame frame = ResolveFloorFrame(roomWidth, roomLength);
        float ceilingBottomY = ResolveCeilingBottomY();

        resolvedFloorCenterX = frame.center.x;
        resolvedFloorCenterZ = frame.center.z;
        resolvedUsableHalfX = frame.usableHalfX;
        resolvedUsableHalfZ = frame.usableHalfZ;
        resolvedCeilingBottomY = ceilingBottomY;

        List<Vector2> smokeOffsets = BuildSmokeOffsets(frame.usableHalfX, frame.usableHalfZ, smokeCount);
        List<Vector2> nozzleOffsets = BuildNozzleOffsets(frame.usableHalfX, frame.usableHalfZ, nozzleCount);

        EnforceMinDistanceBetweenSmokeAndNozzle(
            smokeOffsets,
            nozzleOffsets,
            frame.usableHalfX,
            frame.usableHalfZ,
            minSmokeToNozzleDistance
        );

        SpawnSmokeDetectors(smokeOffsets, frame.center, ceilingBottomY);
        SpawnDischargeNozzles(nozzleOffsets, frame.center, ceilingBottomY);

        Log(
            $"RebuildLayout complete | Scene='{SceneManager.GetActiveScene().name}' | " +
            $"Room={roomWidth:0.##} x {roomLength:0.##} m | Area={roomArea:0.##} sqm | " +
            $"Smoke={smokeCount} | Nozzle={nozzleCount} | " +
            $"FloorCenter=({frame.center.x:0.###}, {frame.center.z:0.###}) | " +
            $"UsableHalf=({frame.usableHalfX:0.###}, {frame.usableHalfZ:0.###}) | " +
            $"CeilingBottomY={ceilingBottomY:0.###}"
        );
    }

    private bool ValidateRequiredRefs()
    {
        if (roomCenter == null)
        {
            LogWarning("RoomCenter is missing.");
            return false;
        }

        if (smokeDetectorsRoot == null)
        {
            LogWarning("SmokeDetectorsRoot is missing.");
            return false;
        }

        if (dischargeNozzlesRoot == null)
        {
            LogWarning("DischargeNozzlesRoot is missing.");
            return false;
        }

        if (smokeDetectorPrefab == null)
        {
            LogWarning("Smoke Detector Prefab is missing.");
            return false;
        }

        if (dischargeNozzlePrefab == null)
        {
            LogWarning("Discharge Nozzle Prefab is missing.");
            return false;
        }

        return true;
    }

    private Vector2 ResolveRoomSizeFromSceneNameOrFallback()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (TryParseRoomSizeFromSceneName(sceneName, out int parsedWidth, out int parsedLength))
        {
            Log($"Parsed room size from scene name '{sceneName}' => {parsedWidth} x {parsedLength} m");
            return new Vector2(parsedWidth, parsedLength);
        }

        LogWarning(
            $"Could not parse room size from scene name '{sceneName}'. " +
            $"Using fallback size {fallbackRoomWidth} x {fallbackRoomLength} m"
        );

        return new Vector2(Mathf.Max(1f, fallbackRoomWidth), Mathf.Max(1f, fallbackRoomLength));
    }

    private bool TryParseRoomSizeFromSceneName(string sceneName, out int width, out int length)
    {
        width = 0;
        length = 0;

        if (string.IsNullOrWhiteSpace(sceneName))
            return false;

        StringBuilder digitsOnly = new StringBuilder();
        foreach (char c in sceneName)
        {
            if (char.IsDigit(c))
                digitsOnly.Append(c);
        }

        string digits = digitsOnly.ToString();
        if (string.IsNullOrEmpty(digits))
            return false;

        if (digits.Length == 2)
        {
            int a = digits[0] - '0';
            int b = digits[1] - '0';

            if (IsSupportedRoomDimension(a) && IsSupportedRoomDimension(b))
            {
                width = a;
                length = b;
                return true;
            }
        }

        if (digits.Length == 3)
        {
            if (int.TryParse(digits.Substring(0, 1), out int a) &&
                int.TryParse(digits.Substring(1, 2), out int b) &&
                IsSupportedRoomDimension(a) && IsSupportedRoomDimension(b))
            {
                width = a;
                length = b;
                return true;
            }

            if (int.TryParse(digits.Substring(0, 2), out int c) &&
                int.TryParse(digits.Substring(2, 1), out int d) &&
                IsSupportedRoomDimension(c) && IsSupportedRoomDimension(d))
            {
                width = c;
                length = d;
                return true;
            }
        }

        if (digits.Length == 4)
        {
            if (int.TryParse(digits.Substring(0, 2), out int a) &&
                int.TryParse(digits.Substring(2, 2), out int b) &&
                IsSupportedRoomDimension(a) && IsSupportedRoomDimension(b))
            {
                width = a;
                length = b;
                return true;
            }
        }

        for (int split = 1; split < digits.Length; split++)
        {
            string left = digits.Substring(0, split);
            string right = digits.Substring(split);

            if (!int.TryParse(left, out int a) || !int.TryParse(right, out int b))
                continue;

            if (IsSupportedRoomDimension(a) && IsSupportedRoomDimension(b))
            {
                width = a;
                length = b;
                return true;
            }
        }

        return false;
    }

    private bool IsSupportedRoomDimension(int value)
    {
        return value >= 5 && value <= 10;
    }

    private int CalculateSmokeDetectorCount(float areaSqm)
    {
        return areaSqm <= SmokeCoverageSqm ? 1 : 2;
    }

    private int CalculateDischargeNozzleCount(float areaSqm)
    {
        return Mathf.Clamp(Mathf.CeilToInt(areaSqm / NozzleCoverageSqm), 1, 4);
    }

    private FloorFrame ResolveFloorFrame(float roomWidth, float roomLength)
    {
        Bounds floorBounds;
        if (TryResolveFloorBounds(out floorBounds))
        {
            float usableHalfX = Mathf.Max(0.1f, floorBounds.extents.x - minWallOffset);
            float usableHalfZ = Mathf.Max(0.1f, floorBounds.extents.z - minWallOffset);

            FloorFrame frame = new FloorFrame
            {
                center = new Vector3(floorBounds.center.x, roomCenter.position.y, floorBounds.center.z),
                usableHalfX = usableHalfX,
                usableHalfZ = usableHalfZ
            };

            Log(
                $"Resolved floor frame from bounds | center=({frame.center.x:0.###}, {frame.center.z:0.###}) " +
                $"| extents=({floorBounds.extents.x:0.###}, {floorBounds.extents.z:0.###}) " +
                $"| usableHalf=({usableHalfX:0.###}, {usableHalfZ:0.###})"
            );

            return frame;
        }

        FloorFrame fallback = new FloorFrame
        {
            center = new Vector3(roomCenter.position.x, roomCenter.position.y, roomCenter.position.z),
            usableHalfX = Mathf.Max(0.1f, roomWidth * 0.5f - minWallOffset),
            usableHalfZ = Mathf.Max(0.1f, roomLength * 0.5f - minWallOffset)
        };

        LogWarning(
            $"Using fallback floor frame from RoomCenter | center=({fallback.center.x:0.###}, {fallback.center.z:0.###}) " +
            $"| usableHalf=({fallback.usableHalfX:0.###}, {fallback.usableHalfZ:0.###})"
        );

        return fallback;
    }

    private bool TryResolveFloorBounds(out Bounds result)
    {
        result = default;

        if (TryGetBoundsFromTransform(floorsTransform, out result))
            return true;

        Transform autoFloors = FindTransformContains("floors");
        if (autoFloors != null && TryGetBoundsFromTransform(autoFloors, out result))
        {
            Log($"Resolved floor bounds from auto-found '{autoFloors.name}'");
            return true;
        }

        Transform autoFloor = FindTransformContains("floor");
        if (autoFloor != null && TryGetBoundsFromTransform(autoFloor, out result))
        {
            Log($"Resolved floor bounds from auto-found '{autoFloor.name}'");
            return true;
        }

        return false;
    }

    private bool TryGetBoundsFromTransform(Transform target, out Bounds combined)
    {
        combined = default;

        if (target == null)
            return false;

        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        bool hasBounds = false;

        foreach (Renderer r in renderers)
        {
            if (r == null)
                continue;

            if (!hasBounds)
            {
                combined = r.bounds;
                hasBounds = true;
            }
            else
            {
                combined.Encapsulate(r.bounds);
            }
        }

        if (hasBounds)
            return true;

        Collider[] colliders = target.GetComponentsInChildren<Collider>(true);
        foreach (Collider c in colliders)
        {
            if (c == null)
                continue;

            if (!hasBounds)
            {
                combined = c.bounds;
                hasBounds = true;
            }
            else
            {
                combined.Encapsulate(c.bounds);
            }
        }

        return hasBounds;
    }

    private Transform FindTransformContains(string keyword)
    {
        Transform[] all = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        for (int i = 0; i < all.Length; i++)
        {
            Transform t = all[i];
            if (t == null)
                continue;

            if (t.name.ToLowerInvariant().Contains(keyword))
                return t;
        }

        return null;
    }

    private float ResolveCeilingBottomY()
    {
        if (TryGetBottomYFromTransform(ceilingsTransform, out float refY, "CeilingsTransform"))
            return refY;

        string[] keywords = { "celings" };
        Transform[] all = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (string keyword in keywords)
        {
            foreach (Transform t in all)
            {
                if (t == null)
                    continue;

                if (!t.name.ToLowerInvariant().Contains(keyword))
                    continue;

                if (TryGetBottomYFromTransform(t, out float autoY, $"auto-found '{t.name}'"))
                    return autoY;
            }
        }

        LogWarning($"Using fallback ceiling bottom Y: {fallbackCeilingBottomLoc:0.###}");
        return fallbackCeilingBottomLoc;
    }

    private bool TryGetBottomYFromTransform(Transform target, out float bottomY, string sourceLabel)
    {
        bottomY = 0f;

        if (target == null)
            return false;

        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        if (renderers != null && renderers.Length > 0)
        {
            float lowestY = float.MaxValue;
            bool found = false;

            foreach (Renderer r in renderers)
            {
                if (r == null)
                    continue;

                lowestY = Mathf.Min(lowestY, r.bounds.min.y);
                found = true;
            }

            if (found)
            {
                bottomY = lowestY;
                Log($"Resolved ceiling bottom Y from {sourceLabel} renderer = {bottomY:0.###}");
                return true;
            }
        }

        Collider[] colliders = target.GetComponentsInChildren<Collider>(true);
        if (colliders != null && colliders.Length > 0)
        {
            float lowestY = float.MaxValue;
            bool found = false;

            foreach (Collider c in colliders)
            {
                if (c == null)
                    continue;

                lowestY = Mathf.Min(lowestY, c.bounds.min.y);
                found = true;
            }

            if (found)
            {
                bottomY = lowestY;
                Log($"Resolved ceiling bottom Y from {sourceLabel} collider = {bottomY:0.###}");
                return true;
            }
        }

        return false;
    }

    private List<Vector2> BuildSmokeOffsets(float usableHalfX, float usableHalfZ, int count)
    {
        List<Vector2> offsets = new List<Vector2>();

        if (count <= 1)
        {
            offsets.Add(Vector2.zero);
            return offsets;
        }

        if (usableHalfX >= usableHalfZ)
        {
            float x = usableHalfX * 0.5f;
            offsets.Add(new Vector2(-x, 0f));
            offsets.Add(new Vector2(+x, 0f));
        }
        else
        {
            float z = usableHalfZ * 0.5f;
            offsets.Add(new Vector2(0f, -z));
            offsets.Add(new Vector2(0f, +z));
        }

        return offsets;
    }

    private List<Vector2> BuildNozzleOffsets(float usableHalfX, float usableHalfZ, int count)
    {
        List<Vector2> offsets = new List<Vector2>();

        float x25 = usableHalfX * 0.5f;
        float z25 = usableHalfZ * 0.5f;
        float x22 = usableHalfX * 0.44f;
        float z22 = usableHalfZ * 0.44f;

        switch (count)
        {
            case 1:
                offsets.Add(Vector2.zero);
                break;

            case 2:
                if (usableHalfX >= usableHalfZ)
                {
                    offsets.Add(new Vector2(-x25, 0f));
                    offsets.Add(new Vector2(+x25, 0f));
                }
                else
                {
                    offsets.Add(new Vector2(0f, -z25));
                    offsets.Add(new Vector2(0f, +z25));
                }
                break;

            case 3:
                if (usableHalfX >= usableHalfZ)
                {
                    offsets.Add(new Vector2(-x22, -z22));
                    offsets.Add(new Vector2(-x22, +z22));
                    offsets.Add(new Vector2(+x22, 0f));
                }
                else
                {
                    offsets.Add(new Vector2(-x22, -z22));
                    offsets.Add(new Vector2(+x22, -z22));
                    offsets.Add(new Vector2(0f, +z22));
                }
                break;

            default:
                offsets.Add(new Vector2(-x25, -z25));
                offsets.Add(new Vector2(-x25, +z25));
                offsets.Add(new Vector2(+x25, -z25));
                offsets.Add(new Vector2(+x25, +z25));
                break;
        }

        return offsets;
    }

    private void EnforceMinDistanceBetweenSmokeAndNozzle(
        List<Vector2> smokeOffsets,
        List<Vector2> nozzleOffsets,
        float usableHalfX,
        float usableHalfZ,
        float minDistance)
    {
        if (smokeOffsets == null || nozzleOffsets == null)
            return;

        for (int i = 0; i < nozzleOffsets.Count; i++)
        {
            Vector2 nozzle = nozzleOffsets[i];

            for (int s = 0; s < smokeOffsets.Count; s++)
            {
                Vector2 smoke = smokeOffsets[s];
                Vector2 delta = nozzle - smoke;
                float dist = delta.magnitude;

                if (dist >= minDistance)
                    continue;

                Vector2 dir = dist > 0.001f ? delta.normalized : Vector2.right;
                nozzle = smoke + dir * minDistance;
                nozzle.x = Mathf.Clamp(nozzle.x, -usableHalfX, usableHalfX);
                nozzle.y = Mathf.Clamp(nozzle.y, -usableHalfZ, usableHalfZ);
            }

            nozzleOffsets[i] = nozzle;
        }
    }

    private void SpawnSmokeDetectors(List<Vector2> offsets, Vector3 floorCenter, float ceilingBottomY)
    {
        for (int i = 0; i < offsets.Count; i++)
        {
            Vector3 worldPos = new Vector3(
                floorCenter.x + offsets[i].x,
                ceilingBottomY + smokeDetectorYOffset,
                floorCenter.z + offsets[i].y
            );

            GameObject go = Instantiate(smokeDetectorPrefab, smokeDetectorsRoot);
            go.name = $"SmokeDetector_{(i + 1):00}";
            go.transform.position = worldPos;
            go.transform.rotation = Quaternion.Euler(smokeDetectorLocalEuler);
        }
    }

    private void SpawnDischargeNozzles(List<Vector2> offsets, Vector3 floorCenter, float ceilingBottomY)
    {
        for (int i = 0; i < offsets.Count; i++)
        {
            Vector3 worldPos = new Vector3(
                floorCenter.x + offsets[i].x,
                ceilingBottomY + dischargeNozzleYOffset,
                floorCenter.z + offsets[i].y
            );

            GameObject go = Instantiate(dischargeNozzlePrefab, dischargeNozzlesRoot);
            go.name = $"DischargeNozzle_{(i + 1):00}";
            go.transform.position = worldPos;
            go.transform.rotation = Quaternion.Euler(dischargeNozzleLocalEuler);
        }
    }

    private void ClearChildren(Transform root)
    {
        if (root == null)
            return;

        List<GameObject> children = new List<GameObject>();
        for (int i = 0; i < root.childCount; i++)
        {
            children.Add(root.GetChild(i).gameObject);
        }

        for (int i = 0; i < children.Count; i++)
        {
            if (Application.isPlaying)
            {
                Destroy(children[i]);
            }
            else
            {
#if UNITY_EDITOR
                DestroyImmediate(children[i]);
#endif
            }
        }
    }

    private void Log(string message)
    {
        if (debugLogs)
            Debug.Log($"[FireSafetyAutoLayout] {message}", this);
    }

    private void LogWarning(string message)
    {
        if (debugLogs)
            Debug.LogWarning($"[FireSafetyAutoLayout] {message}", this);
    }
}