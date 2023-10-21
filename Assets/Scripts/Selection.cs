using System;
using System.Collections.Generic;
using UnityEditor;

public static class Selection
{
	public static event Action<Selectable> OnSelect;
	public static event Action<Selectable> OnDeselect;

	public static event Action<Selectable> OnHoverEnter;
	public static event Action<Selectable> OnHoverExit;


	private static Dictionary<uint, Selectable> _selectables;
	private static Dictionary<uint, Selectable> selectables
	{
		get { return _selectables ??= new(); }
	}
	public static IReadOnlyList<Selectable> Selectables
	{
		get { return new List<Selectable>(selectables.Values); }
	}

	private static HashSet<uint> _active;
	private static HashSet<uint> active
	{
		get { return _active ??= new(); }
	}
	public static IReadOnlyList<uint> Active
	{
		get { return new List<uint>(active); }
	}

	private static HashSet<uint> _hover;
	private static HashSet<uint> hover
	{
		get { return _hover ??= new(); }
	}
	public static IReadOnlyList<uint> Hover
	{
		get { return new List<uint>(hover); }
	}

	public static bool Register(Selectable selectable)
	{
		return selectables.TryAdd(selectable.ID, selectable);
	}
	public static bool Unregister(Selectable selectable)
	{
		return selectables.Remove(selectable.ID);
	}
	public static void Set(IEnumerable<uint> ids)
	{
		HashSet<uint> old = new(active);
		HashSet<uint> set = new(ids);
		old.ExceptWith(ids);
		set.ExceptWith(active);
		Remove(old);
		Add(set);
	}
	public static void Remove(IEnumerable<uint> ids)
	{
		foreach (var id in ids)
		{
			if (selectables.ContainsKey(id))
			{
				if (active.Remove(id))
				{
					selectables[id].Deselect();
					OnDeselect?.Invoke(selectables[id]);
				}
			}
		}
	}
	public static void Add(IEnumerable<uint> ids)
	{
		foreach (uint id in ids)
		{
			if (selectables.ContainsKey(id))
			{
				if (active.Add(id))
				{
					selectables[id].Select();
					OnSelect?.Invoke(selectables[id]);
				}
			}
		}
	}
	public static void SetHover(IEnumerable<uint> ids)
	{
		HashSet<uint> old = new(hover);
		HashSet<uint> set = new(ids);
		old.ExceptWith(set);
		set.ExceptWith(hover);

		foreach (var id in old)
		{
			if (selectables.ContainsKey(id))
			{
				if(hover.Remove(id))
				{ 
					selectables[id].HoverExit();
					OnHoverExit?.Invoke(selectables[id]);
				}
			}
		}
		foreach (var id in set)
		{
			if (selectables.ContainsKey(id))
			{
				if (hover.Add(id))
				{
					selectables[id].HoverEnter();
					OnHoverEnter?.Invoke(selectables[id]);
				}
			}
		}
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
			active.Clear();
			hover.Clear();
		}
	}

#endif
}
