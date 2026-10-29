using UnityEngine;
using UnityEngine.InputSystem;

public class CameraOrbit : MonoBehaviour
{
	[SerializeField] private Transform target;

	[SerializeField] private float rotationSpeed = 0.2f;
	[SerializeField] private float zoomSpeed = 0.5f;
	[SerializeField] private float minDistance = 2f;
	[SerializeField] private float maxDistance = 15f;

	private float distance;
	private float yaw;
	private float pitch;

	void Start()
	{
		if (target == null)
		{
			Debug.LogError("CameraOrbit の Target が設定されていません。");
			enabled = false;
			return;
		}

		Vector3 offset = transform.position - target.position;

		distance = offset.magnitude;

		// 難易度に合わせて開始時のカメラ距離を調整
		float stoneScale = DefficultManager.GetStoneScale();
		distance *= stoneScale;

		yaw = transform.eulerAngles.y;
		pitch = transform.eulerAngles.x;

		if (pitch > 180f)
		{
			pitch -= 360f;
		}

		pitch = Mathf.Clamp(pitch, -80f, 80f);
	}

	void LateUpdate()
	{
		if (Mouse.current == null)
		{
			return;
		}

		// 右クリックを押しながらマウスを動かして回転
		if (Mouse.current.rightButton.isPressed)
		{
			Vector2 mouseDelta = Mouse.current.delta.ReadValue();

			yaw += mouseDelta.x * rotationSpeed;
			pitch -= mouseDelta.y * rotationSpeed;

			pitch = Mathf.Clamp(pitch, -80f, 80f);
		}

		// マウスホイールでズーム
		float scroll = Mouse.current.scroll.ReadValue().y;

		distance -= scroll * zoomSpeed * 0.01f;
		distance = Mathf.Clamp(distance, minDistance, maxDistance);

		// 石を中心にカメラを配置
		Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

		transform.position = target.position + rotation * new Vector3(0f, 0f, -distance);

		transform.rotation = rotation;
	}
}