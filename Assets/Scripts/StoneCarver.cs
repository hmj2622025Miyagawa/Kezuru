using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]

public class StoneCarver : MonoBehaviour
{
	[SerializeField] ParticleSystem waterEffect;
	[SerializeField] float carveRadius = 0.25f;
	[SerializeField] float carveStrength = 0.04f;
	[SerializeField] float waterRange = 20.0f;
	[SerializeField] float waterDistance = 3f;
	[SerializeField] float roughness = 0.12f;
	[SerializeField] int rockSeed = 12345;

	private Mesh mesh;
	private Vector3[] originalVertices;
	private Vector3[] vertices;
	private MeshCollider meshCollider;
	private Camera mainCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
		mainCamera = Camera.main;
		meshCollider = GetComponent<MeshCollider>();

		MeshRenderer renderer = GetComponent<MeshRenderer>();
		Material originalMaterial = renderer.sharedMaterial;

		CreateStoneMesh();

		renderer.sharedMaterial = originalMaterial;
	}

	void CreateStoneMesh()
	{
		mesh = new Mesh();
		mesh.name = "Carvable Stone";

		int rings = 60;
		int segments = 100;

		Vector3[] v =
			new Vector3[(rings + 1) * (segments + 1)];

		int[] triangles = new int[rings * segments * 6];

		for (int y = 0; y <= rings; y++)
		{
			float latitude =
				Mathf.PI * y / rings;

			for (int x = 0; x <= segments; x++)
			{
				float longitude =
					2f * Mathf.PI * x / segments;

				int i = y * (segments + 1) + x;


				v[i] = new Vector3( Mathf.Sin(latitude) * Mathf.Cos(longitude), Mathf.Cos(latitude), Mathf.Sin(latitude) * Mathf.Sin(longitude));

				// 自然な凹凸を作る
				float seedOffset = rockSeed * 0.013f;

				float noise = Mathf.PerlinNoise((v[i].x + seedOffset) * 4.7f, (v[i].y + seedOffset) * 4.7f);

				// 半径を少し変化させる
				float radius = 1f + (noise - 0.5f) * 2f * roughness;

				v[i] *= radius;
			}
		}

		int t = 0;

		for (int y = 0; y < rings; y++)
		{
			for (int x = 0; x < segments; x++)
			{
				int a = y * (segments + 1) + x;
				int b = a + segments + 1;

				triangles[t++] = a;
				triangles[t++] = y * (segments + 1) + x + 1;
				triangles[t++] = b;

				triangles[t++] = y * (segments + 1) + x + 1;
				triangles[t++] = b + 1;
				triangles[t++] = b;
			}
		}

		mesh.vertices = v;
		mesh.triangles = triangles;

		// テクスチャを貼るためのUV座標を設定
		Vector2[] uvs = new Vector2[v.Length];

		for (int y = 0; y <= rings; y++)
		{
			for (int x = 0; x <= segments; x++)
			{
				int i = y * (segments + 1) + x;

				uvs[i] = new Vector2((float)x / segments, (float)y / rings);
			}
		}

		mesh.uv = uvs;

		mesh.RecalculateNormals();
		mesh.RecalculateBounds();

		MeshFilter meshFilter = GetComponent<MeshFilter>();
		meshFilter.mesh = mesh;

		vertices = mesh.vertices;
		originalVertices = mesh.vertices;

		// 元のマテリアルを維持する
		MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
		meshRenderer.enabled = true;

		meshCollider.sharedMesh = null;
		meshCollider.sharedMesh = mesh;
	}

    // Update is called once per frame
    void Update()
    {
		if (mainCamera == null ||
	   Mouse.current == null)
			return;

		Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

		bool isClicking = Mouse.current.leftButton.isPressed;

		if (!isClicking)
		{
			if (waterEffect != null && waterEffect.isPlaying)
			{
				waterEffect.Stop();
			}

			return;
		}

		if (waterEffect != null &&
			!waterEffect.isPlaying)
		{
			waterEffect.Play();
		}

		if (Physics.Raycast(ray, out RaycastHit hit,
			waterRange))
		{
			if (hit.collider.gameObject == gameObject)
			{
				CarveStone(hit.point);

				if (waterEffect != null)
				{
					waterEffect.transform.position = hit.point - ray.direction * waterDistance;

					waterEffect.transform.rotation = Quaternion.LookRotation(ray.direction);
				}
			}
		}
	}

	void CarveStone(Vector3 worldPoint)
	{
		Vector3 localPoint =
			transform.InverseTransformPoint(worldPoint);

		for (int i = 0; i < vertices.Length; i++)
		{
			float distance =
				Vector3.Distance(vertices[i], localPoint);

			if (distance < carveRadius)
			{
				float strength = 1f - distance / carveRadius;

				// 中心ほど深く、縁はなだらかに削る
				strength = strength * strength *
					(3f - 2f * strength);

				// 小さな不規則さを加える
				float variation = Mathf.PerlinNoise(vertices[i].x * 18f + 10f, vertices[i].y * 18f + 10f);

				float depth = carveStrength * strength * Mathf.Lerp(0.75f, 1.25f, variation);

				vertices[i] -= vertices[i].normalized * depth;
			}
		}

		mesh.vertices = vertices;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();

		meshCollider.sharedMesh = null;
		meshCollider.sharedMesh = mesh;
	}
}

