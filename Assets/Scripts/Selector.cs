using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour
{
    private Vector2 dragBeginPosition;
    [field: SerializeField]
    public SelectableIDMap IDMap { get; set; }

    public bool IsDragging => SelectionRect.size.x >= 1 && SelectionRect.size.y >= 1 && Input.GetMouseButton(0);
    private bool ShiftModifierPressed => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    private bool ControlModifierPressed => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    public Rect SelectionRect
    {
        get
        {
            if (Input.GetMouseButton(0))
            {
                Vector2 min = Vector2.Min(dragBeginPosition, MousePosition());
                Vector2 max = Vector2.Max(dragBeginPosition, MousePosition());
                min = Vector2.Max(min, Vector2.zero);
                max = Vector2.Min(max, new Vector2(Screen.width, Screen.height));
                Vector2 size = max - min;
                return new Rect(new Vector2(min.x, min.y), size);
            }
            else return new Rect(MousePosition(), Vector2.zero);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragBeginPosition = MousePosition();
        }

        if (IsMouseWithinScreen())
        {
            IDMap.Sample(SelectionRect, HandleSelection); 
        }
    }
    private void HandleSelection(IEnumerable<uint> ids)
    {
        Selection.SetHover(ids);
        if (Input.GetMouseButtonUp(0))
        {
            if (ShiftModifierPressed)
            {
                Selection.Add(ids);
            }
            else if (ControlModifierPressed)
            {
                Selection.Remove(ids);
            }
            else Selection.Set(ids);
        }
    }
    private bool IsMouseWithinScreen()
    {
        Vector2 position = MousePosition();
        return position.x >= 0 && ((int)position.x) < (Screen.width) &&
               position.y >= 0 && ((int)position.y) < (Screen.height);
    }
    private Vector2 MousePosition()
    {
        return new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
    }
}
