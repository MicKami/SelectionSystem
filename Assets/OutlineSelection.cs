using UnityEngine;

public class OutlineSelection : MonoBehaviour
{
	[Layer]
	public int selectedLayer;
	[Layer]
	public int hoverLayer;

	private void Update()
	{
		foreach (var item in Selection.Selectables)
		{
			item.gameObject.layer = Selection.ActiveContains(item) ? selectedLayer : Selection.HoverContains(item) ? hoverLayer : item.SelectablesLayer;
		}
	}
}
