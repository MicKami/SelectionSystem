using UnityEngine;

public class Rotate : MonoBehaviour
{
	[SerializeField]
	private float angle;
	[SerializeField]
	private Vector3 axis;

	void Update()
	{
		transform.Rotate(axis, angle * Time.deltaTime);
	}
}
