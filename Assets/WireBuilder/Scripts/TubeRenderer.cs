// Author: Mathias Soeholm
// Date: 05/10/2016
// No license, do whatever you want with this script
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public class TubeRenderer : MonoBehaviour
{
	[SerializeField] Vector3[] _positions;
	[SerializeField] int _sides;
	[SerializeField] float _radiusOne;
	[SerializeField] float _radiusTwo;
	[SerializeField] bool _useWorldSpace = true;
	[SerializeField] bool _useTwoRadii = false;
	
	private Vector3[] _vertices;
	private Mesh _mesh;
	private MeshFilter _meshFilter;
	private MeshRenderer _meshRenderer;

	public Material material
	{
		get { return _meshRenderer.material; }
		set { _meshRenderer.material = value; }
	}

	void Awake()
	{
		_meshFilter = GetComponent<MeshFilter>();
		if (_meshFilter == null)
		{
			_meshFilter = gameObject.AddComponent<MeshFilter>();
		}

		_meshRenderer = GetComponent<MeshRenderer>();
		if (_meshRenderer == null)
		{
			_meshRenderer = gameObject.AddComponent<MeshRenderer>();
		}

		_mesh = new Mesh();
		_meshFilter.mesh = _mesh;
        _meshCollider = GetComponent<MeshCollider>();
	}

    private MeshCollider _meshCollider;

	private void OnEnable()
	{
		_meshRenderer.enabled = true;
	}

	private void OnDisable()
	{
		_meshRenderer.enabled = false;
	}

	void Update ()
	{
		GenerateMesh();
	}


	public void SetPositions(Vector3[] positions)
	{
		_positions = positions;
		GenerateMesh();
	}

	private void GenerateMesh()
	{
		if (_mesh == null || _positions == null || _positions.Length <= 1)
		{
			_mesh = new Mesh();
			return;
		}

		var verticesLength = _sides * _positions.Length;
		if (_vertices == null || _vertices.Length != verticesLength)
		{
			_vertices = new Vector3[verticesLength];

			var indices = GenerateIndices();
			var uvs = GenerateUVs();

			if (verticesLength > _mesh.vertexCount)
			{
				_mesh.vertices = _vertices;
				_mesh.triangles = indices;
				_mesh.uv = uvs;
			}
			else
			{
				_mesh.triangles = indices;
				_mesh.vertices = _vertices;
				_mesh.uv = uvs;
			}
		}

		// Calculate frames using Parallel Transport
		Vector3[] forwards = new Vector3[_positions.Length];
		Vector3[] rights = new Vector3[_positions.Length];
		Vector3[] ups = new Vector3[_positions.Length];

		// Calculate Forwards
		for (int i = 0; i < _positions.Length; i++)
		{
			Vector3 f;
			if (i < _positions.Length - 1)
			{
				f = (_positions[i + 1] - _positions[i]).normalized;
			}
			else
			{
				f = (_positions[i] - _positions[i - 1]).normalized;
			}
			
			forwards[i] = f;

			// For internal points, average the tangents for smoother joints (optional but recommended)
			if (i > 0 && i < _positions.Length - 1)
			{
				Vector3 fBefore = (_positions[i] - _positions[i - 1]).normalized;
				Vector3 fAfter = (_positions[i + 1] - _positions[i]).normalized;
				forwards[i] = (fBefore + fAfter).normalized;
			}
		}

		// Calculate Initial Frame
		// We use a stable Up vector (World Up) unless the cable is vertical
		Vector3 f0 = forwards[0];
		Vector3 upHint = Vector3.up;
		if (Mathf.Abs(Vector3.Dot(f0, upHint)) > 0.95f) upHint = Vector3.right; // Handle vertical start

		rights[0] = Vector3.Cross(upHint, f0).normalized;
		ups[0] = Vector3.Cross(f0, rights[0]).normalized;

		// Propagate Frame (Parallel Transport)
		for (int i = 1; i < _positions.Length; i++)
		{
			Vector3 fOld = forwards[i - 1];
			Vector3 fNew = forwards[i];
			
			// Find the rotation that aligns the old tangent to the new tangent
			Quaternion rot = Quaternion.FromToRotation(fOld, fNew);
			
			rights[i] = rot * rights[i - 1];
			ups[i] = rot * ups[i - 1];
		}

		// Generate Vertices
		var currentVertIndex = 0;
		var angleStep = (2 * Mathf.PI) / _sides;

		for (int i = 0; i < _positions.Length; i++)
		{
			float t = i / (_positions.Length - 1f);
			float radius = _useTwoRadii ? Mathf.Lerp(_radiusOne, _radiusTwo, t) : _radiusOne;
			
			Vector3 pos = _positions[i];
			Vector3 right = rights[i];
			Vector3 up = ups[i];

			float angle = 0f;
			for (int side = 0; side < _sides; side++)
			{
				float x = Mathf.Cos(angle);
				float y = Mathf.Sin(angle);

				// Circle vertex in local frame defined by right/up
				Vector3 vertex = pos + (right * x * radius) + (up * y * radius);

				_vertices[currentVertIndex++] = _useWorldSpace ? transform.InverseTransformPoint(vertex) : vertex;

				angle += angleStep;
			}
		}

		_mesh.vertices = _vertices;
		_mesh.RecalculateNormals();
		_mesh.RecalculateBounds();

		_meshFilter.mesh = _mesh;
        if (_meshCollider != null) _meshCollider.sharedMesh = _mesh;
	}

	private Vector2[] GenerateUVs()
	{
		var uvs = new Vector2[_positions.Length*_sides];

		for (int segment = 0; segment < _positions.Length; segment++)
		{
			for (int side = 0; side < _sides; side++)
			{
				var vertIndex = (segment * _sides + side);
				var u = side/(_sides-1f);
				var v = segment/(_positions.Length-1f);

				uvs[vertIndex] = new Vector2(u, v);
			}
		}

		return uvs;
	}

	private int[] GenerateIndices()
	{
		// Two triangles and 3 vertices
		var indices = new int[_positions.Length*_sides*2*3];

		var currentIndicesIndex = 0;
		for (int segment = 1; segment < _positions.Length; segment++)
		{
			for (int side = 0; side < _sides; side++)
			{
				var vertIndex = (segment*_sides + side);
				var prevVertIndex = vertIndex - _sides;

				// Triangle one
				indices[currentIndicesIndex++] = prevVertIndex;
				indices[currentIndicesIndex++] = (side == _sides - 1) ? (vertIndex - (_sides - 1)) : (vertIndex + 1);
				indices[currentIndicesIndex++] = vertIndex;
				

				// Triangle two
				indices[currentIndicesIndex++] = (side == _sides - 1) ? (prevVertIndex - (_sides - 1)) : (prevVertIndex + 1);
				indices[currentIndicesIndex++] = (side == _sides - 1) ? (vertIndex - (_sides - 1)) : (vertIndex + 1);
				indices[currentIndicesIndex++] = prevVertIndex;
			}
		}

		return indices;
	}


}