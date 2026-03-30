using UnityEngine;

public class PAC_AirArrowFollower : MonoBehaviour
{
    [System.Serializable]
    public struct PathConfig
    {
        public Vector3 start, c1, c2, end;
        public float speed;          // m/s-ish
        public float phase;          // 0..1 initial t
        public float waveStrength;
        public float waveFrequency;
        public bool lockWaveToFixedSide;

        // NEW
        public float nearScale;      // scale at start
        public float farScale;       // scale at end
        public float ttl;            // seconds
        public float nearLength;  // ระยะใกล้ = สั้น
        public float farLength;   // ระยะไกล = ยาวขึ้น
    }

    private PathConfig _cfg;
    private float _t;
    private bool _inited;

    private Vector3 _fixedSide;
    private float _life;
    private Vector3 _prevPos;

    public void Init(PathConfig cfg)
    {
        _cfg = cfg;
        _t = Mathf.Clamp01(Mathf.Repeat(cfg.phase, 1f));
        _inited = true;

        _life = 0f;
        CacheFixedSide();

        _prevPos = Bezier(_cfg.start, _cfg.c1, _cfg.c2, _cfg.end, _t);
        UpdatePose(Time.time);
        ApplyScaleTaper();
    }

    void Update()
    {
        if (!_inited) return;

        _life += Time.deltaTime;
        if (_cfg.ttl > 0f && _life >= _cfg.ttl)
        {
            Destroy(gameObject);
            return;
        }

        // advance t by speed normalized with distance (กันเร็วเพี้ยน)
        float approxLen = Mathf.Max(0.25f, Vector3.Distance(_cfg.start, _cfg.end));
        float dt = (_cfg.speed / approxLen) * Time.deltaTime;
        _t += dt;

        if (_t >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        UpdatePose(Time.time);
        ApplyScaleTaper();
    }

    void CacheFixedSide()
    {
        Vector3 mainDir = (_cfg.end - _cfg.start);
        if (mainDir.sqrMagnitude < 0.0001f) mainDir = Vector3.forward;
        mainDir.Normalize();

        _fixedSide = Vector3.Cross(Vector3.up, mainDir).normalized;
        if (_fixedSide.sqrMagnitude < 0.0001f) _fixedSide = Vector3.right;
    }

    void UpdatePose(float time)
    {
        Vector3 p = Bezier(_cfg.start, _cfg.c1, _cfg.c2, _cfg.end, _t);

        Vector3 tangent = BezierTangent(_cfg.start, _cfg.c1, _cfg.c2, _cfg.end, _t);
        if (tangent.sqrMagnitude < 0.000001f) tangent = (_cfg.end - _cfg.start);
        tangent.Normalize();

        if (_cfg.waveStrength > 0f)
        {
            Vector3 wobbleDir = _cfg.lockWaveToFixedSide
                ? _fixedSide
                : Vector3.Cross(Vector3.up, tangent).normalized;

            float wave = Mathf.Sin((time * _cfg.waveFrequency) + (_t * Mathf.PI * 2f)) * _cfg.waveStrength;
            p += wobbleDir * wave;
        }

        transform.position = p;
        transform.rotation = Quaternion.LookRotation(tangent, Vector3.up);

        _prevPos = p;
    }

    void ApplyScaleTaper()
    {
        float s = Mathf.Lerp(_cfg.nearScale, _cfg.farScale, _t);
        float len = Mathf.Lerp(_cfg.nearLength, _cfg.farLength, _t);

        // สมมติ arrow ยาวตามแกน Z
        transform.localScale = new Vector3(s, s, s * len);
    }

    static Vector3 Bezier(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        float u = 1f - t;
        return (u * u * u) * a
             + (3f * u * u * t) * b
             + (3f * u * t * t) * c
             + (t * t * t) * d;
    }

    static Vector3 BezierTangent(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        float u = 1f - t;
        return (3f * u * u) * (b - a)
             + (6f * u * t) * (c - b)
             + (3f * t * t) * (d - c);
    }
}
