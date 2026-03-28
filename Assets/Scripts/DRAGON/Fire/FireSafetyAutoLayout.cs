using System.Collections.Generic;
using UnityEngine;

public class FireSafetyAutoLayout : MonoBehaviour
{
    [Header("Anchors")]
    [SerializeField] private Transform roomCenter;
    [SerializeField] private Transform smokeDetectorsRoot;
    [SerializeField] private Transform dischargeNozzlesRoot;

    [Header("Scene References")]
    [SerializeField] private Transform ceilingsTransform;

    [Header("Room Size (meters)")]
    [SerializeField] private float roomWidth = 5f;
    [SerializeField] private float roomLength = 5f;

    [Header("Ceiling Mounting")]
    [Tooltip("If true, calculate room size and ceiling Y from Ceilings Transform.")]
    [SerializeField] private bool resolveFromCeilings = true;

    [Tooltip("Fallback local Y if Ceilings Transform is not assigned.")]
    [SerializeField] private float fallbackCeilingBottomLocalY = 2.75f;

    [Tooltip("How far below the ceiling bottom to place smoke detector root.")]
    [SerializeField] private float smokeDetectorSurfaceOffset = 0.00f;

    [Tooltip("How far below the ceiling bottom to place nozzle root.")]
    [SerializeField] private float dischargeNozzleSurfaceOffset = 0.00f;

    [Header("Placement Rules")]
    [SerializeField] private float minWallOffset = 0.8f;
    [SerializeField] private float minSmokeToNozzleDistance = 1.0f;

    [Header("Device Rotation")]
    [SerializeField] private Vector3 smokeDetectorLocalEuler = Vector3.zero;
    [SerializeField] private Vector3 dischargeNozzleLocalEuler = new Vector3(90f, 0f, 0f);

    [Header("Runtime")]
    [SerializeField] private bool applyOnStart = true;
    [SerializeField] private bool debugLogs = true;

    private readonly List<Transform> smokeDevices = new();
    private readonly List<Transform> nozzleDevices = new();

    private void Awake()
    {
        CacheChildren();
    }

    private void Start()
    {
        if (applyOnStart)
        {
            ApplyLayout();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        roomWidth = Mathf.Clamp(roomWidth, 5f, 10f);
        roomLength = Mathf.Clamp(roomLength, 5f, 10f);
        fallbackCeilingBottomLocalY = Mathf.Max(0.1f, fallbackCeilingBottomLocalY);
        minWallOffset = Mathf.Clamp(minWallOffset, 0.1f, 2f);
        minSmokeToNozzleDistance = Mathf.Clamp(minSmokeToNozzleDistance, 0.1f, 3f);
        smokeDetectorSurfaceOffset = Mathf.Clamp(smokeDetectorSurfaceOffset, 0f, 0.5f);
        dischargeNozzleSurfaceOffset = Mathf.Clamp(dischargeNozzleSurfaceOffset, 0f, 0.5f);
    }
#endif

    [ContextMenu("Apply Fire Safety Layout")]
    public void ApplyLayout()
    {
        CacheChildren();

        if (roomCenter == null)
        {
            Debug.LogWarning("[FireSafetyAutoLayout] RoomCenter is missing.");
            return;
        }

        float ceilingBottomY = fallbackCeilingBottomLocalY;

        if (resolveFromCeilings && ceilingsTransform != null)
        {
            if (!TryResolveRoomFromCeilings(out roomWidth, out roomLength, out ceilingBottomY))
            {
                Debug.LogWarning("[FireSafetyAutoLayout] Failed to resolve from ceilings. Using fallback values.");
                ceilingBottomY = fallbackCeilingBottomLocalY;
            }
        }

        float area = roomWidth * roomLength;
        int smokeCount = GetRequiredSmokeCount(area);
        int nozzleCount = GetRequiredNozzleCount(area);

        List<Vector3> smokePositions = BuildSmokePositions(smokeCount, ceilingBottomY - smokeDetectorSurfaceOffset);
        List<Vector3> nozzlePositions = BuildNozzlePositions(nozzleCount, ceilingBottomY - dischargeNozzleSurfaceOffset);

        EnforceMinDistanceBetweenSmokeAndNozzles(smokePositions, nozzlePositions);

        ApplyDevices(smokeDevices, smokeCount, smokePositions, Quaternion.Euler(smokeDetectorLocalEuler), "Smoke");
        ApplyDevices(nozzleDevices, nozzleCount, nozzlePositions, Quaternion.Euler(dischargeNozzleLocalEuler), "Nozzle");

        if (debugLogs)
        {
            Debug.Log(
                $"[FireSafetyAutoLayout] Applied | size={roomWidth:0.##} x {roomLength:0.##} m | " +
                $"area={area:0.##} sqm | ceilingBottomY={ceilingBottomY:0.###} | smoke={smokeCount} | nozzle={nozzleCount}"
            );
        }
    }

    private void CacheChildren()
    {
        smokeDevices.Clear();
        nozzleDevices.Clear();

        if (smokeDetectorsRoot != null)
        {
            foreach (Transform child in smokeDetectorsRoot)
            {
                if (child.name.StartsWith("SmokeDetector_"))
                    smokeDevices.Add(child);
            }
        }

        if (dischargeNozzlesRoot != null)
        {
            foreach (Transform child in dischargeNozzlesRoot)
            {
                if (child.name.StartsWith("DischargeNozzle_"))
                    nozzleDevices.Add(child);
            }
        }

        smokeDevices.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        nozzleDevices.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
    }

    private bool TryResolveRoomFromCeilings(out float width, out float length, out float ceilingBottomLocalY)
    {
        width = roomWidth;
        length = roomLength;
        ceilingBottomLocalY = fallbackCeilingBottomLocalY;

        Renderer[] renderers = ceilingsTransform.GetComponentsInChildren<Renderer>(true);
        if (renderers == null || renderers.Length == 0)
            return false;

        Bounds combined = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            combined.Encapsulate(renderers[i].bounds);
        }

        Vector3 localCenter = transform.InverseTransformPoint(combined.center);
        Vector3 localSize = transform.InverseTransformVector(combined.size);

        width = Mathf.Clamp(Mathf.Abs(localSize.x), 5f, 10f);
        length = Mathf.Clamp(Mathf.Abs(localSize.z), 5f, 10f);

        float localMinY = transform.InverseTransformPoint(new Vector3(combined.center.x, combined.min.y, combined.center.z)).y;
        ceilingBottomLocalY = localMinY;

        // keep RoomCenter only as X/Z anchor; don't force Y
        Vector3 rc = roomCenter.localPosition;
        roomCenter.localPosition = new Vector3(localCenter.x, rc.y, localCenter.z);

        return true;
    }

    private int GetRequiredSmokeCount(float area)
    {
        return area <= 64f ? 1 : 2;
    }

    private int GetRequiredNozzleCount(float area)
    {
        return Mathf.Clamp(Mathf.CeilToInt(area / 30f), 1, 4);
    }

    private List<Vector3> BuildSmokePositions(int count, float y)
    {
        List<Vector3> positions = new();

        Vector3 center = roomCenter.localPosition;
        center.y = y;

        if (count <= 0)
            return positions;

        if (count == 1)
        {
            positions.Add(center);
            return positions;
        }

        bool splitAlongWidth = roomWidth >= roomLength;

        if (splitAlongWidth)
        {
            float x1 = center.x + ClampX(-roomWidth * 0.25f);
            float x2 = center.x + ClampX(roomWidth * 0.25f);

            positions.Add(new Vector3(x1, y, center.z));
            positions.Add(new Vector3(x2, y, center.z));
        }
        else
        {
            float z1 = center.z + ClampZ(-roomLength * 0.25f);
            float z2 = center.z + ClampZ(roomLength * 0.25f);

            positions.Add(new Vector3(center.x, y, z1));
            positions.Add(new Vector3(center.x, y, z2));
        }

        return positions;
    }

    private List<Vector3> BuildNozzlePositions(int count, float y)
    {
        List<Vector3> positions = new();

        Vector3 center = roomCenter.localPosition;
        center.y = y;

        if (count <= 0)
            return positions;

        if (count == 1)
        {
            positions.Add(center);
            return positions;
        }

        if (count == 2)
        {
            bool splitAlongWidth = roomWidth >= roomLength;

            if (splitAlongWidth)
            {
                float x1 = center.x + ClampX(-roomWidth * 0.25f);
                float x2 = center.x + ClampX(roomWidth * 0.25f);

                positions.Add(new Vector3(x1, y, center.z));
                positions.Add(new Vector3(x2, y, center.z));
            }
            else
            {
                float z1 = center.z + ClampZ(-roomLength * 0.25f);
                float z2 = center.z + ClampZ(roomLength * 0.25f);

                positions.Add(new Vector3(center.x, y, z1));
                positions.Add(new Vector3(center.x, y, z2));
            }

            return positions;
        }

        if (count == 3)
        {
            bool splitAlongWidth = roomWidth >= roomLength;

            if (splitAlongWidth)
            {
                float x1 = center.x + ClampX(-roomWidth * 0.30f);
                float x2 = center.x;
                float x3 = center.x + ClampX(roomWidth * 0.30f);

                positions.Add(new Vector3(x1, y, center.z));
                positions.Add(new Vector3(x2, y, center.z));
                positions.Add(new Vector3(x3, y, center.z));
            }
            else
            {
                float z1 = center.z + ClampZ(-roomLength * 0.30f);
                float z2 = center.z;
                float z3 = center.z + ClampZ(roomLength * 0.30f);

                positions.Add(new Vector3(center.x, y, z1));
                positions.Add(new Vector3(center.x, y, z2));
                positions.Add(new Vector3(center.x, y, z3));
            }

            return positions;
        }

        // count == 4 -> square pattern
        float xA = center.x + ClampX(-roomWidth * 0.25f);
        float xB = center.x + ClampX(roomWidth * 0.25f);
        float zA = center.z + ClampZ(-roomLength * 0.25f);
        float zB = center.z + ClampZ(roomLength * 0.25f);

        positions.Add(new Vector3(xA, y, zA));
        positions.Add(new Vector3(xB, y, zA));
        positions.Add(new Vector3(xA, y, zB));
        positions.Add(new Vector3(xB, y, zB));

        return positions;
    }

    private void EnforceMinDistanceBetweenSmokeAndNozzles(List<Vector3> smokePositions, List<Vector3> nozzlePositions)
    {
        for (int i = 0; i < nozzlePositions.Count; i++)
        {
            Vector3 nozzle = nozzlePositions[i];

            for (int j = 0; j < smokePositions.Count; j++)
            {
                Vector3 smoke = smokePositions[j];
                float distXZ = Vector2.Distance(
                    new Vector2(nozzle.x, nozzle.z),
                    new Vector2(smoke.x, smoke.z)
                );

                if (distXZ < minSmokeToNozzleDistance)
                {
                    Vector3 dir = new Vector3(nozzle.x - smoke.x, 0f, nozzle.z - smoke.z);
                    if (dir.sqrMagnitude < 0.0001f)
                        dir = new Vector3(1f, 0f, 0f);

                    dir.Normalize();
                    Vector3 moved = smoke + dir * minSmokeToNozzleDistance;
                    moved.x = ClampAbsoluteX(moved.x, roomCenter.localPosition.x);
                    moved.z = ClampAbsoluteZ(moved.z, roomCenter.localPosition.z);
                    moved.y = nozzle.y;

                    nozzlePositions[i] = moved;
                    nozzle = moved;
                }
            }
        }
    }

    private void ApplyDevices(
        List<Transform> devices,
        int requiredCount,
        List<Vector3> positions,
        Quaternion localRotation,
        string label)
    {
        for (int i = 0; i < devices.Count; i++)
        {
            bool shouldEnable = i < requiredCount && i < positions.Count;

            if (devices[i] == null)
                continue;

            devices[i].gameObject.SetActive(shouldEnable);

            if (!shouldEnable)
                continue;

            devices[i].localPosition = positions[i];
            devices[i].localRotation = localRotation;

            if (debugLogs)
            {
                Debug.Log(
                    $"[FireSafetyAutoLayout] {label} {devices[i].name} -> " +
                    $"localPosition={devices[i].localPosition}, localRotation={devices[i].localEulerAngles}"
                );
            }
        }
    }

    private float ClampX(float x)
    {
        return Mathf.Clamp(x, -roomWidth * 0.5f + minWallOffset, roomWidth * 0.5f - minWallOffset);
    }

    private float ClampZ(float z)
    {
        return Mathf.Clamp(z, -roomLength * 0.5f + minWallOffset, roomLength * 0.5f - minWallOffset);
    }

    private float ClampAbsoluteX(float x, float centerX)
    {
        float min = centerX - roomWidth * 0.5f + minWallOffset;
        float max = centerX + roomWidth * 0.5f - minWallOffset;
        return Mathf.Clamp(x, min, max);
    }

    private float ClampAbsoluteZ(float z, float centerZ)
    {
        float min = centerZ - roomLength * 0.5f + minWallOffset;
        float max = centerZ + roomLength * 0.5f - minWallOffset;
        return Mathf.Clamp(z, min, max);
    }
}