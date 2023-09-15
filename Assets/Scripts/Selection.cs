using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;


public static class Selection
{
    public static bool AddSelectable(SelectableBase selectable)
    {
        return selectables.TryAdd(selectable.ID, selectable);
    }
    public static bool RemoveSelectable(SelectableBase selectable)
    {
        return selectables.Remove(selectable.ID);
    }
    private static Dictionary<uint, SelectableBase> _selectables;
    private static Dictionary<uint, SelectableBase> selectables
    {
        get
        {
            _selectables ??= new();
            return _selectables;
        }
    }
    private static ReadOnlyDictionary<uint, SelectableBase> _readonlySelectables; 
    public static ReadOnlyDictionary<uint, SelectableBase> Selectables
    {
        get 
        {
            _readonlySelectables ??= new ReadOnlyDictionary<uint, SelectableBase>(selectables);
            return _readonlySelectables;
        }
    }

    private static List<SelectableBase> _active;
    public static List<SelectableBase> Active
    {
        get
        {
            _active ??= new();
            return _active;
        }
        set { _active = value; }
    }
    private static List<SelectableBase> _hover;
    public static List<SelectableBase> Hover
    {
        get 
        {
            _hover ??= new();
            return _hover;
        }
        set { _hover = value; }
    }


}
