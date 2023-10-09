using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour
{
	private Vector2 dragBeginPosition;
	private bool mouseUp;

	[field: SerializeField]
	public SelectableIDSampler IDSampler { get; set; }
	[SerializeField]
	private KeyCode AddToSelectionKey = KeyCode.LeftShift;
	[SerializeField]
	private KeyCode RemoveFromSelectionKey = KeyCode.LeftControl;

	public bool IsDragging => SelectionRect.size.x >= 1 && SelectionRect.size.y >= 1 && Input.GetMouseButton(0);
	private bool AddModifierPressed => Input.GetKey(AddToSelectionKey);
	private bool RemoveModifierPressed => Input.GetKey(RemoveFromSelectionKey);
	public Rect SelectionRect
	{
		get
		{
			if (Input.GetMouseButton(0))
			{
				var clampedMousePos = ClampedMousePosition();
				Vector2 min = Vector2.Min(dragBeginPosition, clampedMousePos);
				Vector2 max = Vector2.Max(dragBeginPosition, clampedMousePos);
				Vector2 size = max - min;
				return new Rect(new Vector2(min.x, min.y), size);
			}
			else return new Rect(ClampedMousePosition(), Vector2.zero);
		}
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			dragBeginPosition = ClampedMousePosition();
		}
		if (IsMouseWithinScreen() || IsDragging)
		{
			IDSampler.Sample(SelectionRect, HandleSelection);
		}
		mouseUp = Input.GetMouseButtonUp(0);
	}
	private void HandleSelection(HashSet<uint> ids)
	{
		Selection.SetHover(ids);
		if (mouseUp)
		{
			if (AddModifierPressed)
			{
				Selection.Add(ids);
			}
			else if (RemoveModifierPressed)
			{
				Selection.Remove(ids);
			}
			else Selection.Set(ids);
		}
	}
	private bool IsMouseWithinScreen()
	{
		Vector2 position = ClampedMousePosition();
		return position.x >= 0 && ((int)position.x) < (Screen.width - 1) &&
			   position.y >= 0 && ((int)position.y) < (Screen.height - 1);
	}

	private Vector2 ClampedMousePosition()
	{
		return Vector2.Max(Vector2.zero, Vector2.Min(new Vector2(Mathf.Ceil(Input.mousePosition.x), Mathf.Ceil(Input.mousePosition.y)), new Vector2(Screen.width, Screen.height)));
	}
}
