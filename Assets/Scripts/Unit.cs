using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
	private static HashSet<Unit> _selectedUnits = new();
	public static IReadOnlyList<Unit> SelectedUnits => new List<Unit>(_selectedUnits);

	private Selectable selectable;
	private NavMeshAgent agent;
	public bool IsSelected => selectable.Status.HasFlag(SelectionStatus.Selected);
	private void Awake()
	{
		selectable = GetComponent<Selectable>();
		agent = GetComponent<NavMeshAgent>();
		_selectedUnits?.Clear();
	}

	private void OnEnable()
	{
		Selection.OnSelect += Selection_OnSelect;
		Selection.OnDeselect += Selection_OnDeselect;	
	}
	private void OnDisable()
	{
		Selection.OnSelect -= Selection_OnSelect;
		Selection.OnDeselect -= Selection_OnDeselect;
	}

	private void Selection_OnDeselect(Selectable selectable)
	{
		if (selectable == this.selectable)
		{
			_selectedUnits.Remove(this);
		}
	}

	private void Selection_OnSelect(Selectable selectable)
	{
		if(selectable == this.selectable)
		{
			_selectedUnits.Add(this);
		}
	}

	public void MoveToLocation(Vector3 point)
	{
		agent.destination = point;
	}
}
