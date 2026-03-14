using UnityEngine;

public class PAC_AirArrowFollower : MonoBehaviour
{
    [System.Serializable]
    public struct PathConfig
    {
        public Vector3 start, c1, c2, end;
        public float speed;
        public float phase;
        public float waveStrength;
        public float waveFrequency;
    }

    private PathConfig _cfg;
    private float _t;
    private bool _inited;

    public void Init(PathConfig cfg)
    {
        _cfg = cfg;
        _t = Mathf.Repeat(cfg.phase, 1f);
        _inited = true;
        UpdatePose(Time.time);
    }

    void Update()
    {
        if (!_inited) return;

        _t = Mathf.Repeat(_t + (_cfg.speed * Time.deltaTime * 0.35f), 1f);
        UpdatePose(Time.time);
    }

    void UpdatePose(float time)
    {
        Vector3 p = Bezier(_cfg.start, _cfg.c1, _cfg.c2, _cfg.end, _t);

        Vector3 tangent = BezierTangent(_cfg.start, _cfg.c1, _cfg.c2, _cfg.end, _t).normalized;
        Vector3 side = Vector3.Cross(Vector3.up, tangent).normalized;
        if (side.sqrMagnitude < 0.0001f) side = Vector3.right;

        float wave = Mathf.Sin((time * _cfg.waveFrequency) + (_t * 6.28318f)) * _cfg.waveStrength;
        p += side * wave;

        transform.position = p;

        if (tangent.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(tangent, Vector3.up);
    }

    static Vector3 Bezier(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        float u = 1f - t;
        return (u*u*u) * a
             + (3f*u*u*t) * b
             + (3f*u*t*t) * c
             + (t*t*t) * d;
    }

    static Vector3 BezierTangent(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        float u = 1f - t;
        return (3f*u*u) * (b - a)
             + (6f*u*t) * (c - b)
             + (3f*t*t) * (d - c);
    }
}
