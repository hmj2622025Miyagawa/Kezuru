using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class StoneCarver : MonoBehaviour
{
	[Header("水の設定")]
	[SerializeField] ParticleSystem waterEffect;
	[SerializeField] float waterRange = 20.0f;
	[SerializeField] float waterDistance = 3f;
	[SerializeField] private ParticleSystem waterSplashEffect;

	[Header("石を削る設定")]
	[SerializeField] float carveRadius = 0.25f;
	[SerializeField] float carveStrength = 0.04f;

	[Header("石の凹凸設定")]
	[SerializeField] float roughness = 0.12f;
	[SerializeField] int rockSeed = 12345;

	[Header("石の残量設定")]
	[SerializeField] Slider completionSlider;
	[SerializeField] string clearSceneName = "ClearScene";

	[SerializeField] private ParticleSystem stoneBreakEffect;

	[SerializeField] private AudioSource waterAudio;
	[SerializeField] private AudioSource carvingAudio;
	[SerializeField] private AudioSource clearAudio;

	// 石の残量がこの割合以下になったらクリア
	[SerializeField, Range(0.001f, 0.3f)]
	float remainingThreshold = 0.01f;

	private Mesh mesh;
	private Vector3[] originalVertices;
	private Vector3[] vertices;
	private MeshCollider meshCollider;
	private Camera mainCamera;

	private float initialStoneSize;
	private bool isCleared = false;

	void Start()
	{
		GameTimer.StartTimer();

		transform.localScale *= DefficultManager.GetStoneScale();

		mainCamera = Camera.main;
		meshCollider = GetComponent<MeshCollider>();

		MeshRenderer renderer = GetComponent<MeshRenderer>();
		Material originalMaterial = renderer.sharedMaterial;

		CreateStoneMesh();

		// 元のマテリアルを維持
		renderer.sharedMaterial = originalMaterial;

		// 初期状態の石の大きさを保存
		initialStoneSize = CalculateStoneSize();

		// 残量ゲージを初期化
		if (completionSlider != null)
		{
			completionSlider.minValue = 0f;
			completionSlider.maxValue = 1f;
			completionSlider.value = 1f;
		}
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
			float latitude = Mathf.PI * y / rings;

			for (int x = 0; x <= segments; x++)
			{
				float longitude =
					2f * Mathf.PI * x / segments;

				int i = y * (segments + 1) + x;

				v[i] = new Vector3(
					Mathf.Sin(latitude) * Mathf.Cos(longitude),
					Mathf.Cos(latitude),
					Mathf.Sin(latitude) * Mathf.Sin(longitude)
				);

				// 自然なランダム凹凸
				float seedOffset = rockSeed * 0.013f;

				float noise = Mathf.PerlinNoise((v[i].x + seedOffset) * 4.7f,(v[i].y + seedOffset) * 4.7f);

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
				triangles[t++] = a + 1;
				triangles[t++] = b;

				triangles[t++] = a + 1;
				triangles[t++] = b + 1;
				triangles[t++] = b;
			}
		}

		mesh.vertices = v;
		mesh.triangles = triangles;

		// テクスチャ用UV座標
		Vector2[] uvs = new Vector2[v.Length];

		for (int y = 0; y <= rings; y++)
		{
			for (int x = 0; x <= segments; x++)
			{
				int i = y * (segments + 1) + x;

				uvs[i] = new Vector2((float)x / segments,(float)y / rings);
			}
		}

		mesh.uv = uvs;

		mesh.RecalculateNormals();
		mesh.RecalculateBounds();

		MeshFilter meshFilter = GetComponent<MeshFilter>();
		meshFilter.mesh = mesh;

		vertices = mesh.vertices;
		originalVertices = mesh.vertices;

		// 元のマテリアルを維持
		MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
		meshRenderer.enabled = true;

		meshCollider.sharedMesh = null;
		meshCollider.sharedMesh = mesh;
	}

	void Update()
	{
		if (isCleared)
			return;

		if (mainCamera == null || Mouse.current == null)
			return;

		Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

		bool isClicking = Mouse.current.leftButton.isPressed;

		if (!isClicking)
		{
			if (waterEffect != null && waterEffect.isPlaying)
				waterEffect.Stop();

			if (waterAudio != null && waterAudio.isPlaying)
				waterAudio.Stop();

			if (carvingAudio != null && carvingAudio.isPlaying)
			{
				carvingAudio.Stop();
			}
			return;
		}

		if (waterEffect != null && !waterEffect.isPlaying)
			waterEffect.Play();

		if (waterAudio != null && !waterAudio.isPlaying)
			waterAudio.Play();

		if (Physics.Raycast(ray, out RaycastHit hit, waterRange))
		{
			if (hit.collider.gameObject == gameObject)
			{
				CarveStone(hit.point);
				if (waterSplashEffect != null && !waterSplashEffect.isPlaying)
				{
					waterSplashEffect.transform.position = hit.point;

					waterSplashEffect.transform.rotation = Quaternion.LookRotation(hit.normal);

					waterSplashEffect.Play();
				}

				if (waterEffect != null && !isCleared)
				{
					waterEffect.transform.position = hit.point - ray.direction * waterDistance;

					waterEffect.transform.rotation = Quaternion.LookRotation(ray.direction);
				}
			}
		}
	}

	void CarveStone(Vector3 worldPoint)
	{
		Vector3 localPoint = transform.InverseTransformPoint(worldPoint);

		for (int i = 0; i < vertices.Length; i++)
		{
			float distance = Vector3.Distance(vertices[i], localPoint);

			if (distance < carveRadius)
			{
				float strength = 1f - distance / carveRadius;

				strength = strength * strength *
					(3f - 2f * strength);

				float variation = Mathf.PerlinNoise( vertices[i].x * 18f + 10f, vertices[i].y * 18f + 10f);

				float depth = carveStrength * strength * Mathf.Lerp(0.75f, 1.25f, variation);

				vertices[i] -= vertices[i].normalized * depth;
			}
		}

		mesh.vertices = vertices;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();

		meshCollider.sharedMesh = null;
		meshCollider.sharedMesh = mesh;

		if (carvingAudio != null && !carvingAudio.isPlaying)
		{
			carvingAudio.Play();
		}

		UpdateRemaining();
	}

	// 石の大きさを計算する
	float CalculateStoneSize()
	{
		if (vertices == null || vertices.Length == 0)
			return 0f;

		float total = 0f;

		foreach (Vector3 vertex in vertices)
		{
			float radius = vertex.magnitude;
			total += radius * radius * radius;
		}

		return total / vertices.Length;
	}

	// 石の残量ゲージを更新する
	void UpdateRemaining()
	{
		float currentSize = CalculateStoneSize();

		float remaining = initialStoneSize > 0f ? Mathf.Clamp01(currentSize / initialStoneSize) : 0f;

		if (completionSlider != null)
			completionSlider.value = remaining;

		// 石がほぼなくなったらクリア
		if (remaining <= remainingThreshold && !isCleared)
		{
			isCleared = true;
			GameTimer.StopTimer();

			if (waterEffect != null)
			{
				waterEffect.Stop();
			}

			if (clearAudio != null)
			{
				clearAudio.Play();
			}

			if (stoneBreakEffect != null)
			{
				stoneBreakEffect.transform.position = transform.position;
				stoneBreakEffect.Play();
			}

			GetComponent<MeshRenderer>().enabled = false;
			GetComponent<MeshCollider>().enabled = false;

			StartCoroutine(ClearAfterEffect());
		}

		System.Collections.IEnumerator ClearAfterEffect()
		{
			// クリア音が再生され終わるまで待つ
			if (clearAudio != null && clearAudio.clip != null)
			{
				yield return new WaitWhile(() => clearAudio.isPlaying);
			}

			yield return new WaitForSeconds(0.5f);

			SceneManager.LoadScene(clearSceneName);
		}
	}
}