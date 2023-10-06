using System;
using System.Collections.Generic;
using UnityEditor;

public static class Selection
{

	private static Dictionary<uint, SelectableBase> _selectables;
	private static Dictionary<uint, SelectableBase> selectables
	{
		get { return _selectables ??= new(); }
	}
	public static IEnumerable<SelectableBase> Selectables
	{
		get { return selectables.Values; }
	}

	private static HashSet<uint> _active;
	private static HashSet<uint> active
	{
		get { return _active ??= new(); }
	}
	public static IEnumerable<uint> Active
	{
		get { return active; }
	}

	private static HashSet<uint> _hover;
	private static HashSet<uint> hover
	{
		get { return _hover ??= new(); }
	}
	public static IEnumerable<uint> Hover
	{
		get { return hover; }
	}

	public static bool Register(SelectableBase selectable)
	{
		return selectables.TryAdd(selectable.ID, selectable);
	}
	public static bool Unregister(SelectableBase selectable)
	{
		return selectables.Remove(selectable.ID);
	}

	public static void ClearActive()
	{
		foreach (uint id in active)
		{
			selectables[id].Deselect();
		}
		active.Clear();
	}
	public static void Set(HashSet<uint> ids)
	{
		HashSet<uint> old = new(active);
		old.ExceptWith(ids);
		ids.ExceptWith(active);
		Remove(old);
		Add(ids);
	}
	private static void Remove(uint id)
	{
		if (selectables.ContainsKey(id))
		{
			if (active.Remove(id))
			{
				selectables[id].Deselect();
			}
		}
	}
	public static void Remove(HashSet<uint> ids)
	{
		foreach (var id in ids)
		{
			Remove(id);
		}
	}
	private static void Add(uint id)
	{
		if (selectables.ContainsKey(id))
		{
			if (active.Add(id))
			{
				selectables[id].Select();
			}
		}
	}
	public static void Add(HashSet<uint> ids)
	{
		foreach (uint id in ids)
		{
			Add(id);
		}
	}

	public static bool ActiveContains(SelectableBase item)
	{
		return active.Contains(item.ID);
	}

	public static bool HoverContains(SelectableBase item)
	{
		return hover.Contains(item.ID);
	}
	public static void ClearHover()
	{
		hover.Clear();
	}
	public static void SetHover(HashSet<uint> ids)
	{
		_hover = ids;
	}

#if UNITY_EDITOR
	[InitializeOnLoadMethod]
	private static void Initialize()
	{
		EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
	}

	private static void EditorApplication_playModeStateChanged(PlayModeStateChange state)
	{
		if (state == PlayModeStateChange.ExitingPlayMode)
		{
			ClearActive();
			ClearHover();
		}
	}

#endif
}
