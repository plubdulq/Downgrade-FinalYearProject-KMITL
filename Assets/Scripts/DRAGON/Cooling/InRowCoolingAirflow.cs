using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class InRowCoolingAirflow : MonoBehaviour
{
    [Header("Transforms")]
    public Transform supplyOutlet;
    public Transform returnInlet;

    [Header("Targeting (NO PHYSICS MODE)")]
    public float range = 5000f;
    public int maxTargets = 2;

    [Header("Cooling Power")]
    public float baseCoolingPerSecond = 50000f;
    public float hotBonusMultiplier = 1f;

    [Header("Airflow Visual (LineRenderer)")]
    public bool enableVisual = true;
    public float arcHeight = 1.1f;
    [Range(8, 64)] public int segments = 24;
    public float scrollSpeed = 1.2f;
    public bool useURPBaseMap = true;

    [Header("Debug")]
    public bool debugDraw = true;

    private LineRenderer lr;
    private readonly List<RackHeatReceiver> targets = new();

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.enabled = false;
        lr.positionCount = 0;
        lr.useWorldSpace = true;
    }

    void Update()
    {
        Transform outlet = supplyOutlet ? supplyOutlet : transform;

        FindTargets_NoPhysics(outlet.position);
        ApplyCooling(outlet.position);

        if (enableVisual) DrawAirflow(outlet.position);
        else lr.enabled = false;

        ScrollLineTexture();
    }

    void FindTargets_NoPhysics(Vector3 origin)
    {
        targets.Clear();

        // หา Rack ทุกตัวในซีน (ไม่พึ่ง collider/layer)
        var racks = Object.FindObjectsOfType<RackHeatReceiver>(true);
        if (racks == null || racks.Length == 0)
        {
            if (debugDraw) Debug.LogWarning("[InRow] No RackHeatReceiver found in scene.");
            return;
        }

        // เลือกที่ใกล้ที่สุดตาม range
        var scored = new List<(RackHeatReceiver rack, float dist)>(racks.Length);

        foreach (var r in racks)
        {
            if (!r || !r.gameObject.activeInHierarchy) continue;

            float d = Vector3.Distance(origin, r.transform.position);
            if (d > range) continue;
            scored.Add((r, d));
        }

        scored.Sort((a, b) => a.dist.CompareTo(b.dist));

        for (int i = 0; i < scored.Count && targets.Count < maxTargets; i++)
            targets.Add(scored[i].rack);

        if (debugDraw) Debug.Log($"[InRow] Targets selected: {targets.Count}");
    }

    void ApplyCooling(Vector3 origin)
    {
        if (targets.Count == 0) return;

        foreach (var rack in targets)
        {
            if (!rack) continue;

            float dist = Vector3.Distance(origin, rack.transform.position);
            float distance01 = Mathf.InverseLerp(range, 0f, dist);

            float hot01 = rack.Heat01;
            float hotBonus = 1f + (hot01 * hotBonusMultiplier);

            float cooling = baseCoolingPerSecond * distance01 * hotBonus;
            rack.ApplyCooling(cooling);
        }
    }

    void DrawAirflow(Vector3 start)
    {
        if (targets.Count == 0)
        {
            lr.enabled = false;
            return;
        }

        lr.enabled = true;

        var rack = targets[0];
        if (!rack) { lr.enabled = false; return; }

        Vector3 end = rack.transform.position + Vector3.up * 1.0f;

        // ถ้าไม่มี returnInlet ก็ให้เด้งเลยที่ rack
        Vector3 ret = returnInlet ? returnInlet.position : end;

        int segA = segments;
        int segB = returnInlet ? segments : 0;

        lr.positionCount = returnInlet ? ((segA + 1) + (segB + 1) - 1) : (segA + 1);

        // supply -> rack
        for (int i = 0; i <= segA; i++)
        {
            float t = i / (float)segA;
            Vector3 p = Bezier(start, start + Vector3.up * arcHeight, end + Vector3.up * arcHeight, end, t);
            lr.SetPosition(i, p);
        }

        if (!returnInlet) return;

        // rack -> return
        for (int i = 1; i <= segB; i++)
        {
            float t = i / (float)segB;
            Vector3 p = Bezier(end, end + Vector3.up * arcHeight, ret + Vector3.up * arcHeight, ret, t);
            lr.SetPosition(segA + i, p);
        }
    }

    void ScrollLineTexture()
    {
        if (!enableVisual || !lr.enabled || !lr.material) return;

        float offset = Time.time * scrollSpeed;

        if (useURPBaseMap && lr.material.HasProperty("_BaseMap"))
            lr.material.SetTextureOffset("_BaseMap", new Vector2(-offset, 0f));
        else if (lr.material.HasProperty("_MainTex"))
            lr.material.SetTextureOffset("_MainTex", new Vector2(-offset, 0f));
    }

    Vector3 Bezier(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        float u = 1f - t;
        return (u * u * u) * a
             + (3f * u * u * t) * b
             + (3f * u * t * t) * c
             + (t * t * t) * d;
    }
}
