using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selectable : MonoBehaviour
{
    public static uint IDsCount { get; private set; }
    [RuntimeInitializeOnLoadMethod]
    private static void InitIDs()
    {
        IDsCount = 0;
    }

    [field: SerializeField]
    public SelectionData SelectionData { get; set; }
    public uint ID { get; private set; }
    public Color32 Color32 { get; private set; }
    public Renderer Renderer{ get; private set; }

    private void Start()
    {
        ID = GetNextUniqueID();
        Color32 = IDToColor(ID);
        Renderer = GetComponent<Renderer>();
    }    

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
        if (!SelectionData.Selectables.Contains(this))
            SelectionData.Selectables.Add(this);
    }
    private void OnDisable()
    {
        if (SelectionData.Selectables.Contains(this))
            SelectionData.Selectables.Remove(this);
    }
}
