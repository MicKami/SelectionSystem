using UnityEngine;

public class OutlineSelection : MonoBehaviour
{
	[Layer]
	public int selectedLayer;
	[Layer]
	public int hoverLayer;

	private void OnEnable()
	{
		Selection.OnSelect += Selection_OnSelect;
		Selection.OnHoverEnter += Selection_OnHoverEnter;
		Selection.OnDeselect += Selection_Deselect;
		Selection.OnHoverExit += Selection_OnHoverExit; ;
	}

	private void Selection_OnHoverExit(Selectable selectable)
	{
		if (selectable.Status == SelectionStatus.None)
		{
			selectable.gameObject.layer = selectable.SelectablesLayer;
		}
	}

	private void Selection_Deselect(Selectable selectable)
	{
		selectable.gameObject.layer = selectable.Status == SelectionStatus.Hovered ? hoverLayer : selectable.SelectablesLayer;
	}

	private void Selection_OnHoverEnter(Selectable selectable)
	{
		if(selectable.Status == SelectionStatus.Hovered)
		{
			selectable.gameObject.layer = hoverLayer;
		}
	}

	private void Selection_OnSelect(Selectable selectable)
	{
		selectable.gameObject.layer = selectedLayer;
	}

	private void OnDisable()
	{
		Selection.OnSelect -= Selection_OnSelect;
		Selection.OnHoverEnter -= Selection_OnHoverEnter;
		Selection.OnDeselect -= Selection_Deselect;
		Selection.OnHoverExit -= Selection_OnHoverExit;
	}
}
