using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Selectable : MonoBehaviour
{
	private static uint _nextID { get; set; }
	private uint GetNextUniqueID()
	{
		return ++_nextID;
	}
	[RuntimeInitializeOnLoadMethod(loadType: RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Initialize()
	{
		SceneManager.activeSceneChanged += (_, _) => _nextID = 0; ;
	}

	[field: Layer, SerializeField]
	public int SelectablesLayer { get; private set; }
	public uint ID { get; private set; }
	public SelectionStatus Status { get; private set; }

	private void Awake()
	{
		ID = GetNextUniqueID();
		gameObject.layer = SelectablesLayer;
		GetComponent<Renderer>().material.SetColor("_UnlitColor", SelectionUtility.IDToColor(ID));
	}
	private void OnEnable()
	{
		Selection.Register(this);
	}
	private void OnDisable()
	{
		Selection.Unregister(this);
	}
	public void Select()
	{
		Status |= SelectionStatus.Selected;
	}

	public void Deselect()
	{
		Status ^= SelectionStatus.Selected;
	}

	public void HoverExit()
	{
		Status ^= SelectionStatus.Hovered;
	}

	public void HoverEnter()
	{
		Status |= SelectionStatus.Hovered;
	}
}