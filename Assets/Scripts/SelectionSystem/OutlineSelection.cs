using UnityEngine;

public class OutlineSelection : MonoBehaviour
{
	[field: Layer, SerializeField]
	public int SelectedLayer { get; set; }
	[field: Layer, SerializeField]
	public int HoverLayer { get; set; }

	private void OnEnable()
	{
		Selection.OnSelect += Selection_SetLayer;
		Selection.OnDeselect += Selection_SetLayer;
		Selection.OnHoverEnter += Selection_SetLayer;
		Selection.OnHoverExit += Selection_SetLayer; ;
	}
	private void Selection_SetLayer(Selectable selectable)
	{
		var layer = selectable.SelectablesLayer;
		if (selectable.Status.HasFlag(SelectionStatus.Selected))
		{
			layer = SelectedLayer;
		}
		else if (selectable.Status.HasFlag(SelectionStatus.Hovered))
		{
			layer = HoverLayer;
		}
		selectable.gameObject.layer = layer;
	}
	private void OnDisable()
	{
		Selection.OnSelect -= Selection_SetLayer;
		Selection.OnDeselect -= Selection_SetLayer;
		Selection.OnHoverEnter -= Selection_SetLayer;
		Selection.OnHoverExit -= Selection_SetLayer;
	}
}
