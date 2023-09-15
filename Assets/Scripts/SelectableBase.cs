using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SelectableBase : MonoBehaviour
{
    public static uint IDsCount { get; private set; }
    [RuntimeInitializeOnLoadMethod(loadType: RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        IDsCount = 0;
    }
    public uint ID { get; private set; }
    public Color32 Color32 { get; private set; }
    public Renderer Renderer{ get; private set; }

    protected virtual void Awake()
    {
        ID = GetNextUniqueID();
        Color32 = IDToColor(ID);
        Renderer = GetComponentInChildren<Renderer>(true);
        if (Renderer == null) Renderer = GetComponentInParent<Renderer>(true);
        if (Renderer == null) Debug.LogWarning("No Renderer component found!");
    }

    public abstract void Select();
    public abstract void Deselect();

    private uint GetNextUniqueID()
    {
        return ++IDsCount;
    }
    public static Color32 IDToColor(uint number)
    {
        var intBytes = System.BitConverter.GetBytes(number);

        return new Color32(intBytes[0], intBytes[1], intBytes[2], intBytes[3]);
    }
    public static uint ColorToID(Color32 color)
    {
        return System.BitConverter.ToUInt32(new byte[] { color.r, color.g, color.b, color.a }, 0);
    }

    private void OnEnable()
    {
        Selection.AddSelectable(this);
    }
    private void OnDisable()
    {
        Selection.RemoveSelectable(this);
    }
}
