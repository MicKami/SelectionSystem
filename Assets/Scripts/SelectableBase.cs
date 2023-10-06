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
    [field:Layer, SerializeField]
    public int SelectablesLayer { get; private set; }
    public uint ID { get; private set; }
    public Color32 Color32 { get; private set; }
    public Renderer Renderer{ get; private set; }


    protected virtual void Awake()
    {
        ID = GetNextUniqueID();
        Color32 = SelectionUtility.IDToColor(ID);
        Renderer = GetComponentInChildren<Renderer>(true);
        if (Renderer == null) Renderer = GetComponentInParent<Renderer>(true);
        if (Renderer == null) Debug.LogWarning("No Renderer component found!");

        gameObject.layer = SelectablesLayer;
        Renderer.material.SetColor("_UnlitColor", Color32);
    }

    public abstract void Select();
    public abstract void Deselect();    
	private uint GetNextUniqueID()
    {
        return ++IDsCount;
    }


    private void OnEnable()
    {
        Selection.Register(this);
    }
    private void OnDisable()
    {
        Selection.Unregister(this);
    }
}
