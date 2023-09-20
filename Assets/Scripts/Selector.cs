using UnityEngine;
using UnityEngine.UI;

public class Selector : MonoBehaviour
{
    private bool isDragging;
    private Vector2 dragBeginPosition;
    private Rect selectionRect;

    [field: SerializeField]
    public SelectableIDMap IDMap { get; set; }

    private void Update()
    {
        if (!isDragging)
        {
            var mousePosition = MousePosition();
            if (IsPositionWithinScreen(mousePosition))
            {
                IDMap.SampleAtPosition(MousePosition(), id =>
                {
                    Selection.SetHover(id);
                    if (Input.GetMouseButtonDown(0))
                    {
                        Selection.Set(id);
                    }
                });
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            dragBeginPosition = MousePosition();
        }
        if (Input.GetMouseButton(0))
        {
            Vector2 min = Vector2.Min(dragBeginPosition, MousePosition());
            Vector2 max = Vector2.Max(dragBeginPosition, MousePosition());
            min = Vector2.Max(min, Vector2.zero);
            max = Vector2.Min(max, new Vector2(Screen.width, Screen.height));
            Vector2 size = max - min;
            selectionRect = new Rect(new Vector2(min.x, min.y), size);
            isDragging = true;
        }
        if (isDragging && selectionRect.size.x > 1 && selectionRect.size.y > 1)
        {
            IDMap.SampleAtRegion(selectionRect, ids =>
            {
                Selection.SetHover(ids);
                if (Input.GetMouseButtonUp(0))
                {
                    Selection.ClearHover();
                    Selection.Add(ids);
                }
            });
        }
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            selectionRect.size = Vector2.zero;
        }
    }

    private void OnGUI()
    {
        if (isDragging)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, new Color(0, 0, 0, 0.15f));
            texture.Apply();
            GUI.DrawTexture(selectionRect, texture);
        }
    }



    private bool IsPositionWithinScreen(Vector2 position)
    {
        return position.x >= 0 && ((int)position.x) < (Screen.width) &&
               position.y >= 0 && ((int)position.y) < (Screen.height);
    }

    private Vector2 MousePosition()
    {
        return new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
    }
}
