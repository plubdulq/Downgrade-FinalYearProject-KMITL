using System.Collections.Generic;
using UnityEngine;

public class PAC_AirflowEmitter : MonoBehaviour
{
    [Header("Outlets")]
    public List<Transform> outlets = new();

    [Header("Arrow Prefab")]
    public PAC_AirArrowFollower arrowPrefab;

    [Header("Visual (no rack yet)")]
    public float fallbackDistance = 2.5f;
    public float curveSideStrength = 0.35f;
    public float curveUpStrength = 0.15f;
    public float waveStrength = 0.08f;
    public float waveFrequency = 1.2f;

    [Header("Stream")]
    public int arrowsPerOutlet = 10;
    public float arrowSpeed = 1.6f;
    public float arrowSpacingT = 0.08f;
    public float arrowScale = 1f;

    [Header("Debug")]
    public bool drawGizmos = true;

    private readonly List<PAC_AirArrowFollower> _spawned = new();

    void Start()
    {
        if (arrowPrefab == null || outlets.Count == 0)
        {
            Debug.LogWarning("[PAC_AirflowEmitter] Missing arrowPrefab or outlets.");
            return;
        }

        BuildStreams();
    }

    void OnDisable()
    {
        for (int i = 0; i < _spawned.Count; i++)
            if (_spawned[i]) Destroy(_spawned[i].gameObject);

        _spawned.Clear();
    }

    public void BuildStreams()
    {
        for (int i = 0; i < _spawned.Count; i++)
            if (_spawned[i]) Destroy(_spawned[i].gameObject);

        _spawned.Clear();

        foreach (var outlet in outlets)
        {
            if (!outlet) continue;

            Vector3 start = outlet.position;
            Vector3 end = outlet.position + outlet.forward * fallbackDistance;

            ComputeBezierControls(outlet, start, end, out Vector3 c1, out Vector3 c2);

            for (int i = 0; i < arrowsPerOutlet; i++)
            {
                var arrow = Instantiate(arrowPrefab, start, outlet.rotation, transform);
                arrow.name = $"AirArrow_{outlet.name}_{i:00}";
                arrow.transform.localScale *= arrowScale;

                float phase = (i * arrowSpacingT) % 1f;

                arrow.Init(new PAC_AirArrowFollower.PathConfig
                {
                    start = start,
                    c1 = c1,
                    c2 = c2,
                    end = end,
                    speed = arrowSpeed,
                    phase = phase,
                    waveStrength = waveStrength,
                    waveFrequency = waveFrequency
                });

                _spawned.Add(arrow);
            }
        }
    }

    void ComputeBezierControls(Transform outlet, Vector3 start, Vector3 end, out Vector3 c1, out Vector3 c2)
    {
        Vector3 dir = (end - start);
        float dist = Mathf.Max(dir.magnitude, 0.5f);

        Vector3 fwd = outlet.forward;
        Vector3 side = outlet.right;

        // stable curve sign so it doesn’t flip randomly
        float sign = (outlet.GetInstanceID() % 2 == 0) ? 1f : -1f;

        Vector3 mid1 = start + fwd * (dist * 0.35f);
        Vector3 mid2 = start + fwd * (dist * 0.70f);

        c1 = mid1 + side * (curveSideStrength * sign) + Vector3.up * curveUpStrength;
        c2 = mid2 - side * (curveSideStrength * sign * 0.6f) + Vector3.up * (curveUpStrength * 0.5f);
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos || outlets == null) return;

        Gizmos.color = Color.cyan;
        foreach (var outlet in outlets)
        {
            if (!outlet) continue;
            Gizmos.DrawWireSphere(outlet.position, 0.06f);
            Gizmos.DrawLine(outlet.position, outlet.position + outlet.forward * fallbackDistance);
        }
    }
}
