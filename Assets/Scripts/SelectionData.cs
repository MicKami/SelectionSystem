using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Selection Data", fileName = "New Selection Data")]
public class SelectionData : ScriptableObject
{
    public HashSet<Selectable> Selectables { get; set; } = new();
    [field: SerializeField]
    public List<Selectable> Selection { get; set; } = new();
    [field: SerializeField]
    public List<Selectable> Hover { get; set; } = new();

#if UNITY_EDITOR
    private void Initialize(UnityEditor.PlayModeStateChange stateChange)
    {
        if (stateChange == UnityEditor.PlayModeStateChange.ExitingEditMode || stateChange == UnityEditor.PlayModeStateChange.ExitingPlayMode)
        {
            Selectables = new();
            Selection = new();
            Hover = new();
        }
    }
    private void OnEnable()
    {
        UnityEditor.EditorApplication.playModeStateChanged += Initialize;
    }
    private void OnDisable()
    {
        UnityEditor.EditorApplication.playModeStateChanged -= Initialize;
    }
#endif
}
