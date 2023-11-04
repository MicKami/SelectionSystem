using UnityEngine;
using static UnityEngine.Mathf;
public class CameraController : MonoBehaviour
{
	[SerializeField]
	private Transform cameraTransform;
	[SerializeField]
	private float movementSpeed;
	[SerializeField]
	private float movementDuration;
	[SerializeField]
	private Vector2 bounds;

	[SerializeField]
	private float zoomSpeed;
	[SerializeField]
	private float minZoom;
	[SerializeField]
	private float maxZoom;
	[SerializeField]
	private float zoomDuration;

	private float targetZoom;
	private float zoom;
	Vector3 cameraDirection;
	Vector3 newPosition;

	private void Start()
	{
		cameraDirection = (cameraTransform.localPosition - transform.position).normalized;
		targetZoom = Lerp(minZoom, maxZoom, 0.5f);
		zoom = targetZoom;
	}
	private void LateUpdate()
	{
		Move();
		Zoom();
	}

	private void Zoom()
	{
		targetZoom -= Input.mouseScrollDelta.y * zoomSpeed * Time.deltaTime;
		targetZoom = Clamp(targetZoom, minZoom, maxZoom);
		zoom = zoom * Pow(targetZoom / zoom, Time.deltaTime / zoomDuration);
		cameraTransform.localPosition = cameraDirection * zoom;
	}

	private void Move()
	{
		Vector3 movement = transform.forward * Input.GetAxisRaw("Vertical") + transform.right * Input.GetAxisRaw("Horizontal");
		movement.Normalize();
		newPosition += movement * movementSpeed * Time.deltaTime;
		Vector3 min = Vector3.Max(new Vector3(-bounds.x / 2, 0, -bounds.y / 2), newPosition);
		newPosition = Vector3.Min(new Vector3(bounds.x / 2, 0, bounds.y / 2), min);
		transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime / movementDuration);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawCube(newPosition, Vector3.one);
		Gizmos.color = Color.black;
		Gizmos.DrawWireCube(Vector3.up / 2, new Vector3(bounds.x, 1, bounds.y));
	}
}
