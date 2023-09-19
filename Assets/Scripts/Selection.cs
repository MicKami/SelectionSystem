using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


public static class Selection
{

    private static Dictionary<uint, SelectableBase> _selectables;
    private static Dictionary<uint, SelectableBase> selectables
    {
        get { return _selectables ??= new(); }
    }
    private static ReadOnlyCollection<SelectableBase> _selectablesReadOnly;
    public static ReadOnlyCollection<SelectableBase> Selectables
    {
        get { return _selectablesReadOnly ??= new(selectables.Values.ToList()); }
    }
    private static List<SelectableBase> _active;
    private static List<SelectableBase> active
    {
        get { return _active ??= new(); }
    }
    private static ReadOnlyCollection<SelectableBase> _activeReadOnly;
    public static ReadOnlyCollection<SelectableBase> Active
    {
        get { return _activeReadOnly ??= new(active); }
    }
    private static List<SelectableBase> _hover;
    private static List<SelectableBase> hover
    {
        get { return _hover ??= new(); }
    }
    private static ReadOnlyCollection<SelectableBase> _hoverReadOnly;
    public static ReadOnlyCollection<SelectableBase> Hover
    {
        get { return _hoverReadOnly ??= new(hover); }
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
        foreach (SelectableBase selectable in active)
        {
            selectable.Deselect();
        }
        active.Clear();
    }
    public static void Set(uint id)
    {
        ClearActive();
        Add(id);
    }
    public static void Set(IEnumerable<uint> ids)
    {
        ClearActive();
        Add(ids);
    }
    public static void Add(uint id)
    {
        if (selectables.ContainsKey(id))
        {
            var selection = selectables[id];
            selection.Select();
            active.Add(selection);
        }
    }
    public static void Add(IEnumerable<uint> ids)
    {
        foreach (uint id in ids)
        {
            Add(id);
        }
    }
    public static void ClearHover()
    {
        hover.Clear();
    }
    public static void SetHover(uint id)
    {
        ClearHover();
        if (selectables.ContainsKey(id))
        {
            hover.Add(selectables[id]);
        }
    }
    public static void SetHover(IEnumerable<uint> ids)
    {
        ClearHover();
        foreach (uint id in ids)
        {
            if (selectables.ContainsKey(id))
            {
                hover.Add(selectables[id]);
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
            ClearActive();
            ClearHover();
        }
    } 
#endif
}
