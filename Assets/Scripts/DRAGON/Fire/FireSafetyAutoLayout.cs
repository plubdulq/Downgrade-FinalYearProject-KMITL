using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FireSafetyAutoLayout : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject smokeDetectorPrefab;
    public GameObject dischargeNozzlePrefab;

    [Header("Spawn Parent")]
    [Tooltip("Parent สำหรับ object ที่ script spawn ขึ้นมา")]
    public Transform spawnedRoot;

    [Header("Room Layout")]
    [Tooltip("Offset จากตำแหน่ง object นี้ ไปยัง 'กลางห้อง' ถ้า object นี้ไม่ได้อยู่กลางห้องจริง")]
    public Vector3 layoutCenterOffset = Vector3.zero;

    [Tooltip("ความสูงเพดานที่ต้องการใช้ spawn อุปกรณ์")]
    public float ceilingY = 2.8f;

    [Tooltip("ระยะเผื่อจากผนัง ไม่ให้วางชิดขอบเกินไป")]
    public float edgePadding = 0.8f;

    [Tooltip("ระยะขั้นต่ำระหว่าง nozzle กับ smoke detector")]
    public float minimumSmokeNozzleSeparation = 1.0f;

    [Header("Rotation")]
    public Vector3 smokeRotationEuler = Vector3.zero;
    public Vector3 nozzleRotationEuler = Vector3.zero;

    [Header("Behaviour")]
    [Tooltip("ถ้า true จะลบของเก่าที่เคย spawn โดย script นี้ก่อนสร้างใหม่")]
    public bool clearOldSpawnedObjects = true;

    [Tooltip("ถ้า true จะ auto spawn ตอน Start")]
    public bool autoSpawnOnStart = true;

    [Tooltip("เปิด debug log")]
    public bool debugLogs = true;

    private const string SpawnedTagName = "FireSafetyAutoSpawned";

    private void Start()
    {
        if (autoSpawnOnStart)
        {
            GenerateLayout();
        }
    }

    [ContextMenu("Generate Layout Now")]
    public void GenerateLayout()
    {
        if (!ValidateSetup())
            return;

        if (!TryParseRoomSizeFromSceneName(SceneManager.GetActiveScene().name, out int width, out int length))
        {
            Debug.LogError($"[FireSafetyAutoLayout] Scene name '{SceneManager.GetActiveScene().name}' parse ไม่ได้. " +
                           $"รองรับชื่อแบบ 55, 56, 88, 910, 1010");
            return;
        }

        int area = width * length;
        int smokeCount = GetSmokeCount(area);
        int nozzleCount = GetNozzleCount(area);

        if (debugLogs)
        {
            Debug.Log($"[FireSafetyAutoLayout] Scene={SceneManager.GetActiveScene().name}, Room={width}x{length}, Area={area} sqm, Smoke={smokeCount}, Nozzle={nozzleCount}");
        }

        if (clearOldSpawnedObjects)
        {
            ClearSpawnedObjects();
        }

        Vector3 roomCenter = transform.position + layoutCenterOffset;

        List<Vector3> smokePositions = CalculateSmokePositions(width, length, smokeCount, roomCenter);
        List<Vector3> nozzlePositions = CalculateNozzlePositions(width, length, nozzleCount, roomCenter, smokePositions);

        SpawnSmokeDetectors(smokePositions);
        SpawnDischargeNozzles(nozzlePositions);
    }

    private bool ValidateSetup()
    {
        if (!smokeDetectorPrefab)
        {
            Debug.LogError("[FireSafetyAutoLayout] smokeDetectorPrefab ยังไม่ได้ assign");
            return false;
        }

        if (!dischargeNozzlePrefab)
        {
            Debug.LogError("[FireSafetyAutoLayout] dischargeNozzlePrefab ยังไม่ได้ assign");
            return false;
        }

        if (!spawnedRoot)
        {
            Debug.LogWarning("[FireSafetyAutoLayout] spawnedRoot ยังไม่ได้ assign -> จะใช้ object นี้เป็น parent แทน");
            spawnedRoot = transform;
        }

        return true;
    }

    private bool TryParseRoomSizeFromSceneName(string sceneName, out int width, out int length)
    {
        width = 0;
        length = 0;

        if (string.IsNullOrWhiteSpace(sceneName))
            return false;

        // รองรับเฉพาะตัวเลขล้วน
        foreach (char c in sceneName)
        {
            if (!char.IsDigit(c))
                return false;
        }

        // ห้องของคุณอยู่ในช่วง 5-10 เมตร
        // ดังนั้นชื่อ scene ที่เป็นไปได้:
        // 55   = 5,5
        // 56   = 5,6
        // 910  = 9,10
        // 1010 = 10,10

        List<int> numbers = ExtractValidRoomParts(sceneName);

        if (numbers.Count == 2)
        {
            width = numbers[0];
            length = numbers[1];
            return true;
        }

        return false;
    }

    private List<int> ExtractValidRoomParts(string sceneName)
    {
        List<int> result = new List<int>();

        // วิธีง่ายและชัวร์สำหรับช่วง 5..10
        // ทดลองแยกเป็น 1+1, 1+2, 2+1, 2+2 แล้วเช็คว่าเลขอยู่ในช่วง 5..10
        for (int split = 1; split < sceneName.Length; split++)
        {
            string left = sceneName.Substring(0, split);
            string right = sceneName.Substring(split);

            if (int.TryParse(left, out int a) && int.TryParse(right, out int b))
            {
                if (a >= 5 && a <= 10 && b >= 5 && b <= 10)
                {
                    result.Add(a);
                    result.Add(b);
                    return result;
                }
            }
        }

        return result;
    }

    private int GetSmokeCount(int area)
    {
        if (area <= 64) return 1;
        return 2; // >64 ถึง 100
    }

    private int GetNozzleCount(int area)
    {
        if (area <= 24) return 0;      // กันพลาด เผื่อขนาดแปลก
        if (area <= 30) return 1;
        if (area <= 60) return 2;
        if (area <= 90) return 3;
        return 4; // 91-100
    }

    private List<Vector3> CalculateSmokePositions(int width, int length, int smokeCount, Vector3 roomCenter)
    {
        List<Vector3> positions = new List<Vector3>();

        float halfWidth = width * 0.5f;
        float halfLength = length * 0.5f;

        if (smokeCount <= 0)
            return positions;

        if (smokeCount == 1)
        {
            positions.Add(new Vector3(
                roomCenter.x,
                ceilingY,
                roomCenter.z
            ));
            return positions;
        }

        // 2 smoke = split ตามแกนที่ยาวกว่า
        bool splitAlongWidth = width >= length;

        if (splitAlongWidth)
        {
            float x1 = roomCenter.x - halfWidth * 0.5f;
            float x2 = roomCenter.x + halfWidth * 0.5f;

            x1 = ClampXToRoom(x1, roomCenter.x, halfWidth);
            x2 = ClampXToRoom(x2, roomCenter.x, halfWidth);

            positions.Add(new Vector3(x1, ceilingY, roomCenter.z));
            positions.Add(new Vector3(x2, ceilingY, roomCenter.z));
        }
        else
        {
            float z1 = roomCenter.z - halfLength * 0.5f;
            float z2 = roomCenter.z + halfLength * 0.5f;

            z1 = ClampZToRoom(z1, roomCenter.z, halfLength);
            z2 = ClampZToRoom(z2, roomCenter.z, halfLength);

            positions.Add(new Vector3(roomCenter.x, ceilingY, z1));
            positions.Add(new Vector3(roomCenter.x, ceilingY, z2));
        }

        return positions;
    }

    private List<Vector3> CalculateNozzlePositions(int width, int length, int nozzleCount, Vector3 roomCenter, List<Vector3> smokePositions)
    {
        List<Vector3> positions = new List<Vector3>();

        float halfWidth = width * 0.5f;
        float halfLength = length * 0.5f;

        if (nozzleCount <= 0)
            return positions;

        switch (nozzleCount)
        {
            case 1:
            {
                positions.Add(new Vector3(roomCenter.x, ceilingY, roomCenter.z));
                break;
            }

            case 2:
            {
                // กระจายตามแกนที่ยาวกว่า
                bool splitAlongWidth = width >= length;

                if (splitAlongWidth)
                {
                    float x1 = roomCenter.x - halfWidth * 0.25f;
                    float x2 = roomCenter.x + halfWidth * 0.25f;

                    x1 = ClampXToRoom(x1, roomCenter.x, halfWidth);
                    x2 = ClampXToRoom(x2, roomCenter.x, halfWidth);

                    positions.Add(new Vector3(x1, ceilingY, roomCenter.z));
                    positions.Add(new Vector3(x2, ceilingY, roomCenter.z));
                }
                else
                {
                    float z1 = roomCenter.z - halfLength * 0.25f;
                    float z2 = roomCenter.z + halfLength * 0.25f;

                    z1 = ClampZToRoom(z1, roomCenter.z, halfLength);
                    z2 = ClampZToRoom(z2, roomCenter.z, halfLength);

                    positions.Add(new Vector3(roomCenter.x, ceilingY, z1));
                    positions.Add(new Vector3(roomCenter.x, ceilingY, z2));
                }
                break;
            }

            case 3:
            {
                // วางเป็นสามเหลี่ยมสมดุลแบบ practical:
                // 1 จุดด้านบนกลาง + 2 จุดด้านล่างซ้าย/ขวา
                float xLeft = ClampXToRoom(roomCenter.x - halfWidth * 0.28f, roomCenter.x, halfWidth);
                float xRight = ClampXToRoom(roomCenter.x + halfWidth * 0.28f, roomCenter.x, halfWidth);
                float zTop = ClampZToRoom(roomCenter.z + halfLength * 0.22f, roomCenter.z, halfLength);
                float zBottom = ClampZToRoom(roomCenter.z - halfLength * 0.22f, roomCenter.z, halfLength);

                positions.Add(new Vector3(roomCenter.x, ceilingY, zTop));
                positions.Add(new Vector3(xLeft, ceilingY, zBottom));
                positions.Add(new Vector3(xRight, ceilingY, zBottom));
                break;
            }

            case 4:
            {
                // วางเป็น 2x2 บนเพดาน
                float x1 = ClampXToRoom(roomCenter.x - halfWidth * 0.25f, roomCenter.x, halfWidth);
                float x2 = ClampXToRoom(roomCenter.x + halfWidth * 0.25f, roomCenter.x, halfWidth);
                float z1 = ClampZToRoom(roomCenter.z - halfLength * 0.25f, roomCenter.z, halfLength);
                float z2 = ClampZToRoom(roomCenter.z + halfLength * 0.25f, roomCenter.z, halfLength);

                positions.Add(new Vector3(x1, ceilingY, z1));
                positions.Add(new Vector3(x2, ceilingY, z1));
                positions.Add(new Vector3(x1, ceilingY, z2));
                positions.Add(new Vector3(x2, ceilingY, z2));
                break;
            }
        }

        // พยายามขยับ nozzle ให้ห่าง smoke detector
        for (int i = 0; i < positions.Count; i++)
        {
            positions[i] = PushAwayFromSmokeIfNeeded(
                positions[i],
                smokePositions,
                roomCenter,
                halfWidth,
                halfLength,
                minimumSmokeNozzleSeparation
            );
        }

        return positions;
    }

    private Vector3 PushAwayFromSmokeIfNeeded(
        Vector3 nozzlePos,
        List<Vector3> smokePositions,
        Vector3 roomCenter,
        float halfWidth,
        float halfLength,
        float minDistance)
    {
        Vector3 adjusted = nozzlePos;

        for (int i = 0; i < smokePositions.Count; i++)
        {
            Vector3 smoke = smokePositions[i];

            Vector2 a = new Vector2(adjusted.x, adjusted.z);
            Vector2 b = new Vector2(smoke.x, smoke.z);

            float dist = Vector2.Distance(a, b);
            if (dist < minDistance)
            {
                Vector2 dir = (a - b);
                if (dir.sqrMagnitude < 0.0001f)
                {
                    dir = new Vector2(1f, 0f);
                }
                dir.Normalize();

                Vector2 pushed = b + dir * minDistance;

                adjusted.x = ClampXToRoom(pushed.x, roomCenter.x, halfWidth);
                adjusted.z = ClampZToRoom(pushed.y, roomCenter.z, halfLength);
            }
        }

        adjusted.y = ceilingY;
        return adjusted;
    }

    private float ClampXToRoom(float x, float centerX, float halfWidth)
    {
        return Mathf.Clamp(x, centerX - halfWidth + edgePadding, centerX + halfWidth - edgePadding);
    }

    private float ClampZToRoom(float z, float centerZ, float halfLength)
    {
        return Mathf.Clamp(z, centerZ - halfLength + edgePadding, centerZ + halfLength - edgePadding);
    }

    private void SpawnSmokeDetectors(List<Vector3> positions)
    {
        Quaternion rot = Quaternion.Euler(smokeRotationEuler);

        for (int i = 0; i < positions.Count; i++)
        {
            GameObject obj = Instantiate(smokeDetectorPrefab, positions[i], rot, spawnedRoot);
            obj.name = $"SmokeDetector_Auto_{i + 1}";
            MarkAsSpawned(obj);
        }
    }

    private void SpawnDischargeNozzles(List<Vector3> positions)
    {
        Quaternion rot = Quaternion.Euler(nozzleRotationEuler);

        for (int i = 0; i < positions.Count; i++)
        {
            GameObject obj = Instantiate(dischargeNozzlePrefab, positions[i], rot, spawnedRoot);
            obj.name = $"DischargeNozzle_Auto_{i + 1}";
            MarkAsSpawned(obj);
        }
    }

    private void MarkAsSpawned(GameObject obj)
    {
        obj.tag = SafeEnsureTagOrFallback(obj.tag);
        AutoSpawnMarker marker = obj.GetComponent<AutoSpawnMarker>();
        if (!marker)
        {
            marker = obj.AddComponent<AutoSpawnMarker>();
        }
    }

    private string SafeEnsureTagOrFallback(string currentTag)
    {
        // ถ้า project ไม่มี tag นี้จริง Unity จะโยน error ตอน assign tag
        // เลยใช้ Untagged แล้ว rely กับ component marker แทน
        return "Untagged";
    }

    private void ClearSpawnedObjects()
    {
        if (!spawnedRoot)
            return;

        List<GameObject> toDestroy = new List<GameObject>();

        foreach (Transform child in spawnedRoot)
        {
            if (child.GetComponent<AutoSpawnMarker>())
            {
                toDestroy.Add(child.gameObject);
            }
        }

        for (int i = 0; i < toDestroy.Count; i++)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(toDestroy[i]);
            else
                Destroy(toDestroy[i]);
#else
            Destroy(toDestroy[i]);
#endif
        }

        if (debugLogs && toDestroy.Count > 0)
        {
            Debug.Log($"[FireSafetyAutoLayout] Cleared old spawned objects: {toDestroy.Count}");
        }
    }
}

public class AutoSpawnMarker : MonoBehaviour
{
    // เอาไว้ mark ว่า object นี้ถูก spawn โดย FireSafetyAutoLayout
}