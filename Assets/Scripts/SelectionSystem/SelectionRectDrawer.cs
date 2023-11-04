using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionRectDrawer : MonoBehaviour
{
    [SerializeField] private Selector selector;
    [SerializeField] private int borderPixelSize = 1;
    [SerializeField] private Color borderColor = Color.white;
    [SerializeField] private Color backgroundColor = Color.clear;

    private Material selectionRectMaterial;    
    private Texture2D rectTexture;
   

	private void Awake()
	{
		selectionRectMaterial = new Material(Shader.Find("Hidden/SelectionRect"));
		selectionRectMaterial.SetFloat("_BorderPixelSize", borderPixelSize);
		selectionRectMaterial.SetColor("_BorderColor", borderColor);
		selectionRectMaterial.SetColor("_BackgroundColor", backgroundColor);
		rectTexture = new Texture2D(1, 1);
		rectTexture.SetPixel(0, 0, Color.white);
		rectTexture.Apply();
	}

	private void OnGUI()
    {
        if (Event.current.type == EventType.Repaint && selector.IsDragging)
        {
            var rect = selector.SelectionRect;
            rect.y = Screen.height - rect.y - rect.height;
            Graphics.DrawTexture(rect, rectTexture, selectionRectMaterial, 0);
        }
    }
}
