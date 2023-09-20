using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SelectionUtility
{
    public static Color32 IDToColor(uint number)
    {
        var intBytes = System.BitConverter.GetBytes(number);

        return new Color32(intBytes[0], intBytes[1], intBytes[2], intBytes[3]);
    }
    public static uint ColorToID(Color32 color)
    {
        return System.BitConverter.ToUInt32(new byte[] { color.r, color.g, color.b, color.a }, 0);
    }
}
