using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Rendering.InspectorCurveEditor;


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

    private static HashSet<SelectableBase> _active;
    private static HashSet<SelectableBase> active
    {
        get { return _active ??= new(); }
    }
    public static IEnumerable<SelectableBase> Active
    {
        get { return active; }
    }

    private static HashSet<SelectableBase> _hover;
    private static HashSet<SelectableBase> hover
    {
        get { return _hover ??= new(); }
    }
    public static IEnumerable<SelectableBase> Hover
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
        foreach (SelectableBase selectable in active)
        {
            selectable.Deselect();
        }
        active.Clear();
    }
    private static bool RemoveExcept(uint IDToKeep)
    {
        bool contains = false;
        foreach (SelectableBase selectable in active)
        {
            if (selectable.ID == IDToKeep)
            {
                contains = true;
            }
            else selectable.Deselect();
        }
        active.Clear();
        if (contains)
        {
            active.Add(selectables[IDToKeep]);
        }
        return contains;
    }
    public static void Set(uint id)
    {
        if (selectables.ContainsKey(id))
        {
            if (!RemoveExcept(id))
            {
                Add(id);
            }
        }
        else ClearActive();
    }
    public static void Set(IEnumerable<uint> ids)
    {
        ClearActive();
        Add(ids);
    }
    public static bool Remove(uint id)
    {
        if (selectables.ContainsKey(id))
        {
            var selection = selectables[id];
            if (active.Remove(selection))
            {
                selection.Deselect();
                return true;
            }
        }
        return false;
    }
    public static void Remove(IEnumerable<uint> ids)
    {
        foreach (var id in ids)
        {
            Remove(id);
        }
    }
    public static void Add(uint id)
    {
        if (selectables.ContainsKey(id))
        {
            var selection = selectables[id];
            if (active.Add(selection))
            {
                selection.Select();
            }
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
