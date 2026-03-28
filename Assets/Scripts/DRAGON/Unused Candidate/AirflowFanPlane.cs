using UnityEngine;

[ExecuteAlways]
public class AirflowFanPlane : MonoBehaviour
{
    [Header("Shape (Fan / Trapezoid)")]
    [Min(0.1f)] public float length = 3f;
    [Min(0.01f)] public float nearWidth = 0.35f;
    [Min(0.01f)] public float farWidth = 1.2f;

    [Header("Visual")]
    public Color color = new Color(0.2f, 0.65f, 1f, 1f); // ฟ้า
    [Range(0f, 1f)] public float alpha = 0.25f;
    public bool doubleSided = true;

    [Header("Fade (near -> far)")]
    [Range(0f, 1f)] public float nearAlphaMul = 1f;
    [Range(0f, 1f)] public float farAlphaMul = 0.05f;

    [Header("Motion")]
    public bool invertScrollDirection = false;

    public bool animate = true;
    [Min(0f)] public float scrollSpeed = 0.6f;
    [Min(0f)] public float pulseSpeed = 1.5f;
    [Range(0f, 0.5f)] public float pulseAmount = 0.10f;

    [Header("Editor")]
    public bool autoRebuild = true;

    MeshFilter mf;
    MeshRenderer mr;
    Material runtimeMat;

    void OnEnable()
    {
        Ensure();
        Rebuild();
        ApplyColor(alpha);
    }

    void OnValidate()
    {
        if (!autoRebuild) return;
        Ensure();
        Rebuild();
        ApplyColor(alpha);
    }

    void Update()
    {
        if (!animate || runtimeMat == null) return;

        // scroll texture (ให้มันดูไหล)
        if (runtimeMat.HasProperty("_MainTex") && runtimeMat.mainTexture != null)
        {
            Vector2 off = runtimeMat.mainTextureOffset;
            float dir = invertScrollDirection ? -1f : 1f;
            off.y += dir * scrollSpeed * Time.deltaTime;
            runtimeMat.mainTextureOffset = off;
        }

        // pulse alpha
        float a = alpha;
        if (pulseAmount > 0f && pulseSpeed > 0f)
            a = Mathf.Clamp01(alpha + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount);

        ApplyColor(a);
    }

    void Ensure()
    {
        mf = GetComponent<MeshFilter>() ?? gameObject.AddComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();

        if (runtimeMat == null)
        {
            // Shader ที่ tint สีชัวร์
            Shader sh = Shader.Find("Unlit/Transparent") ?? Shader.Find("Unlit/Texture") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
            runtimeMat = new Material(sh);
            runtimeMat.name = "AirflowFanPlane_Mat (Runtime)";

            // Texture ไว้ทำเส้น/ไหล (สีขาว แต่เราจะ tint ด้วย material)
            runtimeMat.mainTexture = BuildAirTexture();
            runtimeMat.mainTexture.wrapMode = TextureWrapMode.Repeat;

            mr.sharedMaterial = runtimeMat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
        }
        else
        {
            mr.sharedMaterial = runtimeMat;
        }
    }

    void ApplyColor(float a)
    {
        if (runtimeMat == null) return;

        Color c = color;
        c.a = a;

        // Built-in Unlit/Transparent ใช้ _Color ได้
        if (runtimeMat.HasProperty("_Color")) runtimeMat.SetColor("_Color", c);
        // Standard บางทีใช้ _Color
        if (runtimeMat.HasProperty("_BaseColor")) runtimeMat.SetColor("_BaseColor", c);
    }

    public void Rebuild()
    {
        length = Mathf.Max(0.1f, length);
        nearWidth = Mathf.Max(0.01f, nearWidth);
        farWidth = Mathf.Max(0.01f, farWidth);

        var mesh = BuildTrapezoidMesh(length, nearWidth, farWidth, doubleSided, nearAlphaMul, farAlphaMul);
        mf.sharedMesh = mesh;
    }

    static Mesh BuildTrapezoidMesh(float length, float nearW, float farW, bool doubleSided, float nearAM, float farAM)
    {
        float n = nearW * 0.5f;
        float f = farW * 0.5f;

        // บนระนาบ X-Z
        Vector3 v0 = new Vector3(-n, 0f, 0f);
        Vector3 v1 = new Vector3(n, 0f, 0f);
        Vector3 v2 = new Vector3(f, 0f, length);
        Vector3 v3 = new Vector3(-f, 0f, length);

        // UV ใช้ให้ texture scroll ได้
        Vector2 uv0 = new Vector2(0f, 0f);
        Vector2 uv1 = new Vector2(1f, 0f);
        Vector2 uv2 = new Vector2(1f, 1f);
        Vector2 uv3 = new Vector2(0f, 1f);

        // Vertex colors ทำ fade ใกล้->ไกล
        Color cNear = new Color(1, 1, 1, Mathf.Clamp01(nearAM));
        Color cFar = new Color(1, 1, 1, Mathf.Clamp01(farAM));

        if (!doubleSided)
        {
            var m = new Mesh();
            m.name = "AirflowFanPlaneMesh";
            m.vertices = new[] { v0, v1, v2, v3 };
            m.uv = new[] { uv0, uv1, uv2, uv3 };
            m.colors = new[] { cNear, cNear, cFar, cFar };
            m.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }
        else
        {
            // หน้า
            Vector3[] verts = { v0, v1, v2, v3, v0, v3, v2, v1 };
            Vector2[] uvs = { uv0, uv1, uv2, uv3, uv0, uv3, uv2, uv1 };
            Color[] cols = { cNear, cNear, cFar, cFar, cNear, cFar, cFar, cNear };
            int[] tris = { 0, 1, 2, 0, 2, 3, 4, 5, 6, 4, 6, 7 };

            var m = new Mesh();
            m.name = "AirflowFanPlaneMesh";
            m.vertices = verts;
            m.uv = uvs;
            m.colors = cols;
            m.triangles = tris;
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }
    }

    static Texture2D BuildAirTexture()
    {
        int w = 64, h = 16;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < h; y++)
        {
            float v = y / (float)(h - 1);
            for (int x = 0; x < w; x++)
            {
                float u = x / (float)(w - 1);

                float a = Mathf.SmoothStep(0.9f, 0.05f, v);
                float stripe = (Mathf.Sin((u * 10f + v * 6f) * Mathf.PI * 2f) * 0.5f + 0.5f);
                a *= Mathf.Lerp(0.65f, 1.0f, stripe);

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }

        tex.Apply(false, false);
        return tex;
    }
}
