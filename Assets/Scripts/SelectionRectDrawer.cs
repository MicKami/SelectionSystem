using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionRectDrawer : MonoBehaviour
{
    [SerializeField] private Selector selector;
    [SerializeField] private int borderPixelSize = 1;
    [SerializeField] private Color borderColor = Color.white;
    [SerializeField] private Color backgroundColor = Color.clear;

    private Material _selectionRectMaterial;
    public Material selectionRectMaterial
    {
        get
        {
            return _selectionRectMaterial ??= new Material(Shader.Find("Custom/SelectionRect"));
        }
    }
    private Texture2D _rectTexture;
    private Texture2D rectTexture
    {
        get
        {
            if (_rectTexture == null)
            {
                _rectTexture = new Texture2D(1, 1);
                _rectTexture.SetPixel(0, 0, Color.white);
                _rectTexture.Apply();
            }
            return _rectTexture;
        }
    }

    private void OnGUI()
    {
        if (Event.current.type == EventType.Repaint)
        {
            Graphics.DrawTexture(selector.SelectionRect, rectTexture, selectionRectMaterial, 0);
        }
    }
    private void OnValidate()
    {
        borderPixelSize = Mathf.Max(borderPixelSize, 0);
        if (selectionRectMaterial != null)
        {
            selectionRectMaterial.SetFloat("_BorderPixelSize", borderPixelSize);
            selectionRectMaterial.SetColor("_BorderColor", borderColor);
            selectionRectMaterial.SetColor("_BackgroundColor", backgroundColor);
        }
    }
}
