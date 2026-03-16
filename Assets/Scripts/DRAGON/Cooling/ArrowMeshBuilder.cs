#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ArrowMeshBuilder : MonoBehaviour
{
    [Header("Arrow Shape")]
    [Range(0.05f, 2f)] public float length = 0.30f;
    [Range(0.02f, 1f)] public float shaftWidth = 0.06f;
    [Range(0.02f, 1f)] public float headLength = 0.10f;
    [Range(0.02f, 1f)] public float headWidth = 0.12f;

    [Header("3D")]
    [Range(0.001f, 0.2f)] public float thickness = 0.02f; // ความหนาแกน Y

    void Awake() => Build();

#if UNITY_EDITOR
    void OnValidate()
    {
        EditorApplication.delayCall += () =>
        {
            if (this == null) return;
            Build();
        };
    }
#endif

    void Build()
    {
        var mf = GetComponent<MeshFilter>();
        var mesh = new Mesh();
        mesh.name = "ArrowMesh_3D";

        float L = Mathf.Max(0.01f, length);
        float hw = shaftWidth * 0.5f;
        float hh = headWidth * 0.5f;
        float hl = Mathf.Clamp(headLength, 0.01f, L * 0.9f);
        float shaftEnd = L - hl;

        float yTop = thickness * 0.5f;
        float yBot = -thickness * 0.5f;

        // outline points on XZ plane (7 points)
        Vector3[] outline =
        {
            new Vector3(-hw, 0, 0),           // 0
            new Vector3( hw, 0, 0),           // 1
            new Vector3( hw, 0, shaftEnd),    // 2
            new Vector3( hh, 0, shaftEnd),    // 3 (head right)
            new Vector3(  0, 0, L),           // 4 (tip)
            new Vector3(-hh, 0, shaftEnd),    // 5 (head left)
            new Vector3(-hw, 0, shaftEnd),    // 6
        };

        // build top & bottom vertices
        // top: 0..6, bottom: 7..13
        Vector3[] v = new Vector3[14];
        for (int i = 0; i < 7; i++)
        {
            v[i] = new Vector3(outline[i].x, yTop, outline[i].z);
            v[i + 7] = new Vector3(outline[i].x, yBot, outline[i].z);
        }

        // Triangles:
        // Top face (fan from a center-ish index -> we'll use a triangle fan from point 0 is not safe
        // We'll triangulate manually for this simple arrow polygon:
        // Polygon order: 0-1-2-3-4-5-6 (counter-clockwise looking from +Y)
        // We'll make top triangles: (0,1,6) + (1,2,6) + (2,5,6)? not correct
        // Safer: hand-triangulate into 5 tris:
        // (0,1,6), (1,2,6), (2,5,6), (2,3,5), (3,4,5)
        // And bottom face reversed winding.

        var tris = new System.Collections.Generic.List<int>(100);

        // --- TOP (0..6)
        AddTri(tris, 0, 1, 6);
        AddTri(tris, 1, 2, 6);
        AddTri(tris, 2, 5, 6);
        AddTri(tris, 2, 3, 5);
        AddTri(tris, 3, 4, 5);

        // --- BOTTOM (7..13) reverse winding
        AddTri(tris, 7 + 6, 7 + 1, 7 + 0);
        AddTri(tris, 7 + 6, 7 + 2, 7 + 1);
        AddTri(tris, 7 + 6, 7 + 5, 7 + 2);
        AddTri(tris, 7 + 5, 7 + 3, 7 + 2);
        AddTri(tris, 7 + 5, 7 + 4, 7 + 3);

        // --- SIDES connect outline edges (0-1-2-3-4-5-6-0)
        int[] loop = { 0, 1, 2, 3, 4, 5, 6, 0 };
        for (int i = 0; i < loop.Length - 1; i++)
        {
            int aTop = loop[i];
            int bTop = loop[i + 1];
            int aBot = aTop + 7;
            int bBot = bTop + 7;

            // two tris per quad (aTop-bTop-bBot) + (aTop-bBot-aBot)
            AddTri(tris, aTop, bTop, bBot);
            AddTri(tris, aTop, bBot, aBot);
        }

        mesh.vertices = v;
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        mf.mesh = mesh;
    }

    static void AddTri(System.Collections.Generic.List<int> t, int a, int b, int c)
    {
        t.Add(a); t.Add(b); t.Add(c);
    }
}
