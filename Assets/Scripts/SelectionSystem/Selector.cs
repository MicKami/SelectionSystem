using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Selector : MonoBehaviour
{

	[SerializeField]
	private KeyCode AddToSelectionKey = KeyCode.LeftShift;
	[SerializeField]
	private KeyCode RemoveFromSelectionKey = KeyCode.LeftControl;

	private Vector2 dragBeginPosition;
	private bool dragFromUI;
	private bool AddModifierPressed => Input.GetKey(AddToSelectionKey);
	private bool RemoveModifierPressed => Input.GetKey(RemoveFromSelectionKey);
	public bool IsDragging => (SelectionRect.size.x > 1 || SelectionRect.size.y > 1) && Input.GetMouseButton(0) && !dragFromUI;
	public Rect SelectionRect
	{
		get
		{
			if (Input.GetMouseButton(0))
			{
				var clampedMousePos = ClampedMousePosition();
				Vector2 min = Vector2.Min(dragBeginPosition, clampedMousePos);
				Vector2 max = Vector2.Max(dragBeginPosition, clampedMousePos);
				Vector2 size = max - min + Vector2.one;
				return new Rect(new Vector2(min.x, min.y), size);
			}
			else return new Rect(MousePosition(), Vector2.zero);
		}
	}


	private async void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			dragBeginPosition = ClampedMousePosition();
			dragFromUI = EventSystem.current.IsPointerOverGameObject();
		}
		if (IsMouseWithinScreen() || IsDragging)
		{
			var ids = await SelectableIDMapSampler.Sample(SelectionRect);
			HandleSelection(ids);
		}
	}
	private void HandleSelection(IEnumerable<uint> ids)
	{
		Selection.SetHover(ids);
		if (Input.GetMouseButtonUp(0))
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
		Vector2 position = MousePosition();
		return position.x >= 0 && ((int)position.x) < (Screen.width) &&
			   position.y >= 0 && ((int)position.y) < (Screen.height);
	}
	private Vector2 ClampedMousePosition()
	{
		return Vector2.Max(Vector2.zero, Vector2.Min(MousePosition(), new Vector2(Screen.width - 1, Screen.height - 1)));
	}
	private Vector2 MousePosition()
	{
		return new Vector2(Mathf.Floor(Input.mousePosition.x), Mathf.Floor(Input.mousePosition.y));
	}
}
