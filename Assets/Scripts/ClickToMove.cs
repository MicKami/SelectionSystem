using UnityEngine;

public class ClickToMove : MonoBehaviour
{
	[SerializeField]
	private float spacing;

	private void Update()
	{
		Plane plane = new Plane(Vector3.up, Vector3.zero);
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (plane.Raycast(ray, out float distance))
		{
			if (Input.GetMouseButton(1))
			{
				var selectedUnits = Unit.SelectedUnits;
				if (selectedUnits.Count == 0) return;
				var points = UnitFormation_Hexagon(selectedUnits.Count);

				for (int i = 0; i < selectedUnits.Count; i++)
				{
					var unit = selectedUnits[i];
					Vector3 point = ray.GetPoint(distance) + points[i];
					unit.MoveToLocation(point);
				}
			}
		}
	}

	private Vector3[] UnitFormation_Hexagon(int count)
	{
		return AxialToWorld(GetAxialCoords(count));
	}

	readonly Vector2Int[] axialDirections =
	{
		new Vector2Int(1, 0),
		new Vector2Int(1, -1),
		new Vector2Int(0, -1),
		new Vector2Int(-1, 0),
		new Vector2Int(-1, 1),
		new Vector2Int(0, 1),
	};

	private Vector2Int[] GetAxialCoords(int count)
	{
		Vector2Int[] result = new Vector2Int[count];
		result[0] = Vector2Int.zero;
		int n = 1;
		int radius = Mathf.CeilToInt((count - 1) / 6f);
		for (int k = 1; k <= radius; k++)
		{
			var hex = axialDirections[4] * k;
			for (int i = 0; i < 6; i++)
			{
				for (int j = 0; j < k; j++)
				{
					if (n >= count) break;
					result[n] = hex;
					hex += axialDirections[i];
					n++;
				}
			}
		}

		return result;
	}

	static readonly float sqrt3 = Mathf.Sqrt(3);
	private Vector3[] AxialToWorld(Vector2Int[] coords)
	{
		Vector3[] result = new Vector3[coords.Length];
		for (int i = 0; i < coords.Length; i++)
		{
			result[i] = AxialToWorld(coords[i]);
		}
		return result;
	}

	private Vector3 AxialToWorld(Vector2Int coord)
	{
		float horizontalSpacing = spacing * sqrt3;
		float verticalSpacing = spacing * 1.5f; ;
		float x = coord.x * horizontalSpacing + (horizontalSpacing / 2) * coord.y;
		float y = coord.y * verticalSpacing;
		return new Vector3(x, 0, y);
	}
}
