using System.Collections.Generic;
using UnityEngine;

public class PAC_AirflowEmitter : MonoBehaviour
{
    int _leftIdx = 0;
    int _rightIdx = 0;

    [Header("Outlets (supply air)")]
    public List<Transform> outlets = new(); // OutletLeft, OutletRight

    [Header("Arrow Prefab")]
    public PAC_AirArrowFollower arrowPrefab;

    [Header("Targeting")]
    public LayerMask targetMask;      // AirTarget
    public int maxTargets = 16;

    [Header("Fan Level (1-5)")]
    [Range(1, 5)] public int fanLevel = 1;

    [Header("Range (used as search radius only)")]
    public float baseRange = 1.2f;    // fan=1
    public float rangePerLevel = 0.6f;

    [Header("Aisle / Plane Gate (stop 'front/back random airflow')")]
    [Tooltip("ยอมให้เป้าหมายเลื่อนไปหน้า/หลังจาก PAC ได้ (เมตร). ยิ่งน้อยยิ่ง strict.")]
    public float maxForwardOffset = 0.9f;

    [Tooltip("ถ้า true จะกรองด้วยระนาบ (หน้า/หลัง) ก่อนเสมอ")]
    public bool enforceSameAislePlane = true;

    [Header("Max Side Distance per Fan Level (meters, per side)")]
    [Tooltip("ระยะซ้าย/ขวาสูงสุด (เมตร) ต่อ fan level. fan5 = ไปได้ไกลสุดเท่านี้ต่อฝั่ง")]
    public float[] maxSideDistanceByLevel = new float[5] { 1.2f, 1.8f, 2.4f, 3.0f, 3.6f };

    [Header("Bezier")]
    public float curveSideStrength = 0.25f;
    public float curveUpStrength = 0.05f;

    [Header("Wave")]
    public float waveStrength = 0.05f;
    public float waveFrequency = 1.2f;

    [Header("Stream (per outlet)")]
    public float baseSpawnPerSec = 4f;
    public float spawnPerLevel = 1.2f;
    public float baseArrowSpeed = 1.6f;
    public float speedPerLevel = 0.35f;

    [Header("Arrow Scale Taper")]
    public float nearScale = 0.95f;
    public float farScale = 0.65f;

    [Header("Side Fallback")]
    public bool allowCrossSideFallback = false;

    [Header("Debug")]
    public bool drawGizmos = true;

    [Header("Density By Distance (visual strength)")]
    public bool enableDensityByDistance = true;

    [Tooltip("X=normalized distance (0 near..1 far), Y=strength (1 near..0 far)")]
    public AnimationCurve distanceStrengthCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Tooltip("Burst arrows when target is near (more lines = stronger airflow)")]
    public int nearBurst = 3;

    [Tooltip("Burst arrows when target is far (less lines)")]
    public int farBurst = 1;

    [Tooltip("Speed multiplier at near distance")]
    public float nearSpeedMul = 1.25f;

    [Tooltip("Speed multiplier at far distance")]
    public float farSpeedMul = 0.75f;

    [Tooltip("Phase random per burst")]
    public float phaseSpread = 0.25f;

    readonly List<AirIntakeTarget> _targets = new();
    readonly Collider[] _hits = new Collider[128];
    float _acc;

    void Start()
    {
        if (!arrowPrefab || outlets.Count == 0)
        {
            Debug.LogWarning("[PAC_AirflowEmitter] Missing arrowPrefab or outlets.");
            enabled = false;
            return;
        }

        // safety: ensure array length
        if (maxSideDistanceByLevel == null || maxSideDistanceByLevel.Length != 5)
            maxSideDistanceByLevel = new float[5] { 1.2f, 1.8f, 2.4f, 3.0f, 3.6f };
    }

    void Update()
    {
        FindTargets();

        float spawnRate = Mathf.Max(0f, baseSpawnPerSec + (fanLevel - 1) * spawnPerLevel);
        _acc += spawnRate * Time.deltaTime;

        while (_acc >= 1f)
        {
            _acc -= 1f;
            SpawnOncePerOutlet();
        }
    }

    float MaxSideDist()
    {
        int idx = Mathf.Clamp(fanLevel, 1, 5) - 1;
        return Mathf.Max(0.1f, maxSideDistanceByLevel[idx]);
    }

    void FindTargets()
    {
        _targets.Clear();

        // ใช้ OverlapSphere เป็น "search broad phase" เท่านั้น
        float searchRange = baseRange + (fanLevel - 1) * rangePerLevel;
        // เพิ่มนิดเพราะเราจะคัดด้วย gate อีกที
        float broad = Mathf.Max(searchRange, MaxSideDist()) + 1.0f;

        int count = Physics.OverlapSphereNonAlloc(transform.position, broad, _hits, targetMask, QueryTriggerInteraction.Collide);
        if (count <= 0) return;

        Vector3 pacPos = transform.position;
        Vector3 pacFwd = transform.forward; pacFwd.y = 0f; pacFwd = pacFwd.sqrMagnitude < 0.0001f ? Vector3.forward : pacFwd.normalized;
        Vector3 pacRight = transform.right; pacRight.y = 0f; pacRight = pacRight.sqrMagnitude < 0.0001f ? Vector3.right : pacRight.normalized;

        float maxSide = MaxSideDist();
        float maxFwd = Mathf.Max(0.05f, maxForwardOffset);

        for (int i = 0; i < count; i++)
        {
            var h = _hits[i];
            if (!h) continue;

            var t = h.GetComponentInChildren<AirIntakeTarget>();
            if (!t || !t.intakePoint) continue;

            Vector3 p = t.intakePoint.position;
            Vector3 delta = p - pacPos;
            delta.y = 0f;

            // 1) Plane Gate: กัน rack วางหน้า/หลังคนละระนาบ
            if (enforceSameAislePlane)
            {
                float fwdOffset = Mathf.Abs(Vector3.Dot(delta, pacFwd));   // ระยะหน้า/หลัง
                if (fwdOffset > maxFwd) continue;
            }

            // 2) Side Distance Cap: fan level คุมระยะซ้าย/ขวาสูงสุด
            float sideAbs = Mathf.Abs(Vector3.Dot(delta, pacRight)); // ระยะซ้าย/ขวา
            if (sideAbs > maxSide) continue;

            // unique
            bool dup = false;
            for (int k = 0; k < _targets.Count; k++)
                if (_targets[k] == t) { dup = true; break; }
            if (dup) continue;

            _targets.Add(t);
        }

        // sort by distance to PAC (2D)
        _targets.Sort((a, b) =>
        {
            Vector3 da = a.intakePoint.position - pacPos; da.y = 0f;
            Vector3 db = b.intakePoint.position - pacPos; db.y = 0f;
            return da.sqrMagnitude.CompareTo(db.sqrMagnitude);
        });

        if (maxTargets > 0 && _targets.Count > maxTargets)
            _targets.RemoveRange(maxTargets, _targets.Count - maxTargets);
    }

    void SpawnOncePerOutlet()
    {
        if (_targets.Count == 0) return;

        float baseSpeed = baseArrowSpeed + (fanLevel - 1) * speedPerLevel;

        foreach (var outlet in outlets)
        {
            if (!outlet) continue;

            Transform endT = PickEndRoundRobin(outlet);
            if (!endT) continue;

            Vector3 start = outlet.position;
            Vector3 end = endT.position;

            // --- Distance -> Strength (0..1) ---
            float maxSide = MaxSideDist(); // fan-level cap
            float dist = Vector3.Distance(start, end);
            float nd = (maxSide <= 0.01f) ? 1f : Mathf.Clamp01(dist / maxSide);

            // strength: near=1, far=0
            float s = enableDensityByDistance ? distanceStrengthCurve.Evaluate(nd) : 1f;

            // burst + speed based on strength
            int burst = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(farBurst, nearBurst, s)), 1, 8);
            float speedMul = Mathf.Lerp(farSpeedMul, nearSpeedMul, s);
            float speed = baseSpeed * speedMul;

            // fire burst arrows
            for (int b = 0; b < burst; b++)
            {
                float phase = Random.value * phaseSpread; // กระจาย phase กันชนกัน
                SpawnArrow(outlet, start, end, speed, phase);
            }
        }
    }

    Transform PickEndRoundRobin(Transform outlet)
    {
        if (_targets.Count == 0) return null;

        Vector3 origin = outlet.position;
        Vector3 pacRight = transform.right; pacRight.y = 0f; pacRight = pacRight.sqrMagnitude < 0.0001f ? Vector3.right : pacRight.normalized;

        bool isRightOutlet = IsRightOutletByName(outlet.name);
        if (!IsOutletNameReliable(outlet.name))
            isRightOutlet = Vector3.Dot(outlet.right, transform.right) > 0f;

        float maxSide = MaxSideDist();

        List<Transform> sideTargets = new List<Transform>(16);
        for (int i = 0; i < _targets.Count; i++)
        {
            var t = _targets[i];
            if (!t || !t.intakePoint) continue;

            Vector3 d = (t.intakePoint.position - origin);
            d.y = 0f;

            // ✅ เพิ่มความ strict อีกชั้น: ต่อฝั่งต้องไม่เกิน maxSide จริง ๆ
            float sideSigned = Vector3.Dot(d, pacRight); // + = right
            if (Mathf.Abs(sideSigned) > maxSide) continue;

            if (isRightOutlet)
            {
                if (sideSigned < 0.05f) continue;
            }
            else
            {
                if (sideSigned > -0.05f) continue;
            }

            sideTargets.Add(t.intakePoint);
        }

        if (sideTargets.Count == 0)
        {
            if (!allowCrossSideFallback) return null;

            for (int i = 0; i < _targets.Count; i++)
                if (_targets[i] && _targets[i].intakePoint)
                    sideTargets.Add(_targets[i].intakePoint);

            if (sideTargets.Count == 0) return null;
        }

        sideTargets.Sort((a, b) =>
            Vector3.SqrMagnitude(a.position - origin).CompareTo(Vector3.SqrMagnitude(b.position - origin)));

        if (isRightOutlet)
        {
            var t = sideTargets[_rightIdx % sideTargets.Count];
            _rightIdx++;
            return t;
        }
        else
        {
            var t = sideTargets[_leftIdx % sideTargets.Count];
            _leftIdx++;
            return t;
        }
    }

    static bool IsOutletNameReliable(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        string n = name.ToLower();
        return n.Contains("left") || n.Contains("right");
    }

    static bool IsRightOutletByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return true;
        string n = name.ToLower();
        if (n.Contains("right")) return true;
        if (n.Contains("left")) return false;
        return true;
    }

    void ComputeBezierControls(Transform outlet, Vector3 start, Vector3 end, out Vector3 c1, out Vector3 c2)
    {
        Vector3 dir = (end - start);
        float dist = Mathf.Max(dir.magnitude, 0.2f);

        // ✅ ระยะใกล้ไม่บังคับให้เป็น 1.2 แล้ว
        // แต่ยังกันไม่ให้สั้นเกินจนแบน/กระตุก
        float curveDist = Mathf.Clamp(dist, 0.35f, 6.0f);

        Vector3 fwd = outlet.forward;
        Vector3 side = outlet.right;

        // ✅ ทำให้ "ใกล้โค้งน้อย" โดย scale ความโค้งตามระยะ
        // dist <= ~1m => scale ต่ำ, dist ไกล => scale สูงขึ้น
        float t = Mathf.Clamp01(curveDist / 2.0f);   // 0..1
        float sideScale = Mathf.Lerp(0.15f, 1.0f, t);
        float upScale = Mathf.Lerp(0.25f, 1.0f, t);

        Vector3 mid1 = start + fwd * (curveDist * 0.35f);
        Vector3 mid2 = start + fwd * (curveDist * 0.70f);

        c1 = mid1 + side * (curveSideStrength * sideScale) + Vector3.up * (curveUpStrength * upScale);
        c2 = mid2 - side * (curveSideStrength * 0.6f * sideScale) + Vector3.up * (curveUpStrength * 0.5f * upScale);
    }

    void SpawnArrow(Transform outlet, Vector3 start, Vector3 end, float speed, float phase01)
    {
        ComputeBezierControls(outlet, start, end, out Vector3 c1, out Vector3 c2);

        var arrow = Instantiate(arrowPrefab, start, Quaternion.identity);
        arrow.name = $"AirArrow_{outlet.name}_{Time.frameCount}";

        float approxLen = Mathf.Max(0.25f, Vector3.Distance(start, end));
        float ttl = Mathf.Clamp(approxLen / Mathf.Max(0.2f, speed) + 0.6f, 1.2f, 6.5f);

        arrow.Init(new PAC_AirArrowFollower.PathConfig
        {
            start = start,
            c1 = c1,
            c2 = c2,
            end = end,
            speed = speed,
            phase = phase01,
            waveStrength = waveStrength,
            waveFrequency = waveFrequency,
            lockWaveToFixedSide = true,
            nearScale = nearScale,
            farScale = farScale,
            ttl = ttl,
            nearLength = 0.35f,
            farLength = 1.00f,
        });
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        float maxSide = MaxSideDist();

        Gizmos.color = Color.cyan;
        // วาด "ช่องลม" คร่าว ๆ เป็นเส้นสองข้าง
        Vector3 p = transform.position;
        Vector3 r = transform.right; r.y = 0f; r.Normalize();
        Vector3 f = transform.forward; f.y = 0f; f.Normalize();

        // เส้นซ้าย/ขวา
        Gizmos.DrawLine(p + r * maxSide, p + r * maxSide + f * 3f);
        Gizmos.DrawLine(p - r * maxSide, p - r * maxSide + f * 3f);

        // ขอบหน้า/หลัง (plane gate)
        if (enforceSameAislePlane)
        {
            float m = Mathf.Max(0.05f, maxForwardOffset);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(p + f * m - r * maxSide, p + f * m + r * maxSide);
            Gizmos.DrawLine(p - f * m - r * maxSide, p - f * m + r * maxSide);
        }
    }
}
